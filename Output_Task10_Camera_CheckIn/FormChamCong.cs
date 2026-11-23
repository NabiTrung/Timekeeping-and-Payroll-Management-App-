using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;

namespace QuanLyChamCong
{
    public partial class FormChamCong : Form
    {
        private readonly CameraHelper _cameraHelper = new();
        private bool _isInitializingCameras;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MaNV { get; set; } = string.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TenNV { get; set; } = string.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ChucVu { get; set; } = string.Empty;

        public FormChamCong()
        {
            InitializeComponent();
        }

        public FormChamCong(string maNV, string tenNV, string chucVu)
        {
            InitializeComponent();

            MaNV = maNV;
            TenNV = tenNV;
            ChucVu = chucVu;
        }

        private async void FormChamCong_Load(object sender, EventArgs e)
        {
            lblMaNV.Text = "Mã nhân viên: " + MaNV;
            lblTenNV.Text = "Nhân viên: " + TenNV;
            lblChucVu.Text = "Chức vụ: " + ChucVu;

            _cameraHelper.OnError += CameraHelper_OnError;
            await KhoiTaoCameraAsync();
        }

        private async Task KhoiTaoCameraAsync()
        {
            _isInitializingCameras = true;
            cboCamera.Enabled = false;
            btnBatTatCamera.Enabled = false;
            lblTrangThaiCamera.Text = "Camera: Đang quét thiết bị...";

            try
            {
                List<CameraDeviceInfo> cameras = await Task.Run(() => CameraHelper.GetAvailableCameras());

                if (IsDisposed)
                    return;

                cboCamera.Items.Clear();
                foreach (CameraDeviceInfo cam in cameras)
                {
                    cboCamera.Items.Add(cam);
                }

                if (cboCamera.Items.Count == 0)
                {
                    lblTrangThaiCamera.Text = "Camera: Không tìm thấy webcam trên máy.";
                    btnBatTatCamera.Text = "Quét lại";
                    btnBatTatCamera.Enabled = true;
                    return;
                }

                cboCamera.SelectedIndex = 0;
                cboCamera.Enabled = true;
                btnBatTatCamera.Enabled = true;

                BatCameraDangChon();
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    lblTrangThaiCamera.Text = "Camera: Lỗi quét thiết bị.";
                    btnBatTatCamera.Text = "Quét lại";
                    btnBatTatCamera.Enabled = true;
                    MessageBox.Show(
                        "Không thể khởi tạo camera: " + ex.Message,
                        "Lỗi Camera",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            finally
            {
                _isInitializingCameras = false;
            }
        }

        private bool BatCameraDangChon()
        {
            if (cboCamera.SelectedItem is not CameraDeviceInfo selectedCamera)
            {
                lblTrangThaiCamera.Text = "Camera: Chưa chọn thiết bị.";
                btnBatTatCamera.Text = "Bật Camera";
                return false;
            }

            if (_cameraHelper.Start(selectedCamera.Index, picCamera, out string errorMessage))
            {
                lblTrangThaiCamera.Text = $"Camera: Đang bật ({selectedCamera.Name})";
                btnBatTatCamera.Text = "Tắt Camera";
                return true;
            }

            lblTrangThaiCamera.Text = "Camera: Không thể kết nối.";
            btnBatTatCamera.Text = "Bật Camera";
            MessageBox.Show(
                errorMessage,
                "Lỗi Camera",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        private void CameraHelper_OnError(string errorMessage)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed)
                        return;

                    lblTrangThaiCamera.Text = "Camera: Đã dừng do lỗi thiết bị.";
                    btnBatTatCamera.Text = "Bật Camera";
                    MessageBox.Show(
                        errorMessage,
                        "Cảnh báo Camera",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }));
            }
            catch
            {
            }
        }

        private void cboCamera_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isInitializingCameras)
                return;

            BatCameraDangChon();
        }

        private async void btnBatTatCamera_Click(object? sender, EventArgs e)
        {
            if (cboCamera.Items.Count == 0)
            {
                await KhoiTaoCameraAsync();
                return;
            }

            if (_cameraHelper.IsRunning)
            {
                _cameraHelper.Stop();
                lblTrangThaiCamera.Text = "Camera: Đã tắt.";
                btnBatTatCamera.Text = "Bật Camera";
            }
            else
            {
                BatCameraDangChon();
            }
        }

        private async void btnCheckIn_Click(object sender, EventArgs e)
        {
            btnCheckIn.Enabled = false;
            btnCheckOut.Enabled = false;

            try
            {
                CapturedImageResult? captured = null;

                if (!_cameraHelper.IsRunning && cboCamera.Items.Count > 0)
                {
                    BatCameraDangChon();
                }

                if (_cameraHelper.IsRunning)
                {
                    try
                    {
                        captured = _cameraHelper.CaptureAndSave(MaNV);
                    }
                    catch (Exception ex)
                    {
                        DialogResult choice = MessageBox.Show(
                            $"Không thể chụp ảnh Check-In ({ex.Message}).\nBạn vẫn muốn tiếp tục chấm công vào không kèm ảnh?",
                            "Cảnh báo Camera",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (choice != DialogResult.Yes)
                            return;
                    }
                }
                else
                {
                    DialogResult choice = MessageBox.Show(
                        "Camera hiện chưa bật hoặc không khả dụng.\nBạn vẫn muốn tiếp tục chấm công vào không kèm ảnh?",
                        "Không có Camera",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (choice != DialogResult.Yes)
                        return;
                }

                CheckInRequestData? payload = captured == null
                    ? null
                    : new CheckInRequestData
                    {
                        ImagePath = captured.RelativePath,
                        ImageBase64 = captured.Base64String
                    };

                ResponsePacket response = await TcpClientHelper.SendRequestAsync(
                    TcpActions.CheckIn,
                    MaNV,
                    payload);

                if (IsDisposed)
                    return;

                if (response.Success)
                {
                    CheckInResponseData? data = response.DeserializeData<CheckInResponseData>();
                    string detail = string.Empty;

                    if (!string.IsNullOrWhiteSpace(data?.GioVao))
                        detail += $"\nGiờ vào: {data.GioVao}";

                    if (!string.IsNullOrWhiteSpace(data?.TrangThai))
                        detail += $"\nTrạng thái: {data.TrangThai}";

                    string savedPath = !string.IsNullOrWhiteSpace(data?.AnhCheckIn)
                        ? data.AnhCheckIn
                        : (captured?.RelativePath ?? string.Empty);

                    if (!string.IsNullOrWhiteSpace(savedPath))
                        detail += $"\nẢnh Check-In: {savedPath}";

                    string message = string.IsNullOrWhiteSpace(response.Message)
                        ? "Chấm công vào thành công!"
                        : response.Message;

                    MessageBox.Show(
                        message + detail,
                        "Chấm công vào",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        response.FormatErrorDisplay("Chấm công vào thất bại!"),
                        "Chấm công vào",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnCheckIn.Enabled = true;
                    btnCheckOut.Enabled = true;
                }
            }
        }

        private async void btnCheckOut_Click(object sender, EventArgs e)
        {
            btnCheckIn.Enabled = false;
            btnCheckOut.Enabled = false;

            try
            {
                ResponsePacket response = await TcpClientHelper.SendRequestAsync(
                    TcpActions.CheckOut,
                    MaNV);

                if (IsDisposed)
                    return;

                if (response.Success)
                {
                    CheckOutResponseData? data = response.DeserializeData<CheckOutResponseData>();
                    string detail = string.Empty;

                    if (!string.IsNullOrWhiteSpace(data?.GioRa))
                        detail += $"\nGiờ ra: {data.GioRa}";

                    if (!string.IsNullOrWhiteSpace(data?.TrangThai))
                        detail += $"\nTrạng thái: {data.TrangThai}";

                    string message = string.IsNullOrWhiteSpace(response.Message)
                        ? "Chấm công ra thành công!"
                        : response.Message;

                    MessageBox.Show(
                        message + detail,
                        "Chấm công ra",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        response.FormatErrorDisplay("Chấm công ra thất bại!"),
                        "Chấm công ra",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnCheckIn.Enabled = true;
                    btnCheckOut.Enabled = true;
                }
            }
        }

        private void btnLichSu_Click(object sender, EventArgs e)
        {
            using FormLichSu form = new FormLichSu(MaNV);
            form.ShowDialog();
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormChamCong_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _cameraHelper.OnError -= CameraHelper_OnError;
            _cameraHelper.Dispose();

            if (picCamera.Image != null)
            {
                picCamera.Image.Dispose();
                picCamera.Image = null;
            }
        }
    }
}
