using System;
using System.Runtime.Serialization;

namespace DataTransferObjects
{
    [Serializable]
    [DataContract]
    public class ClientMarginColorInfo
    {
        [DataMember]
        public int Id
        {
            get;
            set;
        }

        [DataMember]
        public decimal EndRange
        {
            get;
            set;
        }

        [DataMember]
        public decimal StartRange
        {
            get;
            set;
        }

        [DataMember]
        public ColorInformation ColorInfo
        {
            get;
            set;
        }
    }
}
