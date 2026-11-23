using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using QuanLyChamCong.Models;

namespace QuanLyChamCong.Services
{
    public static class TcpClientHelper
    {
        public static string ServerHost { get; set; } = "127.0.0.1";
        public static int ServerPort { get; set; } = 8888;
        public static int TimeoutMilliseconds { get; set; } = 8000;

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void Configure(string host, int port, int timeoutMilliseconds = 8000)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Địa chỉ Server không được để trống.", nameof(host));

            if (port is <= 0 or > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "Cổng Server phải nằm trong khoảng 1 - 65535.");

            if (timeoutMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds), "Timeout phải lớn hơn 0 ms.");

            ServerHost = host.Trim();
            ServerPort = port;
            TimeoutMilliseconds = timeoutMilliseconds;
        }

        public static Task<ResponsePacket> SendRequestAsync(
            string action,
            string? maNV = null,
            object? data = null,
            CancellationToken cancellationToken = default)
        {
            var request = new RequestPacket
            {
                RequestId = Guid.NewGuid().ToString("N"),
                Action = action,
                MaNV = maNV,
                Data = data
            };

            return SendRequestAsync(request, cancellationToken);
        }

        public static async Task<ResponsePacket> SendRequestAsync(
            RequestPacket request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.RequestId))
            {
                request.RequestId = Guid.NewGuid().ToString("N");
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeoutMilliseconds);

            try
            {
                using var client = new TcpClient();
                client.SendTimeout = TimeoutMilliseconds;
                client.ReceiveTimeout = TimeoutMilliseconds;

                await client.ConnectAsync(ServerHost, ServerPort, timeoutCts.Token).ConfigureAwait(false);

                using NetworkStream stream = client.GetStream();
                using var writer = new StreamWriter(
                    stream,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    bufferSize: 4096,
                    leaveOpen: true)
                {
                    NewLine = "\n",
                    AutoFlush = true
                };
                using var reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 4096,
                    leaveOpen: true);

                string requestJson = JsonSerializer.Serialize(request, SerializerOptions);
                await writer.WriteLineAsync(requestJson.AsMemory(), timeoutCts.Token).ConfigureAwait(false);
                await writer.FlushAsync(timeoutCts.Token).ConfigureAwait(false);

                string? responseJson = await reader.ReadLineAsync(timeoutCts.Token).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    return CreateErrorResponse(
                        request.RequestId,
                        TcpErrorCodes.ServerDisconnected,
                        "Server đã đóng kết nối hoặc không trả về dữ liệu.");
                }

                ResponsePacket? response = JsonSerializer.Deserialize<ResponsePacket>(responseJson, SerializerOptions);
                if (response == null)
                {
                    return CreateErrorResponse(
                        request.RequestId,
                        TcpErrorCodes.InvalidResponse,
                        "Không thể đọc phản hồi JSON từ Server.");
                }

                if (!string.Equals(response.RequestId, request.RequestId, StringComparison.Ordinal))
                {
                    return CreateErrorResponse(
                        request.RequestId,
                        TcpErrorCodes.RequestIdMismatch,
                        $"Mã RequestId không khớp (gửi '{request.RequestId}', nhận '{response.RequestId}').");
                }

                return response;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return CreateErrorResponse(
                    request.RequestId,
                    TcpErrorCodes.Timeout,
                    $"Kết nối tới Server ({ServerHost}:{ServerPort}) quá thời gian chờ ({TimeoutMilliseconds / 1000.0:0.#}s).");
            }
            catch (OperationCanceledException)
            {
                return CreateErrorResponse(
                    request.RequestId,
                    TcpErrorCodes.Cancelled,
                    "Yêu cầu đã bị hủy.");
            }
            catch (SocketException ex)
            {
                return CreateErrorResponse(
                    request.RequestId,
                    TcpErrorCodes.ConnectionFailed,
                    $"Không thể kết nối tới Server ({ServerHost}:{ServerPort}): {ex.Message}");
            }
            catch (IOException ex)
            {
                return CreateErrorResponse(
                    request.RequestId,
                    TcpErrorCodes.ServerDisconnected,
                    $"Mất kết nối trong quá trình trao đổi dữ liệu với Server: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return CreateErrorResponse(
                    request.RequestId,
                    TcpErrorCodes.InvalidResponse,
                    $"Dữ liệu phản hồi từ Server không đúng định dạng JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(
                    request.RequestId,
                    TcpErrorCodes.ClientError,
                    $"Lỗi giao tiếp Client: {ex.Message}");
            }
        }

        private static ResponsePacket CreateErrorResponse(string requestId, string errorCode, string message)
        {
            return new ResponsePacket
            {
                RequestId = requestId,
                Success = false,
                ErrorCode = errorCode,
                Message = message
            };
        }
    }
}
