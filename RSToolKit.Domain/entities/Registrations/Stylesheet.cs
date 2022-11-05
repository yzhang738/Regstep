using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Entities
{
    public class Stylesheet : IRSData
    {

        public List<CSS> Css { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }

        public Guid ModifiedBy { get; set; }

        public Stylesheet()
        {
            Css = new List<CSS>();
        }
    }
}
