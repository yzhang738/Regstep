using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain.JItems;
using RSToolKit.Domain.Data;

namespace RSToolKit.Domain.Engines
{
    public class FilterEngine
    {
        public static IEnumerable<T> Filter<T>(IEnumerable<T> items, IEnumerable<JTableFilter> filters)
            where T : IFilterable
        {
            var filtereditems = new List<T>();
            foreach (var item in items)
            {
                // Now we start the filtering.
                var enumerator = filters.GetEnumerator();
                var passed = true;
                var groupTest = true;
                JTableFilter first_groupFilter = null;
                while (enumerator.MoveNext())
                {
                    if (first_groupFilter == null)
                        first_groupFilter = enumerator.Current;
                    var filter = enumerator.Current;
                    var test = item.TestValue(filter.ActingOn, filter.Value, filter.Test, filter.CaseSensitive);
                    if (filter == first_groupFilter)
                    {
                        groupTest = test;
                    }
                    else
                    {
                        switch (filter.Link)
                        {
                            case "and":
                                groupTest &= test;
                                break;
                            case "or":
                                groupTest |= test;
                                break;
                            default:
                                groupTest = test;
                                break;
                        }
                    }
                    if (filter.GroupNext)
                        continue;
                    switch (first_groupFilter.Link)
                    {
                        case "and":
                            passed &= groupTest;
                            break;
                        case "or":
                            passed |= groupTest;
                            break;
                        default:
                            passed = groupTest;
                            break;
                    }
                    groupTest = true;
                    first_groupFilter = null;
                }
                if (passed)
                    filtereditems.Add(item);
            }
            return filtereditems;
        }
    }
}
