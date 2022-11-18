using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Data.Types;
using MediaCloud.WebApp.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class TagRepository : Repository<Tag>, IListBuildable<Tag>
    {
        public TagRepository(AppDbContext context, ILogger logger, Guid actorId) 
            : base(context, logger, actorId)
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

        public async Task RecalculateCountsAsync()
        {
            var query = @"UPDATE 'Tags' AS t
                          SET 'PreviewsCount' = (
                                SELECT Count(*) 
                                FROM 'PreviewTag' AS pt 
                                WHERE pt.TagsId = t.Id
                          )";

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(query);

            _logger.LogInformation($"Recalculated <{rowsAffected}> tags usage count by: {_actorId}");
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
                }
                else
                {
                    positiveTags.Add(tag);
                }
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
    }
}
