using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Entities;

namespace RSToolKit.WebUI.Models
{
    public class PageInfo
    {
        public Guid UId { get; set; }
        public bool Enabled { get; set; }
        public int PageNumber { get; set; }
    }
}