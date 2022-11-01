using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Services
{
    public class PreviewRepository : Repository<Preview>, IRepository<Preview>
    {
        public PreviewRepository(AppDbContext context) : base(context)
        {
        }

        public Preview? Get(Guid id)
        {
            return _context.Previews.Where(x => x.Id == id)
                                        .FirstOrDefault();
        }

        public int GetMediaCount(Guid id)
        {
            return _context.Medias.Where(x => x.Preview.Id == id).Count();
        }


        public int GetListCount(ListBuilder<Preview> listBuilder)
        {
            var tags = new TagRepository(_context).GetRangeByString(listBuilder.Filtering.Filter);

            if (!tags.Any() && !string.IsNullOrEmpty(listBuilder.Filtering.Filter))
            {
                return new() { };
            }

            List<Guid> tagIds = tags.Select(x => x.Id).ToList();

            int count = 0;
            if (tags.Any())
            {
                count = _context.Previews.AsQueryable()
                                            .Where(x => x.Tags.Where(y => tagIds.Contains(y.Id))
                                                .Count() == tagIds.Count)
                                            .Count();
            }
            else
            {
                count = _context.Previews.AsQueryable()
                                            .Count();
            }

            return count;
        }

        private Guid GenerateSeededGuid(int seed)
        {
            var r = new Random(seed);
            var guid = new byte[16];
            r.NextBytes(guid);

            return new Guid(guid);
        }

        public List<Preview> GetList(ListBuilder<Preview> listBuilder)
        {
            var tags = new TagRepository(_context).GetRangeByString(listBuilder.Filtering.Filter);

            if (!tags.Any() && !string.IsNullOrEmpty(listBuilder.Filtering.Filter))
            {
                return new() { };
            }

            List<Guid> tagIds = tags.Select(x => x.Id).ToList();

            var previews = new List<Preview>();
            if (tags.Any())
            {
                if (listBuilder.Sorting.PropertyName.Contains("Random"))
                {
                    var seed = 0;
                    int.TryParse(listBuilder.Sorting.PropertyName.Split('-').Last(), out seed);

                    var previewIds = _context.Previews.Select(x => x.Id)
                                                        .ToList();
                    previewIds = previewIds.Shuffle(seed).ToList();

                    previews = _context.Previews.AsQueryable()
                                            .Include(x => x.Tags)
                                            .Where(x => (x.Tags.Where(y => tagIds.Contains(y.Id))
                                                .Count() == tagIds.Count) && previewIds.Any(id => id == x.Id))
                                            .ToList()
                                            .OrderBy(l => previewIds.IndexOf(l.Id))
                                            .ToList();
                }
                else
                {
                    previews = _context.Previews.AsQueryable()
                                            .Include(x => x.Tags)
                                            .Where(x => x.Tags.Where(y => tagIds.Contains(y.Id))
                                                .Count() == tagIds.Count)
                                            .Order(listBuilder.Sorting.GetOrder())
                                            .Skip(listBuilder.Pagination.Offset)
                                            .Take(listBuilder.Pagination.Count)
                                            .ToList();
                }
            }
            else
            {
                if (listBuilder.Sorting.PropertyName.Contains("Random"))
                {
                    var seed = 0;
                    int.TryParse(listBuilder.Sorting.PropertyName.Split('-').Last(), out seed);

                    var previewIds = _context.Previews.Select(x => x.Id)
                                                        .ToList();
                    previewIds = previewIds.Shuffle(seed)
                                            .Skip(listBuilder.Pagination.Offset)
                                            .Take(listBuilder.Pagination.Count)
                                            .ToList();

                    previews = _context.Previews.Where(l => previewIds.Any(id => id == l.Id))
                                              .ToList()
                                              .OrderBy(l => previewIds.IndexOf(l.Id))
                                              .ToList();
                }
                else
                {
                    previews = _context.Previews.AsQueryable()
                                            .Order(listBuilder.Sorting.GetOrder())
                                            .Skip(listBuilder.Pagination.Offset)
                                            .Take(listBuilder.Pagination.Count)
                                            .ToList();
                }
            }

            if (!previews.Any())
            {
                return new();
            }

            return previews.ToList();
        }
    }
}
