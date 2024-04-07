using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.WebApp.Services.ActorProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.TaskScheduler.Tasks
{
    internal class FileNameComparer : IComparer<string>
    {

        public int Compare(string? x, string? y)
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
            var orderedContent = content.OrderByDescending(x => x.FileName, new FileNameComparer());
            Content = orderedContent.Where(x => x.FileName.Contains("mp4") == false)
                                                      .Select(x => x.GetBytes())
                                                      .ToList();
            IsCollection = isCollection;
            TagString = tagString ?? "";
        }

        public override void DoTheTask(IServiceProvider serviceProvider, IActorProvider actorProvider)
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var statisticProvider = new StatisticProvider(context, actorProvider);
            var pictureService = serviceProvider.GetRequiredService<IPictureService>();

            var tagRepository = new TagRepository(context, statisticProvider, actorProvider);
            var mediasRepository = new MediaRepository(context, statisticProvider, actorProvider, pictureService);

            var foundTags = tagRepository.GetRangeByString(TagString);
            var medias = new List<Media>();

            if (IsCollection)
            {
                medias = mediasRepository.CreateCollection(Content);
            }
            else
            {
                medias = mediasRepository.CreateRange(Content);
            }

            var preview = medias.Select(x => x.Preview).Where(x => x.Order == 0).First();
            tagRepository.UpdatePreviewLinks(foundTags, preview);
        }
    }
}
