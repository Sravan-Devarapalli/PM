﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DataTransferObjects;
using DataTransferObjects.Reports.HumanCapital;
using PraticeManagement.Controls;
using PraticeManagement.Controls.Reports.HumanCapital;
using PraticeManagement.Utils;
using DataTransferObjects.Filters;

namespace PraticeManagement.Reporting
{
    public partial class TerminationReport : System.Web.UI.Page
    {
        #region constants

        private const string W2Hourly = "W2-Hourly";
        private const string W2Salary = "W2-Salary";
        private string TerminationReportExport = "Termination Report";
        private string ShowPanel = "ShowPanel('{0}', '{1}','{2}');";
        private string HidePanel = "HidePanel('{0}');";
        private string OnMouseOver = "onmouseover";
        private string OnMouseOut = "onmouseout";

        #endregion

        #region Properties

        //ddlPeriod Items
        //Last Month : 1
        //Last 3 months : 2
        //Last 6 months : 3
        //Last 9 months : 4
        //Last 12 months : 5
        //Year to Date : 6
        //Custom Date Range: 0

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
                        if (selectedVal == 1)
                        {
                            return Utils.Calendar.LastMonthStartDate(now);
                        }
                        else if (selectedVal == 2)
                        {
                            return Utils.Calendar.Last3MonthStartDate(now);
                        }
                        else if (selectedVal == 3)
                        {
                            return Utils.Calendar.Last6MonthStartDate(now);
                        }
                        else if (selectedVal == 4)
                        {
                            return Utils.Calendar.Last9MonthStartDate(now);
                        }
                        else if (selectedVal == 5)
                        {
                            return Utils.Calendar.Last12MonthStartDate(now);
                        }
                        else
                        {
                            return Utils.Calendar.YearStartDate(now);
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

                        if (selectedVal == 6)
                        {
                            return now;
                        }
                        else
                        {
                            return Utils.Calendar.LastMonthEndDate(now);
                        }
                    }
                }
                return null;
            }
        }

        //ddlPeriod Items
        //Last Month : 1
        //Last 3 months : 2
        //Last 6 months : 3
        //Last 9 months : 4
        //Last 12 months : 5
        //Year to Date : 6
        //Custom Date Range: 0
        public String Range
        {
            get
            {
                string range = string.Empty;
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    switch (RangeSelected)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            range = StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) + " - " + EndDate.Value.ToString(Constants.Formatting.EntryDateFormat);
                            break;
                        case 6:
                            range = "Year To Date (" + StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) + " - " + EndDate.Value.ToString(Constants.Formatting.EntryDateFormat) + ")";
                            break;
                        default:
                            range = StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) + " - " + EndDate.Value.ToString(Constants.Formatting.EntryDateFormat);
                            break;
                    }
                }
                return range;
            }
        }

        public int RangeSelected
        {
            get
            {
                int selectedValue = 0;
                int.TryParse(ddlPeriod.SelectedValue, out selectedValue);
                return selectedValue;
            }
        }

        public string PayTypes
        {
            get
            {
                return cblTimeScales.SelectedItemsXmlFormat;
            }
        }

        public List<int> PayTypesList
        {
            get
            {
                return cblTimeScales.SelectedValues;
            }
        }

        public string Titles
        {
            get
            {
                return cblTitles.SelectedItemsXmlFormat;
            }
        }

        public string TerminationReasons
        {
            get
            {
                return cblTerminationReasons.SelectedItemsXmlFormat;
            }
        }

        public string Practices
        {
            get
            {
                return cblPractices.SelectedItemsXmlFormat;
            }
        }

        public bool ExcludeInternalProjects
        {
            get
            {
                return chbInternalProjects.Checked;
            }
        }

        public bool SetSelectedFilters { get; set; }

        #endregion

        #region Page Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (this.cblTimeScales != null && this.cblTimeScales.Items.Count == 0)
                {
                    DataHelper.FillTimescaleList(this.cblTimeScales, Resources.Controls.AllTypes);

                }
                if (this.cblTitles != null && this.cblTitles.Items.Count == 0)
                {
                    DataHelper.FillTitleList(this.cblTitles, "All Titles", true);
                }
                if (this.cblTerminationReasons != null && this.cblTerminationReasons.Items.Count == 0)
                {
                    DataHelper.FillTerminationReasonsList(this.cblTerminationReasons, "All Reasons");
                }
                if (this.cblPractices != null && this.cblPractices.Items.Count == 0)
                {
                    DataHelper.FillPracticeList(this.cblPractices, Resources.Controls.AllPracticesText);
                }
                SetDefalultfilter();
                GetFilterValuesForSession();
                LoadActiveView();

                lblAttrition.Attributes[OnMouseOver] = string.Format(ShowPanel, lblAttrition.ClientID, pnlAtrritionCalculation.ClientID, 0);
                lblAttrition.Attributes[OnMouseOut] = string.Format(HidePanel, pnlAtrritionCalculation.ClientID);

                imgAttritionHint.Attributes[OnMouseOver] = string.Format(ShowPanel, imgAttritionHint.ClientID, pnlAttrition.ClientID, 175);
                imgAttritionHint.Attributes[OnMouseOut] = string.Format(HidePanel, pnlAttrition.ClientID);

            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            diRange.FromDate = StartDate;
            diRange.ToDate = EndDate;
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
        }

        #endregion

        #region Methods

        private void SetDefalultfilter()
        {
            ddlPeriod.SelectedValue = "6";
            this.chbInternalProjects.Checked = false;
            SelectAllItems(this.cblPractices);
            SelectAllItems(this.cblTitles);
            SelectAllItems(this.cblTerminationReasons);
            SelectDefaultTimeScaleItems(this.cblTimeScales);
        }

        private void SelectAllItems(ScrollingDropDown ddlItems)
        {
            foreach (ListItem item in ddlItems.Items)
            {
                item.Selected = true;
            }
        }

        private void SelectDefaultTimeScaleItems(ScrollingDropDown cblTimeScales)
        {
            foreach (ListItem item in cblTimeScales.Items)
            {
                item.Selected = (item.Text == W2Hourly || item.Text == W2Salary);
            }
        }

        private void populateGraph(int count, int percentage, Literal ltr, HtmlTableRow tr)
        {
            ltr.Text = count.ToString();
            if (percentage == 0)
            {
                tr.Height = "1px";
            }
            else
            {
                tr.Height = percentage + "px";
            }
        }

        public void PopulateHeaderSection(TerminationPersonsInRange reportData)
        {
            List<int> ratioList = (new int[] { reportData.TerminationsW2SalaryCountInTheRange, reportData.TerminationsW2HourlyCountInTheRange, reportData.TerminationsContractorsCountInTheRange }).ToList();
            int height = 80;
            List<int> percentageList = DataTransferObjects.Utils.Generic.GetProportionateRatio(ratioList, height);

            ltPersonCount.Text = reportData.PersonList.Count + " Terminations";
            lbRange.Text = Range;

            ltrlTotalEmployees.Text = reportData.TerminationsEmployeeCountInTheRange.ToString();
            ltrlTotalContractors.Text = reportData.TerminationsContractorsCountInTheRange.ToString();
            ltrlW2SalaryCount.Text = reportData.TerminationsW2SalaryCountInTheRange.ToString();
            ltrlW2HourlyCount.Text = reportData.TerminationsW2HourlyCountInTheRange.ToString();
            ltrlContractorsCount.Text = reportData.TerminationsContractorsCountInTheRange.ToString();

            populateGraph(reportData.TerminationsW2SalaryCountInTheRange, percentageList[0], ltrlW2SalaryCount, trW2Salary);
            populateGraph(reportData.TerminationsW2HourlyCountInTheRange, percentageList[1], ltrlW2HourlyCount, trW2Hourly);
            populateGraph(reportData.TerminationsContractorsCountInTheRange, percentageList[2], ltrlContractorsCount, trContrator);
            LoadAttrition();
        }

        /// <summary>
        /// Loads the attrition data with respect to date range.
        /// </summary>
        private void LoadAttrition()
        {
            List<TerminationPersonsInRange> data = ServiceCallers.Custom.Report(r => r.TerminationReportGraph(StartDate.Value, EndDate.Value)).ToList();
            if (data.Any())
            {
                TerminationPersonsInRange trp = data.Last();
                lblAttrition.Text = lblPopUpArrition.Text = trp.Attrition.ToString("0.00%");
                lblPopUPTerminations.Text = lblPopUPTerminationsCount.Text = lblPopUPTerminationsCountDenominator.Text = trp.TerminationsCumulativeEmployeeCountInTheRange.ToString();
                lblPopUPActivensCount.Text = lblPopUPActivens.Text = trp.ActivePersonsCountAtTheBeginning.ToString();
                lblPopUPNewHiresCount.Text = lblPopUPNewHires.Text = trp.NewHiredCumulativeInTheRange.ToString();
            }
        }

        private void LoadActiveView()
        {
            SetSelectedFilters = true;
            if (mvTerminationReport.ActiveViewIndex == 0)
            {
                tpSummary.PopulateData();
            }
            else
            {
                tpGraph.hlnkGraphHiddenField.Text = TerminationReportGraphView.SeeLast12Months;
                tpGraph.PopulateGraph();
            }
        }

        private void SwitchView(Control control, int viewIndex)
        {
            SelectView(control, viewIndex);
            LoadActiveView();
        }

        private void SelectView(Control sender, int viewIndex)
        {
            mvTerminationReport.ActiveViewIndex = viewIndex;

            SetCssClassEmpty();

            ((WebControl)sender.Parent).CssClass = "SelectedSwitch";
        }

        private void SetCssClassEmpty()
        {
            foreach (TableCell cell in tblPersonViewSwitch.Rows[0].Cells)
            {
                cell.CssClass = string.Empty;
            }

        }

        protected string GetDateFormat(DateTime date)
        {
            return date.ToString(Constants.Formatting.EntryDateFormat);
        }


        #endregion

        #region Control Events

        protected void Filters_Changed(object sender, EventArgs e)
        {
            LoadActiveView();
            SaveFilterValuesForSession();
        }

        protected void btnView_Command(object sender, CommandEventArgs e)
        {
            int viewIndex = int.Parse((string)e.CommandArgument);
            SwitchView((Control)sender, viewIndex);
        }

        protected void ddlPeriod_SelectedIndexChanged(object sender, EventArgs e)
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
        #endregion

        public void SaveFilterValuesForSession()
        {
            TerminationReportFilters filter = new TerminationReportFilters();
            filter.PracticeIds = cblPractices.SelectedItems;
            filter.ReportPeriod = ddlPeriod.SelectedValue;
            filter.ReportStartDate = diRange.FromDate;
            filter.ReportEndDate = diRange.ToDate;
            filter.ExcludeInternalPractices = chbInternalProjects.Checked;
            filter.PayTypeIds = cblTimeScales.SelectedItems;
            filter.TitleIds = cblTitles.SelectedItems;
            filter.TerminationReasonIds = cblTerminationReasons.SelectedItems;
            ReportsFilterHelper.SaveFilterValues(ReportName.TerminationReport, filter);
        }

        private void GetFilterValuesForSession()
        {
            var filters = ReportsFilterHelper.GetFilterValues(ReportName.TerminationReport) as TerminationReportFilters;
            if (filters != null)
            {
                cblPractices.UnSelectAll();
                cblPractices.SelectedItems = filters.PracticeIds;
                diRange.FromDate = filters.ReportStartDate;
                diRange.ToDate = filters.ReportEndDate;
                ddlPeriod.SelectedValue = filters.ReportPeriod;
                chbInternalProjects.Checked = filters.ExcludeInternalPractices;
                cblTimeScales.UnSelectAll();
                cblTimeScales.SelectedItems = filters.PayTypeIds;
                cblTitles.UnSelectAll();
                cblTitles.SelectedItems = filters.TitleIds;
                cblTerminationReasons.UnSelectAll();
                cblTerminationReasons.SelectedItems = filters.TerminationReasonIds;
            }
        }
    }
}

