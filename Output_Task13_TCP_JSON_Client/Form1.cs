using System;
using System.Windows.Forms;
using QuanLyChamCong;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnKiemTra_Click(object sender, EventArgs e)
        {
            btnKiemTra.Enabled = false;

            try
            {
                ResponsePacket response = await TcpClientHelper.SendRequestAsync(TcpActions.Ping);

                if (IsDisposed)
                    return;

                if (response.Success)
                {
                    string message = string.IsNullOrWhiteSpace(response.Message)
                        ? "Kết nối Server thành công!"
                        : response.Message;

                    MessageBox.Show(
                        message,
                        "Kiểm tra kết nối",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        response.FormatErrorDisplay("Không thể kết nối Server."),
                        "Lỗi kết nối",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnKiemTra.Enabled = true;
                }
            }
        }

        private async void btnDangNhap_Click(object sender, EventArgs e)
        {
            string maNV = txtMaNV.Text.Trim();
            string matKhau = txtMatKhau.Text;

            if (maNV == "" || matKhau == "")
            {
                MessageBox.Show(
                    "Vui lòng nhập mã nhân viên và mật khẩu!",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            btnDangNhap.Enabled = false;
            btnMoDangKy.Enabled = false;
            btnKiemTra.Enabled = false;

            try
            {
                var payload = new LoginRequestData
                {
                    MatKhau = matKhau
                };

                ResponsePacket response = await TcpClientHelper.SendRequestAsync(
                    TcpActions.Login,
                    maNV,
                    payload);

                if (IsDisposed)
                    return;

                if (response.Success)
                {
                    LoginResponseData? loginData = response.DeserializeData<LoginResponseData>();
                    string maNVDangNhap = !string.IsNullOrWhiteSpace(loginData?.MaNV)
                        ? loginData.MaNV
                        : maNV;
                    string tenNV = !string.IsNullOrWhiteSpace(loginData?.Ten)
                        ? loginData.Ten
                        : (!string.IsNullOrWhiteSpace(loginData?.TenNV) ? loginData.TenNV : maNVDangNhap);
                    string chucVu = !string.IsNullOrWhiteSpace(loginData?.ChucVu)
                        ? loginData.ChucVu
                        : "NhanVien";

                    MessageBox.Show(
                        $"Đăng nhập thành công!\nTên: {tenNV}\nChức vụ: {chucVu}",
                        "Đăng nhập",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    using (FormChamCong form = new FormChamCong(maNVDangNhap, tenNV, chucVu))
                    {
                        this.Hide();
                        form.ShowDialog();
                    }

                    if (!IsDisposed)
                    {
                        txtMatKhau.Clear();
                        this.Show();
                    }
                }
                else
                {
                    MessageBox.Show(
                        response.FormatErrorDisplay("Đăng nhập thất bại!"),
                        "Đăng nhập thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnDangNhap.Enabled = true;
                    btnMoDangKy.Enabled = true;
                    btnKiemTra.Enabled = true;
                }
            }
        }

        private void btnDatLaiMatKhau_Click(object sender, EventArgs e)
        {
            try
            {
                DatabaseHelper db = new DatabaseHelper();

                bool ketQua = db.DatLaiMatKhau("NV002", "123456");

                MessageBox.Show(
                    ketQua ? "Đặt lại mật khẩu thành công!" :
                             "Không tìm thấy nhân viên NV002!"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnMoDangKy_Click(object sender, EventArgs e)
        {
            using FormDangKy form = new FormDangKy();
            form.ShowDialog();
        }
    }
}
