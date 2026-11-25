
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace QuanLyChamCongServer
{
    public static class JsonTcpHelper
    {
        private const int MaxMessageSize = 64 * 1024;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static Request? ReadRequest(NetworkStream stream)
        {
            using MemoryStream memory = new MemoryStream();

            int value;

            while ((value = stream.ReadByte()) != -1)
            {
                if (value == '\n')
                    break;

                memory.WriteByte((byte)value);

                if (memory.Length > MaxMessageSize)
                    throw new InvalidDataException("Request vượt quá 64 KB.");
            }

            if (memory.Length == 0)
                return null;

            string json = Encoding.UTF8.GetString(memory.ToArray());

            return JsonSerializer.Deserialize<Request>(
                json.TrimEnd('\r'),
                JsonOptions);
        }

        public static void WriteResponse(
            NetworkStream stream,
            Response response)
        {
            string json = JsonSerializer.Serialize(
                response,
                JsonOptions);

            byte[] data = Encoding.UTF8.GetBytes(json + "\n");

            if (data.Length > MaxMessageSize)
                throw new InvalidDataException("Response vượt quá 64 KB.");

            stream.Write(data, 0, data.Length);
        }
    }
}

