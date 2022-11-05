using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RSToolKit.Domain.Entities.Email
{
    public class Unsubscribe
    {
        [Key]
        public Guid UId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }
        public string Email { get; set; }
        public Guid UnsubscribeFrom { get; set; }
        public DateTimeOffset DateSubmitted { get; set; }

        public Unsubscribe()
        {
            DateSubmitted = DateTimeOffset.UtcNow;
        }

        public static Unsubscribe New(string email, Guid unsubscribeFrom)
        {
            return new Unsubscribe()
            {
                UId = Guid.NewGuid(),
                Email = email,
                UnsubscribeFrom = unsubscribeFrom
            };
        }
    }
}
