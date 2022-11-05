using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public class Result
    {
        public string Message { get; set; }
        public bool Success { get; set; }

        public Result()
        {
            Message = "";
            Success = false;
        }
    }
}
