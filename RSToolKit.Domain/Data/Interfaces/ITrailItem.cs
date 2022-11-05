using System;
namespace RSToolKit.Domain.Data
{
    public interface ITrailItem
    {
        string Action { get; set; }
        DateTimeOffset ActionDate { get; set; }
        string Controller { get; set; }
        Guid Id { get; set; }
        string Label { get; set; }
        System.Collections.Generic.Dictionary<string, string> Parameters { get; set; }
        System.Web.Routing.RouteValueDictionary RVD();
    }
}
