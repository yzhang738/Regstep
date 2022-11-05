using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data
{
    public class CascadeDeleteAttribute : Attribute
    {
        public CascadeDeleteAttribute()
        {

        }
    }

    public class ClearKeyOnDeleteAttribute : Attribute
    {
        public string PropertyName { get; set;}

        public ClearKeyOnDeleteAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    public class ClearRelationshipAttribute : Attribute
    {
        public string PropertyName { get; set; }
        public string Relationship { get; set; }

        public ClearRelationshipAttribute(string propertyName, string relationship = "")
        {
            PropertyName = propertyName;
            Relationship = relationship;
        }
    }
}
