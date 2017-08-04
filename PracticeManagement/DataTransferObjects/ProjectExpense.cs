using System;
using System.Runtime.Serialization;
using System.Web;
using System.Collections.Generic;

namespace DataTransferObjects
{
    /// <summary>
    ///
    /// </summary>
    /// 
    [Serializable]
    [DataContract]
    public class ProjectExpense : IIdNameObject
    {
        [DataMember]
        public int? Id { get; set; }

        /// <summary>
        /// Expense name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        public string HtmlEncodedName
        {
            get
            {
                return HttpUtility.HtmlEncode(Name);
            }
        }

        [DataMember]
        public int ProjectId { get; set; }

        /// <summary>
        /// Expense Start Date
        /// </summary>
        [DataMember]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Expense End Date
        /// </summary>
        [DataMember]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Actual Expense $
        /// </summary>
        [DataMember]
        public decimal? Amount { get; set; }

        /// <summary>
        /// ExpectedAmount $
        /// </summary>
        [DataMember]
        public decimal ExpectedAmount { get; set; }

        /// <summary>
        /// ProjectedRemainingAmount $
        /// </summary>
        [DataMember]
        public decimal ProjectedRemainingAmount { get; set; }

        [DataMember]
        public decimal BudgetAmount { get; set; }

      
        public decimal EACAmount
        {
            get
            {
                return (Amount != null ? Amount.Value : 0) + ProjectedRemainingAmount;
            }
        }

        /// <summary>
        /// Reimbursement %
        /// </summary>
        [DataMember]
        public decimal Reimbursement { get; set; }

        /// <summary>
        /// Milestone
        /// </summary>
        [DataMember]
        public Milestone Milestone { get; set; }

        [DataMember]
        public string MilestoneName { get; set; }
        /// <summary>
        /// Reimbursed $
        /// </summary>
        public decimal ReimbursementAmount
        {
            get
            {
                if (Amount != null)
                {
                    return 0.01M * Amount.Value * Reimbursement;
                }
                else
                {
                    return 0;
                }
            }
        }

        public decimal EstimatedReimbursementAmount
        {
            get
            {
                return 0.01M * ExpectedAmount * Reimbursement;
            }
        }

        public decimal Difference
        {
            get
            {
                if (Amount != null)
                {
                    return ExpectedAmount - Amount.Value;
                }
                else
                {
                    return ExpectedAmount;
                }
            }
        }

        public decimal DifferencePer
        {
            get
            {
                return ExpectedAmount != 0 ? Difference / ExpectedAmount : 0;
            }
        }

        [DataMember]
        public ExpenseType Type
        {
            get;
            set;
        }

        [DataMember]
        public List<PeriodicalExpense> MonthlyExpense
        {
            get;
            set;
        }
    }
}

