using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.MediaUploader.Tasks
{
    internal class FileNameComparer : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

        public int Compare(string a, string b)
        {
            return StrCmpLogicalW(a, b);
        }
    }

    public class UploadTask : Task, ITask
    {
        public List<byte[]> Content { get; set; }

        public bool IsCollection { get; set; }

        public string TagString { get; set; }

        public UploadTask(Actor actor, List<IFormFile> content, bool isCollection, string? tagString) 
            : base(actor)
        {
            Id = Guid.NewGuid();
            var orderedContent = content.OrderByDescending(x => x.FileName, new FileNameComparer());
            Content = orderedContent.Where(x => x.FileName.Contains("mp4") == false)
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
            repository.SetCurrentActor(Actor);

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
