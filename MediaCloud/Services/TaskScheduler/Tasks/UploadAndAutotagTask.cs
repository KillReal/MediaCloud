using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.WebApp;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Statistic;
using System.Text;
using Blob = MediaCloud.Data.Models.Blob;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Services.TaskScheduler.Tasks;
using MediaCloud.WebApp.Controllers;

namespace MediaCloud.TaskScheduler.Tasks
{
   public class UploadAndAutotagTask(User user, List<UploadedFile> uploadedFiles, bool isCollection, string? tagString) 
      : UploadTask(user, uploadedFiles, isCollection, tagString)
   {
      public override void DoTheTask(IServiceProvider serviceProvider, IUserProvider userProvider)
      {
         base.DoTheTask(serviceProvider, userProvider);

         var previewsIds = _processedPreviews.Where(x => x.BlobType
            .Contains("image"))
            .Select(x => x.Id)
            .ToList();

         var taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();
         taskScheduler.AddTask(new AutotagPreviewTask(User, previewsIds));

         CompletionMessage += " and autotagging task(-s) added to queue";
      }
   }
}
