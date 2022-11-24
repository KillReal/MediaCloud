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
            Content = content.OrderBy(x => x.FileName).Where(x => x.FileName.Contains("mp4") == false)
                                                      .Select(x => x.GetBytes())
                                                      .ToList();
            IsCollection = isCollection;
            TagString = tagString ?? "";
        }

        public override int GetWorkCount()
        {
            return Content.Count;
        }

        public override void DoTheTask()
        {
            var repository = Scheduler.GetRepository();

            var foundTags = repository.Tags.GetRangeByString(TagString);
            var medias = new List<Media>();

            if (IsCollection)
            {
                medias = repository.Medias.CreateCollection(Content);
                medias.First(x => x.Preview.Order == 0).Preview.Tags = foundTags;
            }
            else
            {
                medias = repository.Medias.CreateRange(Content);
                medias.ForEach(x => x.Preview.Tags = foundTags);
            }

            repository.Medias.Update(medias);
        }
    }
}
