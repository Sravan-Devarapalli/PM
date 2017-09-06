using System;
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
using System.Web.Security;
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
        private const int numberOfFixedCol = 28;
        private const int ProjectStateColumnIndex = 0;
        private const int ProjectNumberColumnIndex = 1;
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
        private PraticeManagement.BudgetManagement HostingPageIsBudgetManagementReport
        {
            get
            {
                if (Page is PraticeManagement.BudgetManagement)
                {
                    return ((PraticeManagement.BudgetManagement)Page);
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

        private bool showDetailedReport
        {
            get
            {
                return HostingPageIsBudgetManagementReport != null && ddlLevel.SelectedValue == "2";
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
                try
                {
                    return CompanyPerformanceState.ProjectList.ToList().OrderBy(p => p.ProjectNumber).ToArray();
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        private Project[] UIProjectList
        {
            get
            {
                Project[] list = ProjectList;
                if ((DataPoints == null || DataPoints.Contains(1)) && IsZeroProjectsSupressed)
                {
                    switch (CalculationsType)
                    {
                        case 1:
                            list = ProjectList.ToList().Where(p => p.ComputedFinancials != null && p.ComputedFinancials.Revenue != 0).OrderBy(p => p.ProjectNumber).ToArray();
                            break;
                        case 2:
                            list = ProjectList.ToList().Where(p => p.ComputedFinancials != null && p.ComputedFinancials.ActualRevenue != 0).OrderBy(p => p.ProjectNumber).ToArray();
                            break;
                        case 3:
                            list = ProjectList.ToList().Where(p => p.ComputedFinancials != null && p.ComputedFinancials.BudgetRevenue != 0).OrderBy(p => p.ProjectNumber).ToArray();
                            break;
                        case 4:
                            list = ProjectList.ToList().Where(p => p.ComputedFinancials != null && p.ComputedFinancials.EACRevenue != 0).OrderBy(p => p.ProjectNumber).ToArray();
                            break;
                    }
                }
                if ((DataPoints == null || DataPoints.Contains(2)) && IsZeroProjectsSupressed)
                {
                    switch (CalculationsType)
                    {
                        case 1:
                            list = ProjectList.ToList().Where(p => p.ComputedFinancials != null && p.ComputedFinancials.GrossMargin != 0).OrderBy(p => p.ProjectNumber).ToArray();
                            break;
                        case 2:
                            list = ProjectList.ToList().Where(p => p.ComputedFinancials != null && p.ComputedFinancials.ActualGrossMargin != 0).OrderBy(p => p.ProjectNumber).ToArray();
                            break;
                        case 3:
                            list = ProjectList.ToList().Where(p => p.ComputedFinancials != null && p.ComputedFinancials.BudgetGrossMargin != 0).OrderBy(p => p.ProjectNumber).ToArray();
                            break;
                        case 4:
                            list = ProjectList.ToList().Where(p => p.ComputedFinancials != null && p.ComputedFinancials.EACGrossMargin != 0).OrderBy(p => p.ProjectNumber).ToArray();
                            break;
                    }
                }
                return list;
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

        private int ActualPeriod
        {
            get
            {
                return int.Parse(ddlActualPeriod.SelectedValue);
            }
        }

        public DateTime? ActualsEndDate
        {
            get
            {
                var now = Utils.Generic.GetNowWithTimeZone();
                if (CalculationsType == 2 || CalculationsType == 4)
                {
                    if (HostingPageIsBudgetManagementReport != null)
                    {
                        return Utils.Calendar.MonthEndDate(now.AddMonths(-1));
                    }
                    else
                    {

                        if (ActualPeriod == 30)
                        {
                            return Utils.Calendar.MonthEndDate(now.AddMonths(-1));
                        }
                        else if (ActualPeriod == 15)
                        {
                            return Utils.Calendar.PayrollPerviousEndDate(now);
                        }
                        else if (ActualPeriod == 0)
                        {
                            return null;
                        }
                        else
                        {
                            return now;
                        }
                    }
                }
                else
                {
                    return Utils.Calendar.MonthEndDate(now.AddMonths(-1));
                }
            }
        }

        public List<int> DataPoints
        {
            get
            {
                //1-revenue, 2-Margin
                return ddldataPoints.SelectedValues;
            }
        }

        private string FeeTypeIds
        {
            get
            {
                return ddlFeeType.SelectedItems;
            }
            set
            {
                ddlFeeType.SelectedItems = value;
            }

        }

        public int? FeeType
        {
            get
            {
                if (HostingPageIsBudgetManagementReport != null)
                {
                    var sel = ddlFeeType.SelectedValues;
                    if (sel == null || (sel.Contains(1) && sel.Contains(2)))
                    {
                        return null;
                    }
                    else if (sel.Contains(1))
                    {
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsZeroProjectsSupressed
        {
            get
            {
                return ddlSupressZeroBalance.SelectedValue == "1";
            }
        }

        public int ExportType
        {
            get
            {
                return Convert.ToInt32(ddlExportOptions.SelectedValue);
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
                for (int i = 0; i < 28; i++)//there are 28 columns before month columns.
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
                dataNumberDateCellStyle.DataFormat = "$#,##0_);($#,##0)";

                CellStyles dataPerCellStyle = new CellStyles();
                dataPerCellStyle.DataFormat = "0.00%_);(0.00%)";

                CellStyles[] dataCellStylearray = { dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataStartDateCellStyle, dataStartDateCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, wrapdataCellStyle, dataCellStyle };
                List<CellStyles> dataCellStyleList = dataCellStylearray.ToList();

                var coloumnWidth = new List<int>();
                for (int i = 0; i < 17; i++)
                    coloumnWidth.Add(0);
                coloumnWidth.Add(50);
                coloumnWidth.Add(0);

                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                dataCellStyleList.Add(dataCellStyle);
                if (renderMonthColumns)
                {
                    var monthsInPeriod = GetPeriodLength();
                    for (int i = 0; i < monthsInPeriod; i++)
                    {
                        dataCellStyleList.Add(dataNumberDateCellStyle);
                    }
                }
                dataCellStyleList.Add(dataNumberDateCellStyle);
                dataCellStyleList.Add(dataNumberDateCellStyle);
                dataCellStyleList.Add(dataNumberDateCellStyle);
                if (HostingPageIsBudgetManagementReport != null)
                {
                    dataCellStyleList.Add(dataPerCellStyle);
                }
                else
                {
                    dataCellStyleList.Add(dataNumberDateCellStyle);
                    dataCellStyleList.Add(dataNumberDateCellStyle);
                    dataCellStyleList.Add(dataNumberDateCellStyle);
                    dataCellStyleList.Add(dataPerCellStyle);
                }
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

        private SheetStyles ScreenOnlySheetStyle
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
                for (int i = 0; i < 7; i++)//there are 28 columns before month columns.
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

                CellStyles dataStartDateCellStyle = new CellStyles();
                dataStartDateCellStyle.DataFormat = "mm/dd/yy;@";

                CellStyles dataNumberDateCellStyle = new CellStyles();
                dataNumberDateCellStyle.DataFormat = "$#,##0_);($#,##0)";

                CellStyles dataPerCellStyle = new CellStyles();
                dataPerCellStyle.DataFormat = "0.00%_);(0.00%)";

                CellStyles[] dataCellStylearray = { dataCellStyle, dataCellStyle, dataCellStyle, dataCellStyle, dataStartDateCellStyle, dataStartDateCellStyle, dataCellStyle };
                List<CellStyles> dataCellStyleList = dataCellStylearray.ToList();

                if (renderMonthColumns)
                {
                    var monthsInPeriod = GetPeriodLength();
                    for (int i = 0; i < monthsInPeriod; i++)
                    {
                        dataCellStyleList.Add(dataNumberDateCellStyle);
                    }
                }
                dataCellStyleList.Add(dataNumberDateCellStyle);
                if (CalculationsType != 3)
                {
                    dataCellStyleList.Add(dataNumberDateCellStyle);
                    dataCellStyleList.Add(dataNumberDateCellStyle);
                }

                dataCellStyleList.Add(dataPerCellStyle);

                RowStyles datarowStyle = new RowStyles(dataCellStyleList.ToArray());

                var rowStylelist = new List<RowStyles>();
                rowStylelist.Add(headerrowStyle);

                for (int i = 0; i < ExportRowsCount; i++)
                {
                    rowStylelist.Add(datarowStyle);
                }

                RowStyles[] rowStylearray = rowStylelist.ToArray();


                SheetStyles sheetStyle = new SheetStyles(rowStylearray);
                sheetStyle.TopRowNo = headerRowsCount;
                sheetStyle.IsFreezePane = true;
                sheetStyle.FreezePanColSplit = 0;
                sheetStyle.FreezePanRowSplit = headerRowsCount;

                return sheetStyle;
            }
        }

        private int ExportRowsCount
        {
            get;
            set;
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
                    if (pager.PageSize != pager.TotalRowCount && pager.TotalRowCount != 0)
                    {
                        Response.Redirect(Request.Url.AbsoluteUri);
                    }
                }
            }
            if (HostingPageIsProjectsReport != null)
            {
                lnkAddProject.Visible = false;
            }
            if (HostingPageIsBudgetManagementReport != null)
            {
                lnkAddProject.Visible = false;
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
                else if (pager.TotalRowCount != 0)
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
            //raj
            CompanyPerformanceFilterSettings filter = GetFilterSettings();
            if (HostingPageIsProjectsReport != null)
            {
                ReportsFilterHelper.SaveFilterValues(ReportName.ProjectSummaryReport, filter);
            }
            else if (HostingPageIsBudgetManagementReport != null)
            {
                ReportsFilterHelper.SaveFilterValues(ReportName.BudgetManagementReport, filter);
            }
            else
            {
                SerializationHelper.SerializeCookie(filter, CompanyPerformanceFilterKey);
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
                     CalculateRangeSelected = ProjectCalculateRangeType.ProjectValueInRange,
                     HideAdvancedFilter = false,
                     CalculationsType = Convert.ToInt32(ddlCalculationsType.SelectedValue),
                     DataPointsIdsList = ddldataPoints.SelectedItems,
                     //ViewIdsList = ddlCalculationsType.SelectedValue,
                     SupressZeroProjects = ddlSupressZeroBalance.SelectedValue,
                     FeeTypeIds = FeeTypeIds,
                     Level = ddlLevel.SelectedValue,
                     ActualEndDate = ActualsEndDate
                 };
            return filter;
        }

        protected void lvProjects_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                var row = e.Item.FindControl("boundingRow") as HtmlTableRow;


                if (HostingPageIsBudgetManagementReport != null)
                {
                    if (showDetailedReport)
                    {
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "ShowDetailedColumns();", true);
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "HideDetailedColumns();", true);
                    }
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "HideColumns();", true);
                }

                if (row.Cells.Count == numberOfFixedCol)
                {
                    var monthsInPeriod = GetPeriodLength();
                    //Raj
                    if (HostingPageIsBudgetManagementReport != null)
                    {
                        monthsInPeriod = 0;
                    }
                    for (int i = 0; i < monthsInPeriod + 1; i++)   // + 1 means a cell for total column
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }

                    if (CalculationsType != 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            var td = new HtmlTableCell() { };
                            td.Attributes["class"] = "CompPerfMonthSummary";
                            row.Cells.Insert(row.Cells.Count, td);
                        }
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
                        personListAnalyzer = new SeniorityAnalyzer(DataHelper.CurrentPerson);
                        personListAnalyzer.OneWithGreaterSeniorityExists(project.ProjectPersons);

                        var htmlRow = e.Item.FindControl("boundingRow") as HtmlTableRow;
                        FillProjectStateCell(htmlRow, cssClass, project.Status);
                        FillProjectNumberCell(e.Item, project);
                        FillClientNameCell(e.Item, project);
                        FillProjectNameCell(htmlRow, project);
                        FillProjectStartCell(e.Item, project);
                        FillProjectEndCell(e.Item, project);
                        if (HostingPageIsBudgetManagementReport != null && showDetailedReport)
                        {
                            FillDivisionCell(e.Item, project);
                            FillPracticeCell(e.Item, project);
                            FillChannelCell(e.Item, project);
                            FillSubChannelCell(e.Item, project);
                            FillRevenueCell(e.Item, project);
                            FillOfferingCell(e.Item, project);
                            FillStatusCell(e.Item, project);
                            FillNewOrExtCell(e.Item, project);
                            FillSalesPersonCell(e.Item, project);
                            FillDetailsCell(e.Item, project);
                        }
                    }
                    //Raj
                    if (HostingPageIsBudgetManagementReport == null)
                    {
                        FillMonthCells(row, project, personListAnalyzer);
                    }
                    FillTotalsCell(row, project, personListAnalyzer);
                }
            }
        }

        private void FillMonthCells(HtmlTableRow row, Project project, SeniorityAnalyzer personListAnalyzer)
        {
            DateTime monthBegin = GetMonthBegin();

            int periodLength = GetPeriodLength();

            // Displaying the interest values (main cell data)
            for (int i = numberOfFixedCol, k = 0;
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

                                PracticeManagementCurrency revenue = new PracticeManagementCurrency();
                                PracticeManagementCurrency margin = new PracticeManagementCurrency();

                                switch (CalculationsType)
                                {
                                    case 1:
                                        revenue = interestValue.Value.Revenue;
                                        margin = interestValue.Value.GrossMargin;
                                        break;
                                    case 2:
                                        revenue = interestValue.Value.ActualRevenue;
                                        margin = interestValue.Value.ActualGrossMargin;
                                        break;
                                    case 3:
                                        revenue = interestValue.Value.BudgetRevenue;
                                        margin = interestValue.Value.BudgetGrossMargin;
                                        break;
                                    case 4:
                                        revenue = interestValue.Value.EACRevenue;
                                        margin = interestValue.Value.EACGrossMargin;
                                        break;
                                }

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

        private static void FillDetailsCell(ListViewItem e, Project project)
        {
            var lblIsHouseAccount = e.FindControl("lblIsHouseAccount") as Label;
            lblIsHouseAccount.Text = project.Client.IsHouseAccount ? "Yes" : string.Empty;

            var lblBusinessGroup = e.FindControl("lblBusinessGroup") as Label;
            lblBusinessGroup.Text = project.BusinessGroup.HtmlEncodedName;

            var lblBusinessUnit = e.FindControl("lblBusinessUnit") as Label;
            lblBusinessUnit.Text = project.Group.HtmlEncodedName;

            var lblBuyer = e.FindControl("lblBuyer") as Label;
            lblBuyer.Text = project.BuyerName;

            var lblProjectManager = e.FindControl("lblProjectManager") as Label;
            lblProjectManager.Text = (project.ProjectOwner != null) ? project.ProjectOwner.Name : string.Empty;

            var lblEngagementManager = e.FindControl("lblEngagementManager") as Label;
            lblEngagementManager.Text = (project.SeniorManagerName != null) ? project.SeniorManagerName : string.Empty;

            var lblExecutiveInCharge = e.FindControl("lblExecutiveInCharge") as Label;
            lblExecutiveInCharge.Text = (project.Director != null && project.Director.Name != null) ? project.Director.Name.ToString() : string.Empty;

            var lblPricingList = e.FindControl("lblPricingList") as Label;
            lblPricingList.Text = project.PricingList != null ? project.PricingList.HtmlEncodedName : "";

            var lblPONumber = e.FindControl("lblPONumber") as Label;
            lblPONumber.Text = project.PONumber;

            var lblClientTimeEntry = e.FindControl("lblClientTimeEntry") as Label;
            lblClientTimeEntry.Text = project.IsClientTimeEntryRequired ? "Yes" : "No";

            var lblPreviousProjectNumber = e.FindControl("lblPreviousProjectNumber") as Label;
            lblPreviousProjectNumber.Text = project.PreviousProject != null ? project.PreviousProject.ProjectNumber : string.Empty;

            var lblOutSourceId = e.FindControl("lblOutSourceId") as Label;
            lblOutSourceId.Text = project.OutsourceId != 3 ? DataHelper.GetDescription((OutsourceId)project.OutsourceId) : string.Empty;

            var lblCapabilities = e.FindControl("lblCapabilities") as Label;

            lblCapabilities.Text = (project.Capabilities != null && project.Capabilities != string.Empty) ? project.Capabilities.TrimEnd(',', ' ') : string.Empty;
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
                switch (CalculationsType)
                {
                    case 1:
                        totalRevenue = project.ComputedFinancials.Revenue;
                        totalMargin = project.ComputedFinancials.GrossMargin;
                        break;
                    case 2:
                        totalRevenue = project.ComputedFinancials.ActualRevenue;
                        totalMargin = project.ComputedFinancials.ActualGrossMargin;
                        break;
                    case 3:
                        totalRevenue = project.ComputedFinancials.BudgetRevenue;
                        totalMargin = project.ComputedFinancials.BudgetGrossMargin;
                        break;
                    case 4:
                        totalRevenue = project.ComputedFinancials.EACRevenue;
                        totalMargin = project.ComputedFinancials.EACGrossMargin;
                        break;
                }
            }

            int totalCellNumber = CalculationsType == 3 ? row.Cells.Count - 1 : row.Cells.Count - 4;

            // Render Total Revenue and Margin for current Project
            if (project.Id.HasValue)
            {
                bool greaterSeniorityExists = personListAnalyzer != null && personListAnalyzer.GreaterSeniorityExists;
                row.Cells[totalCellNumber].InnerHtml = HostingPageIsBudgetManagementReport != null ? GetMonthReportTableAsHtml(project.ComputedFinancials.BudgetRevenue, project.ComputedFinancials.BudgetGrossMargin, greaterSeniorityExists) : GetMonthReportTableAsHtml(totalRevenue, totalMargin, greaterSeniorityExists);
                row.Cells[totalCellNumber].Attributes["class"] = "CompPerfTotalSummary";
                if (CalculationsType != 3)
                {
                    row.Cells[totalCellNumber + 1].InnerHtml = HostingPageIsBudgetManagementReport != null ? GetMonthReportTableAsHtml(totalRevenue, totalMargin, greaterSeniorityExists) : GetMonthReportTableAsHtml(project.ComputedFinancials.BudgetRevenue, project.ComputedFinancials.BudgetGrossMargin, greaterSeniorityExists);
                    row.Cells[totalCellNumber + 1].Attributes["class"] = "CompPerfTotalSummary";

                    var varianceRevenue = totalRevenue - project.ComputedFinancials.BudgetRevenue;
                    var varianceMargin = totalMargin - project.ComputedFinancials.BudgetGrossMargin;

                    row.Cells[totalCellNumber + 2].InnerHtml = GetMonthReportTableAsHtml(varianceRevenue, varianceMargin, greaterSeniorityExists);
                    row.Cells[totalCellNumber + 2].Attributes["class"] = "CompPerfTotalSummary";

                    decimal revPer = project.ComputedFinancials.BudgetRevenue != 0M ? varianceRevenue.Value * 100 / project.ComputedFinancials.BudgetRevenue.Value : 0M;
                    decimal marper = project.ComputedFinancials.BudgetGrossMargin != 0M ? varianceMargin.Value * 100 / project.ComputedFinancials.BudgetGrossMargin.Value : 0M;
                    row.Cells[totalCellNumber + 3].InnerHtml = GetVariancePercentageTableAsHtml(revPer, marper, greaterSeniorityExists);
                }
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
        }

        private static void FillStatusCell(ListViewItem e, Project project)
        {
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
            var lblStartDate = e.FindControl(LabelStartDateId) as Label;

            if (project.StartDate.HasValue)
            {
                lblStartDate.Text = project.StartDate.Value.ToString("MM/dd/yyyy");
            }
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
            var tdProjectName = row.FindControl("tdProjectName") as HtmlTableCell;

            HtmlAnchor anchor = new HtmlAnchor();
            anchor.InnerText = project.Name;
            anchor.HRef = GetRedirectUrl(project.Id.Value, Constants.ApplicationPages.ProjectDetail);
            anchor.Attributes["Description"] = PrepareToolTipView(project);
            anchor.Attributes["onmouseout"] = "HidePanel();";
            anchor.Attributes["onmouseover"] = "SetTooltipText(this.attributes['Description'].value,this);";

            tdProjectName.Controls.Add(anchor);
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
            var btnClient = e.FindControl(ButtonClientNameId) as HyperLink;
            btnClient.Text = project.Client.HtmlEncodedName;
            btnClient.NavigateUrl = GetRedirectUrl(project.Client.Id.Value, Constants.ApplicationPages.ClientDetails);
        }

        private static void FillDivisionCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Division cell content
            var lblDivision = e.FindControl("lblDivision") as Label;
            lblDivision.Text = project.Division != null ? project.Division.Name : string.Empty;
        }

        private static void FillPracticeCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Pracice cell content
            var lblPractice = e.FindControl("lblPractice") as Label;

            lblPractice.Text = project.Practice != null ? project.Practice.Name : string.Empty;
        }

        private static void FillChannelCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Channel cell content
            var lblChannel = e.FindControl("lblChannel") as Label;

            lblChannel.Text = project.Channel != null ? project.Channel.Name : string.Empty;
        }

        private static void FillSubChannelCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // SubChannel cell content
            var lblSubChannel = e.FindControl("lblSubChannel") as Label;

            lblSubChannel.Text = project.SubChannel != null ? project.SubChannel : string.Empty;
        }

        private static void FillRevenueCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // RevenueType cell content
            var lblRevenue = e.FindControl("lblRevenue") as Label;

            lblRevenue.Text = project.RevenueType != null ? project.RevenueType.Name : string.Empty;
        }

        private static void FillOfferingCell(ListViewItem e, Project project)
        {
            var row = e.FindControl("boundingRow") as HtmlTableRow;
            // Offering cell content
            var lblOffering = e.FindControl("lblOffering") as Label;

            lblOffering.Text = project.Offering != null ? project.Offering.Name : string.Empty;
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
            lvProjects.DataSource = SortProjects(UIProjectList);
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
            return UIProjectList;
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

            foreach (var project in UIProjectList)
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
            lvProjects.DataSource = UIProjectList;
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
                            switch (CalculationsType)
                            {
                                case 1:
                                    financials.Revenue += projectFinancials.Value.Revenue;
                                    financials.GrossMargin += projectFinancials.Value.GrossMargin;
                                    break;
                                case 2:
                                    financials.Revenue += projectFinancials.Value.ActualRevenue;
                                    financials.GrossMargin += projectFinancials.Value.ActualGrossMargin;
                                    break;
                                case 3:
                                    financials.Revenue += projectFinancials.Value.BudgetRevenue;
                                    financials.GrossMargin += projectFinancials.Value.BudgetGrossMargin;
                                    break;
                                case 4:
                                    financials.Revenue += projectFinancials.Value.EACRevenue;
                                    financials.GrossMargin += projectFinancials.Value.EACGrossMargin;
                                    break;
                            }
                        }
                    }
                }

                financialSummaryRevenue.ProjectedFinancialsByMonth.Add(dtTemp, financials);

                var projectsHavingFinancials = projects.Where(project => project.ComputedFinancials != null && project.Id.Value != defaultProjectIdValue);

                if (projectsHavingFinancials != null)
                {
                    switch (CalculationsType)
                    {
                        case 1:
                            financialSummaryRevenue.ComputedFinancials.Revenue = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.Revenue);
                            financialSummaryRevenue.ComputedFinancials.GrossMargin = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.GrossMargin);
                            break;
                        case 2:
                            financialSummaryRevenue.ComputedFinancials.Revenue = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.ActualRevenue);
                            financialSummaryRevenue.ComputedFinancials.GrossMargin = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.ActualGrossMargin);
                            break;
                        case 3:
                            financialSummaryRevenue.ComputedFinancials.Revenue = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.BudgetRevenue);
                            financialSummaryRevenue.ComputedFinancials.GrossMargin = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.BudgetGrossMargin);
                            break;
                        case 4:
                            financialSummaryRevenue.ComputedFinancials.Revenue = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.EACRevenue);
                            financialSummaryRevenue.ComputedFinancials.GrossMargin = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.EACGrossMargin);
                            break;
                    }
                    financialSummaryRevenue.ComputedFinancials.BudgetRevenue = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.BudgetRevenue);
                    financialSummaryRevenue.ComputedFinancials.BudgetGrossMargin = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.BudgetGrossMargin);

                    //ETC hear is budget to selection variance
                    financialSummaryRevenue.ComputedFinancials.EACRevenue = financialSummaryRevenue.ComputedFinancials.Revenue - financialSummaryRevenue.ComputedFinancials.BudgetRevenue;
                    financialSummaryRevenue.ComputedFinancials.EACGrossMargin = financialSummaryRevenue.ComputedFinancials.GrossMargin - financialSummaryRevenue.ComputedFinancials.BudgetGrossMargin;

                    financialSummaryRevenue.ComputedFinancials.ActualRevenue = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.ActualRevenue);
                    financialSummaryRevenue.ComputedFinancials.ActualGrossMargin = projectsHavingFinancials.Sum(proj => proj.ComputedFinancials.ActualGrossMargin);
                    //financialSummaryRevenue.ComputedFinancials.GrossMargin = projectsHavingFinancials.Sum(proj => CalculationsType == 2 ? proj.ComputedFinancials.ActualGrossMargin : CalculationsType == 1 ? proj.ComputedFinancials.GrossMargin : proj.ComputedFinancials.PreviousMonthsActualMarginValue);
                    //financialSummaryRevenue.ComputedFinancials.Revenue = projectsHavingFinancials.Sum(proj => CalculationsType == 2 ? proj.ComputedFinancials.ActualRevenue : CalculationsType == 1 ? proj.ComputedFinancials.Revenue : proj.ComputedFinancials.PreviousMonthsActualRevenueValue);
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

                ListItem all = new ListItem("All Data Points", "");
                ListItem rev = new ListItem("Revenue", "1");
                ListItem mar = new ListItem("Contribution Margin", "2");

                ddldataPoints.Items.Add(all);
                ddldataPoints.Items.Add(rev);
                ddldataPoints.Items.Add(mar);
                ddldataPoints.SelectedItems = "1,";

                if (HostingPageIsBudgetManagementReport != null)
                {
                    ddlCalculationsType.Items.Clear();
                    ListItem budget = new ListItem("Budget only", "3");
                    ListItem eac = new ListItem("Budget to ETC", "4");
                    ListItem actual = new ListItem("Budget to-date to Actual to-date", "2");
                    ListItem projected = new ListItem("Budget Remaining to Projected Remaining", "1");
                    ddlCalculationsType.Items.Add(budget);
                    ddlCalculationsType.Items.Add(eac);
                    ddlCalculationsType.Items.Add(actual);
                    ddlCalculationsType.Items.Add(projected);
                    ddlCalculationsType.SelectedValue = "3";

                }
                DataHelper.FillRevenueTypeList(cblRevenueType, "All Revenue Types");



                tdLevelLbl.Visible = tdLevelValue.Visible = tdFeeTypeLbl.Visible = tdFeeType.Visible = HostingPageIsBudgetManagementReport != null;

                //if (HostingPageIsBudgetManagementReport != null)
                //{
                ListItem allFeeTypes = new ListItem("All", "");
                ListItem TMOnly = new ListItem("T&M Only", "1");
                ListItem FFOnly = new ListItem("Fixed Fee Only", "2");

                ddlFeeType.Items.Add(allFeeTypes);
                ddlFeeType.Items.Add(TMOnly);
                ddlFeeType.Items.Add(FFOnly);
                ddlFeeType.SelectedIndex = 0;
                //}
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
                ddldataPoints.SelectedItems = filter.DataPointsIdsList;
                //ddlCalculationsType.SelectedValue = filter.ViewIdsList; //need to review 
                ddlSupressZeroBalance.SelectedValue = filter.SupressZeroProjects;
                ddlLevel.SelectedValue = filter.Level;
                FeeTypeIds = filter.FeeTypeIds;
                ddlPeriod.SelectedIndex = ddlPeriod.Items.IndexOf(ddlPeriod.Items.FindByValue(filter.PeriodSelected.ToString()));
                ddlView.SelectedIndex = ddlView.Items.IndexOf(ddlView.Items.FindByValue(filter.ViewSelected.ToString()));
                //ddlCalculateRange.SelectedIndex = ddlCalculateRange.Items.IndexOf(ddlCalculateRange.Items.FindByValue(filter.CalculateRangeSelected.ToString()));

                ddlCalculationsType.SelectedValue = filter.CalculationsType == 0 ? "4" : filter.CalculationsType.ToString();
            }
            else
            {
                Page.Validate(valsPerformance.ValidationGroup);
            }
        }

        private CompanyPerformanceFilterSettings InitFilter()
        {
            if (HostingPageIsProjectsReport != null)
            {
                return ReportsFilterHelper.GetFilterValues(ReportName.ProjectSummaryReport) as CompanyPerformanceFilterSettings ??
                   new CompanyPerformanceFilterSettings();
            }
            else if (HostingPageIsBudgetManagementReport != null)
            {
                return ReportsFilterHelper.GetFilterValues(ReportName.BudgetManagementReport) as CompanyPerformanceFilterSettings ??
                  new CompanyPerformanceFilterSettings();
            }
            else
            {
                return SerializationHelper.DeserializeCookie(CompanyPerformanceFilterKey) as CompanyPerformanceFilterSettings ??
                       new CompanyPerformanceFilterSettings();

            }
        }

        private void AddMonthColumn(HtmlTableRow row, DateTime periodStart, int monthsInPeriod, int insertPosition, bool showColumn)
        {
            if (row != null)
            {
                while (row.Cells.Count > numberOfFixedCol + 1)
                {
                    row.Cells.RemoveAt(numberOfFixedCol);
                }

                for (int i = insertPosition, k = 0; k < monthsInPeriod; i++, k++)
                {
                    var newColumn = new HtmlTableCell("td");
                    row.Cells.Insert(i, newColumn);

                    row.Cells[i].InnerHtml = periodStart.ToString(Constants.Formatting.CompPerfMonthYearFormat);
                    string extClass = showColumn ? "" : "hideMonthCol";
                    row.Cells[i].Attributes["class"] = "CompPerfMonthSummary " + extClass;

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
            int totalCell = CalculationsType == 3 ? row.Cells.Count - 1 : row.Cells.Count - 4;
            if (CalculationsType == 3)
            {
                row.Cells[row.Cells.Count - 1].InnerHtml = GetMonthReportTableAsHtml(totalRevenue, totalMargin, OneGreaterSeniorityExists);
            }
            else
            {
                row.Cells[row.Cells.Count - 4].InnerHtml = HostingPageIsBudgetManagementReport != null ? GetMonthReportTableAsHtml(summary.ComputedFinancials.BudgetRevenue, summary.ComputedFinancials.BudgetGrossMargin, OneGreaterSeniorityExists) : GetMonthReportTableAsHtml(totalRevenue, totalMargin, OneGreaterSeniorityExists);
                row.Cells[row.Cells.Count - 3].InnerHtml = HostingPageIsBudgetManagementReport != null ? GetMonthReportTableAsHtml(totalRevenue, totalMargin, OneGreaterSeniorityExists) : GetMonthReportTableAsHtml(summary.ComputedFinancials.BudgetRevenue, summary.ComputedFinancials.BudgetGrossMargin, OneGreaterSeniorityExists);
                row.Cells[row.Cells.Count - 2].InnerHtml = GetMonthReportTableAsHtml(summary.ComputedFinancials.EACRevenue, summary.ComputedFinancials.EACGrossMargin, OneGreaterSeniorityExists);
                var budgetRevenue = summary.ComputedFinancials.BudgetRevenue;
                var budgetMargin = summary.ComputedFinancials.BudgetGrossMargin;
                totalRevenue = summary.ComputedFinancials.EACRevenue;
                totalMargin = summary.ComputedFinancials.EACGrossMargin;
                decimal revenuePerc = budgetRevenue != 0M ? totalRevenue.Value * 100 / budgetRevenue.Value : 0M;
                decimal marginPerc = budgetMargin != 0M ? totalMargin.Value * 100 / budgetMargin.Value : 0M;
                row.Cells[row.Cells.Count - 1].InnerHtml = GetVariancePercentageTableAsHtml(revenuePerc, marginPerc, OneGreaterSeniorityExists);
            }

        }

        private string GetVariancePercentageTableAsHtml(decimal revenuePer, decimal marginPer, bool GreaterSeniorityExists)
        {
            string outterHtml = string.Empty;

            var stringWriter = new System.IO.StringWriter();

            var reportTable = new Table() { Width = Unit.Percentage(100) };
            if (DataPoints == null || DataPoints.Contains(1))
            {
                var tr = new TableRow() { };
                tr.Cells.Add(new TableCell() { HorizontalAlign = HorizontalAlign.Left, Text = "" });

                tr.Cells.Add(new TableCell() { HorizontalAlign = HorizontalAlign.Right, Text = revenuePer < 0 ? string.Format("<span class=\"Bench\">({0}%)</span>", Math.Abs(revenuePer).ToString("#0.00")) : string.Format("{0}%", revenuePer.ToString("##0.00")) });
                reportTable.Rows.Add(tr);
            }
            if (DataPoints == null || DataPoints.Contains(2))
            {
                var tr = new TableRow();
                tr.Cells.Add(new TableCell() { HorizontalAlign = HorizontalAlign.Left, Text = "" });
                tr.Cells.Add(new TableCell()
                {
                    HorizontalAlign = HorizontalAlign.Right,
                    Text = GreaterSeniorityExists ? "(Hidden)" : marginPer < 0 ? string.Format("<span class=\"Bench\">({0}%)</span>", Math.Abs(marginPer).ToString("#0.00")) : string.Format("{0}%", marginPer.ToString("##0.00"))
                });
                reportTable.Rows.Add(tr);
            }

            var div = new Panel() { CssClass = "cell-pad" };
            div.Controls.Add(reportTable);
            using (HtmlTextWriter wr = new HtmlTextWriter(stringWriter))
            {
                div.RenderControl(wr);
                outterHtml = stringWriter.ToString();
            }

            return outterHtml;
        }

        #region Month table from resources

        private Table GetMonthReportTable(PracticeManagementCurrency revenue, PracticeManagementCurrency margin, bool greaterSeniorityExists)
        {
            margin.FormatStyle = NumberFormatStyle.Margin;
            //var marginText = greaterSeniorityExists ? Resources.Controls.HiddenCellText : margin.Value.ToString(CurrencyDisplayFormat);
            var reportTable = new Table() { Width = Unit.Percentage(100) };
            if (DataPoints == null || DataPoints.Contains(1))
            {
                var tr = new TableRow() { CssClass = "Revenue" };
                tr.Cells.Add(new TableCell() { HorizontalAlign = HorizontalAlign.Left, Text = "" });

                tr.Cells.Add(new TableCell() { HorizontalAlign = HorizontalAlign.Right, Text = revenue.ToString() });
                reportTable.Rows.Add(tr);
            }
            if (DataPoints == null || DataPoints.Contains(2))
            {
                var tr = new TableRow();// { CssClass = "Margin" };
                tr.Cells.Add(new TableCell() { HorizontalAlign = HorizontalAlign.Left, Text = "" });
                tr.Cells.Add(new TableCell()
                {
                    HorizontalAlign = HorizontalAlign.Right,
                    Text = margin.ToString(greaterSeniorityExists)//as part of #2786.
                });
                reportTable.Rows.Add(tr);
            }
            return reportTable;
        }

        private string GetMonthReportTableAsHtml(PracticeManagementCurrency revenue, PracticeManagementCurrency margin, bool greaterSeniorityExists)
        {
            string outterHtml = string.Empty;

            var stringWriter = new System.IO.StringWriter();
            revenue.DoNotShowDecimals = margin.DoNotShowDecimals = true;

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
            if (ExportType == 1)
            {
                if (HostingPageIsBudgetManagementReport == null || (HostingPageIsBudgetManagementReport != null && !showDetailedReport))
                {
                    ExportScreenOnlySummaryData();
                    return;
                }
            }

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

                                    Salesperson = (pro.SalesPersonName != null) ? pro.SalesPersonName : string.Empty,
                                    ProjectManager = (pro.ProjectOwner != null) ? pro.ProjectOwner.Name : string.Empty,
                                    SeniorManager = (pro.SeniorManagerName != null) ? pro.SeniorManagerName : string.Empty,
                                    Director = (pro.Director != null && pro.Director.Name != null) ? pro.Director.Name.ToString() : string.Empty,
                                    PricingList = (pro.PricingList != null && pro.PricingList.Name != null) ? pro.PricingList.Name : string.Empty,
                                    PONumber = (pro.PONumber != null && pro.PONumber != null) ? pro.PONumber : string.Empty,
                                    ClientTimeEntryRequired = (pro.IsClientTimeEntryRequired ? "Yes" : "No"),
                                    PreviousProject = pro.PreviousProject != null ? pro.PreviousProject.ProjectNumber : string.Empty,
                                    OutsourceId = (pro.OutsourceId != 3 && pro.OutsourceId != 0) ? DataHelper.GetDescription((OutsourceId)pro.OutsourceId) : string.Empty,
                                    Type = Revenue,
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

                                              Salesperson = (pro.SalesPersonName != null) ? pro.SalesPersonName : string.Empty,
                                              ProjectManager = (pro.ProjectOwner != null) ? pro.ProjectOwner.Name : string.Empty,
                                              SeniorManager = (pro.SeniorManagerName != null) ? pro.SeniorManagerName : string.Empty,
                                              Director = (pro.Director != null && pro.Director.Name != null) ? pro.Director.Name.ToString() : string.Empty,
                                              PricingList = (pro.PricingList != null && pro.PricingList.Name != null) ? pro.PricingList.Name : string.Empty,
                                              PONumber = (pro.PONumber != null && pro.PONumber != null) ? pro.PONumber : string.Empty,
                                              ClientTimeEntryRequired = (pro.IsClientTimeEntryRequired ? "Yes" : "No"),
                                              PreviousProject = pro.PreviousProject != null ? pro.PreviousProject.ProjectNumber : string.Empty,
                                              OutsourceId = pro.OutsourceId != 3 && pro.OutsourceId != 0 ? DataHelper.GetDescription((OutsourceId)pro.OutsourceId) : string.Empty,
                                              Type = Margin,
                                          }).ToList();

            switch (ExportType)
            {
                case 2: //revenue
                    break;
                case 3: //Margin
                    projectsData = projectsDataWithMargin;
                    break;
                case 4: //revenue & margin
                    projectsData.AddRange(projectsDataWithMargin);
                    break;
            }


            projectsData = projectsData.OrderBy(s => (s.Status == ProjectStatusType.Projected.ToString()) ? s.StartDate : s.EndDate).ThenBy(s => s.ProjectNumber).ThenByDescending(s => s.Type).ToList();
            renderMonthColumns = HostingPageIsBudgetManagementReport != null ? false : true;
            var dataProjected = PrepareDataTable(ExportProjectList, (object[])projectsData.ToArray());

            string dateRangeTitle = string.Format(ExportDateRangeFormat, diRange.FromDate.Value.ToShortDateString(), diRange.ToDate.Value.ToShortDateString());
            DataTable header = new DataTable();
            header.Columns.Add(dateRangeTitle);
            headerRowsCount = header.Rows.Count + 3;
            List<SheetStyles> sheetStylesList = new List<SheetStyles>();
            coloumnsCount = dataProjected.Columns.Count;
            sheetStylesList.Add(HeaderSheetStyle);
            sheetStylesList.Add(DataSheetStyle);
            sheetStylesList.Add(HeaderSheetStyle);
            sheetStylesList.Add(DataSheetStyle);

            var dataSetList = new List<DataSet>();
            var dataset = new DataSet();
            dataset.DataSetName = "Summary";
            dataset.Tables.Add(header);
            dataset.Tables.Add(dataProjected);
            dataSetList.Add(dataset);

            var fileName = HostingPageIsProjectSummary != null ? "Projects.xls" : "ProjectBudgetSummaryReport.xls";
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

        private DataTable PrepareDataTable(Project[] projectsList, Object[] propertyBags)
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
            data.Columns.Add("Salesperson");
            data.Columns.Add("Project Manager");
            data.Columns.Add("Engagement Manager");
            data.Columns.Add("Executive in Charge");
            data.Columns.Add("Pricing List");
            data.Columns.Add("PO Number");
            data.Columns.Add("Client Time Entry Required");
            data.Columns.Add("Previous Project Number");
            data.Columns.Add("Outsource Id Indicator");
            data.Columns.Add("Type");
            //Add Month and Total columns.
            if (renderMonthColumns)
            {
                for (int i = 0; i < monthsInPeriod; i++)
                {
                    data.Columns.Add(periodStart.AddMonths(i).ToString(Constants.Formatting.EntryDateFormat));
                }
            }

            if (HostingPageIsBudgetManagementReport != null)
            {
                data.Columns.Add("Budget");
                if (CalculationsType != 3)
                {
                    data.Columns.Add(GetTotalColumnHeader() + " Total");
                    data.Columns.Add("$ Variance");
                    data.Columns.Add("% Variance");
                }
            }
            else
            {
                data.Columns.Add(GetTotalColumnHeader() + " Total");
                data.Columns.Add("Budget");
                data.Columns.Add("Actual");
                data.Columns.Add("Projected Remaining");
                data.Columns.Add("ETC");
                data.Columns.Add("Budget to ETC $ Variance");
                data.Columns.Add("Budget to ETC % Variance");
            }

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
                            if (property.GetValue(propertyBag).ToString() == "P001123")
                            {

                            }
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
                                                PracticeManagementCurrency revenue = new PracticeManagementCurrency();
                                                PracticeManagementCurrency margin = new PracticeManagementCurrency();

                                                switch (CalculationsType)
                                                {
                                                    case 1:
                                                        revenue = interestValue.Value.Revenue;
                                                        margin = interestValue.Value.GrossMargin;
                                                        break;
                                                    case 2:
                                                        revenue = interestValue.Value.ActualRevenue;
                                                        margin = interestValue.Value.ActualGrossMargin;
                                                        break;
                                                    case 3:
                                                        revenue = interestValue.Value.BudgetRevenue;
                                                        margin = interestValue.Value.BudgetGrossMargin;
                                                        break;
                                                    case 4:
                                                        revenue = interestValue.Value.EACRevenue;
                                                        margin = interestValue.Value.EACGrossMargin;
                                                        break;
                                                }

                                                columnValue = isMargin ? margin : revenue;
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

                            var budgetValue = 0M;
                            var actualValue = 0M;
                            var ETCValue = 0M;
                            var projRemainingValue = 0M;
                            var budgetToETCVar = 0M;
                            var budgetToETCPer = 0M;
                            var budgetToSelVar = 0M;
                            var budgetToSelVarPer = 0M;


                            PracticeManagementCurrency totalRevenue = new PracticeManagementCurrency();
                            PracticeManagementCurrency totalMargin = new PracticeManagementCurrency();

                            if (project.ComputedFinancials != null && !greaterSeniorityExists)
                            {

                                switch (CalculationsType)
                                {
                                    case 1:
                                        totalRevenue = project.ComputedFinancials.Revenue;
                                        totalMargin = project.ComputedFinancials.GrossMargin;
                                        break;
                                    case 2:
                                        totalRevenue = project.ComputedFinancials.ActualRevenue;
                                        totalMargin = project.ComputedFinancials.ActualGrossMargin;
                                        break;
                                    case 3:
                                        totalRevenue = project.ComputedFinancials.BudgetRevenue;
                                        totalMargin = project.ComputedFinancials.BudgetGrossMargin;
                                        break;
                                    case 4:
                                        totalRevenue = project.ComputedFinancials.EACRevenue;
                                        totalMargin = project.ComputedFinancials.EACGrossMargin;
                                        break;
                                }
                                columnValue = isMargin ? totalMargin : totalRevenue;
                                budgetValue = isMargin ? project.ComputedFinancials.BudgetGrossMargin : project.ComputedFinancials.BudgetRevenue;
                                budgetToSelVar = columnValue - budgetValue;
                                budgetToSelVarPer = budgetValue != 0M ? budgetToSelVar / (budgetValue) : 0M;
                            }

                            if (HostingPageIsBudgetManagementReport != null)
                            {
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, budgetValue < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetValue);
                                column++;
                                if (CalculationsType != 3)
                                {
                                    objects[column] = string.Format(NPOIExcel.CustomColorKey, columnValue < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : columnValue);
                                    column++;
                                    objects[column] = string.Format(NPOIExcel.CustomColorKey, budgetToSelVar < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetToSelVar);
                                    column++;
                                    objects[column] = string.Format(NPOIExcel.CustomColorKey, budgetToSelVarPer < 0 ? "red" : "black", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetToSelVarPer.ToString());
                                }
                            }
                            else
                            {
                                if (project.ComputedFinancials != null && !greaterSeniorityExists)
                                {
                                    actualValue = isMargin ? project.ComputedFinancials.ActualGrossMargin : project.ComputedFinancials.ActualRevenue;
                                    projRemainingValue = isMargin ? (project.ComputedFinancials.EACGrossMargin - project.ComputedFinancials.ActualGrossMargin) : (project.ComputedFinancials.EACRevenue - project.ComputedFinancials.ActualRevenue);
                                    ETCValue = isMargin ? project.ComputedFinancials.EACGrossMargin : project.ComputedFinancials.EACRevenue;
                                    budgetToETCVar = isMargin ? (project.ComputedFinancials.EACGrossMargin - project.ComputedFinancials.BudgetGrossMargin) : (project.ComputedFinancials.EACRevenue - project.ComputedFinancials.BudgetRevenue);
                                    budgetToETCPer = isMargin ? (project.ComputedFinancials.BudgetGrossMargin != 0M ? budgetToETCVar / (project.ComputedFinancials.BudgetGrossMargin.Value) : 0M) : (project.ComputedFinancials.BudgetRevenue != 0M ? budgetToETCVar / (project.ComputedFinancials.BudgetRevenue.Value) : 0M);
                                }
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, columnValue < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : columnValue);
                                column++;
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, budgetValue < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetValue);
                                column++;
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, actualValue < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : actualValue);
                                column++;
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, projRemainingValue < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : projRemainingValue);
                                column++;
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, ETCValue < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : ETCValue);
                                column++;
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, budgetToETCVar < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetToETCVar);
                                column++;
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, budgetToETCPer < 0 ? "red" : "black", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetToETCPer.ToString());
                            }
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

            var emptyRow = new object[data.Columns.Count];
            for (int i = 0; i < data.Columns.Count; i++)
            {
                emptyRow[i] = string.Empty;
            }
            data.Rows.Add(emptyRow);

            var summary = CalculateSummaryTotals(projectsList, periodStart, PeriodEnd);


            if (ExportType == 2 || ExportType == 4)
            {
                int columnCount = 0;
                var totalRow = new object[data.Columns.Count];
                totalRow[0] = string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Total");
                columnCount++;
                for (int i = 1; i < 27; i++)
                {
                    totalRow[i] = string.Empty;
                    columnCount++;
                }

                totalRow[27] = "Services Revenue";
                columnCount++;
                if (renderMonthColumns)
                {
                    var monthStart = periodStart;
                    // Displaying the month values 
                    for (int i = 28, k = 0;
                     k < monthsInPeriod;
                     i++, k++, monthStart = monthStart.AddMonths(1))
                    {
                        DateTime monthEnd = GetMonthEnd(ref monthStart);

                        foreach (KeyValuePair<DateTime, ComputedFinancials> interestValue in
                            summary.ProjectedFinancialsByMonth)
                        {
                            if (IsInMonth(interestValue.Key, monthStart, monthEnd))
                            {
                                var colValue = interestValue.Value.Revenue.Value;
                                totalRow[i] = string.Format(NPOIExcel.CustomColorKey, colValue < 0 ? "red" : "green", colValue);
                            }
                        }
                        columnCount++;
                    }
                }

                if (HostingPageIsBudgetManagementReport != null)
                {
                    var budgetValue = summary.ComputedFinancials.BudgetRevenue.Value;
                    totalRow[columnCount] = string.Format(NPOIExcel.CustomColorKey, budgetValue < 0 ? "red" : "green", budgetValue);

                    if (CalculationsType != 3)
                    {
                        var columnValue = summary.ComputedFinancials.Revenue.Value;
                        totalRow[columnCount + 1] = string.Format(NPOIExcel.CustomColorKey, columnValue < 0 ? "red" : "green", columnValue);
                        var budgetToSelVar = columnValue - budgetValue;
                        var budgetToSelVarPer = budgetValue != 0M ? budgetToSelVar / (budgetValue) : 0M;
                        totalRow[columnCount + 2] = string.Format(NPOIExcel.CustomColorKey, budgetToSelVar < 0 ? "red" : "green", budgetToSelVar);
                        totalRow[columnCount + 3] = string.Format(NPOIExcel.CustomColorKey, budgetToSelVarPer < 0 ? "red" : "black", budgetToSelVarPer.ToString());
                    }
                }
                else
                {
                    var columnValue = summary.ComputedFinancials.Revenue.Value;
                    var budgetValue = summary.ComputedFinancials.BudgetRevenue.Value;
                    var ETCValue = summary.ComputedFinancials.EACRevenue.Value;
                    var actualValue = summary.ComputedFinancials.ActualRevenue.Value;
                    var projRemainingValue = ETCValue - actualValue;
                    var budgetToETCVar = budgetValue - ETCValue;
                    var budgetToETCPer = budgetValue != 0 ? budgetToETCVar / budgetValue : 0M;

                    totalRow[columnCount] = string.Format(NPOIExcel.CustomColorKey, columnValue < 0 ? "red" : "green", columnValue);
                    totalRow[columnCount + 1] = string.Format(NPOIExcel.CustomColorKey, budgetValue < 0 ? "red" : "green", budgetValue);

                    totalRow[columnCount + 2] = string.Format(NPOIExcel.CustomColorKey, actualValue < 0 ? "red" : "green", actualValue);

                    totalRow[columnCount + 3] = string.Format(NPOIExcel.CustomColorKey, projRemainingValue < 0 ? "red" : "green", projRemainingValue);
                    totalRow[columnCount + 4] = string.Format(NPOIExcel.CustomColorKey, ETCValue < 0 ? "red" : "green", ETCValue);
                    totalRow[columnCount + 5] = string.Format(NPOIExcel.CustomColorKey, budgetToETCVar < 0 ? "red" : "green", budgetToETCVar);
                    totalRow[columnCount + 6] = string.Format(NPOIExcel.CustomColorKey, budgetToETCPer < 0 ? "red" : "black", budgetToETCPer);
                }


                data.Rows.Add(totalRow);
            }


            if (ExportType == 3 || ExportType == 4)
            {
                int columnCount = 0;
                var totalRow = new object[data.Columns.Count];
                totalRow[0] = string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Total");
                columnCount++;
                for (int i = 1; i < 27; i++)
                {
                    totalRow[i] = string.Empty;
                    columnCount++;
                }

                totalRow[27] = "Cont. Margin";
                columnCount++;
                if (renderMonthColumns)
                {
                    var monthStart = periodStart;
                    // Displaying the month values 
                    for (int i = 28, k = 0;
                     k < monthsInPeriod;
                     i++, k++, monthStart = monthStart.AddMonths(1))
                    {
                        DateTime monthEnd = GetMonthEnd(ref monthStart);

                        foreach (KeyValuePair<DateTime, ComputedFinancials> interestValue in
                            summary.ProjectedFinancialsByMonth)
                        {
                            if (IsInMonth(interestValue.Key, monthStart, monthEnd))
                            {
                                var colValue = interestValue.Value.GrossMargin.Value;
                                totalRow[i] = string.Format(NPOIExcel.CustomColorKey, colValue < 0 ? "red" : "purple", colValue);
                            }
                        }
                        columnCount++;
                    }
                }
                if (HostingPageIsBudgetManagementReport != null)
                {
                    var budgetValue = summary.ComputedFinancials.BudgetGrossMargin.Value;
                    totalRow[columnCount] = string.Format(NPOIExcel.CustomColorKey, budgetValue < 0 ? "red" : "purple", budgetValue);

                    if (CalculationsType != 3)
                    {
                        var columnValue = summary.ComputedFinancials.GrossMargin.Value;
                        totalRow[columnCount + 1] = string.Format(NPOIExcel.CustomColorKey, columnValue < 0 ? "red" : "purple", columnValue);
                        var budgetToSelVar = budgetValue - columnValue;
                        var budgetToSelVarPer = budgetValue != 0M ? budgetToSelVar / (budgetValue) : 0M;
                        totalRow[columnCount + 2] = string.Format(NPOIExcel.CustomColorKey, budgetToSelVar < 0 ? "red" : "purple", budgetToSelVar);
                        totalRow[columnCount + 3] = string.Format(NPOIExcel.CustomColorKey, budgetToSelVarPer < 0 ? "red" : "purple", budgetToSelVarPer.ToString());
                    }
                }
                else
                {
                    var columnValue = summary.ComputedFinancials.GrossMargin.Value;
                    var budgetValue = summary.ComputedFinancials.BudgetGrossMargin.Value;
                    var ETCValue = summary.ComputedFinancials.EACGrossMargin.Value;
                    var actualValue = summary.ComputedFinancials.ActualGrossMargin.Value;
                    var projRemainingValue = ETCValue - actualValue;
                    var budgetToETCVar = ETCValue - budgetValue;
                    var budgetToETCPer = budgetValue != 0 ? budgetToETCVar / budgetValue : 0M;

                    totalRow[columnCount] = string.Format(NPOIExcel.CustomColorKey, columnValue < 0 ? "red" : "purple", columnValue);
                    totalRow[columnCount + 1] = string.Format(NPOIExcel.CustomColorKey, budgetValue < 0 ? "red" : "purple", budgetValue);
                    totalRow[columnCount + 2] = string.Format(NPOIExcel.CustomColorKey, actualValue < 0 ? "red" : "purple", actualValue);
                    totalRow[columnCount + 3] = string.Format(NPOIExcel.CustomColorKey, projRemainingValue < 0 ? "red" : "purple", projRemainingValue);
                    totalRow[columnCount + 4] = string.Format(NPOIExcel.CustomColorKey, ETCValue < 0 ? "red" : "purple", ETCValue);
                    totalRow[columnCount + 5] = string.Format(NPOIExcel.CustomColorKey, budgetToETCVar < 0 ? "red" : "purple", budgetToETCVar);
                    totalRow[columnCount + 6] = string.Format(NPOIExcel.CustomColorKey, budgetToETCPer < 0 ? "red" : "black", budgetToETCPer);
                }

                data.Rows.Add(totalRow);
            }

            return data;
        }

        private void ExportScreenOnlySummaryData()
        {
            var projectsData = (from pro in UIProjectList
                                where pro != null
                                select new
                                {
                                    ProjectID = pro.Id != null ? pro.Id.ToString() : string.Empty,
                                    ProjectNumber = pro.ProjectNumber != null ? pro.ProjectNumber.ToString() : string.Empty,
                                    Account = (pro.Client != null && pro.Client.Name != null) ? pro.Client.Name.ToString() : string.Empty,
                                    ProjectName = pro.Name != null ? pro.Name : string.Empty,
                                    Status = (pro.Status != null && pro.Status.Name != null) ? pro.Status.Name.ToString() : string.Empty,
                                    StartDate = pro.StartDate.HasValue ? pro.StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                    EndDate = pro.EndDate.HasValue ? pro.EndDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                    Type = Revenue,
                                }).ToList();

            var projectsDataWithMargin = (from pro in UIProjectList
                                          where pro != null
                                          select new
                                          {
                                              ProjectID = pro.Id != null ? pro.Id.ToString() : string.Empty,
                                              ProjectNumber = pro.ProjectNumber != null ? pro.ProjectNumber.ToString() : string.Empty,
                                              Account = (pro.Client != null && pro.Client.Name != null) ? pro.Client.Name.ToString() : string.Empty,
                                              ProjectName = pro.Name != null ? pro.Name : string.Empty,
                                              Status = (pro.Status != null && pro.Status.Name != null) ? pro.Status.Name.ToString() : string.Empty,
                                              StartDate = pro.StartDate.HasValue ? pro.StartDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                              EndDate = pro.EndDate.HasValue ? pro.EndDate.Value.ToString(Constants.Formatting.EntryDateFormat) : string.Empty,
                                              Type = Margin,
                                          }).ToList();

            if (DataPoints == null || (DataPoints.Contains(2) && DataPoints.Contains(1)))
            {
                projectsData.AddRange(projectsDataWithMargin);
            }
            else if (DataPoints.Contains(2))
            {
                projectsData = projectsDataWithMargin;
            }
            //Raj
            renderMonthColumns = HostingPageIsBudgetManagementReport != null ? false : true;
            projectsData = projectsData.OrderBy(s => (s.Status == ProjectStatusType.Projected.ToString()) ? s.StartDate : s.EndDate).ThenBy(s => s.ProjectNumber).ThenByDescending(s => s.Type).ToList();

            ExportRowsCount = projectsData.Count();

            var data = PrepareScreenOnlyDataTable(ExportProjectList, (object[])projectsData.ToArray());

            string dateRangeTitle = string.Format(ExportDateRangeFormat, diRange.FromDate.Value.ToShortDateString(), diRange.ToDate.Value.ToShortDateString());
            DataTable header = new DataTable();
            header.Columns.Add(dateRangeTitle);
            headerRowsCount = header.Rows.Count + 3;
            List<SheetStyles> sheetStylesList = new List<SheetStyles>();
            coloumnsCount = data.Columns.Count;
            sheetStylesList.Add(HeaderSheetStyle);
            sheetStylesList.Add(ScreenOnlySheetStyle);

            var dataSetList = new List<DataSet>();
            var dataset = new DataSet();
            dataset.DataSetName = "Summary - ScreenOnly";
            dataset.Tables.Add(header);
            dataset.Tables.Add(data);
            dataSetList.Add(dataset);
            var fileName = HostingPageIsProjectSummary != null ? "Projects.xls" : "ProjectBudgetSummaryReport.xls";
            NPOIExcel.Export(fileName, dataSetList, sheetStylesList);
        }

        private DataTable PrepareScreenOnlyDataTable(Project[] projectsList, Object[] propertyBags)
        {
            var periodStart = GetMonthBegin();
            var monthsInPeriod = GetPeriodLength();

            DataTable data = new DataTable();

            data.Columns.Add("Project Number");
            data.Columns.Add("Account");
            data.Columns.Add("Project Name");
            data.Columns.Add("Status");
            data.Columns.Add("Start Date");
            data.Columns.Add("End Date");
            data.Columns.Add("Type");
            //Add Month and Total columns.
            if (renderMonthColumns)
            {
                for (int i = 0; i < monthsInPeriod; i++)
                {
                    data.Columns.Add(periodStart.AddMonths(i).ToString(Constants.Formatting.EntryDateFormat));
                }
            }
            data.Columns.Add(HostingPageIsBudgetManagementReport != null ? "Budget" : GetTotalColumnHeader());
            if (CalculationsType != 3)
            {
                data.Columns.Add(HostingPageIsBudgetManagementReport != null ? GetTotalColumnHeader() : "Budget");
                data.Columns.Add(HostingPageIsBudgetManagementReport != null ? "$ Variance" : "Budget to Variance");
                data.Columns.Add(HostingPageIsBudgetManagementReport != null ? "% Variance" : "Budget to % Variance");
            }

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
                                                PracticeManagementCurrency revenue = new PracticeManagementCurrency();
                                                PracticeManagementCurrency margin = new PracticeManagementCurrency();

                                                switch (CalculationsType)
                                                {
                                                    case 1:
                                                        revenue = interestValue.Value.Revenue;
                                                        margin = interestValue.Value.GrossMargin;
                                                        break;
                                                    case 2:
                                                        revenue = interestValue.Value.ActualRevenue;
                                                        margin = interestValue.Value.ActualGrossMargin;
                                                        break;
                                                    case 3:
                                                        revenue = interestValue.Value.BudgetRevenue;
                                                        margin = interestValue.Value.BudgetGrossMargin;
                                                        break;
                                                    case 4:
                                                        revenue = interestValue.Value.EACRevenue;
                                                        margin = interestValue.Value.EACGrossMargin;
                                                        break;
                                                }

                                                columnValue = isMargin ? margin : revenue;
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

                            var budgetValue = 0M;

                            var budgetToSelVar = 0M;
                            var budgetToSelPer = 0M;
                            if (project.ComputedFinancials != null && !greaterSeniorityExists)
                            {
                                PracticeManagementCurrency totalRevenue = new PracticeManagementCurrency();
                                PracticeManagementCurrency totalMargin = new PracticeManagementCurrency();
                                switch (CalculationsType)
                                {
                                    case 1:
                                        totalRevenue = project.ComputedFinancials.Revenue;
                                        totalMargin = project.ComputedFinancials.GrossMargin;
                                        break;
                                    case 2:
                                        totalRevenue = project.ComputedFinancials.ActualRevenue;
                                        totalMargin = project.ComputedFinancials.ActualGrossMargin;
                                        break;
                                    case 3:
                                        totalRevenue = project.ComputedFinancials.BudgetRevenue;
                                        totalMargin = project.ComputedFinancials.BudgetGrossMargin;
                                        break;
                                    case 4:
                                        totalRevenue = project.ComputedFinancials.EACRevenue;
                                        totalMargin = project.ComputedFinancials.EACGrossMargin;
                                        break;
                                }
                                columnValue = isMargin ? totalMargin : totalRevenue;
                                budgetValue = isMargin ? project.ComputedFinancials.BudgetGrossMargin : project.ComputedFinancials.BudgetRevenue;
                                budgetToSelVar = columnValue - budgetValue;
                                budgetToSelPer = budgetValue != 0M ? budgetToSelVar / budgetValue : 0M;
                            }
                            string totalColomncolor = columnValue < 0 ? "red" : isMargin ? "purple" : "green";
                            objects[column] = HostingPageIsBudgetManagementReport != null ? string.Format(NPOIExcel.CustomColorKey, budgetValue < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetValue) : string.Format(NPOIExcel.CustomColorKey, totalColomncolor, greaterSeniorityExists && isMargin ? (object)"(Hidden)" : columnValue);
                            if (CalculationsType != 3)
                            {
                                column++;
                                objects[column] = HostingPageIsBudgetManagementReport != null ? string.Format(NPOIExcel.CustomColorKey, totalColomncolor, greaterSeniorityExists && isMargin ? (object)"(Hidden)" : columnValue) : string.Format(NPOIExcel.CustomColorKey, budgetValue < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetValue);
                                column++;
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, budgetToSelVar < 0 ? "red" : isMargin ? "purple" : "green", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetToSelVar);
                                column++;
                                objects[column] = string.Format(NPOIExcel.CustomColorKey, budgetToSelPer < 0 ? "red" : "black", greaterSeniorityExists && isMargin ? (object)"(Hidden)" : budgetToSelPer.ToString());
                            }
                        }
                        column++;
                    }
                }

                data.Rows.Add(objects);
            }

            //Total Cell
            var emptyRow = new object[data.Columns.Count];
            for (int i = 0; i < data.Columns.Count; i++)
            {
                emptyRow[i] = string.Empty;
            }
            data.Rows.Add(emptyRow);

            var summary = CalculateSummaryTotals(projectsList, periodStart, PeriodEnd);

            if (DataPoints == null || DataPoints.Contains(1))
            {
                int columnCount = 0;
                var totalRow = new object[data.Columns.Count];
                totalRow[0] = string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Total");
                columnCount++;
                for (int i = 1; i < 6; i++)
                {
                    totalRow[i] = string.Empty;
                    columnCount++;
                }

                totalRow[6] = "Services Revenue";
                columnCount++;
                if (renderMonthColumns)
                {
                    var monthStart = periodStart;
                    // Displaying the month values 
                    for (int i = 7, k = 0;
                     k < monthsInPeriod;
                     i++, k++, monthStart = monthStart.AddMonths(1))
                    {
                        DateTime monthEnd = GetMonthEnd(ref monthStart);

                        foreach (KeyValuePair<DateTime, ComputedFinancials> interestValue in
                            summary.ProjectedFinancialsByMonth)
                        {
                            if (IsInMonth(interestValue.Key, monthStart, monthEnd))
                            {
                                var colValue = interestValue.Value.Revenue.Value;
                                totalRow[i] = string.Format(NPOIExcel.CustomColorKey, colValue < 0 ? "red" : "green", colValue);
                            }
                        }
                        columnCount++;
                    }
                }

                PracticeManagementCurrency totalRevenue = 0M;


                // Calculate Total Revenue and Margin for current Project
                if (summary.ComputedFinancials != null)
                {
                    totalRevenue = summary.ComputedFinancials.Revenue;
                }

                if (CalculationsType == 3)
                {
                    totalRow[columnCount] = string.Format(NPOIExcel.CustomColorKey, totalRevenue.Value < 0 ? "red" : "green", totalRevenue.Value);
                    columnCount++;
                }
                else
                {
                    var val1 = HostingPageIsBudgetManagementReport != null ? summary.ComputedFinancials.BudgetRevenue.Value : totalRevenue.Value;
                    totalRow[columnCount] = string.Format(NPOIExcel.CustomColorKey, val1 < 0 ? "red" : "green", val1);
                    var val2 = HostingPageIsBudgetManagementReport != null ? totalRevenue.Value : summary.ComputedFinancials.BudgetRevenue.Value;
                    totalRow[columnCount + 1] = string.Format(NPOIExcel.CustomColorKey, val2 < 0 ? "red" : "green", val2);
                    totalRow[columnCount + 2] = string.Format(NPOIExcel.CustomColorKey, summary.ComputedFinancials.EACRevenue.Value < 0 ? "red" : "green", summary.ComputedFinancials.EACRevenue.Value);
                    var budgetRevenue = summary.ComputedFinancials.BudgetRevenue;
                    var budgetMargin = summary.ComputedFinancials.BudgetGrossMargin;
                    totalRevenue = summary.ComputedFinancials.EACRevenue;
                    decimal revenuePerc = budgetRevenue.Value != 0M ? totalRevenue.Value / budgetRevenue.Value : 0M;
                    totalRow[columnCount + 3] = string.Format(NPOIExcel.CustomColorKey, revenuePerc < 0 ? "red" : "black", revenuePerc); ;
                }

                data.Rows.Add(totalRow);
            }
            if (DataPoints == null || DataPoints.Contains(2))
            {
                int columnCount = 0;
                var totalRow = new object[data.Columns.Count];
                totalRow[0] = string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Total");
                columnCount++;
                for (int i = 1; i < 6; i++)
                {
                    totalRow[i] = string.Empty;
                    columnCount++;
                }

                totalRow[6] = "Cont. Margin";
                columnCount++;
                if (renderMonthColumns)
                {
                    var monthStart = periodStart;
                    // Displaying the month values 
                    for (int i = 7, k = 0;
                     k < monthsInPeriod;
                     i++, k++, monthStart = monthStart.AddMonths(1))
                    {
                        DateTime monthEnd = GetMonthEnd(ref monthStart);

                        foreach (KeyValuePair<DateTime, ComputedFinancials> interestValue in
                            summary.ProjectedFinancialsByMonth)
                        {
                            if (IsInMonth(interestValue.Key, monthStart, monthEnd))
                            {
                                var colValue = interestValue.Value.GrossMargin.Value;
                                totalRow[i] = string.Format(NPOIExcel.CustomColorKey, colValue < 0 ? "red" : "purple", colValue);
                            }
                        }
                        columnCount++;
                    }
                }

                PracticeManagementCurrency totalMargin = 0M;
                totalMargin.FormatStyle = NumberFormatStyle.Margin;

                // Calculate Total Revenue and Margin for current Project
                if (summary.ComputedFinancials != null)
                {
                    totalMargin = summary.ComputedFinancials.GrossMargin.Value;
                }

                if (CalculationsType == 3)
                {
                    totalRow[columnCount] = string.Format(NPOIExcel.CustomColorKey, totalMargin < 0 ? "red" : "purple", totalMargin.Value); ;
                    columnCount++;
                }
                else
                {
                    var val1 = HostingPageIsBudgetManagementReport != null ? summary.ComputedFinancials.BudgetGrossMargin.Value : totalMargin.Value;
                    totalRow[columnCount] = string.Format(NPOIExcel.CustomColorKey, val1 < 0 ? "red" : "purple", val1);
                    var val2 = HostingPageIsBudgetManagementReport != null ? totalMargin.Value : summary.ComputedFinancials.BudgetGrossMargin.Value;
                    totalRow[columnCount + 1] = string.Format(NPOIExcel.CustomColorKey, val2 < 0 ? "red" : "purple", val2);
                    totalRow[columnCount + 2] = string.Format(NPOIExcel.CustomColorKey, summary.ComputedFinancials.EACGrossMargin.Value < 0 ? "red" : "purple", summary.ComputedFinancials.EACGrossMargin.Value);
                    var budgetMargin = summary.ComputedFinancials.BudgetGrossMargin;
                    totalMargin = summary.ComputedFinancials.EACGrossMargin;
                    decimal marginPerc = budgetMargin.Value != 0M ? totalMargin.Value / budgetMargin.Value : 0M;
                    totalRow[columnCount + 3] = string.Format(NPOIExcel.CustomColorKey, marginPerc < 0 ? "red" : "purple", marginPerc);
                }

                data.Rows.Add(totalRow);
            }


            return data;
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

            lvProjects.DataSource = UIProjectList;
            lvProjects.DataBind();
        }

        protected void lvProjects_OnDataBound(object sender, EventArgs e)
        {
            if (HostingPageIsBudgetManagementReport != null)
            {
                SetHeaderMonths(false);
                if (showDetailedReport)
                {
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "ShowDetailedColumns();", true);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "HideDetailedColumns();", true);
                }
            }
            else
            {
                SetHeaderMonths(true);
                ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "HideColumns();", true);
            }

            var pager = GetPager();

            if (pager != null)
            {
                pager.Visible = (pager.PageSize < pager.TotalRowCount);
                if (pager.TotalRowCount != 0)
                {
                    lblPageCount.Text = string.Format(PageViewCountFormat, (pager.StartRowIndex + 1), (pager.StartRowIndex + GetCurrentPageCount()), GetTotalCount());
                }
                else
                {
                    lblPageCount.Text = string.Format(PageViewCountFormat, 0, 0, 0);
                }
            }
            else
            {
                lblPageCount.Text = string.Format(PageViewCountFormat, 0, 0, 0);
            }
        }

        private void SetHeaderMonths(bool strShowMonths)
        {
            var row = lvProjects.FindControl("lvHeader") as System.Web.UI.HtmlControls.HtmlTableRow;
            if (row != null)
            {
                var periodStart = diRange.FromDate.Value;
                var monthsInPeriod = GetPeriodLength();
                Page.Validate(valsPerformance.ValidationGroup);

                //Raj
                if (!IsPostBack || Page.IsValid)
                {
                    AddMonthColumn(row, periodStart, monthsInPeriod, numberOfFixedCol, strShowMonths);
                }


                string totalHeaderText = HostingPageIsBudgetManagementReport != null ? "Budget" : GetTotalColumnHeader();
                var div = new Panel() { CssClass = CompPerfHeaderDivCssClass };
                div.Controls.Add(new Label() { Text = totalHeaderText });

                var stringWriter = new System.IO.StringWriter();
                using (HtmlTextWriter wr = new HtmlTextWriter(stringWriter))
                {
                    div.RenderControl(wr);
                    var s = stringWriter.ToString();
                    row.Cells[row.Cells.Count - 1].InnerHtml = s;
                    row.Cells[row.Cells.Count - 1].Attributes["class"] = ".no-wrap alignCenter BorderRightC5C5C5";
                }

                for (int i = numberOfFixedCol; i < row.Cells.Count - 1; i++)
                {
                    PopulateMiniReportCell(row, i);
                }

                if (CalculationsType != 3)
                {
                    for (int i = 1; i < 4; i++)
                    {
                        var budgetColumn = new HtmlTableCell("td");
                        row.Cells.Insert(row.Cells.Count, budgetColumn);
                        row.Cells[row.Cells.Count - 1].Attributes["class"] = "alignCenter BorderRightC5C5C5";
                    }
                    string headerText = "Total";// string.Format(TotalHeaderFormat, ddlCalculateRange.SelectedItem.Text);
                    var divHeader = new Panel() { CssClass = CompPerfHeaderDivCssClass };
                    divHeader.Controls.Add(new Label() { Text = totalHeaderText });


                    using (HtmlTextWriter wr = new HtmlTextWriter(stringWriter))
                    {
                        div.RenderControl(wr);
                        var s = stringWriter.ToString();
                        row.Cells[row.Cells.Count - 1].InnerHtml = s;
                        row.Cells[row.Cells.Count - 1].Attributes["class"] = ".no-wrap alignCenter BorderRightC5C5C5";
                    }
                    row.Cells[row.Cells.Count - 3].InnerHtml = PrepareHeader(HostingPageIsBudgetManagementReport != null ? GetTotalColumnHeader() : "Budget");
                    row.Cells[row.Cells.Count - 2].InnerHtml = PrepareHeader(HostingPageIsBudgetManagementReport != null ? "$ Variance" : "Budget Variance");
                    row.Cells[row.Cells.Count - 1].InnerHtml = PrepareHeader(HostingPageIsBudgetManagementReport != null ? "% Variance" : "Budget % Variance");
                }

                // fill summary
                row = lvProjects.FindControl("lvSummary") as System.Web.UI.HtmlControls.HtmlTableRow;
                var tdSummary = row.FindControl("tdSummary") as System.Web.UI.HtmlControls.HtmlTableCell;
                tdSummary.ColSpan = HostingPageIsBudgetManagementReport != null && showDetailedReport ? 28 : 6;
                while (row.Cells.Count > 1)
                {
                    row.Cells.RemoveAt(1);
                }
                //Raj
                if (HostingPageIsBudgetManagementReport != null)
                {
                    monthsInPeriod = 0;
                }
                for (int i = 0; i < monthsInPeriod + 1; i++)   // + 1 means a cell for total column
                {
                    var td = new HtmlTableCell() { };
                    td.Attributes["class"] = "CompPerfMonthSummary";
                    row.Cells.Insert(row.Cells.Count, td);
                }
                if (CalculationsType != 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var td = new HtmlTableCell() { };
                        td.Attributes["class"] = "CompPerfMonthSummary";
                        row.Cells.Insert(row.Cells.Count, td);
                    }
                }

                var summary = CalculateSummaryTotals(UIProjectList, periodStart, PeriodEnd);

                FillSummaryTotalRow(monthsInPeriod, summary, row);
            }
        }

        private string GetTotalColumnHeader()
        {
            string headerText = "";
            switch (CalculationsType)
            {
                case 1:
                    headerText = "Projected";
                    break;
                case 2:
                    headerText = "Actual";
                    break;
                case 3:
                    headerText = "Budget";
                    break;
                case 4:
                    headerText = "ETC";
                    break;
            }
            return headerText;
        }

        private string PrepareHeader(string headerText)
        {
            var div = new Panel() { CssClass = CompPerfHeaderDivCssClass };
            div.Controls.Add(new Label() { Text = headerText });

            var stringWriter = new System.IO.StringWriter();
            using (HtmlTextWriter wr = new HtmlTextWriter(stringWriter))
            {
                div.RenderControl(wr);
            }
            return stringWriter.ToString();
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

