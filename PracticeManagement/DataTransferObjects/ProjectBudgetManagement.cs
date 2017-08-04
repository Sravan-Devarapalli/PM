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
    public class ProjectBudgetManagement
    {


        [DataMember]
        public int ProjectId
        {
            get;
            set;
        }

        [DataMember]
        public List<ProjectBudgetResource> BudgetResources
        {
            get;
            set;
        }

        [DataMember]
        public ProjectRevenue BudgetSummary
        {
            get;
            set;
        }

        [DataMember]
        public ProjectRevenue ProjectedSummary
        {
            get;
            set;
        }

        [DataMember]
        public ProjectRevenue ActualsSummary
        {
            get;
            set;
        }

        [DataMember]
        public ProjectRevenue EACSummary {
            get;
            set;
            //get {
            //    return new ProjectRevenue
            //    {
            //        Hours = ActualsSummary.Hours + ProjectedSummary.Hours,
            //        Revenue = ActualsSummary.Revenue + ProjectedSummary.Revenue,
            //        Expenses = ActualsSummary.Expenses + ProjectedSummary.Expenses,
            //        Margin = ActualsSummary.Margin + ProjectedSummary.Margin
            //    };
            //}
        }
    }
}

