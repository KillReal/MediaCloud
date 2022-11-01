using MediaCloud.Data.Models;
using MediaCloud.Uploader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.Uploader
{
    public static class Scheduler
    {
        private static Thread _workRoutine = new(new ThreadStart(WorkRoutine));

        public static bool IsRunning { get; set; }

        public static int WorkersActive
        {
            get => IsRunning
                ? 1
                : 0;
        }

        public static void Run()
        {
            IsRunning = true;

            _workRoutine.Start();
        }

        public static void Stop()
        {
            IsRunning = false;

            _workRoutine.Join();
        }

        public static void WorkRoutine()
        {
            while (IsRunning)
            {
                if (Queue.IsEmpty)
                {
                    IsRunning = false;
                    return;
                }

                var task = Queue.GetTask();
                var foundTags = Uploader.TagRepository.GetRangeByString(task.TagString);
                var medias = new List<Media>();

                if (task.IsCollection)
                {
                    medias = Uploader.MediaRepository.CreateCollection(task.Content);
                    medias.First(x => x.Preview.Order == 0).Preview.Tags = foundTags;
                }
                else
                {
                    medias = Uploader.MediaRepository.CreateRange(task.Content);

                    foreach (var media in medias)
                    {
                        media.Preview.Tags = foundTags;
                    }
                }

                Uploader.MediaRepository.Update(medias);
            }
        }
    }
}
