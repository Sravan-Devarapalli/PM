﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using DataTransferObjects;
using PraticeManagement.Configuration;
using PraticeManagement.FilterObjects;
using PraticeManagement.Utils;
using DataTransferObjects.Filters;

namespace PraticeManagement.Controls
{
    public partial class ActivityLogControl : UserControl
    {
        #region Constants

        private const string ViewstateDisplayVisible = "DISPLAY_VISIBLE";
        private const string ProjectDisplayViewstate = "DISPLAY_PROJECT";
        private const string PersonDisplayViewstate = "DISPLAY_PERSON";
        private const string ViewstateEventSource = "EVENT_SOURCE";
        private const string OpportunityIdViewstate = "OPPORTUNITY_ID";
        private const string MilestoneIdViewstate = "MILESTONE_ID";
        private const string ModifiedByNameAttribute = "ModifiedByName";
        private const string UserNameAttribute = "UserName";
        private const string UserAttribute = "User";

        private const string LoggedInActivity = "logged in";
        private const string NewValuesTag = "new_values";
        private const string LoginTag = "login";

        #endregion

        #region Titles

        //  Provides mapping between event sources and drop-down item titles
        private static readonly Dictionary<ActivityEventSource, string> EventSourceTitles =
            new Dictionary<ActivityEventSource, string>();

        //  Provides mapping between even sources and target drop-downs to set value at
        private static readonly Dictionary<ActivityEventSource, DropDownList> ControlMapping =
            new Dictionary<ActivityEventSource, DropDownList>();

        private string _currentUrl;
        private XsltArgumentList _argumentList;

        static ActivityLogControl()
        {
        }

        public string LabelTextBeforeDropDown
        {
            get { return Label3.Text; }
            set { Label3.Text = value; }
        }



        /// <summary>
        /// 	Default constructor of ActivityLogControl.
        /// </summary>
        public ActivityLogControl()
        {
            ControlMapping.Clear();
            ControlMapping.Add(ActivityEventSource.Project, ddlProjects);
            ControlMapping.Add(ActivityEventSource.Person, ddlPersonName);
        }

        protected void InitXsltParams()
        {
            _currentUrl = HttpUtility.UrlEncode(Request.Url.AbsoluteUri) + (IsActivityLogPage ? (Request.Url.Query.Length > 0 ? string.Empty : Constants.FilterKeys.QueryStringOfApplyFilterFromCookie) : string.Empty);

            _argumentList = new XsltArgumentList();
            _argumentList.AddParam("currentUrl", string.Empty, _currentUrl);
        }

        #endregion

        #region Properties

        /// <summary>
        /// 	Defines whether to show Display drop-down or not
        /// </summary>
        public bool ShowDisplayDropDown
        {
            get
            {
                var obj = ViewState[ViewstateDisplayVisible];

                if (obj != null)
                    return (bool)obj;

                return true;
            }

            set { ViewState[ViewstateDisplayVisible] = value; }
        }

        /// <summary>
        /// 	Defines whether to show Project drop-down or not
        /// </summary>
        public bool ShowProjectDropDown
        {
            get
            {
                var obj = ViewState[ProjectDisplayViewstate];

                if (obj != null)
                    return (bool)obj;

                return true;
            }

            set { ViewState[ProjectDisplayViewstate] = value; }
        }

        /// <summary>
        /// 	Defines whether to show Project drop-down or not
        /// </summary>
        public bool ShowPersonDropDown
        {
            get
            {
                var obj = ViewState[PersonDisplayViewstate];

                if (obj != null)
                    return (bool)obj;

                return true;
            }

            set { ViewState[PersonDisplayViewstate] = value; }
        }

        /// <summary>
        /// 	Defines whether to show Display drop-down or not
        /// </summary>
        public int? OpportunityId
        {
            get
            {
                var obj = ViewState[OpportunityIdViewstate];

                if (obj != null)
                    return (int?)obj;

                return null;
            }

            set { ViewState[OpportunityIdViewstate] = value; }
        }

        public int? MilestoneId
        {
            get
            {
                var obj = ViewState[MilestoneIdViewstate];

                if (obj != null)
                    return (int?)obj;

                return null;
            }

            set { ViewState[MilestoneIdViewstate] = value; }
        }
        /// <summary>
        /// 	Sets Display drop-down value
        /// </summary>
        public ActivityEventSource DisplayDropDownValue
        {
            get
            {
                var obj = ViewState[ViewstateEventSource];

                if (obj != null)
                    return (ActivityEventSource)obj;

                return ActivityEventSource.All;
            }
            set
            {
                ViewState[ViewstateEventSource] = value;
            }
        }

        public bool IsReset { get; set; }

        public bool IsFreshRequest { get; set; }

        public bool IsActivityLogPage { get; set; }

        public bool ValidationSummaryEnabled
        {
            get
            {
                return valSum.Enabled;
            }
            set
            {
                valSum.Enabled = value;
            }
        }

        private PraticeManagement.Config.ActivityLog HostingPage
        {
            get { return ((PraticeManagement.Config.ActivityLog)Page); }
        }

        private PraticeManagement.Config.VendorDetail VendorDetailPage
        {
            get
            {
                if (Page is PraticeManagement.Config.VendorDetail)
                {
                    return ((PraticeManagement.Config.VendorDetail)Page);
                }
                else
                {
                    return null;
                }
            }
        }

        private string VendorId
        {
            get
            {
                if (VendorDetailPage != null && VendorDetailPage.VendorId != null)
                {
                    return VendorDetailPage.VendorId.Value.ToString();
                }
                else
                {
                    return null;
                }
            }

        }

        #endregion

        public DateTime? FromDateFilterValue
        {
            get { return diRange.FromDate; }
            set { diRange.FromDate = value; }
        }

        public DateTime? ToDateFilterValue
        {
            get { return diRange.ToDate; }
            set { diRange.ToDate = value; }
        }

        public bool IsFiltersReadingFromCookie { get; set; }


        private static string GetStringByValue(ActivityEventSource value)
        {
            //return Enum.GetName(typeof(ActivityEventSource), value);
            int val = (int)value;
            return val.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack || IsFreshRequest)
            {
                if (IsActivityLogPage)
                {
                    var cookie = SerializationHelper.DeserializeCookie(Constants.FilterKeys.ActivityLogFilterCookie) as ActivityLogFilter;
                    if (Request.QueryString[Constants.FilterKeys.ApplyFilterFromCookieKey] == "true" && cookie != null)
                    {
                        if (cookie.FiltersChanged)
                        {
                            IsFiltersReadingFromCookie = true;
                            DisplayDropDownValue = (ActivityEventSource)Enum.Parse(typeof(ActivityEventSource), cookie.EventSourceSelected);
                            FromDateFilterValue = cookie.FromDateFilterValue;
                            ToDateFilterValue = cookie.ToDateFilterValue;
                            ddlPeriod.SelectedValue = cookie.PeriodSelected.ToString();

                            if (!string.IsNullOrEmpty(cookie.ProjectSelected))
                                ddlProjects.SelectedValue = cookie.ProjectSelected.Trim();
                            if (!string.IsNullOrEmpty(cookie.PersonSelected))
                                ddlPersonName.SelectedValue = cookie.PersonSelected.Trim();

                            hdnResetFilter.Value = cookie.FiltersChanged.ToString();
                            btnResetFilter.Enabled = cookie.FiltersChanged;
                        }
                        gvActivities.PageIndex = cookie.CurrentIndex;
                    }
                    spnProjects.Disabled = false;
                }

                FillEventList();

                ddlProjects.DataBind();
                ddlPersonName.DataBind();

                if (!IsActivityLogPage)
                {
                    var display = ShowDisplayDropDown;
                    ddlEventSource.Visible = display;
                    lblDisplay.Visible = display;
                    spnProjects.Visible = ShowProjectDropDown;
                    spnPersons.Visible = ShowPersonDropDown;
                }
            }

            InitXsltParams();

            if (!IsPostBack)
            {
                ResetFilters();
            }


            if (IsActivityLogPage)
            {
                tblActivitylog.Attributes["class"] = "CompPerfTable WholeWidth no-wrap";
                ddlEventSource.Width = Unit.Percentage(87);
                ddlPersonName.Width = Unit.Percentage(93);
                ddlProjects.Width = Unit.Percentage(90);
                tdEventSource.Attributes["class"] = "padLeft0 padRight0 Width23Percent";
                tdYear.Attributes["class"] = "padLeft2 Width12Percent";
                spnPersons.Attributes["class"] = "Width23Percent";
                spnProjects.Attributes["class"] = "Width24Percent";
                tdBtnList.Attributes["class"] = "Width18Percent";
                tdBtnList.Align = "right";
                divActivitylog.Attributes["class"] = "Padding10 bgColor_d4dff8";
                btnResetFilter.Visible = true;
            }
            else
            {
                ddlEventSource.Width = Unit.Pixel(100);
                ddlPersonName.Width = Unit.Pixel(150);
                ddlProjects.Width = Unit.Pixel(150);
            }
        }

        private void ResetFilters()
        {
            if (!IsFiltersReadingFromCookie)
            {
                SetPeriodSelection(-7);
            }

            if (IsActivityLogPage)
            {
                if (!IsFiltersReadingFromCookie)
                {
                    ddlPeriod.SelectedValue = "-7";
                    ddlEventSource.SelectedIndex = ddlPersonName.SelectedIndex = ddlProjects.SelectedIndex = 0;
                    hdnResetFilter.Value = "false";
                    btnResetFilter.Enabled = false;
                }
                ActivityLogOnChangeEvents();
            }
            else
            {
                ddlPeriod.Attributes["onchange"] = "CheckAndShowCustomDatesPoup(this);";
            }
        }

        private void ActivityLogOnChangeEvents()
        {
            ddlPeriod.Attributes["onchange"] = "EnableResetButton(); CheckAndShowCustomDatesPoup(this);";
            ddlEventSource.Attributes["onchange"] = "disableProjectsDropDown(); EnableResetButton();";
            ddlPersonName.Attributes["onchange"] = ddlProjects.Attributes["onchange"] = "EnableResetButton();";
            diRange.OnClientChange = "EnableResetButtonForDateIntervalChange";
        }

        protected void Page_Prerender(object sender, EventArgs e)
        {

            if (IsActivityLogPage)
            {
                ActivityLogOnChangeEvents();
                EnableProjectsDropDown();
                if (!IsReset)
                {
                    GetFilterValuesForSession();
                }
            }
            else
            {
                ddlPeriod.Attributes["onchange"] = "CheckAndShowCustomDatesPoup(this);";
            }

            if (!Request.Url.AbsolutePath.Contains("PersonDetail.aspx"))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetTooltipsForallDropDowns", "SetTooltipsForallDropDowns();ModifyInnerTextToWrapText();", true);
            }

            if (IsFreshRequest)
            {
                Update();
            }

            lblCustomDateRange.Text = string.Format("({0}&nbsp;-&nbsp;{1})",
                     diRange.FromDate.Value.ToString(Constants.Formatting.EntryDateFormat),
                     diRange.ToDate.Value.ToString(Constants.Formatting.EntryDateFormat)
                     );
            if (ddlPeriod.SelectedValue == "0")
            {
                lblCustomDateRange.Attributes.Add("class", "fontBold");
                imgCalender.Attributes.Add("class", "");
            }
            else
            {
                lblCustomDateRange.Attributes.Add("class", "displayNone");
                imgCalender.Attributes.Add("class", "displayNone");
            }
            hdnStartDate.Value = diRange.FromDate.Value.ToString(Constants.Formatting.EntryDateFormat);
            hdnEndDate.Value = diRange.ToDate.Value.ToString(Constants.Formatting.EntryDateFormat);
            var clFromDate = diRange.FindControl("clFromDate") as AjaxControlToolkit.CalendarExtender;
            var clToDate = diRange.FindControl("clToDate") as AjaxControlToolkit.CalendarExtender;
            hdnStartDateCalExtenderBehaviourId.Value = clFromDate.BehaviorID;
            hdnEndDateCalExtenderBehaviourId.Value = clToDate.BehaviorID;
        }

        private void EnableProjectsDropDown()
        {
            int eventSourceValue = Convert.ToInt32(ddlEventSource.SelectedValue);
            ddlProjects.Enabled = !((eventSourceValue >= 3 && eventSourceValue <= 5) || (eventSourceValue >= 32 && eventSourceValue <= 42) || (eventSourceValue >= 21 && eventSourceValue <= 27));
        }

        private void SaveFilterSettings()
        {
            ActivityLogFilter filter = GetFilterSettings();
            SerializationHelper.SerializeCookie(filter, Constants.FilterKeys.ActivityLogFilterCookie);
        }

        private ActivityLogFilter GetFilterSettings()
        {
            var filter = new ActivityLogFilter
            {
                EventSourceSelected = ddlEventSource.SelectedValue,
                FromDateFilterValue = FromDateFilterValue,
                ToDateFilterValue = ToDateFilterValue,
                PersonSelected = ddlPersonName.SelectedValue.Trim(),
                ProjectSelected = ddlProjects.SelectedValue.Trim(),
                CurrentIndex = gvActivities.PageIndex,
                FiltersChanged = Convert.ToBoolean(hdnResetFilter.Value),
                PeriodSelected = Convert.ToInt32(ddlPeriod.SelectedValue)
            };
            return filter;
        }

        private void FillEventList()
        {
            ddlEventSource.DataBind();

            ddlEventSource.SelectedValue = GetStringByValue(DisplayDropDownValue);
        }


        /// <summary>
        /// If we're on the page with ID parameter, select corresponding entity
        /// </summary>
        private void SelectFilterType()
        {
            var entityId = Page.Request[Constants.QueryStringParameterNames.Id];
            if (!string.IsNullOrEmpty(entityId))
            {
                entityId = entityId.Trim();
                var selectedValue = ddlEventSource.SelectedValue;

                if (selectedValue == GetStringByValue(ActivityEventSource.Project))
                    PrepareDropDown(entityId, ddlProjects);
                else if (selectedValue == GetStringByValue(ActivityEventSource.TargetPerson))
                    PrepareDropDown(entityId, ddlPersonName);
            }
        }

        private static void PrepareDropDown(string entityId, DropDownList listControl)
        {
            listControl.SelectedValue = entityId;
            listControl.Enabled = false;
        }

        protected void btnUpdateView_Click(object sender, EventArgs e)
        {
            UpdateGrid();

            if (hdnResetFilter.Value == "true")
                btnResetFilter.Enabled = true;
            else
                btnResetFilter.Enabled = false;
            if (IsActivityLogPage)
            {
                SaveFilterValuesForSession();
            }
        }

        protected void btnResetFilter_Click(object sender, EventArgs e)
        {
            ResetFilters();
            Update();
            IsReset = true;
            if (IsActivityLogPage)
            {
                SaveFilterValuesForSession();
            }
        }

        private void UpdateGrid()
        {
            gvActivities.PageIndex = 0;
            gvActivities.DataBind();
        }

        public void Update()
        {
            UpdateGrid();
            updActivityLog.Update();
        }

        protected void gvActivities_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                InitTransformation(e, "xmlChanges");
                InitTransformation(e, "xmlActivityItem");
            }
            if (e.Row.RowType == DataControlRowType.Pager)
            {
                TableCell tblcell0 = e.Row.Cells[0];
                tblcell0.Attributes.Add("colSpan", "1");

                TableCell tblcell = new TableCell();
                tblcell.Text = "&nbsp;&nbsp; of " + gvActivities.PageCount + " Pages";
                tblcell.Attributes.Add("colSpan", "2");
                e.Row.Cells.AddAt(1, tblcell);
            }
        }

        protected void gvActivities_OnDataBound(object sender, EventArgs e)
        {
            if (IsActivityLogPage)
            {
                SaveFilterSettings();
            }
        }

        protected void ddlProjects_OnDataBound(object sender, EventArgs e)
        {
            AddEmptyRow(ddlProjects, Resources.Controls.AllProjects);
            SelectFilterType();
        }

        private static void AddEmptyRow(DropDownList dropDown, string text)
        {
            if (dropDown.Items.Count > 0)
                dropDown.Items.Insert(
                    0, new ListItem(text, string.Empty));
        }

        protected void ddlPersonName_OnDataBound(object sender, EventArgs e)
        {
            AddEmptyRow(ddlPersonName, Resources.Controls.AnyUserText);
            SelectFilterType();
        }

        private void InitTransformation(GridViewRowEventArgs e, string xmlchangesControl)
        {
            var entityChanges = e.Row.FindControl(xmlchangesControl) as Xml;

            if (entityChanges != null)
                entityChanges.TransformArgumentList = _argumentList;
        }

        private void SetPeriodSelection(int periodSelected)
        {
            DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, Constants.Dates.FirstDay);

            if (periodSelected < 0)
            {
                DateTime startMonth = new DateTime();
                DateTime endMonth = new DateTime();

                if (periodSelected > -30)
                {
                    startMonth = DateTime.Now.Date.AddDays(periodSelected + 1);
                    endMonth = DateTime.Now.Date;
                }
                else
                {
                    startMonth = currentMonth.AddMonths((periodSelected / 30) + 1);
                    endMonth = currentMonth;
                }

                diRange.FromDate = startMonth;
                diRange.ToDate = new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
            }
            else
            {
                mpeCustomDates.Show();
            }
        }

        protected void odsActivities_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (ddlPeriod.SelectedValue == "0")
            {
                e.InputParameters["startDateFilter"] = diRange.FromDate.HasValue ? diRange.FromDate : SettingsHelper.GetCurrentPMTime().AddYears(-1);
                e.InputParameters["endDateFilter"] = diRange.ToDate.HasValue ? diRange.ToDate.Value.AddDays(1) : SettingsHelper.GetCurrentPMTime().AddDays(1);
            }
            else
            {
                var periodSelected = Convert.ToInt32(ddlPeriod.SelectedValue);

                DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, Constants.Dates.FirstDay);
                DateTime startMonth = new DateTime();
                DateTime endMonth = new DateTime();

                if (periodSelected > -30)
                {
                    startMonth = DateTime.Now.Date.AddDays(periodSelected + 1);
                    endMonth = DateTime.Now.Date;
                }
                else
                {
                    startMonth = currentMonth.AddMonths((periodSelected / 30) + 1);
                    endMonth = currentMonth;
                }

                e.InputParameters["startDateFilter"] = startMonth;
                e.InputParameters["endDateFilter"] = new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
            }
            e.InputParameters["sourceFilter"] = ddlEventSource.SelectedValue;
            e.InputParameters["opportunityId"] = (this.OpportunityId == null ? null : this.OpportunityId.ToString());
            e.InputParameters["milestoneId"] = (this.MilestoneId == null ? null : this.MilestoneId.ToString());

            e.InputParameters["personId"] = string.IsNullOrEmpty(ddlPersonName.SelectedValue) ?
                                                null : ddlPersonName.SelectedValue;
            e.InputParameters["projectId"] = string.IsNullOrEmpty(ddlProjects.SelectedValue) ?
                                                null : ddlProjects.SelectedValue;

            e.InputParameters["vendorId"] = VendorId;
            e.InputParameters["practiceAreas"] = true;
            e.InputParameters["sowBudget"] = true;
            e.InputParameters["director"] = true;
            e.InputParameters["poAmount"] = true;
            e.InputParameters["capabilities"] = true;
            e.InputParameters["newOrExtension"] = true;
            e.InputParameters["poNumber"] = true;
            e.InputParameters["projectStatus"] = true;
            e.InputParameters["salesPerson"] = true;
            e.InputParameters["projectOwner"] = true;
            e.InputParameters["recordPerChange"] = false;
            e.InputParameters["division"] = true;
            e.InputParameters["channel"] = true;
            e.InputParameters["offering"] = true;
            e.InputParameters["revenueType"] = true;
            e.InputParameters["budget"] = true;
            e.InputParameters["margin"] = true;
        }

        public object AddDefaultProjectAndMileStoneInfo(object logDataObject)
        {
            if (logDataObject != null)
            {
                var logDataStr = logDataObject.ToString();
                var XmlDoc = new XmlDocument();
                XmlDoc.LoadXml(logDataStr);
                var Root = XmlDoc.FirstChild;
                var defaultProjectId = MileStoneConfigurationManager.GetProjectId();
                var defaultMileStoneId = MileStoneConfigurationManager.GetMileStoneId();
                if (defaultProjectId.HasValue)
                {
                    var defaultProjectIdElement = XmlDoc.CreateElement("DefaultProjectId");
                    defaultProjectIdElement.InnerText = defaultProjectId.Value.ToString();
                    Root.InsertAfter(defaultProjectIdElement, Root.LastChild);
                }
                if (defaultMileStoneId.HasValue)
                {
                    var defaultMileStoneIdElement = XmlDoc.CreateElement("DefaultMileStoneId");
                    defaultMileStoneIdElement.InnerText = defaultMileStoneId.Value.ToString();
                    Root.InsertAfter(defaultMileStoneIdElement, Root.LastChild);
                }

                return Root.OuterXml;
            }
            else
            {
                return logDataObject;
            }
        }

        public string GetModifiedByDetails(object personId, object personLastFirstName, string SystemUser, object logData)
        {
            if (personId != null)
            {
                return personLastFirstName.ToString() == "N/A, " ? "N/A" : personLastFirstName.ToString();
            }

            string modifiedOrUser = string.Empty;

            if (logData != null)
            {
                var logDataStr = logData.ToString();
                var XmlDoc = new XmlDocument();
                XmlDoc.LoadXml(logDataStr);
                var Root = XmlDoc.FirstChild;
                if (Root.HasChildNodes)
                {
                    var newValues = Root.FirstChild;
                    if (Root.Name.ToLower() == LoginTag)
                    {
                        modifiedOrUser = GetAttribute(newValues, UserNameAttribute);
                    }
                    else if (Root.Name.ToLower() == "becomeuser" || Root.Name.ToLower() == "export")
                    {
                        modifiedOrUser = GetAttribute(newValues, UserAttribute);
                    }
                    else if (Root.Name.ToLower() == "note")
                    {
                        modifiedOrUser = GetAttribute(newValues, "By");
                    }
                    //else if (Root.Name.ToLower() == "error")
                    //{
                    //    modifiedOrUser = GetAttribute(newValues, "Login");
                    //}
                    else if (logData.ToString().Contains(ModifiedByNameAttribute))
                    {
                        modifiedOrUser = GetAttribute(newValues, ModifiedByNameAttribute);
                    }
                    else if (logData.ToString().Contains(UserNameAttribute))
                    {
                        modifiedOrUser = GetAttribute(newValues, UserNameAttribute);
                    }
                }
            }

            return string.IsNullOrEmpty(modifiedOrUser) ? SystemUser : modifiedOrUser;
        }

        private string GetAttribute(XmlNode newValues, string attribute)
        {
            if (newValues.Name.ToLower() == NewValuesTag && newValues.OuterXml.Contains(attribute))
            {
                var modifiedBy = newValues.Attributes.GetNamedItem(attribute);
                if (modifiedBy != null)
                    return modifiedBy.Value;
            }
            return null;
        }

        public string NoNeedToShowActivityType(object activityName)
        {
            return activityName.ToString().ToLower() == LoggedInActivity ? string.Empty : activityName.ToString();
        }

        private void SaveFilterValuesForSession()
        {
            ActivityLogFilters filter = new ActivityLogFilters();
            filter.EventSource = ddlEventSource.SelectedValue;
            filter.ReportPeriod = ddlPeriod.SelectedValue;
            filter.SelectedPerson = ddlPersonName.SelectedValue;
            filter.ReportStartDate = FromDateFilterValue;
            filter.ReportEndDate = ToDateFilterValue;
            filter.SelectedProject = ddlProjects.SelectedValue;
            ReportsFilterHelper.SaveFilterValues(ReportName.ActivityLogReport, filter);
        }

        private void GetFilterValuesForSession()
        {
            var filters = ReportsFilterHelper.GetFilterValues(ReportName.ActivityLogReport) as ActivityLogFilters;
            if (filters != null)
            {
                ddlEventSource.SelectedValue = filters.EventSource;
                ddlPeriod.SelectedValue = filters.ReportPeriod;
                ddlPersonName.SelectedValue = filters.SelectedPerson;
                ddlProjects.SelectedValue = filters.SelectedProject;
                FromDateFilterValue = filters.ReportStartDate;
                ToDateFilterValue = filters.ReportEndDate;
            }
        }

        protected void odsProjects_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["IsMilestone"] = DisplayDropDownValue == ActivityEventSource.Milestone;
        }
    }
}

