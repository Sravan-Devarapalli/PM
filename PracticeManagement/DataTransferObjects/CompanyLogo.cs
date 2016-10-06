﻿using System;
using System.Runtime.Serialization;

namespace DataTransferObjects
{
    public class CompanyLogo
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public Byte[] Data { get; set; }
    }
}
