using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection;

namespace RSToolKit.Domain.Data
{
    public interface ISecure
        : IRSData
    {
        bool Delete { get; set; }
        string Value { get; }
        Guid UId { get; set; }
        string Peek();
    }

    public interface ISecureHolder
        : IPerson
    {
    }

    public interface ISecureMap
        : IPersonData
    {
    }

    public interface ISecureGroup
        : INode
    {
    }

    public enum SecureAccessType
    {
        [StringValue("Unknown")]
        Unknown = 0,
        [StringValue("Insertion")]
        Insertion = 1,
        [StringValue("Read")]
        Read = 2,
        [StringValue("Write")]
        Write = 3,
        [StringValue("Delete")]
        Delete = 4,
        [StringValue("Clean Up Deletion")]
        DeleteCleanUp = 5
    }
}
