using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RSToolKit.Domain.Entities.Clients
{
    /// <summary>
    /// Holds the information on the logo.
    /// </summary>
    [Table("CompanyLogos")]
    public class CompanyLogo
        : INode
    {
        /// <summary>
        /// The primary key and sorting index.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        /// <summary>
        /// The unique identifier.
        /// </summary>
        [Index(IsClustered = false, IsUnique = true)]
        public Guid UId { get; set; }
        /// <summary>
        /// The date created.
        /// </summary>
        public DateTimeOffset DateCreated { get; set; }
        /// <summary>
        /// The date modified.
        /// </summary>
        public DateTimeOffset DateModified { get; set; }
        /// <summary>
        /// The modification token.
        /// </summary>
        public Guid ModificationToken { get; set; }
        /// <summary>
        /// The last person to modify the file.
        /// </summary>
        public Guid ModifiedBy { get; set; }
        /// <summary>
        /// The binary representation of the image.
        /// </summary>
        public byte[] BinaryData { get; set; }
        /// <summary>
        /// The file type as MIME.
        /// </summary>
        public string MIME { get; set; }
        /// <summary>
        /// The company key.
        /// </summary>
        public Guid CompanyKey { get; set; }
        /// <summary>
        /// The company that owns the item.
        /// </summary>
        [ForeignKey("CompanyKey")]
        public Company Company { get; set; }
        /// <summary>
        /// The size of the image in bytes.
        /// </summary>
        public long SizeInBytes { get; set; }
        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public CompanyLogo()
        {
            BinaryData = new byte[0];
        }
    }
}
