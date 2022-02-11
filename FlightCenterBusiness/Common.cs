using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Kendo.Mvc;

namespace FlightCenterBusiness
{
    [Serializable]
    public static class Common
    {
        public static IQueryable<T> FilterMembers<T>(IQueryable<T> modelQuery, IList<IFilterDescriptor> filters)
        {
            foreach (var ifilter in filters)
            {
                if (ifilter.GetType() == typeof(CompositeFilterDescriptor))
                {
                    modelQuery = FilterMembers<T>(modelQuery, ((CompositeFilterDescriptor)ifilter).FilterDescriptors);
                }
                else
                {
                    modelQuery = FilterProperties<T>(modelQuery, ifilter);
                }
            }

            return modelQuery;
        }

        public static IQueryable<T> FilterProperties<T>(IQueryable<T> modelQuery, IFilterDescriptor iFilter)
        {
            var filter = (FilterDescriptor)iFilter;

            var valueObjectString = filter.Value.ToString();
            var filterValue = valueObjectString.ToLower();

            // Get the property type of the filter
            Type propertyType = typeof(T).GetProperty(filter.Member).PropertyType;

            // Apply filter setting for each filter type
            if (propertyType == typeof(int))
            {
                switch (filter.Operator)
                {
                    case FilterOperator.IsLessThan:
                        modelQuery = modelQuery.Where(r => (int)r.GetReflectedPropertyValue(filter.Member) < Convert.ToInt32(filterValue));
                        break;
                    case FilterOperator.IsLessThanOrEqualTo:
                        modelQuery = modelQuery.Where(r => (int)r.GetReflectedPropertyValue(filter.Member) <= Convert.ToInt32(filterValue));
                        break;
                    case FilterOperator.IsEqualTo:
                        modelQuery = modelQuery.Where(r => (int)r.GetReflectedPropertyValue(filter.Member) == Convert.ToInt32(filterValue));
                        break;
                    case FilterOperator.IsNotEqualTo:
                        modelQuery = modelQuery.Where(r => (int)r.GetReflectedPropertyValue(filter.Member) != Convert.ToInt32(filterValue));
                        break;
                    case FilterOperator.IsGreaterThanOrEqualTo:
                        modelQuery = modelQuery.Where(r => (int)r.GetReflectedPropertyValue(filter.Member) >= Convert.ToInt32(filterValue));
                        break;
                    case FilterOperator.IsGreaterThan:
                        modelQuery = modelQuery.Where(r => (int)r.GetReflectedPropertyValue(filter.Member) > Convert.ToInt32(filterValue));
                        break;
                }
            }
            else
            {
                switch (filter.Operator)
                {
                    case FilterOperator.Contains:
                        modelQuery = modelQuery.Where(r => ((string)r.GetReflectedPropertyValue(filter.Member)).Contains(filterValue));
                        break;
                    case FilterOperator.EndsWith:
                        modelQuery = modelQuery.Where(r => ((string)r.GetReflectedPropertyValue(filter.Member)).EndsWith(filterValue));
                        break;
                    case FilterOperator.IsEqualTo:
                        modelQuery = modelQuery.Where(r => ((string)r.GetReflectedPropertyValue(filter.Member)).Equals(filterValue));
                        break;
                    case FilterOperator.IsNotEqualTo:
                        modelQuery = modelQuery.Where(r => !((string)r.GetReflectedPropertyValue(filter.Member)).Equals(filterValue));
                        break;
                    case FilterOperator.StartsWith:
                        modelQuery = modelQuery.Where(r => ((string)r.GetReflectedPropertyValue(filter.Member)).StartsWith(filterValue));
                        break;
                }
            }

            return modelQuery;
        }

        public static IQueryable<T> SortMembers<T>(IQueryable<T> modelQuery, IList<SortDescriptor> sortDescriptors)
        {
            // Apply sorting from the UI if there is any
            if (sortDescriptors.Any())
            {
                foreach (SortDescriptor sortDescriptor in sortDescriptors)
                {
                    if (sortDescriptor.SortDirection == ListSortDirection.Ascending)
                    {
                        // Sort the field that is specified by sortDescriptor.Member
                        modelQuery = modelQuery.OrderBy(r => r.GetReflectedPropertyValue(sortDescriptor.Member));
                    }
                    else
                    {
                        // Sort the field that is specified by sortDescriptor.Member
                        modelQuery = modelQuery.OrderByDescending(r => r.GetReflectedPropertyValue(sortDescriptor.Member));
                    }
                }
            }

            return modelQuery;
        }

    }
}
