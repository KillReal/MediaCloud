using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class MediaRepository : Repository<Media>
    {
        public MediaRepository(AppDbContext context, ILogger logger)
            : base(context, logger)
        {
        }

        public Media Create(byte[] file)
        {
            var media = new Media(file);
            media.Preview = new Preview(media);
            media.Creator = new ActorRepository(_context).GetCurrent();
            media.Updator = media.Creator;

            _context.Add(media);
            SaveChanges();

            _logger.LogInformation($"Created new media with id: {media.Id} by: {media.Creator.Id}");
            return media;
        }

        public List<Media> CreateRange(List<byte[]> files)
        {
            if (files.Count == 1)
            {
                return new() { Create(files[0]) };
            }

            var medias = new List<Media>();
            
            while (files.Count > 0)
            {
                var file = files.Last();
                var media = new Media(file);
                media.Preview = new Preview(media);
                media.Creator = new ActorRepository(_context).GetCurrent();
                media.Updator = media.Creator;

                files.Remove(file);
                medias.Add(media);
            }

            _context.AddRange(medias);
            SaveChanges();

            _logger.LogInformation($"Created <{medias.Count}> new medias by: {medias.First().Creator.Id}");
            return medias;
        }

        public List<Media> CreateCollection(List<byte[]> files)
        {
            if (files.Count == 1)
            {
                return new() { Create(files[0]) };
            }

            var medias = CreateRange(files);
            SaveChanges();

            var previews = medias.Select(x => x.Preview).ToList();

            var collection = new Collection(previews);
            for (var i = 0; i < previews.Count; i++)
            {
                if (i != 0)
                {
                    previews[i].Order = 1;
                }

                collection.Count = previews.Count;
                previews[i].Collection = collection;
            }

            new PreviewRepository(_context, _logger).Update(previews);
            SaveChanges();

            _logger.LogInformation($"Created new collection with id: {collection.Id} by: {collection.Creator.Id}");
            return medias;
        }
    }
}
