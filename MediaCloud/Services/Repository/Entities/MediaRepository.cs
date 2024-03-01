using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Extensions;
using MediaCloud.WebApp.Services;
using MediaCloud.WebApp.Services.Repository.Entities.Base;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.IO;

namespace MediaCloud.Repositories
{
    public class MediaRepository : Repository<Media>
    {
        private Media GetMediaFromFile(byte[] file)
        {
            var stream = new MemoryStream(file);
            var convertedImage = Image.Load(stream);
            var media = new Media(file, convertedImage.Width, convertedImage.Height);
            media.Preview = new Preview(media, convertedImage);
            media.Creator = new ActorRepository(_context).Get(_actorId);
            media.Updator = media.Creator;
            media.Preview.Creator = media.Creator;
            media.Preview.Updator = media.Creator;

            return media;
        }

        public MediaRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public override void Remove(Media entity)
        {
            var size = entity.Size;
            base.Remove(entity);
            _statisticService.MediasCountChanged.Invoke(-1, -size);
        }

        public override void Remove(List<Media> entities)
        {
            var count = entities.Count;
            var size = entities.Sum(x => x.Size);
            base.Remove(entities);
            _statisticService.MediasCountChanged.Invoke(-count, -size);
        }

        public Media Create(byte[] file, List<Tag> tags)
        {
            var media = GetMediaFromFile(file);
            media.Preview.Tags = tags;
            _context.Add(media);
            SaveChanges();

            _logger.LogInformation($"Created new media with id: {media.Id} by: {_actorId}");
            _statisticService.MediasCountChanged.Invoke(1, media.Size);
            return media;
        }


        public List<Media> CreateRange(List<byte[]> files, List<Tag> tags)
        {
            if (files.Count == 1)
            {
                return new() { Create(files[0], tags) };
            }

            var medias = GetMediasRange(files);

            if (tags.Any())
            {
                medias.ForEach(x => x.Preview.Tags = tags);
            }

            _context.AddRange(medias);
            SaveChanges();

            _logger.LogInformation($"Created <{medias.Count}> new medias by: {_actorId}");
            _statisticService.MediasCountChanged.Invoke(medias.Count, medias.Sum(x => x.Size));
            return medias;
        }

        private List<Media> GetMediasRange(List<byte[]> files)
        {
            var medias = new List<Media>();

            while (files.Count > 0)
            {
                var file = files.Last();
                medias.Add(GetMediaFromFile(file));
                files.Remove(file);
            }

            return medias;
        }

        public List<Media> CreateCollection(List<byte[]> files, List<Tag> tags)
        {
            if (files.Count == 1)
            {
                return new() { Create(files[0], tags) };
            }

            var medias = GetMediasRange(files);

            if (tags.Any())
            {
                medias.First().Preview.Tags = tags;
            }

            var previews = medias.Select(x => x.Preview).ToList();

            var collection = new Collection(previews);
            collection.Creator = new ActorRepository(_context).Get(_actorId);
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
            _context.Medias.AddRange(medias);
            SaveChanges();
            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            _logger.LogInformation($"Created new collection with <{collection.Count}> previews and id: {collection.Id} by: {_actorId}");

            return medias;
        }
    }
}
