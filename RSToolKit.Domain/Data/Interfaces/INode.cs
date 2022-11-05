using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Data;
using RSToolKit.Domain.Errors;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    public interface INode
        : IRSData
    {
        Guid UId { get; set; }
        DateTimeOffset DateCreated { get; set; }
        DateTimeOffset DateModified { get; set; }
        Guid ModificationToken { get; set; }
        Guid ModifiedBy { get; set; }
    }
}