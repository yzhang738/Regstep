using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Entities;

namespace RSToolKit.Domain.Data
{
    public interface IPerson
        : IEmailRecipient, INamedNode
    {
        IEnumerable<IPersonData> IData { get; }
        bool CompareData(string key, string value, string test, ref List<Guid> matchedContacts, bool caseSensitive = false);
        Guid HolderKey { get; }
        IPersonHolder Holder { get; set; }
        IEnumerable<ISecure> GetSecuredItems(RSToolKit.Domain.Entities.Clients.User user);
        IPersonData FindData(string key);
        SetDataResult SetData(string key, string value, bool ignoreValidation = false, bool ingnoreCapacity = false, bool ignoreRequired = false, bool resetValueOnError = true);
    }

    public interface IPersonHolder
        : ISecureGroup
    {
        IEnumerable<IPerson> Persons { get; }
    }

    public interface IPersonData 
        : INodeItem
    {
        Guid UId { get; set; }
        Guid ReferenceKey { get; }
        string Value { get; set; }
        Guid ParentKey { get; }
        IPerson Parent { get; set;}
    }
}
