using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace QuanLyChamCong.Services
{
    public sealed class CameraDeviceInfo
    {
        public int Index { get; init; }
        public string Name { get; init; } = string.Empty;

        public override string ToString() => Name;
    }

    public sealed class CapturedImageResult
    {
        public string RelativePath { get; init; } = string.Empty;
        public string FullPath { get; init; } = string.Empty;
        public string Base64String { get; init; } = string.Empty;
        public DateTime CapturedAt { get; init; }
    }

    public sealed class CameraHelper : IDisposable
    {
        private readonly object _syncLock = new();
        private CancellationTokenSource? _cts;
        private Task? _captureTask;
        private PictureBox? _previewBox;
        private Mat? _latestFrame;
        private int _uiUpdatePending;
        private volatile bool _isRunning;
        private bool _disposed;

        public bool IsRunning => _isRunning;

        public event Action<string>? OnError;

        public static List<CameraDeviceInfo> GetAvailableCameras(int maxProbeCount = 5)
        {
            var cameras = new List<CameraDeviceInfo>();

            for (int index = 0; index < maxProbeCount; index++)
            {
                bool opened = TryProbeCamera(index, VideoCaptureAPIs.DSHOW);
                if (!opened && index == 0)
                {
                    opened = TryProbeCamera(index, VideoCaptureAPIs.ANY);
                }

                if (opened)
                {
                    cameras.Add(new CameraDeviceInfo
                    {
                        Index = index,
                        Name = $"Camera {index + 1} (Thiết bị #{index})"
                    });
                }
                else if (index > 0)
                {
                    // Dừng quét sớm sau khi gặp chỉ số không tồn tại đầu tiên (ngoại trừ index 0 đã fallback).
                    break;
                }
            }

            return cameras;
        }

        private static bool TryProbeCamera(int index, VideoCaptureAPIs api)
        {
            try
            {
                using var probe = new VideoCapture(index, api);
                if (!probe.IsOpened())
                    return false;

                probe.Release();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Start(int cameraIndex, PictureBox previewBox, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (_disposed)
            {
                errorMessage = "CameraHelper đã bị giải phóng.";
                return false;
            }

            if (previewBox == null)
            {
                errorMessage = "PictureBox hiển thị camera không hợp lệ.";
                return false;
            }

            Stop();

            VideoCapture? capture = OpenVideoCapture(cameraIndex, out errorMessage);
            if (capture == null)
            {
                return false;
            }

            _previewBox = previewBox;
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            _isRunning = true;
            Interlocked.Exchange(ref _uiUpdatePending, 0);

            _captureTask = Task.Run(() => CaptureLoop(capture, token), token);
            return true;
        }

        private static VideoCapture? OpenVideoCapture(int cameraIndex, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
                if (!capture.IsOpened())
                {
                    capture.Dispose();
                    capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.ANY);
                }

                if (!capture.IsOpened())
                {
                    capture.Dispose();
                    errorMessage = $"Không thể mở Camera #{cameraIndex}. Camera có thể chưa kết nối hoặc đang bị ứng dụng khác chiếm dụng.";
                    return null;
                }

                capture.Set(VideoCaptureProperties.FrameWidth, 640);
                capture.Set(VideoCaptureProperties.FrameHeight, 480);
                return capture;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi khởi tạo Camera #{cameraIndex}: {ex.Message}";
                return null;
            }
        }

        private void CaptureLoop(VideoCapture capture, CancellationToken token)
        {
            const int maxConsecutiveEmptyFrames = 50;
            int emptyFrameCount = 0;

            using var frame = new Mat();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!capture.Read(frame) || frame.Empty())
                    {
                        emptyFrameCount++;
                        if (emptyFrameCount >= maxConsecutiveEmptyFrames)
                        {
                            OnError?.Invoke("Không nhận được khung hình từ camera (camera bị ngắt kết nối hoặc bị ứng dụng khác chiếm dụng).");
                            break;
                        }

                        Thread.Sleep(30);
                        continue;
                    }

                    emptyFrameCount = 0;

                    lock (_syncLock)
                    {
                        _latestFrame?.Dispose();
                        _latestFrame = frame.Clone();
                    }

                    PushPreviewToPictureBox(frame, token);
                    Thread.Sleep(33);
                }
            }
            catch (OperationCanceledException)
            {
                // Dừng bình thường theo yêu cầu.
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    OnError?.Invoke("Lỗi luồng camera: " + ex.Message);
                }
            }
            finally
            {
                _isRunning = false;

                try
                {
                    capture.Release();
                }
                catch
                {
                }

                capture.Dispose();
            }
        }

        private void PushPreviewToPictureBox(Mat frame, CancellationToken token)
        {
            PictureBox? target = _previewBox;
            if (target == null || target.IsDisposed || !target.IsHandleCreated || token.IsCancellationRequested)
                return;

            // Bỏ qua frame preview nếu UI chưa xử lý xong frame trước để tránh tràn bộ nhớ GDI+.
            if (Interlocked.CompareExchange(ref _uiUpdatePending, 1, 0) != 0)
                return;

            Bitmap? bitmap = null;

            try
            {
                bitmap = BitmapConverter.ToBitmap(frame);

                target.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (!token.IsCancellationRequested && !target.IsDisposed)
                        {
                            Image? oldImage = target.Image;
                            target.Image = bitmap;
                            bitmap = null;
                            oldImage?.Dispose();
                        }
                    }
                    finally
                    {
                        bitmap?.Dispose();
                        Interlocked.Exchange(ref _uiUpdatePending, 0);
                    }
                }));
            }
            catch
            {
                bitmap?.Dispose();
                Interlocked.Exchange(ref _uiUpdatePending, 0);
            }
        }

        public CapturedImageResult CaptureAndSave(
            string maNV,
            string relativeFolder = "Images/CheckIn",
            int jpegQuality = 80)
        {
            Mat? snapshot = null;

            try
            {
                lock (_syncLock)
                {
                    if (_latestFrame != null && !_latestFrame.Empty())
                    {
                        snapshot = _latestFrame.Clone();
                    }
                }

                if (snapshot == null || snapshot.Empty())
                {
                    throw new InvalidOperationException("Chưa có khung hình từ camera để chụp.");
                }

                DateTime now = DateTime.Now;
                string safeMaNV = SanitizeFileSegment(string.IsNullOrWhiteSpace(maNV) ? "UNKNOWN" : maNV.Trim());
                string fileName = $"{safeMaNV}_{now:yyyyMMdd_HHmmss}.jpg";

                string normalizedRelativeFolder = relativeFolder
                    .Replace('\\', '/')
                    .Trim('/');

                string diskFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    normalizedRelativeFolder.Replace('/', Path.DirectorySeparatorChar));

                Directory.CreateDirectory(diskFolder);

                string fullPath = Path.Combine(diskFolder, fileName);
                string relativePath = string.IsNullOrEmpty(normalizedRelativeFolder)
                    ? fileName
                    : $"{normalizedRelativeFolder}/{fileName}";

                var encodeParams = new[]
                {
                    new ImageEncodingParam(ImwriteFlags.JpegQuality, Math.Clamp(jpegQuality, 30, 100))
                };

                if (!Cv2.ImEncode(".jpg", snapshot, out byte[] jpgBytes, encodeParams))
                {
                    throw new InvalidOperationException("Không thể mã hóa ảnh chụp sang định dạng JPEG.");
                }

                File.WriteAllBytes(fullPath, jpgBytes);
                string base64 = Convert.ToBase64String(jpgBytes);

                return new CapturedImageResult
                {
                    RelativePath = relativePath,
                    FullPath = fullPath,
                    Base64String = base64,
                    CapturedAt = now
                };
            }
            finally
            {
                snapshot?.Dispose();
            }
        }

        public void Stop()
        {
            _isRunning = false;

            CancellationTokenSource? cts = _cts;
            _cts = null;

            if (cts != null)
            {
                try
                {
                    cts.Cancel();
                }
                catch
                {
                }
            }

            Task? captureTask = _captureTask;
            _captureTask = null;

            if (captureTask != null)
            {
                try
                {
                    captureTask.Wait(TimeSpan.FromSeconds(2));
                }
                catch
                {
                }
            }

            cts?.Dispose();

            lock (_syncLock)
            {
                _latestFrame?.Dispose();
                _latestFrame = null;
            }

            ClearPreviewSafely();
        }

        private void ClearPreviewSafely()
        {
            PictureBox? target = _previewBox;
            _previewBox = null;

            if (target == null || target.IsDisposed)
                return;

            void ClearAction()
            {
                if (target.IsDisposed)
                    return;

                Image? oldImage = target.Image;
                target.Image = null;
                oldImage?.Dispose();
            }

            try
            {
                if (target.InvokeRequired)
                {
                    if (target.IsHandleCreated)
                    {
                        target.BeginInvoke(new Action(ClearAction));
                    }
                }
                else
                {
                    ClearAction();
                }
            }
            catch
            {
            }
        }

        private static string SanitizeFileSegment(string value)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            char[] buffer = value.ToCharArray();

            for (int i = 0; i < buffer.Length; i++)
            {
                if (Array.IndexOf(invalidChars, buffer[i]) >= 0 ||
                    buffer[i] == '/' ||
                    buffer[i] == '\\' ||
                    buffer[i] == ' ')
                {
                    buffer[i] = '_';
                }
            }

            return new string(buffer);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Stop();
        }
    }
}
