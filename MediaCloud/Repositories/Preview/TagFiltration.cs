using System.Linq.Expressions;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Data.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.WebApp.Repositories
{
    public class TagFiltration<T> : IHasCustomAliases where T : ITaggable
    {
        private readonly bool _noTags;
        private List<Guid> _positiveTagIds { get; set; }

        private List<Guid> _negativeTagIds { get; set; }

        public TagFiltration(string filter, DbSet<Tag> tagsDbSet)
        {
            var tags = GetDeduplicatedTags(filter);
            
            if (filter.Contains("notags"))
            {
                _noTags = true;
                return;
            }

            var positiveTags = new List<string>();
            var negativeTags = new List<string>();
            
            foreach (var tag in tags)
            {
                if (tag.Contains('!'))
                {
                    negativeTags.Add(tag[1..]);
                    continue;
                }

                positiveTags.Add(tag);
            }

            _positiveTagIds = tagsDbSet.Where(x => positiveTags.Any(y => y.ToLower() == x.Name.ToLower()))
                .Select(x => x.Id).ToList();
            _negativeTagIds = tagsDbSet.Where(x => negativeTags.Any(y => y.ToLower() == x.Name.ToLower()))
                .Select(x => x.Id).ToList();
        }
        
        public Expression<Func<T, bool>> GetExpression()
        {
            if (_noTags)
            {
                return x => x.Tags.Count == 0;
            }
            
            return x => (x.Tags.Concat(x.Collection.Previews.Where(x => x.Order != 0)
                                    .SelectMany(z => z.Tags))
                                .Distinct()
                                .Where(y => _positiveTagIds.Contains(y.Id))
                                .Count() == _positiveTagIds.Count 
                                    && (_positiveTagIds.Count != 0 || _negativeTagIds.Count != 0) 
                        )
                        && (x.Tags.Concat(x.Collection.Previews.Where(x => x.Order != 0)
                                .SelectMany(z => z.Tags))
                            .Distinct()
                            .Where(y => _negativeTagIds.Contains(y.Id))
                            .Any() == false
                        );
        }
        
        private static string[] GetDeduplicatedTags(string tagString)
        {
            var tags = tagString.Split(' ');

            return tags.Length < 2 
                ? tags 
                : [.. tags.Distinct()];
        }

        public static List<string> GetAliasSuggestions()
        {
            return ["NoTags"];
        }
    }
}
