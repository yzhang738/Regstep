using RSToolKit.Domain.Data;
using System;
using System.Linq;

namespace RSToolKit.Domain.Logging
{
    public static class Counting
    {
        public static long Increment(Guid key, CountType type = CountType.Unknown)
        {
            using (var context = new EFDbContext())
            {
                Counter count;
                if (type == CountType.Unknown)
                {
                    count = context.Counters.FirstOrDefault(c => c.Key == key);
                }
                else
                {
                    count = context.Counters.FirstOrDefault(c => c.Key == key && c.Type == type);
                }
                if (count == null)
                {
                    count = new Counter(key, type);
                    context.Counters.Add(count);
                }
                count.Count++;
                context.SaveChanges();
                return count.Count;
            }
        }
    }
}
