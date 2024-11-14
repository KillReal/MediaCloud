using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.UserProvider;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.EntityFrameworkCore;
using NLog;
using Preview = MediaCloud.Data.Models.Preview;

namespace MediaCloud.Repositories
{
    public class TagRepository(AppDbContext context, StatisticProvider statisticProvider, IUserProvider actorProvider) : BaseRepository<Tag>(context, statisticProvider, LogManager.GetLogger("CollectionRepository"), actorProvider), IListBuildable<Tag>
    {
        private static string DeduplicateTagString(string tagString)
        {
            var tags = tagString.Split(' ');

            if (tags.Length < 2)
            {
                return tagString;
            }

            return string.Join(' ', tags.Distinct());
        }

        public bool Create(Tag tag)
        {
            try
            {
                tag.Creator = _context.Users.First(x => x.Id == _actor.Id);
                tag.Updator = tag.Creator;

                _context.Tags.Add(tag);
                SaveChanges();

                _logger.Info("Created new tag with id:{tag.Id} by: {_actor.Name}", tag.Id, _actor.Name);
                _statisticProvider.TagsCountChanged.Invoke(1);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Error on creating new tag with id: {tag.Id} exception: {ex}", tag.Id, ex);
                return false;
            }
        }

        public override void Remove(Tag entity)
        {
            base.Remove(entity);
            _statisticProvider.TagsCountChanged.Invoke(-1);
        }

        public override void Remove(List<Tag> entities)
        {
            var count = entities.Count;
            base.Remove(entities);
            _statisticProvider.TagsCountChanged.Invoke(count);
        }

        public void UpdatePreviewLinks(List<Tag> tags, Preview preview)
        {
            if (preview.Tags.Count == 0 && tags.Count == 0)
            {
                return;
            }

            var tagsToUnlink = preview.Tags.Except(tags).ToList();
            var tagsToLink = tags.Except(preview.Tags).ToList();

            foreach (var tag in tagsToUnlink)
            {
                preview.Tags.Remove(tag);
                tag.PreviewsCount -= 1;
            };

            foreach (var tag in tagsToLink)
            {
                preview.Tags.Add(tag);
                tag.PreviewsCount += 1;
            }

            var affectedTags = tagsToLink.Union(tagsToUnlink);

            _context.Previews.Update(preview);
            _context.Tags.UpdateRange(affectedTags);
            _context.SaveChanges();

            _logger.Info("Recalculated <{affectedTags.Count()}> tags usage count by: {_actor.Name}", affectedTags.Count(), _actor.Name);
        }

        public List<Tag> GetRangeByString(string? tagsString)
        {
            if (string.IsNullOrEmpty(tagsString))
            {
                return [];
            }
            tagsString = DeduplicateTagString(tagsString).ToLower();
            var tags = tagsString.ToLower().Split(' ');
            return [.. _context.Tags.Where(x => tags.Any(y => y.ToLower() == x.Name.ToLower())
                                         && x.CreatorId == _actor.Id)];
        }

        public List<Tag> GetRangeByAliasString(string? aliasesString)
        {
            if (string.IsNullOrEmpty(aliasesString))
            {
                return [];
            }
            aliasesString = DeduplicateTagString(aliasesString).ToLower();
            var aliases = aliasesString.ToLower().Split(' ');
            
            var selectedTags = _context.Tags.AsEnumerable().Where(x => aliases.Where(y => x.Alias
                    .Split(' ')
                    .Any(z => z == y))
                    .Any()
                    && x.CreatorId == _actor.Id)
                .ToList();

            selectedTags.RemoveAll(x => x.Alias.Split(' ')
                .Any(y => y.Contains('!') && aliases.Contains(y.Replace("!", ""))));
            selectedTags.RemoveAll(x => x.Alias.Split(' ')
                .Any(y => y.Contains('+') && aliases.Contains(y.Replace("+", "")) == false));

            return selectedTags;
        }

        public async Task<List<Tag>> GetListAsync(ListBuilder<Tag> listBuilder)
        {
            return await _context.Tags.AsNoTracking().Order(listBuilder.Sorting.GetOrder())
                                               .Where(x => x.CreatorId == _actor.Id)
                                               .Skip(listBuilder.Pagination.Offset)
                                               .Take(listBuilder.Pagination.Count)
                                               .ToListAsync();
        }

        public async Task<int> GetListCountAsync(ListBuilder<Tag> listBuilder)
            => await _context.Tags.Where(x => x.CreatorId == _actor.Id).AsNoTracking().CountAsync();

        /// <summary>
        /// Return list of tags ordered by PreviewsCount with specified count.
        /// </summary>
        /// <param name="limit"> List count. </param>
        /// <returns> List of tags. </returns>
        public List<Tag> GetTopUsed(int limit)
        {
            return [.. _context.Tags.OrderByDescending(x => x.PreviewsCount)
                                .Where(x => x.CreatorId == _actor.Id)
                                .Take(limit)];
        }

        public List<string> GetSuggestionsByString(string searchString, int limit = 10)
        {
            return [.. _context.Tags.Where(x => x.Name.ToLower().StartsWith(searchString.ToLower()) && x.CreatorId == _actor.Id)
                                .OrderByDescending(x => x.PreviewsCount)
                                .Select(x => x.Name)
                                .Take(limit)];
        }
    }
}
