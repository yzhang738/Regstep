using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSToolKit.Domain.Entities
{
    public class Pointer : INodeItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [ForeignKey("FolderKey")]
        public virtual Folder Folder { get; set; }
        public Guid? FolderKey { get; set; }

        public Guid Target { get; set; }

        public Pointer()
        {
        }

        public INode GetNode()
        {
            return Folder as INode;
        }
    }
}
