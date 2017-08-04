using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObjects.Reports
{
    [DataContract]
    [Serializable]
    public class ProjectBurnFinancials
    {
        [DataMember]
        public int ProjectId
        {
            get;
            set;
        }

        [DataMember]
        public List<ProjectBudgetResource> Resources
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<DateTime, ComputedFinancials> Financials
        {
            get;
            set;
        }

        [DataMember]
        public List<ProjectExpense> Expenses
        {
            get;
            set;
        }

        [DataMember]
        public decimal MarginGoal
        {
            get;
            set;
        }

        [DataMember]
        public int? BudgetAmount
        {
            get;
            set;
        }

        [DataMember]
        public int TotalProjectDays
        {
            get;
            set;
        }

        [DataMember]
        public int RemainingDays
        {
            get;
            set;
        }

        public int CompletedDays
        {
            get {
                return TotalProjectDays - RemainingDays;
            }
        }

    }
}

