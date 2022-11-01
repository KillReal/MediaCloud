using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class MediaRepository : Repository<Media>, IListBuildable<Media>
    {

        public MediaRepository(AppDbContext context) : base(context)
        {
        }

        public List<Media> GetList(ListBuilder<Media> listBuilder)
        {
            return _context.Medias.AsNoTracking()
                                  .Order(listBuilder.Order)
                                  .ToList();
        }

        public async Task<int> GetListCountAsync(ListBuilder<Media> listBuilder)
        {
            return await _context.Medias.AsNoTracking()
                                        .CountAsync();
        }

        public Media Create(byte[] file)
        {
            var media = new Media(file);
            media.Preview = new Preview(media);
            media.Creator = new ActorRepository(_context).GetCurrent();
            media.Updator = media.Creator;

            _context.Add(media);

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

            return medias;
        }

        public List<Media> CreateCollection(List<byte[]> files)
        {
            if (files.Count == 1)
            {
                return new() { Create(files[0]) };
            }

            var medias = CreateRange(files);
            _context.SaveChanges();

            var previews = new List<Preview>();
            foreach (var media in medias)
            {
                previews.Add(media.Preview);
            }

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

            _context.UpdateRange(previews);

            return medias;
        }
    }
}
