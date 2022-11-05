using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Components;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities
{
    public class RegistrantFile : IFormItem
    {

        [Key, ForeignKey("RegistrantData")]
        public Guid UId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        public virtual RegistrantData RegistrantData { get; set; }

        [Required]
        public string FileType { get; set; }

        [Required]
        public string Extension { get; set; }

        [Required]
        public byte[] BinaryData { get; set; }

        public RegistrantFile()
        {
            UId = Guid.NewGuid();
        }

        public Form GetForm()
        {
            return RegistrantData.GetForm();
        }

        public INode GetNode()
        {
            return RegistrantData.GetNode();
        }
    }
}
