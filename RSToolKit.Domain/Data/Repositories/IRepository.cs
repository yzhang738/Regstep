using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using RSToolKit.Domain.Entities.Clients;
using System.Security.Principal;
using RSToolKit.Domain.Security;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Contains methods to retrieve Items from the database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public interface IRepository<T, K>
        : IRepository
        where T : IRSData
        where K : struct
    {
        T Find(K key, bool usePermissions = true);
        T First(Expression<Func<T, bool>> search, bool usePermissions = true);
        Task<T> FindAsync(K key, bool usePermissions = true);
        IEnumerable<T> Search(Expression<Func<T, bool>> search, bool usePermissions = true);
        Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> search, bool usePermissions = true);
        bool Remove(T node, bool usePermissions = true);
        Task<bool> RemoveAsync(T node, bool usePermissions = true);
        bool RemoveRange(IEnumerable<T> nodes, bool usePermissions = true);
        Task<bool> RemoveRangeAsync(IEnumerable<T> nodes, bool usePermissions = true);
        IEnumerable<T> RemoveNonPermissive(IEnumerable<T> nodes, SecurityAccessType accessType);
        T RemoveNonPermissive(T node, SecurityAccessType accessType);
        Task<IEnumerable<T>> RemoveNonPermissiveAsync(IEnumerable<T> nodes, SecurityAccessType accessType);
        Task<T> RemoveNonPermissiveAsync(T node, SecurityAccessType accessType);
        void Add(T node, bool usePermissions = true);
    }

    public interface IRepository
        : IDisposable
    {
        /// <summary>
        /// The context being used.
        /// </summary>
        EFDbContext Context { get; }
        /// <summary>
        /// A boolean letting us know if this context is unique to this scope.
        /// </summary>
        bool ContextInScope { get; }
        /// <summary>
        /// Commits the changes to the database.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        int Commit();
        /// <summary>
        /// Commits the changes to the database asyncrounously.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        Task<int> CommitAsync();
    }
}
