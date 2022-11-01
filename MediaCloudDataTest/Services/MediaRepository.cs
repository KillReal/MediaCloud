using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Services
{
    public class MediaRepository : Repository<Media>, IRepository<Media>
    {

        public MediaRepository(AppDbContext context) : base(context)
        {
        }

        public Media? Get(Guid id)
        {
            return _context.Medias.Where(x => x.Id == id)
                                        .FirstOrDefault();
        }

        public Media GetOffsetByPreview(Guid previewId, int offset)
        {
            return _context.Medias.Where(x => x.Preview.Id == previewId).OrderBy(x => x.Rank).Skip(offset).First();
        }

        public List<Media> GetList(ListBuilder<Media> listBuilder)
        {
            var medias = _context.Medias.AsQueryable().Order(listBuilder.Sorting.GetOrder());
            if (medias == null)
            {
                return new();
            }

            return medias.ToList();
        }

        public int GetListCount(ListBuilder<Media> listBuilder)
        {
            return _context.Medias.Count();
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

            //TODO: try
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

            //TODO: try
            var medias = new List<Media>
            {
                new Media(files[0])
            };

            var preview = new Preview(medias.First());
            medias.First().Preview = preview;

            for (var i = 1; i < files.Count; i++)
            {
                var media = new Media(files[i])
                {
                    Preview = preview,
                    Creator = new ActorRepository(_context).GetCurrent()
                };

                media.Updator = media.Creator;
                medias.Add(media);
            }

            _context.AddRange(medias);
            _context.SaveChanges();

            return medias;
        }
    }
}
