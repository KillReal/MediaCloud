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

        public Media Create(IFormFile file)
        {
            var media = new Media(file);
            media.Preview = new Preview(media);
            media.Creator = new ActorRepository(_context).GetCurrent();
            media.Updator = media.Creator;

            _context.Add(media);
            _context.SaveChanges();

            return media;
        }

        public List<Media> CreateRange(List<IFormFile> files)
        {
            if (files.Count == 1)
            {
                return new() { Create(files[0]) };
            }

            var medias = new List<Media>();
            foreach (var file in files)
            {
                var media = new Media(file);
                media.Preview = new Preview(media);
                media.Creator = new ActorRepository(_context).GetCurrent();
                media.Updator = media.Creator;

                medias.Add(media);
            }

            _context.AddRange(medias);
            _context.SaveChanges();

            return medias;
        }

        public List<Media> CreateCollection(List<IFormFile> files)
        {
            if (files.Count == 1)
            {
                return new() { Create(files[0]) };
            }

            var medias = CreateRange(files);

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
            _context.SaveChanges();

            return medias;
        }
    }
}
