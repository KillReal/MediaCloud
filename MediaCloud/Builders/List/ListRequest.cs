namespace MediaCloud.Builders.List
{
    /// <summary>
    /// Request for <see cref="ListBuilder{T}"/> with parameters of building.
    /// </summary>
    public class ListRequest
    {
        /// <summary>
        /// Entities count for single page.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Entities offset for certain page.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Sort property, see <see cref="WebApp.Builders.List.Components.Sorting"/> for propertyName formatting.
        /// </summary>
        public string Sort { get; set; } = "UpdatedAtDesc";

        /// <summary>
        /// Filter for entities.
        /// </summary>
        public string Filter { get; set; } = "";

        public bool? IsUseAutoload { get; set; } = null;
    }
}
