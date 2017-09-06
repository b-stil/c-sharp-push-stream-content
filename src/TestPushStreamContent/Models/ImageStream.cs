using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TestPushStreamContent.Models
{
    public class ImageStream
    {
        private readonly string _imageFile;

        public object Boundary { get; private set; } = "ImageFile";

        public ImageStream(string imageFile)
        {
            _imageFile = imageFile;
        }

        /// <summary>
        /// Sends the byte array down.
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="content"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            try
            {
                //read the image file
                using (var image = Image.FromFile(_imageFile))
                {

                    int pageCount = image.GetFrameCount(FrameDimension.Page);
                    for (int i = 0; i < pageCount; i++)
                    {
                        image.SelectActiveFrame(FrameDimension.Page, i);
                        MemoryStream pngStream = new MemoryStream();
                        pngStream.Position = 0;
                        image.Save(pngStream, ImageFormat.Png);
                        byte[] png = pngStream.ToArray();
                        pngStream.Dispose();
                        //write header
                        var header = $"--{Boundary}\r\nContent-Type: image/png\r\nContent-Length: {png.Length}\r\n\r\n";
                        var headerData = Encoding.UTF8.GetBytes(header);
                        await outputStream.WriteAsync(headerData, 0, headerData.Length);
                        await outputStream.WriteAsync(png, 0, png.Length);
                        await outputStream.FlushAsync();
                        System.Diagnostics.Debug.Write(i);
                        await Task.Delay(1000 / 30); //have to delay in order for the stream to push each piece down
                    }
                }
            }
            catch (HttpException ex)
            {
                if (ex.ErrorCode == -2147023667) // The remote host closed the connection.
                {
                    return;
                }
            }
            finally
            {
                outputStream.Close();
            }
        }

        /// <summary>
        /// Sends a base64 string down.
        /// </summary>
        /// <param name="outputStream"></param>
        /// <param name="content"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task StringWriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            try
            {
                //read the image file
                using (var image = Image.FromFile(_imageFile))
                {
                    byte[] newLine = Encoding.UTF8.GetBytes("\r\n");
                    int pageCount = image.GetFrameCount(FrameDimension.Page);
                    for (int i = 0; i < pageCount; i++)
                    {
                        image.SelectActiveFrame(FrameDimension.Page, i);
                        MemoryStream pngStream = new MemoryStream();
                        pngStream.Position = 0;
                        image.Save(pngStream, ImageFormat.Png);
                        string png = Convert.ToBase64String(pngStream.ToArray());
                        JObject json = new JObject(new JProperty("image", png), new JProperty("page", i + 1));
                        pngStream.Dispose();
                        byte[] pngStringBytes = Encoding.UTF8.GetBytes(json.ToString());
                        //var header = $"--{Boundary}\r\nContent-Type: image/png\r\nContent-Length: {pngStringBytes.Length}\r\n\r\n";
                        //var headerData = Encoding.UTF8.GetBytes(header);
                        //await outputStream.WriteAsync(headerData, 0, headerData.Length);
                        //await outputStream.FlushAsync();
                        //await Task.Delay(1000 / 30); //have to delay in order for the stream to push each piece down
                        await outputStream.WriteAsync(pngStringBytes, 0, pngStringBytes.Length);
                        //await outputStream.WriteAsync(newLine, 0, newLine.Length);
                        await outputStream.FlushAsync();
                        await Task.Delay(1000 / 30); //have to delay in order for the stream to push each piece down
                    }

                }
            }
            catch (HttpException ex)
            {
                if (ex.ErrorCode == -2147023667) // The remote host closed the connection.
                {
                    return;
                }
            }
            finally
            {
                outputStream.Close();
            }
        }

        ///// <summary>
        ///// Sends a base64 string down.
        ///// </summary>
        ///// <param name="outputStream"></param>
        ///// <param name="content"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public async Task StringWriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        //{
        //    try
        //    {
        //        //read the image file
        //        using (var image = Image.FromFile(_imageFile))
        //        {
        //            byte[] newLine = Encoding.UTF8.GetBytes("\r\n");
        //            int pageCount = image.GetFrameCount(FrameDimension.Page);
        //            for (int i = 0; i < pageCount; i++)
        //            {
        //                image.SelectActiveFrame(FrameDimension.Page, i);
        //                MemoryStream pngStream = new MemoryStream();
        //                pngStream.Position = 0;
        //                image.Save(pngStream, ImageFormat.Png);
        //                string png = Convert.ToBase64String(pngStream.ToArray());
        //                JObject json = new JObject(new JProperty("data", png), new JProperty("record", i + 1));
        //                pngStream.Dispose();
        //                //byte[] pngStringBytes = Encoding.UTF8.GetBytes(String.Format("--start--{0}--end--", json.ToString()));
        //                byte[] pngStringBytes = Encoding.UTF8.GetBytes(json.ToString());

        //                //var header = $"--{Boundary}\r\nContent-Type: image/png\r\nContent-Length: {pngStringBytes.Length}\r\n\r\n";
        //                // var headerData = Encoding.UTF8.GetBytes(header);
        //                //await outputStream.WriteAsync(headerData, 0, headerData.Length);
        //                //await outputStream.WriteAsync(newLine, 0, newLine.Length);
        //                //await Task.Delay(1000 / 30); //have to delay in order for the stream to push each piece down
        //                await outputStream.FlushAsync();
        //                await outputStream.WriteAsync(pngStringBytes, 0, pngStringBytes.Length);
        //                await outputStream.FlushAsync();
        //                await outputStream.WriteAsync(newLine, 0, newLine.Length);
        //                await outputStream.FlushAsync();
        //                //await outputStream.FlushAsync();
        //                await Task.Delay(1000 / 30); //have to delay in order for the stream to push each piece down
        //            }
        //        }
        //    }
        //    catch (HttpException ex)
        //    {
        //        if (ex.ErrorCode == -2147023667) // The remote host closed the connection.
        //        {
        //            return;
        //        }
        //    }
        //    finally
        //    {
        //        outputStream.Close();
        //    }
        //}
    }
}