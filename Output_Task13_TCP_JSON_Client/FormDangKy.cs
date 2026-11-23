using System;
using System.Windows.Forms;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;

namespace QuanLyChamCong
{
    public partial class FormDangKy : Form
    {
        public FormDangKy()
        {
            InitializeComponent();
        }

        private void lblTieuDe_Click(object sender, EventArgs e)
        {
        }

        private async void btnDangKy_Click(object sender, EventArgs e)
        {
            string maNV = txtMaNV.Text.Trim();
            string ten = txtTen.Text.Trim();
            string matKhau = txtMatKhau.Text;
            string xacNhan = txtXacNhanMatKhau.Text;

            if (maNV == "" || ten == "" || matKhau == "" || xacNhan == "")
            {
                MessageBox.Show(
                    "Vui lòng nhập đầy đủ thông tin!",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (matKhau != xacNhan)
            {
                MessageBox.Show(
                    "Mật khẩu xác nhận không khớp!",
                    "Lỗi xác nhận",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            btnDangKy.Enabled = false;

            try
            {
                var payload = new RegisterRequestData
                {
                    Ten = ten,
                    MatKhau = matKhau
                };

                ResponsePacket response = await TcpClientHelper.SendRequestAsync(
                    TcpActions.Register,
                    maNV,
                    payload);

                if (IsDisposed)
                    return;

                if (response.Success)
                {
                    string message = string.IsNullOrWhiteSpace(response.Message)
                        ? "Đăng ký thành công!"
                        : response.Message;

                    MessageBox.Show(
                        message,
                        "Đăng ký",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        response.FormatErrorDisplay("Đăng ký thất bại!"),
                        "Đăng ký thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnDangKy.Enabled = true;
                }
            }
        }
    }
}
