using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RSToolKit.WebUI.Infrastructure
{
    public class MissingContentTypeDH : DelegatingHandler
    {
        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            /** Check that this is an IE browser. */
            if (request.RequestUri.AbsoluteUri.Contains("/api/") && (request.Content.Headers.ContentType == null || request.Content.Headers.ContentType.MediaType != "application/json" || request.Content.Headers.ContentType.MediaType != "application/xml"))
            {
                MediaTypeHeaderValue contentTypeValue;
                if (MediaTypeHeaderValue.TryParse("application/json", out contentTypeValue))
                {
                    request.Content.Headers.ContentType = contentTypeValue;
                }
                var body = request.Content.ReadAsStringAsync();
            }

            /** Return request to flow. */
            return base.SendAsync(request, cancellationToken)
               .ContinueWith(task =>
               {
                   // work on the response
                   var response = task.Result;
                   return response;
               });
        }
    }
}