using System;
using System.Collections.Generic;
using System.Text;


using System.Text.Json;

namespace QuanLyChamCong
{
    public class Response
    {
        public string RequestId { get; set; } = "";
        public bool Success { get; set; }
        public string ErrorCode { get; set; } = "NONE";
        public string Message { get; set; } = "";
        public JsonElement Data { get; set; }
    }
}

