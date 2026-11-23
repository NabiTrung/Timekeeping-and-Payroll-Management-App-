using System;
using System.Text.Json;

namespace QuanLyChamCong.Models
{
    public static class TcpActions
    {
        public const string Ping = "PING";
        public const string Login = "LOGIN";
        public const string Register = "REGISTER";
        public const string CheckIn = "CHECKIN";
        public const string CheckOut = "CHECKOUT";
        public const string GetHistory = "GET_HISTORY";
    }

    public static class TcpErrorCodes
    {
        public const string ConnectionFailed = "ERR_CONNECTION_FAILED";
        public const string Timeout = "ERR_TIMEOUT";
        public const string Cancelled = "ERR_CANCELLED";
        public const string ServerDisconnected = "ERR_SERVER_DISCONNECTED";
        public const string InvalidResponse = "ERR_INVALID_RESPONSE";
        public const string RequestIdMismatch = "ERR_REQUEST_ID_MISMATCH";
        public const string ClientError = "ERR_CLIENT_ERROR";
    }

    public class RequestPacket
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString("N");
        public string Action { get; set; } = string.Empty;
        public string? MaNV { get; set; }
        public object? Data { get; set; }
    }

    public class ResponsePacket
    {
        private static readonly JsonSerializerOptions DefaultDeserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public string RequestId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public JsonElement? Data { get; set; }

        public string FormatErrorDisplay(string fallbackMessage = "Thao tác thất bại.")
        {
            string message = string.IsNullOrWhiteSpace(Message) ? fallbackMessage : Message;
            if (!string.IsNullOrWhiteSpace(ErrorCode))
            {
                return $"[{ErrorCode}] {message}";
            }

            return message;
        }

        public T? DeserializeData<T>(JsonSerializerOptions? options = null)
        {
            if (!Data.HasValue ||
                Data.Value.ValueKind == JsonValueKind.Null ||
                Data.Value.ValueKind == JsonValueKind.Undefined)
            {
                return default;
            }

            return Data.Value.Deserialize<T>(options ?? DefaultDeserializeOptions);
        }
    }

    public class LoginRequestData
    {
        public string MatKhau { get; set; } = string.Empty;
    }

    public class LoginResponseData
    {
        public string? MaNV { get; set; }
        public string? Ten { get; set; }
        public string? TenNV { get; set; }
        public string? ChucVu { get; set; }
    }

    public class RegisterRequestData
    {
        public string Ten { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
    }

    public class CheckInRequestData
    {
        public string? ImagePath { get; set; }
        public string? ImageBase64 { get; set; }
    }

    public class CheckInResponseData
    {
        public string? GioVao { get; set; }
        public string? TrangThai { get; set; }
        public string? AnhCheckIn { get; set; }
    }

    public class CheckOutResponseData
    {
        public string? GioRa { get; set; }
        public string? TrangThai { get; set; }
    }

    public class HistoryItemDto
    {
        public string? NgayChamCong { get; set; }
        public string? GioVao { get; set; }
        public string? GioRa { get; set; }
        public string? IP { get; set; }
        public string? TrangThai { get; set; }
        public string? AnhCheckIn { get; set; }
    }

    public class HistoryResponseWrapper
    {
        public List<HistoryItemDto>? Records { get; set; }
        public List<HistoryItemDto>? Items { get; set; }
        public List<HistoryItemDto>? History { get; set; }
    }
}
