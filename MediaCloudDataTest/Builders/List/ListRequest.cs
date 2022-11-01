namespace MediaCloud.Builders.List
{
    public class ListRequest
    {
        public int Count { get; set; } = 30;

        public int Offset { get; set; } = 0;

        public string Sort { get; set; } = "UpdatedAtDesc";

        public string Filter { get; set; } = "";
    }
}
