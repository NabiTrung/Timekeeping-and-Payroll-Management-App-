using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using WinFormsApp1;
namespace QuanLyChamCong
{
    public partial class FormChamCong : Form
    {
        [System.ComponentModel.DesignerSerializationVisibility(
    System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string MaNV { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string TenNV { get; set; }

        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string ChucVu { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(
    System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string SessionToken { get; set; } = "";
        public FormChamCong()
        {
            InitializeComponent();
        }

        public FormChamCong(
        string maNV,
        string tenNV,
        string chucVu,
        string sessionToken)
        {
            InitializeComponent();

            MaNV = maNV;
            TenNV = tenNV;
            ChucVu = chucVu;
            SessionToken = sessionToken;
        }

        private void FormChamCong_Load(object sender, EventArgs e)
        {
            lblMaNV.Text = "Mã nhân viên: " + MaNV;
            lblTenNV.Text = "Nhân viên: " + TenNV;
            lblChucVu.Text = "Chức vụ: " + ChucVu;
        }


        private void btnCheckIn_Click(object sender, EventArgs e)
        {
            try
            {
                Request request = new Request
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Action = "CHECKIN",
                    Data = new
                    {
                        sessionToken = SessionToken
                    }
                };

                Response response = TcpJsonClient.Send(request);

                if (response.Success)
                {
                    MessageBox.Show("Chấm công vào thành công!");
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



        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            try
            {
                Request request = new Request
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Action = "CHECKOUT",
                    Data = new
                    {
                        sessionToken = SessionToken
                    }
                };

                Response response = TcpJsonClient.Send(request);

                if (response.Success)
                {
                    MessageBox.Show("Chấm công ra thành công!");
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


        private void btnLichSu_Click(object sender, EventArgs e)
        {
            FormLichSu form = new FormLichSu(
    MaNV, SessionToken);
            form.ShowDialog();
        }
        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            try
            {
                Request request = new Request
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Action = "LOGOUT",
                    Data = new
                    {
                        sessionToken = SessionToken
                    }
                };

                Response response = TcpJsonClient.Send(request);

                if (response.Success)
                {
                    MessageBox.Show("Đăng xuất thành công!");

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
    }
}