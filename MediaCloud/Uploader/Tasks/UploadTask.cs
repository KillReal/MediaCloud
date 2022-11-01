using MediaCloud.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.MediaUploader.Tasks
{
    public class UploadTask
    {
        public Guid Id { get; set; }
        public List<byte[]> Content { get; set; }

        public bool IsCollection { get; set; }

        public string TagString { get; set; }

        public UploadTask(List<IFormFile> content, bool isCollection = false, string tagString = "")
        {
            Id = Guid.NewGuid();

            Content = new();
            content = content.OrderBy(x => x.FileName).ToList();
            foreach(var file in content)
            {
                Content.Add(file.GetBytes());
            }

            IsCollection = isCollection;
            TagString = tagString;
        }
    }
}
