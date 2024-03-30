using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MediaCloud.WebApp.Services.Statistic
{
    public partial class StatisticProvider
    {
        public Action<int, long> MediasCountChanged { get; set; }
        public Action<int> TagsCountChanged { get; set; }
        public Action<int> ActorsCountChanged { get; set; }
        public Action ActivityFactorRaised { get; set; }

        private void MediasCountChangedAction(int affectedCount, long affectedSize)
        {
            var snapshot = GetTodaySnapshot();
            snapshot.ActivityFactor += 1;
            snapshot.MediasCount += affectedCount;
            snapshot.MediasSize += affectedSize;
            CreateOrUpdateSnapshotByDate(snapshot, DateTime.Now.Date);
        }

        private void TagsCountChangedAction(int affectedCount)
        {
            var snapshot = GetTodaySnapshot();
            snapshot.ActivityFactor += 1;
            snapshot.TagsCount += affectedCount;
            CreateOrUpdateSnapshotByDate(snapshot, DateTime.Now.Date);
        }
        private void ActorsCountChangedAction(int affectedCount)
        {
            var snapshot = GetTodaySnapshot();
            snapshot.ActivityFactor += 1;
            snapshot.ActorsCount += affectedCount;
            CreateOrUpdateSnapshotByDate(snapshot, DateTime.Now.Date);
        }

        private void ActivityFactorRaisedAction()
        {
            var snapshot = GetTodaySnapshot();
            snapshot.ActivityFactor += 1;
            CreateOrUpdateSnapshotByDate(snapshot, DateTime.Now.Date);
        }

    }
}
