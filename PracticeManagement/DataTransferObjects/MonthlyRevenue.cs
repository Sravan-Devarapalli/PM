using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObjects
{
    public class MonthlyRevenue
    {
        [DataMember]
        public int? Id
        {
            get;
            set;
        }

        [DataMember]
        public int? MilestoneId
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
        public Decimal Amount
        {
            get;
            set;
        }
    }
}

