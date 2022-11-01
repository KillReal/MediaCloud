using MediaCloud.Data.Models;
using MediaCloud.MediaUploader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCloud.MediaUploader
{
    public class Worker
    {
        private Thread _workRoutine;

        public Guid CurrentTask = Guid.Empty;

        public bool IsRunning { get; set; } = false;

        public void Run()
        {
            IsRunning = true;

            _workRoutine = new(WorkRoutine);
            _workRoutine.Start();
        }

        public void Stop()
        {
            IsRunning = false;

            _workRoutine.Join();
        }

        public void WorkRoutine()
        {
            while (IsRunning)
            {
                if (Queue.IsEmpty)
                {
                    IsRunning = false;
                    return;
                }

                var task = Queue.GetTask();
                CurrentTask = task.Id;
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

                Uploader.MediaRepository.SaveChanges();

                Uploader.MediaRepository.Update(medias);
                Uploader.MediaRepository.SaveChanges();

                Queue.RemoveTask(task);
                CurrentTask = Guid.Empty;
            }
        }
    }
}
