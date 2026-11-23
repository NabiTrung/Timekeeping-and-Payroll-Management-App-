using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;

namespace QuanLyChamCong
{
    public partial class FormLichSu : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MaNV { get; set; } = string.Empty;

        public FormLichSu()
        {
            InitializeComponent();
        }

        public FormLichSu(string maNV)
        {
            InitializeComponent();
            MaNV = maNV;
        }

        private async void btnTaiLichSu_Click(object sender, EventArgs e)
        {
            await TaiLichSuAsync();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void btnTaiLichSu_Click_1(object sender, EventArgs e)
        {
            await TaiLichSuAsync();
        }

        private void btnDong_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private async Task TaiLichSuAsync()
        {
            btnTaiLichSu.Enabled = false;

            try
            {
                ResponsePacket response = await TcpClientHelper.SendRequestAsync(
                    TcpActions.GetHistory,
                    MaNV);

                if (IsDisposed)
                    return;

                if (!response.Success)
                {
                    MessageBox.Show(
                        response.FormatErrorDisplay("Không thể tải lịch sử chấm công."),
                        "Lịch sử chấm công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                List<HistoryItemDto> items = ParseHistoryItems(response);

                DataTable dt = new DataTable();
                dt.Columns.Add("Ngày chấm công");
                dt.Columns.Add("Giờ vào");
                dt.Columns.Add("Giờ ra");
                dt.Columns.Add("Trạng thái");

                bool coAnhCheckIn = items.Any(item => !string.IsNullOrWhiteSpace(item.AnhCheckIn));
                if (coAnhCheckIn)
                {
                    dt.Columns.Add("Ảnh Check-In");
                }

                foreach (HistoryItemDto item in items)
                {
                    if (coAnhCheckIn)
                    {
                        dt.Rows.Add(
                            item.NgayChamCong ?? string.Empty,
                            item.GioVao ?? string.Empty,
                            item.GioRa ?? string.Empty,
                            item.TrangThai ?? string.Empty,
                            item.AnhCheckIn ?? string.Empty);
                    }
                    else
                    {
                        dt.Rows.Add(
                            item.NgayChamCong ?? string.Empty,
                            item.GioVao ?? string.Empty,
                            item.GioRa ?? string.Empty,
                            item.TrangThai ?? string.Empty);
                    }
                }

                dgvLichSu.DataSource = dt;

                if (items.Count == 0)
                {
                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(response.Message)
                            ? "Chưa có lịch sử chấm công."
                            : response.Message,
                        "Lịch sử chấm công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    MessageBox.Show(
                        "Lỗi xử lý dữ liệu lịch sử: " + ex.Message,
                        "Lịch sử chấm công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnTaiLichSu.Enabled = true;
                }
            }
        }

        private static List<HistoryItemDto> ParseHistoryItems(ResponsePacket response)
        {
            if (!response.Data.HasValue)
                return new List<HistoryItemDto>();

            JsonElement element = response.Data.Value;

            if (element.ValueKind == JsonValueKind.Array)
            {
                return response.DeserializeData<List<HistoryItemDto>>() ?? new List<HistoryItemDto>();
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                HistoryResponseWrapper? wrapper = response.DeserializeData<HistoryResponseWrapper>();
                if (wrapper?.Records != null) return wrapper.Records;
                if (wrapper?.Items != null) return wrapper.Items;
                if (wrapper?.History != null) return wrapper.History;
            }

            return new List<HistoryItemDto>();
        }
    }
}
