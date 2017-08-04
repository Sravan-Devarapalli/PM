using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObjects
{
    public class PeriodicalExpense
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
        public Decimal EstimatedExpense
        {
            get;
            set;
        }

        [DataMember]
        public Decimal? ActualExpense
        {
            get;
            set;
        }

        [DataMember]
        public int? ProjectExpenseId
        {
            get;
            set;
        }

        public Decimal Variance
        {
            get
            {

                return EstimatedExpense - (ActualExpense != null ? ActualExpense.Value : 0);
            }
        }
    }
}

