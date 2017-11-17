using System;
using System.Runtime.Serialization;

namespace DataTransferObjects.Reports
{
    [DataContract]
    [Serializable]
    public class PersonTimeEntriesTotals
    {
        [DataMember]
        public double BillableHours
        {
            get;
            set;
        }

        [DataMember]
        public double BillableHoursUntilToday
        {
            get;
            set;
        }

        [DataMember]
        public double NonBillableHours
        {
            get;
            set;
        }

        [DataMember]
        public double AvailableHours
        {
            get;
            set;
        }

        [DataMember]
        public double ProjectedHours
        {
            get;
            set;
        }

        public string BillableUtilizationPercentage
        {
            get
            {
                return (AvailableHours == 0) ? 0 + "%" : ((double)(BillableHoursUntilToday / AvailableHours) * 100).ToString("0.0") + "%";
            }
        }

        public double BillableAllocatedVsTargetValue
        {
            get
            {
                return BillableHoursUntilToday - ProjectedHours;
            }
        }

        public string BillableAllocatedVsTarget
        {
            get
            {
                var value = BillableHoursUntilToday - ProjectedHours;
                if (value < 0)
                {
                    return "(" + (-1* value) +")";
                }
                else
                {
                    return value.ToString("0");
                }
            }
        }
    }
}