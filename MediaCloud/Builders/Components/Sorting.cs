using DynamicExpression.Entities;
using DynamicExpression.Interfaces;
using System.Reflection;

namespace MediaCloud.Builders.Components
{
    /// <summary>
    /// Sort for <see cref="Builders.List.ListBuilder{T}"/>
    /// </summary>
    public class Sorting
    {
        /// <summary>
        /// Sort property name, use 'Desc' in value for descendation soring.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Init sorting with <see cref="PropertyName"/>.
        /// </summary>
        /// <param name="propertyName">Property name. Use 'Desc'for <code>"PropertyNameDesc"</code> for descendation soring.</param>
        public Sorting(string propertyName)
        {
            PropertyName = propertyName;
        }
         
        /// <summary>
        /// Return ordering for querying entities.
        /// </summary>
        /// <returns><see cref="Ordering"/> for query.</returns>
        public Ordering GetOrder()
        {
            return new Ordering()
            {
                By = PropertyName.Replace("Desc", string.Empty),
                Direction = PropertyName.Contains("Desc") ? 
                    DynamicExpression.Enums.OrderingDirection.Desc 
                    : 
                    DynamicExpression.Enums.OrderingDirection.Asc
            };
        }
    }
}
