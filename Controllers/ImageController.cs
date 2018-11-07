using ImageMagick;
using MacAuth.ConfigModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace MacAuth.Controllers
{
    [Produces("application/json")]
    public class ImageController : Controller
    {
        private readonly IOptions<Clients> _clients;

        public ImageController(IOptions<Clients> clients)
        {
            _clients = clients;
        }

        public ActionResult Index(string ma_client_id, string source_url, int dest_width, int dest_height)
        {
            if(!ValidateRequest(ma_client_id, dest_width, dest_height, out string error))
            {
                return BadRequest(error);
            }

            using (var client = new HttpClient())
            {
                using (var response = client.GetAsync(source_url).Result)
                {
                    response.EnsureSuccessStatusCode();

                    using (var imageStream = new MemoryStream())
                    {
                        using (var inputStream = response.Content.ReadAsStreamAsync().Result)
                        {
                            using (var image = new MagickImage(inputStream))
                            {
                                image.Resize(dest_width, dest_height);
                                image.Depth = 1;
                                image.ColorType = ColorType.Bilevel;
                                image.Format = MagickFormat.Bmp3;

                                image.Write(imageStream);
                                imageStream.Seek(0, SeekOrigin.Begin);

                                var outputStream = new MemoryStream();
                                var image2PICT1 = new Image2PICT1(imageStream);

                                image2PICT1.Write(outputStream);
                                outputStream.Seek(0, SeekOrigin.Begin);

                                var sourceUri = new Uri(source_url);
                                var filename = Path.GetFileNameWithoutExtension(sourceUri.AbsolutePath);

                                if(filename.Length > 26)
                                {
                                    filename = filename.Substring(0, 26);
                                }

                                filename += ".pict";

                                return new FileStreamResult(outputStream, "image/x-pict")
                                {
                                    FileDownloadName = filename
                                };
                            }
                        }
                    }
                }
            }
        }

        private bool ValidateRequest(string ma_client_id, int dest_width, int dest_height, out string error)
        {
            error = null;

            // Validate client id
            if (_clients.Value == null ||
                ma_client_id == null ||
                _clients.Value.FirstOrDefault(c => c.Id == ma_client_id) == null)
            {
                error = "Invalid Client ID.";
                return false;
            }

            if (dest_width <= 0)
            {
                error = "Invalid dest_width.";
                return false;
            }

            if (dest_height <= 0)
            {
                error = "Invalid dest_height.";
                return false;
            }

            return true;
        }
    }
}
