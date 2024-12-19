using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.Services;
using MediaCloud.WebApp;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Statistic;
using NLog;
using SixLabors.ImageSharp;
using System.Data;
using Blob = MediaCloud.Data.Models.Blob;
using MediaCloud.WebApp.Builders.BlobModel;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp.Extensions;

namespace MediaCloud.Repositories
{
    public class BlobRepository(AppDbContext context, StatisticProvider statisticProvider, 
        IUserProvider actorProvider, IPictureService pictureService, IConfigProvider configProvider) : 
        BaseRepository<Blob>(context, statisticProvider, LogManager.GetLogger("CollectionRepository"), actorProvider)
    {
        private BlobModelBuilder _fileModelBuilder = new(pictureService);
        private IConfigProvider _configProvider = configProvider;

        private Blob CreateFile(UploadedFile uploadedFile, User author)
        {
            var fileModel = _fileModelBuilder.Build(uploadedFile);
    
            Blob blob = fileModel.File;
            blob.Preview = fileModel.Preview;
            blob.Creator = author;
            blob.Updator = blob.Creator;
            blob.Preview.Creator = blob.Creator;
            blob.Preview.Updator = blob.Creator;

            return blob;
        }

        public override void Remove(Blob entity)
        {
            var size = entity.Size;
            base.Remove(entity);
            _statisticProvider.MediasCountChanged.Invoke(-1, -size);
        }

        public override void Remove(List<Blob> entities)
        {
            var count = entities.Count;
            var size = entities.Sum(x => x.Size);
            base.Remove(entities);
            _statisticProvider.MediasCountChanged.Invoke(-count, -size);
        }

        public Blob Create(UploadedFile file)
        {
            var author = _context.Users.First(x => x.Id == _actor.Id);
            var blob = CreateFile(file, author);
            _context.Add(blob);
            SaveChanges();

            _logger.Info("Created new media with id: {media.Id} by: {_actor.Name}", blob.Id, _actor.Name);
            _statisticProvider.MediasCountChanged.Invoke(1, blob.Size);
            return blob;
        }


        public List<Blob> CreateRange(List<UploadedFile> files)
        {
            _statisticProvider.ActivityFactorRaised.Invoke();

            if (files.Count == 1)
            {
                return [Create(files[0])];
            }

            var blobs = GetBlobsRange(files);

            _context.AddRange(blobs);
            SaveChanges();

            _logger.Info("Created <{blobs.Count}> new blobs by: {_actor.Name}", blobs.Count, _actor.Name);
            _statisticProvider.MediasCountChanged.Invoke(blobs.Count, blobs.Sum(x => x.Size));
            return blobs;
        }

        private List<Blob> GetBlobsRange(List<UploadedFile> files)
        {
            var blobs = new List<Blob>();
            var author = _context.Users.First(x => x.Id == _actor.Id);

            if (_configProvider.EnvironmentSettings.UseParallelProcessingForUploading)
            {
                var options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = _configProvider.EnvironmentSettings.UploadingMaxParallelDegree
                };

                Parallel.ForEach(files, options, (file) => {
                    blobs.Add(CreateFile(file, author));
                    file.IsProcessed = true;
                });

                blobs = [.. blobs.OrderBy(x => x.Preview.BlobName, new FileComparer())];
            }
            else 
            {
                while (files.Count > 0)
                {
                    var file = files.Last();
                    blobs.Add(CreateFile(file, author));
                    file.IsProcessed = true;
                }
            }

            return blobs;
        }

        public List<Blob> CreateCollection(List<UploadedFile> files)
        {
            _statisticProvider.ActivityFactorRaised.Invoke();

            if (files.Count == 1)
            {
                return [Create(files[0])];
            }

            var blobs = GetBlobsRange(files);
            var previews = blobs.Select(x => x.Preview).ToList();
            var collection = new Collection(previews)
            {
                Creator = _context.Users.First(x => x.Id == _actor.Id)
            };
            collection.Updator = collection.Creator;

            for (var i = 0; i < previews.Count; i++)
            {
                if (i != 0)
                {
                    previews[i].Order = i;
                }

                collection.Count = previews.Count;
                previews[i].Collection = collection;
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = false;        
            _context.Blobs.AddRange(blobs);
            SaveChanges();
            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            _logger.Info("Created new collection with <{collection.Count}> previews and id: {collection.Id} by: {_actor.Name}",
                collection.Count, collection.Id, _actor.Name);
            _statisticProvider.MediasCountChanged.Invoke(blobs.Count, blobs.Sum(x => x.Size));

            return blobs;
        }
    }
}
