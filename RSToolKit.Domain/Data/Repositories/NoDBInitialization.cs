using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace RSToolKit.Domain.Data.Repositories
{
    /// <summary>
    /// Prevents the database from initializing.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NoDBInitialization<T>
        : IDatabaseInitializer<T>
        where T : DbContext
    {
        public void InitializeDatabase(T context)
        {
            // Do nothing, thats the sense of it!
        }
    }
}
