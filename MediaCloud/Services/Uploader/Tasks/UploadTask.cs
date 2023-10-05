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

        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int lx = x.Length, ly = y.Length;

            for (int mx = 0, my = 0; mx < lx && my < ly; mx++, my++)
            {
                if (char.IsDigit(x[mx]) && char.IsDigit(y[my]))
                {
                    long vx = 0, vy = 0;

                    for (; mx < lx && char.IsDigit(x[mx]); mx++)
                        vx = vx * 10 + x[mx] - '0';

                    for (; my < ly && char.IsDigit(y[my]); my++)
                        vy = vy * 10 + y[my] - '0';

                    if (vx != vy)
                        return vx > vy ? 1 : -1;
                }

                if (mx < lx && my < ly && x[mx] != y[my])
                    return x[mx] > y[my] ? 1 : -1;
            }

            return lx - ly;
        }
    }

    public class UploadTask : Task, ITask
    {
        public List<byte[]> Content { get; set; }

        public bool IsCollection { get; set; }

        public string TagString { get; set; }

        public override int GetWorkCount() => Content.Count;

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
