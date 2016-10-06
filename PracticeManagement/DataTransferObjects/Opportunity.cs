﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Web;

namespace DataTransferObjects
{
    /// <summary>
    /// Represents the opportunity.
    /// </summary>
    [Serializable]
    [DataContract]
    [DebuggerDisplay("Opportunity: Name = {Name}")]
    public class Opportunity
    {
        #region Constants

        private const string ClientAndGroupFormat = "{0} - {1}";
        private const string ClientOpportunityFormat = "{0} - {1} - {2}";

        #endregion Constants

        #region Properties

        /// <summary>
        /// Gets or sets an ID of the Opportunity.
        /// </summary>
        [DataMember]
        public int? Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an ProjectId of the related Project.
        /// </summary>
        [DataMember]
        public Project Project
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an EstimatedRevenue of the Opportunity.
        /// </summary>
        [DataMember]
        public Decimal? EstimatedRevenue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an OpportunityIndex of the Opportunity.
        /// </summary>
        [DataMember]
        public int? OpportunityIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an Opportunity Name.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        public string HtmlEncodedName
        {
            get
            {
                return HttpUtility.HtmlEncode(Name);
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="Client"/>.
        /// </summary>
        [DataMember]
        public Client Client
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a <see cref="ProjectGroup"/>.
        /// </summary>
        [DataMember]
        public ProjectGroup Group
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a Salesperson for the Opportunity.
        /// </summary>
        [DataMember]
        public Person Salesperson
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an Opportunity Status.
        /// </summary>
        [DataMember]
        public OpportunityStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// Owner
        /// </summary>
        [DataMember]
        public Person Owner
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an Opportunity Priority.
        /// </summary>
        [DataMember]
        public OpportunityPriority Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a projected start date.
        /// </summary>
        [DataMember]
        public DateTime? ProjectedStartDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a projected start date.
        /// </summary>
        [DataMember]
        public DateTime LastUpdate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a projected end date.
        /// </summary>
        [DataMember]
        public DateTime? ProjectedEndDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a close date.
        /// </summary>
        [DataMember]
        public DateTime? CloseDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an Opportunity Number.
        /// </summary>
        [DataMember]
        public string OpportunityNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a description for the Opportunity.
        /// </summary>
        [DataMember]
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a projected <see cref="Practice"/>.
        /// </summary>
        [DataMember]
        public Practice Practice
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a Buyer Name.
        /// </summary>
        [DataMember]
        public string BuyerName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a date when the opportunity was created.
        /// </summary>
        [DataMember]
        public DateTime CreateDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a pipeline.
        /// </summary>
        [DataMember]
        public string Pipeline
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a proposed.
        /// </summary>
        [DataMember]
        public string Proposed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a send-out.
        /// </summary>
        [DataMember]
        public string SendOut
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a revenue type.
        /// </summary>
        [DataMember]
        public RevenueType OpportunityRevenueType
        {
            get;
            set;
        }

        public Double DaysOld
        {
            get
            {
                return DateTime.Now.Subtract(CreateDate).TotalMilliseconds;
            }
        }

        public Double LastChange
        {
            get
            {
                return DateTime.Now.Subtract(LastUpdate).TotalMilliseconds;
            }
        }

        [DataMember]
        public string ProposedPersonIdList
        {
            get;
            set;
        }

        [DataMember]
        public string StrawManList
        {
            get;
            set;
        }

        [DataMember]
        public List<OpportunityPerson> ProposedPersons
        {
            set;
            get;
        }

        [DataMember]
        public string OutSideResources
        {
            get;
            set;
        }

        [DataMember]
        public PricingList PricingList
        {
            get;
            set;
        }

        [DataMember]
        public BusinessType BusinessType { get; set; }

        #endregion Properties

        #region Formatting

        public string ClientAndGroup
        {
            get
            {
                return Group == null || Group.Name.Equals(ProjectGroup.DefaultGroupName, StringComparison.CurrentCultureIgnoreCase) ?
                    Client.Name :
                    string.Format(ClientAndGroupFormat, Client.Name, Group.Name);
            }
        }

        public string HtmlEncodedClientAndGroup
        {
            get
            {
                return HttpUtility.HtmlEncode(ClientAndGroup);
            }
        }

        public string ClientOpportunity
        {
            get
            {
                return string.Format(ClientOpportunityFormat, Client.Name, OpportunityNumber, Name);
            }
        }

        #endregion Formatting
    }
}
