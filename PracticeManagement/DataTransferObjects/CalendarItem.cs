﻿using System;
using System.Runtime.Serialization;

namespace DataTransferObjects
{
    /// <summary>
    /// Represents a calendar day
    /// </summary>
    [DataContract]
    [Serializable]
    public class CalendarItem : IComparable<CalendarItem>, IEquatable<DateTime>, IEquatable<CalendarItem>
    {
        #region Properties

        /// <summary>
        /// Gets or sets an item's date.
        /// </summary>
        [DataMember]
        public DateTime Date
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? SubstituteDayDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets is a day is day-off
        /// </summary>
        [DataMember]
        public bool DayOff
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the day is the company-wide day off.
        /// </summary>
        [DataMember]
        public bool CompanyDayOff
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an ID of the person
        /// </summary>
        [DataMember]
        public int? PersonId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets if the calendar item is read only for some raeson.
        /// </summary>
        /// <remarks>
        /// Uses to protect data from unapproved editing or when editing is impossible.
        /// </remarks>
        [DataMember]
        public bool ReadOnly
        {
            get;
            set;
        }

        [DataMember]
        public bool IsRecurringHoliday
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? RecurringHolidayDate
        {
            get;
            set;
        }

        [DataMember]
        public string HolidayDescription
        {
            get;
            set;
        }

        [DataMember]
        public int? RecurringHolidayId
        {
            get;
            set;
        }

        [DataMember]
        public double? ActualHours
        {
            get;
            set;
        }

        [DataMember]
        public bool IsFloatingHoliday
        {
            get;
            set;
        }

        [DataMember]
        public bool IsFloatingVacation
        {
            get;
            set;
        }

        [DataMember]
        public int? TimeTypeId { get; set; }

        [DataMember]
        public bool IsUnpaidTimeType { get; set; }

        #endregion Properties

        public int CompareTo(CalendarItem other)
        {
            return other == null ? -1 : other.Date.CompareTo(Date);
        }

        public bool Equals(DateTime other)
        {
            return other.Equals(Date);
        }

        public bool Equals(CalendarItem other)
        {
            return other != null && Equals(other.Date);
        }

        public static implicit operator CalendarItem(DateTime d)
        {
            return ToCalendarItem(d);
        }

        public static CalendarItem ToCalendarItem(DateTime d)
        {
            return new CalendarItem { Date = d };
        }
    }
}

