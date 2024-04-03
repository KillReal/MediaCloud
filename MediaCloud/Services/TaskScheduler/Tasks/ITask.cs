using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService;

namespace MediaCloud.MediaUploader.Tasks
{
    public interface ITask
    {
        public int GetWorkCount();

        public void DoTheTask(IDataService DataService);
    }
}
