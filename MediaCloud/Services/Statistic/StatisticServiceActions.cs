using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.Repository;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MediaCloud.WebApp.Services.Statistic
{
    public partial class StatisticService : IStatisticService
    {
        public Action<int> MediasCountChanged { get; set; }
        public Action<int> TagsCountChanged { get; set; }
        public Action<int> ActorsCountChanged { get; set; }
        public Action ActivityFactorRaised { get; set; }

        private void InitListeners()
        {
            MediasCountChanged += MediasCountChangedAction;
            TagsCountChanged += TagsCountChangedAction;
            ActorsCountChanged += ActorsCountChangedAction;
            ActivityFactorRaised += ActivityFactorRaisedAction;
        }

        private void MediasCountChangedAction(int affectedCount)
        {
            CurrentSnapshot.ActivityFactor += 1;
            CurrentSnapshot.MediasCount += affectedCount;
            ServiceHelper.SaveOrUpdate(CurrentSnapshot);
        }

        private void TagsCountChangedAction(int affectedCount)
        {
            CurrentSnapshot.ActivityFactor += 1;
            CurrentSnapshot.TagsCount += affectedCount;
            ServiceHelper.SaveOrUpdate(CurrentSnapshot);
        }
        private void ActorsCountChangedAction(int affectedCount)
        {
            CurrentSnapshot.ActivityFactor += 1;
            CurrentSnapshot.ActorsCount += affectedCount;
            ServiceHelper.SaveOrUpdate(CurrentSnapshot);
        }

        private void ActivityFactorRaisedAction()
        {
            CurrentSnapshot.ActivityFactor += 1;
            ServiceHelper.SaveOrUpdate(CurrentSnapshot);
        }

    }
}
