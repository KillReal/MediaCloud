﻿using DynamicExpression.Extensions;
using MediaCloud.Builders.List;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Services.Data.Repositories.Interfaces;
using MediaCloud.WebApp.Services.DataService.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Repositories
{
    public class PreviewRepository : BaseRepository<Preview>, IListBuildable<Preview>
    {
        private readonly TagRepository _tagDataService;

        private IQueryable<Preview> SetFilterToQuery(IQueryable<Preview> query, string filter)
        {
            if (string.IsNullOrEmpty(filter) == false)
            {
                // TODO: rework tag type to complete db model with TagTypeDataService.
                // Rework TagType filtering

                if (filter.Contains("notag"))
                {
                    return query.Where(x => x.Tags.Any() == false);
                }

                if (filter.Contains("!character") || filter.Contains("!char"))
                {
                    query = query.Where(x => !x.Tags.Any(x => x.Type == Data.Types.TagType.Character));
                }
                else if (filter.Contains("character") || filter.Contains("char"))
                {
                    query = query.Where(x => x.Tags.Any(x => x.Type == Data.Types.TagType.Character));
                }

                if (filter.Contains("!series"))
                {
                    query = query.Where(x => !x.Tags.Any(x => x.Type == Data.Types.TagType.Series));
                }
                else if (filter.Contains("series"))
                {
                    query = query.Where(x => x.Tags.Any(x => x.Type == Data.Types.TagType.Series));
                }

                return _tagDataService.GetTagFilter(filter).GetQuery(query);
            }

            return query;
        }

        public PreviewRepository(RepositoryContext context) : base(context)
        {
            _tagDataService = new(context);
        }

        public void SetPreviewTags(Preview preview, List<Tag>? tags)
        {
            if (preview.Collection != null)
            {
                preview = preview.Collection.Previews.OrderBy(x => x.Order).First();
            }

            var affectedTags = new List<Tag>(preview.Tags);
            preview.Tags.Clear();
            if (tags != null)
            {
                affectedTags.AddRange(tags);
                preview.Tags.AddRange(tags);
            }

            _context.Previews.Update(preview);
            SaveChanges();

            _tagDataService.RecalculateCounts(affectedTags.Distinct().ToList());
        }

        public async Task<int> GetListCountAsync(ListBuilder<Preview> listBuilder)
        {
            var query = _context.Previews.AsNoTracking().Where(x => x.Order == 0);

            return await SetFilterToQuery(query, listBuilder.Filter).Where(x => x.CreatorId == _actorId)
                                                                    .CountAsync();
        }

        public List<Preview> GetList(ListBuilder<Preview> listBuilder)
        {
            var query = _context.Previews.AsNoTracking().Where(x => x.Order == 0);
            query = SetFilterToQuery(query, listBuilder.Filter);

            if (listBuilder.Sort.Contains("Random") &&
                int.TryParse(listBuilder.Sort.Split('_').Last(), out int seed))
            {
                var previewIdsList = _context.Previews.Where(x => x.Order == 0)
                                                      .Select(x => x.Id)
                                                      .Shuffle(seed)
                                                      .ToList();

                return query.Where(x => previewIdsList.Any(id => id == x.Id) 
                                     && x.CreatorId == _actorId)
                            .Include(x => x.Collection)
                            .ToList()
                            .OrderBy(x => previewIdsList.IndexOf(x.Id))
                            .Skip(listBuilder.Offset)
                            .Take(listBuilder.Count)
                            .ToList();
            }  

            return query.Order(listBuilder.Order)
                        .Where(x => x.CreatorId == _actorId)
                        .Skip(listBuilder.Offset)
                        .Take(listBuilder.Count)
                        .Include(x => x.Collection)
                        .ToList();
        }

        public override bool TryRemove(Guid id)
        {
            var preview = Get(id);

            if (preview == null)
            {
                return false;
            }

            var size = preview.Media.Size;
            
            if (preview.Collection != null)
            {
                preview.Collection.Previews = preview.Collection.Previews.OrderBy(x => x.Order)
                                                                         .ToList();
                if (preview.Order == 0)
                {
                    if (preview.Collection.Previews.Count > 1)
                    {
                        preview.Collection.Previews[1].Order = 0;
                        preview.Collection.Previews[1].Tags = preview.Tags;
                        preview.Collection.Count--;

                        _context.Collections.Update(preview.Collection);
                        _context.Medias.Remove(preview.Media);
                        SaveChanges();
                        _logger.Info("Removed Media in Collection with id: {preview.Collection.Id} by: {_actorId}", 
                            preview.Collection.Id, _actorId);
                        _statisticService.MediasCountChanged.Invoke(-1, -size);

                        return true;
                    }

                    var collectionId = preview.Collection.Id;

                    _context.Medias.Remove(preview.Media);
                    _context.Collections.Remove(preview.Collection);
                    SaveChanges();
                    _logger.Info("Removed Collection with id: {collectionId} by: {_actorId}", collectionId, _actorId);
                    _statisticService.MediasCountChanged.Invoke(-1, -size);

                    return true;
                }
            }

            var mediaId = preview.Media.Id;

            _context.Medias.Remove(preview.Media);
            SaveChanges();
            _logger.Info("Removed Media  with id: {mediaId} by: {_actorId}", mediaId, _actorId);
            _statisticService.MediasCountChanged.Invoke(-1, -size);

            return true;
        }
    }
}
