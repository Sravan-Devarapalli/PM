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
    public class MarginExceptionReason
    {
        [DataMember]
        public int Id
        {
            get;
            set;
        }

        [DataMember]
        public string Reason
        {
            get;
            set;
        }
    }
}

