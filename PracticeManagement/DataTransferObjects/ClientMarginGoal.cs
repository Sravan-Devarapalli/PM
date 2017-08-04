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
    public class ClientMarginGoal
    {
        [DataMember]
        public int? Id
        {
            get;
            set;
        }

        [DataMember]
        public int ClientId
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
        public int MarginGoal
        {
            get;
            set;
        }

        [DataMember]
        public string Comments
        {
            get;
            set;
        }
    }
}

