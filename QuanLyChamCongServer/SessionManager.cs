
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace QuanLyChamCongServer
{
    public static class SessionManager
    {
        // Lưu Token và mã nhân viên tương ứng.
        private static readonly ConcurrentDictionary<string, string>
            Sessions = new();

        // Tạo Session mới.
        public static string TaoSession(string maNV)
        {
            string token = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

            Sessions[token] = maNV;

            return token;
        }

        // Kiểm tra Token và lấy mã nhân viên.
        public static bool KiemTraSession(
            string token, out string maNV)
        {
            return Sessions.TryGetValue(token, out maNV!);
        }

        // Hủy Session khi đăng xuất.
        public static bool XoaSession(string token)
        {
            return Sessions.TryRemove(token, out _);
        }
    }
}
