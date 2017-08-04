using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObjects
{
    [Serializable]
    [DataContract]
    public class OpenTask
    {
        [DataMember]
        public Project Project
        {
            get;
            set;
        }

        [DataMember]
        public ClientMarginException Threshold
        {
            get;
            set;
        }

        [DataMember]
        public decimal Revenue
        {
            get;
            set;
        }

        [DataMember]
        public decimal RevenueNet
        {
            get;
            set;
        }

        [DataMember]
        public decimal GrossMargin
        {
            get;
            set;
        }

        public decimal TargetMargin
        {
            get { return RevenueNet != 0M ? (GrossMargin * 100M) / RevenueNet : 0M; }
        }

        public decimal VarianceMargin
        {
            get
            {
                return TargetMargin - Threshold.MarginGoal;
            }
        }

        public string Task
        {
            get
            {
                string value = string.Empty;
                if ( Threshold.Revenue != 0 && (VarianceMargin < Threshold.MarginThreshold || ((Project.BusinessType == BusinessType.Extension) ? false : Threshold.Revenue <= Revenue)) && Project.Status.Id != (int)ProjectStatusType.Active && Project.TierOneExceptionStatus == 0)
                {
                    value = "Projected Margin Below Threshold, Exception Required";
                }
                if (Threshold.Revenue != 0 && (VarianceMargin < Threshold.MarginThreshold || ((Project.BusinessType == BusinessType.Extension) ? false : Threshold.Revenue <= Revenue)) && Project.Status.Id == (int)ProjectStatusType.Active)
                {
                    value = "Margin Below Threshold, Review Needed";
                }
                if (Project.TierOneExceptionStatus == 1)
                {
                    value = "Project locked for Margin Approval; Tier 1";
                }
                if (Project.TierTwoExceptionStatus == 1 && Project.TierOneExceptionStatus == 2)
                {
                    value = "Project locked for Margin Approval; Tier 2";
                }
                if (Project.Status.Id == (int)ProjectStatusType.AtRisk)
                {
                    value = "At Risk, Review Outstanding Items";
                }

                return value;
            }

        }

    }
}

