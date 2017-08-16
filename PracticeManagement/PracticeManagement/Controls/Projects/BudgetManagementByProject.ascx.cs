using DataTransferObjects;
using PraticeManagement.Utils;
using PraticeManagement.Utils.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PraticeManagement.Controls.Projects
{
    public partial class BudgetManagementByProject : System.Web.UI.UserControl
    {
        private const int coloumnsCount = 6;

        private const string CurrencyDisplayFormat = "$###,###,###,###,###,##0";

        #region Properties
        private PraticeManagement.ProjectDetail HostingPage
        {
            get { return ((PraticeManagement.ProjectDetail)Page); }
        }
        protected int? ProjectId
        {
            get
            {
                try
                {
                    return HostingPage.ProjectId;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        protected int View
        {
            get
            {
                return int.Parse(ddlView.SelectedValue);
            }
            set
            {
                ddlView.SelectedValue = value.ToString();
            }
        }

        protected string DataPoints
        {
            get
            {
                return ddldataPoints.SelectedItems;
            }
            set
            {
                ddldataPoints.SelectedItems = value;
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

                if (View != 5)
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
                else
                {
                    return now;
                }
            }
        }

        protected ProjectBudgetManagement BudgetManagement
        {
            get
            {
                if (ViewState["Project_Budget_Management"] == null)
                {
                    ViewState["Project_Budget_Management"] = ServiceCallers.Custom.Project(p => p.GetBudgetManagementDataForProject(ProjectId.Value, View == 4, ActualsEndDate));
                }
                return ViewState["Project_Budget_Management"] as ProjectBudgetManagement;
            }

        }

        private bool ShowRevenue
        {
            get
            {
                return DataPoints == null || DataPoints.Contains("1");
            }
        }

        private bool ShowMargin
        {
            get
            {
                return DataPoints == null || DataPoints.Contains("2");
            }
        }

        private bool ShowEAC
        {
            get
            {
                return View == 2 || View == 3;
            }
        }

        private bool ShowActuals
        {
            get
            {
                return View == 4 || View == 3;
            }
        }

        private bool ShowProjectedRemaining
        {
            get
            {
                return View == 5 || View == 3;
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
                dataCellStyle.WrapText = true;
                dataCellStyle.IsBold = true;
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
                dataPercentCellStyle.DataFormat = "#,##0.00_);[Red](#,##0.00)";

                CellStyles dataNumberDateCellStyle = new CellStyles();
                dataNumberDateCellStyle.DataFormat = "$#,##0_);[Red]($#,##0)";

                var dataCellStylearray = new List<CellStyles>() { dataCellStyle };

                PreapreStyleSheet(dataCellStylearray);
                if (ShowActuals)
                {
                    PreapreStyleSheet(dataCellStylearray);
                }
                if (ShowProjectedRemaining)
                {
                    PreapreStyleSheet(dataCellStylearray);
                }
                if (ShowEAC)
                {
                    PreapreStyleSheet(dataCellStylearray);
                }

                if (View != 1)
                {
                    dataCellStylearray.Add(dataHoursCellStyle);
                    if (ShowRevenue)
                    {
                        dataCellStylearray.Add(dataNumberDateCellStyle);
                    }
                    if (ShowMargin)
                    {
                        dataCellStylearray.Add(dataNumberDateCellStyle);
                    }
                }
                RowStyles datarowStyle = new RowStyles(dataCellStylearray.ToArray());
                RowStyles[] rowStylearray = { headerrowStyle, headerrowStyle, datarowStyle };
                SheetStyles sheetStyle = new SheetStyles(rowStylearray);


                sheetStyle.MergeRegion.Add(new int[] { 3, 4, 0, 0 });

                if (View == 2 || View == 4 || View == 5)
                {
                    if (ShowRevenue && !ShowMargin)
                    {
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 1, 3 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 4, 6 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 7, 8 });
                    }
                    if (!ShowRevenue && ShowRevenue)
                    {
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 1, 4 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 5, 8 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 9, 10 });
                    }
                    if (ShowRevenue && ShowMargin)
                    {
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 1, 6 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 7, 12 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 13, 15 });
                    }
                }
                else if (View == 1)
                {
                    if (ShowRevenue && !ShowMargin)
                    {
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 1, 3 });
                    }
                    if (!ShowRevenue && ShowRevenue)
                    {
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 1, 4 });
                    }
                    if (ShowRevenue && ShowMargin)
                    {
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 1, 6 });
                    }
                }
                else if (View == 3)
                {
                    if (ShowRevenue && !ShowMargin)
                    {
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 1, 3 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 4, 6 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 7, 9 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 10, 12 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 13, 14 });
                    }
                    if (!ShowRevenue && ShowRevenue)
                    {
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 1, 4 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 5, 8 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 9, 12 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 13, 16 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 17, 18 });
                    }
                    if (ShowRevenue && ShowMargin)
                    {
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 1, 6 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 7, 12 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 13, 18 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 19, 24 });
                        sheetStyle.MergeRegion.Add(new int[] { 3, 3, 25, 27 });
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
            dataPercentCellStyle.DataFormat = "#,##0.00_);[Red](#,##0.00)";

            CellStyles dataNumberDateCellStyle = new CellStyles();
            dataNumberDateCellStyle.DataFormat = "$#,##0_);[Red]($#,##0)";

            if (ShowRevenue)
            {
                dataCellArray.Add(dataNumberDateCellStyle);
            }
            if (ShowMargin)
            {
                dataCellArray.Add(dataNumberDateCellStyle);
            }
            dataCellArray.Add(dataHoursCellStyle);
            if (ShowRevenue)
            {
                dataCellArray.Add(dataNumberDateCellStyle);
            }
            if (ShowMargin)
            {
                dataCellArray.Add(dataNumberDateCellStyle);
                dataCellArray.Add(dataPercentCellStyle);
            }
        }

        #endregion Properties

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ListItem all = new ListItem("All Data Points", "");
                ListItem rev = new ListItem("Revenue", "1");
                ListItem mar = new ListItem("Contribution Margin", "2");

                ddldataPoints.Items.Add(all);
                ddldataPoints.Items.Add(rev);
                ddldataPoints.Items.Add(mar);
                ddldataPoints.SelectedValue = "1";
            }
        }

        private void PopulateData()
        {
            if (!ProjectId.HasValue) return;
            var a = DataPoints;
            if (BudgetManagement.BudgetResources != null && BudgetManagement.BudgetResources.Count > 0 && View != 0)
            {
                repResources.DataSource = BudgetManagement.BudgetResources;
                repResources.DataBind();
            }
        }

        protected void repResources_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Header)
            {
                var thBudget = e.Item.FindControl("thBudget") as HtmlTableCell;
                var thActuals = e.Item.FindControl("thActuals") as HtmlTableCell;
                var thProjectedRemaing = e.Item.FindControl("thProjectedRemaing") as HtmlTableCell;
                var thEAC = e.Item.FindControl("thEAC") as HtmlTableCell;

                var thBudgetMarginRate = e.Item.FindControl("thBudgetMarginRate") as HtmlTableCell;
                var thBudgethMargin = e.Item.FindControl("thBudgethMargin") as HtmlTableCell;
                var thBudgethMarginPer = e.Item.FindControl("thBudgethMarginPer") as HtmlTableCell;
                var thBudgetRate = e.Item.FindControl("thBudgetRate") as HtmlTableCell;
                var thBudgetHours = e.Item.FindControl("thBudgetHours") as HtmlTableCell;
                var thBudgetRevenue = e.Item.FindControl("thBudgetRevenue") as HtmlTableCell;

                var thActualRate = e.Item.FindControl("thActualRate") as HtmlTableCell;
                var thActualHours = e.Item.FindControl("thActualHours") as HtmlTableCell;
                var thActualTotal = e.Item.FindControl("thActualTotal") as HtmlTableCell;
                var thActMarginRate = e.Item.FindControl("thActMarginRate") as HtmlTableCell;
                var thActualMargin = e.Item.FindControl("thActualMargin") as HtmlTableCell;
                var thActMarginPer = e.Item.FindControl("thActMarginPer") as HtmlTableCell;

                var thProjRate = e.Item.FindControl("thProjRate") as HtmlTableCell;
                var thProjHours = e.Item.FindControl("thProjHours") as HtmlTableCell;
                var thProjTotal = e.Item.FindControl("thProjTotal") as HtmlTableCell;
                var thProjMarginRate = e.Item.FindControl("thProjMarginRate") as HtmlTableCell;
                var thProjMargin = e.Item.FindControl("thProjMargin") as HtmlTableCell;
                var thProjMarginPer = e.Item.FindControl("thProjMarginPer") as HtmlTableCell;

                var thEACRate = e.Item.FindControl("thEACRate") as HtmlTableCell;
                var thEACHours = e.Item.FindControl("thEACHours") as HtmlTableCell;
                var thEACTotal = e.Item.FindControl("thEACTotal") as HtmlTableCell;
                var thEACMarginRate = e.Item.FindControl("thEACMarginRate") as HtmlTableCell;
                var thEACMargin = e.Item.FindControl("thEACMargin") as HtmlTableCell;
                var thEACMarginPer = e.Item.FindControl("thEACMarginPer") as HtmlTableCell;

                var thDiff = e.Item.FindControl("thDiff") as HtmlTableCell;
                var thDiffHours = e.Item.FindControl("thDiffHours") as HtmlTableCell;
                var thDiffTotals = e.Item.FindControl("thDiffTotals") as HtmlTableCell;
                var thDiffMargin = e.Item.FindControl("thDiffMargin") as HtmlTableCell;

                if (ShowRevenue && !ShowMargin)
                {
                    thBudget.ColSpan = thActuals.ColSpan = thProjectedRemaing.ColSpan = thEAC.ColSpan = 3;
                    thDiff.ColSpan = 2;
                }
                else if (ShowMargin && !ShowRevenue)
                {
                    thBudget.ColSpan = thActuals.ColSpan = thProjectedRemaing.ColSpan = thEAC.ColSpan = 4;
                    thDiff.ColSpan = 2;
                }
                else if (ShowRevenue && ShowMargin)
                {
                    thBudget.ColSpan = thActuals.ColSpan = thProjectedRemaing.ColSpan = thEAC.ColSpan = 6;
                    thDiff.ColSpan = 3;
                }

                thDiff.Visible = thDiffHours.Visible = View != 1;
                thDiffMargin.Visible = View != 1 && ShowMargin;
                thDiffTotals.Visible = View != 1 && ShowRevenue;

                thBudgetRate.Visible = thBudgetRevenue.Visible = ShowRevenue;
                thBudgetMarginRate.Visible = thBudgethMargin.Visible = thBudgethMarginPer.Visible = ShowMargin;
                if (ShowMargin && !ShowRevenue)
                {
                    thBudgetMarginRate.Attributes.Add("class", "ie-bg Width100Px borderLeftGrey");
                    thActMarginRate.Attributes.Add("class", "ie-bg Width100Px borderLeftGrey");
                    thProjMarginRate.Attributes.Add("class", "ie-bg Width100Px borderLeftGrey");
                    thEACMarginRate.Attributes.Add("class", "ie-bg Width100Px borderLeftGrey");
                }

                thActuals.Visible = thActualHours.Visible = ShowActuals;
                thActualRate.Visible = thActualTotal.Visible = ShowActuals && ShowRevenue;
                thActMarginRate.Visible = thActualMargin.Visible = thActMarginPer.Visible = ShowActuals && ShowMargin;

                thProjectedRemaing.Visible = thProjHours.Visible = ShowProjectedRemaining;
                thProjRate.Visible = thProjTotal.Visible = ShowProjectedRemaining && ShowRevenue;
                thProjMarginRate.Visible = thProjMargin.Visible = thProjMarginPer.Visible = ShowProjectedRemaining && ShowMargin;

                thEAC.Visible = thEACHours.Visible = ShowEAC;
                thEACRate.Visible = thEACTotal.Visible = ShowEAC && ShowRevenue;
                thEACMarginRate.Visible = thEACMargin.Visible = thEACMarginPer.Visible = ShowEAC && ShowMargin;


            }
            else if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                PracticeManagementCurrency _revenue = new PracticeManagementCurrency();
                _revenue.FormatStyle = NumberFormatStyle.Revenue;


                PracticeManagementCurrency _margin = new PracticeManagementCurrency();
                _margin.FormatStyle = NumberFormatStyle.Margin;
                _margin.DoNotShowDecimals = true;

                var resource = e.Item.DataItem as ProjectBudgetResource;

                var tdBudgetRate = e.Item.FindControl("tdBudgetRate") as HtmlTableCell;
                var tdBudgetMarginRate = e.Item.FindControl("tdBudgetMarginRate") as HtmlTableCell;
                var tdBudgetHours = e.Item.FindControl("tdBudgetHours") as HtmlTableCell;
                var tdBudgetRevenue = e.Item.FindControl("tdBudgetRevenue") as HtmlTableCell;
                var tdBudgetMargin = e.Item.FindControl("tdBudgetMargin") as HtmlTableCell;
                var tdBudgetMarginPer = e.Item.FindControl("tdBudgetMarginPer") as HtmlTableCell;


                var tdActRate = e.Item.FindControl("tdActRate") as HtmlTableCell;
                var tdActHours = e.Item.FindControl("tdActHours") as HtmlTableCell;
                var tdActTotal = e.Item.FindControl("tdActTotal") as HtmlTableCell;
                var tdActMarginRate = e.Item.FindControl("tdActMarginRate") as HtmlTableCell;
                var tdActMargin = e.Item.FindControl("tdActMargin") as HtmlTableCell;
                var tdActMarginPer = e.Item.FindControl("tdActMarginPer") as HtmlTableCell;

                var tdProjMarginRate = e.Item.FindControl("tdProjMarginRate") as HtmlTableCell;
                var tdProjMargin = e.Item.FindControl("tdProjMargin") as HtmlTableCell;
                var tdProjMarginPer = e.Item.FindControl("tdProjMarginPer") as HtmlTableCell;
                var tdProjRate = e.Item.FindControl("tdProjRate") as HtmlTableCell;
                var tdProjHours = e.Item.FindControl("tdProjHours") as HtmlTableCell;
                var tdProjTotal = e.Item.FindControl("tdProjTotal") as HtmlTableCell;

                var tdEACRate = e.Item.FindControl("tdEACRate") as HtmlTableCell;
                var tdEACHours = e.Item.FindControl("tdEACHours") as HtmlTableCell;
                var tdEACTotal = e.Item.FindControl("tdEACTotal") as HtmlTableCell;
                var tdEACMarginRate = e.Item.FindControl("tdEACMarginRate") as HtmlTableCell;
                var tdEACMargin = e.Item.FindControl("tdEACMargin") as HtmlTableCell;
                var tdEACMarginPer = e.Item.FindControl("tdEACMarginPer") as HtmlTableCell;

                var tdDiffHours = e.Item.FindControl("tdDiffHours") as HtmlTableCell;
                var tdDiffTotal = e.Item.FindControl("tdDiffTotal") as HtmlTableCell;
                var tdDiffMargin = e.Item.FindControl("tdDiffMargin") as HtmlTableCell;

                var lblHoursDifference = e.Item.FindControl("lblHoursDifference") as Label;
                var lblRevenueDifference = e.Item.FindControl("lblRevenueDifference") as Label;
                var lblMarginDiff = e.Item.FindControl("lblMarginDiff") as Label;

                tdDiffHours.Visible = View != 1;
                tdDiffMargin.Visible = View != 1 && ShowMargin;
                tdDiffTotal.Visible = View != 1 && ShowRevenue;

                if (ShowMargin && !ShowRevenue)
                {
                    tdBudgetMarginRate.Attributes.Add("class", "borderLeftGrey");
                    tdActMarginRate.Attributes.Add("class", "borderLeftGrey");
                    tdProjMarginRate.Attributes.Add("class", "borderLeftGrey");
                    tdEACMarginRate.Attributes.Add("class", "borderLeftGrey");
                }

                tdBudgetRate.Visible = tdBudgetRevenue.Visible = ShowRevenue;
                tdBudgetMarginRate.Visible = tdBudgetMargin.Visible = tdBudgetMarginPer.Visible = ShowMargin;

                tdActHours.Visible = ShowActuals;
                tdActRate.Visible = tdActTotal.Visible = ShowActuals && ShowRevenue;
                tdActMarginRate.Visible = tdActMargin.Visible = tdActMarginPer.Visible = ShowActuals && ShowMargin;

                tdProjHours.Visible = ShowProjectedRemaining;
                tdProjRate.Visible = tdProjTotal.Visible = ShowProjectedRemaining && ShowRevenue;
                tdProjMarginRate.Visible = tdProjMargin.Visible = tdProjMarginPer.Visible = ShowProjectedRemaining && ShowMargin;

                tdEACHours.Visible = ShowEAC;
                tdEACRate.Visible = tdEACTotal.Visible = ShowEAC && ShowRevenue;
                tdEACMarginRate.Visible = tdEACMargin.Visible = tdEACMarginPer.Visible = ShowEAC && ShowMargin;

                var lblBudgetMarginPer = e.Item.FindControl("lblBudgetMarginPer") as Label;
                var lblActMarginPer = e.Item.FindControl("lblActMarginPer") as Label;
                var lblProjMarginPer = e.Item.FindControl("lblProjMarginPer") as Label;
                var lblEACMarginPer = e.Item.FindControl("lblEACMarginPer") as Label;


                lblBudgetMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat, resource.Budget.Revenue != 0 ? resource.Budget.Margin.Value * 100M / resource.Budget.Revenue.Value : 0M);
                lblActMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat, resource.Actuals.Revenue != 0 ? resource.Actuals.Margin.Value * 100M / resource.Actuals.Revenue.Value : 0M);
                lblProjMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat, resource.ProjectedRemaining.Revenue != 0 ? resource.ProjectedRemaining.Margin.Value * 100M / resource.ProjectedRemaining.Revenue.Value : 0M);
                lblEACMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat, resource.EAC.Revenue != 0 ? resource.EAC.Margin.Value * 100M / resource.EAC.Revenue.Value : 0M);

                decimal _hoursDiff = 0;

                switch (View)
                {
                    case 1: break;
                    case 2:
                        _revenue = resource.EAC.Revenue - resource.Budget.Revenue;
                        _margin = resource.EAC.Margin - resource.Budget.Margin;
                        _hoursDiff = resource.EAC.Hours - resource.Budget.Hours;
                        break;
                    case 3:
                        _revenue = resource.EAC.Revenue - resource.Budget.Revenue;
                        _margin = resource.EAC.Margin - resource.Budget.Margin;
                        _hoursDiff = resource.EAC.Hours - resource.Budget.Hours;
                        break;
                    case 4:
                        _revenue = resource.Actuals.Revenue - resource.Budget.Revenue;
                        _margin = resource.Actuals.Margin - resource.Budget.Margin;
                        _hoursDiff = resource.Actuals.Hours - resource.Budget.Hours;
                        break;
                    case 5:
                        _revenue = resource.ProjectedRemaining.Revenue - resource.Budget.Revenue;
                        _margin = resource.ProjectedRemaining.Margin - resource.Budget.Margin;
                        _hoursDiff = resource.ProjectedRemaining.Hours - resource.Budget.Hours;
                        break;
                }
                _revenue.DoNotShowDecimals = _margin.DoNotShowDecimals = true;
                lblRevenueDifference.Text = _revenue.ToString();
                lblMarginDiff.Text = _margin.ToString();
                lblHoursDifference.Text = _hoursDiff < 0 ? string.Format("<span class=\"Bench\">({0})</span>", Math.Abs(_hoursDiff)) : _hoursDiff.ToString();
            }
            else if (e.Item.ItemType == ListItemType.Footer)
            {
                var tdBudgetSummaryRate = e.Item.FindControl("tdBudgetSummaryRate") as HtmlTableCell;
                var tdTotalBudgetHours = e.Item.FindControl("tdTotalBudgetHours") as HtmlTableCell;
                var tdTotalBudgetRevenue = e.Item.FindControl("tdTotalBudgetRevenue") as HtmlTableCell;
                var tdTotalBudgetMargin = e.Item.FindControl("tdTotalBudgetMargin") as HtmlTableCell;
                var tdTotalBudgetMarginPer = e.Item.FindControl("tdTotalBudgetMarginPer") as HtmlTableCell;

                var tdActSummaryRate = e.Item.FindControl("tdActSummaryRate") as HtmlTableCell;
                var tdActTotalHours = e.Item.FindControl("tdActTotalHours") as HtmlTableCell;
                var tdActTotalRevenue = e.Item.FindControl("tdActTotalRevenue") as HtmlTableCell;
                var tdTotalActMargin = e.Item.FindControl("tdTotalActMargin") as HtmlTableCell;
                var tdTotalActMarginPer = e.Item.FindControl("tdTotalActMarginPer") as HtmlTableCell;

                var tdProjSummaryRate = e.Item.FindControl("tdProjSummaryRate") as HtmlTableCell;
                var tdProjTotalHours = e.Item.FindControl("tdProjTotalHours") as HtmlTableCell;
                var tdProjTotalRevenue = e.Item.FindControl("tdProjTotalRevenue") as HtmlTableCell;
                var tdTotalProjMargin = e.Item.FindControl("tdTotalProjMargin") as HtmlTableCell;
                var tdTotalProjMarginPer = e.Item.FindControl("tdTotalProjMarginPer") as HtmlTableCell;

                var tdEACSummaryRate = e.Item.FindControl("tdEACSummaryRate") as HtmlTableCell;
                var tdEACTotalHours = e.Item.FindControl("tdEACTotalHours") as HtmlTableCell;
                var tdEACTotalRevenue = e.Item.FindControl("tdEACTotalRevenue") as HtmlTableCell;
                var tdEACTotalMargin = e.Item.FindControl("tdEACTotalMargin") as HtmlTableCell;
                var tdEACTotalMarginPer = e.Item.FindControl("tdEACTotalMarginPer") as HtmlTableCell;

                var tdBudgetEmptyExpense = e.Item.FindControl("tdBudgetEmptyExpense") as HtmlTableCell;
                var tdBudgetRevenueExpense = e.Item.FindControl("tdBudgetRevenueExpense") as HtmlTableCell;
                var tdBudgetMarginExpense = e.Item.FindControl("tdBudgetMarginExpense") as HtmlTableCell;
                var tdBudgetMarinExpense = e.Item.FindControl("tdBudgetMarinExpense") as HtmlTableCell;

                var tdActEmpty = e.Item.FindControl("tdActEmpty") as HtmlTableCell;
                var tdActExpense = e.Item.FindControl("tdActExpense") as HtmlTableCell;
                var tdActMarginExpense = e.Item.FindControl("tdActMarginExpense") as HtmlTableCell;
                var tdActEmptyMargin = e.Item.FindControl("tdActEmptyMargin") as HtmlTableCell;

                var tdProjEmpty = e.Item.FindControl("tdProjEmpty") as HtmlTableCell;
                var tdProjExpense = e.Item.FindControl("tdProjExpense") as HtmlTableCell;
                var tdProjMarginExpense = e.Item.FindControl("tdProjMarginExpense") as HtmlTableCell;
                var tdProjEmptyMargin = e.Item.FindControl("tdProjEmptyMargin") as HtmlTableCell;

                var tdEACEmpty = e.Item.FindControl("tdEACEmpty") as HtmlTableCell;
                var tdEACExpense = e.Item.FindControl("tdEACExpense") as HtmlTableCell;
                var tdEACMarginExpense = e.Item.FindControl("tdEACMarginExpense") as HtmlTableCell;
                var tdEACEmptyMargin = e.Item.FindControl("tdEACEmptyMargin") as HtmlTableCell;

                var tdDiffEmpty = e.Item.FindControl("tdDiffEmpty") as HtmlTableCell;
                var tdDiffExpense = e.Item.FindControl("tdDiffExpense") as HtmlTableCell;
                var tdDiffMarginExpense = e.Item.FindControl("tdDiffMarginExpense") as HtmlTableCell;

                var tdEmptyBudgetTotal = e.Item.FindControl("tdEmptyBudgetTotal") as HtmlTableCell;
                var tdBudgetTotalRevenue = e.Item.FindControl("tdBudgetTotalRevenue") as HtmlTableCell;
                var tdBudgetTotalMargin = e.Item.FindControl("tdBudgetTotalMargin") as HtmlTableCell;
                var tdBudgetMarginPer = e.Item.FindControl("tdBudgetMarginPer") as HtmlTableCell;

                var tdActEmp = e.Item.FindControl("tdActEmp") as HtmlTableCell;
                var tdActTotal = e.Item.FindControl("tdActTotal") as HtmlTableCell;
                var tdActMargin = e.Item.FindControl("tdActMargin") as HtmlTableCell;
                var tdActMarginPer = e.Item.FindControl("tdActMarginPer") as HtmlTableCell;

                var tdProjEmp = e.Item.FindControl("tdProjEmp") as HtmlTableCell;
                var tdProjTotal = e.Item.FindControl("tdProjTotal") as HtmlTableCell;
                var tdProjMargin = e.Item.FindControl("tdProjMargin") as HtmlTableCell;
                var tdProjMarginPer = e.Item.FindControl("tdProjMarginPer") as HtmlTableCell;

                var tdEACEmp = e.Item.FindControl("tdEACEmp") as HtmlTableCell;
                var tdEACTotal = e.Item.FindControl("tdEACTotal") as HtmlTableCell;
                var tdEACMargin = e.Item.FindControl("tdEACMargin") as HtmlTableCell;
                var tdEACMarginPer = e.Item.FindControl("tdEACMarginPer") as HtmlTableCell;

                var tdDiffSummaryHours = e.Item.FindControl("tdDiffSummaryHours") as HtmlTableCell;
                var tdDiffSummaryTotal = e.Item.FindControl("tdDiffSummaryTotal") as HtmlTableCell;
                var tdDiffSummaryMargin = e.Item.FindControl("tdDiffSummaryMargin") as HtmlTableCell;
                var tdDiffEmp = e.Item.FindControl("tdDiffEmp") as HtmlTableCell;
                var tdDiffTotalRevenue = e.Item.FindControl("tdDiffTotalRevenue") as HtmlTableCell;
                var tdDiffMargin = e.Item.FindControl("tdDiffMargin") as HtmlTableCell;

                tdDiffSummaryHours.Visible = tdDiffEmpty.Visible = tdDiffEmp.Visible = View != 1;
                tdDiffSummaryMargin.Visible = tdDiffMarginExpense.Visible = tdDiffMargin.Visible = View != 1 && ShowMargin;
                tdDiffSummaryTotal.Visible = tdDiffExpense.Visible = tdDiffTotalRevenue.Visible = View != 1 && ShowRevenue;



                if ((ShowRevenue && !ShowMargin) || (ShowMargin && !ShowRevenue))
                {
                    tdBudgetSummaryRate.ColSpan = tdActSummaryRate.ColSpan = tdProjSummaryRate.ColSpan = tdEACSummaryRate.ColSpan = 1;
                    tdBudgetEmptyExpense.ColSpan = tdActEmpty.ColSpan = tdProjEmpty.ColSpan = tdEACEmpty.ColSpan = tdEmptyBudgetTotal.ColSpan =
                      tdActEmp.ColSpan = tdProjEmp.ColSpan = tdEACEmp.ColSpan = 2;
                }
                else if (ShowRevenue && ShowMargin)
                {
                    tdBudgetSummaryRate.ColSpan = tdActSummaryRate.ColSpan = tdProjSummaryRate.ColSpan = tdEACSummaryRate.ColSpan = 2;
                    tdBudgetEmptyExpense.ColSpan = tdActEmpty.ColSpan = tdProjEmpty.ColSpan = tdEACEmpty.ColSpan = tdEmptyBudgetTotal.ColSpan =
                     tdActEmp.ColSpan = tdProjEmp.ColSpan = tdEACEmp.ColSpan = 3;
                }

                tdTotalBudgetRevenue.Visible = tdBudgetTotalRevenue.Visible = tdBudgetRevenueExpense.Visible = ShowRevenue;
                tdTotalBudgetMargin.Visible = tdTotalBudgetMarginPer.Visible = tdBudgetTotalMargin.Visible = tdBudgetMarginPer.Visible = tdBudgetMarginExpense.Visible = tdBudgetMarinExpense.Visible = ShowMargin;

                tdActSummaryRate.Visible = tdActTotalHours.Visible = tdActEmpty.Visible = tdActEmp.Visible = ShowActuals;
                tdActTotalRevenue.Visible = tdActExpense.Visible = tdActTotal.Visible = ShowActuals && ShowRevenue;
                tdTotalActMargin.Visible = tdTotalActMarginPer.Visible = tdActMarginExpense.Visible = tdActEmptyMargin.Visible = tdActMargin.Visible = tdActMarginPer.Visible = ShowActuals && ShowMargin;

                tdProjSummaryRate.Visible = tdProjTotalHours.Visible = tdProjEmpty.Visible = tdProjEmp.Visible = ShowProjectedRemaining;
                tdProjTotalRevenue.Visible = tdProjExpense.Visible = tdProjTotal.Visible = ShowProjectedRemaining && ShowRevenue;
                tdTotalProjMargin.Visible = tdTotalProjMarginPer.Visible = tdProjMarginExpense.Visible = tdProjEmptyMargin.Visible = tdProjMargin.Visible = tdProjMarginPer.Visible = ShowProjectedRemaining && ShowMargin;

                tdEACSummaryRate.Visible = tdEACTotalHours.Visible = tdEACEmpty.Visible = tdEACEmp.Visible = ShowEAC;
                tdEACTotalRevenue.Visible = tdEACExpense.Visible = tdEACTotal.Visible = ShowEAC && ShowRevenue;
                tdEACTotalMargin.Visible = tdEACTotalMarginPer.Visible = tdEACMarginExpense.Visible = tdEACEmptyMargin.Visible = tdEACMargin.Visible = tdEACMarginPer.Visible = ShowEAC && ShowMargin;

                var lblTotalBudgetHours = e.Item.FindControl("lblTotalBudgetHours") as Label;
                lblTotalBudgetHours.Text = BudgetManagement.BudgetSummary.Hours.ToString();


                if (ShowRevenue)
                {
                    var lblTotalRevenue = e.Item.FindControl("lblTotalRevenue") as Label;
                    var lblBudgetRevenueExpense = e.Item.FindControl("lblBudgetRevenueExpense") as Label;
                    var lblBudgetTotal = e.Item.FindControl("lblBudgetTotal") as Label;
                    lblTotalRevenue.Text = BudgetManagement.BudgetSummary.Revenue.ToString();
                    lblBudgetRevenueExpense.Text = BudgetManagement.BudgetSummary.Expenses.ToString("$###,###,###,###,##0");
                    lblBudgetTotal.Text = BudgetManagement.BudgetSummary.TotalRevenue.ToString();

                    if (ShowActuals)
                    {
                        var lblTotalActHours = e.Item.FindControl("lblTotalActHours") as Label;
                        lblTotalActHours.Text = BudgetManagement.ActualsSummary.Hours.ToString();
                        var lblTotalActRevenue = e.Item.FindControl("lblTotalActRevenue") as Label;
                        var lblActExpense = e.Item.FindControl("lblActExpense") as Label;
                        var lblActTotal = e.Item.FindControl("lblActTotal") as Label;
                        lblTotalActRevenue.Text = BudgetManagement.ActualsSummary.Revenue.ToString();
                        lblActExpense.Text = BudgetManagement.ActualsSummary.Expenses.ToString("$###,###,###,###,##0");
                        lblActTotal.Text = BudgetManagement.ActualsSummary.TotalRevenue.ToString();
                    }
                    if (ShowProjectedRemaining)
                    {
                        var lblTotalProjHours = e.Item.FindControl("lblTotalProjHours") as Label;
                        lblTotalProjHours.Text = BudgetManagement.ProjectedSummary.Hours.ToString();
                        var lblTotalProjRevenue = e.Item.FindControl("lblTotalProjRevenue") as Label;
                        var lblProjExpense = e.Item.FindControl("lblProjExpense") as Label;
                        var lblProjTotal = e.Item.FindControl("lblProjTotal") as Label;
                        lblTotalProjRevenue.Text = BudgetManagement.ProjectedSummary.Revenue.ToString();
                        lblProjExpense.Text = BudgetManagement.ProjectedSummary.Expenses.ToString("$###,###,###,###,##0");
                        lblProjTotal.Text = BudgetManagement.ProjectedSummary.TotalRevenue.ToString();
                    }
                    if (ShowEAC)
                    {
                        var lblTotalEACHours = e.Item.FindControl("lblTotalEACHours") as Label;
                        lblTotalEACHours.Text = BudgetManagement.EACSummary.Hours.ToString();
                        var lblTotalEACRevenue = e.Item.FindControl("lblTotalEACRevenue") as Label;
                        var lblEACExpense = e.Item.FindControl("lblEACExpense") as Label;
                        var lblEACTotal = e.Item.FindControl("lblEACTotal") as Label;
                        lblTotalEACRevenue.Text = BudgetManagement.EACSummary.Revenue.ToString();
                        lblEACExpense.Text = BudgetManagement.EACSummary.Expenses.ToString("$###,###,###,###,##0");
                        lblEACTotal.Text = BudgetManagement.EACSummary.TotalRevenue.ToString();
                    }

                }
                if (ShowMargin)
                {
                    var lblBudgetMargin = e.Item.FindControl("lblBudgetMargin") as Label;
                    var lblBudgetMarginPer = e.Item.FindControl("lblBudgetMarginPer") as Label;
                    var lblTotalBudgetMargin = e.Item.FindControl("lblTotalBudgetMargin") as Label;
                    var lblTotalBudgetMarginPer = e.Item.FindControl("lblTotalBudgetMarginPer") as Label;

                    var lblBudgetMarginExpense = e.Item.FindControl("lblBudgetMarginExpense") as Label;
                    lblBudgetMarginExpense.Text = BudgetManagement.BudgetSummary.Expenses.ToString("$###,###,###,###,##0");
                    lblBudgetMargin.Text = BudgetManagement.BudgetSummary.TotalMargin.ToString();
                    lblBudgetMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat,
                                                           BudgetManagement.BudgetSummary.TotalRevenue != 0 ? BudgetManagement.BudgetSummary.TotalMargin.Value * 100M / BudgetManagement.BudgetSummary.TotalRevenue.Value : 0M);
                    lblTotalBudgetMargin.Text = BudgetManagement.BudgetSummary.Margin.ToString();
                    lblTotalBudgetMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat,
                                                           BudgetManagement.BudgetSummary.Revenue != 0 ? BudgetManagement.BudgetSummary.Margin.Value * 100M / BudgetManagement.BudgetSummary.Revenue.Value : 0M);
                    if (ShowActuals)
                    {
                        var lblTotalActHours = e.Item.FindControl("lblTotalActHours") as Label;
                        lblTotalActHours.Text = BudgetManagement.ActualsSummary.Hours.ToString();
                        var lblActMargin = e.Item.FindControl("lblActMargin") as Label;
                        var lblActMarginPer = e.Item.FindControl("lblActMarginPer") as Label;
                        var lblTotalActMargin = e.Item.FindControl("lblTotalActMargin") as Label;
                        var lblTotalActMarginPer = e.Item.FindControl("lblTotalActMarginPer") as Label;
                        var lblActMarginExpense = e.Item.FindControl("lblActMarginExpense") as Label;
                        lblActMarginExpense.Text = BudgetManagement.ActualsSummary.Expenses.ToString("$###,###,###,###,##0");

                        lblActMargin.Text = BudgetManagement.ActualsSummary.TotalMargin.ToString();
                        lblActMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat,
                                                           BudgetManagement.ActualsSummary.TotalRevenue != 0 ? BudgetManagement.ActualsSummary.TotalMargin.Value * 100M / BudgetManagement.ActualsSummary.TotalRevenue.Value : 0M);
                        lblTotalActMargin.Text = BudgetManagement.ActualsSummary.Margin.ToString();
                        lblTotalActMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat,
                                                               BudgetManagement.ActualsSummary.Revenue != 0 ? BudgetManagement.ActualsSummary.Margin.Value * 100M / BudgetManagement.ActualsSummary.Revenue.Value : 0M);
                    }

                    if (ShowProjectedRemaining)
                    {
                        var lblTotalProjHours = e.Item.FindControl("lblTotalProjHours") as Label;
                        lblTotalProjHours.Text = BudgetManagement.ProjectedSummary.Hours.ToString();
                        var lblProjMargin = e.Item.FindControl("lblProjMargin") as Label;
                        var lblProjMarginPer = e.Item.FindControl("lblProjMarginPer") as Label;
                        var lblTotalProjMargin = e.Item.FindControl("lblTotalProjMargin") as Label;
                        var lblTotalProjMarginPer = e.Item.FindControl("lblTotalProjMarginPer") as Label;

                        var lblProjMarginExpense = e.Item.FindControl("lblProjMarginExpense") as Label;
                        lblProjMarginExpense.Text = BudgetManagement.ProjectedSummary.Expenses.ToString("$###,###,###,###,##0");

                        lblTotalProjMargin.Text = BudgetManagement.ProjectedSummary.Margin.ToString();
                        lblTotalProjMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat,
                                                               BudgetManagement.ProjectedSummary.Revenue != 0 ? BudgetManagement.ProjectedSummary.Margin.Value * 100M / BudgetManagement.ProjectedSummary.Revenue.Value : 0M);
                        lblProjMargin.Text = BudgetManagement.ProjectedSummary.TotalMargin.ToString();
                        lblProjMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat,
                                                          BudgetManagement.ProjectedSummary.TotalRevenue != 0 ? BudgetManagement.ProjectedSummary.TotalMargin.Value * 100M / BudgetManagement.ProjectedSummary.TotalRevenue.Value : 0M);
                    }
                    if (ShowEAC)
                    {
                        var lblTotalEACHours = e.Item.FindControl("lblTotalEACHours") as Label;
                        lblTotalEACHours.Text = BudgetManagement.EACSummary.Hours.ToString();
                        var lblEACMargin = e.Item.FindControl("lblEACMargin") as Label;
                        var lblEACMarginPer = e.Item.FindControl("lblEACMarginPer") as Label;

                        var lblEACMarginExpense = e.Item.FindControl("lblEACMarginExpense") as Label;
                        lblEACMarginExpense.Text = BudgetManagement.EACSummary.Expenses.ToString("$###,###,###,###,##0");

                        lblEACMargin.Text = BudgetManagement.EACSummary.TotalMargin.ToString();
                        lblEACMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat,
                                                           BudgetManagement.EACSummary.TotalRevenue != 0 ? BudgetManagement.EACSummary.TotalMargin.Value * 100M / BudgetManagement.EACSummary.TotalRevenue.Value : 0M);
                        var lblEACTotalMargin = e.Item.FindControl("lblEACTotalMargin") as Label;
                        var lblEACTotalMarginPer = e.Item.FindControl("lblEACTotalMarginPer") as Label;
                        lblEACTotalMargin.Text = BudgetManagement.EACSummary.Margin.ToString();
                        lblEACTotalMarginPer.Text = string.Format(Constants.Formatting.PercentageFormat,
                                                               BudgetManagement.EACSummary.Revenue != 0 ? BudgetManagement.EACSummary.Margin.Value * 100M / BudgetManagement.EACSummary.Revenue.Value : 0M);
                    }
                }

                if (View != 1)
                {
                    var lblHoursDifferenceSummary = e.Item.FindControl("lblHoursDifferenceSummary") as Label;
                    var lblRevenueDifferenceSummary = e.Item.FindControl("lblRevenueDifferenceSummary") as Label;
                    var lblMarginDifferenceSummary = e.Item.FindControl("lblMarginDifferenceSummary") as Label;
                    var lblExpenseDiff = e.Item.FindControl("lblExpenseDiff") as Label;
                    var lblMarginExpense = e.Item.FindControl("lblMarginExpense") as Label;
                    var lblDiffTotal = e.Item.FindControl("lblDiffTotal") as Label;
                    var lblDiffMargin = e.Item.FindControl("lblDiffMargin") as Label;

                    decimal _hrsDiff = 0;


                    switch (View)
                    {
                        case 1: break;
                        case 2:
                            _hrsDiff = BudgetManagement.EACSummary.Hours - BudgetManagement.BudgetSummary.Hours;
                            lblRevenueDifferenceSummary.Text = (BudgetManagement.EACSummary.Revenue - BudgetManagement.BudgetSummary.Revenue).ToString();
                            lblMarginDifferenceSummary.Text = (BudgetManagement.EACSummary.Margin - BudgetManagement.BudgetSummary.Margin).ToString();
                            lblMarginExpense.Text = lblExpenseDiff.Text = (BudgetManagement.EACSummary.Expenses - BudgetManagement.BudgetSummary.Expenses).ToString("$###,###,###,###,##0");
                            lblDiffTotal.Text = string.Format(PracticeManagementCurrency.RevenueFormat, (BudgetManagement.EACSummary.TotalRevenue - BudgetManagement.BudgetSummary.TotalRevenue).Value.ToString(CurrencyDisplayFormat));
                            lblDiffMargin.Text = string.Format(PracticeManagementCurrency.RevenueFormat, (BudgetManagement.EACSummary.TotalMargin - BudgetManagement.BudgetSummary.TotalMargin).Value.ToString(CurrencyDisplayFormat));
                            break;
                        case 3:
                            _hrsDiff = BudgetManagement.EACSummary.Hours - BudgetManagement.BudgetSummary.Hours;
                            lblRevenueDifferenceSummary.Text = (BudgetManagement.EACSummary.Revenue - BudgetManagement.BudgetSummary.Revenue).ToString();
                            lblMarginDifferenceSummary.Text = (BudgetManagement.EACSummary.Margin - BudgetManagement.BudgetSummary.Margin).ToString();
                            lblMarginExpense.Text = lblExpenseDiff.Text = (BudgetManagement.EACSummary.Expenses - BudgetManagement.BudgetSummary.Expenses).ToString("$###,###,###,###,##0");
                            lblDiffTotal.Text = (BudgetManagement.EACSummary.TotalRevenue - BudgetManagement.BudgetSummary.TotalRevenue).ToString();
                            lblDiffMargin.Text = (BudgetManagement.EACSummary.TotalMargin - BudgetManagement.BudgetSummary.TotalMargin).ToString();
                            break;
                        case 4:
                            _hrsDiff = BudgetManagement.ActualsSummary.Hours - BudgetManagement.BudgetSummary.Hours;
                            lblRevenueDifferenceSummary.Text = (BudgetManagement.ActualsSummary.Revenue - BudgetManagement.BudgetSummary.Revenue).ToString();
                            lblMarginDifferenceSummary.Text = (BudgetManagement.ActualsSummary.Margin - BudgetManagement.BudgetSummary.Margin).ToString();
                            lblMarginExpense.Text = lblExpenseDiff.Text = (BudgetManagement.ActualsSummary.Expenses - BudgetManagement.BudgetSummary.Expenses).ToString("$###,###,###,###,##0");
                            lblDiffTotal.Text = (BudgetManagement.ActualsSummary.TotalRevenue - BudgetManagement.BudgetSummary.TotalRevenue).ToString();
                            lblDiffMargin.Text = (BudgetManagement.ActualsSummary.TotalMargin - BudgetManagement.BudgetSummary.TotalMargin).ToString();
                            break;
                        case 5:
                            _hrsDiff = BudgetManagement.ProjectedSummary.Hours - BudgetManagement.BudgetSummary.Hours;
                            lblRevenueDifferenceSummary.Text = (BudgetManagement.ProjectedSummary.Revenue - BudgetManagement.BudgetSummary.Revenue).ToString();
                            lblMarginDifferenceSummary.Text = (BudgetManagement.ProjectedSummary.Margin - BudgetManagement.BudgetSummary.Margin).ToString();
                            lblMarginExpense.Text = lblExpenseDiff.Text = (BudgetManagement.ProjectedSummary.Expenses - BudgetManagement.BudgetSummary.Expenses).ToString("$###,###,###,###,##0");
                            lblDiffTotal.Text = (BudgetManagement.ProjectedSummary.TotalRevenue - BudgetManagement.BudgetSummary.TotalRevenue).ToString();
                            lblDiffMargin.Text = (BudgetManagement.ProjectedSummary.TotalMargin - BudgetManagement.BudgetSummary.TotalMargin).ToString();
                            break;
                    }
                    lblHoursDifferenceSummary.Text = _hrsDiff < 0 ? string.Format("<span class=\"Bench\">({0})</span>", Math.Abs(_hrsDiff)) : _hrsDiff.ToString();
                }
            }

        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            ViewState["Project_Budget_Management"] = null;
            PopulateData();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            string fileName = "Budget Management By Project -" + HostingPage.Project.ProjectNumber + ".xls";
            var sheetStylesList = new List<SheetStyles>();
            var dataSetList = new List<DataSet>();

            if (BudgetManagement.BudgetResources != null && BudgetManagement.BudgetResources.Count > 0 && View != 0)
            {
                string Title = "Budget Management By Project -" + HostingPage.Project.Name;
                string filters = "View:" + ddlView.SelectedItem.Text + ",\n Data Points:" + (ddldataPoints.SelectedItems != null ? ddldataPoints.SelectedItemsText : "All Data points");
                DataTable header = new DataTable();
                header.Columns.Add(Title);
                header.Rows.Add(filters);
                var data = PrepareDataTable(BudgetManagement);
                sheetStylesList.Add(HeaderSheetStyle);
                sheetStylesList.Add(DataSheetStyle);
                var dataset = new DataSet();
                dataset.DataSetName = "BudgetManagementByProject_" + HostingPage.Project.ProjectNumber;
                dataset.Tables.Add(header);
                dataset.Tables.Add(data);
                dataSetList.Add(dataset);
            }
            else
            {
                string Title = "There are no resources for the selected filters.";
                DataTable header = new DataTable();
                header.Columns.Add(Title);
                sheetStylesList.Add(HeaderSheetStyle);
                var dataset = new DataSet();
                dataset.DataSetName = "BudgetManagementByProject_" + HostingPage.Project.ProjectNumber;
                dataset.Tables.Add(header);
                dataSetList.Add(dataset);
            }
            NPOIExcel.Export(fileName, dataSetList, sheetStylesList);
        }

        public DataTable PrepareDataTable(ProjectBudgetManagement BudgetManagement)
        {
            DateTime now = SettingsHelper.GetCurrentPMTime();
            DataTable data = new DataTable();
            List<object> row;

            var showMargin = ShowMargin;
            var showrevenue = ShowRevenue;
            var showActual = ShowActuals;
            var showProjected = ShowProjectedRemaining;
            var showEAC = ShowEAC;

            data.Columns.Add("Title/ Role");
            if (showrevenue && !showMargin)
            {
                data.Columns.Add("Budget");
            }
            if (showMargin && !showrevenue)
            {
                data.Columns.Add("Budget");
            }
            if (showrevenue && showMargin)
            {
                data.Columns.Add("Budget");
                data.Columns.Add("");
            }
            data.Columns.Add("");
            if (showrevenue)
            {
                data.Columns.Add("");
            }
            if (showMargin)
            {
                data.Columns.Add("");
                data.Columns.Add("");
            }

            if (showActual)
            {
                if (showrevenue && !showMargin)
                {
                    data.Columns.Add("Actuals");
                }
                if (showMargin && !showrevenue)
                {
                    data.Columns.Add("Actuals");
                }
                if (showrevenue && showMargin)
                {
                    data.Columns.Add("Actuals");
                    data.Columns.Add("");
                }
                data.Columns.Add("");
                if (showrevenue)
                {
                    data.Columns.Add("");
                }
                if (showMargin)
                {
                    data.Columns.Add("");
                    data.Columns.Add("");
                }
            }

            if (showProjected)
            {
                if (showrevenue && !showMargin)
                {
                    data.Columns.Add("Projected Remaining");
                }
                if (showMargin && !showrevenue)
                {
                    data.Columns.Add("Projected Remaining");
                }
                if (showrevenue && showMargin)
                {
                    data.Columns.Add("Projected Remaining");
                    data.Columns.Add("");
                }
                data.Columns.Add("");
                if (showrevenue)
                {
                    data.Columns.Add("");
                }
                if (showMargin)
                {
                    data.Columns.Add("");
                    data.Columns.Add("");
                }
            }

            if (ShowEAC)
            {
                if (showrevenue && !showMargin)
                {
                    data.Columns.Add("ETC");
                }
                if (showMargin && !showrevenue)
                {
                    data.Columns.Add("ETC");
                }
                if (showrevenue && showMargin)
                {
                    data.Columns.Add("ETC");
                    data.Columns.Add("");
                }
                data.Columns.Add("");
                if (showrevenue)
                {
                    data.Columns.Add("");
                }
                if (showMargin)
                {
                    data.Columns.Add("");
                    data.Columns.Add("");
                }
            }

            if (View != 1)
            {
                data.Columns.Add("Difference");
                if (showrevenue)
                {
                    data.Columns.Add("");
                }
                if (showMargin)
                {
                    data.Columns.Add("");
                }
            }
            row = new List<object>();

            row.Add("Title/ Role");
            if (showrevenue)
            {
                row.Add("Final Rate/Hr.");
            }
            if (showMargin)
            {
                row.Add("Margin Rate");
            }
            row.Add("Hours");
            if (showrevenue)
            {
                row.Add("Total Revenue");
            }
            if (showMargin)
            {
                row.Add("Total Margin");
                row.Add("Margin %");
            }

            if (showActual)
            {
                if (showrevenue)
                {
                    row.Add("Actual Rate/Hr.");
                }
                if (showMargin)
                {
                    row.Add("Actual Margin Rate");
                }
                row.Add("Actual Hours");
                if (showrevenue)
                {
                    row.Add("Actual Total Revenue");
                }
                if (showMargin)
                {
                    row.Add("Actual Total Margin");
                    row.Add("Actual Margin %");
                }
            }

            if (showProjected)
            {
                if (showrevenue)
                {
                    row.Add("Projected Rate/Hr.");
                }
                if (showMargin)
                {
                    row.Add("Projected Margin Rate");
                }
                row.Add("Projected Hours");
                if (showrevenue)
                {
                    row.Add("Projected Total Revenue");
                }
                if (showMargin)
                {
                    row.Add("Projected Total Margin");
                    row.Add("Projected Margin %");
                }
            }

            if (ShowEAC)
            {
                if (showrevenue)
                {
                    row.Add("ETC Rate/Hr.");
                }
                if (showMargin)
                {
                    row.Add("ETC Margin Rate");
                }
                row.Add("ETC Hours");
                if (showrevenue)
                {
                    row.Add("ETC Total Revenue");
                }
                if (showMargin)
                {
                    row.Add("ETC Total Margin");
                    row.Add("ETC Margin %");
                }
            }

            if (View != 1)
            {
                row.Add("Hours Difference");
                if (showrevenue)
                {
                    row.Add("Total Revenue Difference");
                }
                if (showMargin)
                {
                    row.Add("Total Margin Difference");
                    //data.Columns.Add("Margin %");
                }
            }
            data.Rows.Add(row.ToArray());
            foreach (var resource in BudgetManagement.BudgetResources)
            {
                row = new List<object>();
                row.Add(resource.Person.LastName + ", " + resource.Person.FirstName + "(" + resource.Person.Title.TitleName + ")");
                //budget
                PrepareExcelGrid(row, resource.Budget, showrevenue, showMargin);
                if (showActual)
                {
                    PrepareExcelGrid(row, resource.Actuals, showrevenue, showMargin);

                }
                if (showProjected)
                {
                    PrepareExcelGrid(row, resource.ProjectedRemaining, showrevenue, showMargin);

                }
                if (showEAC)
                {
                    PrepareExcelGrid(row, resource.EAC, showrevenue, showMargin);

                }
                switch (View)
                {
                    case 1: break;
                    case 2:

                        row.Add(resource.Budget.Hours - resource.EAC.Hours);
                        if (showrevenue)
                        {
                            row.Add(resource.Budget.Revenue.Value - resource.EAC.Revenue.Value);
                        }
                        if (showMargin)
                        {
                            row.Add(resource.Budget.Margin.Value - resource.EAC.Margin.Value);
                        }
                        break;
                    case 3:
                        row.Add(resource.Budget.Hours - resource.EAC.Hours);
                        if (showrevenue)
                        {
                            row.Add(resource.Budget.Revenue.Value - resource.EAC.Revenue.Value);
                        }
                        if (showMargin)
                        {
                            row.Add(resource.Budget.Margin.Value - resource.EAC.Margin.Value);
                        }

                        break;
                    case 4:
                        row.Add(resource.Budget.Hours - resource.Actuals.Hours);
                        if (showrevenue)
                        {
                            row.Add(resource.Budget.Revenue.Value - resource.Actuals.Revenue.Value);
                        }
                        if (showMargin)
                        {
                            row.Add(resource.Budget.Margin.Value - resource.Actuals.Margin.Value);
                        }

                        break;
                    case 5:
                        row.Add(resource.Budget.Hours - resource.ProjectedRemaining.Hours);
                        if (showrevenue)
                        {
                            row.Add(resource.Budget.Revenue.Value - resource.ProjectedRemaining.Revenue.Value);
                        }
                        if (showMargin)
                        {
                            row.Add(resource.Budget.Margin.Value - resource.ProjectedRemaining.Margin.Value);
                        }
                        break;
                }

                data.Rows.Add(row.ToArray());
            }
            //Services Total
            row = new List<object>();
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Services Total"));
            PrepareExcelGrid(row, BudgetManagement.BudgetSummary, showrevenue, showMargin, true);
            if (showActual)
            {
                PrepareExcelGrid(row, BudgetManagement.ActualsSummary, showrevenue, showMargin, isSummary: true);

            }
            if (showProjected)
            {
                PrepareExcelGrid(row, BudgetManagement.ProjectedSummary, showrevenue, showMargin, isSummary: true);

            }
            if (showEAC)
            {
                PrepareExcelGrid(row, BudgetManagement.EACSummary, showrevenue, showMargin, isSummary: true);

            }
            switch (View)
            {
                case 1: break;
                case 2:

                    row.Add(BudgetManagement.BudgetSummary.Hours - BudgetManagement.EACSummary.Hours);
                    if (showrevenue)
                    {
                        row.Add((BudgetManagement.BudgetSummary.Revenue.Value - BudgetManagement.EACSummary.Revenue.Value));
                    }
                    if (showMargin)
                    {
                        row.Add((BudgetManagement.BudgetSummary.Margin.Value - BudgetManagement.EACSummary.Margin.Value));
                    }
                    break;
                case 3:
                    row.Add(BudgetManagement.BudgetSummary.Hours - BudgetManagement.EACSummary.Hours);
                    if (showrevenue)
                    {
                        row.Add((BudgetManagement.BudgetSummary.Revenue.Value - BudgetManagement.EACSummary.Revenue.Value));
                    }
                    if (showMargin)
                    {
                        row.Add((BudgetManagement.BudgetSummary.Margin.Value - BudgetManagement.EACSummary.Margin.Value));
                    }

                    break;
                case 4:
                    row.Add(BudgetManagement.BudgetSummary.Hours - BudgetManagement.ActualsSummary.Hours);
                    if (showrevenue)
                    {
                        row.Add((BudgetManagement.BudgetSummary.Revenue.Value - BudgetManagement.ActualsSummary.Revenue.Value));
                    }
                    if (showMargin)
                    {
                        row.Add((BudgetManagement.BudgetSummary.Margin.Value - BudgetManagement.ActualsSummary.Margin.Value));
                    }

                    break;
                case 5:
                    row.Add(BudgetManagement.BudgetSummary.Hours - BudgetManagement.ProjectedSummary.Hours);
                    if (showrevenue)
                    {
                        row.Add((BudgetManagement.BudgetSummary.Revenue.Value - BudgetManagement.ProjectedSummary.Revenue.Value));
                    }
                    if (showMargin)
                    {
                        row.Add((BudgetManagement.BudgetSummary.Margin.Value - BudgetManagement.ProjectedSummary.Margin.Value));
                    }
                    break;
            }
            data.Rows.Add(row.ToArray());

            row = new List<object>();
            for (int i = 0; i < data.Columns.Count; i++)
            {
                row.Add("");
            }
            data.Rows.Add(row.ToArray());

            //Expense
            row = new List<object>();
            row.Add("Project Expenses");
            //budget
            if (showrevenue)
            {
                row.Add("");
            }
            if (showMargin)
            {
                row.Add("");
            }
            row.Add("");
            if (showrevenue)
            {
                row.Add(BudgetManagement.BudgetSummary.Expenses);
            }
            if (showMargin)
            {
                row.Add(BudgetManagement.BudgetSummary.Expenses);
                row.Add("");
            }
            if (showActual)
            {
                if (showrevenue)
                {
                    row.Add("");
                }
                if (showMargin)
                {
                    row.Add("");
                }
                row.Add("");
                if (showrevenue)
                {
                    row.Add(BudgetManagement.ActualsSummary.Expenses);
                }
                if (showMargin)
                {
                    row.Add(BudgetManagement.ActualsSummary.Expenses);
                    row.Add("");
                }
            }
            if (showProjected)
            {
                if (showrevenue)
                {
                    row.Add("");
                }
                if (showMargin)
                {
                    row.Add("");
                }
                row.Add("");
                if (showrevenue)
                {
                    row.Add(BudgetManagement.ProjectedSummary.Expenses);
                }
                if (showMargin)
                {
                    row.Add(BudgetManagement.ProjectedSummary.Expenses);
                    row.Add("");
                }
            }
            if (showEAC)
            {
                if (showrevenue)
                {
                    row.Add("");
                }
                if (showMargin)
                {
                    row.Add("");
                }
                row.Add("");
                if (showrevenue)
                {
                    row.Add(BudgetManagement.EACSummary.Expenses);
                }
                if (showMargin)
                {
                    row.Add(BudgetManagement.EACSummary.Expenses);
                    row.Add("");
                }
            }

            switch (View)
            {
                case 1: break;
                case 2:

                    row.Add("");
                    if (showrevenue)
                    {
                        row.Add(BudgetManagement.BudgetSummary.Expenses - BudgetManagement.EACSummary.Expenses);
                    }
                    if (showMargin)
                    {
                        row.Add(BudgetManagement.BudgetSummary.Expenses - BudgetManagement.EACSummary.Expenses);
                    }
                    break;
                case 3:
                    row.Add("");
                    if (showrevenue)
                    {
                        row.Add(BudgetManagement.BudgetSummary.Expenses - BudgetManagement.EACSummary.Expenses);
                    }
                    if (showMargin)
                    {
                        row.Add(BudgetManagement.BudgetSummary.Expenses - BudgetManagement.EACSummary.Expenses);
                    }

                    break;
                case 4:
                    row.Add("");
                    if (showrevenue)
                    {
                        row.Add(BudgetManagement.BudgetSummary.Expenses - BudgetManagement.ActualsSummary.Expenses);
                    }
                    if (showMargin)
                    {
                        row.Add(BudgetManagement.BudgetSummary.Expenses - BudgetManagement.ActualsSummary.Expenses);
                    }

                    break;
                case 5:
                    row.Add("");
                    if (showrevenue)
                    {
                        row.Add(BudgetManagement.BudgetSummary.Expenses - BudgetManagement.ProjectedSummary.Expenses);
                    }
                    if (showMargin)
                    {
                        row.Add(BudgetManagement.BudgetSummary.Expenses - BudgetManagement.ProjectedSummary.Expenses);
                    }
                    break;
            }
            data.Rows.Add(row.ToArray());

            //Total

            row = new List<object>();
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Total Expected Billing"));
            if (showrevenue)
            {
                row.Add("");
            }
            if (showMargin)
            {
                row.Add("");
            }
            row.Add("");
            if (showrevenue)
            {
                row.Add(BudgetManagement.BudgetSummary.TotalRevenue.Value);
            }
            if (showMargin)
            {
                row.Add(BudgetManagement.BudgetSummary.TotalMargin.Value);
                row.Add((BudgetManagement.BudgetSummary.TotalRevenue != 0 ? BudgetManagement.BudgetSummary.TotalMargin.Value * 100M / BudgetManagement.BudgetSummary.TotalRevenue.Value : 0M));
            }
            if (showActual)
            {
                if (showrevenue)
                {
                    row.Add("");
                }
                if (showMargin)
                {
                    row.Add("");
                }
                row.Add("");
                if (showrevenue)
                {
                    row.Add(BudgetManagement.ActualsSummary.TotalRevenue.Value);
                }
                if (showMargin)
                {
                    row.Add(BudgetManagement.ActualsSummary.TotalMargin.Value);
                    row.Add((BudgetManagement.ActualsSummary.TotalRevenue != 0 ? BudgetManagement.ActualsSummary.TotalMargin.Value * 100M / BudgetManagement.ActualsSummary.TotalRevenue.Value : 0M));
                }
            }
            if (showProjected)
            {
                if (showrevenue)
                {
                    row.Add("");
                }
                if (showMargin)
                {
                    row.Add("");
                }
                row.Add("");
                if (showrevenue)
                {
                    row.Add(BudgetManagement.ProjectedSummary.TotalRevenue.Value);
                }
                if (showMargin)
                {
                    row.Add(BudgetManagement.ProjectedSummary.TotalMargin.Value);
                    row.Add((BudgetManagement.ProjectedSummary.TotalRevenue != 0 ? BudgetManagement.ProjectedSummary.TotalMargin.Value * 100M / BudgetManagement.ProjectedSummary.TotalRevenue.Value : 0M));
                }
            }
            if (showEAC)
            {
                if (showrevenue)
                {
                    row.Add("");
                }
                if (showMargin)
                {
                    row.Add("");
                }
                row.Add("");
                if (showrevenue)
                {
                    row.Add(BudgetManagement.EACSummary.TotalRevenue.Value);
                }
                if (showMargin)
                {
                    row.Add(BudgetManagement.EACSummary.TotalMargin.Value);
                    row.Add((BudgetManagement.EACSummary.TotalRevenue != 0 ? BudgetManagement.EACSummary.TotalMargin.Value * 100M / BudgetManagement.EACSummary.TotalRevenue.Value : 0M));
                }
            }

            switch (View)
            {
                case 1: break;
                case 2:

                    row.Add("");
                    if (showrevenue)
                    {
                        row.Add((BudgetManagement.BudgetSummary.TotalRevenue - BudgetManagement.EACSummary.TotalRevenue).Value);
                    }
                    if (showMargin)
                    {
                        row.Add((BudgetManagement.BudgetSummary.TotalMargin - BudgetManagement.EACSummary.TotalMargin).Value);
                    }
                    break;
                case 3:
                    row.Add("");
                    if (showrevenue)
                    {
                        row.Add((BudgetManagement.BudgetSummary.TotalRevenue - BudgetManagement.EACSummary.TotalRevenue).Value);
                    }
                    if (showMargin)
                    {
                        row.Add((BudgetManagement.BudgetSummary.TotalMargin - BudgetManagement.EACSummary.TotalMargin).Value);
                    }

                    break;
                case 4:
                    row.Add("");
                    if (showrevenue)
                    {
                        row.Add((BudgetManagement.BudgetSummary.TotalRevenue - BudgetManagement.ActualsSummary.TotalRevenue).Value);
                    }
                    if (showMargin)
                    {
                        row.Add((BudgetManagement.BudgetSummary.TotalMargin - BudgetManagement.ActualsSummary.TotalMargin).Value);
                    }

                    break;
                case 5:
                    row.Add("");
                    if (showrevenue)
                    {
                        row.Add((BudgetManagement.BudgetSummary.TotalRevenue - BudgetManagement.ProjectedSummary.TotalRevenue).Value);
                    }
                    if (showMargin)
                    {
                        row.Add((BudgetManagement.BudgetSummary.TotalMargin - BudgetManagement.ProjectedSummary.TotalMargin).Value);
                    }
                    break;
            }
            data.Rows.Add(row.ToArray());
            return data;
        }

        private void PrepareExcelGrid(List<object> row, ProjectRevenue financial, bool showRevenue, bool showMargin, bool isSummary = false)
        {
            //if (!isBold)
            //{
            if (showRevenue)
            {
                row.Add(isSummary ? "" : financial.BillRate.Value.ToString());
            }
            if (showMargin)
            {
                row.Add(isSummary ? "" : financial.MarginRate.Value.ToString());
            }
            row.Add(financial.Hours);
            if (showRevenue)
            {
                row.Add(isSummary ? financial.Revenue.Value : financial.Revenue.Value);
            }
            if (showMargin)
            {
                row.Add(financial.Margin.Value);
                row.Add(isSummary ? (financial.Revenue != 0 ? financial.Margin.Value * 100M / financial.Revenue.Value : 0M) : (financial.Revenue != 0 ? financial.Margin.Value * 100M / financial.Revenue.Value : 0M));
            }
            //}
            //else
            //{
            //    if (showRevenue)
            //    {
            //        row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", isSummary ? "" : financial.BillRate.Value.ToString("$###,##0.00")));
            //    }
            //    if (showMargin)
            //    {
            //        row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", isSummary ? "" : financial.MarginRate.Value.ToString("$###,##0.00")));
            //    }
            //    row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", financial.Hours));
            //    if (showRevenue)
            //    {
            //        row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", (isSummary ? financial.Revenue.Value : financial.Revenue.Value).ToString(CurrencyDisplayFormat)));
            //    }
            //    if (showMargin)
            //    {
            //        row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", financial.Margin.Value.ToString(CurrencyDisplayFormat)));
            //        row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", (isSummary ? (financial.Revenue != 0 ? financial.Margin.Value * 100M / financial.Revenue.Value : 0M) : (financial.Revenue != 0 ? financial.Margin.Value * 100M / financial.Revenue.Value : 0M)).ToString("P")));
            //    }

            //}
        }
    }
}

