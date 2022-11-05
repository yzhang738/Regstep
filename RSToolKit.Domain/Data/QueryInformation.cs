using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSToolKit.Domain;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities;
using RSToolKit.Domain.Entities.Clients;

namespace RSToolKit.Domain.Data
{
    public class SortingInformation : IComparer<SortingInformation>, IComparable<SortingInformation>
    {
        public string ActingOn { get; set; }
        public bool Descending { get; set; }

        [JsonIgnore]
        public string Type
        {
            get
            {
                if (Descending) return "DESC";
                else return "ASC";
            }
        }

        public SortingInformation()
        {
            ActingOn = "Id";
            Descending = true;
        }

        public int CompareTo(SortingInformation other)
        {
            return ActingOn.CompareTo(other.ActingOn);
        }

        public int Compare(SortingInformation x, SortingInformation y)
        {
            return x.CompareTo(y);
        }
    }

    public class FilterInformation : IFilter, IComparable<FilterInformation>
    {
        public FilterLink Link { get; set; }
        public LogicTest Test { get; set; }
        public string ActingOn { get; set; }
        public string Value { get; set; }
        public int Priority { get; set; }
        public bool GroupNext { get; set; }

        public FilterInformation()
        {
            Link = FilterLink.None;
            Test = LogicTest.Equal;
            ActingOn = "Id";
            Value = "1";
            Priority = 1;
            GroupNext = false;
        }

        public int CompareTo(FilterInformation other)
        {
            return Priority.CompareTo(other.Priority);
        }

        public static IEnumerable<Titem> Filter<Titem>(IEnumerable<Titem> list, IEnumerable<IFilter> filters, Company company, FormsRepository repository)
            where Titem : IFilterable
        {
            if (filters.Count() == 0)
                return list;
            var filtered = new List<Titem>();
            var take = true;
            var tests = new List<Tuple<bool, FilterLink>>();
            var sortedFilters = filters.OrderBy(f => f.Priority).ToList();
            foreach (var item in list)
            {
                for (var i = 0; i < sortedFilters.Count; i++)
                {
                    var filter = sortedFilters[i];
                    var groupTest = true;
                    var first = true;
                    var grouping = filter.GroupNext;
                    var test = false;
                    do
                    {
                        if (!filter.GroupNext)
                        {
                            grouping = false;
                        }
                        test = item.TestValue(filter.ActingOn, filter.Value, filter.Test, company, repository);
                        switch (filter.Link)
                        {
                            case FilterLink.And:
                                groupTest = groupTest && test;
                                break;
                            case FilterLink.Or:
                                if (first)
                                    groupTest = test;
                                else
                                    groupTest = groupTest || test;
                                break;
                            case FilterLink.None:
                            default:
                                groupTest = test;
                                break;
                        }
                        first = false;
                        if (!grouping)
                            break;
                        i++;
                        if (i < sortedFilters.Count)
                            filter = sortedFilters[i];
                        else
                            break;
                    } while (grouping);
                    tests.Add(new Tuple<bool, FilterLink>(groupTest, (i + 1) < sortedFilters.Count ? sortedFilters[(i + 1)].Link : FilterLink.None));
                }
                take = tests.Count > 0 ? tests[0].Item1 : true;
                for (int i = 1; i < tests.Count; i++)
                {
                    switch (tests[i - 1].Item2)
                    {
                        case FilterLink.And:
                            take = take && tests[i].Item1;
                            break;
                        case FilterLink.Or:
                            take = take || tests[i].Item1;
                            break;
                        case FilterLink.None:
                        default:
                            take = tests[i].Item1;
                            break;
                    }
                }
                if (take)
                {
                    filtered.Add(item);
                }
            }
            return filtered.AsEnumerable<Titem>();
        }
    }

    public class FilterGroup
    {
        public List<FilterInformation> Filters { get; set; }
        public FilterLink Link { get; set; }
        public FilterGroup()
        {
            Filters = new List<FilterInformation>();
            Link = FilterLink.None;
        }
    }


    // New stuff that works like magic.
    // This shit right here.  This shit right here. This shit is called Black Ice!

    public class QueryFilter : IFilter
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Index(IsClustered = true, IsUnique = true)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [JsonIgnore]
        [ForeignKey("SingleFormReportKey")]
        public virtual SingleFormReport SingleFormReport { get; set; }
        public Guid? SingleFormReportKey { get; set; }

        [JsonIgnore]
        [ForeignKey("ContactReportKey")]
        public virtual ContactReport ContactReport { get; set; }
        public Guid? ContactReportKey { get; set; }

        public FilterLink Link { get; set; }
        public LogicTest Test { get; set; }

        public string ActingOn { get; set; }
        public string Value { get; set; }

        [NotMapped]
        public int Priority
        {
            get
            {
                return Order;
            }
            set
            {
                return;
            }
        }

        public int Order { get; set; }
        public bool GroupNext { get; set; }

        public QueryFilter()
        {
            UId = Guid.Empty;
            Order = -1;
            Test = LogicTest.Equal;
            Link = FilterLink.None;
            GroupNext = false;
        }

        public static QueryFilter New(FilterLink link = FilterLink.None, LogicTest test = LogicTest.Equal, string actingOn = "", string value = "", int order = -1, bool groupNext = false, SingleFormReport singleFormReport = null, ContactReport contactReport = null)
        {
            var filter = new QueryFilter()
            {
                UId = Guid.NewGuid(),
                Link = link,
                Test = test,
                ActingOn = actingOn,
                Value = value,
                Order = order,
                GroupNext = groupNext,
                SingleFormReport = singleFormReport,
                SingleFormReportKey = singleFormReport != null ? (Guid?)singleFormReport.UId : null,
                ContactReport = contactReport,
                ContactReportKey = contactReport != null ? (Guid?)contactReport.UId : null
            };
            return filter;
        }

        public static QueryFilter New(QueryFilter filter)
        {
            var n_filter = new QueryFilter()
            {
                UId = Guid.NewGuid(),
                Link = filter.Link,
                Test = filter.Test,
                ActingOn = filter.ActingOn,
                Value = filter.Value,
                Order = filter.Order,
                GroupNext = filter.GroupNext,
                SingleFormReport = filter.SingleFormReport,
                SingleFormReportKey = filter.SingleFormReportKey,
                ContactReport = filter.ContactReport,
                ContactReportKey = filter.ContactReportKey
            };
            return n_filter;
        }
    }

    public enum FilterLink
    {
        None = 0,
        And = 1,
        Or = 2
    }

    public enum FilterTest
    {
        Equal = 0,
        NotEqual = 1,
        GreaterThan = 2,
        LessThan = 3,
        GreaterThanOrEqual = 4,
        LessThanOrEqual = 5,
        In = 6,
        NotIn = 7,
        StartsWith = 8,
        Contains = 9,
        EndsWith = 10
    }
}
