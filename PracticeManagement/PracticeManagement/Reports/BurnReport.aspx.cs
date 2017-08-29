using DataTransferObjects;
using DataTransferObjects.Filters;
using DataTransferObjects.Reports;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PraticeManagement.Controls;
using PraticeManagement.Objects;
using PraticeManagement.Security;
using PraticeManagement.Utils;
using PraticeManagement.Utils.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace PraticeManagement.Reports
{
    public partial class BurnReport : System.Web.UI.Page
    {
        private const string ProjectViewState = "ProjectViewState";
        private const string ReportDataViewState = "ReportData";
        private const int BudgetSeriesIndex = 0;
        private const int ActualSeriesIndex = 1;
        private const int ProjectedSeriesIndex = 2;
        private const int BudgetAmountSeriesIndex = 3;
        private const int ExpenseSeriesIndex = 4;
        private const string HideText = "(Hidden)";
        private const string CurrencyLargeDisplayFormat = "$###,###,###,###,##0";
        private string RowSpliter = Guid.NewGuid().ToString();
        private string ColoumSpliter = Guid.NewGuid().ToString();
        private const string TITLE_FORMAT_PDF = "Burn Report \n{0} \nFor: {1}, By: {2}.";

        public string ProjectNumber
        {
            get
            {
                return txtProjectNumber.Text;
            }
            set
            {
                txtProjectNumber.Text = value;
            }
        }

        private Project SelectedProject
        {
            get
            {
                return ViewState["SelectedProject"] as Project;
            }
            set
            {
                ViewState["SelectedProject"] = value;
            }
        }

        private int Step
        {
            get
            {
                return int.Parse(ddlDetalization.SelectedValue);
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

        public DateTime? StartDate
        {
            get
            {
                var periodSelected = int.Parse(ddlPeriod.SelectedValue);

                if (periodSelected == 0)
                {
                    return diRange.FromDate.Value;
                }
                else
                {
                    var now = Utils.Generic.GetNowWithTimeZone();

                    if (periodSelected == 1)
                    {
                        return null;
                    }
                    else if (periodSelected == -3)
                    {
                        return Utils.Calendar.CurrentQuarterStartDate(now);
                    }
                    else if (periodSelected == 3)
                    {
                        return Utils.Calendar.NextQuarterStartDate(now);
                    }
                    else if (periodSelected == -6)
                    {
                        return Utils.Calendar.CurrentHalfStartDate(now);
                    }
                    else if (periodSelected == 6)
                    {
                        return Utils.Calendar.NextHalfStartDate(now);
                    }
                    else
                    {
                        return diRange.FromDate.Value;
                    }
                }
            }
        }

        public DateTime? EndDate
        {
            get
            {
                var periodSelected = int.Parse(ddlPeriod.SelectedValue);

                if (periodSelected == 0)
                {
                    return diRange.ToDate.Value;
                }
                else
                {
                    var now = Utils.Generic.GetNowWithTimeZone();

                    if (periodSelected == 1)
                    {
                        return null;
                    }
                    else if (periodSelected == -3)
                    {
                        return Utils.Calendar.CurrentQuarterEndDate(now);
                    }
                    else if (periodSelected == 3)
                    {
                        return Utils.Calendar.NextQuarterEndDate(now);
                    }
                    else if (periodSelected == -6)
                    {
                        return Utils.Calendar.CurrentHalfEndDate(now);
                    }
                    else if (periodSelected == 6)
                    {
                        return Utils.Calendar.NextHalfEndDate(now);
                    }
                    else
                    {
                        return diRange.ToDate.Value;
                    }
                }
            }
        }

        public string SelectedPeriod
        {
            get
            {
                return ddlPeriod.SelectedValue;
            }
            set
            {
                ddlPeriod.SelectedValue = value;
            }
        }

        public int SelectedDetails
        {
            get
            {
                if (rbnHours.Checked)
                {
                    return 1;
                }
                else if (rbnRevenue.Checked)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
        }

        public int SelectedRevenueType
        {
            get
            {
                if (chbGridBudget.Checked)
                {
                    return 1;
                }
                else if (chbGridActuals.Checked)
                {
                    return 2;
                }
                else if (chbGridProjected.Checked)
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
        }

        private ProjectBurnFinancials ReportData
        {
            get
            {
                if (ViewState[ReportDataViewState] == null)
                {
                    ViewState[ReportDataViewState] = ServiceCallers.Custom.Report(r => r.GetBurnReportFortheProject(SelectedProject.Id.Value, SelectedPeriod == "1" ? null : StartDate, SelectedPeriod == "1" ? null : EndDate, Step, ActualsEndDate));
                }

                return ViewState[ReportDataViewState] as ProjectBurnFinancials;
            }

        }

        private Dictionary<DateTime, ComputedFinancials> Financials
        {
            get
            {
                if (ViewState["Financials"] == null)
                {
                    ViewState["Financials"] = ServiceCallers.Custom.Report(r => r.GetWeeklyFinancialsForProject(SelectedProject.Id.Value, SelectedPeriod == "1" ? null : StartDate, SelectedPeriod == "1" ? null : EndDate, Step, ActualsEndDate));
                }
                return ViewState["Financials"] as Dictionary<DateTime, ComputedFinancials>;
            }
        }

        private decimal totalBudgetHrs
        {
            get;
            set;
        }

        private PracticeManagementCurrency totalBudgetRevenue
        {
            get;
            set;
        }

        private PracticeManagementCurrency totalBudgetMargin
        {
            get;
            set;
        }

        private decimal totalActualHrs
        {
            get;
            set;
        }

        private PracticeManagementCurrency totalActualRevenue
        {
            get;
            set;
        }

        private PracticeManagementCurrency totalActualMargin
        {
            get;
            set;
        }

        private decimal totalProjHrs
        {
            get;
            set;
        }

        private PracticeManagementCurrency totalProjRevenue
        {
            get;
            set;
        }

        private PracticeManagementCurrency totalProjMargin
        {
            get;
            set;
        }

        private decimal totalEACHrs
        {
            get;
            set;
        }

        private PracticeManagementCurrency totalEACRevenue
        {
            get;
            set;
        }

        private PracticeManagementCurrency totalEACMargin
        {
            get;
            set;
        }

        private PracticeManagementCurrency TotalBudgetExpense
        {
            get;
            set;
        }

        private PracticeManagementCurrency TotalActualExpense
        {
            get;
            set;
        }

        private PracticeManagementCurrency TotalProjectedExpense
        {
            get;
            set;
        }

        private PracticeManagementCurrency TotalEACExpense
        {
            get;
            set;
        }

        private SeniorityAnalyzer PersonListAnalyzer
        {
            get;
            set;
        }

        private bool GreaterSeniorityExists
        {
            get
            {
                return PersonListAnalyzer != null && PersonListAnalyzer.GreaterSeniorityExists;
            }
        }

        private bool IsFromBrowserSession
        {
            get;
            set;
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
                dataCellStyle.WrapText = true;
                dataCellStyle.IsBold = true;
                CellStyles[] dataCellStylearray = { dataCellStyle };
                RowStyles datarowStyle = new RowStyles(dataCellStylearray);

                RowStyles[] rowStylearray = { headerrowStyle, datarowStyle };

                SheetStyles sheetStyle = new SheetStyles(rowStylearray);
                sheetStyle.MergeRegion.Add(new int[] { 0, 0, 0, 5 });
                sheetStyle.IsAutoResize = false;

                return sheetStyle;
            }
        }

        private int numberOfWeeks
        {
            get;
            set;
        }

        private SheetStyles DataSheetStyle
        {
            get
            {
                CellStyles headerWrapCellStyle = new CellStyles();
                headerWrapCellStyle.IsBold = true;
                headerWrapCellStyle.HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                headerWrapCellStyle.WrapText = true;

                List<CellStyles> headerCellStyleList = new List<CellStyles>();
                headerCellStyleList.Add(headerWrapCellStyle);
                RowStyles headerrowStyle = new RowStyles(headerCellStyleList.ToArray());

                CellStyles dataCellStyle = new CellStyles();

                CellStyles dataPercentCellStyle = new CellStyles();
                dataPercentCellStyle.DataFormat = "0.00%";

                CellStyles dataHoursCellStyle = new CellStyles();
                dataPercentCellStyle.DataFormat = "#,##0.00";



                CellStyles dataNumberDateCellStyle = new CellStyles();
                dataNumberDateCellStyle.DataFormat = "$#,##0_);($#,##0)";

                var dataCellStylearray = new List<CellStyles>() { dataCellStyle, dataCellStyle, dataNumberDateCellStyle };

                for (int i = 0; i < numberOfWeeks; i++)
                {
                    if (!showRevenue && i == 36)
                    {
                        break;
                    }
                    if (showRevenue && i == 63)
                    {
                        break;
                    }
                    PreapreStyleSheet(dataCellStylearray);
                }

                RowStyles datarowStyle = new RowStyles(dataCellStylearray.ToArray());
                RowStyles[] rowStylearray = { headerrowStyle, headerrowStyle, datarowStyle };
                SheetStyles sheetStyle = new SheetStyles(rowStylearray);


                sheetStyle.MergeRegion.Add(new int[] { 3, 4, 0, 0 });
                sheetStyle.MergeRegion.Add(new int[] { 3, 4, 1, 1 });
                sheetStyle.MergeRegion.Add(new int[] { 3, 4, 2, 2 });

                for (int i = 0; i < numberOfWeeks; i++)
                {
                    if (!showRevenue)
                    {
                        if (i == 36)
                        {
                            break;
                        }
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, (i * 7) + 3, (i * 7) + 9 });
                    }
                    else
                    {
                        if (i == 63)
                        {
                            break;
                        }
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, (i * 4) + 3, (i * 4) + 6 });
                    }
                }

                sheetStyle.IsFreezePane = true;
                sheetStyle.FreezePanColSplit = 0;

                sheetStyle.TopRowNo = 4;
                sheetStyle.FreezePanRowSplit = 5;

                return sheetStyle;
            }
        }

        private void PreapreStyleSheet(List<CellStyles> dataCellArray)
        {
            CellStyles dataCellStyle = new CellStyles();

            CellStyles dataPercentCellStyle = new CellStyles();
            dataPercentCellStyle.DataFormat = "0.00%";

            CellStyles dataHoursCellStyle = new CellStyles();
            dataHoursCellStyle.DataFormat = "#,##0.00";

            CellStyles dataNumberDateCellStyle = new CellStyles();
            dataNumberDateCellStyle.DataFormat = "$#,##0_);($#,##0)";

            if (rbnHours.Checked)
            {
                dataCellArray.Add(dataHoursCellStyle);
                dataCellArray.Add(dataHoursCellStyle);
                dataCellArray.Add(dataHoursCellStyle);
            }

            if (rbnMargin.Checked)
            {
                dataCellArray.Add(dataNumberDateCellStyle);
                dataCellArray.Add(dataNumberDateCellStyle);
                dataCellArray.Add(dataNumberDateCellStyle);
            }

            dataCellArray.Add(dataNumberDateCellStyle);
            dataCellArray.Add(dataNumberDateCellStyle);
            dataCellArray.Add(dataNumberDateCellStyle);
            dataCellArray.Add(dataPercentCellStyle);
        }

        private TableStyles _PdfProjectListTableStyle;

        private TableStyles PdfProjectListTableStyle
        {
            get
            {
                if (_PdfProjectListTableStyle == null)
                {
                    TdStyles HeaderStyle = new TdStyles("center", true, false, 10, 1);
                    HeaderStyle.BackgroundColor = "light-gray";
                    TdStyles ContentStyle1 = new TdStyles("left", false, false, 9, 1);
                    TdStyles ContentStyle2 = new TdStyles("center", false, false, 9, 1);


                    TdStyles[] HeaderStyleArray = { HeaderStyle };
                    TdStyles[] ContentStyleArray = { ContentStyle1, ContentStyle2 };

                    TrStyles HeaderRowStyle = new TrStyles(HeaderStyleArray);
                    TrStyles ContentRowStyle = new TrStyles(ContentStyleArray);

                    TrStyles[] RowStyleArray = { HeaderRowStyle, ContentRowStyle };
                    float[] widths = { 1.075f, 1.09f, 1.09f, 1.1f, 1.1f };
                    _PdfProjectListTableStyle = new TableStyles(widths, RowStyleArray, 80, "custom", new int[] { 245, 250, 255 });
                    _PdfProjectListTableStyle.IsColoumBorders = false;
                }
                return _PdfProjectListTableStyle;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var clients = DataHelper.GetAllClientsSecure(null, true, true);
                DataHelper.FillListDefault(ddlClients, "-- Select an Account -- ", clients as object[], false);
                divReport.Visible = false;
                divEmpty.Visible = false;


            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GetFilterValuesForSession();
            }
            var now = Utils.Generic.GetNowWithTimeZone();
            diRange.FromDate = StartDate.HasValue ? StartDate : Utils.Calendar.WeekStartDate(now);
            diRange.ToDate = EndDate.HasValue ? EndDate : Utils.Calendar.WeekEndDate(now);

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
            hdnStartDateTxtBoxId.Value = (diRange.FindControl("tbFrom") as TextBox).ClientID;
            hdnEndDateTxtBoxId.Value = (diRange.FindControl("tbTo") as TextBox).ClientID;
            if (!IsPostBack && IsFromBrowserSession)
            {
                PopulateReport();
            }
        }

        protected void btnUpdateReport_Click(object sender, EventArgs e)
        {
            PopulateReport();
            SaveFilterValuesForSession();
        }

        private void PopulateReport()
        {
            if (ValidateProject())
            {
                if (SelectedProject.Status.Id == (int)ProjectStatusType.Active || SelectedProject.Status.Id == (int)ProjectStatusType.AtRisk || SelectedProject.Status.Id == (int)ProjectStatusType.Completed)
                {
                    ViewState[ReportDataViewState] = ViewState["Financials"] = null;
                    lblProjectName.Text = SelectedProject.ProjectNumber + "-" + SelectedProject.Name;
                    lblDateRange.Text = SelectedProject.StartDate.Value.ToString("MM/dd/yyyy") + " - " + SelectedProject.EndDate.Value.ToString("MM/dd/yyyy");
                    if (ReportData != null)
                    {
                        if (ReportData.Resources != null && ReportData.Resources.Count > 0)
                        {
                            PersonListAnalyzer = new SeniorityAnalyzer(DataHelper.CurrentPerson);
                            PersonListAnalyzer.OneWithGreaterSeniorityExists(SelectedProject.ProjectPersons);

                            divReport.Visible = true;
                            divProjectInfo.Visible = true;
                            btnExport.Enabled = true;
                            repResources.DataSource = ReportData.Resources.Take(10);
                            repResources.DataBind();

                            lnkViewAllResources.Visible = (ReportData.Resources.Count > 10);
                            hdnProjectNumber.Value = SelectedProject.Id.Value.ToString();
                            lnkProject.NavigateUrl = String.Format(Constants.ApplicationPages.DetailRedirectFormat + "&ShowPersonsTab={2}", Constants.ApplicationPages.ProjectDetail, hdnProjectNumber.Value, "true");
                            repResources.Visible = true;
                            UpdateGraph();
                        }
                        else
                        {
                            repResources.Visible = false;
                        }
                        if (ReportData.Expenses != null && ReportData.Expenses.Count > 0)
                        {
                            repExpenses.DataSource = ReportData.Expenses.Take(10);
                            lnkViewAllResources.Visible = lnkViewAllResources.Visible ? true : ReportData.Expenses.Count > 10;
                            repExpenses.DataBind();
                            repExpenses.Visible = true;
                        }
                        else
                        {
                            repExpenses.Visible = false;
                        }
                        uplGrid.Update();
                    }
                    else
                    {
                        divReport.Visible = false;
                        divEmpty.Visible = true;
                        btnExport.Enabled = false;
                    }
                    var Weeklykeys = GetWeeksInPeriod();
                    numberOfWeeks = Weeklykeys.Count;
                    int noOfColumns = 3;
                    for (int i = 0; i < numberOfWeeks; i++)
                    {
                        noOfColumns += 7;
                    }
                    if (noOfColumns < 256)
                    {
                        btnLimitedExport.Visible = false;
                        btnExport.Visible = true;
                    }
                    else
                    {
                        btnLimitedExport.Visible = true;
                        btnExport.Visible = false;
                    }
                }
                else
                {
                    msgError.ShowErrorMessage("Select Active or At-Risk or Completed Project.");
                    divReport.Visible = false;
                    divEmpty.Visible = false;
                    btnExport.Enabled = false;
                    divProjectInfo.Visible = false;
                }

                //updConsReport.Update();
                ClearFilters();
            }
            else
            {
                divReport.Visible = false;
                divEmpty.Visible = false;
                btnExport.Enabled = false;
                divProjectInfo.Visible = false;
                //updConsReport.Update();
            }
            updConsReport.Update();
        }

        private bool ValidateProject()
        {
            try
            {
                SelectedProject = ServiceCallers.Custom.Project(p => p.GetProjectShortByProjectNumberForPerson(ProjectNumber, HttpContext.Current.User.Identity.Name));
                if (SelectedProject != null)
                {
                    var noOfDays = (SelectedProject.EndDate.Value - SelectedProject.StartDate.Value).TotalDays;
                    selectedProjectLength.Value = noOfDays.ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                msgError.ShowErrorMessage(string.IsNullOrEmpty(ProjectNumber) ? "Please enter a project number." : ex.Message);
                return false;
            }
        }

        private void ClearFilters()
        {
            ltrlNoProjectsText.Visible = false;
            ClearAndAddFirsItemForDdlProjects();
            ddlProjects.SelectedIndex = ddlClients.SelectedIndex = 0;
        }

        public void UpdateGraph(bool isupdateGraphOnly = false)
        {
            ReportData.Financials = Financials;
            decimal actualAmount = 0M;
            decimal margin = 0M;
            decimal EACRevenue = 0M;
            decimal EACHours = 0M;
            decimal actualHours = 0M;
            decimal budgetAmount = 0M;
            if (ReportData.Financials != null && ReportData.Financials.Count > 0)
            {
                ReportData.Financials.OrderBy(r => r.Key);
                InitAxis(activityChart.ChartAreas["MainArea"].AxisX);

                KeyValuePair<DateTime, ComputedFinancials> act = new KeyValuePair<DateTime, ComputedFinancials>();
                var isActualsExists = ReportData.Financials.Any(_ => _.Value.ActualRevenue != 0);
                if (isActualsExists)
                {
                    act = ReportData.Financials.Last(_ => _.Value.ActualRevenue != 0);
                }

                foreach (KeyValuePair<DateTime, ComputedFinancials> financial in ReportData.Financials)
                {
                    if (isActualsExists && chbActuals.Checked && financial.Key <= act.Key)
                    {
                        activityChart.Series[ActualSeriesIndex].Points.AddXY(financial.Key, financial.Value.ActualRevenue.Value);
                    }
                    if (chbBudget.Checked)
                    {
                        activityChart.Series[BudgetSeriesIndex].Points.AddXY(financial.Key, financial.Value.BudgetRevenue.Value);
                    }
                    if (chbProjected.Checked)
                    {
                        activityChart.Series[ProjectedSeriesIndex].Points.AddXY(financial.Key, financial.Value.Revenue.Value);
                    }
                    if (ReportData.BudgetAmount != null)
                    {
                        activityChart.Series[BudgetAmountSeriesIndex].Points.AddXY(financial.Key, ReportData.BudgetAmount);// update with budget variable
                    }
                }

                if (!isupdateGraphOnly)
                {
                    var summary = ReportData.Financials.Last();

                    actualAmount = isActualsExists ? act.Value.ActualRevenue : 0;
                    margin = isActualsExists ? act.Value.ActualGrossMargin : 0;
                    EACRevenue = summary.Value.EACRevenue;
                    EACHours = summary.Value.EACHours;
                    actualHours = summary.Value.ActualHours;
                    budgetAmount = summary.Value.BudgetRevenue;

                    lblBudgteAmount.Text = budgetAmount.ToString(CurrencyLargeDisplayFormat);
                    lblActualRevenue.Text = lblEACActualRevenue.Text = actualAmount.ToString(CurrencyLargeDisplayFormat);
                    lblEACRevenue.Text = EACRevenue.ToString(CurrencyLargeDisplayFormat);
                    lblEACHrs.Text = EACHours.ToString("###,###,##0.##") + " Hours";
                    lblActualHrs.Text = actualHours.ToString("###,###,##0.##") + " hours used";
                    //margin tile
                    var marginPer = actualAmount != 0 ? (margin * 100 / actualAmount) : 0;
                    lblMarginPer.Text = GreaterSeniorityExists ? "(Hidden)" : string.Format(Constants.Formatting.PercentageFormat, marginPer);
                    lblMarginAmount.Text = GreaterSeniorityExists ? "(Hidden)" : margin.ToString(CurrencyLargeDisplayFormat);
                    lblMarginGoal.Text = "Goal: " + string.Format(Constants.Formatting.PercentageFormat, ReportData.MarginGoal);

                    var actualBudgetPer = (ReportData.BudgetAmount != null && ReportData.BudgetAmount != 0M) ? (decimal)(actualAmount / ReportData.BudgetAmount) : 0;
                    pgrBudget.Attributes.CssStyle.Add("width", actualBudgetPer > 100 ? 100.ToString("#0.##%") : actualBudgetPer.ToString("#0.##%"));
                    lblBudgetPer.Text = actualBudgetPer.ToString("##0%");

                    var EACAmountPer = EACRevenue != 0M ? actualAmount / EACRevenue : 0;
                    pgrEACAmount.Attributes.CssStyle.Add("width", EACAmountPer > 100 ? 100.ToString("#0.##%") : EACAmountPer.ToString("#0.##%"));
                    lblEACAmountPer.Text = EACAmountPer.ToString("##0%");

                    var EACHrsPer = EACHours != 0M ? actualHours / EACHours : 0;
                    pgrEACHrs.Attributes.CssStyle.Add("width", EACHrsPer > 100 ? 100.ToString("#0.##%") : EACHrsPer.ToString("#0.##%"));
                    lblEACHrsPer.Text = EACHrsPer.ToString("##0%");

                    pgrHrs.Attributes.CssStyle.Add("width", ((decimal)ReportData.CompletedDays / ReportData.TotalProjectDays).ToString("#.##%"));
                    lblRemainingdays.Text = ReportData.RemainingDays.ToString() + " days remaining";

                    var marginVariance = Math.Round(marginPer, 2) - ReportData.MarginGoal;
                    if (marginVariance >= 0)
                    {
                        divMargin.Style.Add("background-color", "green");
                    }
                    else if (marginVariance < 0 && marginVariance >= -5)
                    {
                        divMargin.Style.Add("background-color", "yellow");
                        divMargin.Style.Add("color", "black");
                    }
                    else if (marginVariance < -5)
                    {
                        divMargin.Style.Add("background-color", "red");
                    }
                }
            }
            uplGraph.Update();
        }

        private void InitAxis(Axis horizAxis)
        {
            var startDate = StartDate != null ? StartDate.Value : SelectedProject.StartDate.Value;
            var endDate = EndDate != null ? EndDate.Value : SelectedProject.EndDate.Value;

            var beginPeriodLocal = startDate;
            var endPeriodLocal = endDate;
            var noOfDays = (endPeriodLocal - beginPeriodLocal).TotalDays;

            if (Step == 7 && noOfDays < 30)
            {
                if ((int)startDate.DayOfWeek > 0)
                {
                    beginPeriodLocal = startDate.AddDays(-1 * ((int)startDate.DayOfWeek));
                }
                if ((int)endDate.DayOfWeek < 6)
                {
                    endPeriodLocal = endDate.AddDays(6 - ((int)endDate.DayOfWeek));
                }
            }
            else if (Step == 30 || (Step == 7 && noOfDays > 30))
            {
                beginPeriodLocal = startDate;
                endPeriodLocal = endDate;

                if ((int)beginPeriodLocal.DayOfWeek > 0)
                {
                    beginPeriodLocal = beginPeriodLocal.AddDays(-1 * ((int)beginPeriodLocal.DayOfWeek));
                }
                if ((int)endPeriodLocal.DayOfWeek < 6)
                {
                    endPeriodLocal = endPeriodLocal.AddDays(6 - ((int)endPeriodLocal.DayOfWeek));
                }
            }
            //  Set min and max values
            horizAxis.Minimum = beginPeriodLocal.ToOADate();
            horizAxis.Maximum = endPeriodLocal.AddDays(1).ToOADate();
            horizAxis.IsLabelAutoFit = true;
            horizAxis.IsStartedFromZero = true;

            StripLine stripLine = new StripLine();
            stripLine.ForeColor = Color.Gray;
            stripLine.BorderColor = Color.Gray;
            stripLine.BorderWidth = 1;
            stripLine.StripWidthType = DateTimeIntervalType.Days;
            stripLine.Interval = 0;
            stripLine.IntervalOffset = DateTime.Today.ToOADate();
            stripLine.BorderDashStyle = ChartDashStyle.Solid;
            stripLine.ToolTip = "Today";
            horizAxis.StripLines.Add(stripLine);

            if (Step == 1 && noOfDays < 30)
            {
                horizAxis.IntervalType = DateTimeIntervalType.Days;
                horizAxis.Interval = 1;

                horizAxis.IntervalOffset = GetOffset(startDate);
                horizAxis.IntervalOffsetType = DateTimeIntervalType.Days;
            }
            else if (Step == 7 && (noOfDays < 92 || SelectedPeriod == "-3" || SelectedPeriod == "3"))
            {
                horizAxis.Minimum = beginPeriodLocal.ToOADate();
                horizAxis.Maximum = endPeriodLocal.AddDays(1).ToOADate();

                horizAxis.IntervalType = DateTimeIntervalType.Weeks;
                horizAxis.Interval = 1;

                horizAxis.IntervalOffset = 0;
                horizAxis.IntervalOffsetType = DateTimeIntervalType.Days;
            }
            else if (Step == 30 || ((Step == 7 || Step == 1) && noOfDays > 92))
            {
                var beginPeriod = startDate;
                var endPeriod = endDate;

                if ((int)beginPeriod.DayOfWeek > 1)
                {
                    double period = Convert.ToDouble("-" + (int)beginPeriod.DayOfWeek);
                    horizAxis.Minimum = beginPeriod.AddDays(period + 1).ToOADate();
                }
                if ((int)endPeriod.DayOfWeek < 6)
                {
                    double period = Convert.ToDouble((int)endPeriod.DayOfWeek);
                    horizAxis.Maximum = endPeriod.AddDays(6 - period).ToOADate();
                }

                horizAxis.IntervalType = DateTimeIntervalType.Months;
                horizAxis.Interval = 1;
            }
        }

        private int GetOffset(DateTime date)
        {
            //Offset for sunday is 0,monday is -6,tuesday is -5,wednesday is -4,thursday is -3,friday is -2,saturday is -1
            if (date.DayOfWeek == DayOfWeek.Sunday)
                return 0;
            else
                return -1 * (7 - (int)date.DayOfWeek);
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            ExportToExcel();
            UpdateGraph(true);
        }


        private void ExportToExcel(bool limitColumns = false)
        {
            showMargin = rbnMargin.Checked;
            showHours = rbnHours.Checked;
            showRevenue = rbnRevenue.Checked;
            showActual = chbGridActuals.Checked;
            showProjected = chbGridProjected.Checked;
            showEAC = chbGridEAC.Checked;
            showBudget = chbGridBudget.Checked;

            var exportDate = ServiceCallers.Custom.Report(r => r.GetBurnReportExportDataFortheProject(SelectedProject.Id.Value, SelectedPeriod == "1" ? null : StartDate, SelectedPeriod == "1" ? null : EndDate, ActualsEndDate, showBudget, showProjected, showEAC, showActual)).ToList();

            string fileName = "Burn Report-" + SelectedProject.ProjectNumber + ".xls";
            var sheetStylesList = new List<SheetStyles>();
            var dataSetList = new List<DataSet>();

            if (exportDate != null && exportDate.Count() > 0)
            {
                string Title = "Burn Report: -" + SelectedProject.ProjectNumber + SelectedProject.Name;
                string filters = "For:" + (ddlPeriod.SelectedValue != "0" ? ddlPeriod.SelectedItem.Text : StartDate.Value.ToString("MM/dd/yy") + " - " + EndDate.Value.ToString("MM/dd/yy")) + ",\n BY:" + ddlDetalization.SelectedItem.Text + ",\n Actuals:" + ddlActualPeriod.SelectedItem.Text;
                DataTable header = new DataTable();
                header.Columns.Add(Title);
                header.Rows.Add(filters);
                var data = PrepareDataTable(exportDate);
                var dataset = new DataSet();
                dataset.DataSetName = "BurnReport_" + SelectedProject.ProjectNumber;
                dataset.Tables.Add(header);

                if (limitColumns)
                {
                    DataTable temp = data.Clone();
                    for (int i = data.Columns.Count - 1; i > 255; i--)
                    {
                        temp.Columns.RemoveAt(i);
                    }

                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        var row = data.Rows[i];
                        List<object> nr = new List<object>();
                        for (int j = 0; j < 256; j++)
                        {
                            var r = row[j];
                            nr.Add(r);
                        }
                        temp.Rows.Add(nr.ToArray());
                    }
                    dataset.Tables.Add(temp);
                }
                else
                {
                    dataset.Tables.Add(data);
                }
                sheetStylesList.Add(HeaderSheetStyle);
                sheetStylesList.Add(DataSheetStyle);
                dataSetList.Add(dataset);
            }
            else
            {
                string Title = "There are no resources for the selected filters.";
                DataTable header = new DataTable();
                header.Columns.Add(Title);
                sheetStylesList.Add(HeaderSheetStyle);
                var dataset = new DataSet();
                dataset.DataSetName = "BurnReport_" + SelectedProject.ProjectNumber;
                dataset.Tables.Add(header);
                dataSetList.Add(dataset);
            }
            NPOIExcel.Export(fileName, dataSetList, sheetStylesList);
            updConsReport.Update();
        }


        private void PrepareHeaders(List<object> row, string text)
        {
            if (!showRevenue)
            {
                if (showBudget)
                {
                    row.Add("Budget " + text);
                }
                if (showActual)
                {
                    row.Add("Actual " + text);
                }
                if (showProjected)
                {
                    row.Add("Projected " + text);
                }
                if (showEAC)
                {
                    row.Add("ETC " + text);
                }
                row.Add("Variance " + text);
            }
            if (showBudget)
            {
                row.Add("Budget Revenue");
            }
            if (showActual)
            {
                row.Add("Actual Revenue");
            }
            if (showProjected)
            {
                row.Add("Projected Revenue");
            }
            if (showEAC)
            {
                row.Add("ETC Revenue");
            }
            row.Add("Variance (Revenue)");
            row.Add("Variance (%)");
        }

        private bool showMargin
        {
            get;
            set;
        }
        private bool showHours
        {
            get;
            set;
        }
        private bool showRevenue
        {
            get;
            set;
        }
        private bool showActual
        {
            get;
            set;
        }
        private bool showProjected
        {
            get;
            set;
        }
        private bool showEAC
        {
            get;
            set;
        }
        private bool showBudget
        {
            get;
            set;
        }

        private void PrepareExcelGrid(List<object> row, ComputedFinancials financials)
        {
            if (financials != null)
            {
                if (!showRevenue)
                {
                    if (showHours)
                    {
                        if (showBudget)
                        {
                            row.Add(financials.BudgetHours);
                        }
                        if (showActual)
                        {
                            row.Add(financials.ActualHours);
                        }
                        if (showProjected)
                        {
                            row.Add(financials.HoursBilled);
                        }
                        if (showEAC)
                        {
                            row.Add(financials.EACHours);
                        }
                        switch (VarianceType)
                        {
                            case 1:
                                row.Add(financials.BudgetHours - financials.HoursBilled);
                                break;
                            case 2:
                                row.Add(financials.BudgetHours - financials.EACHours);
                                break;
                            case 3:
                                row.Add(financials.BudgetHours - financials.ActualHours);
                                break;
                            case 4:
                                row.Add(financials.HoursBilled - financials.EACHours);
                                break;
                            case 5:
                                row.Add(financials.HoursBilled - financials.ActualHours);
                                break;
                            case 6:
                                row.Add(financials.EACHours - financials.ActualHours);
                                break;
                            default:
                                row.Add("Variance ");
                                break;
                        }
                    }
                    else
                    {
                        if (showBudget)
                        {
                            row.Add(financials.BudgetGrossMargin.Value);
                        }
                        if (showActual)
                        {
                            row.Add(financials.ActualGrossMargin.Value);
                        }
                        if (showProjected)
                        {
                            row.Add(financials.GrossMargin.Value);
                        }
                        if (showEAC)
                        {
                            row.Add(financials.EACGrossMargin.Value);
                        }
                        switch (VarianceType)
                        {
                            case 1:
                                row.Add(financials.BudgetGrossMargin.Value - financials.GrossMargin.Value);
                                break;
                            case 2:
                                row.Add(financials.BudgetGrossMargin.Value - financials.EACGrossMargin.Value);
                                break;
                            case 3:
                                row.Add(financials.BudgetGrossMargin.Value - financials.ActualGrossMargin.Value);
                                break;
                            case 4:
                                row.Add(financials.GrossMargin.Value - financials.EACGrossMargin.Value);
                                break;
                            case 5:
                                row.Add(financials.GrossMargin.Value - financials.ActualGrossMargin.Value);
                                break;
                            case 6:
                                row.Add(financials.EACGrossMargin.Value - financials.ActualGrossMargin.Value);
                                break;
                            default:
                                row.Add("Variance ");
                                break;
                        }
                    }
                }
                if (showBudget)
                {
                    row.Add(financials.BudgetRevenue.Value);
                }
                if (showActual)
                {
                    row.Add(financials.ActualRevenue.Value);
                }
                if (showProjected)
                {
                    row.Add(financials.Revenue.Value);
                }
                if (showEAC)
                {
                    row.Add(financials.EACRevenue.Value);
                }
                decimal revenueVariance = 0M;
                switch (VarianceType)
                {
                    case 1:
                        revenueVariance = financials.BudgetRevenue.Value - financials.Revenue.Value;
                        row.Add(revenueVariance);
                        row.Add(financials.BudgetRevenue.Value != 0 ? revenueVariance / financials.BudgetRevenue.Value : 0);
                        break;
                    case 2:
                        revenueVariance = financials.BudgetRevenue.Value - financials.EACRevenue.Value;
                        row.Add(revenueVariance);
                        row.Add(financials.BudgetRevenue.Value != 0 ? revenueVariance / financials.BudgetRevenue.Value : 0);
                        break;
                    case 3:
                        revenueVariance = financials.BudgetRevenue.Value - financials.ActualRevenue.Value;
                        row.Add(revenueVariance);
                        row.Add(financials.BudgetRevenue.Value != 0 ? revenueVariance / financials.BudgetRevenue.Value : 0);
                        break;
                    case 4:
                        revenueVariance = financials.Revenue.Value - financials.EACRevenue.Value;
                        row.Add(revenueVariance);
                        row.Add(financials.Revenue.Value != 0 ? revenueVariance / financials.Revenue.Value : 0);
                        break;
                    case 5:
                        revenueVariance = financials.Revenue.Value - financials.ActualRevenue.Value;
                        row.Add(revenueVariance);
                        row.Add(financials.Revenue.Value != 0 ? revenueVariance / financials.Revenue.Value : 0);
                        break;
                    case 6:
                        revenueVariance = financials.EACRevenue.Value - financials.ActualRevenue.Value;
                        row.Add(revenueVariance);
                        row.Add(financials.EACRevenue.Value != 0 ? revenueVariance / financials.EACRevenue.Value : 0);
                        break;
                    default:
                        row.Add("Variance ");
                        row.Add("Variance %");
                        break;
                }
            }
            else
            {
                if (!showRevenue)
                {
                    row.Add("");
                    row.Add("");
                    row.Add("");
                }
                row.Add("");
                row.Add("");
                row.Add("");
                row.Add("");

            }
        }

        private int VarianceType
        {
            get;
            set;
        }

        private List<DateTime> GetWeeksInPeriod()
        {
            var startdate = SelectedPeriod == "1" ? SelectedProject.StartDate.Value : StartDate.Value;
            var endDate = SelectedPeriod == "1" ? SelectedProject.EndDate.Value : EndDate.Value;

            var weeks = new List<DateTime>();

            var temp = startdate;
            while (temp <= endDate)
            {
                weeks.Add(temp);
                temp = temp.AddDays(7);
            }

            return weeks;
        }

        public DataTable PrepareDataTable(List<ProjectBudgetResource> Resources)
        {
            DateTime now = SettingsHelper.GetCurrentPMTime();
            DataTable data = new DataTable();
            List<object> row;



            if (showBudget && showProjected)
            {
                VarianceType = 1;
            }
            if (showBudget && showEAC)
            {
                VarianceType = 2;
            }
            if (showBudget && showActual)
            {
                VarianceType = 3;
            }
            if (showProjected && showEAC)
            {
                VarianceType = 4;
            }
            if (showProjected && showActual)
            {
                VarianceType = 5;
            }
            if (showEAC && showActual)
            {
                VarianceType = 6;
            }


            data.Columns.Add("Resource");
            data.Columns.Add("Role");
            data.Columns.Add("Bill Rate");

            var Weeklykeys = GetWeeksInPeriod();

            numberOfWeeks = Weeklykeys.Count;

            for (int i = 0; i < numberOfWeeks; i++)
            {
                if (!showRevenue)
                {
                    data.Columns.Add(Weeklykeys[i].ToString("MM/dd/yyyy"));
                    data.Columns.Add(Weeklykeys[i].ToString("MM-dd-yy") + i.ToString());
                    data.Columns.Add(Weeklykeys[i].ToString("MM/dd/yyyy HH:mm") + i.ToString());
                }

                data.Columns.Add(showRevenue ? Weeklykeys[i].ToString("MM/dd/yyyy") : Weeklykeys[i].ToString("MM/dd/yyyy") + i.ToString());
                data.Columns.Add(Weeklykeys[i].ToString("MM/dd/yyyy hh:mm tt") + i.ToString());
                data.Columns.Add(Weeklykeys[i].ToString("yyyy/MM/dd") + i.ToString());
                data.Columns.Add(Weeklykeys[i].ToString("yyyy'-'MM'-'dd HH':'mm':'ss'Z'") + i.ToString());
            }

            row = new List<object>();

            row.Add("Resource");
            row.Add("Role");
            row.Add("Bill Rate");



            for (int i = 0; i < numberOfWeeks; i++)
            {
                if (showHours)
                {
                    PrepareHeaders(row, "Hours");
                }
                if (showMargin)
                {
                    PrepareHeaders(row, "Margin");
                }
                if (showRevenue)
                {
                    PrepareHeaders(row, "");
                }
            }

            data.Rows.Add(row.ToArray());
            var SummaryFinancial = new Dictionary<DateTime, ComputedFinancials>();
            foreach (var resource in Resources)
            {
                row = new List<object>();
                row.Add(resource.Person.LastName + ", " + resource.Person.FirstName);
                row.Add(resource.Role != null ? resource.Role.Name : "");
                row.Add(resource.BillRate);

                foreach (var key in Weeklykeys)
                {

                    foreach (var revenue in resource.WeeklyFinancials)
                    {
                        if (revenue.Key == key)
                        {
                            PrepareExcelGrid(row, revenue.Value);
                            if (SummaryFinancial.Any(_ => _.Key == revenue.Key))
                            {
                                var temp = SummaryFinancial.FirstOrDefault(_ => _.Key == revenue.Key);
                                temp.Value.BudgetHours += revenue.Value.BudgetHours;
                                temp.Value.ActualHours += revenue.Value.ActualHours;
                                temp.Value.HoursBilled += revenue.Value.HoursBilled;
                                temp.Value.EACHours += revenue.Value.EACHours;

                                temp.Value.BudgetRevenue += revenue.Value.BudgetRevenue;
                                temp.Value.ActualRevenue += revenue.Value.ActualRevenue;
                                temp.Value.Revenue += revenue.Value.Revenue;
                                temp.Value.EACRevenue += revenue.Value.EACRevenue;

                                temp.Value.BudgetGrossMargin += revenue.Value.BudgetGrossMargin;
                                temp.Value.ActualGrossMargin += revenue.Value.ActualGrossMargin;
                                temp.Value.GrossMargin += revenue.Value.GrossMargin;
                                temp.Value.EACGrossMargin += revenue.Value.EACGrossMargin;

                            }
                            else
                            {
                                SummaryFinancial.Add(revenue.Key, revenue.Value);
                            }
                            break;
                        }

                    }
                    if (!resource.WeeklyFinancials.Any(_ => _.Key == key))
                    {
                        PrepareExcelGrid(row, null);
                    }

                }
                data.Rows.Add(row.ToArray());
            }
            //Services Total
            row = new List<object>();
            for (int i = 0; i < data.Columns.Count; i++)
            {
                row.Add("");
            }
            data.Rows.Add(row.ToArray());

            row = new List<object>();
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Grand Total"));
            row.Add("");
            row.Add("");
            foreach (var key in Weeklykeys)
            {
                foreach (var rev in SummaryFinancial)
                {
                    if (rev.Key == key)
                    {
                        PrepareExcelGrid(row, rev.Value);
                    }
                }
            }
            data.Rows.Add(row.ToArray());

            return data;
        }

        protected void ddlClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAndAddFirsItemForDdlProjects();

            if (ddlClients.SelectedIndex != 0)
            {
                ddlProjects.Enabled = true;

                int clientId = Convert.ToInt32(ddlClients.SelectedItem.Value);

                var projects = DataHelper.GetBurnReportProjectByClient(clientId, HttpContext.Current.User.Identity.Name);
                projects = projects.OrderBy(p => p.Status.Name).ThenBy(p => p.ProjectNumber).ToArray();

                foreach (var project in projects)
                {
                    if (project.ProjectNumber == "P000952")
                    { }
                    var li = new System.Web.UI.WebControls.ListItem(project.ProjectNumber + " - " + project.Name,
                                           project.ProjectNumber.ToString());
                    ddlProjects.Items.Add(li);
                }
            }

            mpeProjectSearch.Show();
        }

        protected void ddlProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlProjects.SelectedValue != string.Empty)
            {
                var projectNumber = ddlProjects.SelectedItem.Value;

                PopulateControls(projectNumber);
            }
            else
            {
                mpeProjectSearch.Show();
            }
        }

        private void PopulateControls(string projectNumber)
        {
            txtProjectNumber.Text = projectNumber;
        }

        protected void lnkProjectNumber_Click(object sender, EventArgs e)
        {
            var lnkProjectNumber = sender as LinkButton;
            PopulateControls(lnkProjectNumber.Attributes["ProjectNumber"]);
        }

        protected void btnclose_Click(object sender, EventArgs e)
        {
            ClearAndAddFirsItemForDdlProjects();
            ddlProjects.SelectedIndex = ddlClients.SelectedIndex = 0;
        }

        private void ClearAndAddFirsItemForDdlProjects()
        {
            System.Web.UI.WebControls.ListItem firstItem = new System.Web.UI.WebControls.ListItem("-- Select a Project --", string.Empty);
            ddlProjects.Items.Clear();
            ddlProjects.Items.Add(firstItem);
            ddlProjects.Enabled = false;
        }

        protected void repResources_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            ScriptManager.RegisterStartupScript(this.Page, Page.GetType(), "", "ShowColumns();", true);

            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var resource = e.Item.DataItem as ProjectBudgetResource;

                var lblBudMargin = e.Item.FindControl("lblBudMargin") as Label;
                var lblActMargin = e.Item.FindControl("lblActMargin") as Label;
                var lblProjMargin = e.Item.FindControl("lblProjMargin") as Label;
                var lblEACMargin = e.Item.FindControl("lblEACMargin") as Label;



                lblBudMargin.Text = resource.Budget.Margin.ToString(GreaterSeniorityExists);
                lblActMargin.Text = resource.Actuals.Margin.ToString(GreaterSeniorityExists);
                lblProjMargin.Text = resource.Projected.Margin.ToString(GreaterSeniorityExists);
                lblEACMargin.Text = resource.EAC.Margin.ToString(GreaterSeniorityExists);



                totalBudgetHrs += resource.Budget.Hours;
                totalActualHrs += resource.Actuals.Hours;
                totalProjHrs += resource.Projected.Hours;
                totalEACHrs += resource.EAC.Hours;

                totalBudgetRevenue += resource.Budget.Revenue;
                totalBudgetMargin += resource.Budget.Margin;

                totalActualRevenue += resource.Actuals.Revenue;
                totalActualMargin += resource.Actuals.Margin;

                totalProjRevenue += resource.Projected.Revenue;
                totalProjMargin += resource.Projected.Margin;

                totalEACRevenue += resource.EAC.Revenue;
                totalEACMargin += resource.EAC.Margin;
            }
            if (e.Item.ItemType == ListItemType.Footer)
            {


                var lblBudgetHrs = e.Item.FindControl("lblBudgetHrs") as Label;
                var lblBudgetRev = e.Item.FindControl("lblBudgetRev") as Label;
                var lblBudgetMargin = e.Item.FindControl("lblBudgetMargin") as Label;
                var lblActualHrs = e.Item.FindControl("lblActualHrs") as Label;
                var lblActualRev = e.Item.FindControl("lblActualRev") as Label;
                var lblActualMargin = e.Item.FindControl("lblActualMargin") as Label;
                var lblProjHrs = e.Item.FindControl("lblProjHrs") as Label;
                var lblProjRev = e.Item.FindControl("lblProjRev") as Label;
                var lblProjMargin = e.Item.FindControl("lblProjMargin") as Label;
                var lblEACHrs = e.Item.FindControl("lblEACHrs") as Label;
                var lblEACRev = e.Item.FindControl("lblEACRev") as Label;
                var lblEACMargin = e.Item.FindControl("lblEACMargin") as Label;

                lblBudgetHrs.Text = totalBudgetHrs.ToString("###,###,##0.##");
                lblBudgetRev.Text = totalBudgetRevenue.ToString();
                lblBudgetMargin.Text = GreaterSeniorityExists ? "(Hidden)" : totalBudgetMargin.ToString();
                lblActualHrs.Text = totalActualHrs.ToString("###,###,##0.##");
                lblActualRev.Text = totalActualRevenue.ToString();
                lblActualMargin.Text = GreaterSeniorityExists ? "(Hidden)" : totalActualMargin.ToString();
                lblProjHrs.Text = totalProjHrs.ToString("###,###,##0.##");
                lblProjRev.Text = totalProjRevenue.ToString();
                lblProjMargin.Text = GreaterSeniorityExists ? "(Hidden)" : totalProjMargin.ToString();
                lblEACHrs.Text = totalEACHrs.ToString("###,###,##0.##");
                lblEACRev.Text = totalEACRevenue.ToString();
                lblEACMargin.Text = GreaterSeniorityExists ? "(Hidden)" : totalEACMargin.ToString();

            }
        }

        protected void repExpenses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var expense = e.Item.DataItem as ProjectExpense;

                TotalBudgetExpense += expense.BudgetAmount;
                TotalActualExpense += expense.Amount != null ? expense.Amount.Value : 0;
                TotalProjectedExpense += expense.ExpectedAmount;
                TotalEACExpense += expense.EACAmount;

            }
            if (e.Item.ItemType == ListItemType.Footer)
            {
                var lblTotalBudgetExpense = e.Item.FindControl("lblTotalBudgetExpense") as Label;
                var lblTotalActualExpense = e.Item.FindControl("lblTotalActualExpense") as Label;
                var lblTotalProjectedExpense = e.Item.FindControl("lblTotalProjectedExpense") as Label;
                var lblTotalEACExpense = e.Item.FindControl("lblTotalEACExpense") as Label;

                lblTotalBudgetExpense.Text = TotalBudgetExpense.Value.ToString("$###,###,##0");
                lblTotalActualExpense.Text = TotalActualExpense.Value.ToString("$###,###,##0");
                lblTotalProjectedExpense.Text = TotalProjectedExpense.Value.ToString("$###,###,##0");
                lblTotalEACExpense.Text = TotalEACExpense.Value.ToString("$###,###,##0");
            }
        }

        protected void chbBudget_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGraph(true);
        }

        protected void lnkViewAllResources_Click(object sender, EventArgs e)
        {
            PersonListAnalyzer = new SeniorityAnalyzer(DataHelper.CurrentPerson);
            PersonListAnalyzer.OneWithGreaterSeniorityExists(SelectedProject.ProjectPersons);

            if (ReportData.Resources != null && ReportData.Resources.Count > 0)
            {
                repResources.DataSource = ReportData.Resources;
                repResources.DataBind();
            }
            if (ReportData.Expenses != null && ReportData.Expenses.Count > 0)
            {
                repExpenses.DataSource = ReportData.Expenses;
                repExpenses.DataBind();
            }

        }

        private void SaveFilterValuesForSession()
        {
            TimeReports filter = new TimeReports();
            filter.ProjectNumber = txtProjectNumber.Text;
            filter.ReportPeriod = ddlPeriod.SelectedValue;
            filter.SelectedView = ddlDetalization.SelectedValue;
            filter.StartDate = diRange.FromDate.Value;
            filter.EndDate = diRange.ToDate.Value;
            filter.ActualsEndDate = ddlActualPeriod.SelectedValue;
            ReportsFilterHelper.SaveFilterValues(ReportName.BurnReport, filter);
        }

        private void GetFilterValuesForSession()
        {
            var filters = ReportsFilterHelper.GetFilterValues(ReportName.BurnReport) as TimeReports;
            if (filters != null)
            {
                txtProjectNumber.Text = filters.ProjectNumber;
                ddlPeriod.SelectedValue = filters.ReportPeriod;
                ddlDetalization.SelectedValue = filters.SelectedView;
                diRange.FromDate = filters.StartDate;
                diRange.ToDate = filters.EndDate;
                ddlActualPeriod.SelectedValue = filters.ActualsEndDate;
                IsFromBrowserSession = true;
            }
            else
            {
                IsFromBrowserSession = false;
            }
        }

        public void PDFExport()
        {
            HtmlToPdfBuilder builder = new HtmlToPdfBuilder(iTextSharp.text.PageSize.A4);
            string filename = "Burn Report_" + SelectedProject.ProjectNumber + ".pdf";
            byte[] pdfDataInBytes = this.RenderPdf(builder);

            HttpContext.Current.Response.ContentType = "Application/pdf";
            HttpContext.Current.Response.AddHeader(
                "content-disposition", string.Format("attachment; filename={0}", Utils.Generic.EncodedFileName(filename)));

            int len = pdfDataInBytes.Length;
            int bytes;
            byte[] buffer = new byte[1024];
            Stream outStream = HttpContext.Current.Response.OutputStream;
            using (MemoryStream stream = new MemoryStream(pdfDataInBytes))
            {
                while (len > 0 && (bytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, bytes);
                    HttpContext.Current.Response.Flush();
                    len -= bytes;
                }
            }
        }

        private byte[] RenderPdf(HtmlToPdfBuilder builder)
        {
            ChartForPdf();
            int pageCount = GetPageCount(builder);
            MemoryStream file = new MemoryStream();
            Document document = new Document(builder.PageSize);
            document.SetPageSize(iTextSharp.text.PageSize.A4_LANDSCAPE.Rotate());

            MyPageEventHandler e = new MyPageEventHandler()
            {
                PageCount = pageCount,
                PageNo = 1
            };
            PdfWriter writer = PdfWriter.GetInstance(document, file);
            writer.PageEvent = e;
            document.Open();

            string reportHeader = "Burn Report: " + SelectedProject.ProjectNumber + " - " + SelectedProject.Name;
            iTextSharp.text.Font contentFont = iTextSharp.text.FontFactory.GetFont("Webdings", 18, iTextSharp.text.Font.NORMAL);
            var headerPara = new Paragraph(reportHeader, contentFont);

            headerPara.Alignment = Element.ALIGN_CENTER;
            document.Add(headerPara);
            document.Add(new Paragraph("\n"));

            document.Add(ConsultingImage());
            document.Add(TileHtmlToPDF());

            var styles = new List<TrStyles>();

            var resources = ReportData.Resources;

            if (resources != null && resources.Count > 0)
            {
                resources = resources.OrderBy(r => r.Person.LastName).ToList();
                document.NewPage();
                string reportDataInPdfString = string.Empty;

                reportDataInPdfString += "Resource" + ColoumSpliter;

                if (showBudget)
                {
                    reportDataInPdfString += "Budget " + (showHours ? "Hours" : showRevenue ? "Revenue" : "Margin") + ColoumSpliter;
                }

                if (showProjected)
                {
                    reportDataInPdfString += "Projected " + (showHours ? "Hours" : showRevenue ? "Revenue" : "Margin") + ColoumSpliter;
                }
                if (showEAC)
                {
                    reportDataInPdfString += "ETC " + (showHours ? "Hours" : showRevenue ? "Revenue" : "Margin") + ColoumSpliter;
                }
                if (showActual)
                {
                    reportDataInPdfString += "Actual " + (showHours ? "Hours" : showRevenue ? "Revenue" : "Margin") + ColoumSpliter;
                }
                reportDataInPdfString += "Variance" + ColoumSpliter;

                reportDataInPdfString += "Variance %" + RowSpliter;

                foreach (var resource in resources)
                {
                    reportDataInPdfString += PrepareResourcePDFData(resource);
                }



                var grandTotal = new ProjectBudgetResource();
                grandTotal.Budget = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.Budget.Hours),
                    Margin = resources.Sum(_ => _.Budget.Margin),
                    Revenue = resources.Sum(_ => _.Budget.Revenue)
                };

                grandTotal.Actuals = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.Actuals.Hours),
                    Margin = resources.Sum(_ => _.Actuals.Margin),
                    Revenue = resources.Sum(_ => _.Actuals.Revenue)
                };

                grandTotal.Projected = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.Projected.Hours),
                    Margin = resources.Sum(_ => _.Projected.Margin),
                    Revenue = resources.Sum(_ => _.Projected.Revenue)
                };

                grandTotal.ProjectedRemaining = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.ProjectedRemaining.Hours),
                    Margin = resources.Sum(_ => _.ProjectedRemaining.Margin),
                    Revenue = resources.Sum(_ => _.ProjectedRemaining.Revenue)
                };

                grandTotal.EAC = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.EAC.Hours),
                    Margin = resources.Sum(_ => _.EAC.Margin),
                    Revenue = resources.Sum(_ => _.EAC.Revenue)
                };

                reportDataInPdfString += PrepareResourcePDFData(grandTotal, true);

                var table = builder.GetPdftable(reportDataInPdfString, PdfProjectListTableStyle, RowSpliter, ColoumSpliter);
                document.Add((IElement)table);
            }
            else
            {
                document.Add((IElement)PDFHelper.GetPdfHeaderLogo());
            }


            if (ReportData.Expenses != null && ReportData.Expenses.Count > 0)
            {
                document.NewPage();
                string reportDataInPdfString = string.Empty;
                reportDataInPdfString += "Expense" + ColoumSpliter;
                if (showBudget)
                {
                    reportDataInPdfString += "Budget" + ColoumSpliter;
                }
                if (showProjected)
                {
                    reportDataInPdfString += "Projected" + ColoumSpliter;
                }
                if (showEAC)
                {
                    reportDataInPdfString += "ETC" + ColoumSpliter;
                }
                if (showActual)
                {
                    reportDataInPdfString += "Actual" + ColoumSpliter;
                }
                reportDataInPdfString += "Variance" + ColoumSpliter + "Variance %";
                reportDataInPdfString += RowSpliter;

                var expenses = ReportData.Expenses.OrderBy(_ => _.HtmlEncodedName).ToList();

                var totalExpense = new ProjectExpense();

                foreach (var expense in expenses)
                {
                    reportDataInPdfString += PreareExpensePDF(expense);
                }
                totalExpense.Name = "Grand Total";
                totalExpense.ExpectedAmount = expenses.Sum(_ => _.ExpectedAmount);
                totalExpense.Amount = expenses.Sum(_ => _.Amount);
                totalExpense.BudgetAmount = expenses.Sum(_ => _.BudgetAmount);
                totalExpense.ProjectedRemainingAmount = expenses.Sum(_ => _.ProjectedRemainingAmount);
                reportDataInPdfString += PreareExpensePDF(totalExpense);

                var table = builder.GetPdftable(reportDataInPdfString, PdfProjectListTableStyle, RowSpliter, ColoumSpliter);
                document.Add((IElement)table);
            }
            else
            {
                document.Add((IElement)PDFHelper.GetPdfHeaderLogo());
            }

            document.Close();
            return file.ToArray();
        }

        private string PreareExpensePDF(ProjectExpense expense)
        {
            string reportDataInPdfString = "";
            reportDataInPdfString += expense.Name + ColoumSpliter;
            if (showBudget)
            {
                reportDataInPdfString += expense.BudgetAmount.ToString(CurrencyLargeDisplayFormat) + ColoumSpliter;
            }
            if (showProjected)
            {
                reportDataInPdfString += expense.ExpectedAmount.ToString(CurrencyLargeDisplayFormat) + ColoumSpliter;
            }
            if (showEAC)
            {
                reportDataInPdfString += expense.EACAmount.ToString(CurrencyLargeDisplayFormat) + ColoumSpliter;
            }
            if (showActual)
            {
                reportDataInPdfString += (expense.Amount != null ? expense.Amount.Value.ToString(CurrencyLargeDisplayFormat) : 0.ToString(CurrencyLargeDisplayFormat)) + ColoumSpliter;
            }

            decimal varianceExpense = 0;
            decimal varianceExpensePer = 0;

            switch (VarianceType)
            {
                case 1:
                    varianceExpense = expense.BudgetAmount - expense.ExpectedAmount;
                    varianceExpensePer = expense.BudgetAmount != 0 ? varianceExpense / expense.BudgetAmount : 0;
                    break;
                case 2:

                    varianceExpense = expense.BudgetAmount - expense.EACAmount;
                    varianceExpensePer = expense.BudgetAmount != 0 ? varianceExpense / expense.BudgetAmount : 0;
                    break;
                case 3:


                    varianceExpense = expense.BudgetAmount - expense.Amount.Value;
                    varianceExpensePer = expense.BudgetAmount != 0 ? varianceExpense / expense.BudgetAmount : 0;

                    break;
                case 4:

                    varianceExpense = expense.ExpectedAmount - expense.EACAmount;
                    varianceExpensePer = expense.ExpectedAmount != 0 ? varianceExpense / expense.ExpectedAmount : 0;
                    break;
                case 5:
                    varianceExpense = expense.ExpectedAmount - expense.Amount.Value;
                    varianceExpensePer = expense.ExpectedAmount != 0 ? varianceExpense / expense.ExpectedAmount : 0;
                    break;
                case 6:

                    varianceExpense = expense.EACAmount - expense.Amount.Value;
                    varianceExpensePer = expense.EACAmount != 0 ? varianceExpense / expense.EACAmount : 0;
                    break;

            }
            reportDataInPdfString += varianceExpense.ToString(CurrencyLargeDisplayFormat) + ColoumSpliter;
            reportDataInPdfString += varianceExpensePer.ToString("P") + RowSpliter;
            return reportDataInPdfString;
        }

        public void ChartForPdf()
        {
            ReportData.Financials = Financials;

            if (ReportData.Financials != null && ReportData.Financials.Count > 0)
            {
                ReportData.Financials.OrderBy(r => r.Key);

                var act = new KeyValuePair<DateTime, ComputedFinancials>();
                var isActualsExist = ReportData.Financials.Any(_ => _.Value.ActualRevenue != 0);
                if (isActualsExist)
                {
                    act = ReportData.Financials.Last(_ => _.Value.ActualRevenue != 0);
                }
                InitAxis(chartPdf.ChartAreas["MainArea"].AxisX);
                foreach (KeyValuePair<DateTime, ComputedFinancials> financial in ReportData.Financials)
                {
                    if (isActualsExist && chbActuals.Checked && financial.Key <= act.Key)
                    {
                        chartPdf.Series[ActualSeriesIndex].Points.AddXY(financial.Key, financial.Value.ActualRevenue.Value);
                    }
                    if (chbBudget.Checked)
                    {
                        chartPdf.Series[BudgetSeriesIndex].Points.AddXY(financial.Key, financial.Value.BudgetRevenue.Value);
                    }
                    if (chbProjected.Checked)
                    {
                        chartPdf.Series[ProjectedSeriesIndex].Points.AddXY(financial.Key, financial.Value.Revenue.Value);
                    }
                    if (ReportData.BudgetAmount != null)
                    {
                        chartPdf.Series[BudgetAmountSeriesIndex].Points.AddXY(financial.Key, ReportData.BudgetAmount);// update with budget variable
                    }
                }
            }
        }

        public PdfPTable ConsultingImage()
        {
            PdfPTable headerTable = new PdfPTable(1);
            MemoryStream chartImage = new MemoryStream();
            chartPdf.SaveImage(chartImage, ChartImageFormat.Png);

            System.Drawing.Image img = System.Drawing.Image.FromStream(chartImage);
            Bitmap objBitmap = new Bitmap(img, new Size(img.Width, img.Height + 50));
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance((System.Drawing.Image)objBitmap, ImageFormat.Png);
            PdfPCell logo = new PdfPCell(image, true);

            logo.Border = PdfPCell.NO_BORDER;
            headerTable.AddCell(logo);
            return headerTable;
        }

        private PdfPTable TileHtmlToPDF()
        {
            PdfPTable headerTable = new PdfPTable(1);

            var a = hdnPdfData.Value;
            var base64Data = Regex.Match(hdnPdfData.Value, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            byte[] bytes = Convert.FromBase64String(base64Data);

            MemoryStream ms = new MemoryStream(bytes);

            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            Bitmap objBitmap = new Bitmap(img, new Size(img.Width, img.Height));
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance((System.Drawing.Image)objBitmap, ImageFormat.Png);
            PdfPCell logo = new PdfPCell(image, true);

            logo.Border = PdfPCell.NO_BORDER;
            headerTable.AddCell(logo);
            return headerTable;
        }

        private int GetPageCount(HtmlToPdfBuilder builder)
        {
            MemoryStream file = new MemoryStream();
            Document document = new Document(builder.PageSize);
            document.SetPageSize(iTextSharp.text.PageSize.A4_LANDSCAPE.Rotate());
            MyPageEventHandler e = new MyPageEventHandler()
            {
                PageCount = 0,
                PageNo = 1
            };
            PdfWriter writer = PdfWriter.GetInstance(document, file);
            writer.PageEvent = e;
            document.Open();
            document.Add(new Paragraph(""));
            document.Add(new Paragraph("\n"));
            document.Add(ConsultingImage());
            document.Add(TileHtmlToPDF());


            var styles = new List<TrStyles>();

            var resources = ReportData.Resources;

            if (resources != null && resources.Count > 0)
            {
                resources = resources.OrderBy(r => r.Person.LastName).ToList();
                document.NewPage();
                string reportDataInPdfString = string.Empty;

                reportDataInPdfString += "Resource" + ColoumSpliter;

                if (showBudget)
                {
                    reportDataInPdfString += "Budget " + (showHours ? "Hours" : showRevenue ? "Revenue" : "Margin") + ColoumSpliter;
                }

                if (showProjected)
                {
                    reportDataInPdfString += "Projected " + (showHours ? "Hours" : showRevenue ? "Revenue" : "Margin") + ColoumSpliter;
                }
                if (showEAC)
                {
                    reportDataInPdfString += "ETC " + (showHours ? "Hours" : showRevenue ? "Revenue" : "Margin") + ColoumSpliter;
                }
                if (showActual)
                {
                    reportDataInPdfString += "Actual " + (showHours ? "Hours" : showRevenue ? "Revenue" : "Margin") + ColoumSpliter;
                }
                reportDataInPdfString += "Variance" + ColoumSpliter;

                reportDataInPdfString += "Variance %" + ColoumSpliter + RowSpliter;

                foreach (var resource in resources)
                {
                    reportDataInPdfString += PrepareResourcePDFData(resource);
                }

                var grandTotal = new ProjectBudgetResource();
                grandTotal.Budget = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.Budget.Hours),
                    Margin = resources.Sum(_ => _.Budget.Margin),
                    Revenue = resources.Sum(_ => _.Budget.Revenue)
                };

                grandTotal.Actuals = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.Actuals.Hours),
                    Margin = resources.Sum(_ => _.Actuals.Margin),
                    Revenue = resources.Sum(_ => _.Actuals.Revenue)
                };

                grandTotal.Projected = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.Projected.Hours),
                    Margin = resources.Sum(_ => _.Projected.Margin),
                    Revenue = resources.Sum(_ => _.Projected.Revenue)
                };

                grandTotal.ProjectedRemaining = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.ProjectedRemaining.Hours),
                    Margin = resources.Sum(_ => _.ProjectedRemaining.Margin),
                    Revenue = resources.Sum(_ => _.ProjectedRemaining.Revenue)
                };

                grandTotal.EAC = new ProjectRevenue
                {
                    Hours = resources.Sum(_ => _.EAC.Hours),
                    Margin = resources.Sum(_ => _.EAC.Margin),
                    Revenue = resources.Sum(_ => _.EAC.Revenue)
                };

                reportDataInPdfString += PrepareResourcePDFData(grandTotal, true);

                var table = builder.GetPdftable(reportDataInPdfString, PdfProjectListTableStyle, RowSpliter, ColoumSpliter);
                document.Add((IElement)table);
            }
            else
            {
                document.Add((IElement)PDFHelper.GetPdfHeaderLogo());
            }

            if (ReportData.Expenses != null && ReportData.Expenses.Count > 0)
            {
                document.NewPage();
                string reportDataInPdfString = string.Empty;

                reportDataInPdfString += "Expense" + ColoumSpliter;
                if (showBudget)
                {
                    reportDataInPdfString += "Budget" + ColoumSpliter;
                }
                if (showProjected)
                {
                    reportDataInPdfString += "Projected" + ColoumSpliter;
                }
                if (showEAC)
                {
                    reportDataInPdfString += "ETC" + ColoumSpliter;
                }
                if (showActual)
                {
                    reportDataInPdfString += "Actual" + ColoumSpliter;
                }
                reportDataInPdfString += "Variance" + ColoumSpliter + "Variance %";
                reportDataInPdfString += RowSpliter;

                var expenses = ReportData.Expenses;
                var totalExpense = new ProjectExpense();

                foreach (var expense in expenses)
                {
                    reportDataInPdfString += PreareExpensePDF(expense);
                }
                totalExpense.Name = "Grand Total";
                totalExpense.ExpectedAmount = expenses.Sum(_ => _.ExpectedAmount);
                totalExpense.Amount = expenses.Sum(_ => _.Amount);
                totalExpense.BudgetAmount = expenses.Sum(_ => _.BudgetAmount);
                totalExpense.ProjectedRemainingAmount = expenses.Sum(_ => _.ProjectedRemainingAmount);
                reportDataInPdfString += PreareExpensePDF(totalExpense);
                var table = builder.GetPdftable(reportDataInPdfString, PdfProjectListTableStyle, RowSpliter, ColoumSpliter);
                document.Add((IElement)table);
            }
            else
            {
                document.Add((IElement)PDFHelper.GetPdfHeaderLogo());
            }

            return writer.CurrentPageNumber;
        }

        private string PrepareResourcePDFData(ProjectBudgetResource resource, bool isSummary = false)
        {
            string reportDataInPdfString = string.Empty;

            decimal varHours = 0;
            decimal varPer = 0;
            PracticeManagementCurrency varAmount = 0;

            reportDataInPdfString += (isSummary ? "Grand Total" : resource.Person.LastName + ", " + resource.Person.FirstName) + ColoumSpliter;

            if (showHours)
            {
                if (showBudget)
                {
                    reportDataInPdfString += resource.Budget.Hours.ToString("###,###,##0.##") + ColoumSpliter;
                }

                if (showProjected)
                {
                    reportDataInPdfString += resource.Projected.Hours.ToString("###,###,##0.##") + ColoumSpliter;
                }
                if (showEAC)
                {
                    try
                    {
                        reportDataInPdfString += resource.EAC.Hours.ToString("###,###,##0.##") + ColoumSpliter;
                    }
                    catch (Exception e)
                    {
                        var a = e.InnerException;
                    }
                }
                if (showActual)
                {

                    reportDataInPdfString += resource.Actuals.Hours.ToString("###,###,##0.##") + ColoumSpliter;

                }

                switch (VarianceType)
                {
                    case 1:
                        varHours = resource.Budget.Hours - resource.Projected.Hours;
                        varPer = resource.Budget.Hours != 0 ? varHours / resource.Budget.Hours : -1;
                        break;
                    case 2:
                        varHours = resource.Budget.Hours - resource.EAC.Hours;
                        varPer = resource.Budget.Hours != 0 ? varHours / resource.Budget.Hours : -1;
                        break;
                    case 3:
                        varHours = resource.Budget.Hours - resource.Actuals.Hours;
                        varPer = resource.Budget.Hours != 0 ? varHours / resource.Budget.Hours : -1;
                        break;
                    case 4:
                        varHours = resource.Projected.Hours - resource.EAC.Hours;
                        varPer = resource.Projected.Hours != 0 ? varHours / resource.Projected.Hours : -1;
                        break;
                    case 5:
                        varHours = resource.Projected.Hours - resource.Actuals.Hours;
                        varPer = resource.Projected.Hours != 0 ? varHours / resource.Projected.Hours : -1;
                        break;
                    case 6:
                        varHours = resource.EAC.Hours - resource.Actuals.Hours;
                        varPer = resource.EAC.Hours != 0 ? varHours / resource.EAC.Hours : -1;
                        break;

                }
                reportDataInPdfString += varHours.ToString("###,###,##0.##") + ColoumSpliter;
                reportDataInPdfString += varPer.ToString("P");
            }
            else if (showMargin)
            {
                if (showBudget)
                {
                    reportDataInPdfString += (GreaterSeniorityExists ? HideText : resource.Budget.Margin.Value.ToString(CurrencyLargeDisplayFormat)) + ColoumSpliter;
                }

                if (showProjected)
                {
                    reportDataInPdfString += (GreaterSeniorityExists ? HideText : resource.Projected.Margin.Value.ToString(CurrencyLargeDisplayFormat)) + ColoumSpliter;
                }
                if (showEAC)
                {
                    reportDataInPdfString += (GreaterSeniorityExists ? HideText : resource.EAC.Margin.Value.ToString(CurrencyLargeDisplayFormat)) + ColoumSpliter;
                }
                if (showActual)
                {
                    reportDataInPdfString += (GreaterSeniorityExists ? HideText : resource.Actuals.Margin.Value.ToString(CurrencyLargeDisplayFormat)) + ColoumSpliter;
                }
                switch (VarianceType)
                {
                    case 1:
                        varAmount = resource.Budget.Margin - resource.Projected.Margin;
                        varPer = resource.Budget.Margin != 0 ? varAmount / resource.Budget.Margin : -1;
                        break;
                    case 2:
                        varAmount = resource.Budget.Margin - resource.EAC.Margin;
                        varPer = resource.Budget.Margin != 0 ? varAmount / resource.Budget.Margin : -1;
                        break;
                    case 3:
                        varAmount = resource.Budget.Margin - resource.Actuals.Margin;
                        varPer = resource.Budget.Margin != 0 ? varAmount / resource.Budget.Margin : -1;
                        break;
                    case 4:
                        varAmount = resource.Projected.Margin - resource.EAC.Margin;
                        varPer = resource.Projected.Margin != 0 ? varAmount / resource.Projected.Margin : -1;
                        break;
                    case 5:
                        varAmount = resource.Projected.Margin - resource.Actuals.Margin;
                        varPer = resource.Projected.Margin != 0 ? varAmount / resource.Projected.Margin : -1;
                        break;
                    case 6:
                        varAmount = resource.EAC.Margin - resource.Actuals.Margin;
                        varPer = resource.EAC.Margin != 0 ? varAmount / resource.EAC.Margin : -1;
                        break;

                }
                reportDataInPdfString += (GreaterSeniorityExists ? HideText : varAmount.Value.ToString(CurrencyLargeDisplayFormat)) + ColoumSpliter;
                reportDataInPdfString += varPer.ToString("P");
            }
            else if (showRevenue)
            {
                if (showBudget)
                {
                    reportDataInPdfString += resource.Budget.Revenue.Value.ToString(CurrencyLargeDisplayFormat) + ColoumSpliter;
                }

                if (showProjected)
                {
                    reportDataInPdfString += resource.Projected.Revenue.Value.ToString(CurrencyLargeDisplayFormat) + ColoumSpliter;
                }
                if (showEAC)
                {
                    reportDataInPdfString += resource.EAC.Revenue.Value.ToString(CurrencyLargeDisplayFormat) + ColoumSpliter;
                }
                if (showActual)
                {
                    reportDataInPdfString += resource.Actuals.Revenue.Value.ToString(CurrencyLargeDisplayFormat) + ColoumSpliter;
                }
                switch (VarianceType)
                {
                    case 1:
                        varAmount = resource.Budget.Revenue - resource.Projected.Revenue;
                        varPer = resource.Budget.Revenue != 0 ? varAmount / resource.Budget.Revenue : -1;
                        break;
                    case 2:
                        varAmount = resource.Budget.Revenue - resource.EAC.Revenue;
                        varPer = resource.Budget.Revenue != 0 ? varAmount / resource.Budget.Revenue : -1;
                        break;
                    case 3:
                        varAmount = resource.Budget.Revenue - resource.Actuals.Revenue;
                        varPer = resource.Budget.Revenue != 0 ? varAmount / resource.Budget.Revenue : -1;
                        break;
                    case 4:
                        varAmount = resource.Projected.Revenue - resource.EAC.Revenue;
                        varPer = resource.Projected.Revenue != 0 ? varAmount / resource.Projected.Revenue : -1;
                        break;
                    case 5:
                        varAmount = resource.Projected.Revenue - resource.Actuals.Revenue;
                        varPer = resource.Projected.Revenue != 0 ? varAmount / resource.Projected.Revenue : -1;
                        break;
                    case 6:
                        varAmount = resource.EAC.Revenue - resource.Actuals.Revenue;
                        varPer = resource.EAC.Revenue != 0 ? varAmount / resource.EAC.Revenue : -1;
                        break;

                }
                reportDataInPdfString += varAmount.Value.ToString(CurrencyLargeDisplayFormat) + ColoumSpliter;
                reportDataInPdfString += varPer.ToString("P");
            }

            reportDataInPdfString += RowSpliter;

            return reportDataInPdfString;
        }

        protected void btnPdf_Click(object sender, EventArgs e)
        {
            DataHelper.InsertExportActivityLogMessage("Burn Report - PDF");
            showMargin = rbnMargin.Checked;
            showHours = rbnHours.Checked;
            showRevenue = rbnRevenue.Checked;
            showActual = chbGridActuals.Checked;
            showProjected = chbGridProjected.Checked;
            showEAC = chbGridEAC.Checked;
            showBudget = chbGridBudget.Checked;

            if (showBudget && showProjected)
            {
                VarianceType = 1;
            }
            if (showBudget && showEAC)
            {
                VarianceType = 2;
            }
            if (showBudget && showActual)
            {
                VarianceType = 3;
            }
            if (showProjected && showEAC)
            {
                VarianceType = 4;
            }
            if (showProjected && showActual)
            {
                VarianceType = 5;
            }
            if (showEAC && showActual)
            {
                VarianceType = 6;
            }


            PDFExport();
        }

        protected void btnExcelSubmit_Click(object sender, EventArgs e)
        {
            ExportToExcel(true);
        }

        protected void btnLimitedExport_Click(object sender, EventArgs e)
        {
            mpeExcel.Show();
        }

        protected void txtProjectNumber_TextChanged(object sender, EventArgs e)
        {
            if (ValidateProject() && ddlPeriod.SelectedValue == "1")
            {
                var noOfDays = (SelectedProject.EndDate.Value - SelectedProject.StartDate.Value).TotalDays;
                selectedProjectLength.Value = noOfDays.ToString();
            }
        }
    }
}

