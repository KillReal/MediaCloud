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

        public StatisticSnapshot Merge(StatisticSnapshot snapshot)
        {
            MediasCount += snapshot.MediasCount;
            TagsCount += snapshot.TagsCount;
            ActorsCount += snapshot.ActorsCount;
            MediasSize += snapshot.MediasSize;

            Creator = snapshot.Creator;
            Updator = snapshot.Updator;

            return this;
        }

        public bool IsEmpty()
        {
            return (MediasCount == 0) &&
                    (TagsCount == 0) &&
                    (ActorsCount == 0) &&
                    (MediasSize == 0);
        }
    }
}
