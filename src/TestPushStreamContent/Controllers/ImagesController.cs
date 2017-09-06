using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using TestPushStreamContent.Models;

namespace TestPushStreamContent.Controllers
{
    public class ImagesController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var imageStream = new ImageStream(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "Record_110_pages.tif"));
            Func<Stream, HttpContent, TransportContext, Task> func = imageStream.StringWriteToStream;
            var response = Request.CreateResponse();
            response.Content = new PushStreamContent(func, new MediaTypeHeaderValue("text/event-stream"));

            //NOTE: this will cause the browser to replace the page with the image, this is not what we were going for since we want all images to show up
            //response.Content.Headers.Remove("Content-Type");
            //response.Content.Headers.TryAddWithoutValidation("Content-Type", "multipart/x-mixed-replace;boundary=" + imageStream.Boundary);
            //
            //response.Content.Headers.TryAddWithoutValidation("Content-Type", "text/html");
            return response;
        }
    }
}
