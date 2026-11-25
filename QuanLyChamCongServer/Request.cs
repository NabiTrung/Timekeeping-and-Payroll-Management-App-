using System;
using System.Collections.Generic;
using System.Text;


namespace QuanLyChamCongServer
{
    public class Request
    {
        public string RequestId { get; set; } = "";
        public string Action { get; set; } = "";
        public object? Data { get; set; }
    }
}

