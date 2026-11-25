using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Windows.Forms;
using QuanLyChamCong;
using System.Text.Json;
namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void btnKiemTra_Click(object sender, EventArgs e)
        {
            try
            {
                Request request = new Request
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Action = "PING"
                };

                Response response = TcpJsonClient.Send(request);

                MessageBox.Show(response.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể kết nối Server: " + ex.Message);
            }
        }



        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            string maNV = txtMaNV.Text.Trim();
            string matKhau = txtMatKhau.Text;

            if (maNV == "" || matKhau == "")
            {
                MessageBox.Show(
                    "Vui lòng nhập mã nhân viên và mật khẩu!");
                return;
            }

            try
            {
                Request request = new Request
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Action = "LOGIN",
                    Data = new
                    {
                        maNV = maNV,
                        matKhau = matKhau
                    }
                };

                Response response = TcpJsonClient.Send(request);

                if (response.Success)
                {
                    string tenNV = "";
                    string chucVu = "";
                    string sessionToken = "";

                    if (response.Data.ValueKind == JsonValueKind.Object)
                    {
                        if (response.Data.TryGetProperty(
                            "ten", out JsonElement ten))
                        {
                            tenNV = ten.GetString() ?? "";
                        }

                        if (response.Data.TryGetProperty(
    "sessionToken", out JsonElement token))
                        {
                            sessionToken = token.GetString() ?? "";
                        }
                    }

                    MessageBox.Show(
                        "Đăng nhập thành công!\n" +
                        "Tên: " + tenNV + "\n" +
                        "Chức vụ: " + chucVu);

                    FormChamCong form = new FormChamCong(
    maNV, tenNV, chucVu, sessionToken);

                    this.Hide();
                    form.ShowDialog();
                    this.Show();
                }
                else
                {
                    MessageBox.Show(response.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối Server: " + ex.Message);
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
            FormDangKy form = new FormDangKy();
            form.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
