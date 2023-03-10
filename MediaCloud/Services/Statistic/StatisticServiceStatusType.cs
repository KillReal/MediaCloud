using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MediaCloud.WebApp.Services.Statistic
{
    public enum StatisticServiceStatusType
    {
        [Display(Name = "Listen for new events")]
        ListenForEvents = 0,
        [Display(Name = "Recalculating statistic")]
        Recalculating = 1,
    }
}
