using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediaCloud.Data;
using MediaCloud.Builders.List;
using MediaCloud.Data.Models;
using MediaCloud.Services;
using System.Drawing;
using System.Drawing.Imaging;

namespace MediaCloud.Pages.Medias
{
    public class CollectionModel : PageModel
    {
        private PreviewRepository PreviewRepository; 
        private MediaRepository MediaRepository;

        public CollectionModel(AppDbContext context)
        {
            PreviewRepository = new(context);
            MediaRepository = new(context);
        }

        private byte[] LowerResolution(byte[] imageBytes, int maxSize = -1)
        {
            if (maxSize == -1)
            {
                //maxSize = Convert.ToInt32(_configuration["PreviewMaxSize"]);
                maxSize = 700;
            }

            Stream stream = new MemoryStream(imageBytes);
            Image image = new Bitmap(stream);
            Size size = image.Size;
            int width = maxSize;
            int height = maxSize;
            float[] div = new float[] { size.Width / (float)width, size.Height / (float)height };
            float maxDiv = Math.Max(div[0], div[1]);

            if (maxDiv > 1.0)
            {
                Bitmap bitmap = new Bitmap(image, new Size(Convert.ToInt32(size.Width / maxDiv), Convert.ToInt32(size.Height / maxDiv)));
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Jpeg);
                imageBytes = ms.ToArray();
            }

            return imageBytes;
        }

        public IActionResult OnGet(Guid id, string returnUrl = "/Medias/Index")
        {
            var preview = PreviewRepository.Get(id);
            
            Medias = new List<Media>();
            for (int i = 0; i < preview.Medias.Count; i++)
            {
                preview.Medias[i].Content = LowerResolution(preview.Medias[i].Content);
                Medias.Add(preview.Medias[i]);
            }

            Medias = Medias.OrderBy(x => x.Rank).ToList();
            PreviewId = id;
            ReturnUrl = returnUrl.Replace("$", "&");

            return Page();
        }

        [BindProperty]
        public Guid PreviewId { get; set; }
        [BindProperty]
        public List<Media> Medias { get; set; }
        [BindProperty]
        public string ReturnUrl { get; set; }
    }
}
