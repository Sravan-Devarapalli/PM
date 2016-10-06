﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using PraticeManagement.Controls;
using DataTransferObjects;
using System.Web.Security;
using DataTransferObjects.Reports;
using DataTransferObjects.Filters;
using PraticeManagement.Utils;


namespace PraticeManagement.Reporting
{
    public partial class PersonDetailTimeReport : Page
    {
        public const string PersonIdKey = "PersonId";
        public const string StartDateKey = "StartDate";
        public const string EndDateKey = "EndDate";
        public const string PeriodSelectedkey = "PeriodSelected";
        public const string PersonNameKey = "Name";
        private string ShowPanel = "ShowPanel('{0}', '{1}','{2}');";
        private string HidePanel = "HidePanel('{0}');";
        private string OnMouseOver = "onmouseover";
        private string OnMouseOut = "onmouseout";

        public string PersonIdFromQueryString
        {
            get
            {
                return Request.QueryString[PersonIdKey];
            }
        }
        public string StartDateFromQueryString
        {
            get
            {
                return Request.QueryString[StartDateKey];
            }
        }
        public string EndDatFromQueryString
        {
            get
            {
                return Request.QueryString[EndDateKey];
            }
        }
        public string PeriodSelectedFromQueryString
        {
            get
            {
                return Request.QueryString[PeriodSelectedkey];
            }
        }
        public DateTime? StartDate
        {
            get
            {
                int selectedVal = 0;
                if (int.TryParse(ddlPeriod.SelectedValue, out selectedVal))
                {
                    if (selectedVal == 0)
                    {
                        return diRange.FromDate.Value;
                    }
                    else
                    {
                        var now = Utils.Generic.GetNowWithTimeZone();
                        if (selectedVal > 0)
                        {
                            if (selectedVal == 1)
                            {
                                return Utils.Calendar.QuarterStartDate(now, 1);
                            }
                            else if (selectedVal == 2)
                            {
                                return Utils.Calendar.QuarterStartDate(now, 2);
                            }
                            else if (selectedVal == 3)
                            {
                                return Utils.Calendar.QuarterStartDate(now, 3);
                            }
                            else if (selectedVal == 4)
                            {
                                return Utils.Calendar.QuarterStartDate(now, 4);
                            }
                            else if (selectedVal == 7)
                            {
                                return Utils.Calendar.WeekStartDate(now);
                            }
                            else if (selectedVal == 30)
                            {
                                return Utils.Calendar.MonthStartDate(now);
                            }
                            else if (selectedVal == 15)
                            {
                                return Utils.Calendar.PayrollCurrentStartDate(now);
                            }
                            else
                            {
                                return Utils.Calendar.YearStartDate(now);
                            }

                        }
                        else if (selectedVal < 0)
                        {
                            if (selectedVal == -7)
                            {
                                return Utils.Calendar.LastWeekStartDate(now);
                            }
                            else if (selectedVal == -15)
                            {
                                return Utils.Calendar.PayrollPerviousStartDate(now);
                            }
                            else if (selectedVal == -30)
                            {
                                return Utils.Calendar.LastMonthStartDate(now);
                            }
                            else if (selectedVal == -365)
                            {
                                return Utils.Calendar.LastYearStartDate(now);
                            }
                            else if (selectedVal == -1)
                            {
                                return SelectedPersonFirstHireDate;
                            }
                        }
                        else
                        {
                            return diRange.FromDate.Value;
                        }
                    }
                }
                return null;
            }
        }

        public DateTime? EndDate
        {
            get
            {
                int selectedVal = 0;
                if (int.TryParse(ddlPeriod.SelectedValue, out selectedVal))
                {
                    if (selectedVal == 0)
                    {
                        return diRange.ToDate.Value;
                    }
                    else
                    {
                        var now = Utils.Generic.GetNowWithTimeZone();
                        DateTime firstDay = new DateTime(now.Year, now.Month, 1);
                        if (selectedVal > 0)
                        {
                            //7
                            //15
                            //30
                            //365

                            if (selectedVal == 1)
                            {
                                return Utils.Calendar.QuarterEndDate(now, 1);
                            }
                            else if (selectedVal == 2)
                            {
                                return Utils.Calendar.QuarterEndDate(now, 2);
                            }
                            else if (selectedVal == 3)
                            {
                                return Utils.Calendar.QuarterEndDate(now, 3);
                            }
                            else if (selectedVal == 4)
                            {
                                return Utils.Calendar.QuarterEndDate(now, 4);
                            }
                            else if (selectedVal == 7)
                            {
                                return Utils.Calendar.WeekEndDate(now);
                            }
                            else if (selectedVal == 15)
                            {
                                return Utils.Calendar.PayrollCurrentEndDate(now);
                            }
                            else if (selectedVal == 30)
                            {
                                return Utils.Calendar.MonthEndDate(now);
                            }
                            else if (selectedVal == 365)
                            {
                                return Utils.Calendar.YearEndDate(now);
                            }
                        }
                        else if (selectedVal < 0)
                        {
                            if (selectedVal == -7)
                            {
                                return Utils.Calendar.LastWeekEndDate(now);
                            }
                            else if (selectedVal == -15)
                            {
                                return Utils.Calendar.PayrollPerviousEndDate(now);
                            }
                            else if (selectedVal == -30)
                            {
                                return Utils.Calendar.LastMonthEndDate(now);
                            }
                            else if (selectedVal == -365)
                            {
                                return Utils.Calendar.LastYearEndDate(now);
                            }
                            else if (selectedVal == -1)
                            {
                                if (SelectedPerson.TerminationDate.HasValue)
                                {
                                    return SelectedPerson.TerminationDate;
                                }
                                else
                                {
                                    return now;
                                }

                            }
                        }
                        else
                        {
                            return diRange.ToDate.Value;
                        }
                    }
                }
                return null;
            }
        }

        public String Range
        {
            get
            {
                string range = string.Empty;
                int selectedVal = 0;
                if (int.TryParse(ddlPeriod.SelectedValue, out selectedVal) && StartDate.HasValue && EndDate.HasValue)
                {
                    if (selectedVal == -1)
                    {
                        range = "Total Employment (" + StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) + " - " + EndDate.Value.ToString(Constants.Formatting.EntryDateFormat) + ")";
                    }
                    else
                    {
                        if (StartDate.Value == Utils.Calendar.MonthStartDate(StartDate.Value) && EndDate.Value == Utils.Calendar.MonthEndDate(StartDate.Value))
                        {
                            range = StartDate.Value.ToString("MMMM yyyy");
                        }
                        else if (StartDate.Value == Utils.Calendar.YearStartDate(StartDate.Value) && EndDate.Value == Utils.Calendar.YearEndDate(StartDate.Value))
                        {
                            range = StartDate.Value.ToString("yyyy");
                        }
                        else
                        {
                            range = StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) + " - " + EndDate.Value.ToString(Constants.Formatting.EntryDateFormat);
                        }
                    }
                }
                return range;
            }
        }

        public String RangeForExcel
        {
            get
            {
                string range = string.Empty;
                int selectedVal = 0;
                if (int.TryParse(ddlPeriod.SelectedValue, out selectedVal) && StartDate.HasValue && EndDate.HasValue)
                {
                    if (selectedVal == -1)
                    {
                        range = "Total Employment (" + StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) + " - " + EndDate.Value.ToString(Constants.Formatting.EntryDateFormat) + ")";
                    }
                    else
                    {
                        if (StartDate.Value == Utils.Calendar.YearStartDate(StartDate.Value) && EndDate.Value == Utils.Calendar.YearEndDate(StartDate.Value))
                        {
                            range = StartDate.Value.ToString("yyyy");
                        }
                        else
                        {
                            range = StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) + " - " + EndDate.Value.ToString(Constants.Formatting.EntryDateFormat);
                        }
                    }
                }
                return range;
            }
        }

        public int SelectedPersonId
        {
            get
            {
                return Convert.ToInt32(ddlPerson.SelectedValue);
            }
        }

        public DateTime SelectedPersonFirstHireDate
        {
            get
            {
                if (ViewState["SelectedPersonFirstHireDate"] == null)
                {
                    var employeeHistory = ServiceCallers.Custom.Person(p => p.GetPersonEmploymentHistoryById(SelectedPersonId));
                    DateTime minHireDate = employeeHistory.Min(p => p.HireDate);
                    ViewState["SelectedPersonFirstHireDate"] = minHireDate;
                }
                return (DateTime)ViewState["SelectedPersonFirstHireDate"];
            }
            set
            {
                ViewState["SelectedPersonFirstHireDate"] = value;
            }

        }

        public Person SelectedPerson
        {
            get
            {
                Person person;
                if (ViewState["SelectedPerson"] == null)
                {
                    person = ServiceCallers.Custom.Person(p => p.GetPersonById(SelectedPersonId));
                    ViewState["SelectedPersonFirstHireDate"] = null;
                    ViewState["SelectedPerson"] = person;
                }
                else
                {
                    person = (Person)ViewState["SelectedPerson"];
                    if (person.Id != SelectedPersonId)
                    {
                        person = ServiceCallers.Custom.Person(p => p.GetPersonById(SelectedPersonId));
                        ViewState["SelectedPersonFirstHireDate"] = null;
                        ViewState["SelectedPerson"] = person;
                    }
                }
                return person;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            dlPersonDiv.Style.Add("display", "none");
            if (!IsPostBack)
            {
                GetFilterValuesForSession();

                bool userIsAdministrator = Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.AdministratorRoleName);
                bool userIsDirector = Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.DirectorRoleName);
                bool userIsBusinessUnitManager = Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.BusinessUnitManagerRoleName);
                bool userIsOperations = Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.OperationsRoleName);

                var currentPerson = DataHelper.CurrentPerson;

                if (userIsAdministrator || userIsDirector || userIsBusinessUnitManager || userIsOperations)
                {
                    string statusIds = (int)PersonStatusType.Active + "," + (int)PersonStatusType.TerminationPending;
                    //DataHelper.FillPersonList(ddlPerson, null, statusIds, false);
                    DataHelper.FillPersonListWithPreferrdFirstName(ddlPerson, null, statusIds);
                }
                else
                {
                    ddlPerson.Items.Clear();
                    var logInPerson = ServiceCallers.Custom.Person(p => p.GetStrawmanDetailsById(currentPerson.Id.Value));
                    ddlPerson.Items.Add(new ListItem(logInPerson.PreferredOrFirstName, currentPerson.Id.Value.ToString()));
                    imgSearch.Visible = false;
                }
                ddlPerson.SelectedValue = currentPerson.Id.Value.ToString();

                if (!String.IsNullOrEmpty(PersonIdFromQueryString))
                {
                    ddlPerson.SelectedValue = PersonIdFromQueryString;
                    if (ddlPerson.SelectedValue != PersonIdFromQueryString)
                    {
                        var person = ServiceCallers.Custom.Person(p => p.GetPersonDetailsShort(int.Parse(PersonIdFromQueryString)));
                        ddlPerson.Items.Add(new ListItem(person.Name, PersonIdFromQueryString));
                        ddlPerson.SelectedValue = PersonIdFromQueryString;
                    }
                }

                lblBillableUtilization.Attributes[OnMouseOver] = string.Format(ShowPanel, lblBillableUtilization.ClientID, pnlBillableUtilizationCalculation.ClientID, 50);
                lblBillableUtilization.Attributes[OnMouseOut] = string.Format(HidePanel, pnlBillableUtilizationCalculation.ClientID);
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {

            if (timeEntryReportHeader.Count == 2)
            {
                tdFirst.Attributes["class"] = "Width50Percent";
                tdThird.Attributes["class"] = "Width20Percent";
            }
            else if (timeEntryReportHeader.Count == 1)
            {
                tdFirst.Attributes["class"] = "Width35Percent";
                tdThird.Attributes["class"] = "Width35Percent";
            }

            int personId = int.Parse(ddlPerson.SelectedItem.Value);
            Person person = ServiceCallers.Custom.Person(p => p.GetStrawmanDetailsById(personId));
            lblPersonname.ToolTip = lblPersonname.Text = ddlPerson.SelectedItem.Text;
            string personType = person.IsOffshore ? "Offshore" : string.Empty;
            string payType = person.CurrentPay.TimescaleName;
            string personStatusAndType = string.IsNullOrEmpty(personType) && string.IsNullOrEmpty(payType) ? string.Empty :
                                                                             string.IsNullOrEmpty(payType) ? personType :
                                                                             string.IsNullOrEmpty(personType) ? payType :
                                                                                                                 payType + ", " + personType;
            lblPersonStatus.ToolTip = lblPersonStatus.Text = personStatusAndType;
            var now = Utils.Generic.GetNowWithTimeZone();
            diRange.FromDate = StartDate.HasValue ? StartDate : Utils.Calendar.WeekStartDate(now);
            diRange.ToDate = EndDate.HasValue ? EndDate : Utils.Calendar.WeekEndDate(now);

            //From other report's link
            if (!string.IsNullOrEmpty(PeriodSelectedFromQueryString) && !IsPostBack)
            {
                ddlPeriod.SelectedValue = PeriodSelectedFromQueryString;
                if ((ddlPeriod.SelectedValue == "Please Select" || ddlPeriod.SelectedValue == "0") && !string.IsNullOrEmpty(StartDateFromQueryString))
                {
                    diRange.FromDate = Convert.ToDateTime(StartDateFromQueryString);
                    diRange.ToDate = Convert.ToDateTime(EndDatFromQueryString);
                    ddlPeriod.SelectedValue = "0";
                }
            }
            lbRange.ToolTip = lbRange.Text = Range;
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

            if (!IsPostBack)
            {
                LoadActiveView();
            }

        }

        protected void ddlPerson_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchView(lnkbtnSummary, 0);
        }

        protected void btnCustDatesOK_Click(object sender, EventArgs e)
        {
            Page.Validate(valSumDateRange.ValidationGroup);
            if (Page.IsValid)
            {
                hdnStartDate.Value = StartDate.Value.Date.ToShortDateString();
                hdnEndDate.Value = EndDate.Value.Date.ToShortDateString();
                LoadActiveView();
                SaveFilterValuesForSession();
            }
            else
            {
                mpeCustomDates.Show();
            }
        }

        protected void btnCustDatesCancel_OnClick(object sender, EventArgs e)
        {
            diRange.FromDate = Convert.ToDateTime(hdnStartDate.Value);
            diRange.ToDate = Convert.ToDateTime(hdnEndDate.Value);
        }

        protected void ddlPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (ddlPeriod.SelectedValue != "Please Select")
            {
                if (ddlPeriod.SelectedValue != "0")
                {
                    LoadActiveView();
                    SaveFilterValuesForSession();
                }
                else
                {
                    mpeCustomDates.Show();
                }
            }
            else
            {
                SwitchView(lnkbtnSummary, 0);
            }
        }

        protected void btnView_Command(object sender, CommandEventArgs e)
        {
            int viewIndex = int.Parse((string)e.CommandArgument);
            SwitchView((Control)sender, viewIndex);
        }

        private void SwitchView(Control control, int viewIndex)
        {
            SelectView(control, viewIndex);
            LoadActiveView();
            SaveFilterValuesForSession();
        }

        private void SetCssClassEmpty()
        {
            foreach (TableCell cell in tblPersonViewSwitch.Rows[0].Cells)
            {
                cell.CssClass = string.Empty;
            }
        }

        private void SelectView(Control sender, int viewIndex)
        {
            mvPersonDetailReport.ActiveViewIndex = viewIndex;

            SetCssClassEmpty();

            ((WebControl)sender.Parent).CssClass = "SelectedSwitch";
        }

        private void LoadActiveView()
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                divWholePage.Style.Remove("display");
                var result = ServiceCallers.Custom.Report(r => r.GetPersonTimeEntriesTotalsByPeriod(SelectedPersonId, StartDate.Value, EndDate.Value));
                PopulateTotalSection(result);
                if (mvPersonDetailReport.ActiveViewIndex == 0)
                {
                    PopulateSummaryDetails();
                }
                else
                {
                    PopulatePersonDetailReportDetails();
                }
            }
            else
            {
                divWholePage.Style.Add("display", "none");
            }
        }

        private void PopulatePersonDetailReportDetails()
        {
            var list = ServiceCallers.Custom.Report(r => r.PersonTimeEntriesDetails(SelectedPersonId, StartDate.Value, EndDate.Value)).ToList();
            ucpersonDetailReport.DatabindRepepeaterPersonDetails(list);
        }

        private void PopulateSummaryDetails()
        {
            var list = ServiceCallers.Custom.Report(r => r.PersonTimeEntriesSummary(SelectedPersonId, StartDate.Value, EndDate.Value)).ToList();
            ucpersonSummaryReport.DatabindRepepeaterSummary(list);
        }

        private void PopulateTotalSection(PersonTimeEntriesTotals personTimeEntriesTotals)
        {
            var billablePercent = 0;
            var nonBillablePercent = 0;
            if (personTimeEntriesTotals.BillableHours != 0 || personTimeEntriesTotals.NonBillableHours != 0)
            {
                billablePercent = DataTransferObjects.Utils.Generic.GetBillablePercentage(personTimeEntriesTotals.BillableHours, personTimeEntriesTotals.NonBillableHours);
                nonBillablePercent = (100 - billablePercent);
            }

            lblBillableUtilization.Text = lblBillableUtilizationPercentage.Text = personTimeEntriesTotals.BillableUtilizationPercentage;
            ltrlBillableHours.Text = personTimeEntriesTotals.BillableHours.ToString(Constants.Formatting.DoubleValue);
            lblTotalBillableHours.Text = lblTotalBillableHoursInBold.Text = personTimeEntriesTotals.BillableHoursUntilToday.ToString(Constants.Formatting.NumberFormatWithCommas);
            ltrlNonBillableHours.Text = personTimeEntriesTotals.NonBillableHours.ToString(Constants.Formatting.DoubleValue);
            ltrlTotalHours.Text = (personTimeEntriesTotals.BillableHours + personTimeEntriesTotals.NonBillableHours).ToString(Constants.Formatting.DoubleValue);
            ltrlBillablePercent.Text = billablePercent.ToString();
            ltrlNonBillablePercent.Text = nonBillablePercent.ToString();
            lblTotalAvailableHours.Text = lblTotalAvailableHoursInBold.Text = personTimeEntriesTotals.AvailableHours.ToString(Constants.Formatting.NumberFormatWithCommas);

            if (billablePercent == 0 && nonBillablePercent == 0)
            {
                trBillable.Height = "1px";
                trNonBillable.Height = "1px";
            }
            else if (billablePercent == 100)
            {
                trBillable.Height = "80px";
                trNonBillable.Height = "1px";
            }
            else if (billablePercent == 0 && nonBillablePercent == 100)
            {
                trBillable.Height = "1px";
                trNonBillable.Height = "80px";
            }
            else
            {
                int billablebarHeight = (int)(((float)80 / (float)100) * billablePercent);
                trBillable.Height = billablebarHeight.ToString() + "px";
                trNonBillable.Height = (80 - billablebarHeight).ToString() + "px";
            }

        }

        protected void lnkPerson_OnClick(object sender, EventArgs e)
        {
            LinkButton lbtn = sender as LinkButton;

            int personId = Convert.ToInt32(lbtn.Attributes["PersonId"]);

            var selectedItem = ddlPerson.Items.FindByValue(personId.ToString());

            if (selectedItem != null)
            {
                ddlPerson.SelectedValue = personId.ToString();
            }
            else
            {
                Person person = ServiceCallers.Custom.Person(p => p.GetStrawmanDetailsById(personId));
                ddlPerson.Items.Add(new ListItem(person.PreferredOrFirstName, personId.ToString()));
                ddlPerson.SelectedValue = personId.ToString();
            }

            ddlPeriod.SelectedValue = "Please Select";
            LoadActiveView();
            txtSearch.Text = "";
            SaveFilterValuesForSession();
        }

        protected String GetPersonFirstLastName(Person person)
        {
            return person.PersonLastFirstName;
        }

        protected void btnSearch_OnClick(object sender, EventArgs e)
        {
            string looked = txtSearch.Text;
            if (!string.IsNullOrEmpty(looked))
            {
                var personList = ServiceCallers.Custom.Person(p => p.GetPersonListBySearchKeyword(looked));
                dlPersonDiv.Style.Add("display", "");
                if (personList.Length > 0)
                {
                    repPersons.Visible = true;
                    divEmptyResults.Style.Add("display", "none");
                }
                else
                {
                    repPersons.Visible = false;
                    divEmptyResults.Style.Add("display", "");
                }
                repPersons.DataSource = personList;
                repPersons.DataBind();
                btnSearch.Attributes.Remove("disabled");
            }
            mpePersonSearch.Show();
        }

        private void SaveFilterValuesForSession()
        {
            TimeReports filter = new TimeReports();
            filter.Person = ddlPerson.SelectedValue;
            filter.ReportPeriod = ddlPeriod.SelectedValue;
            filter.StartDate = diRange.FromDate.Value;
            filter.EndDate = diRange.ToDate.Value;
            ReportsFilterHelper.SaveFilterValues(ReportName.ByPersonReport, filter);
        }

        private void GetFilterValuesForSession()
        {
            var filters = ReportsFilterHelper.GetFilterValues(ReportName.ByPersonReport) as TimeReports;
            if (filters != null)
            {
                ddlPerson.SelectedValue = filters.Person;

                ddlPeriod.SelectedValue = filters.ReportPeriod;
                diRange.FromDate = filters.StartDate;
                diRange.ToDate = filters.EndDate;
            }
        }

    }
}

