using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace RSToolKit.WebUI
{
    /// <summary>
    /// A <code>JsonResult</code> that using Newtonsoft Json.Net to serialize the information.
    /// </summary>
    public class JsonNetResult
        : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            var response = context.HttpContext.Response;
            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data == null)
            {
                return;
            }
            var serializedObject = JsonConvert.SerializeObject(Data, Formatting.Indented);
            response.Write(serializedObject);
        }

        /// <summary>
        /// Creates a new JsonNetResult success object.
        /// </summary>
        /// <param name="location">The location the object can be accessed at.</param>
        /// <param name="message">The message to display.</param>
        /// <returns>The JsonResult.</returns>
        public static JsonNetResult Success(string location = null, string message = "The operation completed successfuly.", object data = null)
        {
            var d = Infrastructure.Json.JsonResultData.Success(location, message, data);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = d };
        }

        /// <summary>
        /// Creates a new JsonNetResult failure object.
        /// </summary>
        /// <param name="location">The location the object can be accessed at.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="errors">The list of errors.</param>
        /// <returns></returns>
        public static JsonNetResult Failure(string location = null, string message = "The operation failed", IEnumerable<Infrastructure.Json.JsonError> errors = null, object data = null)
        {
            var d = Infrastructure.Json.JsonResultData.Failure(location, message, errors, data);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = d };
        }

        /// <summary>
        /// Creates a new JsonNetResult failure object.
        /// </summary>
        /// <param name="location">The location the object can be accessed at.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="modelState">The state to grab errors from.</param>
        /// <returns></returns>
        public static JsonNetResult Failure(ModelStateDictionary modelState, string location = null, string message = "The operation failed", object data = null)
        {
            var d = Infrastructure.Json.JsonResultData.Failure(location, message, modelState, data);
            return new JsonNetResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = d };
        }

    }
}