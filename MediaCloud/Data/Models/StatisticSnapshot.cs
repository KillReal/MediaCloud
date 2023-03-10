namespace MediaCloud.Data.Models
{
    public class StatisticSnapshot : Entity
    {
        public int MediasCount { get; set; }
        public int TagsCount { get; set; }
        public int ActorsCount { get; set; }
        public long MediasSize { get; set; }
        public long ActivityFactor { get; set; }
        public DateTime TakenAt { get; set; }

        public StatisticSnapshot AppendParameters(StatisticSnapshot snapshot)
        {
            MediasCount += snapshot.MediasCount;
            TagsCount += snapshot.TagsCount;
            ActorsCount += snapshot.ActorsCount;
            MediasSize += snapshot.MediasSize;

            return this;
        }
    }
}
