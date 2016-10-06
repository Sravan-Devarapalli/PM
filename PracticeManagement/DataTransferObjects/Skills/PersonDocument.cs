﻿using System;
using System.Runtime.Serialization;

namespace DataTransferObjects.Skills
{
    [DataContract]
    [Serializable]
    public class PersonDocument
    {
        [DataMember]
        public DocumentType Type
        {
            get;
            set;
        }

        [DataMember]
        public Person Person
        {
            get;
            set;
        }

        [DataMember]
        public string Url
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? ModificationDate
        {
            get;
            set;
        }
    }
}
