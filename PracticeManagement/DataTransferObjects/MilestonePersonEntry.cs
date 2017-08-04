﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace DataTransferObjects
{
    public enum MilestonePersonEntryFormat
    {
        ProjectMilestoneStartDate,
        Default = ProjectMilestoneStartDate
    }

    [Serializable]
    [DataContract]
    [DebuggerDisplay("MilestonePersonEntry; MilestonePersonId = {MilestonePersonId}, StartDate = {StartDate}, EndDate = {EndDate}")]
    public class MilestonePersonEntry : IEquatable<MilestonePersonEntry>, IComparable<MilestonePersonEntry>
    {
        #region MyRegion

        private const string PROJECT_MILESTONE_START_DATE_FORMAT = "{0} ({1})";

        #endregion MyRegion

        #region Properties

        /// <summary>
        /// Corresponding milestone
        /// </summary>
        [DataMember]
        public Milestone ParentMilestone { get; set; }

        /// <summary>
        /// Corresponding person
        /// </summary>
        [DataMember]
        public Person ThisPerson { get; set; }

        /// <summary>
        /// Gets or sets an ID of the parent record
        /// </summary>
        [DataMember]
        public int MilestonePersonId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets when the <see cref="Person"/> starts his/her work on the <see cref="Milestone"/>.
        /// </summary>
        [DataMember]
        public DateTime StartDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets when the <see cref="Person"/> ends his/her work on the <see cref="Milestone"/>.
        /// </summary>
        [DataMember]
        public DateTime? EndDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a number of working hours per day for the person on the given milestone.
        /// </summary>
        [DataMember]
        public decimal HoursPerDay
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a person location.
        /// </summary>
        [DataMember]
        public String Location
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a hourly bill rate for the project milestone association
        /// </summary>
        [DataMember]
        public PracticeManagementCurrency? HourlyAmount
        {
            get;
            set;
        }

        [DataMember]
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an VacationDays of the person on Milestone
        /// </summary>
        [DataMember]
        public int VacationDays
        {
            get;
            set;
        }

        private decimal _vacationHours = -1;

        public decimal VacationHours
        {
            get
            {
                _vacationHours = 0;
                if (PersonTimeOffList == null)
                    PersonTimeOffList = new Dictionary<DateTime, decimal>();
                foreach (DateTime date in PersonTimeOffList.Keys)
                {
                    if (date < StartDate || (EndDate.HasValue && (!EndDate.HasValue || date > EndDate.Value))) continue;
                    decimal TimeOffhours = PersonTimeOffList[date];
                    _vacationHours += TimeOffhours == 8 ? HoursPerDay : (((Decimal)TimeOffhours) / 8) * HoursPerDay;
                }
                return _vacationHours;
            }
        }

        private decimal _TimeOffHours;

        public decimal TimeOffHours
        {
            get
            {
                _TimeOffHours = 0;
                if (PersonTimeOffList == null)
                    PersonTimeOffList = new Dictionary<DateTime, decimal>();
                foreach (DateTime date in PersonTimeOffList.Keys)
                {
                    if (date < StartDate || (EndDate.HasValue && (!EndDate.HasValue || date > EndDate.Value))) continue;
                    decimal TimeOffhours = PersonTimeOffList[date];
                    _TimeOffHours += TimeOffhours;
                }
                return _TimeOffHours;
            }
        }

        [DataMember]
        public bool HasTimeEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a calculated person's rate for the milestone.
        /// </summary>
        [DataMember]
        public ComputedFinancials ComputedFinancials
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an expected workload.
        /// </summary>
        /// [DataMember]
        public decimal ProjectedWorkload
        {
            get
            {
                return ProjectedWorkloadWithVacation - VacationHours;
            }
        }

        /// <summary>
        /// Gets or sets an estimated Client Discount.
        /// </summary>
        [DataMember]
        [Obsolete]
        public PracticeManagementCurrency EstimatedClientDiscount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an expected workload + vacation days.
        /// </summary>
        [DataMember]
        public decimal ProjectedWorkloadWithVacation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a person role for the milestone.
        /// </summary>
        [DataMember]
        public PersonRole Role
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<DateTime, Decimal> PersonTimeOffList
        {
            get;
            set;
        }

        [DataMember]
        public bool MSBadgeRequired
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? BadgeStartDate
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? BadgeEndDate
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? ConsultantEndDate
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? RequestDate
        {
            get;
            set;
        }

        [DataMember]
        public bool BadgeException
        {
            get;
            set;
        }

        [DataMember]
        public bool IsApproved
        {
            get;
            set;
        }

        [DataMember]
        public decimal? Discount
        {
            get;
            set;
        }

        [DataMember]
        public bool DiscountLocked
        {
            get;
            set;
        }

        [DataMember]
        public bool IsNewToBudget
        {
            get;
            set;
        }

        [DataMember]
        public int IsDiscountAtResourceLevel {
            //1- milestone level, 2- resource level
            get;
            set;
        }

        #endregion Properties

        #region Constructor

        public MilestonePersonEntry()
        {
        }

        /// <summary>
        /// Init constructor of MilestonePersonEntry.
        /// </summary>
        public MilestonePersonEntry(string milestonePersonId)
            : this(Convert.ToInt32(milestonePersonId))
        {
        }

        /// <summary>
        /// Init constructor of MilestonePersonEntry.
        /// </summary>
        public MilestonePersonEntry(int milestonePersonId)
        {
            MilestonePersonId = milestonePersonId;
        }

        #endregion Constructor

        #region IEquatable<MilestonePersonEntry> Members

        public bool Equals(MilestonePersonEntry other)
        {
            return other != null && MilestonePersonId == other.MilestonePersonId;
        }

        #endregion IEquatable<MilestonePersonEntry> Members

        #region IComparable<MilestonePersonEntry> Members

        public int CompareTo(MilestonePersonEntry other)
        {
            return DateTime.Compare(StartDate, other.StartDate);
        }

        #endregion IComparable<MilestonePersonEntry> Members

        #region ToString

        public string ToString(MilestonePersonEntryFormat format)
        {
            switch (format)
            {
                case MilestonePersonEntryFormat.ProjectMilestoneStartDate:
                    return
                        string.Format(
                            PROJECT_MILESTONE_START_DATE_FORMAT,
                            ParentMilestone.ToString(Milestone.MilestoneFormat.ProjectMilestone),
                            StartDate.ToShortDateString());
            }

            return base.ToString();
        }

        public override string ToString()
        {
            return ToString(MilestonePersonEntryFormat.Default);
        }

        #endregion ToString

        #region "Members used in Milestone Detail Resources tab."

        public bool IsEditMode { get; set; }

        public bool IsShowPlusButton { get; set; }

        public int ShowingPlusButtonEntryId { get; set; }

        public int ExtendedResourcesRowCount { get; set; }

        public bool IsNewEntry { get; set; }

        public bool IsRepeaterEntry { get; set; }

        public bool IsAddButtonEntry { get; set; }

        public Dictionary<string, string> EditedEntryValues { get; set; }

        public MilestonePersonEntry PreviouslySavedEntry { get; set; }

        #endregion "Members used in Milestone Detail Resources tab."
    }
}

