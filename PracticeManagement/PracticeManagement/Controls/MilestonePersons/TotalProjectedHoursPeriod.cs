using System.Collections.Generic;
using DataTransferObjects;

namespace PraticeManagement.Controls.MilestonePersons
{
    public class TotalProjectedHoursPeriod : ProjectedHoursPeriod
    {
        public TotalProjectedHoursPeriod(IActivityPeriod activityPeriod)
            : base(activityPeriod)
        {
        }

        public new IEnumerable<DatePoint> Period
        {
            get
            {
                return new[]
                           {
                               new DatePoint
                                   {
                                       Date = ActivityPeriod.StartDate,
                                       Value = TotalProjectedHours
                                   },
                               new DatePoint
                                   {
                                       Date = ActivityPeriod.EndDate,
                                       Value = TotalProjectedHours
                                   },
                           };
            }
        }

        public override IEnumerable<DatePoint> CumulativePeriod
        {
            get { return Period; }
        }
    }
}
