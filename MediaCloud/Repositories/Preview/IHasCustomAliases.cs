using System.Linq.Expressions;
using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Repositories;

public interface IHasCustomAliases
{
    public static abstract List<string> GetAliasSuggestions();
}