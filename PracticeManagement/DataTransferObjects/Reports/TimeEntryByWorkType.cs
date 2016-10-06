﻿using System;
using System.Runtime.Serialization;
using System.Web;
using DataTransferObjects.TimeEntry;

namespace DataTransferObjects.Reports
{
    [DataContract]
    [Serializable]
    public class TimeEntryByWorkType
    {
        [DataMember]
        public TimeTypeRecord TimeType { get; set; }

        [DataMember]
        public string Note { get; set; }

        [DataMember]
        public double BillableHours { get; set; }

        [DataMember]
        public double NonBillableHours { get; set; }

        [DataMember]
        public double ForecastedHoursDaily { get; set; }

        [DataMember]
        public double BillRate { get; set; }

        [DataMember]
        public string BillingType { get; set; }

        /// <summary>
        /// Hourly Bill Rate from MilestonePerson
        /// </summary>
        [DataMember]
        public decimal? HourlyRate { get; set; }

        /// <summary>
        /// Hourly Pay Rate from Pay(Compensation) Table.
        /// </summary>
        [DataMember]
        public decimal? PayRate { get; set; }

        /// <summary>
        /// Returns the Timescale type Name.
        /// </summary>
        [DataMember]
        public string PayType { get; set; }

        public double TotalHours
        {
            get
            {
                return BillableHours + NonBillableHours;
            }
        }

        public string NoteForExport
        {
            get
            {
                return Note.Replace("\n", " ").Replace("\r", " ");
            }
        }

        public string HtmlEncodedNoteForExport
        {
            get
            {
                return HttpUtility.HtmlEncode(NoteForExport);
            }
        }

        public string HTMLNote
        {
            get
            {
                return Note.Replace("\n", "<br/>");
            }
        }

        public string HtmlEncodedHTMLNote
        {
            get
            {
                return HttpUtility.HtmlEncode(HTMLNote).Replace("&lt;br/&gt;", "<br/>");
            }
        }
    }
}

