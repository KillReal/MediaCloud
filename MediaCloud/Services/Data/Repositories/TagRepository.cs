using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Data.Types;
using MediaCloud.WebApp.Repositories.Base;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using Microsoft.EntityFrameworkCore;
using static MediaCloud.WebApp.Services.ConfigurationService;
using Preview = MediaCloud.Data.Models.Preview;

namespace MediaCloud.Repositories
{
    public class TagRepository : BaseRepository<Tag>, IListBuildable<Tag>
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

        public TagRepository(RepositoryContext context) : base(context)
        {
        }

        public bool Create(Tag tag)
        {
            try
            {
                tag.Creator = _context.Actors.First(x => x.Id == _actor.Id);
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
            var tagsToUnlink = preview.Tags.Except(tags).ToList();
            var tagsToLink = tags.Except(preview.Tags).ToList();

            tagsToUnlink.ForEach(x =>
            {
                x.Previews.Remove(preview);
                x.PreviewsCount -= 1;
            });

            tagsToLink.ForEach(x =>
            {
                x.Previews.Add(preview);
                x.PreviewsCount += 1;
            });

            var affectedTags = tagsToLink.Union(tagsToUnlink);

            _context.Tags.UpdateRange(affectedTags);
            _context.SaveChanges();

            _logger.Info("Recalculated <{affectedTags.Count()}> tags usage count by: {_actor.Name}", affectedTags.Count(), _actor.Name);
        }

        public List<Tag> GetRangeByString(string? tagsString)
        {
            if (string.IsNullOrEmpty(tagsString))
            {
                return new();
            }
            tagsString = DeduplicateTagString(tagsString).ToLower();
            var tags = tagsString.ToLower().Split(' ');
            return _context.Tags.Where(x => tags.Any(y => y == x.Name.ToLower())
                                         && x.CreatorId == _actor.Id)
                                .ToList();
        }

        public List<Tag> GetList(ListBuilder<Tag> listBuilder)
        {
            return _context.Tags.AsNoTracking().Order(listBuilder.Order)
                                               .Where(x => x.CreatorId == _actor.Id)
                                               .Skip(listBuilder.Offset)
                                               .Take(listBuilder.Count)
                                               .ToList();
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
            return _context.Tags.OrderByDescending(x => x.PreviewsCount)
                                .Where(x => x.CreatorId == _actor.Id)
                                .Take(limit)
                                .ToList();
        }

        public List<string> GetSuggestionsByString(string searchString, int limit = 10)
        {
            return _context.Tags.Where(x => x.Name.ToLower().StartsWith(searchString.ToLower()) && x.CreatorId == _actor.Id)
                                .OrderByDescending(x => x.PreviewsCount)
                                .Select(x => x.Name)
                                .Take(limit)
                                .ToList();
        }
    }
}
