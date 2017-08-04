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
    public class ClientMarginGoalLog
    {
        [DataMember]
        public int ClientId
        {
            get;
            set;
        }

        [DataMember]
        public int Acivity
        {
            get;
            set;
        }

        [DataMember]
        public string PersonName
        {
            get;
            set;
        }

        [DataMember]
        public DateTime LogTime
        {
            get; set;
        }

        [DataMember]
        public DateTime? OldStartDate
        {
            get; set;
        }

        [DataMember]
        public DateTime? NewStartDate
        {
            get; set;
        }

        [DataMember]
        public DateTime? OldEndDate
        {
            get; set;
        }

        [DataMember]
        public DateTime? NewEndDate
        {
            get; set;
        }

        [DataMember]
        public int? OldMarginGoal
        {
            get;
            set;
        }

        [DataMember]
        public int? NewMarginGoal
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

        public string ActivityName
        {
            get
            {
                string action = "";
                switch (Acivity)
                {
                    case 1:
                        action = "Added";
                        break;
                    case 2:
                        action = "Changed";
                        break;
                    case 3:
                        action = "Deleted";
                        break;
                }
                return action;
            }
        }
    }
}

