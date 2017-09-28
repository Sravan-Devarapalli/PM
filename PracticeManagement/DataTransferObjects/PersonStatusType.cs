using System.ComponentModel;
using System.Runtime.Serialization;

namespace DataTransferObjects
{
    /// <summary>
    /// Determines the status of project
    /// </summary>
    [DataContract]
    public enum PersonStatusType
    {
        [EnumMember]
        Active = 1,

        [EnumMember]
        Terminated = 2,

        [EnumMember]
        //Projected = Contingent
        Contingent = 3,

        [EnumMember]
        Inactive = 4,

        [EnumMember]
        [Description("Termination Pending")]
        TerminationPending = 5,

        [EnumMember]
        [Description("Right to Present")]
        RightToPresent = 6
    }
}
