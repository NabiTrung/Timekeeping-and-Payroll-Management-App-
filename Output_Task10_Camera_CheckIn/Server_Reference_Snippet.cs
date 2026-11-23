// FILE HƯỚNG DẪN THAM KHẢO CHO SERVER (Task 11 / Task 12)
// Không compile file này trực tiếp nguyên xi đè lên project; hãy copy 2 phần phương thức bên dưới vào:
//   1) DatabaseHelper.cs (namespace QuanLyChamCong)
//   2) ServerManager.cs (namespace QuanLyChamCongServer)

using System;
using System.Data;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace QuanLyChamCongServer.ReferenceSnippets
{
    internal static class ServerCameraIntegrationReference
    {
        /* ============================================================================
         * PHẦN 1: Cập nhật trong QuanLyChamCong\DatabaseHelper.cs
         * - Giữ tham số mặc định anhCheckIn = null để tương thích ngược với code cũ.
         * - Bổ sung cột AnhCheckIn vào INSERT và SELECT lịch sử chấm công.
         * ============================================================================
         *
         * public bool CheckIn(string maNV, string ip, string? anhCheckIn = null)
         * {
         *     try
         *     {
         *         using (SqlConnection conn = GetConnection())
         *         {
         *             conn.Open();
         *
         *             string sqlKiemTra = @"
         *                 SELECT COUNT(*)
         *                 FROM ChamCong
         *                 WHERE MaNV = @MaNV
         *                   AND NgayChamCong = @NgayChamCong";
         *
         *             using (SqlCommand cmdKiemTra = new SqlCommand(sqlKiemTra, conn))
         *             {
         *                 cmdKiemTra.Parameters.AddWithValue("@MaNV", maNV);
         *                 cmdKiemTra.Parameters.AddWithValue("@NgayChamCong", DateTime.Today);
         *
         *                 int soBanGhi = (int)cmdKiemTra.ExecuteScalar();
         *                 if (soBanGhi > 0)
         *                 {
         *                     return false;
         *                 }
         *             }
         *
         *             DateTime gioVao = DateTime.Now;
         *             string trangThai = gioVao.TimeOfDay > new TimeSpan(8, 0, 0)
         *                 ? "DiMuon"
         *                 : "DungGio";
         *
         *             string sqlThem = @"
         *                 INSERT INTO ChamCong
         *                     (MaNV, NgayChamCong, GioVao, IP, TrangThai, AnhCheckIn)
         *                 VALUES
         *                     (@MaNV, @NgayChamCong, @GioVao, @IP, @TrangThai, @AnhCheckIn)";
         *
         *             using (SqlCommand cmdThem = new SqlCommand(sqlThem, conn))
         *             {
         *                 cmdThem.Parameters.AddWithValue("@MaNV", maNV);
         *                 cmdThem.Parameters.AddWithValue("@NgayChamCong", DateTime.Today);
         *                 cmdThem.Parameters.AddWithValue("@GioVao", gioVao);
         *                 cmdThem.Parameters.AddWithValue("@IP", ip);
         *                 cmdThem.Parameters.AddWithValue("@TrangThai", trangThai);
         *                 cmdThem.Parameters.AddWithValue(
         *                     "@AnhCheckIn",
         *                     string.IsNullOrWhiteSpace(anhCheckIn) ? (object)DBNull.Value : anhCheckIn);
         *
         *                 return cmdThem.ExecuteNonQuery() > 0;
         *             }
         *         }
         *     }
         *     catch (SqlException)
         *     {
         *         return false;
         *     }
         * }
         *
         * public DataTable LayLichSuChamCong(string maNV)
         * {
         *     DataTable bangDuLieu = new DataTable();
         *
         *     using (SqlConnection conn = GetConnection())
         *     {
         *         conn.Open();
         *
         *         string sql = @"
         *             SELECT NgayChamCong, GioVao, GioRa, IP, TrangThai, AnhCheckIn
         *             FROM ChamCong
         *             WHERE MaNV = @MaNV
         *             ORDER BY NgayChamCong DESC";
         *
         *         using (SqlCommand cmd = new SqlCommand(sql, conn))
         *         {
         *             cmd.Parameters.AddWithValue("@MaNV", maNV);
         *             using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
         *             {
         *                 adapter.Fill(bangDuLieu);
         *             }
         *         }
         *     }
         *
         *     return bangDuLieu;
         * }
         */

        /* ============================================================================
         * PHẦN 2: Cập nhật trong QuanLyChamCongServer\ServerManager.cs (Action CHECKIN)
         * - Giải mã ImageBase64 từ JSON Request Data, lưu ảnh an toàn vào thư mục Server.
         * - Lưu đường dẫn tương đối vào cột AnhCheckIn trong SQL Server.
         * - Trả JSON ResponsePacket khớp RequestId cho Client.
         * ============================================================================ */

        public static string? LuuAnhCheckInTuBase64(string maNV, string? imagePathHint, string? imageBase64)
        {
            if (string.IsNullOrWhiteSpace(imageBase64))
            {
                return string.IsNullOrWhiteSpace(imagePathHint) ? null : imagePathHint;
            }

            byte[] imageBytes = Convert.FromBase64String(imageBase64);
            string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerImages", "CheckIn");
            Directory.CreateDirectory(baseFolder);

            string? hintedFileName = string.IsNullOrWhiteSpace(imagePathHint)
                ? null
                : Path.GetFileName(imagePathHint);

            string fileName = !string.IsNullOrWhiteSpace(hintedFileName)
                ? hintedFileName
                : $"{maNV}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";

            // Lọc ký tự lạ để tránh Path Traversal.
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalid, '_');
            }

            string fullPath = Path.Combine(baseFolder, fileName);
            File.WriteAllBytes(fullPath, imageBytes);

            return $"ServerImages/CheckIn/{fileName}";
        }

        /*
         * Ví dụ đoạn xử lý case "CHECKIN" trong ServerManager.cs khi Server dùng JSON (Task 11):
         *
         * case "CHECKIN":
         * {
         *     string maNVCheckIn = request.MaNV?.Trim() ?? string.Empty;
         *     if (string.IsNullOrWhiteSpace(maNVCheckIn))
         *     {
         *         return ErrorResponse(request.RequestId, "INVALID_MA_NV", "Ma nhan vien khong hop le");
         *     }
         *
         *     string? imagePath = null;
         *     string? imageBase64 = null;
         *     if (request.Data is JsonElement dataEl && dataEl.ValueKind == JsonValueKind.Object)
         *     {
         *         if (dataEl.TryGetProperty("ImagePath", out var pathProp))
         *             imagePath = pathProp.GetString();
         *         if (dataEl.TryGetProperty("ImageBase64", out var b64Prop))
         *             imageBase64 = b64Prop.GetString();
         *     }
         *
         *     string? savedRelativePath = LuuAnhCheckInTuBase64(maNVCheckIn, imagePath, imageBase64);
         *
         *     var db = new QuanLyChamCong.DatabaseHelper();
         *     bool ok = db.CheckIn(maNVCheckIn, ip, savedRelativePath);
         *     if (!ok)
         *     {
         *         return ErrorResponse(request.RequestId, "CHECKIN_FAILED", "Cham cong that bai, co the da cham cong hom nay");
         *     }
         *
         *     return new
         *     {
         *         RequestId = request.RequestId,
         *         Success = true,
         *         ErrorCode = "",
         *         Message = "Cham cong vao thanh cong",
         *         Data = new
         *         {
         *             GioVao = DateTime.Now.ToString("HH:mm:ss"),
         *             TrangThai = DateTime.Now.TimeOfDay > new TimeSpan(8, 0, 0) ? "DiMuon" : "DungGio",
         *             AnhCheckIn = savedRelativePath
         *         }
         *     };
         * }
         */
    }
}
