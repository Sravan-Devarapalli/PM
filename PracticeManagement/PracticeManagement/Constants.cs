﻿
namespace PraticeManagement
{
    /// <summary>
    /// Provides a generally used constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Contains the names of the HTML attributes.
        /// </summary>
        public static class HtmlAttributes
        {
            public const string OnDblClick = "ondblclick";
        }

        public static class ResourceKeys
        {
            # region Report

            public const string StartDateKey = "StartDateKey";
            public const string GranularityKey = "GranularityKey";
            public const string PeriodKey = "PeriodKey";
            public const string ProjectedPersonsKey = "ProjectedPersonsKey";
            public const string ProjectedProjectsKey = "ProjectedProjectsKey";
            public const string ActivePersonsKey = "ActivePersonsKey";
            public const string ActiveProjectsKey = "ActiveProjectsKey";
            public const string ProposedProjectsKey = "ProposedProjectsKey";
            public const string ExperimentalProjectsKey = "ExperimentalProjectsKey";
            public const string TimescaleIdListKey = "TimescaleIdListKey";
            public const string PracticeIdListKey = "PracticeIdListKey";
            public const string ExcludeInternalPracticesKey = "ExcludeInternalPracticesKey";
            public const string InternalProjectsKey = "InternalProjectsKey";
            public const string CompletedProjectsKey = "CompletedProjectsKey";
            public const string SortIdKey = "SortIdKey";
            public const string SortDirectionKey = "SortDirectionKey";
            public const string AvgUtilKey = "AvgUtilKey";
            public const string EndDateKey = "EndDateKey";
            public const string DivisionIdListKey = "DivisionIdListKey";
            public const string ExcludeInvestmentResourceKey = "ExcludeInvestmentResourceKey";

            # endregion Report

            # region SMTP

            public const string MailServerKey = "MailServer";
            public const string PortNumberKey = "PortNumber";
            public const string SSLEnabledKey = "SSLEnabled";
            public const string SMTPAuthRequiredKey = "SMTPAuthRequired";
            public const string UserNameKey = "UserName";
            public const string PasswordKey = "Password";
            public const string PMSupportEmailAddressKey = "PMSupportEmailAddress";

            # endregion SMTP

            # region Application

            public const string TimeZoneKey = "TimeZone";
            public const string IsDayLightSavingsTimeEffectKey = "IsDayLightSavingsTimeEffect";
            public const string IsDefaultMarginInfoEnabledForAllClientsKey = "IsDefaultMarginInfoEnabledForAllClients";
            public const string IsDefaultMarginInfoEnabledForAllPersonsKey = "IsDefaultMarginInfoEnabledForAllPersons";
            public const string OldPasswordCheckCountKey = "OldPasswordCheckCount";
            public const string ChangePasswordTimeSpanLimitInDaysKey = "ChangePasswordTimeSpanLimitInDays";
            public const string FailedPasswordAttemptCountKey = "FailedPasswordAttemptCount";
            public const string PasswordAttemptWindowKey = "PasswordAttemptWindow";
            public const string IsLockOutPolicyEnabledKey = "IsLockOutPolicyEnabled";
            public const string UnlockUserMinituesKey = "UnlockUserMinitues";
            public const string FormsAuthenticationTimeOutKey = "FormsAuthenticationTimeOutMin";
            public const string NotesRequiredForTimeEntryKey = "NotesRequiredForTimeEntry";

            #endregion

            # region PROJECT

            public const string AttachmentFileSize = "AttachmentFileSize";
            public const string DefaultHoursPerDayKey = "DefaultHoursPerDay";

            #endregion


        }

        public static class FilterKeys
        {
            public const string ApplyFilterFromCookieKey = "ApplyFilterFromCookie";
            public const string QueryStringOfApplyFilterFromCookie = "?ApplyFilterFromCookie=true";
            public const string PersonFilterCookie = "PersonFilter";
            public const string ActivityLogFilterCookie = "ActivityLogFilter";
            public const string BenchReportFilterCookie = "BenchReportFilter";
            public const string GenericTimeEntryFilterCookie = "GenericTimeEntryFilter";
            public const string ConsultantUtilTimeLineFilterCookie = "ConsultantUtilTimeLineFilter";
            public const string ConsultingCapacityFilterCookie = "ConsultingCapacityFilter";
            public const string ConsultingDemandFilterCookie = "ConsultingDemandFilter";
            public const string ByAccountReportFitlerCookie = "ByAccountReportFilter";
            public const string Unassigned = "Unassigned";
        }

        /// <summary>
        /// Contains the names of the application's pages
        /// </summary>
        public static class ApplicationPages
        {
            public const string PTOTimelineWithFilterQueryString = "~/Reports/PTOReport.aspx?ApplyFilterFromCookie=true";
            public const string Vendors = "~/Config/Vendors.aspx";
            public const string VendorDetail = "~/Config/VendorDetail.aspx";
            public const string AppRootUrl = "~/";
            public const string LoginPage = "~/Login.aspx";
            public const string DashboardPage = "~/Dashboard.aspx";
            public const string PersonDetail = "~/PersonDetail.aspx";
            public const string StrawManDetail = "~/StrawManDetails.aspx";
            public const string PersonMargin = "~/PersonMargin.aspx";
            public const string ProjectDetail = "~/ProjectDetail.aspx";
            public const string ClientList = "~/Config/Clients.aspx";
            public const string ClientDetails = "~/ClientDetails.aspx";
            public const string ConsultantsUtilizationReport = "~/Reports/UtilizationTable.aspx";
            public const string ConsTimelineReport = "~/Reports/UtilizationTimeline.aspx";
            public const string ConsTimelineReportDetails = "~/Reports/UtilizationTimeline.aspx#details";
            public const string MilestoneDetail = "~/MilestoneDetail.aspx";
            public const string MilestonePersonList = "~/MilestonePersonList.aspx";
            public const string MilestonePersonDetail = "~/MilestonePersonDetail.aspx";
            public const string PersonOverheadCalculation = "~/PersonOverheadCalculation.aspx";
            public const string OverheadDetail = "~/OverheadDetail.aspx";
            public const string Calendar = "~/Calendar.aspx";
            public const string Projects = "~/Projects.aspx";
            public const string CompensationDetail = "~/CompensationDetail.aspx";
            public const string ProjectCSATDetails = "~/ProjectCSATDetails.aspx";
            public const string ExpenseDetail = "~/ExpenseDetail.aspx";
            public const string ExpenseCategoryList = "~/ExpenseCategoryList.aspx";
            public const string OpportunityDetail = "~/OpportunityDetail.aspx";
            public const string OpportunityList = "~/DiscussionReview2.aspx";
            public const string TimeEntryForAdmin = "~/TimeEntry.aspx?day={0}&SelectedPersonId={1}";
            public const string TimeEntry = "~/TimeEntry.aspx?day={0}";
            public const string TimeEntry_NewForAdmin = "~/TimeEntry_New.aspx?day={0}&SelectedPersonId={1}";
            public const string TimeEntry_New = "~/TimeEntry_New.aspx?day={0}";
            public const string DetailsWithPrevNext = "{0}?id={1}&index={2}";
            public const string DetailRedirectFormat = "{0}?id={1}";
            public const string DetailRedirectWithReturnFormat = "{0}?id={1}&returnTo={2}";
            public const string ProjectDetailRedirectWithReturnFormat = "{0}?clientid={1}";
            public const string RedirectPersonIdFormat = "{0}?id={1}&personId={2}";
            public const string RedirectOpportunityIdFormat = "{0}?id={1}&opportunityId={2}";
            public const string RedirectMilestonePersonIdFormat = "{0}?id={1}&milestonePersonId={2}";
            public const string RedirectMilestonePersonIdFormatWithReturn = "{0}?id={1}&milestonePersonId={2}&returnTo={3}";
            public const string RedirectStartDateFormat = "{0}?id={1}&startDate={2}";
            public const string RedirectProjectIdFormat = "{0}?id={1}&projectId={2}";
            public const string RedirectStartDateAndStrawmanFormat = "{0}?id={1}&startDate={2}&Isstrawman={3}";
            public const string MilestonePrevNextRedirectFormat = "{0}?id={1}&projectId={2}"; // &returnTo={3}";
            public const string MilestoneWithReturnFormat = "{0}?id={1}&projectId={2}&returnTo={3}";
            public const string TimeEntryReport = "~/Reports/TimeEntryReport.aspx";
            public const string PageHasBeenRemoved = "~/GuestPages/PageHasBeenRemoved.aspx";
            public const string PageNotFound = "~/GuestPages/PageNotFound.aspx";
            public const string UtilizationTimelineWithDetails = "~/Reports/UtilizationTimeline.aspx#details";
            public const string ClientDetailsWithReturnFormat = "{0}?{1}&Id={2}";
            public const string ClientDetailsWithoutClientIdFormat = "{0}?{1}";
            public const string ChangePasswordErrorpage = "~/GuestPages/ChangePasswordError.aspx";
            public const string ChangePasswordPage = "~/ChangePassword.aspx";
            public const string Set_userPage = "~/set_user.aspx";
            public const string UtilizationTimelineWithFilterQueryStringAndDetails = "~/Reports/UtilizationTimeline.aspx?ApplyFilterFromCookie=true#details";
            public const string UtilizationTimelineWithFilterQueryString = "~/Reports/UtilizationTimeline.aspx?ApplyFilterFromCookie=true";
            public const string OpportunitySummary = "~/DiscussionReview2.aspx";
            public const string AccessDeniedPage = "~/GuestPages/AccessDenied.aspx";
            public const string ConsultingCapacityWithFilterQueryString = "~/Reports/ConsultingCapacity.aspx?ApplyFilterFromCookie=true";
            public const string ConsultingCapacityWithFilterQueryStringAndDetails = "~/Reports/ConsultingCapacity.aspx?ApplyFilterFromCookie=true#details";
            public const string ConsultingCapacityWithDetails = "~/Reports/ConsultingCapacity.aspx#details";
            public const string StrawmanDetails = "~/StrawManDetails.aspx";
            public const string ConsultingDemand = "~/Reports/ConsultingDemand.aspx";
            public const string ConsultingDemandWithFilterQueryString = "~/Reports/ConsultingDemand.aspx?ApplyFilterFromCookie=true";
            public const string TimeEntryReport_new = "~/Reports/TimeEntryReport_new.aspx";
            public const string PersonDetailReport = "~/Reports/PersonDetailTimeReport.aspx";
            public const string TimePeriodSummaryReport = "~/Reports/TimePeriodSummaryReport.aspx";
            public const string ProjectSummaryReport = "~/Reports/ProjectSummaryReport.aspx";
            public const string AccountSummaryReport = "~/Reports/AccountSummaryReport.aspx";
            public const string NewHireReport = "~/Reports/NewHireReport.aspx";
            public const string TerminationReport = "~/Reports/TerminationReport.aspx";
            public const string SkillsEntryPageFormat = "~/SkillsEntry.aspx?Id={0}";
            public const string SkillsEntryPage = "~/SkillsEntry.aspx";
            public const string PersonPictureHandlerFormat = "~/Controls/PersonPicture.ashx?PersonId={0}";
            public const string PersonsPage = "~/Config/Persons.aspx";
            public const string ProjectsListPage = "~/Reports/ProjectsList.aspx";
            public const string ConsultingDemand_New = "~/Reports/ConsultingDemand_new.aspx";
            public const string CSATReport = "~/Reports/CSATReport.aspx";
            public const string SkillsProfile = "~/SkillsProfile.aspx";
            public const string ProjectFeedbackReport = "~/Reports/ProjectFeedbackReport.aspx";
            public const string BillingReport = "~/Reports/BillingReport.aspx";
            public const string RecruitingMetricsReport = "~/Reports/RecruitingMetricsReport.aspx";
            public const string ResourceExceptionReport = "~/Reports/ResourceExceptionReport.aspx";
            public const string PayrollDistributionReport = "~/Reports/PayrollDistributionReport.aspx";
            public const string BadgedOnProjectReport = "~/Reports/Badge/BadgedOnProjectReport.aspx?StartDate={0}&EndDate={1}&PayTypes={2}&PersonStatuses={3}";
            public const string BadgedNotOnProjectReport = "~/Reports/Badge/BadgedNotOnProjectReport.aspx?StartDate={0}&EndDate={1}&PayTypes={2}&PersonStatuses={3}";
            public const string ClockNotStartedReport = "~/Reports/Badge/ClockNotStartedReport.aspx?StartDate={0}&EndDate={1}&PayTypes={2}&PersonStatuses={3}";
            public const string BadgeBlockedReport = "~/Reports/Badge/BadgeBlockedReport.aspx?StartDate={0}&EndDate={1}&PayTypes={2}&PersonStatuses={3}";
            public const string BadgeBreakReport = "~/Reports/Badge/BadgeBreakReport.aspx?StartDate={0}&EndDate={1}&PayTypes={2}&PersonStatuses={3}";
            public const string BadgedResourcesByTimeReport = "~/Reports/Badge/BadgeResourceByTime.aspx";
            public const string BadgedOnProject = "~/Reports/Badge/BadgedOnProjectReport.aspx";
            public const string BadgedOnProjectException = "~/Reports/Badge/BadgedOnProjectBasedExceptionReport.aspx?StartDate={0}&EndDate={1}&PayTypes={2}&PersonStatuses={3}";
            public const string BadgedNotOnProjectException = "~/Reports/Badge/BadgedNotOnPersonBasedExceptionReport.aspx?StartDate={0}&EndDate={1}&PayTypes={2}&PersonStatuses={3}";
        }

        /// <summary>
        /// Contains the names of the application controls pages
        /// </summary>
        public static class ApplicationControls
        {
            public const string ProjectNameCellControl = "~/Controls/ProjectNameCell.ascx";
            public const string ProjectNameCellRoundedControl = "~/Controls/ProjectNameCellRounded.ascx";
        }

        /// <summary>
        /// Contains the names of the application resource files
        /// </summary>
        public static class ApplicationResources
        {
            public const string AddCommentIcon = "~/Images/balloon-plus.png";
            public const string RecentCommentIcon = "~/Images/balloon-ellipsis.png";
        }

        /// <summary>
        /// Contains the names of the report templates.
        /// </summary>
        public static class ReportTemplates
        {
            public const string MonthMiniReport = "~/Reports/Xslt/MonthMiniReport.xslt";
        }

        public static class Dates
        {
            public const int FirstMonth = 1;
            public const int LastMonth = 12;
            public const int FirstDay = 1;
            public const int WorkDaysInMonth = 20;
            public const int DefaultViewableMonths = 2;
            public const string ValidMinDate = "1/1/1975";
            public const string ValidMaxDate = "12/31/2100";
            public const int FYFirstMonth = 1;
            public const int FYLastMonth = 12;
            public const int HistoryDays = 7;
        }

        /// <summary>
        /// Contains the formatting strings
        /// </summary>
        public static class Formatting
        {
            public const string FullMonthYearFormat= "MMMM yyyy";
            public const string DateFormatWithoutDelimiter = "MMddyyyy";
            public const string IntegerNumberFormat = "{0:##############0}";
            public const string CurrencyZero = "$0.00";
            public const string MonthYearFormat = "MMM yyyy";
            public const string CompPerfMonthYearFormat = "MMM-yy";
            public const string SystemCurrencyFormat = "c";
            public const string TwoDecimalsFormat = "#######0.##";
            public const string PercentageFormat = "{0:##0.0#}%";
            public const string SearchResultFormat = "<span class=\"found\">$0</span>";
            public const string EntryDateFormat = "MM/dd/yyyy";
            public const string DateTimeFormat = "MM/dd/yyyy HH:mm:ss"; 
            public const string SortingDateFormat = "yyyyMMdd";
            public const string DoubleFormat = "F2";
            public const string CurrentVersionFormat = "Binaries: v{0}.{1}.{2}.{3} [{4:%y-MM-dd}] | Database: {5}";
            public const string Ellipsis = "...";
            public const string UnknownValue = "?";
            public const string GreetingUserName = "{0} {1}";
            public const string DoubleValueWithZeroPadding = "00.00";
            public const string DoubleValue = "0.00";
            public const string DoubleValueWithPercent = "0.0%";
            public const string CurrencyFormat = "$###,###,###,###,###,###,###";
            public const string ReportDateFormat = "MM/dd/yyyy (dddd)";
            public const string CurrencyExcelReportFormat = "$####,###,###,###,###,##0.00";
            public const string CurrencyExcelReportFormatWithoutDecimal = "$####,###,###,###,###,##0";
            public const string NumberFormatWithCommas = "###,###,###,###,###,###,###0";
            public const string NumberFormatWithCommasAndDecimals = "###,###,###,###,###,###,###0.00";
            public const string DoubleValueWithSinglePrecision = "0.0";
        }

        public static class HttpHeaders
        {
            public const string CacheControlNoCache = "no-cache";
        }

        /// <summary>
        /// Activity log XML strings
        /// </summary>
        public static class ActityLog
        {
            public const string ErrorLogMessage = @"<Error><NEW_VALUES	Login = ""{0}"" SourcePage = ""{1}"" SourceQuery = ""{2}""  ExcMsg=""{3}"" ExcSrc=""{4}"" InnerExcMsg=""{5}"" InnerExcSrc=""{6}""><OLD_VALUES /></NEW_VALUES></Error>";
            public const string ExportLogMessage = @"<Export><NEW_VALUES User = ""{0}"" From=""{1}""></NEW_VALUES><OLD_VALUES /></Export>";

            public const int ErrorMessageId = 20;
            public const int TaskPerformedMessageId = 6;
        }

        public static class QueryStringParameterNames
        {
            public const string Id = "id";
            public const string ReturnUrl = "returnTo";
            public const string RedirectUrlArgument = "redirectUrl";
            public const string QueryStringSeparator = "?";
            public const string RedirectFormat = "{0}?returnTo={1}";
            public const string RedirectWithQueryStringFormat = "{0}&returnTo={1}";
            public const string ActiveOnly = "activeOnly";
            public const string ClientId = "clientId";
            public const string SalesId = "salesId";
            public const string Index = "index";
            public const string SortOrder = "sortOrder";
            public const string SortDirection = "sortDirection";
            public const string RangeArgument = "Range";
            public const string ViewArgument = "View";
            public const string StartDateArgument = "StartDate";
            public const string EndDateArgument = "EndDate";
            public const string IncludeAllArgument = "IncludeAll";
            public const string ProjectNumberArgument = "ProjectNumber";

        }

        public static class MethodParameterNames
        {
            public const string ID = "id";
            public const string MILESTONE_PERSON_ID = "milestonePersonId";
            public const string MODIFIED_BY_ID = "modifiedById";
            public const string PERSON_ID = "personId";
            public const string REQUESTER_ID = "requesterId";
            public const string SORT_EXPRESSION = "sortExpression";
            public const string TIME_TYPE_ID = "timeTypeId";
            public const string USER_NAME = "userName";
        }

        public static class ControlNames
        {
            public const string DDL_PROJECT_MILESTONES_EDIT = "ddlProjectMilestonesEdit";
            public const string DDL_TIMETYPE_EDIT = "ddlTimeTypeEdit";
            public const string HF_PERSON = "hfPerson";
        }

        public static class CssClassNames
        {
            public const string DIMMED_ROW = "declined-row";
        }

        public static class EntityNames
        {
            public const string IsChargeableEntity = "IsChargeable";
            public const string IsCorrectEntity = "IsCorrect";
            public const string ReviewStatusEntity = "ReviewStatus";
        }

        public static class Scripts
        {
            public const string CheckDirtyWithPostback = "if (showDialod()) {{{0}; return false; }} if(checkhdnchbActive())return true; return false;";
            public const string GoBack = "history.back();return false;";
        }

        public static class Variables
        {
            public const string IsStrawMan = "IsStrawMan";
            public const string OptionGroup = "OptionGroup";
            public const string HasPermissionToEditCalender = "HasPermissionToEditCalender";

        }

        public static class OpportunityPriorityIds
        {
            public const int PriorityIdOfPO = 5;
            public const int PriorityIdOfA = 1;
            public const int PriorityIdOfB = 2;
        }

        public static class ConsultingDemandSortColumnNames
        {
            public const string Title = "Title";
            public const string Skill = "Skill";
            public const string OpportunityNumber = "OpportunityNumber";
            public const string ProjectNumber = "ProjectNumber";
            public const string AccountName = "AccountName";
            public const string ProjectName = "ProjectName";
            public const string ResourceStartDate = "ResourceStartDate";
            public const string SortDescendingOrder = "Desc";
            public const string MonthStartDate = "MonthStartDate";
            public const string Count = "Count";
            public const string SalesStage = "SalesStage";
        }
    }
}

