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

        public UploadTask(List<IFormFile> content, Guid actorId, bool isCollection, string? tagString) 
            : base(actorId)
        {
            Id = Guid.NewGuid();
            Content = content.OrderBy(x => x.FileName).Select(x => x.GetBytes()).ToList();
            IsCollection = isCollection;
            TagString = tagString ?? "";
        }

        public override int GetWorkCount()
        {
            return Content.Count;
        }

        public override void DoTheTask()
        {
            var context = Scheduler.GetContext();
            var logger = Scheduler.GetLogger();

            var tagRepository = new TagRepository(context, logger, ActorId);
            var mediaRepository = new MediaRepository(context, logger, ActorId);

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
                medias.ForEach(x => x.Preview.Tags = foundTags);
            }

            mediaRepository.Update(medias);
        }
    }
}
