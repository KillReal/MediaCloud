using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Data.Types;
using MediaCloud.WebApp.Repositories.Base;
using MediaCloud.WebApp.Services.Repository.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class TagRepository : Repository<Tag>, IListBuildable<Tag>
    {
        public TagRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public bool Create(Tag tag)
        {
            try
            {
                tag.Creator = new ActorRepository(_context).Get(_actorId);
                tag.Updator = tag.Creator;

                _context.Tags.Add(tag);
                SaveChanges();

                _logger.LogInformation($"Created new tag with id:{tag.Id} by: {_actorId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error on creating new tag with id:{tag.Id} exception: {ex}");
                return false;
            }
        }

        public async Task RecalculateCountsAsync(List<Tag> tags)
        {
            tags.ForEach(x => x.PreviewsCount = (_context.Tags.Find(x.Id) ?? new()).Previews.Count);
            _context.Tags.UpdateRange(tags);

            _logger.LogInformation($"Recalculated <{tags.Count}> tags usage count by: {_actorId}");
        }

        public List<Tag> GetRangeByString(string? tagsString)
        {
            if (string.IsNullOrEmpty(tagsString))
            {
                return new();
            }

            return _context.Tags.Where(x => tagsString.ToLower().Contains(x.Name.ToLower())
                                         && x.Creator.Id == _actorId)
                                .ToList();
        }

        public TagFilter<Preview> GetTagFilter(string tagsString)
        {
            var tags = tagsString.ToLower().Split(' ');
            
            var positiveTags = new List<string>();
            var negativeTags = new List<string>();
            foreach (var tag in tags)
            {
                if (tag.Contains('!'))
                {
                    negativeTags.Add(tag.Remove(0, 1));
                    continue;
                }

                positiveTags.Add(tag);
            }

            var positiveTagIds = _context.Tags.Where(x => positiveTags.Contains(x.Name.ToLower()))
                                              .Select(x => x.Id);
            var negativeTagIds = _context.Tags.Where(x => negativeTags.Contains(x.Name.ToLower()))
                                              .Select(x => x.Id);

            return new(positiveTagIds.ToList(), negativeTagIds.ToList());
        }

        public List<Tag> GetList(ListBuilder<Tag> listBuilder)
        {
            return _context.Tags.AsNoTracking().Order(listBuilder.Order)
                                               .Where(x => x.Creator.Id == _actorId)
                                               .Skip(listBuilder.Offset)
                                               .Take(listBuilder.Count)
                                               .ToList();
        }

        public async Task<int> GetListCountAsync(ListBuilder<Tag> listBuilder) 
            => await _context.Tags.Where(x => x.Creator.Id == _actorId).AsNoTracking().CountAsync();

        /// <summary>
        /// Return list of tags ordered by PreviewsCount with specified count.
        /// </summary>
        /// <param name="limit"> List count. </param>
        /// <returns> List of tags. </returns>
        public List<Tag> GetTopUsed(int limit)
        {
            return _context.Tags.OrderByDescending(x => x.PreviewsCount)
                                .Where(x => x.Creator.Id == _actorId)
                                .Take(limit)
                                .ToList();
        }
    }
}
