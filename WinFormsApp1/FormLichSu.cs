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
    public partial class FormLichSu : Form
    {
        [System.ComponentModel.DesignerSerializationVisibility(
    System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string MaNV { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(
    System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string SessionToken { get; set; } = "";

        public FormLichSu()
        {
            InitializeComponent();
        }

        public FormLichSu(string maNV, string sessionToken)
        {
            InitializeComponent();

            MaNV = maNV;
            SessionToken = sessionToken;
        }

        private void btnTaiLichSu_Click(object sender, EventArgs e)
        {
            try
            {
                Request request = new Request
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Action = "HISTORY",
                    Data = new
                    {
                        sessionToken = SessionToken
                    }
                };

                Response response = TcpJsonClient.Send(request);

                if (!response.Success)
                {
                    MessageBox.Show(response.Message);
                    return;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("Ngày chấm công");
                dt.Columns.Add("Giờ vào");
                dt.Columns.Add("Giờ ra");
                dt.Columns.Add("Mã ca");
                dt.Columns.Add("Tên ca");
                dt.Columns.Add("Trạng thái");

                if (response.Data.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (System.Text.Json.JsonElement row in response.Data.EnumerateArray())
                    {
                        dt.Rows.Add(
                            row.GetProperty("ngayChamCong").ToString(),
                            row.GetProperty("gioVao").ToString(),
                            row.GetProperty("gioRa").ToString(),
                            row.GetProperty("maCa").ToString(),
                            row.GetProperty("tenCa").ToString(),
                            row.GetProperty("trangThai").ToString()
                        );
                    }
                }

                dgvLichSu.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối Server: " + ex.Message);
            }
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnTaiLichSu_Click_1(object sender, EventArgs e)
        {
            btnTaiLichSu_Click(sender, e);
        }

        private void btnDong_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormLichSu_Load(object sender, EventArgs e)
        {

        }
    }
}