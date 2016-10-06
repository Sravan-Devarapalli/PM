﻿using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using DataTransferObjects;
using DataTransferObjects.ContextObjects;
using System.Linq;
using PraticeManagement.FilterObjects;

namespace PraticeManagement.Controls.Reports
{
    public partial class BenchReport : System.Web.UI.UserControl
    {
        private const string ConsultantNameSortOrder = "ConsultantNameSortOrder";
        private const string PracticeSortOrder = "PracticeSortOrder";
        private const string StatusSortOrder = "StatusSortOrder";
        private const string Descending = "Descending";
        private const string Ascending = "Ascending";
        private const string ReportContextKey = "ReportContext";
        private const string BenchListKey = "BenchList";
        private const string ViewStatePreviousSortExpression = "PreviousSortExpression";
        private const string ViewStatePreviousSortOrder = "PreviousSortOrder";

        private BenchReportContext ReportContext
        {
            get
            {
                if (ViewState[ReportContextKey] == null)
                {
                    BenchReportContext reportContext = new BenchReportContext()
                    {
                        Start = BegPeriod,
                        End = EndPeriod,
                        ActivePersons = cbActivePersons.Checked,
                        ProjectedPersons = cbProjectedPersons.Checked,
                        ActiveProjects = true,
                        ProjectedProjects = true,
                        ExperimentalProjects = true,
                        UserName = DataHelper.CurrentPerson.Alias,
                        PracticeIds = string.IsNullOrEmpty(cblPractices.SelectedItems) ? string.Empty : cblPractices.SelectedItems
                    };
                    ViewState[ReportContextKey] = reportContext;
                }
                return ViewState[ReportContextKey] as BenchReportContext;
            }
            set
            {
                ViewState[ReportContextKey] = value;
            }
        }

        private IEnumerable<Project> BenchList
        {
            get
            {
                if (ViewState[BenchListKey] == null)
                {
                    var benchList = GetBenchList();
                    ViewState[BenchListKey] = benchList;
                }
                return ViewState[BenchListKey] as IEnumerable<Project>;
            }
            set
            {
                ViewState[BenchListKey] = value;
            }
        }

        private BenchReportSortExpression SortExpression
        {
            get
            {
                return (BenchReportSortExpression)ViewState[ViewStatePreviousSortExpression];
            }
            set
            {
                ViewState[ViewStatePreviousSortExpression] = value;
            }
        }

        private SortDirection SortOrder
        {
            get
            {
                return ViewState[ViewStatePreviousSortOrder] == null ? SortDirection.Ascending : (SortDirection)ViewState[ViewStatePreviousSortOrder];
            }
            set
            {
                ViewState[ViewStatePreviousSortOrder] = value;
            }
        }

        public DateTime BegPeriod
        {
            get
            {
                DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, Constants.Dates.FirstDay);
                var periodSelected = int.Parse(ddlPeriod.SelectedValue);

                if (periodSelected > 0)
                {
                    DateTime startMonth = new DateTime();


                    if (periodSelected < 13)
                    {
                        startMonth = currentMonth;
                    }
                    else
                    {
                        Dictionary<string, DateTime> fPeriod = DataHelper.GetFiscalYearPeriod(currentMonth);
                        startMonth = fPeriod["StartMonth"];
                    }
                    return startMonth;

                }
                else if (periodSelected < 0)
                {
                    DateTime startMonth = new DateTime();

                    if (periodSelected > -13)
                    {
                        startMonth = currentMonth.AddMonths(Convert.ToInt32(ddlPeriod.SelectedValue));
                    }
                    else
                    {
                        Dictionary<string, DateTime> fPeriod = DataHelper.GetFiscalYearPeriod(currentMonth.AddYears(-1));
                        startMonth = fPeriod["StartMonth"];
                    }
                    return startMonth;
                }
                else
                {
                    return diRange.FromDate.Value;
                }
            }
        }

        public DateTime EndPeriod
        {
            get
            {
                DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, Constants.Dates.FirstDay);
                var periodSelected = int.Parse(ddlPeriod.SelectedValue);
                
                if (periodSelected > 0)
                {
                    DateTime endMonth = new DateTime();

                    if (periodSelected < 13)
                    {
                        endMonth = currentMonth.AddMonths(Convert.ToInt32(ddlPeriod.SelectedValue) - 1);
                    }
                    else
                    {
                        Dictionary<string, DateTime> fPeriod = DataHelper.GetFiscalYearPeriod(currentMonth);
                        endMonth = fPeriod["EndMonth"];
                    }
                    return new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
                }
                else if (periodSelected < 0)
                {
                    DateTime endMonth = new DateTime();

                    if (periodSelected > -13)
                    {
                        endMonth = currentMonth.AddMonths(Convert.ToInt32(ddlPeriod.SelectedValue));
                    }
                    else
                    {
                        Dictionary<string, DateTime> fPeriod = DataHelper.GetFiscalYearPeriod(currentMonth.AddYears(-1));
                        endMonth = fPeriod["EndMonth"];
                    }
                    return new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
                }
                else
                {
                    return diRange.ToDate.Value;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DataHelper.FillPracticeList(this.cblPractices, Resources.Controls.AllPracticesText);
                var cookie = SerializationHelper.DeserializeCookie(Constants.FilterKeys.BenchReportFilterCookie);
                if (Request.QueryString[Constants.FilterKeys.ApplyFilterFromCookieKey] == "true" && cookie != null)
                {
                    SetFilters(cookie);
                    DatabindGrid(false);
                }
                else
                {
                    SelectAllItems(this.cblPractices);
                }
            }

            AddAttributesToCheckBoxes(this.cblPractices);

            if (hdnFiltersChanged.Value == "false")
            {
                btnResetFilter.Attributes.Add("disabled", "true");
            }
            else
            {
                btnResetFilter.Attributes.Remove("disabled");
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            diRange.FromDate = BegPeriod;
            diRange.ToDate = EndPeriod;
            lblCustomDateRange.Text = string.Format("({0}&nbsp;-&nbsp;{1})",
                    diRange.FromDate.Value.ToString(Constants.Formatting.EntryDateFormat),
                    diRange.ToDate.Value.ToString(Constants.Formatting.EntryDateFormat)
                    );
            if (ddlPeriod.SelectedValue == "0")
            {
                lblCustomDateRange.Attributes.Add("class", "");
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

        private void SetFilters(object cookie)
        {
            var filter = cookie as BenchReportFilter;

            diRange.FromDate = filter.Start;
            diRange.ToDate = filter.End;
            ddlPeriod.SelectedValue = filter.PeriodSelected.ToString();
            cblPractices.SelectedItems = filter.PracticeIds;
            cbActivePersons.Checked = (bool)filter.ActivePersons;
            cbProjectedPersons.Checked = (bool)filter.ProjectedPersons;
            hdnFiltersChanged.Value = filter.FiltersChanged.ToString();

            SortOrder = filter.SortOrder;
            SortExpression = filter.SortExpression;
        }

        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {
            DataHelper.InsertExportActivityLogMessage("Bench Report");
            GridViewExportUtil.Export("BenchRollOff.xls", gvBench);
        }

        private IEnumerable<Project> GetBenchList()
        {
            var benchList = ReportsHelper.GetBenchListWithoutBenchTotalAndAdminCosts(ReportContext);

            return benchList.ToList<Project>().FindAll(p => (p.Practice != null
                                                             && !string.IsNullOrEmpty(p.Practice.Name)
                                                             && !string.IsNullOrEmpty(p.ProjectNumber)
                                                             )
                                                      );
        }

        private void SelectAllItems(ScrollingDropDown ddlpractices)
        {
            foreach (ListItem item in ddlpractices.Items)
            {
                item.Selected = true;
            }
        }

        private void DatabindGrid(bool defaultSortOrder)
        {
            var benchList = BenchList;
            AddMonthColumn(3, new DateTime(ReportContext.Start.Year, ReportContext.Start.Month, Constants.Dates.FirstDay), GetPeriodLength());

            if (defaultSortOrder)
            {
                gvBench.DataSource = BenchListWithSort(BenchReportSortExpression.ConsultantName, SortDirection.Ascending);
            }
            else
            {
                gvBench.DataSource = BenchListWithSort(SortExpression, SortOrder);
            }
            gvBench.DataBind();

            if (gvBench.Rows.Count > 0)
            {
                btnExportToExcel.Visible = true;
            }
            else
            {
                btnExportToExcel.Visible = false;
            }
        }

        private IEnumerable<Project> BenchListWithSort(BenchReportSortExpression sortExpression, SortDirection sortDirection)
        {
            IEnumerable<Project> benchList = BenchList;
            switch (sortExpression)
            {
                case BenchReportSortExpression.ConsultantName:
                    if (sortDirection == SortDirection.Descending)
                    {
                        benchList = benchList.OrderByDescending(p => p.Name);
                        gvBench.Attributes[ConsultantNameSortOrder] = Ascending;
                    }
                    else
                    {
                        benchList = benchList.OrderBy(p => p.Name);
                        gvBench.Attributes[ConsultantNameSortOrder] = Descending;
                    }
                    gvBench.Attributes[PracticeSortOrder] = Ascending;
                    gvBench.Attributes[StatusSortOrder] = Ascending;
                    break;

                case BenchReportSortExpression.Practice:
                    if (sortDirection == SortDirection.Descending)
                    {
                        benchList = benchList.OrderByDescending(P => P.Practice == null ? string.Empty : P.Practice.Name);
                        gvBench.Attributes[PracticeSortOrder] = Ascending;
                    }
                    else
                    {
                        benchList = benchList.OrderBy(P => P.Practice == null ? string.Empty : P.Practice.Name);
                        gvBench.Attributes[PracticeSortOrder] = Descending;
                    }
                    gvBench.Attributes[ConsultantNameSortOrder] = Ascending;
                    gvBench.Attributes[StatusSortOrder] = Ascending;
                    break;

                case BenchReportSortExpression.Status:
                    if (sortDirection == SortDirection.Descending)
                    {
                        benchList = benchList.OrderByDescending(P => P.ProjectNumber);
                        gvBench.Attributes[StatusSortOrder] = Ascending;
                    }
                    else
                    {
                        benchList = benchList.OrderBy(P => P.ProjectNumber);
                        gvBench.Attributes[StatusSortOrder] = Descending;
                    }
                    gvBench.Attributes[ConsultantNameSortOrder] = Ascending;
                    gvBench.Attributes[PracticeSortOrder] = Ascending;
                    break;
            }

            SortExpression = sortExpression;
            SortOrder = sortDirection;
            return benchList;
        }

        protected void btnUpdateReport_Click(object sender, EventArgs e)
        {
            Page.Validate(valSummaryPerformance.ValidationGroup);
            if (Page.IsValid)
            {
                ReportContext = null;
                BenchList = null;
                DatabindGrid(true);
            }
        }

        protected void btnResetFilter_OnClick(object sender, EventArgs e)
        {
            ddlPeriod.SelectedValue = "1";
            diRange.FromDate = BegPeriod;
            diRange.ToDate = EndPeriod;
            SelectAllItems(this.cblPractices);
            cbActivePersons.Checked = true;
            cbProjectedPersons.Checked = true;
            hdnFiltersChanged.Value = "false";
            btnResetFilter.Attributes.Add("disabled", "true");
        }

        private void AddAttributesToCheckBoxes(ScrollingDropDown ddl)
        {
            foreach (ListItem item in ddl.Items)
            {
                item.Attributes.Add("onclick", "EnableResetButton();");
            }
        }

        protected void btnSortConsultant_Click(object sender, EventArgs e)
        {
            var sortOrder = gvBench.Attributes[ConsultantNameSortOrder];
            var benchList = BenchList;
            if (string.IsNullOrEmpty(sortOrder) || sortOrder == Descending)
            {
                benchList = BenchListWithSort(BenchReportSortExpression.ConsultantName, SortDirection.Descending);
            }
            else
            {
                benchList = BenchListWithSort(BenchReportSortExpression.ConsultantName, SortDirection.Ascending);
            }

            AddMonthColumn(3, ReportContext.Start, GetPeriodLength());
            gvBench.DataSource = benchList;
            gvBench.DataBind();
        }

        protected void btnSortPractice_Click(object sender, EventArgs e)
        {

            var sortOrder = gvBench.Attributes[PracticeSortOrder];
            var benchList = BenchList;
            if (string.IsNullOrEmpty(sortOrder) || sortOrder == Ascending)
            {
                benchList = BenchListWithSort(BenchReportSortExpression.Practice, SortDirection.Ascending);
            }
            else
            {
                benchList = BenchListWithSort(BenchReportSortExpression.Practice, SortDirection.Descending);
            }

            AddMonthColumn(3, ReportContext.Start, GetPeriodLength());
            gvBench.DataSource = benchList;
            gvBench.DataBind();

        }

        protected void btnSortStatus_Click(object sender, EventArgs e)
        {
            var sortOrder = gvBench.Attributes[StatusSortOrder];
            var benchList = BenchList;

            if (string.IsNullOrEmpty(sortOrder) || sortOrder == Ascending)
            {
                benchList = BenchListWithSort(BenchReportSortExpression.Status, SortDirection.Ascending);
            }
            else
            {
                benchList = BenchListWithSort(BenchReportSortExpression.Status, SortDirection.Descending);
            }

            AddMonthColumn(3, ReportContext.Start, GetPeriodLength());
            gvBench.DataSource = benchList;
            gvBench.DataBind();
        }

        private void AddMonthColumn(int numberOfFixedColumns, DateTime periodStart, int monthsInPeriod)
        {
            // Remove columns from previous report if there was one
            for (var i = numberOfFixedColumns; i < gvBench.Columns.Count; i++)
                gvBench.Columns[i].Visible = false;

            // Add columns for new months);
            for (int i = numberOfFixedColumns, k = 0; k < monthsInPeriod; i++, k++)
            {
                gvBench.Columns[i].HeaderText =
                    Resources.Controls.TableHeaderOpenTag +
                    periodStart.ToString("MMM, yyyy") +
                    Resources.Controls.TableHeaderCloseTag;
                periodStart = periodStart.AddMonths(1);
            }
        }

        protected void gvBench_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var project = e.Row.DataItem as Project;
                bool rowVisible = false;

                if (project != null)
                {
                    //var monthBegin = rpReportFilter.MonthBegin;
                    var monthBegin = new DateTime(ReportContext.Start.Year, ReportContext.Start.Month, Constants.Dates.FirstDay); ;

                    int periodLength = GetPeriodLength();

                    // Displaying the interest values (main cell data)
                    for (int i = 3, k = 0;
                        k < periodLength;
                        i++, k++, monthBegin = monthBegin.AddMonths(1))
                    {
                        var monthEnd =
                            new DateTime(monthBegin.Year,
                                monthBegin.Month,
                                DateTime.DaysInMonth(monthBegin.Year, monthBegin.Month));

                        gvBench.Columns[i].Visible = true;
                        gvBench.Columns[i].HeaderText =
                            Resources.Controls.TableHeaderOpenTag +
                            monthBegin.ToString("MMM, yyyy") +
                            Resources.Controls.TableHeaderCloseTag;

                        foreach (KeyValuePair<DateTime, ComputedFinancials> interestValue in
                            project.ProjectedFinancialsByMonth)
                        {
                            if (IsInMonth(interestValue.Key, monthBegin, monthEnd) && interestValue.Value.GrossMargin.Value != 0M)
                            {
                                rowVisible = true;

                                if (interestValue.Value.EndDate.HasValue)
                                {
                                    if (IsInMonth(interestValue.Value.EndDate.Value, monthBegin, monthEnd))
                                    {
                                        e.Row.Cells[i].Text =
                                            interestValue.Value.EndDate.Value.ToShortDateString();
                                        e.Row.Cells[i].HorizontalAlign = HorizontalAlign.Right;
                                    }
                                    else
                                    {
                                        e.Row.Cells[i].Text = Resources.Controls.BusyLabel;
                                        e.Row.Cells[i].HorizontalAlign = HorizontalAlign.Center;
                                    }
                                }
                                else
                                {
                                    e.Row.Cells[i].Text = Resources.Controls.FreeLabel;
                                    e.Row.Cells[i].HorizontalAlign = HorizontalAlign.Center;
                                }
                            }
                        }
                    }
                }

                for (int i = 3; i < e.Row.Cells.Count; i++)
                {
                    e.Row.Cells[i].CssClass = "CompPerfData";
                }

                e.Row.Visible = rowVisible;
            }
        }

        protected void gvBench_OnDataBound(object sender, EventArgs e)
        {
            SaveFilterSettings();
        }

        protected void SaveFilterSettings()
        {
            var filter = GetFilterSettings();
            SerializationHelper.SerializeCookie(filter, Constants.FilterKeys.BenchReportFilterCookie);
        }

        protected BenchReportFilter GetFilterSettings()
        {
            var filter = new BenchReportFilter
            {
                Start = BegPeriod,
                End = EndPeriod,
                PeriodSelected = Convert.ToInt32(ddlPeriod.SelectedValue),
                ActivePersons = cbActivePersons.Checked,
                ProjectedPersons = cbProjectedPersons.Checked,
                UserName = DataHelper.CurrentPerson.Alias,
                PracticeIds = cblPractices.Items[0].Selected ? null : cblPractices.SelectedItems,
                SortExpression = SortExpression,
                SortOrder = SortOrder,
                FiltersChanged = btnResetFilter.Enabled
            };

            return filter;
        }

        private static bool IsInMonth(DateTime date, DateTime periodStart, DateTime periodEnd)
        {
            bool result =
                (date.Year > periodStart.Year ||
                (date.Year == periodStart.Year && date.Month >= periodStart.Month)) &&
                (date.Year < periodEnd.Year || (date.Year == periodEnd.Year && date.Month <= periodEnd.Month));

            return result;
        }

        /// <summary>
        /// Calculates a length of the selected period in the mounths.
        /// </summary>
        /// <returns>The number of the months within the selected period.</returns>
        private int GetPeriodLength()
        {
            //return 1;
            int mounthsInPeriod =
                (ReportContext.End.Year - ReportContext.Start.Year) * Constants.Dates.LastMonth +
                (ReportContext.End.Month - ReportContext.Start.Month + 1);
            return mounthsInPeriod;
        }

        private static string GetPersonDetailsUrl(object args)
        {
            return string.Format(Constants.ApplicationPages.DetailRedirectFormat,
                                 Constants.ApplicationPages.PersonDetail,
                                 args);
        }

        protected string GetPersonDetailsUrlWithReturn(object id)
        {
            return Utils.Generic.GetTargetUrlWithReturn(GetPersonDetailsUrl(id), Request.Url.AbsoluteUri + (Request.Url.Query.Length > 0 ? string.Empty : Constants.FilterKeys.QueryStringOfApplyFilterFromCookie));
        }
    }
}

