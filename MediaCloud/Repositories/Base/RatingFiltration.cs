using System.Linq.Expressions;
using MediaCloud.Extensions;
using MediaCloud.WebApp.Data.Models.Interfaces;
using MediaCloud.WebApp.Data.Types;

namespace MediaCloud.WebApp.Repositories.Base
{
    public class RatingFiltration<T> where T : IPreviewRatable
    {
        private string _filterWithoutRatings;
        private readonly List<PreviewRatingType> _positiveRatingTypes = [];
        private readonly List<PreviewRatingType> _negativeRatingTypes = [];
        
        public RatingFiltration(string filter)
        {
            var types = Enum.GetValues(typeof(PreviewRatingType)).Cast<PreviewRatingType>();
            
            foreach (var type in types)
            {
                if (filter.Contains($"!{type.GetDisplayName()}", StringComparison.CurrentCultureIgnoreCase))
                {
                    _negativeRatingTypes.Add(type);
                    filter = filter.Replace($"!{type.GetDisplayName().ToLower()}", string.Empty);
                }
                else if (filter.Contains(type.GetDisplayName(), StringComparison.CurrentCultureIgnoreCase))
                {
                    _positiveRatingTypes.Add(type);
                    filter = filter.Replace(type.GetDisplayName().ToLower(), string.Empty);
                }
            }
            
            _filterWithoutRatings = filter;
        }
        
        public Expression<Func<T, bool>> GetExpression()
        {
            if (_positiveRatingTypes.Count == 0 && _negativeRatingTypes.Count == 0)
            {
                return x => true;
            }
            
            return x => (_positiveRatingTypes.Any(y => x.Rating == y) || _positiveRatingTypes.Count == 0) 
                        && _negativeRatingTypes.Any(y => x.Rating == y) == false;
        }

        public string GetFilterWIthoutRatings()
        {
            return _filterWithoutRatings;
        }

        public static List<string> GetAliasSuggestions()
        {
            return Enum.GetValues(typeof(PreviewRatingType)).Cast<PreviewRatingType>().Select(x => x.GetDisplayName()).ToList();
        }
    }
}
