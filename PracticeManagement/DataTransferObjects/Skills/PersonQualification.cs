﻿using System;
using System.Runtime.Serialization;

namespace DataTransferObjects.Skills
{
    [DataContract]
    [Serializable]
    public class PersonQualification
    {
        [DataMember]
        public Person Person
        {
            set;
            get;
        }

        [DataMember]
        public QualificationType QualificationType
        {
            set;
            get;
        }

        [DataMember]
        public string Info
        {
            set;
            get;
        }

        [DataMember]
        public int? DisplayOrder
        {
            set;
            get;
        }

        [DataMember]
        public DateTime ModificationDate
        {
            set;
            get;
        }
    }
}
