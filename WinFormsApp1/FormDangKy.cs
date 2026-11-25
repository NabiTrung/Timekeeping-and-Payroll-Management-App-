using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
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


        private void btnDangKy_Click(object sender, EventArgs e)
        {
            string maNV = txtMaNV.Text.Trim();
            string ten = txtTen.Text.Trim();
            string matKhau = txtMatKhau.Text;
            string xacNhan = txtXacNhanMatKhau.Text;

            if (maNV == "" || ten == "" ||
                matKhau == "" || xacNhan == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (matKhau != xacNhan)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            try
            {
                Request request = new Request
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Action = "REGISTER",
                    Data = new
                    {
                        maNV = maNV,
                        ten = ten,
                        matKhau = matKhau
                    }
                };

                Response response = TcpJsonClient.Send(request);

                if (response.Success)
                {
                    MessageBox.Show("Đăng ký thành công!");
                    this.Close();
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


        private void FormDangKy_Load(object sender, EventArgs e)
        {

        }
    }
}
