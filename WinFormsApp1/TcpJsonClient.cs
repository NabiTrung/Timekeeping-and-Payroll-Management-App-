
using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Text.Json;

namespace QuanLyChamCong
{
    public static class TcpJsonClient
    {
        private const int MaxMessageSize = 64 * 1024;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static Response Send(Request request)
        {
            using TcpClient client = new TcpClient();

            client.Connect("127.0.0.1", 8888);

            using NetworkStream stream = client.GetStream();

            string json = JsonSerializer.Serialize(
                request, JsonOptions);

            byte[] requestBytes =
                new UTF8Encoding(false).GetBytes(json + "\n");

            if (requestBytes.Length > MaxMessageSize)
                throw new InvalidDataException(
                    "Request vượt quá 64 KB.");

            stream.Write(requestBytes, 0, requestBytes.Length);

            using StreamReader reader = new StreamReader(
                stream, Encoding.UTF8);

            string? responseJson = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(responseJson))
                throw new IOException(
                    "Server không trả về dữ liệu.");

            if (Encoding.UTF8.GetByteCount(responseJson) >
                MaxMessageSize)
            {
                throw new InvalidDataException(
                    "Response vượt quá 64 KB.");
            }

            Response? response =
                JsonSerializer.Deserialize<Response>(
                    responseJson, JsonOptions);

            return response ?? throw new JsonException(
                "Response JSON không hợp lệ.");
        }
    }
}
