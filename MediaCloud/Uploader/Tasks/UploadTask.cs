using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.MediaUploader.Tasks
{
    public class UploadTask : Task, ITask
    {
        public List<byte[]> Content { get; set; }

        public bool IsCollection { get; set; }

        public string TagString { get; set; }

        public UploadTask(List<IFormFile> content, bool isCollection, string tagString)
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

        public override int GetWorkCount()
        {
            return Content.Count;
        }

        public override void DoTheTask()
        {
            var context = Scheduler.GetContext();

            var tagRepository = new TagRepository(context);
            var mediaRepository = new MediaRepository(context);

            var foundTags = tagRepository.GetRangeByString(TagString);
            var medias = new List<Media>();

            if (IsCollection)
            {
                medias = mediaRepository.CreateCollection(Content);
                medias.First(x => x.Preview.Order == 0).Preview.Tags = foundTags;
            }
            else
            {
                medias = mediaRepository.CreateRange(Content);

                foreach (var media in medias)
                {
                    media.Preview.Tags = foundTags;
                }
            }

            mediaRepository.SaveChanges();

            mediaRepository.Update(medias);
            mediaRepository.SaveChanges();
        }
    }
}
