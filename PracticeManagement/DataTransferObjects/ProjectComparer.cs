using System;
using System.Collections;


namespace DataTransferObjects
{
    public class ProjectComparer : IComparer
    {
        #region Private Variables

        private string sortBy = string.Empty;

        #endregion Private Variables

        #region Properties

        public string SortBy
        {
            get
            {
                return sortBy;
            }
            set
            {
                sortBy = value;
            }
        }

        #endregion Properties

        #region Constructor

        public ProjectComparer()
        {
            //default constructor
        }

        public ProjectComparer(string pSortBy)
        {
            sortBy = pSortBy;
        }

        #endregion Constructor

        #region Methods

        public Int32 Compare(Object pFirstObject, Object pObjectToCompare)
        {
            if (!(pFirstObject is Project))
                return 0;
            switch (sortBy)
            {
                case "Account":
                    return String.Compare(((Project)pFirstObject).Client.Name, ((Project)pObjectToCompare).Client.Name);
                case "Project":
                    return String.Compare(((Project)pFirstObject).Name, ((Project)pObjectToCompare).Name);
                case "End Date":
                    return Nullable.Compare(((Project)pFirstObject).EndDate, ((Project)pObjectToCompare).EndDate);
                case "Start Date":
                    return Nullable.Compare(((Project)pFirstObject).StartDate, ((Project)pObjectToCompare).StartDate);
                case "Project #":
                    return string.Compare(((Project)pFirstObject).ProjectNumber,
                                          ((Project)pObjectToCompare).ProjectNumber);

                case "Practice Area":
                    return Practice.Compare(((Project)pFirstObject).Practice,
                                          ((Project)pObjectToCompare).Practice);
                case "Division":
                    return ProjectDivision.Compare(((Project)pFirstObject).Division,
                                          ((Project)pObjectToCompare).Division);

                case "Sales Person":
                    return string.Compare(((Project)pFirstObject).SalesPersonName,
                                          ((Project)pObjectToCompare).SalesPersonName);
                case "Channel":
                    return Channel.Compare(((Project)pFirstObject).Channel,
                                          ((Project)pObjectToCompare).Channel);

                case "Channel-Sub":
                    return string.Compare(((Project)pFirstObject).SubChannel,
                                          ((Project)pObjectToCompare).SubChannel);

                case "Revenue Type":
                    return Revenue.Compare(((Project)pFirstObject).RevenueType,
                                          ((Project)pObjectToCompare).RevenueType);
                case "Offering":
                    return Offering.Compare(((Project)pFirstObject).Offering,
                                          ((Project)pObjectToCompare).Offering);
                case "Status":
                    return string.Compare(((Project)pFirstObject).Status.Name,
                                          ((Project)pObjectToCompare).Status.Name);
                case "New/Extension":
                    return string.Compare(((Project)pFirstObject).BusinessType.ToString(),
                                          ((Project)pObjectToCompare).BusinessType.ToString());
                case "House Account":
                    return string.Compare(((Project)pFirstObject).Client.IsHouseAccount.ToString(),
                                          ((Project)pObjectToCompare).Client.IsHouseAccount.ToString());
                case "Business Group":
                    return string.Compare(((Project)pFirstObject).BusinessGroup.HtmlEncodedName,
                                          ((Project)pObjectToCompare).BusinessGroup.HtmlEncodedName);
                case "Business Unit":
                    return string.Compare(((Project)pFirstObject).Group.HtmlEncodedName,
                                          ((Project)pObjectToCompare).Group.HtmlEncodedName);
                case "Buyer":
                    return string.Compare(((Project)pFirstObject).BuyerName,
                                          ((Project)pObjectToCompare).BuyerName);
                case "Project Manager":
                    return string.Compare(((Project)pFirstObject).ProjectOwner.Name,
                                         ((Project)pObjectToCompare).ProjectOwner.Name);
                case "Engagement Manager":
                    return string.Compare(((Project)pFirstObject).SeniorManagerName,
                                         ((Project)pObjectToCompare).SeniorManagerName);
                case "Executive in Charge":
                    return string.Compare(((Project)pFirstObject).Director.Name,
                                        ((Project)pObjectToCompare).Director.Name);
                case "Pricing List":
                    return string.Compare(((Project)pFirstObject).PricingList.HtmlEncodedName,
                                        ((Project)pObjectToCompare).PricingList.HtmlEncodedName);
                case "PO Number":
                    return string.Compare(((Project)pFirstObject).PONumber,
                                      ((Project)pObjectToCompare).PONumber);
                case "Client Time Entry Required":
                    return string.Compare(((Project)pFirstObject).IsClientTimeEntryRequired.ToString(),
                                      ((Project)pObjectToCompare).IsClientTimeEntryRequired.ToString());
                case "Previous Project Number":
                    return string.Compare(((Project)pFirstObject).PreviousProject != null ? ((Project)pFirstObject).PreviousProject.ProjectNumber : "",
                                          ((Project)pObjectToCompare).PreviousProject != null ? ((Project)pFirstObject).PreviousProject.ProjectNumber : "");
                case "Outsource Id Indicator":
                    return string.Compare(((Project)pFirstObject).OutsourceId.ToString(),
                                        ((Project)pObjectToCompare).OutsourceId.ToString());

                default:
                    return 0;
            }
        }

        #endregion Methods
    }
}

