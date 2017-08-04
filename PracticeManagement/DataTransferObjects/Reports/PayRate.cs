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
    public class PayRate
    {
        [DataMember]
        public int PersonId
        {
            get;
            set;
        }

        [DataMember]
        public PracticeManagementCurrency BillRate
        {
            get;
            set;
        }

        [DataMember]
        public PracticeManagementCurrency PersonCost
        {
            get;
            set;
        }
    }
}

