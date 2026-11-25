using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyChamCong
{
    public class DatabaseHelper
    {
        private string connectionString =
    @"Data Source=AWGUSTORYA\MSSQLSERVER01;
      Initial Catalog=QuanLyChamCong;
      Integrated Security=True;
      TrustServerCertificate=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public bool KiemTraKetNoi()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool DangKyNhanVien(
    string maNV,
    string ten,
    string matKhauHash)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = @"INSERT INTO NhanVien
       (MaNV, Ten, MatKhauHash, ChucVu)
       VALUES
       (@MaNV, @Ten, @MatKhauHash, @ChucVu)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNV", maNV);
                        cmd.Parameters.AddWithValue("@Ten", ten);
                        cmd.Parameters.AddWithValue("@MatKhauHash", matKhauHash);
                        cmd.Parameters.AddWithValue("@ChucVu", "NhanVien");

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public bool DangNhap(
            string maNV,
            string matKhau,
            out string ten,
            out string chucVu)
        {
            ten = "";
            chucVu = "";

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                string sql = @"
            SELECT Ten, MatKhauHash, ChucVu
            FROM NhanVien
            WHERE MaNV = @MaNV";

                string matKhauDaBam;

                // Buoc 1: Lay thong tin tai khoan
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MaNV", maNV);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        ten = reader["Ten"].ToString();
                        chucVu = reader["ChucVu"].ToString();

                        matKhauDaBam = reader["MatKhauHash"].ToString();
                    }
                }

                // Buoc 2: Kiem tra mat khau
                bool hopLe = PasswordHelper.KiemTraMatKhau(
                    matKhau,
                    matKhauDaBam
                );

                if (!hopLe)
                    return false;

                // Buoc 3: Neu la PBKDF2 thi chuyen sang BCrypt
                if (PasswordHelper.CanNangCap(matKhauDaBam))
                {
                    string hashMoi = PasswordHelper.BamMatKhau(matKhau);

                    string sqlUpdate = @"
                UPDATE NhanVien
                SET MatKhauHash = @MatKhauMoi
                WHERE MaNV = @MaNV
                AND MatKhauHash = @MatKhauCu";

                    using (SqlCommand cmdUpdate =
                        new SqlCommand(sqlUpdate, conn))
                    {
                        cmdUpdate.Parameters.AddWithValue(
                            "@MatKhauMoi", hashMoi);

                        cmdUpdate.Parameters.AddWithValue(
                            "@MaNV", maNV);

                        cmdUpdate.Parameters.AddWithValue(
                            "@MatKhauCu", matKhauDaBam);

                        cmdUpdate.ExecuteNonQuery();
                    }
                }

                return true;
            }
        }

        public string ThuDangKy()
        {
            string matKhau = "123456";

            string matKhauHash =
                PasswordHelper.BamMatKhau(matKhau);

            bool ketQua = DangKyNhanVien(
                "NV002",
                "Nguyen Van B",
                matKhauHash
            );

            if (ketQua)
                return "Thêm nhân viên thành công!";

            return "Thêm thất bại! Có thể mã nhân viên đã tồn tại.";
        }

        public bool CheckIn(string maNV, string ip)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sqlKiemTra = @"
                SELECT COUNT(*)
                FROM ChamCong
                WHERE MaNV = @MaNV
                  AND NgayChamCong = @NgayChamCong";

                    using (SqlCommand cmdKiemTra =
                        new SqlCommand(sqlKiemTra, conn))
                    {
                        cmdKiemTra.Parameters.AddWithValue("@MaNV", maNV);
                        cmdKiemTra.Parameters.AddWithValue(
                            "@NgayChamCong", DateTime.Today);

                        int soBanGhi = (int)cmdKiemTra.ExecuteScalar();

                        if (soBanGhi > 0)
                            return false;
                    }

                    DateTime gioVao = DateTime.Now;

                    string trangThai =
                        gioVao.TimeOfDay > new TimeSpan(8, 0, 0)
                        ? "DiMuon"
                        : "DungGio";

                    string sqlThem = @"
                INSERT INTO ChamCong
                    (MaNV, NgayChamCong, GioVao, IP, TrangThai, MaCa)
                VALUES
                    (@MaNV, @NgayChamCong, @GioVao, @IP, @TrangThai, @MaCa)";

                    using (SqlCommand cmdThem =
                        new SqlCommand(sqlThem, conn))
                    {
                        cmdThem.Parameters.AddWithValue("@MaNV", maNV);
                        cmdThem.Parameters.AddWithValue(
                            "@NgayChamCong", DateTime.Today);
                        cmdThem.Parameters.AddWithValue("@GioVao", gioVao);
                        cmdThem.Parameters.AddWithValue("@IP", ip);
                        cmdThem.Parameters.AddWithValue("@TrangThai", trangThai);
                        cmdThem.Parameters.AddWithValue("@MaCa", "HC");

                        return cmdThem.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public bool CheckOut(string maNV)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = @"
                UPDATE ChamCong
                SET GioRa = @GioRa
                WHERE MaNV = @MaNV
                  AND NgayChamCong = @NgayChamCong
                  AND GioVao IS NOT NULL
                  AND GioRa IS NULL";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNV", maNV);
                        cmd.Parameters.AddWithValue("@NgayChamCong", DateTime.Today);
                        cmd.Parameters.AddWithValue("@GioRa", DateTime.Now);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }
        public DataTable LayLichSuChamCong(string maNV)
        {
            DataTable bangDuLieu = new DataTable();

            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = @"
    SELECT 
        cc.NgayChamCong,
        cc.GioVao,
        cc.GioRa,
        cc.IP,
        cc.TrangThai,
        cc.MaCa,
        cl.TenCa
    FROM ChamCong AS cc
    INNER JOIN CaLam AS cl
        ON cc.MaCa = cl.MaCa
    WHERE cc.MaNV = @MaNV
    ORDER BY cc.NgayChamCong DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNV", maNV);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(bangDuLieu);
                        }
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return bangDuLieu;
        }
        public string ThuCheckIn()
        {
            bool ketQua = CheckIn("NV001", "127.0.0.1");

            if (ketQua)
                return "Chấm công vào thành công!";

            return "Chấm công thất bại! Có thể nhân viên đã chấm công hôm nay.";
        }
        public string ThuCheckOut()
        {
            bool ketQua = CheckOut("NV001");

            if (ketQua)
                return "Chấm công ra thành công!";

            return "Chấm công ra thất bại! Có thể chưa chấm công vào hoặc đã chấm công ra.";
        }
        public string ThuLichSuChamCong()
        {
            DataTable bangDuLieu = LayLichSuChamCong("NV001");

            return "Số bản ghi chấm công: " + bangDuLieu.Rows.Count;
        }
        public bool DatLaiMatKhau(string maNV, string matKhauMoi)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                string sql = @"UPDATE NhanVien
       SET MatKhauHash = @MatKhauHash
       WHERE MaNV = @MaNV";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@MatKhauHash",
                        PasswordHelper.BamMatKhau(matKhauMoi)
                    );

                    cmd.Parameters.AddWithValue("@MaNV", maNV);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}