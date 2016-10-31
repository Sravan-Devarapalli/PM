﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using AjaxControlToolkit;
using DataTransferObjects;
using PraticeManagement.Configuration;
using PraticeManagement.ProjectService;
using PraticeManagement.Security;
using PraticeManagement.Utils;
using PraticeManagement.Utils.Excel;

namespace PraticeManagement.Controls.Projects
{
    public partial class ProjectSummary : System.Web.UI.UserControl
    {
        #region Constants

        private const string CurrencyDisplayFormat = "$###,###,###,###,###,##0";
        private const string CurrencyExcelReportFormat = "$####,###,###,###,###,##0.00";
        //private const int NumberOfFixedColumns = 12;
        private const int ProjectStateColumnIndex = 0;
        private const int ProjectNumberColumnIndex = 1;
        private const int ClientNameColumnIndex = 2;
        private const int ProjectNameColumnIndex = 3;
        private const int StartDateColumnIndex = 4;
        private const int EndDateColumnIndex = 5;

        private const int MaxPeriodLength = 24;

        private const string ButtonClientNameId = "btnClientName";
        private const string LabelProjectNumberId = "lblProjectNumber";
        private const string LabelStartDateId = "lblStartDate";
        private const string LabelEndDateId = "lblEndDate";

        private const string LabelHeaderIdFormat = "lblHeader{0}";
        private const string LabelHeaderIdToolTipView = "{0} Monthly Report";
        private const string PanelHeaderIdFormat = "pnlHeader{0}";
        private const string PanelReportIdFormat = "pnlReport{0}";
        private const string CloseReportButtonIdFormat = "btnCloseReport{0}";
        private const string ReportCellIdFormat = "cellReport{0}";

        private const string TotalHeaderFormat = "Total ({0})";
        private const string STR_SortExpression = "SortExpression";
        private const string STR_SortDirection = "SortDirection";
        private const string STR_SortColumnId = "SortColumnId";
        private const string ToolTipView = "<b>Buyer Name:&nbsp;</b>{0}<br/><b>Salesperson:&nbsp;</b>{1}<br/><b>Owner:&nbsp;</b>{2}<br/><b>SOW Budget:&nbsp;</b>{3}<br/><b>Resources:&nbsp;</b>{4}<br/>";
        private const string AppendPersonFormat = "{0}{1}, {2}";
        private const string CompPerfHeaderDivCssClass = "ie-bg no-wrap";
        private const string HintDivCssClass = "hint";
        private const string OneGreaterSeniorityExistsKey = "ProjectsListOneGreaterSeniorityExists";
        private const string Revenue = "Revenue";
        private const string ServiceRevenue = "Services Revenue";
        private const string Margin = "Cont. Margin";

        private const string CompanyPerformanceFilterKey = "CompanyPerformanceFilterKey";

        private const string ViewExportProjects = "ExportProjects";
        private const string ViewExportAllProjects = "ExportAllProjects";
        private const string ExportDateRangeFormat = "Date Range: {0} - {1}";

        protected const string PagerNextCommand = "Next";
        protected const string PagerPrevCommand = "Prev";

        private const string PageViewCountFormat = "Viewing {0} - {1} of {2} Projects";

        #endregion Constants

        #region Fields

        private bool userIsAdministrator;
        private bool renderMonthColumns;
        private int coloumnsCount = 1;
        private int headerRowsCount = 1;

        #endregion Fields

        #region Properties

        private PraticeManagement.Config.ProjectsReport HostingPageIsProjectsReport
        {
            get
            {
                if (Page is PraticeManagement.Config.ProjectsReport)
                {
                    return ((PraticeManagement.Config.ProjectsReport)Page);
                }
                else
                {
                    return null;
                }
            }
        }

        private PraticeManagement.Projects HostingPageIsProjectSummary
        {
            get
            {
                if (Page is PraticeManagement.Projects)
                {
                    return ((PraticeManagement.Projects)Page);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Indicates if there were ata least one field that was hidden
        /// </summary>
        private bool OneGreaterSeniorityExists
        {
            get
            {
                if (Session[OneGreaterSeniorityExistsKey] == null)
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(Session[OneGreaterSeniorityExistsKey]);
                }
            }
            set
            {
                Session[OneGreaterSeniorityExistsKey] = value;
            }
        }

        /// <summary>
        /// Gets a selected period start.
        /// </summary>
        private DateTime PeriodStart
        {
            get
            {
                return diRange.FromDate.Value;
            }
        }

        /// <summary>
        /// Gets a selected period end.
        /// </summary>
        private DateTime PeriodEnd
        {
            get
            {
                return diRange.ToDate.Value;
            }
        }

        private string SelectedClientIds
        {
            get
            {
                return cblClient.SelectedItems;
            }
            set
            {
                cblClient.SelectedItems = value;
            }
        }

        private string SelectedSalespersonIds
        {
            get
            {
                return cblSalesperson.SelectedItems;
            }
            set
            {
                cblSalesperson.SelectedItems = value;
            }
        }

        private string SelectedPracticeIds
        {
            get
            {
                return cblPractice.SelectedItems;
            }
            set
            {
                cblPractice.SelectedItems = value;
            }
        }

        private string SelectedDivisionIds
        {
            get
            {
                return cblDivision.SelectedItems;
            }
            set
            {
                cblDivision.SelectedItems = value;
            }
        }

        private string SelectedChannelIds
        {
            get
            {
                return cblChannel.SelectedItems;
            }
            set
            {
                cblChannel.SelectedItems = value;
            }
        }

        private string SelectedRevenueTypeIds
        {
            get
            {
                return cblRevenueType.SelectedItems;
            }
            set
            {
                cblRevenueType.SelectedItems = value;
            }
        }

        private string SelectedOfferingIds
        {
            get
            {
                return cblOffering.SelectedItems;
            }
            set
            {
                cblOffering.SelectedItems = value;
            }
        }

        private string SelectedGroupIds
        {
            get
            {
                return cblProjectGroup.SelectedItems;
            }
            set
            {
                cblProjectGroup.SelectedItems = value;
            }
        }

        private string SelectedProjectOwnerIds
        {
            get
            {
                return cblProjectOwner.SelectedItems;
            }
            set
            {
                cblProjectOwner.SelectedItems = value;
            }
        }

        private int NumberOfFixedColumns
        {
            get
            {
                //return HostingPageIsProjectsReport != null ? 15 : 12;
                return 15;
            }
        }

        /// <summary>
        /// Gets a list of projects to be displayed.
        /// </summary>
        private Project[] ProjectList
        {
            get
            {
                CompanyPerformanceState.Filter = GetFilterSettings();
                bool isProjectSummaryCachedToday = false;
                if ((ddlPeriod.SelectedValue == "-13" || ddlPeriod.SelectedValue == "13") && isProjectSummaryCachedToday)
                {
                    CompanyPerformanceState.Filter.FinancialsFromCache = true;
                }
                else
                {
                    CompanyPerformanceState.Filter.FinancialsFromCache = false;
                }
                return CompanyPerformanceState.ProjectList.ToList().OrderBy(p => p.ProjectNumber).ToArray();
            }
        }

        private Project[] ExportProjectList
        {
            get
            {
                if (ViewState[ViewExportProjects] == null)
                {
                    ViewState[ViewExportProjects] = ProjectList;
                }
                return (Project[])ViewState[ViewExportProjects];
            }
        }

        private Project[] ExportAllProjectList
        {
            get
            {
                if (ViewState[ViewExportAllProjects] == null)
                {
                    ViewState[ViewExportAllProjects] = GetProjectListAll();
                }
                return (Project[])ViewState[ViewExportAllProjects];
            }
        }

        private int ListViewSortColumnId
        {
            get { return ViewState[STR_SortColumnId] != null ? (int)ViewState[STR_SortColumnId] : ProjectNumberColumnIndex; }
            set { ViewState[STR_SortColumnId] = value; }
        }

        private string PrevListViewSortExpression
        {
            get { return ViewState[STR_SortExpression] as string ?? string.Empty; }
            set { ViewState[STR_SortExpression] = value; }
        }

        private string ListViewSortDirection
        {
            get { return ViewState[STR_SortDirection] as string ?? "Ascending"; }
            set { ViewState[STR_SortDirection] = value; }
        }

        /// <summary>
        /// Gets a text to be searched for.
        /// </summary>
        public string SearchText
        {
            get
            {
                return txtSearchText.Text;
            }
        }

        public bool IsSearchClicked
        {
            get;
            set;
        }

        public int CalculationsType
        {
            get
            {
                return Convert.ToInt32(ddlCalculationsType.SelectedValue);
            }
        }

        private SheetStyles HeaderSheetStyle
        {
            get
            {
                CellStyles cellStyle = new CellStyles();
                cellStyle.IsBold = true;
                cellStyle.BorderStyle = NPOI.SS.UserModel.BorderStyle.None;
                cellStyle.FontHeight = 350;
                CellStyles[] cellStylearray = { cellStyle };
                RowStyles headerrowStyle = new RowStyles(cellStylearray);
                headerrowStyle.Height = 500;

                CellStyles dataCellStyle = new CellStyles();
                CellStyles[] dataCellStylearray = { dataCellStyle };
                RowStyles datarowStyle = new RowStyles(dataCellStylearray);

                RowStyles[] rowStylearray = { headerrowStyle, datarowStyle };

                SheetStyles sheetStyle = new SheetStyles(rowStylearray);
                sheetStyle.MergeRegion.Add(new int[] { 0, 0, 0, coloumnsCount - 1 });
                sheetStyle.IsAutoResize = false;

                return sheetStyle;
            }
        }

        private SheetStyles DataSheetStyle
        {
            get
            {
                CellStyles headerCellStyle = new CellStyles();
                headerCellStyle.IsBold = true;
                headerCellStyle.HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

                CellStyles monthNameHeaderCellStyle = new CellStyles();
                monthNameHeaderCellStyle.DataFormat = "[$-409]mmmm-yy;@";
                monthNameHeaderCellStyle.IsBold = true;
                monthNameHeaderCellStyle.HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

                List<CellStyles> headerCellStyleList = new List<CellStyles>();
                for (int i = 0; i < 19; i++)//there are 14 columns before month columns.
                    headerCellStyleList.Add(headerCellStyle);

                if (renderMonthColumns)
                {
                    var monthsInPeriod = GetPeriodLength();
                    for (int i = 0; i < monthsInPeriod; i++)
                    {
                        headerCellStyleList.Add(monthNameHeaderCellStyle);
                    }
                }
                headerCellStyleList.Add(headerCellStyle);

                RowStyles headerrowStyle = new RowStyles(headerCellStyleList.ToArray());

                CellStyles dataCellStyle = new CellStyles();

                CellStyles wrapdataCellStyle = new CellStyles();
                wrapdataCellStyle.WrapText = true;

                CellStyles dataStartDateCellStyle = new CellStyles();
                dataStartDateCellStyle.DataFormat = "mm/dd/yy;@";

                CellStyles dataNumberDateCellStyle = new CellStyles();
                dataNumberDateCellStyle.DataFormat = "$#,##0.00_);($#,##0.00)";

                CellStyles[] dataCellStylearray = { dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataStartDateCellStyle, dataStartDateCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle };
                List<CellStyles> dataCellStyleList = dataCellStylearray.ToList();

                var coloumnWidth = new List<int>();
                for (int i = 0; i < 12; i++)
                    coloumnWidth.Add(0);
                coloumnWidth.Add(50);
                coloumnWidth.Add(0);

                if (renderMonthColumns)
                {
                    var monthsInPeriod = GetPeriodLength();
                    for (int i = 0; i < monthsInPeriod; i++)
                    {
                        dataCellStyleList.Add(dataNumberDateCellStyle);
                    }
                }
                dataCellStyleList.Add(dataNumberDateCellStyle);
                dataCellStyleList.Add(wrapdataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                //CAST OWNER dataCellStyleList.Add(dataCellStyle);

                RowStyles datarowStyle = new RowStyles(dataCellStyleList.ToArray());

                RowStyles[] rowStylearray = { headerrowStyle, datarowStyle };
                SheetStyles sheetStyle = new SheetStyles(rowStylearray);
                sheetStyle.TopRowNo = headerRowsCount;
                sheetStyle.IsFreezePane = true;
                sheetStyle.FreezePanColSplit = 0;
                sheetStyle.FreezePanRowSplit = headerRowsCount;
                sheetStyle.ColoumnWidths = coloumnWidth;
                return sheetStyle;
            }
        }

        private string ErrorMessage { get; set; }

        #endregion Properties

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            userIsAdministrator = Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.AdministratorRoleName);

            if (IsPostBack)
            {
                PreparePeriodView();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Security
            if (!userIsAdministrator)
            {
                Person current = DataHelper.CurrentPerson;
                int? personId = current != null ? current.Id : null;

                bool userIsSalesperson =
                    Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.SalespersonRoleName);
                bool userIsPracticeManager =
                    Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.PracticeManagerRoleName);
                bool userIsBusinessUnitManager =
                    Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.BusinessUnitManagerRoleName);

                bool userIsDirector =
                    Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.DirectorRoleName);
                bool userIsSeniorLeadership =
               Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.SeniorLeadershipRoleName);// #2913: userIsSeniorLeadership is added as per the requirement.

                SelectItemInControl(userIsSalesperson, cblSalesperson, personId);
                SelectItemInControl((userIsPracticeManager || userIsBusinessUnitManager || userIsDirector || userIsSeniorLeadership), cblProjectOwner, personId);// #2817: userIsDirector is added as per the requirement.

                bool userIsProjectLead =
                    Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.ProjectLead);
                if (userIsProjectLead && !(userIsSalesperson || userIsPracticeManager || userIsBusinessUnitManager || userIsDirector || userIsSeniorLeadership))
                {
                    lnkAddProject.Visible = false;//as per #2941 .
                }

            }

            // Client side validator is not applicable here.
            reqSearchText.IsValid = true;

           
            lblCustomDateRange.Text = string.Format("({0}&nbsp;-&nbsp;{1})",
                    diRange.FromDate.Value.ToString(Constants.Formatting.EntryDateFormat),
                    diRange.ToDate.Value.ToString(Constants.Formatting.EntryDateFormat)
                    );
            if (ddlPeriod.SelectedValue == "0")
            {
                lblCustomDateRange.Attributes.Add("class", "fontBold");
                imgCalender.Attributes.Remove("class");
            }
            else
            {
                lblCustomDateRange.Attributes.Add("class", "displayNone");
                imgCalender.Attributes.Add("class", "displayNone");
            }

            var pager = GetPager();
            if (pager != null && !IsSearchClicked)
            {
                if (ddlView.SelectedValue != "1")
                {
                    if (pager.PageSize != Convert.ToInt32(ddlView.SelectedValue))
                    {
                        Response.Redirect(Request.Url.AbsoluteUri);
                    }
                }
                else
                {
                    if (pager.PageSize != pager.TotalRowCount)
                    {
                        Response.Redirect(Request.Url.AbsoluteUri);
                    }
                }
            }
            imgExportAllToExcel.Visible = userIsAdministrator;
            if (HostingPageIsProjectsReport != null)
            {
                lnkAddProject.Visible = false;
                imgExportAllToExcel.Visible = false;
            }
            hdnStartDate.Value = diRange.FromDate.Value.ToString(Constants.Formatting.EntryDateFormat);
            hdnEndDate.Value = diRange.ToDate.Value.ToString(Constants.Formatting.EntryDateFormat);
            hdnStartDateTxtBoxId.Value = (diRange.FindControl("tbFrom") as TextBox).ClientID;
            hdnEndDateTxtBoxId.Value = (diRange.FindControl("tbTo") as TextBox).ClientID;

        }

        public void ddlPeriod_SelectedIndexChanged()
        {
            int periodSelected = Convert.ToInt32(ddlPeriod.SelectedValue);

            SetPeriodSelection(periodSelected);
        }

        private void SetddlView()
        {
            DataPager pager = GetPager();
            if (pager != null)
            {
                if (ddlView.SelectedValue != "1")
                {
                    pager.SetPageProperties(0, Convert.ToInt32(ddlView.SelectedValue), false);
                }
                else
                {
                    pager.SetPageProperties(0, pager.TotalRowCount, false);
                }
            }
        }

        private void RemoveToControls(int FromSelectedValue, DropDownList yearToControl)
        {
            foreach (ListItem toYearItem in yearToControl.Items)
            {
                var toYearItemInt = Convert.ToInt32(toYearItem.Value);
                if (toYearItemInt < FromSelectedValue)
                {
                    toYearItem.Enabled = false;
                }
                else
                {
                    toYearItem.Enabled = true;
                }
            }
        }

        private void SetPeriodSelection(int periodSelected)
        {
            DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, Constants.Dates.FirstDay);
            if (periodSelected > 0)
            {
                DateTime startMonth = new DateTime();
                DateTime endMonth = new DateTime();

                if (periodSelected < 13)
                {
                    startMonth = currentMonth;
                    endMonth = currentMonth.AddMonths(Convert.ToInt32(ddlPeriod.SelectedValue) - 1);
                }
                else
                {
                    Dictionary<string, DateTime> fPeriod = DataHelper.GetFiscalYearPeriod(currentMonth);
                    startMonth = fPeriod["StartMonth"];
                    endMonth = fPeriod["EndMonth"];
                }
                diRange.FromDate = startMonth;
                diRange.ToDate = new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
            }
            else if (periodSelected < 0)
            {
                DateTime startMonth = new DateTime();
                DateTime endMonth = new DateTime();

                if (periodSelected > -13)
                {
                    startMonth = currentMonth.AddMonths(Convert.ToInt32(ddlPeriod.SelectedValue) + 1);
                    endMonth = currentMonth;
                }
                else
                {
                    Dictionary<string, DateTime> fPeriod = DataHelper.GetFiscalYearPeriod(currentMonth.AddYears(-1));
                    startMonth = fPeriod["StartMonth"];
                    endMonth = fPeriod["EndMonth"];
                }
                diRange.FromDate = startMonth;
                diRange.ToDate = new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
            }
        }

        /// <summary>
        /// Selects item in list control according to some condition
        /// </summary>
        /// <param name="condition">If true, the item will be selected</param>
        /// <param name="targetControl">Control to select item in</param>
        /// <param name="lookFor">Value to look for in the control</param>
        private static void SelectItemInControl(bool condition, ListControl targetControl, int? lookFor)
        {
            if (condition)
            {
                ListItem targetItem = targetControl.Items.FindByValue(lookFor == null ? null : lookFor.ToString());
                if (targetItem != null)
                    targetItem.Selected = true;
            }
        }

        /// <summary>
        /// Stores a current filter set.
        /// </summary>
        private void SaveFilterSettings()
        {
            CompanyPerformanceFilterSettings filter = GetFilterSettings();
            if (HostingPageIsProjectsReport == null)
            {
                SerializationHelper.SerializeCookie(filter, CompanyPerformanceFilterKey);
            }
            else
            {
                ReportsFilterHelper.SaveFilterValues(ReportName.ProjectSummaryReport, filter);
            }
        }

        /// <summary>
        /// Gets a current filter settings.
        /// </summary>
        /// <returns>The <see cref="CompanyPerformanceFilterSettings"/> with a current filter.</returns>
        public CompanyPerformanceFilterSettings GetFilterSettings()
        {
            var filter =
                 new CompanyPerformanceFilterSettings
                 {
                     StartYear = diRange.FromDate.Value.Year,
                     StartMonth = diRange.FromDate.Value.Month,
                     StartDay = diRange.FromDate.Value.Day,
                     EndYear = diRange.ToDate.Value.Year,
                     EndMonth = diRange.ToDate.Value.Month,
                     EndDay = diRange.ToDate.Value.Day,
                     ClientIdsList = SelectedClientIds,
                     ProjectOwnerIdsList = SelectedProjectOwnerIds,
                     PracticeIdsList = SelectedPracticeIds,
                     DivisionIdsList = SelectedDivisionIds,
                     ChannelIdsList = SelectedChannelIds,
                     OfferingIdsList = SelectedOfferingIds,
                     RevenueTypeIdsList = SelectedRevenueTypeIds,
                     SalespersonIdsList = SelectedSalespersonIds,
                     ProjectGroupIdsList = SelectedGroupIds,
                     ShowActive = chbActive.Checked,
                     ShowAtRisk = chbAtRisk.Checked,
                     ShowCompleted = chbCompleted.Checked,
                     ShowProjected = chbProjected.Checked,
                     ShowInternal = chbInternal.Checked,
                     ShowExperimental = chbExperimental.Checked,
                     ShowProposed = chbProposed.Checked,
                     ShowInactive = chbInactive.Checked,
                     PeriodSelected = Convert.ToInt32(ddlPeriod.SelectedValue),
                     ViewSelected = Convert.ToInt32(ddlView.SelectedValue),

                     CalculateRangeSelected = (ProjectCalculateRangeType)Enum.Parse(typeof(ProjectCalculateRangeType), ddlCalculateRange.SelectedValue),
                     HideAdvancedFilter = false,
                     CalculationsType = Convert.ToInt32(ddlCalculationsType.SelectedValue)
                 };
            return filter;
        }

        protected void lvProjects_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                var row = e.Item.FindControl("boundingRow") as HtmlTableRow;
                if (HostingPageIsProjectsReport == null)
                {
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "HideColumns();", true);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "ShowColumns();", true);
                }
                if (row.Cells.Count == NumberOfFixedColumns)
                {
                    var monthsInPeriod = GetPeriodLength();
                    for (int i = 0; i < monthsInPeriod + 1; i++)   // + 1 means a cell for total column
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                }

                var project = (e.Item as ListViewDataItem).DataItem as Project;
                var rowVisible = false;
                if (project != null)
                {
                    // Determine whether to display the project in the list.
                    rowVisible = IsProjectVisible(project);

                    string cssClass = ProjectHelper.GetIndicatorClassByStatusId(project.Status.Id);

                    if (project.Status.Id == 3 && !project.HasAttachments)
                    {
                        cssClass = "ActiveProjectWithoutSOW";
                    }

                    SeniorityAnalyzer personListAnalyzer = null;

                    if (project.Id.HasValue)
                    {
                        /*
                         * TEMPORARY COMMENT
                         * Will be then used to fix #1257
                         */
                        personListAnalyzer = new SeniorityAnalyzer(DataHelper.CurrentPerson);
                        personListAnalyzer.OneWithGreaterSeniorityExists(project.ProjectPersons);

                        var htmlRow = e.Item.FindControl("boundingRow") as HtmlTableRow;
                        FillProjectStateCell(htmlRow, cssClass, project.Status);
                        FillProjectNumberCell(e.Item, project);
                        FillClientNameCell(e.Item, project);
                        FillProjectNameCell(htmlRow, project);
                        FillProjectStartCell(e.Item, project);
                        FillProjectEndCell(e.Item, project);
                        FillDivisionCell(e.Item, project);
                        FillPracticeCell(e.Item, project);
                        FillChannelCell(e.Item, project);
                        FillSubChannelCell(e.Item, project);
                        FillRevenueCell(e.Item, project);
                        FillOfferingCell(e.Item, project);
                        if (HostingPageIsProjectsReport != null)
                        {
                            FillStatusCell(e.Item, project);
                            FillNewOrExtCell(e.Item, project);
                            FillSalesPersonCell(e.Item, project);
                        }
                    }

                    FillMonthCells(row, project, personListAnalyzer);
                    FillTotalsCell(row, project, personListAnalyzer);
                }
            }
        }

        private void FillMonthCells(HtmlTableRow row, Project project, SeniorityAnalyzer personListAnalyzer)
        {
            DateTime monthBegin = GetMonthBegin();

            int periodLength = GetPeriodLength();

            // Displaying the interest values (main cell data)
            for (int i = NumberOfFixedColumns, k = 0;
                k < periodLength;
                i++, k++, monthBegin = monthBegin.AddMonths(1))
            {
                DateTime monthEnd = GetMonthEnd(ref monthBegin);

                if (project.ProjectedFinancialsByMonth != null)
                {
                    foreach (KeyValuePair<DateTime, ComputedFinancials> interestValue in
                        project.ProjectedFinancialsByMonth)
                    {
                        if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                        {
                            //row.Cells[i].Wrap = false;
                            if (project.Id.HasValue)
                            {
                                // Project.Id != null is for regular projects only
                                bool greaterSeniorityExists = personListAnalyzer != null && personListAnalyzer.GreaterSeniorityExists;

                                var revenue = CalculationsType == 2 ? interestValue.Value.ActualRevenue : CalculationsType == 1 ? interestValue.Value.Revenue : interestValue.Value.PreviousMonthsActualRevenueValue;
                                var margin = CalculationsType == 2 ? interestValue.Value.ActualGrossMargin : CalculationsType == 1 ? interestValue.Value.GrossMargin : interestValue.Value.PreviousMonthsActualMarginValue;

                                row.Cells[i].InnerHtml = GetMonthReportTableAsHtml(revenue, margin, greaterSeniorityExists);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static DateTime GetMonthEnd(ref DateTime monthBegin)
        {
            return new DateTime(monthBegin.Year,
                    monthBegin.Month,
                    DateTime.DaysInMonth(monthBegin.Year, monthBegin.Month));
        }

        private DateTime GetMonthBegin()
        {
            return new DateTime(diRange.FromDate.Value.Year,
                    diRange.FromDate.Value.Month,
                    Constants.Dates.FirstDay);
        }

        private void FillTotalsCell(HtmlTableRow row, Project project, SeniorityAnalyzer personListAnalyzer)
        {
            // Project totals
            PracticeManagementCurrency totalRevenue = 0M;
            PracticeManagementCurrency totalMargin = 0M;
            totalMargin.FormatStyle = NumberFormatStyle.Margin;

            // Calculate Total Revenue and Margin for current Project
            if (project.ComputedFinancials != null)
            {
                totalRevenue = CalculationsType == 2 ? project.ComputedFinancials.ActualRevenue : CalculationsType == 1 ? project.ComputedFinancials.Revenue : project.ComputedFinancials.PreviousMonthsActualRevenueValue; //CalculationsType == 1 ? project.ComputedFinancials.Revenue : project.ComputedFinancials.ActualRevenue;
                totalMargin = CalculationsType == 2 ? project.ComputedFinancials.ActualGrossMargin : CalculationsType == 1 ? project.ComputedFinancials.GrossMargin : project.ComputedFinancials.PreviousMonthsActualMarginValue; //CalculationsType == 1 ? project.ComputedFinancials.GrossMargin : project.ComputedFinancials.ActualGrossMargin;
            }

            // Render Total Revenue and Margin for current Project
            if (project.Id.HasValue)
            {
                // Displaying the project totals
                bool greaterSeniorityExists = personListAnalyzer != null && personListAnalyzer.GreaterSeniorityExists;

                row.Cells[row.Cells.Count - 1].InnerHtml = GetMonthReportTableAsHtml(totalRevenue, totalMargin, greaterSeniorityExists);
                //string.Format(
                //Resources.Controls.ProjectInterestFormat, totalRevenue, strTotalMargin);
                row.Cells[row.Cells.Count - 1].Attributes["class"] = "CompPerfTotalSummary";
                //if (greaterSeniorityExists)
                //    OneGreaterSeniorityExists = true;
            }
        }

        private bool IsProjectVisible(Project project)
        {
            return
                // Project has no milestones - we should display it anyware
                !project.StartDate.HasValue || !project.EndDate.HasValue ||
                // Project has some milestone(s) and we determine whether it falls into the selected date range
                (project.StartDate.Value <= PeriodEnd && project.EndDate >= PeriodStart);
        }

        private static void FillProjectEndCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Project End date cell content
            var lblEndDate = e.FindControl(LabelEndDateId) as Label;
            if (project.EndDate.HasValue)
            {
                lblEndDate.Text = project.EndDate.Value.ToString("MM/dd/yyyy");
            }
            //var indentDiv = new Panel() { CssClass = "cell-pad" };
            //indentDiv.Controls.Add(lblEndDate);

            //row.Cells[EndDateColumnIndex].Controls.Add(indentDiv);
            //row.Cells[StartDateColumnIndex].Attributes["class"] = "CompPerfPeriod";
        }

        private static void FillStatusCell(ListViewItem e, Project project)
        {
            //var row = e.FindControl("boundingRow") as HtmlTableRow;
            var lblStatus = e.FindControl("lblStatus") as Label;
            lblStatus.Text = project.Status.Name;
        }

        private static void FillNewOrExtCell(ListViewItem e, Project project)
        {
            var lblNewOrExt = e.FindControl("lblNewOrExt") as Label;
            lblNewOrExt.Text = project.BusinessType != BusinessType.Undefined ? DataHelper.GetDescription(project.BusinessType) : string.Empty;

        }

        private static void FillSalesPersonCell(ListViewItem e, Project project)
        {
            var lblSalesPerson = e.FindControl("lblSalesPerson") as Label;
            lblSalesPerson.Text = project.SalesPersonName;

        }

        private static void FillProjectStartCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;

            // Project Start date cell content
            var lblStartDate = e.FindControl(LabelStartDateId) as Label;

            if (project.StartDate.HasValue)
            {
                lblStartDate.Text = project.StartDate.Value.ToString("MM/dd/yyyy");
            }

            //var indentDiv = new Panel() { CssClass = "cell-pad" };
            //indentDiv.Controls.Add(lblStartDate);

            //row.Cells[StartDateColumnIndex].Controls.Add(indentDiv);
            //row.Cells[StartDateColumnIndex].Attributes["class"] = "CompPerfPeriod";
        }

        private void FillProjectStateCell(HtmlTableRow row, string cssClass, ProjectStatus status)
        {
            var toolTip = status.Name;

            if (status.Id == (int)ProjectStatusType.Active)
            {
                if (cssClass == "ActiveProjectWithoutSOW")
                {
                    toolTip = "Active without Attachment";
                }
                else
                {
                    toolTip = "Active with Attachment";
                }
            }

            HtmlAnchor anchor = new HtmlAnchor();
            anchor.Attributes["class"] = cssClass;
            anchor.Attributes["Description"] = toolTip;
            anchor.Attributes["onmouseout"] = "HidePanel();";
            anchor.Attributes["onmouseover"] = "SetTooltipText(this.attributes['Description'].value,this);";
            row.Cells[ProjectStateColumnIndex].Controls.Add(anchor);
        }

        private void FillProjectNameCell(HtmlTableRow row, Project project)
        {
            HtmlAnchor anchor = new HtmlAnchor();
            anchor.InnerText = project.Name;
            anchor.HRef = GetRedirectUrl(project.Id.Value, Constants.ApplicationPages.ProjectDetail);
            anchor.Attributes["Description"] = PrepareToolTipView(project);
            anchor.Attributes["onmouseout"] = "HidePanel();";
            anchor.Attributes["onmouseover"] = "SetTooltipText(this.attributes['Description'].value,this);";

            row.Cells[ProjectNameColumnIndex].Controls.Add(anchor);
        }

        private static string GetRedirectUrl(int argProjectId, string targetUrl)
        {
            return string.Format(Constants.ApplicationPages.DetailRedirectWithReturnFormat,
                                 targetUrl,
                                 argProjectId,
                                 Constants.ApplicationPages.Projects);
        }

        private static void FillClientNameCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Client name cell content
            var btnClient = e.FindControl(ButtonClientNameId) as HyperLink;

            btnClient.Text = project.Client.HtmlEncodedName;
            btnClient.NavigateUrl = GetRedirectUrl(project.Client.Id.Value, Constants.ApplicationPages.ClientDetails);

            // row.Cells[ClientNameColumnIndex].Controls.Add(btnClient);
        }

        private static void FillDivisionCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Division cell content
            var lblDivision = e.FindControl("lblDivision") as Label;

            lblDivision.Text = project.Division != null ? project.Division.Name : string.Empty;


            //row.Cells[ClientNameColumnIndex].Controls.Add(btnClient);
        }

        private static void FillPracticeCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Pracice cell content
            var lblPractice = e.FindControl("lblPractice") as Label;

            lblPractice.Text = project.Practice != null ? project.Practice.Name : string.Empty;


            //row.Cells[ClientNameColumnIndex].Controls.Add(btnClient);
        }

        private static void FillChannelCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Channel cell content
            var lblChannel = e.FindControl("lblChannel") as Label;

            lblChannel.Text = project.Channel != null ? project.Channel.Name : string.Empty;


            //row.Cells[ClientNameColumnIndex].Controls.Add(btnClient);
        }

        private static void FillSubChannelCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // SubChannel cell content
            var lblSubChannel = e.FindControl("lblSubChannel") as Label;

            lblSubChannel.Text = project.SubChannel != null ? project.SubChannel : string.Empty;


            //row.Cells[ClientNameColumnIndex].Controls.Add(btnClient);
        }

        private static void FillRevenueCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // RevenueType cell content
            var lblRevenue = e.FindControl("lblRevenue") as Label;

            lblRevenue.Text = project.RevenueType != null ? project.RevenueType.Name : string.Empty;


            //row.Cells[ClientNameColumnIndex].Controls.Add(btnClient);
        }

        private static void FillOfferingCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Offering cell content
            var lblOffering = e.FindControl("lblOffering") as Label;

            lblOffering.Text = project.Offering != null ? project.Offering.Name : string.Empty;


            //row.Cells[ClientNameColumnIndex].Controls.Add(btnClient);
        }



        private static void FillProjectNumberCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            var lblProjectNumber = e.FindControl(LabelProjectNumberId) as Label;
            lblProjectNumber.Text = HttpUtility.HtmlEncode(project.ProjectNumber);
            row.Cells[ProjectNumberColumnIndex].Controls.Add(lblProjectNumber);
            row.Cells[ProjectNumberColumnIndex].Attributes["class"] = "CompPerfProjectNumber";
        }

        private void BindProjectGrid()
        {
            lvProjects.DataSource = SortProjects(ProjectList);
            lvProjects.DataBind();
        }

        private string GetSortDirection()
        {
            switch (ListViewSortDirection)
            {
                case "Ascending":
                    ListViewSortDirection = "Descending";
                    break;

                case "Descending":
                    ListViewSortDirection = "Ascending";
                    break;
            }
            return ListViewSortDirection;
        }

        private Project[] SortProjects(Project[] projectList)
        {
            if (projectList != null & projectList.Length > 0)
            {
                if (!string.IsNullOrEmpty(PrevListViewSortExpression))
                {
                    var row = lvProjects.FindControl("lvHeader") as System.Web.UI.HtmlControls.HtmlTableRow;
                    var sortExpression = string.Empty;
                    if (row.Cells[ListViewSortColumnId].HasControls())
                    {
                        foreach (var ctrl in row.Cells[ListViewSortColumnId].Controls)
                        {
                            if (ctrl is LinkButton)
                            {
                                var lb = ctrl as LinkButton;
                                sortExpression = lb.Text;
                            }
                        }
                    }
                    var abc = new ProjectComparer(sortExpression);
                    Array.Sort(projectList, abc);
                    if (ListViewSortDirection != "Ascending")
                        Array.Reverse(projectList);
                }
                return projectList;
            }
            return ProjectList;
        }

        protected void custPeriod_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = GetPeriodLength() > 0;
        }

        protected void custPeriodLengthLimit_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = GetPeriodLength() <= MaxPeriodLength;
        }

        protected void btnUpdateView_Click(object sender, EventArgs e)
        {
            ddlPeriod_SelectedIndexChanged();
            ValidateAndDisplay();
        }

        protected void btnClientName_Command(object sender, CommandEventArgs e)
        {
            HostingPageIsProjectsReport.Redirect(string.Format(Constants.ApplicationPages.DetailRedirectFormat,
                Constants.ApplicationPages.ClientDetails,
                e.CommandArgument));
        }

        private static bool IsInMonth(DateTime date, DateTime periodStart, DateTime periodEnd)
        {
            var result =
                (date.Year > periodStart.Year ||
                (date.Year == periodStart.Year && date.Month >= periodStart.Month)) &&
                (date.Year < periodEnd.Year || (date.Year == periodEnd.Year && date.Month <= periodEnd.Month));

            return result;
        }

        private void ValidateAndDisplay()
        {
            // This validator is not applicable here.
            reqSearchText.IsValid = true;

            Page.Validate(valsPerformance.ValidationGroup);
            if (Page.IsValid)
            {
                Display();

                SetddlView();
                lvProjects.DataBind();

                StyledUpdatePanel.Update();
            }
        }

        /// <summary>
        /// Adds to the performance grid one for each the month withing the selected period.
        /// </summary>
        public void Display()
        {
            PreparePeriodView();

            // Clean up the cached values
            CompanyPerformanceState.Clear();
            OneGreaterSeniorityExists = false;
            var personListAnalyzer = new SeniorityAnalyzer(DataHelper.CurrentPerson);

            foreach (var project in ProjectList)
            {
                var greaterSeniorityExists = personListAnalyzer.OneWithGreaterSeniorityExists(project.ProjectPersons);
                if (greaterSeniorityExists)
                {
                    OneGreaterSeniorityExists = greaterSeniorityExists;
                    break;
                }
            }
            SaveFilterSettings();
            // Main GridView
            lvProjects.DataSource = ProjectList;
            lvProjects.DataBind();

            if (!IsPostBack)
            {
                SetddlView();

                lvProjects.DataBind();
            }
        }

        public void RedirectWithBack(string redirectUrl, string backUrl)
        {
            SaveFilterSettings();

        }

        /// <summary>
        /// Computes and displays total values;
        /// </summary>
        private Project CalculateSummaryTotals(
            IList<Project> projects,
            DateTime periodStart,
            DateTime periodEnd)
        {
            int? defaultProjectId = MileStoneConfigurationManager.GetProjectId();
            int defaultProjectIdValue = defaultProjectId.HasValue ? defaultProjectId.Value : 0;
            // Prepare Financial Summary GridView
            var financialSummaryRevenue = new Project
            {
                ProjectedFinancialsByMonth =
                    new Dictionary<DateTime, ComputedFinancials>(),
                ComputedFinancials = new ComputedFinancials()
            };

            for (var dtTemp = periodStart; dtTemp <= periodEnd; dtTemp = dtTemp.AddMonths(1))
            {
                var financials = new ComputedFinancials();

                // Looking through the projects
                foreach (var project in projects)
                {
                    if (project.Id.HasValue && project.Id.Value == defaultProjectIdValue)
                        continue;

                    foreach (var projectFinancials in project.ProjectedFinancialsByMonth)
                    {
                        if (projectFinancials.Key.Year == dtTemp.Year &&
                            projectFinancials.Key.Month == dtTemp.Month &&
                            project.Id.HasValue)
                        {
                            financials.Revenue += CalculationsType == 2 ? projectFinancials.Value.ActualRevenue : CalculationsType == 1 ? projectFinancials.Value.Revenue : projectFinancials.Value.PreviousMonthsActualRevenueValue;
                            financials.GrossMargin += CalculationsType == 2 ? projectFinancials.Value.ActualGrossMargin : CalculationsType == 1 ? projectFinancials.Value.GrossMargin : projectFinancials.Value.PreviousMonthsActualMarginValue;
                        }
                    }
                }

                financialSummaryRevenue.ProjectedFinancialsByMonth.Add(dtTemp, financials);

                var projectsHavingFinancials = projects.Where(project => project.ComputedFinancials != null && project.Id.Value != defaultProjectIdValue);

                if (projectsHavingFinancials != null)
                {
                    financialSummaryRevenue.ComputedFinancials.GrossMargin = projectsHavingFinancials.Sum(proj => CalculationsType == 2 ? proj.ComputedFinancials.ActualGrossMargin : CalculationsType == 1 ? proj.ComputedFinancials.GrossMargin : proj.ComputedFinancials.PreviousMonthsActualMarginValue);
                    financialSummaryRevenue.ComputedFinancials.Revenue = projectsHavingFinancials.Sum(proj => CalculationsType == 2 ? proj.ComputedFinancials.ActualRevenue : CalculationsType == 1 ? proj.ComputedFinancials.Revenue : proj.ComputedFinancials.PreviousMonthsActualRevenueValue);
                }
            }

            return financialSummaryRevenue;
        }

        /// <summary>
        /// Executes preliminary operations to the view be ready to display the data.
        /// </summary>
        private void PreparePeriodView()
        {
            if (!IsPostBack)
            {
                var filter = InitFilter();

                //  If current user is administrator, don't apply restrictions
                var person =
                    (Roles.IsUserInRole(
                        DataHelper.CurrentPerson.Alias,
                        DataTransferObjects.Constants.RoleNames.AdministratorRoleName)
                        || Roles.IsUserInRole(
                        DataHelper.CurrentPerson.Alias,
                        DataTransferObjects.Constants.RoleNames.OperationsRoleName))
                    ? null : DataHelper.CurrentPerson;

                // If person is not administrator, return list of values when [All] is selected
                //      this is needed because we apply restrictions and don't want
                //      NULL to be returned, because that would mean all and restrictions
                //      are not going to be applied
                if (person != null)
                {
                    cblSalesperson.AllSelectedReturnType = ScrollingDropDown.AllSelectedType.AllItems;
                    cblProjectOwner.AllSelectedReturnType = ScrollingDropDown.AllSelectedType.AllItems;
                    cblPractice.AllSelectedReturnType = ScrollingDropDown.AllSelectedType.AllItems;
                    cblClient.AllSelectedReturnType = ScrollingDropDown.AllSelectedType.AllItems;
                    cblProjectGroup.AllSelectedReturnType = ScrollingDropDown.AllSelectedType.AllItems;
                }

                PraticeManagement.Controls.DataHelper.FillSalespersonList(
                    person, cblSalesperson,
                    Resources.Controls.AllSalespersonsText,
                    true);

                PraticeManagement.Controls.DataHelper.FillProjectOwnerList(cblProjectOwner,
                    "All People with Project Access",
                    true,
                    person, true);

                PraticeManagement.Controls.DataHelper.FillPracticeList(
                    person,
                    cblPractice,
                    Resources.Controls.AllPracticesText);

                PraticeManagement.Controls.DataHelper.FillClientsAndGroups(
                    cblClient, cblProjectGroup);

                DataHelper.FillProjectDivisionList(cblDivision, false, true);

                DataHelper.FillChannelList(cblChannel, "All Channels");

                DataHelper.FillOfferingsList(cblOffering, "All Offerings");

                DataHelper.FillRevenueTypeList(cblRevenueType, "All Revenue Types");

                // Set the default viewable interval.
                diRange.FromDate = filter.PeriodStart;
                diRange.ToDate = filter.PeriodEnd;

                //chbPeriodOnly.Checked = filter.TotalOnlySelectedDateWindow;

                chbActive.Checked = filter.ShowActive;
                chbCompleted.Checked = filter.ShowCompleted;
                chbExperimental.Checked = filter.ShowExperimental;
                chbProjected.Checked = filter.ShowProjected;
                chbInternal.Checked = filter.ShowInternal;
                chbInactive.Checked = filter.ShowInactive;
                chbProposed.Checked = filter.ShowProposed;
                chbAtRisk.Checked = filter.ShowAtRisk;

                SelectedClientIds = filter.ClientIdsList;
                SelectedPracticeIds = filter.PracticeIdsList;
                SelectedDivisionIds = filter.DivisionIdsList;
                SelectedChannelIds = filter.ChannelIdsList;
                SelectedOfferingIds = filter.OfferingIdsList;
                SelectedRevenueTypeIds = filter.RevenueTypeIdsList;
                SelectedProjectOwnerIds = filter.ProjectOwnerIdsList;
                SelectedSalespersonIds = filter.SalespersonIdsList;
                SelectedGroupIds = filter.ProjectGroupIdsList;

                ddlPeriod.SelectedIndex = ddlPeriod.Items.IndexOf(ddlPeriod.Items.FindByValue(filter.PeriodSelected.ToString()));
                ddlView.SelectedIndex = ddlView.Items.IndexOf(ddlView.Items.FindByValue(filter.ViewSelected.ToString()));
                ddlCalculateRange.SelectedIndex = ddlCalculateRange.Items.IndexOf(ddlCalculateRange.Items.FindByValue(filter.CalculateRangeSelected.ToString()));

                ddlCalculationsType.SelectedValue = filter.CalculationsType == 0 ? "1" : filter.CalculationsType.ToString();
            }
            else
            {
                Page.Validate(valsPerformance.ValidationGroup);
            }
        }

        private CompanyPerformanceFilterSettings InitFilter()
        {
            if (HostingPageIsProjectsReport == null)
            {
                return SerializationHelper.DeserializeCookie(CompanyPerformanceFilterKey) as CompanyPerformanceFilterSettings ??
                       new CompanyPerformanceFilterSettings();
            }
            else
            {
                return ReportsFilterHelper.GetFilterValues(ReportName.ProjectSummaryReport) as CompanyPerformanceFilterSettings ??
                    new CompanyPerformanceFilterSettings();
            }
        }

        private void AddMonthColumn(HtmlTableRow row, DateTime periodStart, int monthsInPeriod, int insertPosition)
        {
            if (row != null)
            {
                while (row.Cells.Count > NumberOfFixedColumns + 1)
                {
                    row.Cells.RemoveAt(NumberOfFixedColumns);
                }

                for (int i = insertPosition, k = 0; k < monthsInPeriod; i++, k++)
                {
                    var newColumn = new HtmlTableCell("td");
                    row.Cells.Insert(i, newColumn);

                    row.Cells[i].InnerHtml = periodStart.ToString(Constants.Formatting.CompPerfMonthYearFormat);
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary";

                    periodStart = periodStart.AddMonths(1);
                }
            }
        }

        /// <summary>
        /// Prepare ToolTip for project Name cell
        /// </summary>
        /// <param name="project">project</param>
        /// <returns>ToolTip</returns>
        private static string PrepareToolTipView(Project project)
        {
            var resources = new StringBuilder();

            var personList = new List<MilestonePerson>();
            if (project.ProjectPersons != null)
            {
                foreach (var projectPerson in project.ProjectPersons)
                {
                    var personExist = false;
                    if (personList != null)
                    {
                        foreach (var mp in personList)
                            if (mp.Person.Id == projectPerson.Person.Id)
                            {
                                personExist = true;
                                break;
                            }
                    }
                    if (!personExist)
                    {
                        personList.Add(projectPerson);
                    }
                }
            }

            var sortedPersons = personList.OrderBy(k => k.Person.LastName).ThenBy(k => k.Person.FirstName).ToList();

            foreach (var t in sortedPersons)
            {
                resources.AppendFormat(AppendPersonFormat,
                                     "<br/>",
                                     HttpUtility.HtmlEncode(t.Person.LastName),
                                     HttpUtility.HtmlEncode(string.IsNullOrEmpty(t.Person.PrefferedFirstName) ? t.Person.FirstName : t.Person.PrefferedFirstName)
                                     );
            }

            return string.Format(ToolTipView,
                 HttpUtility.HtmlEncode(project.BuyerName),
                 HttpUtility.HtmlEncode(project.SalesPersonName),
                 HttpUtility.HtmlEncode(project.ProjectOwner != null ? project.ProjectOwner.Name : string.Empty),
                 project.SowBudget.HasValue ? project.SowBudget.Value.ToString(CurrencyDisplayFormat) : string.Empty,
                 resources
                );
        }

        /// <summary>
        /// Calculates a length of the selected period in the mounths.
        /// </summary>
        /// <returns>The number of the months within the selected period.</returns>
        private int GetPeriodLength()
        {
            int mounthsInPeriod =
                (diRange.ToDate.Value.Year - diRange.FromDate.Value.Year) * Constants.Dates.LastMonth +
                (diRange.ToDate.Value.Month - diRange.FromDate.Value.Month + 1);
            return mounthsInPeriod;
        }

        private const string ANIMATION_SHOW_SCRIPT =
                        @"<OnClick>
                        	<Sequence>
                        		<Parallel Duration=""0"" AnimationTarget=""{0}"">
                        			<Discrete Property=""style"" propertyKey=""border"" ValuesScript=""['thin solid navy']""/>
                        		</Parallel>
                        		<Parallel Duration="".4"" Fps=""20"" AnimationTarget=""{0}"">
                        			<Resize Width=""250"" Height=""160"" Unit=""px"" />
                        		</Parallel>
                        	</Sequence>
                        </OnClick>";

        private const string ANIMATION_HIDE_SCRIPT =
                        @"<OnClick>
                        	<Sequence>
                        		<Parallel Duration="".4"" Fps=""20"" AnimationTarget=""{0}"">
                        			<Resize Width=""0"" Height=""0"" Unit=""px"" />
                        		</Parallel>
                        		<Parallel Duration=""0"" AnimationTarget=""{0}"">
                        			<Discrete Property=""style"" propertyKey=""border"" ValuesScript=""['none']""/>
                        		</Parallel>
                        	</Sequence>
                        </OnClick>";

        private const string POPULATE_EXTENDER_SERVICEMETHOD = "RenderMonthMiniReport";
        private const string POPULATE_EXTENDER_UPDATINGCSS = "Loading";
        private const string MINIREPORT_CSSCLASS = "MiniReport";
        private const string CORRECT_MINI_REPORT_POS_FUNCTION = "correctMonthMiniReportPosition('{0}', '{1}', '{2}');";

        /// <summary>
        /// Generates a month mini-report view.
        /// </summary>
        /// <param name="e">Grid View header row.</param>
        /// <param name="i">Header cell index.</param>
        private void PopulateMiniReportCell(HtmlTableRow row, int i)
        {
            var pnlHeader = new Panel() { ID = string.Format(PanelHeaderIdFormat, i), CssClass = CompPerfHeaderDivCssClass };

            var lblHeader =
                new Label { Text = row.Cells[i].InnerHtml };
            var lblHeaderHint =
                new Label { Text = "&nbsp;", ID = string.Format(LabelHeaderIdFormat, i), CssClass = HintDivCssClass };

            var pnlReport = new Panel { ID = string.Format(PanelReportIdFormat, i), CssClass = MINIREPORT_CSSCLASS };

            var closeButtonId = string.Format(CloseReportButtonIdFormat, i);
            var tblReport = new Table();
            tblReport.Rows.Add(new TableRow());
            tblReport.Rows[0].Cells.Add(new TableHeaderCell());
            tblReport.Rows[0].Cells[0].HorizontalAlign = HorizontalAlign.Right;
            var closeButton = new HtmlInputButton { ID = closeButtonId, Value = "X" };
            closeButton.Attributes["class"] = "mini-report-close";
            tblReport.Rows[0].Cells[0].Controls.Add(closeButton);

            tblReport.Rows.Add(new TableRow());
            tblReport.Rows[1].Cells.Add(new TableCell { ID = string.Format(ReportCellIdFormat, i) });
            var useActuals = CalculationsType != 1;
            pnlReport.Controls.Add(tblReport);
            pnlReport.Controls.Add(
                new DynamicPopulateExtender
                {
                    PopulateTriggerControlID = lblHeaderHint.ID,
                    TargetControlID = tblReport.Rows[1].Cells[0].ID,
                    ContextKey = lblHeader.Text + "," +
                                this.chbProjected.Checked.ToString() + "," +
                                this.chbCompleted.Checked.ToString() + "," +
                                this.chbActive.Checked.ToString() + "," +
                                this.chbExperimental.Checked.ToString() + "," +
                                this.chbProposed.Checked.ToString() + "," +
                                this.chbInternal.Checked.ToString() + "," +
                                this.chbInactive.Checked.ToString() + "," +
                                this.chbInactive.Checked.ToString() + "," +
                                useActuals.ToString(),

                    ServiceMethod = POPULATE_EXTENDER_SERVICEMETHOD,
                    UpdatingCssClass = POPULATE_EXTENDER_UPDATINGCSS,
                });

            row.Cells[i].InnerHtml = string.Empty;
            pnlHeader.Controls.Add(lblHeader);
            pnlHeader.Controls.Add(lblHeaderHint);
            row.Cells[i].Controls.Add(pnlHeader);
            row.Cells[i].Controls.Add(pnlReport);

            var animShow =
                new AnimationExtender
                {
                    TargetControlID = lblHeaderHint.ID,
                    Animations = string.Format(ANIMATION_SHOW_SCRIPT, pnlReport.ID)
                };

            var animHide =
                new AnimationExtender
                {
                    TargetControlID = closeButtonId,
                    Animations = string.Format(ANIMATION_HIDE_SCRIPT, pnlReport.ID)
                };

            row.Cells[i].Controls.Add(animShow);
            row.Cells[i].Controls.Add(animHide);

            lblHeaderHint.Attributes["onclick"]
                = string.Format(
                    CORRECT_MINI_REPORT_POS_FUNCTION,
                    pnlReport.ClientID,
                    lblHeader.ClientID,
                    StyledUpdatePanel.FindControl("horisontalScrollDiv").ClientID);

            lblHeaderHint.ToolTip = string.Format(LabelHeaderIdToolTipView, lblHeader.Text);
        }

        /// <summary>
        /// Generates a month mini-report.
        /// </summary>
        /// <param name="contextKey">Determines a month to the report be generated for.</param>
        /// <returns>A report rendered to HTML.</returns>
        //[WebMethod]
        //[ScriptMethod]
        public static string RenderMonthMiniReport(string contextKey)
        {
            var result = new StringBuilder();
            string monthYear = string.Empty;
            bool showProjected = false,
                showCompleted = false,
                showActive = false,
                showExperimental = false,
                showProposed = false,
                showInternal = false,
                showInactive = false,
                useActuals = false;

            ExtractFilterValues(
                                contextKey,
                                ref monthYear,
                                ref showProjected,
                                ref showCompleted,
                                ref showActive,
                                ref showExperimental,
                                ref showProposed,
                                ref showInternal,
                                ref showInactive,
                                ref useActuals);

            DateTime requestedMonth;
            if (DateTime.TryParseExact(monthYear,
                Constants.Formatting.CompPerfMonthYearFormat,
                CultureInfo.CurrentUICulture,
                DateTimeStyles.None,
                out requestedMonth))
            {
                using (var serviceClient = new ProjectServiceClient())
                {
                    try
                    {
                        string xml = serviceClient.MonthMiniReport(
                            requestedMonth,
                            HttpContext.Current.User.Identity.Name,
                            showProjected,
                            showCompleted,
                            showActive,
                            showExperimental,
                            showProposed,
                            showInternal,
                            showInactive,
                            useActuals);
                        var transform = new XslCompiledTransform();
                        transform.Load(
                            HttpContext.Current.Server.MapPath(Constants.ReportTemplates.MonthMiniReport));
                        using (TextReader txt = new StringReader(xml))
                        using (XmlReader reader = XmlReader.Create(txt))
                        {
                            using (TextWriter writer = new StringWriter(result))
                            {
                                transform.Transform(reader, new XsltArgumentList(), writer);
                            }
                        }
                    }
                    catch (CommunicationException)
                    {
                        serviceClient.Abort();
                        throw;
                    }
                }
            }

            return result.ToString();
        }

        public static void ExtractFilterValues(
                                                string contextKey,
                                                ref string monthYear,
                                                ref bool showProjected,
                                                ref bool showCompleted,
                                                ref bool showActive,
                                                ref bool showExperimental,
                                                ref bool showProposed,
                                                ref bool showInternal,
                                                ref bool showInactive,
                                                ref bool useActuals
                                                )
        {
            string[] parameters = contextKey.Split(',');

            monthYear = parameters[0];
            showProjected = Boolean.Parse(parameters[1]);
            showCompleted = Boolean.Parse(parameters[2]);
            showActive = Boolean.Parse(parameters[3]);
            showExperimental = Boolean.Parse(parameters[4]);
            showProposed = Boolean.Parse(parameters[5]);
            showInternal = Boolean.Parse(parameters[6]);
            showInactive = Boolean.Parse(parameters[7]);
            useActuals = Boolean.Parse(parameters[8]);
        }

        protected void btnView_Command(object sender, CommandEventArgs e)
        {
            ((WebControl)((Control)sender).Parent).CssClass = "SelectedSwitch";
        }

        #endregion Methods

        private void FillSummaryTotalRow(int periodLength, Project summary, System.Web.UI.HtmlControls.HtmlTableRow row)
        {
            DateTime monthBegin = GetMonthBegin();

            // Displaying the interest values (main cell data)
            for (int i = 1, k = 0;
                k < periodLength;
                i++, k++, monthBegin = monthBegin.AddMonths(1))
            {
                DateTime monthEnd = GetMonthEnd(ref monthBegin);

                foreach (KeyValuePair<DateTime, ComputedFinancials> interestValue in
                    summary.ProjectedFinancialsByMonth)
                {
                    if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                    {
                        row.Cells[i].InnerHtml = GetMonthReportTableAsHtml(interestValue.Value.Revenue, interestValue.Value.GrossMargin, OneGreaterSeniorityExists);

                        //string.Format(Resources.Controls.ProjectInterestFormat,
                        //interestValue.Value.Revenue,
                        //interestValue.Value.GrossMargin);
                    }
                }
            }

            // Project totals
            PracticeManagementCurrency totalRevenue = 0M;
            PracticeManagementCurrency totalMargin = 0M;
            totalMargin.FormatStyle = NumberFormatStyle.Margin;

            // Calculate Total Revenue and Margin for current Project
            if (summary.ComputedFinancials != null)
            {
                totalRevenue = summary.ComputedFinancials.Revenue;
                totalMargin = summary.ComputedFinancials.GrossMargin;
            }
            row.Cells[row.Cells.Count - 1].InnerHtml = GetMonthReportTableAsHtml(totalRevenue, totalMargin, OneGreaterSeniorityExists);
        }

        #region Month table from resources

        private Table GetMonthReportTable(PracticeManagementCurrency revenue, PracticeManagementCurrency margin, bool greaterSeniorityExists)
        {
            margin.FormatStyle = NumberFormatStyle.Margin;
            //var marginText = greaterSeniorityExists ? Resources.Controls.HiddenCellText : margin.Value.ToString(CurrencyDisplayFormat);
            var reportTable = new Table() { Width = Unit.Percentage(100) };
            var tr = new TableRow() { CssClass = "Revenue" };
            tr.Cells.Add(new TableCell() { HorizontalAlign = HorizontalAlign.Left, Text = "" });

            tr.Cells.Add(new TableCell() { HorizontalAlign = HorizontalAlign.Right, Text = string.Format(PracticeManagementCurrency.RevenueFormat, revenue.Value.ToString(CurrencyDisplayFormat)) });
            reportTable.Rows.Add(tr);
            tr = new TableRow();// { CssClass = "Margin" };
            tr.Cells.Add(new TableCell() { HorizontalAlign = HorizontalAlign.Left, Text = "" });
            tr.Cells.Add(new TableCell()
            {
                HorizontalAlign = HorizontalAlign.Right,
                Text = margin.ToString(greaterSeniorityExists)//as part of #2786.
            });
            reportTable.Rows.Add(tr);

            return reportTable;
        }

        private string GetMonthReportTableAsHtml(PracticeManagementCurrency revenue, PracticeManagementCurrency margin, bool greaterSeniorityExists)
        {
            string outterHtml = string.Empty;

            var stringWriter = new System.IO.StringWriter();

            var table = GetMonthReportTable(revenue, margin, greaterSeniorityExists);

            var div = new Panel() { CssClass = "cell-pad" };
            div.Controls.Add(table);
            using (HtmlTextWriter wr = new HtmlTextWriter(stringWriter))
            {
                div.RenderControl(wr);
                outterHtml = stringWriter.ToString();
            }

            return outterHtml;
        }

        #endregion Month table from resources

        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {
            DataHelper.InsertExportActivityLogMessage("Projects");
            //var a = ExportProjectList.First(p => p.Id == 6729);

            var projectsData = (from pro in ExportProjectList
                                where pro != null
                                select new
                                {
                                    ProjectID = pro.Id != null ? pro.Id.ToString() : string.Empty,
                                    ProjectNumber = pro.ProjectNumber != null ? pro.ProjectNumber.ToString() : string.Empty,
                                    Account = (pro.Client != null && pro.Client.Name != null) ? pro.Client.Name.ToString() : string.Empty,
                                    HouseAccount = (pro.Client != null && pro.Client.IsHouseAccount == true) ? "Yes" : string.Empty,
                                    BusinessGroup = (pro.BusinessGroup != null && pro.BusinessGroup.Name != null) ? pro.BusinessGroup.Name : string.Empty,
                                    BusinessUnit = (pro.Group != null && pro.Group.Name != null) ? pro.Group.Name : string.Empty,
                                    Buyer = pro.BuyerName != null ? pro.BuyerName : string.Empty,
                                    ProjectName = pro.Name != null ? pro.Name : string.Empty,
                                    BusinessType = (pro.BusinessType != (BusinessType)0) ? DataHelper.GetDescription(pro.BusinessType).ToString() : string.Empty,
                                    Status = (pro.Status != null && pro.Status.Name != null) ? pro.Status.Name.ToString() : string.Empty,
                                    StartDate = pro.StartDate.HasValue ? pro.StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                    EndDate = pro.EndDate.HasValue ? pro.EndDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                    Divsion = (pro.Division != null && pro.Division.Name != null) ? pro.Division.Name : string.Empty,
                                    PracticeArea = (pro.Practice != null && pro.Practice.Name != null) ? pro.Practice.Name : string.Empty,
                                    channel = (pro.Channel != null && pro.Channel.Name != null) ? pro.Channel.Name : string.Empty,
                                    subchannel = (pro.SubChannel != null) ? pro.SubChannel : string.Empty,
                                    offering = (pro.Offering != null && pro.Offering.Name != null) ? pro.Offering.Name : string.Empty,
                                    revenue = (pro.RevenueType != null && pro.RevenueType.Name != null) ? pro.RevenueType.Name : string.Empty,
                                    Capabilities = (pro.Capabilities != null && pro.Capabilities != string.Empty) ? pro.Capabilities.TrimEnd(',', ' ') : string.Empty,
                                    Type = Revenue,
                                    Salesperson = (pro.SalesPersonName != null) ? pro.SalesPersonName : string.Empty,
                                    ProjectManager = (pro.ProjectOwner != null) ? pro.ProjectOwner.Name : string.Empty,
                                    SeniorManager = (pro.SeniorManagerName != null) ? pro.SeniorManagerName : string.Empty,
                                    Director = (pro.Director != null && pro.Director.Name != null) ? pro.Director.Name.ToString() : string.Empty,
                                    PricingList = (pro.PricingList != null && pro.PricingList.Name != null) ? pro.PricingList.Name : string.Empty,
                                    PONumber = (pro.PONumber != null && pro.PONumber != null) ? pro.PONumber : string.Empty,
                                    ClientTimeEntryRequired = (pro.IsClientTimeEntryRequired ? "Yes" : "No"),
                                    PreviousProject = pro.PreviousProject != null ? pro.PreviousProject.ProjectNumber : string.Empty,
                                    OutsourceId = (pro.OutsourceId != 3 && pro.OutsourceId != 0) ? DataHelper.GetDescription((OutsourceId)pro.OutsourceId) : string.Empty
                                }).ToList();//Note: If you add any extra property to this anonymous type object then change insertPosition of month cells in RowDataBound.

            var projectsDataWithMargin = (from pro in ExportProjectList
                                          where pro != null
                                          select new
                                          {
                                              ProjectID = pro.Id != null ? pro.Id.ToString() : string.Empty,
                                              ProjectNumber = pro.ProjectNumber != null ? pro.ProjectNumber.ToString() : string.Empty,
                                              Account = (pro.Client != null && pro.Client.Name != null) ? pro.Client.Name.ToString() : string.Empty,
                                              HouseAccount = (pro.Client != null && pro.Client.IsHouseAccount == true) ? "Yes" : string.Empty,
                                              BusinessGroup = (pro.BusinessGroup != null && pro.BusinessGroup.Name != null) ? pro.BusinessGroup.Name : string.Empty,
                                              BusinessUnit = (pro.Group != null && pro.Group.Name != null) ? pro.Group.Name : string.Empty,
                                              Buyer = pro.BuyerName != null ? pro.BuyerName : string.Empty,
                                              ProjectName = pro.Name != null ? pro.Name : string.Empty,
                                              BusinessType = (pro.BusinessType != (BusinessType)0) ? DataHelper.GetDescription(pro.BusinessType) : string.Empty,
                                              Status = (pro.Status != null && pro.Status.Name != null) ? pro.Status.Name.ToString() : string.Empty,
                                              StartDate = pro.StartDate.HasValue ? pro.StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                              EndDate = pro.EndDate.HasValue ? pro.EndDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                              Divsion = (pro.Division != null && pro.Division.Name != null) ? pro.Division.Name : string.Empty,
                                              PracticeArea = (pro.Practice != null && pro.Practice.Name != null) ? pro.Practice.Name : string.Empty,
                                              channel = (pro.Channel != null && pro.Channel.Name != null) ? pro.Channel.Name : string.Empty,
                                              subchannel = (pro.SubChannel != null) ? pro.SubChannel : string.Empty,
                                              offering = (pro.Offering != null && pro.Offering.Name != null) ? pro.Offering.Name : string.Empty,
                                              revenue = (pro.RevenueType != null && pro.RevenueType.Name != null) ? pro.RevenueType.Name : string.Empty,
                                              Capabilities = (pro.Capabilities != null && pro.Capabilities != string.Empty) ? pro.Capabilities.TrimEnd(',', ' ') : string.Empty,
                                              Type = Margin,
                                              Salesperson = (pro.SalesPersonName != null) ? pro.SalesPersonName : string.Empty,
                                              ProjectManager = (pro.ProjectOwner != null) ? pro.ProjectOwner.Name : string.Empty,
                                              SeniorManager = (pro.SeniorManagerName != null) ? pro.SeniorManagerName : string.Empty,
                                              Director = (pro.Director != null && pro.Director.Name != null) ? pro.Director.Name.ToString() : string.Empty,
                                              PricingList = (pro.PricingList != null && pro.PricingList.Name != null) ? pro.PricingList.Name : string.Empty,
                                              PONumber = (pro.PONumber != null && pro.PONumber != null) ? pro.PONumber : string.Empty,
                                              ClientTimeEntryRequired = (pro.IsClientTimeEntryRequired ? "Yes" : "No"),
                                              PreviousProject = pro.PreviousProject != null ? pro.PreviousProject.ProjectNumber : string.Empty,
                                              OutsourceId = pro.OutsourceId != 3 && pro.OutsourceId != 0 ? DataHelper.GetDescription((OutsourceId)pro.OutsourceId) : string.Empty
                                          }).ToList();

            projectsData.AddRange(projectsDataWithMargin);
            projectsData = projectsData.OrderBy(s => (s.Status == ProjectStatusType.Projected.ToString()) ? s.StartDate : s.EndDate).ThenBy(s => s.ProjectNumber).ThenByDescending(s => s.Type).ToList();

            renderMonthColumns = true;
            var data = PrepareDataTable(ExportProjectList, (object[])projectsData.ToArray(), false);
            var dataActual = PrepareDataTable(ExportProjectList, (object[])projectsData.ToArray(), true);

            string dateRangeTitle = string.Format(ExportDateRangeFormat, diRange.FromDate.Value.ToShortDateString(), diRange.ToDate.Value.ToShortDateString());
            DataTable header = new DataTable();
            header.Columns.Add(dateRangeTitle);
            headerRowsCount = header.Rows.Count + 3;
            List<SheetStyles> sheetStylesList = new List<SheetStyles>();
            coloumnsCount = data.Columns.Count;
            sheetStylesList.Add(HeaderSheetStyle);
            sheetStylesList.Add(DataSheetStyle);
            sheetStylesList.Add(HeaderSheetStyle);
            sheetStylesList.Add(DataSheetStyle);

            var dataSetList = new List<DataSet>();
            var dataset = new DataSet();
            dataset.DataSetName = "Summary - Projected";
            dataset.Tables.Add(header);
            dataset.Tables.Add(data);
            dataSetList.Add(dataset);

            var datasetActual = new DataSet();
            datasetActual.Tables.Add(header.Clone());
            datasetActual.Tables.Add(dataActual);
            datasetActual.DataSetName = "Summary - Actuals";
            dataSetList.Add(datasetActual);
            var fileName = HostingPageIsProjectSummary != null ? "Projects.xls" : "ProjectSummaryReport.xls";
            NPOIExcel.Export(fileName, dataSetList, sheetStylesList);
        }

        private string GetProjectManagers(List<Person> list)
        {
            string names = string.Empty;
            foreach (var person in list)
            {
                names += person.Name + "\n";
            }

            names = names.Remove(names.LastIndexOf("\n"));
            return names;
        }

        private DataTable PrepareDataTable(Project[] projectsList, Object[] propertyBags, bool useActuals)
        {
            var periodStart = GetMonthBegin();
            var monthsInPeriod = GetPeriodLength();

            DataTable data = new DataTable();

            data.Columns.Add("Project Number");
            data.Columns.Add("Account");
            data.Columns.Add("House Account");
            data.Columns.Add("Business Group");
            data.Columns.Add("Business Unit");
            data.Columns.Add("Buyer");
            data.Columns.Add("Project Name");
            data.Columns.Add("New/Extension");
            data.Columns.Add("Status");
            data.Columns.Add("Start Date");
            data.Columns.Add("End Date");
            data.Columns.Add("Division");
            data.Columns.Add("Practice Area");
            data.Columns.Add("Channel");
            data.Columns.Add("Channel-Sub");
            data.Columns.Add("Offering");
            data.Columns.Add("RevenueType");
            data.Columns.Add("Capabilities");
            data.Columns.Add("Type");
            //Add Month and Total columns.
            if (renderMonthColumns)
            {
                for (int i = 0; i < monthsInPeriod; i++)
                {
                    data.Columns.Add(periodStart.AddMonths(i).ToString(Constants.Formatting.EntryDateFormat));
                }
            }
            data.Columns.Add("Total");
            data.Columns.Add("Salesperson");
            data.Columns.Add("Project Manager");
            data.Columns.Add("Engagement Manager");
            data.Columns.Add("Executive in Charge");
            data.Columns.Add("Pricing List");
            data.Columns.Add("PO Number");
            data.Columns.Add("Client Time Entry Required");
            data.Columns.Add("Previous Project Number");
            data.Columns.Add("Outsource Id Indicator");
            foreach (var propertyBag in propertyBags)
            {
                var objects = new object[data.Columns.Count];
                int column = 0;
                Project project = new Project();
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(propertyBag))
                {
                    if (property.Name != "ProjectID")
                    {
                        objects[column] = (property.GetValue(propertyBag) == Revenue) ? ServiceRevenue : property.GetValue(propertyBag);
                        if (property.Name == "ProjectNumber")
                        {
                            project = projectsList.Where(p => p.ProjectNumber == property.GetValue(propertyBag).ToString()).FirstOrDefault();
                        }
                        if (property.Name == "Type")
                        {
                            bool isMargin = property.GetValue(propertyBag).ToString() == Margin;
                            //Add month columns.

                            SeniorityAnalyzer personListAnalyzer = new SeniorityAnalyzer(DataHelper.CurrentPerson);
                            personListAnalyzer.OneWithGreaterSeniorityExists(project.ProjectPersons);
                            bool greaterSeniorityExists = personListAnalyzer != null && personListAnalyzer.GreaterSeniorityExists;
                            //PracticeManagementCurrency columnValue = 0M;
                            //columnValue.FormatStyle = marginType ? NumberFormatStyle.Margin : NumberFormatStyle.Revenue;

                            var columnValue = 0M;
                            if (renderMonthColumns)
                            {
                                var monthStart = periodStart;
                                // Displaying the month values (main cell data)
                                for (int k = 0;
                                    k < monthsInPeriod;
                                    k++, monthStart = monthStart.AddMonths(1))
                                {
                                    column++;
                                    columnValue = 0M;
                                    DateTime monthEnd = GetMonthEnd(ref monthStart);

                                    if (project.ProjectedFinancialsByMonth != null)
                                    {
                                        foreach (KeyValuePair<DateTime, ComputedFinancials> interestValue in
                                            project.ProjectedFinancialsByMonth)
                                        {
                                            if (IsInMonth(interestValue.Key, monthStart, monthEnd))
                                            {
                                                columnValue = isMargin ? (useActuals ? interestValue.Value.ActualGrossMargin : interestValue.Value.GrossMargin) : (useActuals ? interestValue.Value.ActualRevenue : interestValue.Value.Revenue);
                                                break;
                                            }
                                        }
                                    }

                                    string color = columnValue < 0 ? "red" : isMargin ? "purple" : "green";
                                    objects[column] = string.Format(NPOIExcel.CustomColorKey, color, greaterSeniorityExists && isMargin ? (object)"(Hidden)" : columnValue);
                                }
                            }

                            column++;
                            columnValue = 0M;
                            if (project.ComputedFinancials != null && !greaterSeniorityExists)
                            {
                                columnValue = isMargin ? (useActuals ? project.ComputedFinancials.ActualGrossMargin : project.ComputedFinancials.GrossMargin) : (useActuals ? project.ComputedFinancials.ActualRevenue : project.ComputedFinancials.Revenue);
                            }
                            string totalColomncolor = columnValue < 0 ? "red" : isMargin ? "purple" : "green";
                            objects[column] = string.Format(NPOIExcel.CustomColorKey, totalColomncolor, greaterSeniorityExists && isMargin ? (object)"(Hidden)" : columnValue);
                        }
                        else if (property.Name == "ProjectManagers")
                        {
                            objects[column] = project.ProjectManagers != null ? GetProjectManagers(project.ProjectManagers) : string.Empty;
                        }
                        column++;
                    }
                }

                data.Rows.Add(objects);
            }

            return data;
        }

        protected void btnExportAllToExcel_Click(object sender, EventArgs e)
        {
            DataHelper.InsertExportActivityLogMessage("Projects");

            var projectsList = ExportAllProjectList.OrderBy(proj => proj.Id);

            var projectsData = (from pro in projectsList
                                where pro != null
                                select new
                                {
                                    ProjectID = pro.Id != null ? pro.Id.ToString() : string.Empty,
                                    ProjectNumber = pro.ProjectNumber != null ? pro.ProjectNumber.ToString() : string.Empty,
                                    Account = (pro.Client != null && pro.Client.Name != null) ? pro.Client.Name.ToString() : string.Empty,
                                    HouseAccount = (pro.Client != null && pro.Client.IsHouseAccount == true) ? "Yes" : string.Empty,
                                    BusinessGroup = (pro.BusinessGroup != null && pro.BusinessGroup.Name != null) ? pro.BusinessGroup.Name : string.Empty,
                                    BusinessUnit = (pro.Group != null && pro.Group.Name != null) ? pro.Group.Name : string.Empty,
                                    Buyer = pro.BuyerName != null ? pro.BuyerName : string.Empty,
                                    ProjectName = pro.Name != null ? pro.Name : string.Empty,
                                    BusinessType = (pro.BusinessType != (BusinessType)0) ? DataHelper.GetDescription(pro.BusinessType).ToString() : string.Empty,
                                    Status = (pro.Status != null && pro.Status.Name != null) ? pro.Status.Name.ToString() : string.Empty,
                                    StartDate = pro.StartDate.HasValue ? pro.StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                    EndDate = pro.EndDate.HasValue ? pro.EndDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                    Divsion = (pro.Division != null && pro.Division.Name != null) ? pro.Division.Name : string.Empty,
                                    PracticeArea = (pro.Practice != null && pro.Practice.Name != null) ? pro.Practice.Name : string.Empty,

                                    channel = (pro.Channel != null && pro.Channel.Name != null) ? pro.Channel.Name : string.Empty,
                                    subchannel = (pro.SubChannel != null) ? pro.SubChannel : string.Empty,
                                    offering = (pro.Offering != null && pro.Offering.Name != null) ? pro.Offering.Name : string.Empty,
                                    revenue = (pro.RevenueType != null && pro.RevenueType.Name != null) ? pro.RevenueType.Name : string.Empty,
                                    Capabilities = (pro.Capabilities != null && pro.Capabilities != string.Empty) ? pro.Capabilities.TrimEnd(',', ' ') : string.Empty,
                                    Type = Revenue,
                                    Salesperson = (pro.SalesPersonName != null) ? pro.SalesPersonName : string.Empty,
                                    ProjectManager = (pro.ProjectOwner != null) ? pro.ProjectOwner.Name : string.Empty,
                                    SeniorManager = (pro.SeniorManagerName != null) ? pro.SeniorManagerName : string.Empty,
                                    Director = (pro.Director != null && pro.Director.Name != null) ? pro.Director.Name.ToString() : string.Empty,
                                    PricingList = (pro.PricingList != null && pro.PricingList.Name != null) ? pro.PricingList.Name : string.Empty,
                                    PONumber = (pro.PONumber != null && pro.PONumber != null) ? pro.PONumber : string.Empty,
                                    ClientTimeEntryRequired = (pro.IsClientTimeEntryRequired ? "Yes" : "No"),
                                    PreviousProject = pro.PreviousProject != null ? pro.PreviousProject.ProjectNumber : string.Empty,
                                    OutsourceId = pro.OutsourceId != 3 ? DataHelper.GetDescription((OutsourceId)pro.OutsourceId) : string.Empty
                                }).ToList();//Note:- Change insertPosition Of Total cell in RowDataBound if any modifications in projectsData.

            var projectsDataWithMargin = (from pro in projectsList
                                          where pro != null
                                          select new
                                          {
                                              ProjectID = pro.Id != null ? pro.Id.ToString() : string.Empty,
                                              ProjectNumber = pro.ProjectNumber != null ? pro.ProjectNumber.ToString() : string.Empty,
                                              Account = (pro.Client != null && pro.Client.Name != null) ? pro.Client.Name.ToString() : string.Empty,
                                              HouseAccount = (pro.Client != null && pro.Client.IsHouseAccount == true) ? "Yes" : string.Empty,
                                              BusinessGroup = (pro.BusinessGroup != null && pro.BusinessGroup.Name != null) ? pro.BusinessGroup.Name : string.Empty,
                                              BusinessUnit = (pro.Group != null && pro.Group.Name != null) ? pro.Group.Name : string.Empty,
                                              Buyer = pro.BuyerName != null ? pro.BuyerName : string.Empty,
                                              ProjectName = pro.Name != null ? pro.Name : string.Empty,
                                              BusinessType = (pro.BusinessType != (BusinessType)0) ? DataHelper.GetDescription(pro.BusinessType).ToString() : string.Empty,
                                              Status = (pro.Status != null && pro.Status.Name != null) ? pro.Status.Name.ToString() : string.Empty,
                                              StartDate = pro.StartDate.HasValue ? pro.StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                              EndDate = pro.EndDate.HasValue ? pro.EndDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                              Divsion = (pro.Division != null && pro.Division.Name != null) ? pro.Division.Name : string.Empty,
                                              PracticeArea = (pro.Practice != null && pro.Practice.Name != null) ? pro.Practice.Name : string.Empty,

                                              channel = (pro.Channel != null && pro.Channel.Name != null) ? pro.Channel.Name : string.Empty,
                                              subchannel = (pro.SubChannel != null) ? pro.SubChannel : string.Empty,
                                              offering = (pro.Offering != null && pro.Offering.Name != null) ? pro.Offering.Name : string.Empty,
                                              revenue = (pro.RevenueType != null && pro.RevenueType.Name != null) ? pro.RevenueType.Name : string.Empty,
                                              Capabilities = (pro.Capabilities != null && pro.Capabilities != string.Empty) ? pro.Capabilities.TrimEnd(',', ' ') : string.Empty,
                                              Type = Margin,
                                              Salesperson = (pro.SalesPersonName != null) ? pro.SalesPersonName : string.Empty,
                                              ProjectManager = (pro.ProjectOwner != null) ? pro.ProjectOwner.Name : string.Empty,
                                              SeniorManager = (pro.SeniorManagerName != null) ? pro.SeniorManagerName : string.Empty,
                                              Director = (pro.Director != null && pro.Director.Name != null) ? pro.Director.Name.ToString() : string.Empty,
                                              PricingList = (pro.PricingList != null && pro.PricingList.Name != null) ? pro.PricingList.Name : string.Empty,
                                              PONumber = (pro.PONumber != null && pro.PONumber != null) ? pro.PONumber : string.Empty,
                                              ClientTimeEntryRequired = (pro.IsClientTimeEntryRequired ? "Yes" : "No"),
                                              PreviousProject = pro.PreviousProject != null ? pro.PreviousProject.ProjectNumber : string.Empty,
                                              OutsourceId = pro.OutsourceId != 3 ? DataHelper.GetDescription((OutsourceId)pro.OutsourceId) : string.Empty
                                          }).ToList();

            projectsData.AddRange(projectsDataWithMargin);
            projectsData = projectsData.OrderBy(s => s.ProjectID).ThenByDescending(s => s.Type).ToList();

            renderMonthColumns = false;
            var data = PrepareDataTable(projectsList.ToArray(), (object[])projectsData.ToArray(), false);
            var dataActual = PrepareDataTable(projectsList.ToArray(), (object[])projectsData.ToArray(), true);

            List<SheetStyles> sheetStylesList = new List<SheetStyles>();

            sheetStylesList.Add(DataSheetStyle);
            sheetStylesList.Add(DataSheetStyle);

            var dataSetList = new List<DataSet>();
            var dataset = new DataSet();
            dataset.DataSetName = "Summary - Projected";
            dataset.Tables.Add(data);
            dataSetList.Add(dataset);

            var datasetActual = new DataSet();
            datasetActual.Tables.Add(dataActual);
            datasetActual.DataSetName = "Summary - Actuals";
            dataSetList.Add(datasetActual);
            NPOIExcel.Export("Projects.xls", dataSetList, sheetStylesList);
        }

        private Project[] GetProjectListAll()
        {
            using (var serviceClient = new ProjectService.ProjectServiceClient())
            {
                return serviceClient.AllProjectsWithFinancialTotalsAndPersons();
            }
        }

        private void FormatExcelReport(GridView projectsGrid)
        {
            foreach (GridViewRow row in projectsGrid.Rows)
            {
                SeniorityAnalyzer personListAnalyzer = null;
                personListAnalyzer = new SeniorityAnalyzer(DataHelper.CurrentPerson);
                personListAnalyzer.OneWithGreaterSeniorityExists(DataHelper.GetPersonsInMilestone(new Project { Id = Convert.ToInt32(row.Cells[0].Text) }));
                bool greaterSeniorityExists = personListAnalyzer != null && personListAnalyzer.GreaterSeniorityExists;

                row.Cells[0].Visible = false;

                Decimal revenueValue = Convert.ToDecimal(row.Cells[9].Text);
                if (revenueValue < 0)
                {
                    row.Cells[9].ForeColor = Color.Red;
                    row.Cells[9].Text = revenueValue.ToString(CurrencyExcelReportFormat);
                }
                else
                {
                    row.Cells[9].ForeColor = Color.Green;
                    row.Cells[9].Text = revenueValue.ToString(CurrencyExcelReportFormat);
                }

                Decimal marginValue = Convert.ToDecimal(row.Cells[10].Text);
                if (greaterSeniorityExists)
                {
                    row.Cells[10].Text = Resources.Controls.HiddenCellText;
                    row.Cells[10].ForeColor = Color.Purple;
                }
                else if (marginValue < 0)
                {
                    row.Cells[10].ForeColor = Color.Red;
                    row.Cells[10].Text = marginValue.ToString(CurrencyExcelReportFormat);
                }
                else
                {
                    row.Cells[10].ForeColor = Color.Purple;
                    row.Cells[10].Text = marginValue.ToString(CurrencyExcelReportFormat);
                }
            }
        }

        protected void lvProjects_Sorted(object sender, EventArgs e)
        {
            var row = lvProjects.FindControl("lvHeader") as System.Web.UI.HtmlControls.HtmlTableRow;
            for (int i = 1; i < row.Cells.Count; i++)
            {
                HtmlTableCell cell = row.Cells[i];

                if (cell.HasControls())
                {
                    foreach (var ctrl in cell.Controls)
                    {
                        if (ctrl is LinkButton)
                        {
                            var lb = ctrl as LinkButton;
                            lb.CssClass = "arrow";
                            if (i == ListViewSortColumnId)
                            {
                                lb.CssClass += string.Format(" sort-{0}", ListViewSortDirection == "Ascending" ? "up" : "down");
                            }
                        }
                    }
                }
            }
        }

        protected void lvProjects_Sorting(object sender, ListViewSortEventArgs e)
        {
            if (PrevListViewSortExpression != e.SortExpression)
            {
                PrevListViewSortExpression = e.SortExpression;
                ListViewSortDirection = e.SortDirection.ToString();
            }
            else
            {
                ListViewSortDirection = GetSortDirection();
            }

            ListViewSortColumnId = GetSortColumnId(e.SortExpression);
            BindProjectGrid();
        }

        private int GetSortColumnId(string sortExpression)
        {
            int sortColumn = -1;
            return int.TryParse(sortExpression, out sortColumn) ? sortColumn : ProjectNumberColumnIndex;
        }

        protected void lvProjects_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            var dpProject = GetPager();
            dpProject.SetPageProperties(e.StartRowIndex, e.MaximumRows, false);

            lvProjects.DataSource = ProjectList;
            lvProjects.DataBind();
        }

        protected void lvProjects_OnDataBound(object sender, EventArgs e)
        {

            if (HostingPageIsProjectsReport == null)
            {

                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "HideColumns();", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "ShowColumns();", true);
            }
            SetHeaderMonths();
            var pager = GetPager();
            if (pager != null)
            {
                pager.Visible = (pager.PageSize < pager.TotalRowCount);
                lblPageCount.Text = string.Format(PageViewCountFormat, (pager.StartRowIndex + 1), (pager.StartRowIndex + GetCurrentPageCount()), GetTotalCount());
            }
            else
            {
                lblPageCount.Text = string.Format(PageViewCountFormat, 0, 0, 0);
            }
        }

        private void SetHeaderMonths()
        {
            var row = lvProjects.FindControl("lvHeader") as System.Web.UI.HtmlControls.HtmlTableRow;
            if (row != null)
            {
                var periodStart = diRange.FromDate.Value;
                var monthsInPeriod = GetPeriodLength();
                Page.Validate(valsPerformance.ValidationGroup);
                if (!IsPostBack || Page.IsValid)
                    AddMonthColumn(row, periodStart, monthsInPeriod, NumberOfFixedColumns);

                string totalHeaderText = string.Format(TotalHeaderFormat, ddlCalculateRange.SelectedItem.Text);
                var div = new Panel() { CssClass = CompPerfHeaderDivCssClass };
                div.Controls.Add(new Label() { Text = totalHeaderText });

                var stringWriter = new System.IO.StringWriter();
                using (HtmlTextWriter wr = new HtmlTextWriter(stringWriter))
                {
                    div.RenderControl(wr);
                    var s = stringWriter.ToString();
                    row.Cells[row.Cells.Count - 1].InnerHtml = s;
                }

                for (int i = NumberOfFixedColumns; i < row.Cells.Count - 1; i++)
                {
                    PopulateMiniReportCell(row, i);
                }

                // fill summary
                row = lvProjects.FindControl("lvSummary") as System.Web.UI.HtmlControls.HtmlTableRow;
                var tdSummary = row.FindControl("tdSummary") as System.Web.UI.HtmlControls.HtmlTableCell;
                tdSummary.ColSpan = HostingPageIsProjectsReport != null ? 15 : 6;
                while (row.Cells.Count > 1)
                {
                    row.Cells.RemoveAt(1);
                }

                for (int i = 0; i < monthsInPeriod + 1; i++)   // + 1 means a cell for total column
                {
                    var td = new HtmlTableCell() { };
                    td.Attributes["class"] = "CompPerfMonthSummary";
                    row.Cells.Insert(row.Cells.Count, td);
                }
                var summary = CalculateSummaryTotals(ProjectList, periodStart, PeriodEnd);

                FillSummaryTotalRow(monthsInPeriod, summary, row);
            }
        }

        protected void Pager_PagerCommand(object sender, DataPagerCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case PagerNextCommand:
                    int nextPageStartIndex = e.Item.Pager.StartRowIndex + e.Item.Pager.PageSize;
                    if (nextPageStartIndex <= e.TotalRowCount)
                    {
                        e.NewStartRowIndex = nextPageStartIndex;
                        e.NewMaximumRows = e.Item.Pager.MaximumRows;
                    }
                    break;

                case PagerPrevCommand:
                    int prevPageStartIndex = e.Item.Pager.StartRowIndex - e.Item.Pager.PageSize;
                    if (prevPageStartIndex >= 0)
                    {
                        e.NewStartRowIndex = prevPageStartIndex;
                        e.NewMaximumRows = e.Item.Pager.MaximumRows;
                    }
                    break;

                default:
                    throw new ArgumentException(
                        string.Format(
                            "Cannot process the command '{0}'. Expected = 'Prev, Next'",
                            e.CommandName));
            }
        }

        protected bool IsNeedToShowNextButton()
        {
            int currentRecords = GetCurrentPageCount();
            var pager = GetPager();
            //return pager.StartRowIndex + pager.PageSize <= pager.TotalRowCount;

            if (ddlView.SelectedValue == "1")
            {
                return false;
            }
            else
            {
                return !((lvProjects.Items.Count == 0) || (currentRecords == GetTotalCount()) || (currentRecords < Convert.ToInt32(ddlView.SelectedValue)));
            }
        }

        protected bool IsNeedToShowPrevButton()
        {
            return GetPager().StartRowIndex != 0;
        }

        private DataPager GetPager()
        {
            return dpProjects;
        }

        public int GetCurrentPageCount()
        {
            return lvProjects.Items.Count;
        }

        public int GetTotalCount()
        {
            return GetPager().TotalRowCount;
        }
    }
}

