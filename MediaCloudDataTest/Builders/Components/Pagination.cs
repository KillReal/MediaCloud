namespace MediaCloud.Builders.Components
{
    public class Pagination
    {
        public int Count { get; set; }
        public int TotalCount { get; set; }
        public int Offset { get; set; }
        //TODO get setting
        public int MaxPages { get; } = 20;

        public Pagination(int count, int offset)
        {
            Count = count;
            TotalCount = count;
            Offset = offset;
        }

        public Pagination()
        {

        }
    }
}
