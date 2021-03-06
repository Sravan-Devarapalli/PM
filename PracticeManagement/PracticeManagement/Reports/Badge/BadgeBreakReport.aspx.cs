﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DataTransferObjects;
using PraticeManagement.Utils.Excel;
using System.Data;
using PraticeManagement.Utils;
using PraticeManagement.Controls;
using PraticeManagement.PersonStatusService;
using System.ServiceModel;
using System.Text;
using DataTransferObjects.Filters;

namespace PraticeManagement.Reports.Badge
{
    public partial class BadgeBreakReport : System.Web.UI.Page
    {
        public const string StartDateKey = "StartDate";
        public const string EndDateKey = "EndDate";
        public const string PayTypesKey = "PayTypes";
        public const string PersonStatusesKey = "PersonStatuses";
        private int coloumnsCount = 1;
        private int headerRowsCount = 1;

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
                sheetStyle.MergeRegion.Add(new int[] { 0, 0, 0, coloumnsCount > 12 ? coloumnsCount : 13 - 1 });
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

                CellStyles dataDateCellStyle = new CellStyles();
                dataDateCellStyle.DataFormat = "mm/dd/yy;@";
                dataDateCellStyle.HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment.Center;

                List<CellStyles> headerCellStyleList = new List<CellStyles>() { headerCellStyle };

                RowStyles headerrowStyle = new RowStyles(headerCellStyleList.ToArray());

                CellStyles dataCellStyle = new CellStyles();

                var dataCellStylearray = new List<CellStyles>() { dataCellStyle, dataCellStyle, dataDateCellStyle, dataDateCellStyle };

                RowStyles datarowStyle = new RowStyles(dataCellStylearray.ToArray());
                RowStyles[] rowStylearray = { headerrowStyle, datarowStyle };
                SheetStyles sheetStyle = new SheetStyles(rowStylearray);
                sheetStyle.TopRowNo = headerRowsCount;
                sheetStyle.IsFreezePane = true;
                sheetStyle.FreezePanColSplit = 0;
                sheetStyle.FreezePanRowSplit = headerRowsCount;
                return sheetStyle;
            }
        }

        public string StartDateFromQueryString
        {
            get
            {
                return Request.QueryString[StartDateKey];
            }
        }

        public string EndDateFromQueryString
        {
            get
            {
                return Request.QueryString[EndDateKey];
            }
        }

        public string PayTypesFromQueryString
        {
            get
            {
                return Request.QueryString[PayTypesKey];
            }
        }

        public string PersonStatusFromQueryString
        {
            get
            {
                return Request.QueryString[PersonStatusesKey];
            }
        }

        public string PersonStatus
        {
            get
            {
                var clientList = new StringBuilder();
                foreach (ListItem item in cblPersonStatus.Items)
                {
                    if (item.Selected)
                        clientList.Append(item.Value).Append(',');
                    if (item.Value == "1" && item.Selected)
                    {
                        clientList.Append("2").Append(',');
                        clientList.Append("5").Append(',');
                    }
                }
                return clientList.ToString();
            }
        }

        public string PayTypes
        {
            get
            {
                return cblPayTypes.areAllSelected ? null : cblPayTypes.SelectedItems;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                dtpEnd.DateValue = DataTransferObjects.Utils.Generic.MonthEndDate(DateTime.Now);
                dtpStart.DateValue = DataTransferObjects.Utils.Generic.MonthStartDate(DateTime.Now);
                DataHelper.FillTimescaleList(this.cblPayTypes, Resources.Controls.AllTypes);
                cblPayTypes.SelectItems(new List<int>() { 1, 2 });
                FillPersonStatusList();
                cblPersonStatus.SelectItems(new List<int>() { 1, 5 });
                if (!String.IsNullOrEmpty(StartDateFromQueryString))
                {
                    dtpStart.DateValue = Convert.ToDateTime(StartDateFromQueryString);
                    dtpEnd.DateValue = Convert.ToDateTime(EndDateFromQueryString);
                    cblPayTypes.SelectedItems = PayTypesFromQueryString == "null" ? null : PayTypesFromQueryString;
                    cblPersonStatus.SelectedItems = PersonStatusFromQueryString;
                    PopulateData();
                }
                else
                {
                    GetFilterValuesForSession();
                }
            }
        }

        public void FillPersonStatusList()
        {
            using (var serviceClient = new PersonStatusServiceClient())
            {
                try
                {
                    var statuses = serviceClient.GetPersonStatuses();
                    statuses = statuses.Where(p => p.Id != 2 && p.Id != 5 && p.Id != 6).ToArray();
                    DataHelper.FillListDefault(cblPersonStatus, Resources.Controls.AllTypes, statuses, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        protected void custNotMorethan2Years_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!reqBadgeStart.IsValid || !cvLastBadgeStart.IsValid || !reqbadgeEnd.IsValid || !cvbadgeEnd.IsValid)
                return;
            var totalMonths = (((dtpEnd.DateValue.Year - dtpStart.DateValue.Year) * 12) + dtpEnd.DateValue.Month - dtpStart.DateValue.Month);
            args.IsValid = totalMonths < 24;
        }

        protected void custNotBeforeJuly_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!reqBadgeStart.IsValid || !cvLastBadgeStart.IsValid || !reqbadgeEnd.IsValid || !cvbadgeEnd.IsValid)
                return;
            args.IsValid = dtpStart.DateValue >= new DateTime(2014, 7, 1);
        }

        protected void btnUpdateView_Click(object sender, EventArgs e)
        {
            SaveFilterValuesForSession();
            PopulateData();
        }

        public void PopulateData()
        {
            Page.Validate("BadgeReport");
            if (!Page.IsValid)
            {
                divWholePage.Style.Add("display", "none");
                return;
            }
            divWholePage.Style.Remove("display");
            lblRange.Text = dtpStart.DateValue.ToString(Constants.Formatting.EntryDateFormat) + " - " + dtpEnd.DateValue.ToString(Constants.Formatting.EntryDateFormat);
            var paytypes = cblPayTypes.areAllSelected ? null : cblPayTypes.SelectedItems;
            var statuses = PersonStatus;
            var resources = ServiceCallers.Custom.Report(r => r.ListBadgeResourcesByType(paytypes, statuses, dtpStart.DateValue, dtpEnd.DateValue, false, false, false, true, false, false, false).ToList());
            repblocked.DataSource = resources;
            repblocked.DataBind();
            if (resources.Count > 0)
            {
                divEmptyMessage.Style.Add("display", "none");
                repblocked.Visible = tblRange.Visible = true;
            }
            else
            {
                divEmptyMessage.Style.Remove("display");
                repblocked.Visible = tblRange.Visible = false;
            }
        }

        protected void btnExportToExcel_OnClick(object sender, EventArgs e)
        {
            var filename = string.Format("6MosBreakReport_{0}-{1}.xls", dtpStart.DateValue.ToString("MM_dd_yyyy"), dtpEnd.DateValue.ToString("MM_dd_yyyy"));
            var sheetStylesList = new List<SheetStyles>();
            var dataSetList = new List<DataSet>();
            var paytypes = cblPayTypes.areAllSelected ? null : cblPayTypes.SelectedItems;
            var statuses = PersonStatus;
            var report = ServiceCallers.Custom.Report(r => r.ListBadgeResourcesByType(paytypes, statuses, dtpStart.DateValue, dtpEnd.DateValue, false, false, false, true, false, false, false).ToList());
            if (report.Count > 0)
            {
                string dateRangeTitle = string.Format("6 mos break report for the period: {0} to {1}", dtpStart.DateValue.ToString(Constants.Formatting.EntryDateFormat), dtpEnd.DateValue.ToString(Constants.Formatting.EntryDateFormat));
                DataTable header = new DataTable();
                header.Columns.Add(dateRangeTitle);
                headerRowsCount = header.Rows.Count + 3;
                var data = PrepareDataTable(report);
                coloumnsCount = data.Columns.Count;
                sheetStylesList.Add(HeaderSheetStyle);
                sheetStylesList.Add(DataSheetStyle);
                var dataset = new DataSet();
                dataset.DataSetName = "6MonthsBreak";
                dataset.Tables.Add(header);
                dataset.Tables.Add(data);
                dataSetList.Add(dataset);
            }
            else
            {
                string dateRangeTitle = "There are no resources who are in 6 month break period for the selected dates.";
                DataTable header = new DataTable();
                header.Columns.Add(dateRangeTitle);
                sheetStylesList.Add(HeaderSheetStyle);
                var dataset = new DataSet();
                dataset.DataSetName = "6MonthsBreak";
                dataset.Tables.Add(header);
                dataSetList.Add(dataset);
            }
            NPOIExcel.Export(filename, dataSetList, sheetStylesList);
        }

        public DataTable PrepareDataTable(List<MSBadge> report)
        {
            DataTable data = new DataTable();
            List<object> row;

            data.Columns.Add("Resource Name");
            data.Columns.Add("Resource Level");
            data.Columns.Add("6-Month Break Start Date");
            data.Columns.Add("6-Month Break End Date");
            data.Columns.Add("6-Month Organic Start Date");
            data.Columns.Add("6-Month Organic End Date");
            foreach (var reportItem in report)
            {
                row = new List<object>();
                row.Add(reportItem.Person.Name);
                row.Add(reportItem.Person.Title.TitleName);
                row.Add(reportItem.BreakStartDate.HasValue ? reportItem.BreakStartDate.Value.ToShortDateString() : string.Empty);
                row.Add(reportItem.BreakEndDate.HasValue ? reportItem.BreakEndDate.Value.ToShortDateString() : string.Empty);
                row.Add(reportItem.OrganicBreakStartDate.HasValue ? reportItem.OrganicBreakStartDate.Value.ToShortDateString() : string.Empty);
                row.Add(reportItem.OrganicBreakEndDate.HasValue ? reportItem.OrganicBreakEndDate.Value.ToShortDateString() : string.Empty);
                data.Rows.Add(row.ToArray());
            }
            return data;
        }

        protected void repblocked_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var dataItem = (MSBadge)e.Item.DataItem;
                var lblBadgeStart = e.Item.FindControl("lblBadgeStart") as Label;
                var lblBadgeEnd = e.Item.FindControl("lblBadgeEnd") as Label;
                var lblOrganicStart = e.Item.FindControl("lblOrganicStart") as Label;
                var lblOrganicEnd = e.Item.FindControl("lblOrganicEnd") as Label;
                lblBadgeEnd.Text = dataItem.BreakEndDate.HasValue ? dataItem.BreakEndDate.Value.ToShortDateString() : string.Empty;
                lblBadgeStart.Text = dataItem.BreakStartDate.HasValue ? dataItem.BreakStartDate.Value.ToShortDateString() : string.Empty;
                lblOrganicStart.Text = dataItem.OrganicBreakStartDate.HasValue ? dataItem.OrganicBreakStartDate.Value.ToShortDateString() : string.Empty;
                lblOrganicEnd.Text = dataItem.OrganicBreakEndDate.HasValue ? dataItem.OrganicBreakEndDate.Value.ToShortDateString() : string.Empty;
            }
        }

        private void SaveFilterValuesForSession()
        {
            ResourceFilters filter = new ResourceFilters();
            filter.PersonStatusIds = PersonStatus;
            filter.PayTypeIds = PayTypes;
            filter.ReportStartDate = dtpStart.DateValue;
            filter.ReportEndDate = dtpEnd.DateValue;
            ReportsFilterHelper.SaveFilterValues(ReportName.BadgeBreakReport, filter);
        }

        private void GetFilterValuesForSession()
        {
            var filters = ReportsFilterHelper.GetFilterValues(ReportName.BadgeBreakReport) as ResourceFilters;
            if (filters != null)
            {
                cblPayTypes.UnSelectAll();
                cblPayTypes.SelectedItems = filters.PayTypeIds;
                cblPersonStatus.UnSelectAll();
                cblPersonStatus.SelectedItems = filters.PersonStatusIds;
                dtpStart.DateValue = (DateTime)filters.ReportStartDate;
                dtpEnd.DateValue = (DateTime)filters.ReportEndDate;
                PopulateData();
            }

        }
    }
}

