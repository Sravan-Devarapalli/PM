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
    public class ClientMarginException
    {
        [DataMember]
        public int? Id
        {
            get;
            set;
        }

        [DataMember]
        public DateTime StartDate
        {
            get;
            set;
        }

        [DataMember]
        public DateTime EndDate
        {
            get;
            set;
        }

        [DataMember]
        public ApprovalLevel Level
        {
            get;
            set;
        }

        [DataMember]
        public int MarginThreshold
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
        public decimal MarginGoal
        {
            get;
            set;
        }
    }
}

