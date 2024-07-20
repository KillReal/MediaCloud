using DynamicExpression.Entities;

namespace MediaCloud.WebApp.Builders.List.Components
{
    /// <summary>
    /// Sort for <see cref="List.ListBuilder{T}"/>
    /// </summary>
    /// <remarks>
    /// Init sorting with <see cref="PropertyName"/>.
    /// </remarks>
    /// <param name="propertyName">Property name. Use 'Desc'for <code>"PropertyNameDesc"</code> for descendation soring.</param>
    public class Sorting(string propertyName)
    {
        /// <summary>
        /// Sort property name, use 'Desc' in value for descendation soring.
        /// </summary>
        public string PropertyName { get; set; } = propertyName;

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
