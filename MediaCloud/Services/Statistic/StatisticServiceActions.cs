using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.WebApp.Services.DataService;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MediaCloud.WebApp.Services.Statistic
{
    public partial class StatisticService : IStatisticService
    {
        public Action<int, long> MediasCountChanged { get; set; }
        public Action<int> TagsCountChanged { get; set; }
        public Action<int> ActorsCountChanged { get; set; }
        public Action ActivityFactorRaised { get; set; }

        private void MediasCountChangedAction(int affectedCount, long affectedSize)
        {
            _currentSnapshot.ActivityFactor += 1;
            _currentSnapshot.MediasCount += affectedCount;
            _currentSnapshot.MediasSize += affectedSize;
            _serviceHelper.SaveOrUpdate(_currentSnapshot);
        }

        private void TagsCountChangedAction(int affectedCount)
        {
            _currentSnapshot.ActivityFactor += 1;
            _currentSnapshot.TagsCount += affectedCount;
            _serviceHelper.SaveOrUpdate(_currentSnapshot);
        }
        private void ActorsCountChangedAction(int affectedCount)
        {
            _currentSnapshot.ActivityFactor += 1;
            _currentSnapshot.ActorsCount += affectedCount;
            _serviceHelper.SaveOrUpdate(_currentSnapshot);
        }

        private void ActivityFactorRaisedAction()
        {
            _currentSnapshot.ActivityFactor += 1;
            _serviceHelper.SaveOrUpdate(_currentSnapshot);
        }

    }
}
