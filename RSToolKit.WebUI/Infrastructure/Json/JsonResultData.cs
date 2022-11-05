using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace RSToolKit.WebUI.Infrastructure.Json
{
    /// <summary>
    /// A basic JsonResponse
    /// </summary>
    public class JsonResultData
    {
        /// <summary>
        /// Flag for successful process.
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// The message to return.
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// The location of the object in question.
        /// </summary>
        public string location { get; set; }
        /// <summary>
        /// The errors if any.
        /// </summary>
        public ICollection<JsonError> errors { get; set; }
        /// <summary>
        /// The data that is to be sent back.
        /// </summary>
        public object data { get; set; }

        /// <summary>
        /// Initializes the class with success.
        /// </summary>
        public JsonResultData()
        {
            success = true;
            message = "The operation completed successfuly.";
            location = null;
            errors = new List<JsonError>();
            data = null;
        }

        /// <summary>
        /// Creates a failure response.
        /// </summary>
        /// <param name="location">The location to access the object.</param>
        /// <param name="message">The fail message.</param>
        /// <param name="errors">The list of errors.</param>
        /// <returns>The fail response.</returns>
        public static JsonResultData Failure(string location = null, string message = "The operation failed.", IEnumerable<JsonError> errors = null, object data = null)
        {
            errors = errors ?? new JsonError[0];
            return new JsonResultData()
            {
                success = false,
                message = message,
                location = location,
                errors = errors.ToList(),
                data = data
            };
        }

        /// <summary>
        /// Creates a failure response.
        /// </summary>
        /// <param name="location">The location to access the object.</param>
        /// <param name="message">The fail message.</param>
        /// <param name="modelState">The state to grab the errors from.</param>
        /// <returns>The fail response.</returns>
        public static JsonResultData Failure(string location = null, string message = "The operation failed.", ModelStateDictionary modelState = null, object data = null)
        {
            var errors = new List<JsonError>();
            foreach (var state in modelState)
            {
                var jsonError = new JsonError() { Key = state.Key, Errors = state.Value.Errors.Select(e => String.IsNullOrWhiteSpace(e.ErrorMessage) ? (e.Exception != null ? e.Exception.Message : "") : e.ErrorMessage) };
                errors.Add(jsonError);
            }
            return new JsonResultData()
            {
                success = false,
                message = message,
                location = location,
                errors = errors,
                data = data
            };
        }

        /// <summary>
        /// Creates a success response.
        /// </summary>
        /// <param name="location">The location to access the object.</param>
        /// <param name="message">The success message.</param>
        /// <returns>The success response.</returns>
        public static JsonResultData Success(string location = null, string message = "The operation completed successfuly.", object data = null)
        {
            return new JsonResultData()
            {
                message = message,
                location = location,
                data = data
            };
        }

    }
}