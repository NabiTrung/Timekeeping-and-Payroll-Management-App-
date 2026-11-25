
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Text.Json;

namespace QuanLyChamCongServer
{
    public class ServerManager
    {
        private TcpListener? listener;
        private CancellationTokenSource? cts;

        public bool DangChay { get; private set; }

        public event Action<string>? CoNhatKy;

        public async Task BatDauAsync()
        {
            if (DangChay)
                return;

            try
            {
                cts = new CancellationTokenSource();

                listener = new TcpListener(IPAddress.Any, 8888);
                listener.Start();

                DangChay = true;
                CoNhatKy?.Invoke("Server đang lắng nghe cổng 8888.");

                while (!cts.Token.IsCancellationRequested)
                {
                    TcpClient client =
                        await listener.AcceptTcpClientAsync();

                    _ = Task.Run(() => XuLyClient(client));
                }
            }
            catch (ObjectDisposedException)
            {
                // Listener đã được dừng.
            }
            catch (SocketException)
            {
                if (DangChay)
                    CoNhatKy?.Invoke("Lỗi socket khi nhận kết nối.");
            }
            catch (Exception ex)
            {
                CoNhatKy?.Invoke("Lỗi Server: " + ex.Message);
            }
            finally
            {
                DangChay = false;
                CoNhatKy?.Invoke("Server đã dừng.");
            }
        }

        private void XuLyClient(TcpClient client)
        {
            string ip = "Không xác định";

            try
            {
                ip = ((IPEndPoint)client.Client.RemoteEndPoint!)
                    .Address.ToString();

                CoNhatKy?.Invoke("Client kết nối: " + ip);

                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    try
                    {
                        Request? request =
                            JsonTcpHelper.ReadRequest(stream);

                        if (request == null)
                            return;

                        CoNhatKy?.Invoke(
                            "Nhận JSON: " + request.Action);

                        Response response =
                            XuLyRequest(request, ip);

                        JsonTcpHelper.WriteResponse(
                            stream, response);
                    }
                    catch (JsonException)
                    {
                        JsonTcpHelper.WriteResponse(stream,
                            TaoResponse(
                                "",
                                false,
                                "INVALID_REQUEST",
                                "JSON không hợp lệ."));
                    }
                    catch (InvalidDataException)
                    {
                        JsonTcpHelper.WriteResponse(stream,
                            TaoResponse(
                                "",
                                false,
                                "INVALID_REQUEST",
                                "Dữ liệu vượt quá giới hạn cho phép."));
                    }
                }
            }
            catch (Exception ex)
            {
                CoNhatKy?.Invoke(
                    "Lỗi xử lý Client " + ip + ": " + ex.Message);
            }
        }
        private Response XuLyDangXuat(Request request)
        {
            string? token = LayDuLieu(request, "sessionToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return TaoResponse(
                    request.RequestId,
                    false,
                    "UNAUTHORIZED",
                    "Session Token không hợp lệ.");
            }

            if (!SessionManager.XoaSession(token))
            {
                return TaoResponse(
                    request.RequestId,
                    false,
                    "UNAUTHORIZED",
                    "Phiên đăng nhập không hợp lệ hoặc đã đăng xuất.");
            }

            return TaoResponse(
                request.RequestId,
                true,
                "NONE",
                "Đăng xuất thành công.");
        }

        private Response XuLyRequest(Request request, string ip)
        {
            string requestId = request.RequestId ?? "";

            if (string.IsNullOrWhiteSpace(request.Action))
            {
                return TaoResponse(
                    requestId, false,
                    "MISSING_FIELD",
                    "Thiếu action.");
            }

            switch (request.Action.ToUpperInvariant())
            {
                case "PING":
                    return TaoResponse(
                        requestId, true,
                        "NONE",
                        "Server đang hoạt động.");

                case "LOGIN":
                    return XuLyDangNhap(request);

                case "REGISTER":
                    return XuLyDangKy(request);

                case "CHECKIN":
                    return XuLyCheckIn(request, ip);

                case "CHECKOUT":
                    return XuLyCheckOut(request);

                case "HISTORY":
                    return XuLyLichSu(request);

                case "LOGOUT":
                    return XuLyDangXuat(request);

                default:
                    return TaoResponse(
                        requestId, false,
                        "UNKNOWN_ACTION",
                        "Chức năng chưa được hỗ trợ.");
            }
        }


        private Response XuLyDangNhap(Request request)
        {
            string? maNV = LayDuLieu(request, "maNV");
            string? matKhau = LayDuLieu(request, "matKhau");

            if (string.IsNullOrWhiteSpace(maNV) ||
                string.IsNullOrWhiteSpace(matKhau))
            {
                return TaoResponse(
                    request.RequestId, false,
                    "MISSING_FIELD",
                    "Vui lòng nhập mã nhân viên và mật khẩu.");
            }

            try
            {
                QuanLyChamCong.DatabaseHelper db =
                    new QuanLyChamCong.DatabaseHelper();

                bool thanhCong = db.DangNhap(
                    maNV, matKhau,
                    out string ten,
                    out string chucVu);

                if (thanhCong)
                {
                    string token = SessionManager.TaoSession(maNV);

                    return TaoResponse(
                        request.RequestId,
                        true,
                        "NONE",
                        "Đăng nhập thành công.",
                        new
                        {
                            maNV = maNV,
                            ten = ten,
                            chucVu = chucVu,
                            sessionToken = token
                        });
                }

                return TaoResponse(
                    request.RequestId, false,
                    "INVALID_CREDENTIALS",
                    "Sai mã nhân viên hoặc mật khẩu.");
            }
            catch (Exception ex)
            {
                CoNhatKy?.Invoke("Lỗi đăng nhập: " + ex.Message);

                return TaoResponse(
                    request.RequestId, false,
                    "INTERNAL_ERROR",
                    "Lỗi xử lý đăng nhập.");
            }
        }


        private Response XuLyDangKy(Request request)
        {
            string? maNV = LayDuLieu(request, "maNV");
            string? ten = LayDuLieu(request, "ten");
            string? matKhau = LayDuLieu(request, "matKhau");

            if (string.IsNullOrWhiteSpace(maNV) ||
                string.IsNullOrWhiteSpace(ten) ||
                string.IsNullOrWhiteSpace(matKhau))
            {
                return TaoResponse(
                    request.RequestId, false,
                    "MISSING_FIELD",
                    "Vui lòng nhập đầy đủ thông tin.");
            }

            try
            {
                QuanLyChamCong.DatabaseHelper db =
                    new QuanLyChamCong.DatabaseHelper();

                string matKhauHash =
                    QuanLyChamCong.PasswordHelper.BamMatKhau(matKhau);

                bool thanhCong = db.DangKyNhanVien(
                    maNV, ten, matKhauHash);

                if (thanhCong)
                {
                    return TaoResponse(
                        request.RequestId, true,
                        "NONE",
                        "Đăng ký thành công.");
                }

                return TaoResponse(
                    request.RequestId, false,
                    "ACCOUNT_EXISTS",
                    "Đăng ký thất bại, có thể mã nhân viên đã tồn tại.");
            }
            catch (Exception ex)
            {
                CoNhatKy?.Invoke("Lỗi đăng ký: " + ex.Message);

                return TaoResponse(
                    request.RequestId, false,
                    "INTERNAL_ERROR",
                    "Lỗi xử lý đăng ký.");
            }
        }


        private Response XuLyCheckIn(Request request, string ip)
        {
            string? token = LayDuLieu(request, "sessionToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return TaoResponse(
                    request.RequestId, false,
                    "UNAUTHORIZED",
                    "Vui lòng đăng nhập trước khi chấm công.");
            }

            if (!SessionManager.KiemTraSession(token, out string maNV))
            {
                return TaoResponse(
                    request.RequestId, false,
                    "UNAUTHORIZED",
                    "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");
            }

            try
            {
                QuanLyChamCong.DatabaseHelper db =
                    new QuanLyChamCong.DatabaseHelper();

                bool thanhCong = db.CheckIn(maNV, ip);

                if (thanhCong)
                {
                    return TaoResponse(
                        request.RequestId, true,
                        "NONE",
                        "Chấm công vào thành công.");
                }

                return TaoResponse(
                    request.RequestId, false,
                    "ALREADY_CHECKED_IN",
                    "Chấm công thất bại, có thể đã chấm công hôm nay.");
            }
            catch (Exception ex)
            {
                CoNhatKy?.Invoke("Lỗi CHECKIN: " + ex.Message);

                return TaoResponse(
                    request.RequestId, false,
                    "INTERNAL_ERROR",
                    "Lỗi xử lý chấm công vào.");
            }
        }



        private Response XuLyCheckOut(Request request)
        {
            string? token = LayDuLieu(request, "sessionToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return TaoResponse(
                    request.RequestId, false,
                    "UNAUTHORIZED",
                    "Vui lòng đăng nhập trước khi chấm công.");
            }

            if (!SessionManager.KiemTraSession(token, out string maNV))
            {
                return TaoResponse(
                    request.RequestId, false,
                    "UNAUTHORIZED",
                    "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");
            }

            try
            {
                QuanLyChamCong.DatabaseHelper db =
                    new QuanLyChamCong.DatabaseHelper();

                bool thanhCong = db.CheckOut(maNV);

                if (thanhCong)
                {
                    return TaoResponse(
                        request.RequestId, true,
                        "NONE",
                        "Chấm công ra thành công.");
                }

                return TaoResponse(
                    request.RequestId, false,
                    "NOT_CHECKED_IN",
                    "Chấm công ra thất bại, có thể chưa chấm công vào hoặc đã chấm công ra.");
            }
            catch (Exception ex)
            {
                CoNhatKy?.Invoke("Lỗi CHECKOUT: " + ex.Message);

                return TaoResponse(
                    request.RequestId, false,
                    "INTERNAL_ERROR",
                    "Lỗi xử lý chấm công ra.");
            }
        }


        private Response XuLyLichSu(Request request)
        {
            string? token = LayDuLieu(request, "sessionToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return TaoResponse(
                    request.RequestId, false,
                    "UNAUTHORIZED",
                    "Vui lòng đăng nhập trước khi xem lịch sử.");
            }

            if (!SessionManager.KiemTraSession(token, out string maNV))
            {
                return TaoResponse(
                    request.RequestId, false,
                    "UNAUTHORIZED",
                    "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");
            }

            try
            {
                QuanLyChamCong.DatabaseHelper db =
                    new QuanLyChamCong.DatabaseHelper();

                DataTable dt = db.LayLichSuChamCong(maNV);

                var rows = new System.Collections.Generic.List<object>();

                foreach (DataRow row in dt.Rows)
                {

                    rows.Add(new
                    {
                        ngayChamCong =
                            Convert.ToDateTime(row["NgayChamCong"])
                                .ToString("yyyy-MM-dd"),

                        gioVao = row["GioVao"] == DBNull.Value
                            ? ""
                            : Convert.ToDateTime(row["GioVao"])
                                .ToString("HH:mm:ss"),

                        gioRa = row["GioRa"] == DBNull.Value
                            ? ""
                            : Convert.ToDateTime(row["GioRa"])
                                .ToString("HH:mm:ss"),

                        maCa = row["MaCa"] == DBNull.Value
                            ? ""
                            : row["MaCa"].ToString(),

                        tenCa = row["TenCa"] == DBNull.Value
                            ? ""
                            : row["TenCa"].ToString(),

                        trangThai = row["TrangThai"] == DBNull.Value
                            ? ""
                            : row["TrangThai"].ToString()
                    });

                }

                return TaoResponse(
                    request.RequestId, true,
                    "NONE",
                    rows.Count == 0
                        ? "Chưa có lịch sử chấm công."
                        : "Lấy lịch sử chấm công thành công.",
                    rows);
            }
            catch (Exception ex)
            {
                CoNhatKy?.Invoke("Lỗi HISTORY: " + ex.Message);

                return TaoResponse(
                    request.RequestId, false,
                    "INTERNAL_ERROR",
                    "Lỗi xử lý lịch sử chấm công.");
            }
        }


        private string? LayDuLieu(Request request, string tenTruong)
        {
            if (request.Data is not JsonElement data ||
                data.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (!data.TryGetProperty(tenTruong, out JsonElement value))
                return null;

            if (value.ValueKind != JsonValueKind.String)
                return null;

            return value.GetString();
        }

        private Response TaoResponse(
            string? requestId,
            bool success,
            string errorCode,
            string message,
            object? data = null)
        {
            return new Response
            {
                RequestId = requestId ?? "",
                Success = success,
                ErrorCode = errorCode,
                Message = message,
                Data = data
            };
        }

        public void DungServer()
        {
            if (!DangChay)
                return;

            cts?.Cancel();
            listener?.Stop();

            DangChay = false;
            CoNhatKy?.Invoke("Đã gửi yêu cầu dừng Server.");
        }
    }
}
