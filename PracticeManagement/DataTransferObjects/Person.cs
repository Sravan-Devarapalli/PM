﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web;
using DataTransferObjects.Skills;
using DataTransferObjects.CornerStone;

namespace DataTransferObjects
{
    /// <summary>
    /// Data Transfer Object for a Person entity
    /// </summary>
    [DataContract]
    [Serializable]
    public class Person : IEquatable<Person>, IIdNameObject, IComparable<Person>
    {
        #region Constants

        public const string RecruitingOverheadName = "Recruiting";
        public const string PersonNameFormat = "{0}, {1}";

        #endregion Constants

        #region Fields

        private DateTime? _terminationDate;
        private string _alias;
        private Practice _defaultPractice;
        private Pay _currentPay;

        #endregion Fields

        #region Properties - data members

        /// <summary>
        /// Identifies the person in the system
        /// </summary>
        /// <remarks>
        /// This value can be null if the person has not been entered in the system.  If
        /// a Person is saved with a <value>null</value> Id the system should assign a
        /// new id and and change the Id value to that new id.
        /// </remarks>
        [DataMember]
        public int? Id
        {
            get;
            set;
        }

        [DataMember]
        public bool IsWelcomeEmailSent
        {
            get;
            set;
        }

        [DataMember]
        public bool SLTApproval
        {
            set;
            get;
        }

        [DataMember]
        public bool SLTPTOApproval { get; set; }

        public string Name
        {
            get { return PersonLastFirstName; }
            set { throw new NotImplementedException("Unable to set person name"); }
        }

        public string HtmlEncodedName
        {
            get
            {
                return HttpUtility.HtmlEncode(Name);
            }
        }

        /// <summary>
        /// First name of the person, should be able to identify the person in the system along with the LastName
        /// </summary>
        [DataMember]
        public string FirstName
        {
            get;
            set;
        }

        /// <summary>
        /// Last name of the person, should be able to identify the person in the system along with the FirstName
        /// </summary>
        [DataMember]
        public string LastName
        {
            get;
            set;
        }

        [DataMember]
        public string PrefferedFirstName
        {
            get;
            set;
        }

        /// <summary>
        /// Person status
        /// </summary>
        [DataMember]
        public PersonStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// Person Locked-Out value
        /// </summary>
        [DataMember]
        public bool LockedOut
        {
            get;
            set;
        }

        /// <summary>
        /// Person admin or not
        /// </summary>
        [DataMember]
        public bool IsAdmin
        {
            get;
            set;
        }

        /// <summary>
        /// Date of hire
        /// </summary>
        [DataMember]
        public DateTime HireDate
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? FirstHireDate
        {
            get;
            set;
        }

        [DataMember]
        public List<Employment> EmploymentHistory
        {
            get;
            set;
        }

        /// <summary>
        /// Employee number
        /// </summary>
        [DataMember]
        public string EmployeeNumber
        {
            get;
            set;
        }

        [DataMember]
        public string ProjectRoleName
        {
            get;
            set;
        }

        /// <summary>
        /// Person telephone number.
        /// </summary>
        /// <remarks>
        /// <value>null</value> indicates the person as no phone
        /// </remarks>
        [DataMember]
        public string TelephoneNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Date person no longer works for firm.
        /// </summary>
        /// <remarks>
        /// <value>null</value> indicates the person is still employed
        /// </remarks>
        [DataMember]
        public DateTime? TerminationDate
        {
            get { return _terminationDate; }
            set { _terminationDate = value; }
        }

        [DataMember]
        public DateTime? LastTerminationDate
        {
            get;
            set;
        }

        [DataMember]
        public string ImageUrl
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? ModifiedDate
        {
            get;
            set;
        }

        /// <summary>
        /// email address of the person, should be able to identify the person
        /// </summary>
        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        public string AliasWithoutDomain
        {
            get
            {
                return !string.IsNullOrEmpty(Alias) ? Alias.Split('@')[0] : string.Empty;
            }
        }

        public string Domain
        {
            get
            {
                return !string.IsNullOrEmpty(Alias) ? Alias.Split('@')[1].ToLower() : string.Empty;
            }
        }

        [DataMember]
        public bool IsDefaultManager
        {
            get;
            set;
        }

        [DataMember]
        public Practice DefaultPractice
        {
            get { return _defaultPractice; }
            set { _defaultPractice = value; }
        }

        /// <summary>
        /// Gets or sets the recruiterId of the <see cref="Person"/>.
        /// </summary>
        [DataMember]
        public int? RecruiterId
        {
            get;
            set;
        }

        [DataMember]
        public string RecruiterFirstName
        {
            get;
            set;
        }

        [DataMember]
        public string RecruiterLastName
        {
            get;
            set;
        }

        public string RecruiterLastFirstName
        {
            get
            {
                return RecruiterId.HasValue ? string.Format(PersonNameFormat, RecruiterLastName, RecruiterFirstName) : string.Empty;
            }
        }

        [DataMember]
        public int? EmployeeReferralId
        {
            get;
            set;
        }

        [DataMember]
        public string EmployeeReferralFirstName
        {
            get;
            set;
        }

        [DataMember]
        public string EmployeeReferralLastName
        {
            get;
            set;
        }

        public string EmployeeReferralLastFirstName
        {
            get
            {
                return EmployeeReferralId.HasValue ? string.Format(PersonNameFormat, EmployeeReferralLastName, EmployeeReferralFirstName) : string.Empty;
            }
        }

        [DataMember]
        public Pay CurrentPay
        {
            get { return _currentPay; }
            set { _currentPay = value; }
        }

        /// <summary>
        /// Gets or sets a list of the <see cref="Pay"/> objects to determine the payment history for the person.
        /// </summary>
        [DataMember]
        public List<Pay> PaymentHistory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of overheads for the given person.
        /// </summary>
        [DataMember]
        public List<PersonOverhead> OverheadList
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of the projected financial indicators for the interest period.
        /// </summary>
        [DataMember]
        public Dictionary<DateTime, ComputedFinancials> ProjectedFinancialsByMonth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of roles the person is assigned to.
        /// </summary>
        [DataMember]
        public string[] RoleNames
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a person's seniority.
        /// </summary>
        [DataMember]
        public Seniority Seniority
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a person's Title.
        /// </summary>
        [DataMember]
        public Title Title
        {
            set;
            get;
        }

        /// <summary>
        /// List of practices owned by this person
        /// </summary>
        [DataMember]
        public List<Practice> PracticesOwned { get; set; }

        /// <summary>
        /// Gets or sets a person's manager (see #1419).
        /// </summary>
        [DataMember]
        public Person Manager { get; set; }

        [DataMember]
        public string ManagerName { get; set; }

        [DataMember]
        public bool IsInvestmentResource { get; set; }

        [DataMember]
        public int? TargetUtilization { get; set; }

        [DataMember]
        public List<PersonIndustry> Industries
        {
            get;
            set;
        }

        [DataMember]
        public List<PersonDocument> Documents
        {
            get;
            set;
        }

        [DataMember]
        public List<PersonContactInfo> ContactInfoList
        {
            get;
            set;
        }

        [DataMember]
        public List<PersonQualification> Qualifications
        {
            get;
            set;
        }

        [DataMember]
        public List<PersonSkill> Skills
        {
            get;
            set;
        }

        [DataMember]
        public List<PersonTraining> Trainings
        {
            get;
            set;
        }

        [DataMember]
        public List<PersonEmployer> Employers
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? LastLogin { get; set; }

        [DataMember]
        public bool IsStrawMan { get; set; }

        [DataMember]
        public double BillableUtilizationPercent { get; set; }

        [DataMember]
        public bool IsOffshore { get; set; }

        [DataMember]
        public string PaychexID { get; set; }

        [DataMember]
        public PersonDivisionType DivisionType { get; set; }

        [DataMember]
        public PersonDivision Division { get; set; }

        public string OffshoreText
        {
            get
            {
                return IsOffshore ? "Offshore" : "Domestic";
            }
        }

        //Is person assigned to any project or opportuninty for past,present and future purposes
        [DataMember]
        public bool InUse { get; set; }

        [DataMember]
        public int? TerminationReasonid { get; set; }

        [DataMember]
        public string TerminationReason { get; set; }

        [DataMember]
        public List<Profile> Profiles
        {
            get;
            set;
        }

        [DataMember]
        public MSBadge Badge
        {
            get;
            set;
        }

        [DataMember]
        public List<MSBadge> BadgedProjects
        {
            get;
            set;
        }

        [DataMember]
        public bool IsHighlighted { get; set; }

        [DataMember]
        public byte[] PictureData { get; set; }

        /// <summary>
        /// Person has picture or not.
        /// </summary>
        [DataMember]
        public bool HasPicture { get; set; }

        /// <summary>
        /// Person is terminated due to pay or not.Using in worker role
        /// </summary>
        [DataMember]
        public bool IsTerminatedDueToPay { get; set; }

        [DataMember]
        public RecruitingMetrics SourceRecruitingMetrics { get; set; }

        [DataMember]
        public RecruitingMetrics TargetedCompanyRecruitingMetrics { get; set; }

        [DataMember]
        public int? JobSeekersStatusId { get; set; }

        [DataMember]
        public Person EmployeeRefereral { get; set; }

        [DataMember]
        public CohortAssignment CohortAssignment { get; set; }

        [DataMember]
        public int LengthOfTenture { get; set; }

        [DataMember]
        public List<PersonTimeOff> TimeOffHistory { get; set; }

        [DataMember]
        public PersonTimeOff TimeOff { get; set; }

        [DataMember]
        public Location Location { get; set; }

        [DataMember]
        public DivisionCF CFDivision { get; set; }

        [DataMember]
        public List<ProjectFeedback> ProjectFeedbacks { get; set; }

        [DataMember]
        public List<ProjectFeedback> ErrorFeedbacks { get; set; }

        [DataMember]
        public bool IsMBO { get; set; }

        [DataMember]
        public Person PracticeLeadership { get; set; }

        [DataMember]
        public int PracticeDirectorId { get; set; }

        [DataMember]
        public string PracticeDirectorEmployeeNumber { get; set; }

        [DataMember]
        public List<Project> Projects { get; set; }

        [DataMember]
        public DateTime ResourceStartDate { get; set; }

        [DataMember]
        public DateTime ResourceEndDate { get; set; }

        public string FormattedName
        {
            get
            {
                if (Id.HasValue)
                    return HtmlEncodedName;
                return string.Empty;
            }
        }

        public string FormatName
        {
            get
            {
                if (Id.HasValue && Id != -1)
                    return HtmlEncodedName;
                return "Unassigned";
            }
        }

        #endregion Properties - data members

        #region Properties - calculated

        /// <summary>
        /// Gets a person's Raw Hourly Rate
        /// </summary>
        public PracticeManagementCurrency RawHourlyRate
        {
            get
            {
                return CurrentPay != null ? CurrentPay.AmountHourly : 0;
            }
        }

        /// <summary>
        /// Gets a person's Total Overhead
        /// </summary>
        /// <remarks>The OverheadList property must be correctly set before calculating.</remarks>
        public PracticeManagementCurrency TotalOverhead
        {
            get
            {
                decimal result = 0;
                if (OverheadList != null)
                {
                    foreach (PersonOverhead overhead in OverheadList.FindAll(OH => !OH.IsMLF))
                    {
                        result += overhead.HourlyValue;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a person's Loaded hourly rate.
        /// </summary>
        /// <remarks>The OverheadList property must be correctly set before calculating.</remarks>
        public PracticeManagementCurrency LoadedHourlyRate
        {
            get
            {
                return RawHourlyRate + TotalOverhead;
            }
        }

        /// <summary>
        /// Substitution for ToString
        /// Used where it's not allowed to use methods, but properties
        /// </summary>
        public string PersonLastFirstName
        {
            get
            {
                return ToString();
            }
        }

        public string PersonFirstLastName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public string DefaultCareerCounselour
        {
            get
            {
                return Manager != null ? Manager.PersonLastFirstName : "Unassigned";
            }
        }

        public string PreferredOrFirstName
        {
            get
            {
                return string.IsNullOrEmpty(PrefferedFirstName) ? Name : LastName + ", " + PrefferedFirstName;
            }
        }
        public JobSeekersStatus JobSeekersStatus
        {
            get
            {
                if (JobSeekersStatusId != null)
                    return (JobSeekersStatus)JobSeekersStatusId;
                return JobSeekersStatus.Undefined;
            }
        }

        #endregion Properties - calculated

        #region Constructors

        /// <summary>
        /// Init constructor of Person.
        /// </summary>
        public Person(int? id)
        {
            Id = id;
        }

        /// <summary>
        /// Init constructor of Person.
        /// </summary>
        public Person()
        {
        }

        #endregion Constructors

        #region Methods

        public bool Equals(Person other)
        {
            if (other == null)
                return false;

            if (Id.HasValue && other.Id.HasValue)
                return Id.Value == other.Id.Value;

            return false;
        }

        public override bool Equals(Object other)
        {
            return Equals((Person)other);
        }

        public int CompareTo(Person other)
        {
            return other == null ? 1 : PersonLastFirstName.CompareTo(other.PersonLastFirstName);
        }

        public override string ToString()
        {
            return string.Format(
                PersonNameFormat,
                LastName, FirstName);
        }

        public override int GetHashCode()
        {
            return Id.HasValue ? Id.Value : base.GetHashCode();
        }

        #endregion Methods
    }
}

