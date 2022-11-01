namespace MediaCloud.Builders.List
{
    public class ListRequest
    {
        public int Count { get; set; }

        public int Offset { get; set; }

        public string Sort { get; set; }

        public string Filter { get; set; }
    }
}
