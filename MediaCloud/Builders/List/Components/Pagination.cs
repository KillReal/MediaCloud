namespace MediaCloud.WebApp.Builders.List.Components
{
    public class Pagination
    {
        public int Count { get; set; }
        public int TotalCount { get; set; }
        public int Offset { get; set; }
        //TODO get setting
        public int PageMaxCount { get; }

        public int CurrentPageNumber { get; }
        public int StartPageNumber { get; set; }
        public int EndPageNumber { get; set; }

        public Pagination(int count, int offset, int maxPages)
        {
            Count = count;
            TotalCount = count;
            Offset = offset;
            PageMaxCount = maxPages;

            CurrentPageNumber = offset / count;
            StartPageNumber = CurrentPageNumber - PageMaxCount / 2 < 0
                ? 0
                : CurrentPageNumber - PageMaxCount / 2;

            EndPageNumber = (int)Math.Round(CurrentPageNumber + (double)PageMaxCount / 2, MidpointRounding.ToPositiveInfinity);
        }

        /// <summary>
        /// Set <see cref="TotalCount"/> and recalculate page numbers.
        /// </summary>
        /// <param name="totalCount">Total entities count in DataService.</param>
        public void SetTotalCount(int totalCount)
        {
            TotalCount = totalCount;

            var offsetRef = PageMaxCount / 2;
            var leftOffset = CurrentPageNumber - StartPageNumber;
            if (leftOffset < offsetRef)
            {
                EndPageNumber += offsetRef - leftOffset;
            }

            EndPageNumber = EndPageNumber * Count > TotalCount
                ? (int)Math.Round((double)TotalCount / Count, MidpointRounding.ToPositiveInfinity)
                : EndPageNumber;

            var rightOffset = EndPageNumber - CurrentPageNumber;
            if (rightOffset < offsetRef)
            {
                StartPageNumber -= offsetRef - rightOffset;
            }

            StartPageNumber = StartPageNumber < 0
                ? 0
                : StartPageNumber;
        }
    }
}
