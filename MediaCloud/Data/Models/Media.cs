﻿using MediaCloud.Extensions;
using MediaCloud.Services;
using System.ComponentModel.DataAnnotations.Schema;
using SixLabors.ImageSharp;

namespace MediaCloud.Data.Models
{
    public class Media : Entity
    {
        public virtual Preview Preview { get; set; }

        public byte[] Content { get; set; }

        public string Resolution { get; set; }

        public int Rate { get; set; }

        public long Size { get; set; }

        [NotMapped]
        public string SizeInfo
        {
            get => PictureService.FormatSize(Size);
            set => Size = long.Parse(value);
        }

        public Media(byte[] file, int width, int height)
        {
            Content = file;

            //var stream = new MemoryStream(Content);
            //var picture = Image.Load(stream);

            //Resolution = $"{picture.Width}x{picture.Height}";
            Resolution = $"{width}x{height}";
            Size = Content.Length;

            Rate = 0;
        }

        public Media()
        {

        }
    }
}
