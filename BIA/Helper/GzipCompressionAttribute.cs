using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace BIA.Helper
{
    //public class GzipCompressionAttribute : ActionFilterAttribute
    //{
    //    //public override void OnActionExecuted(ActionExecutedContext context)
    //    //{
    //    //    var response = context.HttpContext.Response;

    //    //    if (response != null)
    //    //    {
    //    //        var acceptEncoding = context.HttpContext.Request.Headers["Accept-Encoding"];
    //    //        if (acceptEncoding.ToString().Contains("gzip"))
    //    //        {
    //    //            var content = response.Body;
    //    //            response.Headers.Add("Content-Encoding", "gzip");
    //    //            response.Headers.Add("Vary", "Accept-Encoding");

    //    //            using (var compressedStream = new System.IO.Compression.GZipStream(response.Body, System.IO.Compression.CompressionLevel.Optimal))
    //    //            {
    //    //                response.Body = compressedStream;
    //    //                base.OnActionExecuted(context);
    //    //            }
    //    //        }
    //    //    }
    //    //}



    //}
    public class GzipCompressionAttribute : ActionFilterAttribute
    {

        public async Task OnActionExecutedAsync(ActionExecutedContext context)
        {
            var response = context.HttpContext.Response;

            if (response != null && !response.HasStarted && response.StatusCode == 200)
            {
                var acceptEncoding = context.HttpContext.Request.Headers["Accept-Encoding"];
                if (acceptEncoding.ToString().Contains("gzip"))
                {
                    var originalBody = context.HttpContext.Response.Body;
                    var memoryStream = new MemoryStream();
                    context.HttpContext.Response.Body = memoryStream;

                    // Your custom action logic here (e.g., await base.OnActionExecutedAsync(context))

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var gzipStream = new GZipStream(originalBody, CompressionLevel.Optimal))
                    {
                        await memoryStream.CopyToAsync(gzipStream);
                    }
                }
            }
        }
    }
}
