using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Collections
{
    public class UpdatableKeepAliveDictionary<T>
        : KeepAliveDictionary<T>, IUpdatableCollection
        where T : class, IUpdatable
    {
        IUpdatable IUpdatableCollection.Get(Guid key)
        {
            return this._dic[key] as IUpdatable;
        }

        IEnumerator<IUpdatable> IEnumerable<IUpdatable>.GetEnumerator()
        {
            var enumerator = this._dic.Values.GetEnumerator();
            while (enumerator.MoveNext())
                yield return enumerator.Current as IUpdatable;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
