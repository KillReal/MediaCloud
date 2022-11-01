using DynamicExpression.Entities;
using DynamicExpression.Interfaces;
using System.Reflection;

namespace MediaCloud.Builders.Components
{
    public class Sorting
    {
        public string PropertyName { get; set; }

        public Sorting(string propertyName)
        {
            PropertyName = propertyName;
        }
         
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
