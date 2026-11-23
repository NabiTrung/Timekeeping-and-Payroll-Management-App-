using System;
using System.ComponentModel;
using System.Windows.Forms;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;

namespace QuanLyChamCong
{
    public partial class FormChamCong : Form
    {
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

        private void FormChamCong_Load(object sender, EventArgs e)
        {
            lblMaNV.Text = "Mã nhân viên: " + MaNV;
            lblTenNV.Text = "Nhân viên: " + TenNV;
            lblChucVu.Text = "Chức vụ: " + ChucVu;
        }

        private async void btnCheckIn_Click(object sender, EventArgs e)
        {
            btnCheckIn.Enabled = false;
            btnCheckOut.Enabled = false;

            try
            {
                ResponsePacket response = await TcpClientHelper.SendRequestAsync(
                    TcpActions.CheckIn,
                    MaNV);

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
    }
}
