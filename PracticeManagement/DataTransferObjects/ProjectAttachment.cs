﻿using System;
using System.Runtime.Serialization;

namespace DataTransferObjects
{
    [DataContract]
    [Serializable]
    public class ProjectAttachment
    {
        [DataMember]
        public int AttachmentId
        {
            get;
            set;
        }

        [DataMember]
        public String AttachmentFileName
        {
            get;
            set;
        }

        [DataMember]
        public ProjectAttachmentCategory Category
        {
            get;
            set;
        }

        [DataMember]
        public Byte[] AttachmentData
        {
            get;
            set;
        }

        [DataMember]
        public int AttachmentSize
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? UploadedDate
        {
            get;
            set;
        }

        [DataMember]
        public string Uploader
        {
            get;
            set;
        }
    }
}
