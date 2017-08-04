using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObjects
{
    [DataContract]
    [Serializable]
    public class ProjectBudgetResource
    {

        [DataMember]
        public int ProjectId
        {
            get;
            set;
        }

        [DataMember]
        public int EntryId
        {
            get;
            set;
        }

        [DataMember]
        public Person Person
        {
            get;
            set;
        }

        [DataMember]
        public ProjectRevenue Budget
        {
            get;
            set;
        }

        [DataMember]
        public ProjectRevenue ProjectedRemaining
        {
            get;
            set;
        }

        [DataMember]
        public ProjectRevenue Actuals
        {
            get;
            set;
        }

        [DataMember]
        public ProjectRevenue Projected
        {
            get;
            set;
        }

        [DataMember]
        public ProjectRevenue EAC
        {
            get;
            set;
            //get
            //{
            //    return new ProjectRevenue
            //    {
            //        Hours = (ProjectedRemaining != null ? ProjectedRemaining.Hours : 0) + (Actuals != null ? Actuals.Hours : 0),
            //        Margin = (ProjectedRemaining != null ? ProjectedRemaining.Margin : 0) + (Actuals != null ? Actuals.Margin : 0),
            //        Revenue = (ProjectedRemaining != null ? ProjectedRemaining.Revenue : 0) + (Actuals != null ? Actuals.Revenue : 0)
            //    };
            //}
        }

        [DataMember]
        public Dictionary<DateTime, ComputedFinancials> WeeklyFinancials
        {
            get;
            set;
        }

        [DataMember]
        public decimal? BillRate
        {
            get;
            set;
        }

        [DataMember]
        public PersonRole Role
        {
            get;
            set;
        }

    }
}

