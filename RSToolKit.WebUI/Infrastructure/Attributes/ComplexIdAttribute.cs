using System;
using System.Web.Mvc;

namespace RSToolKit.WebUI.Infrastructure
{
    /// <summary>
    /// Checks to see what the id is and sets it as either a long or a Guid. The parameter in question must be of type object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ComplexIdAttribute
        : ActionFilterAttribute
    {
        protected string _key;

        /// <summary>
        /// Initializes the attribute with the key to check for.
        /// </summary>
        /// <param name="key"></param>
        public ComplexIdAttribute(string parameter)
        {
            _key = parameter;
        }

        /// <summary>
        /// Parses the passed id into either a Guid or long and passes it as an object.
        /// </summary>
        /// <param name="filterContext">The current filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var stringValue = filterContext.RouteData.Values[_key] as string ?? filterContext.ActionParameters[_key].ToString();
            var value = _ParseId(stringValue);
            filterContext.ActionParameters[_key] = value;
        }

        /// <summary>
        /// Parses an id as a string into either a long sorting id or a unique identifier.
        /// </summary>
        /// <param name="id">The id to parse.</param>
        /// <returns>The id that was parsed, or -1L if unable to parse.</returns>
        protected object _ParseId(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                return -1L;
            long lid;
            Guid uid;
            object rid = null;
            if (!long.TryParse(id, out lid))
            {
                if (Guid.TryParse(id, out uid))
                {
                    rid = uid;
                }
            }
            else
            {
                rid = lid;
            }
            return rid ?? -1L;
        }

    }
}