using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.Uploader.Tasks
{
    public class UploadTask
    {
        public List<IFormFile> Content { get; set; }

        public bool IsCollection { get; set; }

        public string TagString { get; set; }
    }
}
