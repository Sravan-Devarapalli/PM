﻿using System;
using System.Runtime.Serialization;

namespace DataTransferObjects.ContextObjects
{
    [DataContract]
    [Serializable]
    public class OpportunityListContext
    {
        public OpportunityListContext()
        {
            ActiveClientsOnly = true;
        }

        [DataMember]
        public bool ActiveClientsOnly { get; set; }

        [DataMember]
        public string SearchText { get; set; }

        [DataMember]
        public int? ClientId { get; set; }

        [DataMember]
        public int? SalespersonId { get; set; }

        [DataMember]
        public int? TargetPersonId { get; set; }

        [DataMember]
        public int? CurrentId { get; set; }

        [DataMember]
        public bool IsDiscussionReview2 { get; set; }
    }
}
