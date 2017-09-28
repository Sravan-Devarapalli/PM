using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DataTransferObjects;
using DataTransferObjects.TimeEntry;
using PraticeManagement.Controls;
using PraticeManagement.MilestonePersonService;
using PraticeManagement.MilestoneService;
using PraticeManagement.ProjectService;
using PraticeManagement.Security;
using PraticeManagement.Utils;
using Resources;
using System.Web;
using System.Threading;
using System.IO;
using PraticeManagement.Utils.Excel;
using System.Data;
using System.Globalization;
using DataTransferObjects.Reports;

namespace PraticeManagement
{
    public partial class MilestoneDetail : PracticeManagementPageBase, IPostBackEventHandler
    {
        #region Constants

        private int headerRowsCount = 1;


        private const string ProjectIdArgument = "projectId";
        private const int FirstMonthColumn = 5;
        private const string TotalHoursHeaderText = "Total Hours";
        private const int TotalRows = 10;
        private const int MilestoneNameLength = 55;
        private const string CurrencyFormat = "$###,###,###,###,##0.##";

        private decimal _totalAmount;
        private decimal _totalReimbursed;
        private decimal _totalReimbursementAmount;
        private int _expensesCount;

        private const string LblTotalamount = "lblTotalAmount";
        private const string LblTotalreimbursement = "lblTotalReimbursed";
        private const string LblTotalreimbursementamount = "lblTotalReimbursementAmount";

        private const string changeMilestonePersonsPopupMessageForStartDate = @"<p>You are trying to set a new milestone start date ({0}) that is earlier than the existing milestone start date ({1}).Do you also want to change the resource badge dates as well? Changing resource badge dates requires Operations approval again.</p><br/>
                                                                    <p>Select ""Change Milestone and Resources""  to update the start dates and badge start dates for all of the Resources attached to this milestone to ({2})
                                                                    or Select ""Change Milestone Only"" to leave the Resource start dates and badge start dates unchanged.</p>" + "<br/><p>" +
                                                                   "<b>Note:</b>  If any Resource(s) has a hire date after the new milestone start date their new start date will be their hire date.</p>" + "<br/>";

        private const string changeMilestonePersonsPopupMessageForEndDate = @"<p>You are trying to set a new milestone end date ({0}) that is later than the existing milestone end date ({1}).Do you also want to change the resource badge dates as well? Changing resource badge dates requires Operations approval again.</p><br/>
                                                                <p>Select ""Change Milestone and Resources""  to update the end dates and badge end dates for all of the Resources attached to this milestone to ({2}) or
                                                                Select ""Change Milestone Only"" to leave the Resource end dates and badge end dates unchanged.</p>" + "<br/><p>" +
                                                                "<b>Note:</b>  If any Resource(s) has a termination date before the new milestone end date their new end date will be their termination date.</p>" + "<br/>";

        private const string RemoveMilestonePersonsPopupMessageForStartDate = @"<p>You are trying to set a start date ({0}) for the milestone that is later than the following Resource(s) termination date:{1} <br/> Select OK to remove these Resource(s) from the milestone and update the milestone start date.</p>" + "<br/><p>" +
                                                                    @"Select Cancel to select another start date or to manually update Resource(s) attached to this milestone.</p>" + "<br/>";

        private const string RemoveMilestonePersonsPopupMessageForEndDate = @"<p>You are trying to set an end date ({0}) for the milestone that is before the following Resource(s) hire date:{1} <br/> If you Select OK these Resource(s) will be removed from the milestone and the end date will be updated.</p>" + "<br/><p>" +
                                                                    @"Select Cancel to select another end date or to manually update Resource(s) attached to this milestone.</p>" + "<br/>";

        private const string format = "{0} {1} ({2})";
        private const string lblCommissionsMessage = "Attribution {0} date will change from {1} to {2} based on the change to the project milestone.";
        private const string BadgeRequestRevisedDatesMailBody = "<html><body>{0} is requesting to change MS badge dates for {1}  from {2}-{3} to {4}-{5} for the milestone - <a href=\"{6}\">{7}</a> in {8}-{9}, please review & approve or decline.</body></html>";
        private const string BadgeRequestRevisedDatesExcpMailBody = "<html><body>{0} is requesting to change MS badge exception dates for {1}  from {2}-{3} to {4}-{5} for the milestone - <a href=\"{6}\">{7}</a> in {8}-{9}, please review & approve or decline.</body></html>";
        private int coloumnsCount = 1;

        #endregion Constants

        #region Fields

        private Milestone milestoneValue;

        protected int prevMilestoneId = -1;
        protected int nextMilestoneId = -1;

        private SeniorityAnalyzer seniorityAnalyzer;
        private MilestonePerson[] _milestonePersons;
        private SeniorityAnalyzer currentPersonSeniorityAnalyzer;

        #endregion Fields

        #region Properties

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
                RowStyles datarowStyle2 = new RowStyles(dataCellStylearray);
                datarowStyle2.Height = 600;

                RowStyles[] rowStylearray = { headerrowStyle, datarowStyle2, datarowStyle };

                SheetStyles sheetStyle = new SheetStyles(rowStylearray);
                sheetStyle.MergeRegion.Add(new int[] { 0, 0, 0, coloumnsCount > 10 ? coloumnsCount : 11 - 1 });
                sheetStyle.IsAutoResize = false;

                return sheetStyle;
            }
        }

        public SheetStyles DataStyles
        {
            get;
            set;
        }

        public bool IsSaveAllClicked
        {
            get;
            set;
        }

        private Milestone MilestoneObject
        {
            get;
            set;
        }

        private MilestoneUpdateObject MilestoneUpdateObject
        {
            get;
            set;
        }

        private MilestoneUpdateObject MilestoneUpdate
        {
            get { return ViewState["MilestoneUpdate_Key"] as MilestoneUpdateObject; }
            set { ViewState["MilestoneUpdate_Key"] = value; }
        }

        public bool ValidateNewEntry { get; set; }

        private bool? IsUserHasPermissionOnProject
        {
            get
            {
                int? milestoneId = MilestoneId;

                if (milestoneId.HasValue)
                {
                    if (ViewState["HasPermission"] == null)
                    {
                        ViewState["HasPermission"] = DataHelper.IsUserHasPermissionOnProject(User.Identity.Name, milestoneId.Value, false);
                    }
                    return (bool)ViewState["HasPermission"];
                }

                return null;
            }
        }

        private bool? IsUserisOwnerOfProject
        {
            get
            {
                int? id = MilestoneId;
                if (id.HasValue)
                {
                    if (ViewState["IsOwnerOfProject"] == null)
                    {
                        ViewState["IsOwnerOfProject"] = DataHelper.IsUserIsOwnerOfProject(User.Identity.Name, id.Value, false);
                    }
                    return (bool)ViewState["IsOwnerOfProject"];
                }

                return null;
            }
        }

        public List<int> MilestoneCSATAttributionCount
        {
            get
            {
                int? id = MilestoneId;
                if (id.HasValue)
                {
                    if (ViewState["MilestoneCSATAttributionCount"] == null)
                    {
                        using (var serviceClient = new MilestoneServiceClient())
                        {
                            try
                            {
                                ViewState["MilestoneCSATAttributionCount"] =
                                    serviceClient.GetMilestoneAndCSATCountsByProject(Milestone.Project.Id.Value).ToList();
                            }
                            catch (FaultException<ExceptionDetail>)
                            {
                                serviceClient.Abort();
                                throw;
                            }
                        }
                    }
                    return (List<int>)ViewState["MilestoneCSATAttributionCount"];
                }

                return null;
            }
            set { ViewState["MilestoneCSATAttributionCount"] = value; }
        }

        public DatePicker dtpPeriodToObject
        {
            get
            {
                return dtpPeriodTo;
            }
        }

        public DatePicker dtpPeriodFromObject
        {
            get
            {
                return dtpPeriodFrom;
            }
        }

        public SeniorityAnalyzer PersonListSeniorityAnalyzer
        {
            get
            {
                if (seniorityAnalyzer == null && MilestoneId.HasValue)
                {
                    seniorityAnalyzer = new SeniorityAnalyzer(DataHelper.CurrentPerson);
                    if (Milestone != null)
                    {
                        PersonListSeniorityAnalyzer.OneWithGreaterSeniorityExists(
                                GetMilestonePersons(Milestone)
                                );
                    }
                }
                return seniorityAnalyzer;
            }
            set
            {
                seniorityAnalyzer = value;
            }
        }

        public SeniorityAnalyzer MilestoneSeniorityAnalyzer
        {
            get
            {
                if (currentPersonSeniorityAnalyzer == null)
                {
                    currentPersonSeniorityAnalyzer = new SeniorityAnalyzer(DataHelper.CurrentPerson);
                }
                return currentPersonSeniorityAnalyzer;
            }
        }

        public int tierOneExceptionStatus
        {
            get
            {
                return (int)ViewState["tierOneExc"];
            }
            set
            {
                ViewState["tierOneExc"] = value;
            }
        }

        public int tierTwoExceptionStatus
        {
            get
            {
                return (int)ViewState["tierTwoExc"];
            }
            set
            {
                ViewState["tierTwoExc"] = value;
            }
        }

        public bool EnableEdit
        {
            get
            {
                return !(tierOneExceptionStatus == 1 || (tierOneExceptionStatus != 3 && tierTwoExceptionStatus == 1));
            }
        }

        public Project Project
        {
            get
            {
                if (ViewState["Project_Key"] == null)
                {
                    using (var serviceClient = new ProjectServiceClient())
                    {
                        try
                        {
                            var project = serviceClient.GetProjectDetailWithoutMilestones(
                                     SelectedProjectId.Value, User.Identity.Name);
                            tierOneExceptionStatus = project.TierOneExceptionStatus;
                            tierTwoExceptionStatus = project.TierTwoExceptionStatus;
                            ViewState["Project_Key"] = project;
                        }
                        catch (FaultException<ExceptionDetail>)
                        {
                            serviceClient.Abort();
                            throw;
                        }
                    }
                }

                return ViewState["Project_Key"] as Project;
            }
            set
            {
                ViewState["Project_Key"] = value;
            }
        }

        public Milestone Milestone
        {
            get
            {
                if (milestoneValue == null && MilestoneId.HasValue)
                {
                    if (ViewState["MileStone"] != null)
                        return ViewState["MileStone"] as Milestone;

                    milestoneValue = GetMilestoneById(MilestoneId);

                    ViewState["MileStone"] = milestoneValue;
                }

                return milestoneValue;
            }
            set
            {
                ViewState["MileStone"] = value;
            }
        }

        public string DiscountType
        {
            get
            {
                if (ViewState["Discount_Type_Key"] == null)
                {
                    ViewState["Discount_Type_Key"] = Milestone.PremiumType == 0 ? "1" : (Milestone.PremiumType).ToString();
                }
                return ViewState["Discount_Type_Key"] as string;
            }
            set
            {
                ViewState["Discount_Type_Key"] = value;
            }
        }

        public Milestone GetMilestoneById(int? id)
        {
            if (id.HasValue)
            {
                using (var serviceClient = new MilestoneServiceClient())
                {
                    try
                    {
                        milestoneValue = serviceClient.GetMilestoneDetail(id.Value);

                        Generic.RedirectIfNullEntity(milestoneValue, Response);

                        ViewState["MileStone"] = milestoneValue;
                    }
                    catch (FaultException<ExceptionDetail>)
                    {
                        serviceClient.Abort();
                        throw;
                    }
                }

                if (milestoneValue != null)
                {
                    milestoneValue.ActualActivity =
                        new List<TimeEntryRecord>(TimeEntryHelper.GetTimeEntriesForMilestone(milestoneValue));
                }
            }

            return milestoneValue;
        }

        public int? SelectedProjectId
        {
            get
            {
                return GetArgumentInt32(ProjectIdArgument);
            }
        }

        public int? MilestoneId
        {
            get
            {
                if (SelectedId.HasValue)
                {
                    return SelectedId;
                }
                else
                {
                    int id;
                    if (Int32.TryParse(hdnMilestoneId.Value, out id))
                    {
                        return id;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                hdnMilestoneId.Value = value.ToString();
            }
        }

        public bool IsShowResources
        {
            get
            {
                return MilestoneId.HasValue;
            }
        }

        public bool IsAttributionPanelDisplayed
        {
            get
            {
                if (ViewState["IsAttributionPanelDisplayed_Key"] == null)
                {
                    ViewState["IsAttributionPanelDisplayed_Key"] = false;
                }

                return (bool)ViewState["IsAttributionPanelDisplayed_Key"];
            }
            set
            {
                ViewState["IsAttributionPanelDisplayed_Key"] = value;
            }
        }

        public PraticeManagement.Controls.Milestones.MilestonePersonList MilestonePersonEntryListControlObject
        {
            get
            {
                return MilestonePersonEntryListControl;
            }
        }

        public MessageLabel lblResultObject
        {
            get
            {
                return lblResult;
            }
        }

        public int IsDiscountAtMilestoneLevel
        {
            get
            {
                // 0- for no change, 1- At miestonelevel, 2- At resource level
                if (ViewState["IsDiscountAtMilestoneLevel"] == null)
                {
                    ViewState["IsDiscountAtMilestoneLevel"] = 0;
                }
                return (int)ViewState["IsDiscountAtMilestoneLevel"];
            }
            set
            {
                ViewState["IsDiscountAtMilestoneLevel"] = value;
            }
        }


        private bool saveMonthlyRevenue
        {
            get;
            set;
        }

        public bool IsAmountAtMilestone
        {
            get
            {
                return chbLockFFAmount.Checked;
            }
        }

        public UpdatePanel UpnlBody
        {
            get
            {
                return upnlBody;
            }
        }

        #endregion Properties

        protected void Page_Load(object sender, EventArgs e)
        {
            IsSaveAllClicked = false;
            ValidateNewEntry = false;
            lblResult.ClearMessage();
            LoadPrevNextMilestones();
        }

        private void LoadPrevNextMilestones()
        {
            if (MilestoneId.HasValue)
            {
                activityLog.MilestoneId = MilestoneId;
                InitPrevNextButtons();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SetTooltipsForallDropDowns", "SetTooltipsForallDropDowns();", true);

            if (!string.IsNullOrEmpty(hdnEditEntryIdIndex.Value))
            {
                SelectView(btnResources, 2, false);
                LoadActiveTabIndex(2);
                MilestonePersonEntryListControl.EditResourceByEntryId(Convert.ToInt32(hdnEditEntryIdIndex.Value));
                hdnEditEntryIdIndex.Value = string.Empty;
            }
        }

        /// <summary>
        /// Initializes 'Previous Milestone' and 'Next Milestone' buttons
        /// </summary>
        protected void InitPrevNextButtons()
        {
            if (Milestone.Project.Id.HasValue && MilestoneId.HasValue && Project.Milestones != null)
            {
                var projectMiles = Project.Milestones.ToArray();

                var projMilesNum = projectMiles.Length;
                if (projMilesNum > 1)
                {
                    int mIndex = Array.FindIndex(projectMiles, v => v.Id == MilestoneId.Value);

                    if (mIndex > 0)
                        InitLink(projectMiles[mIndex - 1], lnkPrevMilestone, divLeft, captionLeft, lblLeft, ref prevMilestoneId);

                    if (mIndex < projMilesNum - 1)
                        InitLink(projectMiles[mIndex + 1], lnkNextMilestone, divRight, captionRight, lblRight, ref nextMilestoneId);

                    divPrevNextMainContent.Visible = true;
                }
                else
                {
                    divPrevNextMainContent.Visible = false;
                }
            }
        }

        private void InitLink(
            Milestone milestone,
            HyperLink hlink,
            HtmlGenericControl div,
            HtmlGenericControl span,
            HtmlGenericControl label,
            ref int milestoneId)
        {
            milestoneId = milestone.Id.Value;
            div.Visible = true;
            hlink.NavigateUrl = MilestoneRedirrectUrl(milestoneId);
            hlink.Attributes.Add("onclick", "javascript:checkDirty(\"" + milestoneId + "\")");

            span.InnerText = milestone.Description;
            label.InnerText
                = string.Format(
                    "({0} - {1})",
                    milestone.StartDate.ToString(Constants.Formatting.EntryDateFormat),
                    milestone.EndDate.ToString(Constants.Formatting.EntryDateFormat));
        }

        /// <summary>
        /// Emits a JavaScript which prevent the loss of non-saved data.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // New record adding
            AllowContinueWithoutSave = MilestoneId.HasValue;
            btnDelete.Enabled = MilestoneId.HasValue;

            // Move milestone fields visibility
            pnlMoveMilestone.Visible = MilestoneId.HasValue;

            // Clone milestone fields visibility
            pnlCloneMilestone.Visible = MilestoneId.HasValue && SelectedProjectId.HasValue;
        }

        protected void gvPeople_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var row = e.Row;
            var milestonePerson = row.DataItem as MilestonePerson;

            if (row.RowType == DataControlRowType.DataRow && milestonePerson != null)
            {
                if (milestonePerson.Person != null && milestonePerson.Person.ProjectedFinancialsByMonth != null &&
                    milestonePerson.Person.ProjectedFinancialsByMonth.Count > 0)
                {
                    var isOtherGreater = PersonListSeniorityAnalyzer.IsOtherGreater(milestonePerson.Person);

                    var dtTemp =
                        new DateTime(milestonePerson.Milestone.StartDate.Year, milestonePerson.Milestone.StartDate.Month,
                                     1);
                    // Filling monthly workload for the person.
                    int currentColumnIndex = FirstMonthColumn;
                    for (;
                        dtTemp <= milestonePerson.Milestone.ProjectedDeliveryDate;
                        currentColumnIndex++, dtTemp = dtTemp.AddMonths(1))
                    {
                        // The person works on the milestone at the month - has some workload
                        foreach (KeyValuePair<DateTime, ComputedFinancials> financials in
                            milestonePerson.Person.ProjectedFinancialsByMonth)
                        {
                            // Find a record for the month we need for the column
                            if (financials.Key.Month == dtTemp.Month && financials.Key.Year == dtTemp.Year)
                                row.Cells[currentColumnIndex].Text =
                                    string.Format(Resources.Controls.MilestoneInterestFormat,
                                                  financials.Value.Revenue, financials.Value.GrossMargin.ToString(isOtherGreater),
                                                  financials.Value.HoursBilled);
                        }
                    }
                }

                if (milestonePerson.StartDate > Milestone.EndDate)
                {
                    row.BackColor = Color.FromArgb(0xff, 0xe6, 0xe0);
                    lblError.ShowErrorMessage(Messages.PersonsStartGreaterMilestoneStart);
                }

                var imgEdit = row.FindControl("imgEdit") as ImageButton;

                if (imgEdit != null)
                {
                    var rowSa = new SeniorityAnalyzer(DataHelper.CurrentPerson);
                    if (rowSa.IsOtherGreater(milestonePerson.Person))
                    {
                        if (!(IsUserisOwnerOfProject.HasValue && IsUserisOwnerOfProject.Value))
                        {
                            if (!Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.PracticeManagerRoleName)
                                || !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.BusinessUnitManagerRoleName)
                                || !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.DirectorRoleName)// #2817: DirectorRoleName is added as per the requirement.
                                || !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.SeniorLeadershipRoleName)) // #2913: userIsSeniorLeadership is added as per the requirement.
                            {
                                imgEdit.Enabled = false;
                            }
                        }
                    }

                    imgEdit.Attributes.Add("mpEntryId", milestonePerson.Entries[0].Id.ToString());
                    imgEdit.Visible = EnableEdit;
                }
            }
        }

        #region Validation

        protected void cvMilestoneName_Validate(object sender, ServerValidateEventArgs e)
        {
            int length = txtMilestoneName.Text.Length;

            e.IsValid = (length <= MilestoneNameLength);
        }

        protected void Revenue_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRevenueState();
            if (Milestone != null)
            {
                LoadActiveTabIndex(mvMilestoneDetailTab.ActiveViewIndex);
            }
            UpnlBody.Update();
        }

        protected void txtFixedRevenue_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFixedRevenue.Text))
            {
                Decimal revenue;
                if (Decimal.TryParse(txtFixedRevenue.Text, out revenue) && Milestone != null && Milestone.ComputedFinancials != null)
                {
                    var premiumDiscount = revenue - Milestone.ComputedFinancials.MilestonePersonAmount;
                    txtMilestoneDiscount.Text = ddlDiscountType.SelectedValue == "1" ? ((decimal)premiumDiscount).ToString("###,###,###,###,##0") : (((decimal)premiumDiscount * 100) / Milestone.ComputedFinancials.MilestonePersonAmount.Value).ToString("##0.##");
                    lblBlendedRate.Text = Milestone.ComputedFinancials.HoursBilled != 0 ? (revenue / Milestone.ComputedFinancials.HoursBilled).ToString(CurrencyFormat) : "-";
                }
                IsDiscountAtMilestoneLevel = 1;
            }
            ShowOrHideDiscount();
            UpnlBody.Update();
        }

        protected void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            IsDiscountAtMilestoneLevel = 1;
            if (!string.IsNullOrEmpty(txtMilestoneDiscount.Text) && !chbLockFFAmount.Checked)
            {
                Decimal discount;
                if (Decimal.TryParse(txtMilestoneDiscount.Text, out discount) && Milestone != null && Milestone.ComputedFinancials != null)
                {
                    var revenue = ddlDiscountType.SelectedValue == "1" ? discount + Milestone.ComputedFinancials.MilestonePersonAmount.Value : ((discount * Milestone.ComputedFinancials.MilestonePersonAmount.Value / 100) + Milestone.ComputedFinancials.MilestonePersonAmount.Value);
                    txtFixedRevenue.Text = revenue.ToString("###,###,###,###,##0.##");
                    lblBlendedRate.Text = Milestone.ComputedFinancials.HoursBilled != 0 ? (revenue / Milestone.ComputedFinancials.HoursBilled).ToString(CurrencyFormat) : "-";
                }
            }
            DiscountType = ddlDiscountType.SelectedValue;
            if (DiscountType == "1")
            {
                lblDoller.Attributes.Add("style", "display: inline");
                lblPercentage.Attributes.Add("style", "display: none");
            }
            else
            {
                lblDoller.Attributes.Add("style", "display: none");
                lblPercentage.Attributes.Add("style", "display: inline");
            }
            //lblDoller.Visible = ddlDiscountType.SelectedValue == "1";
            //lblPercentage.Visible = ddlDiscountType.SelectedValue == "2";
            ShowOrHideDiscount();
            UpnlBody.Update();
        }

        protected void custStartandEndDate_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !(hdnCanShowPopup.Value == "true");

            if (!args.IsValid)
            {
                mpePopup.Show();
            }
        }

        #endregion Validation

        #region Preventing dirty loss

        protected void dtpPeriod_SelectionChanged(object sender, EventArgs e)
        {
            IsDirty = true;
            //lnkbtnMonthlyRevenue.Enabled = false;
            SetPopupMessageTextAndAssignHiddenFieldValues();
            UpnlBody.Update();
        }

        #endregion Preventing dirty loss

        private bool OnSaveClick(MilestoneUpdateObject milestoneUpdateObj = null)
        {
            var result = true;
            //reqFixedRevenue.Enabled = rbtnFixedRevenue.Checked && string.IsNullOrEmpty(txtDiscount.Text);
            Page.Validate(vsumMilestone.ValidationGroup);
            if (Page.IsValid)
            {
                Page.Validate(vsumPopup.ValidationGroup);
                if (Page.IsValid)
                {
                    hdnIsUpdate.Value = true.ToString();
                    Page.Validate("AttributionPopup");
                }
                if (Page.IsValid)
                {
                    bool isNew = !MilestoneId.HasValue;
                    if (!MilestoneId.HasValue)
                    {
                        int? id = SaveData();

                        if (id.HasValue)
                        {
                            MilestoneId = id;
                        }
                    }
                    else
                    {
                        result = ValidateandSaveData(milestoneUpdateObj);
                    }

                    if (MilestoneId.HasValue && result)
                    {
                        if (saveMonthlyRevenue)
                        {
                            SaveMonthlyRevenue();
                        }
                        if (isNew)
                        {
                            Response.Redirect(Utils.Generic.GetTargetUrlWithReturn(String.Format("~/MilestoneDetail.aspx?id={0}&projectId={1}", MilestoneId.Value, SelectedProjectId.Value),
                                                            String.Format(Constants.ApplicationPages.DetailRedirectFormat, Constants.ApplicationPages.ProjectDetail, SelectedProjectId.Value)));
                        }
                        Milestone = GetMilestoneById(MilestoneId);
                        MilestonePersonEntryListControlObject.GetLatestData();
                        mvMilestoneDetailTab.Visible = true;
                        tblMilestoneDetailTabViewSwitch.Visible = true;
                        LoadPrevNextMilestones();
                        lblResult.ShowInfoMessage(Messages.MilestoneSavedMessage);
                        ClearDirty();
                    }
                }
                else
                {
                    lblResult.ClearMessage();
                }
            }
            else
            {
                lblResult.ClearMessage();
            }

            if (Page.IsValid && MilestoneId.HasValue && result)
            {
                if (Milestone != null)
                {
                    Project = Milestone.Project;
                    PopulateControls(Milestone, true);
                }
            }

            LoadActiveTabIndex(mvMilestoneDetailTab.ActiveViewIndex);

            return result && Page.IsValid;
        }

        protected void btnOkAttribution_Click(object sender, EventArgs e)
        {
            if (hdnIsUpdate.Value == false.ToString())
                btnDelete_Click(btnDelete, new EventArgs());
            else
                btnSave_Click(btnSave, new EventArgs());
        }

        protected void btnCancelAttribution_Click(object sender, EventArgs e)
        {
            IsAttributionPanelDisplayed = false;
            if (hdnCanShowPopup.Value == "false")
                btnCancel_OnClick(btnCancelSaving, new EventArgs());
            mpeAttribution.Hide();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Page.Validate(vsumMilestone.ValidationGroup);
            if (Page.IsValid)
            {
                Page.Validate(vsumPopup.ValidationGroup);
                if (Page.IsValid)
                {
                    hdnIsUpdate.Value = true.ToString();
                    Page.Validate("AttributionPopup");
                }
                if (Page.IsValid)
                {
                    var startDate = dtpPeriodFrom.DateValue;
                    var endDate = dtpPeriodTo.DateValue;
                    int monthsCount = Math.Abs((endDate.Month - startDate.Month) + 12 * (endDate.Year - startDate.Year));
                    if (rbtnFixedRevenue.Checked && monthsCount > 0 && txtFixedRevenue.Text != string.Empty && hdnCanShowMonthlyRevenue.Value == "false")
                    {
                        var revenue = Convert.ToDecimal(txtFixedRevenue.Text);
                        if (!MilestoneId.HasValue || (startDate != Milestone.StartDate || endDate != Milestone.EndDate || (Milestone.Amount.HasValue && revenue != Milestone.Amount.Value) || (Milestone.Amount == null && txtFixedRevenue.Text.Length > 0)))
                        {
                            PopulateMonthlyRevenue();
                            return;
                        }

                    }
                    SaveMilestoneData();
                }
            }
            UpnlBody.Update();
        }


        private void SaveMilestoneData()
        {
            if (MilestoneId.HasValue)
            {
                DateTime PeriodFrom = Convert.ToDateTime(hdnPeriodFrom.Value);
                DateTime PeriodTo = Convert.ToDateTime(hdnPeriodTo.Value);

                if ((hdnCanShowPopup.Value == "false" && PeriodFrom != dtpPeriodFrom.DateValue) ||
                    (hdnCanShowPopup.Value == "false" && PeriodTo != dtpPeriodTo.DateValue))
                {
                    var milestoneUpdateObj = new MilestoneUpdateObject();

                    if (tblchangeMilestonePersonsForStartDate.Visible)
                    {
                        milestoneUpdateObj.IsStartDateChangeReflectedForMilestoneAndPersons = rbtnchangeMileStoneAndPersonsStartDate.Checked;
                    }
                    else if (PeriodFrom != dtpPeriodFrom.DateValue)
                    {
                        milestoneUpdateObj.IsStartDateChangeReflectedForMilestoneAndPersons = true;
                    }

                    if (tblchangeMilestonePersonsForEndDate.Visible)
                    {
                        milestoneUpdateObj.IsEndDateChangeReflectedForMilestoneAndPersons = rbtnchangeMileStoneAndPersonsEndDate.Checked;
                    }
                    else if (PeriodTo != dtpPeriodTo.DateValue)
                    {
                        milestoneUpdateObj.IsEndDateChangeReflectedForMilestoneAndPersons = true;
                    }
                    if (PeriodFrom.Date > dtpPeriodFrom.DateValue || PeriodTo.Date < dtpPeriodTo.DateValue || !(PeriodFrom.Date <= dtpPeriodTo.DateValue && dtpPeriodFrom.DateValue <= PeriodTo.Date))
                        milestoneUpdateObj.IsExtendedORCompleteOutOfRange = true;
                    else
                        milestoneUpdateObj.IsExtendedORCompleteOutOfRange = false;


                    OnSaveClick(milestoneUpdateObj);
                }
                else
                {
                    OnSaveClick();
                }
            }
            else
            {
                OnSaveClick();
            }
        }

        protected void btnSavePopup_OnClick(object sender, EventArgs e)
        {
            DateTime PeriodFrom = Convert.ToDateTime(hdnPeriodFrom.Value);
            DateTime PeriodTo = Convert.ToDateTime(hdnPeriodTo.Value);

            var milestoneUpdateObj = new MilestoneUpdateObject();

            if (tblchangeMilestonePersonsForStartDate.Visible)
            {
                milestoneUpdateObj.IsStartDateChangeReflectedForMilestoneAndPersons = rbtnchangeMileStoneAndPersonsStartDate.Checked;
            }
            else if (PeriodFrom != dtpPeriodFrom.DateValue)
            {
                milestoneUpdateObj.IsStartDateChangeReflectedForMilestoneAndPersons = true;
            }

            if (tblchangeMilestonePersonsForEndDate.Visible)
            {
                milestoneUpdateObj.IsEndDateChangeReflectedForMilestoneAndPersons = rbtnchangeMileStoneAndPersonsEndDate.Checked;
            }
            else if (PeriodTo != dtpPeriodTo.DateValue)
            {
                milestoneUpdateObj.IsEndDateChangeReflectedForMilestoneAndPersons = true;
            }
            if (PeriodFrom.Date > dtpPeriodFrom.DateValue || PeriodTo.Date < dtpPeriodTo.DateValue || !(PeriodFrom.Date <= dtpPeriodTo.DateValue && dtpPeriodFrom.DateValue <= PeriodTo.Date))
                milestoneUpdateObj.IsExtendedORCompleteOutOfRange = true;
            else
                milestoneUpdateObj.IsExtendedORCompleteOutOfRange = false;

            hdnCanShowPopup.Value = "false";
            MilestoneUpdate = milestoneUpdateObj;

            var startDate = dtpPeriodFrom.DateValue;
            var endDate = dtpPeriodTo.DateValue;
            int monthsCount = Math.Abs((endDate.Month - startDate.Month) + 12 * (endDate.Year - startDate.Year));
            if (rbtnFixedRevenue.Checked && monthsCount > 0 && txtFixedRevenue.Text != string.Empty && hdnCanShowMonthlyRevenue.Value == "false")
            {
                var revenue = Convert.ToDecimal(txtFixedRevenue.Text);
                if (!MilestoneId.HasValue)
                {
                    PopulateMonthlyRevenue();
                    return;
                }
                else if ((startDate != Milestone.StartDate || endDate != Milestone.EndDate || revenue != Milestone.Amount.Value))
                {
                    PopulateMonthlyRevenue();
                    return;
                }
            }


            var result = OnSaveClick(milestoneUpdateObj);
            hdnCanShowPopup.Value = !result && IsAttributionPanelDisplayed ? "false" : "true";

            if (result)
            {
                SetDefaultValuesInPopup();
            }
        }

        protected void btnCancel_OnClick(object sender, EventArgs e)
        {
            mpePopup.Hide();
            SetDefaultValuesInPopup();
            if (MilestoneId.HasValue && Milestone != null)
            {
                Project = Milestone.Project;
                PopulateControls(Milestone, true);
                lblResult.ClearMessage();
                ClearDirty();
                hdnCanShowPopup.Value = "false";
            }
            if (Milestone != null)
            {
                LoadActiveTabIndex(mvMilestoneDetailTab.ActiveViewIndex);
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            Page.Validate("MilestoneDelete");
            if (Page.IsValid)
            {
                hdnIsUpdate.Value = false.ToString();
                Page.Validate("AttributionPopup");
            }
            if (Page.IsValid)
            {
                try
                {
                    DeleteRecord();
                    IsAttributionPanelDisplayed = false;
                    ReturnToPreviousPage();
                }
                catch (Exception exception)
                {
                    lblError.ShowErrorMessage("{0}", exception.Message);
                }
            }
        }

        protected void custMilestoneDatesConflict_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            DateTime oldStartDate;
            var newMilestoneStartDate = dtpPeriodFrom.DateValue;
            var newMilestoneEndDate = dtpPeriodTo.DateValue;
            if (DateTime.TryParse(hdnPeriodFrom.Value, out oldStartDate) && MilestoneId.HasValue)
            {
                if (oldStartDate.Date >= dtpPeriodFrom.DateValue.Date)
                {
                    var badgedPeople = ServiceCallers.Custom.Milestone(m => m.GetPeopleAssignedInOtherProjectsForGivenRange(newMilestoneStartDate, newMilestoneEndDate, MilestoneId.Value)).ToList();
                    e.IsValid = badgedPeople.Count == 0;
                    if (!e.IsValid)
                    {
                        mpeMilestoneDatesConflict.Show();
                        repBadgePeople.DataSource = badgedPeople;
                        repBadgePeople.DataBind();
                    }
                }
            }
        }

        protected void cvAttributionPopup_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            e.IsValid = true;
            if (IsAttributionPanelDisplayed)
                return;
            List<Attribution> attributionList = new List<Attribution>();
            List<bool> extendAttributionDates = new List<bool>();
            using (var service = new MilestoneServiceClient())
            {
                if (MilestoneId != null)
                {
                    attributionList =
                        service.IsProjectAttributionConflictsWithMilestoneChanges(MilestoneId.Value,
                                                                                  dtpPeriodFrom
                                                                                      .DateValue,
                                                                                  dtpPeriodTo.DateValue,
                                                                                  hdnIsUpdate.Value == true.ToString())
                               .ToList();

                }

            }
            if (attributionList.Any())
            {
                trAttributionRecord.Visible = true;
                IsAttributionPanelDisplayed = true;
                mpeAttribution.Show();
                if (attributionList.Any(x => x.AttributionType == AttributionTypes.Delivery))
                {
                    repDeliveryPersons.DataSource =
                        attributionList.Where(x => x.AttributionType == AttributionTypes.Delivery)
                                       .OrderBy(x => x.TargetName)
                                       .ThenBy(x => x.StartDate);
                    repDeliveryPersons.DataBind();
                }
                if (attributionList.Any(x => x.AttributionType == AttributionTypes.Sales))
                {
                    repSalesPersons.DataSource =
                        attributionList.Where(x => x.AttributionType == AttributionTypes.Sales)
                                       .OrderBy(x => x.TargetName)
                                       .ThenBy(x => x.StartDate);
                    repSalesPersons.DataBind();
                }
                e.IsValid = false;
            }
            else
                trAttributionRecord.Visible = false;

            if (hdnIsUpdate.Value == true.ToString())
            {
                using (var service = new MilestoneServiceClient())
                {
                    if (SelectedProjectId != null)
                        extendAttributionDates =
                            service.ShouldAttributionDateExtend(SelectedProjectId.Value,
                                                                dtpPeriodFrom
                                                                    .DateValue,
                                                                dtpPeriodTo.DateValue)
                                   .ToList();
                }
                if (SelectedProjectId != null && (extendAttributionDates[0] || extendAttributionDates[1]))
                {
                    IsAttributionPanelDisplayed = true;
                    mpeAttribution.Show();
                    if (extendAttributionDates[0])
                    {
                        trCommissionsStartDateExtend.Visible = true;
                        lblCommissionsStartDateExtendMessage.Text = string.Format(lblCommissionsMessage, "start date", Project.StartDate.Value.ToShortDateString(), dtpPeriodFrom.DateValue.ToShortDateString());
                    }
                    else
                    {
                        trCommissionsStartDateExtend.Visible = false;
                    }
                    if (extendAttributionDates[1])
                    {
                        trCommissionsEndDateExtend.Visible = true;
                        lblCommissionsEndDateExtendMessage.Text = string.Format(lblCommissionsMessage, "end date", Project.EndDate.Value.ToShortDateString(), dtpPeriodTo.DateValue.ToShortDateString());
                    }
                    else
                    {
                        trCommissionsEndDateExtend.Visible = false;
                    }
                    e.IsValid = false;
                }
                else
                {
                    trCommissionsStartDateExtend.Visible = false;
                    trCommissionsEndDateExtend.Visible = false;
                }
            }
            else
            {
                trCommissionsStartDateExtend.Visible = false;
                trCommissionsEndDateExtend.Visible = false;
            }

        }

        protected void custExpenseValidate_OnServerValidate(object sender, ServerValidateEventArgs e)
        {
            if (Milestone.Project.StartDate.Value == Milestone.StartDate ||
                Milestone.Project.EndDate.Value == Milestone.EndDate)
            {
                using (var service = new MilestoneServiceClient())
                {
                    if (service.CheckIfExpensesExistsForMilestonePeriod(MilestoneId.Value, null, null))
                    {
                        e.IsValid = false;
                    }
                    else
                    {
                        e.IsValid = true;
                    }
                }
            }
            else
            {
                e.IsValid = true;
            }
        }

        protected void custProjectStatus_OnServerValidate(object sender, ServerValidateEventArgs e)
        {
            e.IsValid = (Project.Status.StatusType == ProjectStatusType.Active && MilestoneCSATAttributionCount[0] == 1) ? false : true;
        }

        protected void custCSATValidate_OnServerValidate(object sender, ServerValidateEventArgs e)
        {
            e.IsValid = (MilestoneCSATAttributionCount[0] != 1 || MilestoneCSATAttributionCount[1] <= 0);
        }

        protected void custAttribution_OnServerValidate(object sender, ServerValidateEventArgs e)
        {
            e.IsValid = (MilestoneCSATAttributionCount[0] != 1 || MilestoneCSATAttributionCount[2] <= 0);
        }

        protected void custFeedback_OnServerValidate(object sender, ServerValidateEventArgs e)
        {
            e.IsValid = true;
            using (var serviceClient = new ProjectServiceClient())
            {
                try
                {
                    e.IsValid = !serviceClient.CheckIfFeedbackExists(null, MilestoneId.Value, null, null);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        protected void btnMoveMilestone_Click(object sender, EventArgs e)
        {
            lblResult.ClearMessage();
            Page.Validate(vsumShiftDays.ValidationGroup);
            if (Page.IsValid)
            {
                try
                {
                    var shiftDays = int.Parse(txtShiftDays.Text);
                    var newStartDate = Milestone.StartDate.AddDays(shiftDays);
                    var newEndDate = Milestone.EndDate.AddDays(shiftDays);

                    var badges = DataHelper.ShiftMilestone(
                        shiftDays,
                        MilestoneId.Value,
                        chbMoveFutureMilestones.Checked);
                    var loggedInPerson = DataHelper.CurrentPerson;
                    foreach (var badge in badges)
                    {
                        var project = new Project()
                        {
                            Id = badge.Project.Id
                        };
                        project.MailBody = badge.IsException ? string.Format(BadgeRequestRevisedDatesExcpMailBody, loggedInPerson.Name, badge.Person.Name, badge.LastBadgeStartDate.Value.ToShortDateString(), badge.LastBadgeEndDate.Value.ToShortDateString(), badge.BadgeStartDate.Value.ToShortDateString(), badge.BadgeEndDate.Value.ToShortDateString(),
                                                             "{0}", Milestone.Description, badge.Project.ProjectNumber, badge.Project.Name) :
                                                                                         string.Format(BadgeRequestRevisedDatesMailBody, loggedInPerson.Name, badge.Person.Name, badge.LastBadgeStartDate.Value.ToShortDateString(), badge.LastBadgeEndDate.Value.ToShortDateString(), badge.BadgeStartDate.Value.ToShortDateString(), badge.BadgeEndDate.Value.ToShortDateString(),
                                                                                         "{0}", Milestone.Description, badge.Project.ProjectNumber, badge.Project.Name);
                        ServiceCallers.Custom.Milestone(m => m.SendBadgeRequestMail(project, Milestone.Id.Value));
                    }
                    ReturnToPreviousPage();
                }
                catch (Exception ex)
                {
                    Logging.LogErrorMessage(
                           ex.Message,
                           ex.Source,
                           ex.InnerException != null ? ex.InnerException.Message : string.Empty,
                           string.Empty,
                           HttpContext.Current.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped),
                           string.Empty,
                           Thread.CurrentPrincipal.Identity.Name);
                }
            }
        }

        protected void btnClone_Click(object sender, EventArgs e)
        {
            Page.Validate(vsumClone.ValidationGroup);
            if (Page.IsValid)
            {
                int cloneDuration = int.Parse(txtCloneDuration.Text);
                using (MilestoneServiceClient serviceClient = new MilestoneServiceClient())
                {
                    try
                    {
                        int cloneId = serviceClient.MilestoneClone(MilestoneId.Value, cloneDuration);

                        Redirect(string.Concat(
                            string.Format(Constants.ApplicationPages.DetailRedirectFormat,
                            Constants.ApplicationPages.MilestoneDetail,
                            cloneId), "&projectId=", SelectedProjectId.Value.ToString()));
                    }
                    catch (CommunicationException)
                    {
                        serviceClient.Abort();
                        throw;
                    }
                }
            }
        }

        private void SaveAndRedirect(object args)
        {
            MilestoneUpdateObject milestoneUpdateObj = null;

            if (MilestoneId.HasValue)
            {
                DateTime PeriodFrom = Convert.ToDateTime(hdnPeriodFrom.Value);
                DateTime PeriodTo = Convert.ToDateTime(hdnPeriodTo.Value);

                if ((hdnCanShowPopup.Value == "false" && PeriodFrom != dtpPeriodFrom.DateValue) ||
                    (hdnCanShowPopup.Value == "false" && PeriodTo != dtpPeriodTo.DateValue))
                {
                    milestoneUpdateObj = new MilestoneUpdateObject();
                    if (PeriodFrom != dtpPeriodFrom.DateValue)
                    {
                        milestoneUpdateObj.IsStartDateChangeReflectedForMilestoneAndPersons = true;
                    }

                    if (PeriodTo != dtpPeriodTo.DateValue)
                    {
                        milestoneUpdateObj.IsEndDateChangeReflectedForMilestoneAndPersons = true;
                    }
                    if (PeriodFrom.Date > dtpPeriodFrom.DateValue || PeriodTo.Date < dtpPeriodTo.DateValue || !(PeriodFrom.Date <= dtpPeriodTo.DateValue && dtpPeriodFrom.DateValue <= PeriodTo.Date))
                        milestoneUpdateObj.IsExtendedORCompleteOutOfRange = true;
                    else
                        milestoneUpdateObj.IsExtendedORCompleteOutOfRange = false;
                }
            }

            if (!SaveDirty || ValidateandSaveData(milestoneUpdateObj))
            {
                int mpId;
                var isInt = int.TryParse(args.ToString(), out mpId);

                if (isInt)
                    Redirect(string.Format(Constants.ApplicationPages.RedirectMilestonePersonIdFormat,
                                           Constants.ApplicationPages.MilestonePersonDetail,
                                           MilestoneId.Value,
                                           args));
                else
                {
                    string url = args.ToString();
                    bool isQueryStringExists = false;

                    try
                    {
                        url = args.ToString().Remove(args.ToString().IndexOf('?'));
                        isQueryStringExists = true;
                    }
                    catch
                    {
                        url = args.ToString();
                        isQueryStringExists = false;
                    }

                    if (isQueryStringExists)
                    {
                        Response.Redirect(args.ToString());
                    }
                    else
                        Redirect(args.ToString());
                }
            }
        }

        protected string GetText(object value, Person person)
        {
            PracticeManagementCurrency c = 0;
            if (value != null)
            {
                c = (PracticeManagementCurrency)value;
            }

            var greaterSeniorityExists = MilestoneSeniorityAnalyzer.IsOtherGreater(person);

            return c.ToString(greaterSeniorityExists);
        }

        protected string GetMpeRedirectUrl(object args)
        {
            var mpePageUrl =
                string.Format(
                       Constants.ApplicationPages.RedirectMilestonePersonIdFormat + "&EnableEdit={3}",
                       Constants.ApplicationPages.MilestonePersonDetail,
                       MilestoneId.Value,
                       args, EnableEdit);

            var url = Request.Url.AbsoluteUri;

            if (!SelectedId.HasValue)
            {
                url = url.Replace("?", "?id=" + MilestoneId.Value.ToString() + "&");
            }

            return Generic.GetTargetUrlWithReturn(mpePageUrl, url);
        }

        protected override bool ValidateAndSave()
        {
            return ValidateandSaveData();
        }

        private bool ValidateandSaveData(MilestoneUpdateObject milestoneUpdateObj = null)
        {

            bool result = false;
            var showApprovedByOpsPopup = false;
            Person currentPerson = DataHelper.CurrentPerson;
            Page.Validate(vsumMilestone.ValidationGroup);
            if (Page.IsValid)
            {
                try
                {
                    Page.Validate(vsumPopup.ValidationGroup);
                    if (Page.IsValid)
                    {
                        hdnIsUpdate.Value = true.ToString();
                        Page.Validate("AttributionPopup");
                    }
                    if (Page.IsValid)
                    {
                        Page.Validate("MilestoneDatesConflict");
                    }
                    if (Page.IsValid)
                    {
                        if (!MilestonePersonEntryListControl.isInsertedRowsAreNotsaved)
                        {
                            result = SaveData(milestoneUpdateObj) > 0;

                            if (result)
                            {
                                ServiceCallers.Custom.MilestonePerson(mp => mp.MilestoneResourceUpdate(MilestoneObject, MilestoneUpdateObject, Context.User.Identity.Name));

                                foreach (var person in Milestone.MilestonePersons)
                                {
                                    if (person.Person.IsStrawMan)
                                        continue;
                                    ServiceCallers.Custom.Person(p => p.UpdateMSBadgeDetailsByPersonId(person.Person.Id.Value, currentPerson.Id.Value));
                                }
                                if (milestoneUpdateObj != null)
                                {
                                    var newMilestonePersons = ServiceCallers.Custom.MilestonePerson(m => m.GetMilestonePersonListByMilestone(Milestone.Id.Value)).ToList();
                                    foreach (var person in newMilestonePersons)
                                    {
                                        //var project = ServiceCallers.Custom.Project(p => p.GetProjectDetailWithoutMilestones(SelectedProjectId.Value, User.Identity.Name));
                                        var project = ServiceCallers.Custom.Project(pro => pro.ProjectGetShortById(SelectedProjectId.Value));//ServiceCallers.Custom.Project(pro => pro.ProjectGetById(SelectedProjectId.Value));
                                        var loggedInPerson = DataHelper.CurrentPerson;
                                        var oldPerson = new MilestonePerson();
                                        if (Milestone.MilestonePersons.Any(m => m.Person.Id == person.Person.Id))
                                            oldPerson = Milestone.MilestonePersons.First(m => m.Person.Id == person.Person.Id);
                                        else
                                            continue;
                                        if ((project.Status.StatusType == ProjectStatusType.Projected || project.Status.StatusType == ProjectStatusType.Active) && person.Entries[0].MSBadgeRequired && person.Entries[0].BadgeStartDate.HasValue && oldPerson.Entries[0].BadgeStartDate.HasValue && (oldPerson.Entries[0].BadgeStartDate.Value > person.Entries[0].BadgeStartDate.Value || oldPerson.Entries[0].BadgeEndDate.Value < person.Entries[0].BadgeEndDate.Value || !(oldPerson.Entries[0].BadgeStartDate.Value <= person.Entries[0].BadgeEndDate.Value && person.Entries[0].BadgeStartDate.Value <= oldPerson.Entries[0].BadgeEndDate.Value)))
                                        {
                                            project.MailBody = person.Entries[0].BadgeException ? string.Format(BadgeRequestRevisedDatesExcpMailBody, loggedInPerson.Name, person.Entries[0].ThisPerson.Name, oldPerson.Entries[0].BadgeStartDate.Value.ToShortDateString(), oldPerson.Entries[0].BadgeEndDate.Value.ToShortDateString(), person.Entries[0].BadgeStartDate.Value.ToShortDateString(), person.Entries[0].BadgeEndDate.Value.ToShortDateString(),
                                                         "{0}", Milestone.Description, project.ProjectNumber, project.Name) :
                                                                                     string.Format(BadgeRequestRevisedDatesMailBody, loggedInPerson.Name, person.Entries[0].ThisPerson.Name, oldPerson.Entries[0].BadgeStartDate.Value.ToShortDateString(), oldPerson.Entries[0].BadgeEndDate.Value.ToShortDateString(), person.Entries[0].BadgeStartDate.Value.ToShortDateString(), person.Entries[0].BadgeEndDate.Value.ToShortDateString(),
                                                                                     "{0}", Milestone.Description, project.ProjectNumber, project.Name);
                                            ServiceCallers.Custom.Milestone(m => m.SendBadgeRequestMail(project, Milestone.Id.Value));
                                            if (!(oldPerson.Entries[0].BadgeStartDate.Value <= person.Entries[0].BadgeEndDate.Value && person.Entries[0].BadgeStartDate.Value <= oldPerson.Entries[0].BadgeEndDate.Value))
                                                showApprovedByOpsPopup = true;
                                        }
                                    }
                                }
                                if (showApprovedByOpsPopup)
                                {
                                    mpeApprovedByOpsWhenCompleteOut.Show();
                                }
                            }
                        }
                        else
                        {
                            var index = mvMilestoneDetailTab.ActiveViewIndex;
                            var control = rowSwitcher.Cells[index].Controls[0];

                            SelectView(btnResources, 2, true);

                            IsSaveAllClicked = true;
                            ValidateNewEntry = true;
                            result = MilestonePersonEntryListControl.ValidateAll();

                            if (result)
                            {
                                result = SaveData(milestoneUpdateObj) > 0;
                                if (result)
                                {
                                    result = MilestonePersonEntryListControl.SaveAll();
                                }

                                if (result)
                                {
                                    ServiceCallers.Custom.MilestonePerson(mp => mp.MilestoneResourceUpdate(MilestoneObject, MilestoneUpdateObject, Context.User.Identity.Name));
                                    foreach (var person in Milestone.MilestonePersons)
                                    {
                                        if (person.Person.IsStrawMan)
                                            continue;
                                        ServiceCallers.Custom.Person(p => p.UpdateMSBadgeDetailsByPersonId(person.Person.Id.Value, currentPerson.Id.Value));
                                    }
                                    if (index != 2)
                                    {
                                        SelectView(control, index, true);
                                    }
                                }
                            }
                            else
                            {
                                //lblResult.ShowErrorMessage("Error occured while saving resources.");
                            }

                            ValidateNewEntry = false;
                            IsSaveAllClicked = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogErrorMessage(ex.Message,
                                               ex.Source,
                                               ex.InnerException != null ? ex.InnerException.Message : string.Empty,
                                               string.Empty,
                                               HttpContext.Current.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped),
                                               string.Empty,
                                               Thread.CurrentPrincipal.Identity.Name);
                }
            }
            return result;
        }

        protected void nMilestone_OnNoteAdded(object source, EventArgs args)
        {
            activityLog.Update();
        }

        public void dis()
        {

            //Display();
            if (Milestone != null)
            {
                var milestone = Milestone;
                Project = Milestone.Project;
                rbtnFixedRevenue.Checked = !milestone.IsHourlyAmount;
                rbtnHourlyRevenue.Checked = milestone.IsHourlyAmount;
                if (rbtnFixedRevenue.Checked)
                {
                    txtFixedRevenue.Text = milestone.Amount != null ? milestone.Amount.Value.Value.ToString() : string.Empty;
                    txtMilestoneDiscount.Text = lblMilestoneDiscount.Text = milestone.PremiumDiscount.HasValue ? milestone.PremiumDiscount.Value.ToString("###,###,###,###,##0.##") : string.Empty;
                    ddlDiscountType.SelectedValue = DiscountType;
                    if (DiscountType == "1")
                    {
                        lblDoller.Attributes.Add("style", "display: inline");
                        lblPercentage.Attributes.Add("style", "display: none");
                    }
                    else
                    {
                        lblDoller.Attributes.Add("style", "display: none");
                        lblPercentage.Attributes.Add("style", "display: inline");
                    }
                    chbLockFFAmount.Checked = milestone.IsAmountAtMilestone;
                    ShowOrHideDiscount();


                    decimal? blendedRate = null;

                    blendedRate = milestone.ComputedFinancials != null ? milestone.ComputedFinancials.BillRate : null;

                    lblBlendedRate.Text = blendedRate != null ? blendedRate.Value.ToString(CurrencyFormat) : "-";
                }
                lblResult.ClearMessage();
            }
        }

        protected override void Display()
        {
            int? id = MilestoneId;
            if (id.HasValue)
            {
                if (Milestone != null)
                {
                    Project = Milestone.Project;
                    PopulateControls(Milestone, true);
                    lblResult.ClearMessage();
                }
            }
            else
            {
                tblMilestoneDetailTabViewSwitch.Visible = false;
                mvMilestoneDetailTab.Visible = false;

                // Creating a new record
                Project project = this.Project;

                if (project != null)
                {
                    PopulateProjectControls(project);

                    dtpPeriodFrom.DateValue = project.MilestoneDefaultStartDate;
                    DateTime dtPeriodTo = project.MilestoneDefaultStartDate;
                    dtpPeriodTo.DateValue = dtPeriodTo.AddDays(1);
                    chbIsChargeable.Checked = project.IsChargeable;
                }

                chbConsultantsCanAdjust.Checked = false;

                gvPeople.DataBind();
            }

            UpdateRevenueState();

            if (!IsPostBack && mvMilestoneDetailTab.ActiveViewIndex == 0 && Milestone != null)
            {
                //PopulatePeopleGrid();
            }
        }

        private IEnumerable<Person> GetMilestonePersons(DataTransferObjects.Milestone Milestone)
        {
            foreach (var milestonePerson in Milestone.MilestonePersons)
            {
                yield return milestonePerson.Person;
            }
        }

        private void PopulatePeopleGrid()
        {
            if (Milestone == null)
                return;

            _milestonePersons = Milestone.MilestonePersons.OrderBy(mp => mp.Entries[0].ThisPerson.LastName).ThenBy(mp => mp.StartDate).AsQueryable().ToArray();

            if (_milestonePersons.Length > 0)
            {
                DateTime dtTemp =
                    new DateTime(_milestonePersons[0].Milestone.StartDate.Year, _milestonePersons[0].Milestone.StartDate.Month, 1);

                // Create the columns for the milestone months
                for (int i = FirstMonthColumn;
                    dtTemp <= _milestonePersons[0].Milestone.ProjectedDeliveryDate;
                    i++, dtTemp = dtTemp.AddMonths(1))
                {
                    var column = new BoundField
                    {
                        HeaderText = Resources.Controls.TableHeaderOpenTag +
                                                     dtTemp.ToString(Constants.Formatting.MonthYearFormat) +
                                                     Resources.Controls.TableHeaderCloseTag,
                        HtmlEncode = false
                    };
                    gvPeople.Columns.Insert(i, column);
                }
            }

            gvPeople.DataSource = _milestonePersons;
            gvPeople.DataBind();

            if (gvPeople.FooterRow != null)
            {
                for (int i = 0; i < gvPeople.FooterRow.Cells.Count; i++)
                {
                    gvPeople.FooterRow.Cells[i].RowSpan = TotalRows;
                }

                if (_milestonePersons != null && _milestonePersons.Length > 0)
                {
                    // Totals by months
                    DateTime dtTemp =
                        new DateTime(_milestonePersons[0].Milestone.StartDate.Year, _milestonePersons[0].Milestone.StartDate.Month, 1);
                    DateTime dtEnd = _milestonePersons[0].Milestone.ProjectedDeliveryDate;

                    Person currentPerson = DataHelper.CurrentPerson;

                    ComputedFinancials milestoneFinancials = new ComputedFinancials();
                    int r = 0;

                    for (int i = FirstMonthColumn; dtTemp <= dtEnd; i++, dtTemp = dtTemp.AddMonths(1))
                    {
                        SeniorityAnalyzer sa = new SeniorityAnalyzer(currentPerson);

                        ComputedFinancials financials = new ComputedFinancials();

                        bool oneGreaterExists = false;
                        foreach (MilestonePerson milestonePerson in _milestonePersons)
                        {
                            bool isOtherGreater = sa.IsOtherGreater(milestonePerson.Person);
                            if (isOtherGreater)
                                oneGreaterExists = true;

                            var financialsByMonth = milestonePerson.Person.ProjectedFinancialsByMonth;
                            if (financialsByMonth != null)
                            {
                                foreach (KeyValuePair<DateTime, ComputedFinancials> tmpFinancials in financialsByMonth)
                                {
                                    // Serch for the computed financials for the month
                                    if (tmpFinancials.Key.Month == dtTemp.Month &&
                                        tmpFinancials.Key.Year == dtTemp.Year)
                                    {
                                        financials.Revenue += tmpFinancials.Value.Revenue;
                                        financials.RevenueNet += tmpFinancials.Value.RevenueNet;
                                        financials.GrossMargin += tmpFinancials.Value.GrossMargin;
                                        financials.HoursBilled += tmpFinancials.Value.HoursBilled;
                                        break;
                                    }
                                }
                            }

                        }

                        if (financials.HoursBilled > 0)
                        {
                            gvPeople.FooterRow.Cells[i].Font.Bold = true;
                            gvPeople.FooterRow.Cells[i].Text =
                                string.Format(Resources.Controls.MilestoneSummaryInterestFormat,
                                financials.Revenue,
                                financials.GrossMargin.ToString(oneGreaterExists),
                                financials.HoursBilled,
                                oneGreaterExists ? Resources.Controls.HiddenCellText : financials.TargetMargin.ToString("##0.00"));
                        }
                        milestoneFinancials.Revenue += financials.Revenue;
                        milestoneFinancials.GrossMargin += financials.GrossMargin;
                        milestoneFinancials.HoursBilled += financials.HoursBilled;
                        r = i;
                    }
                    gvPeople.FooterRow.Cells[r + 2].Font.Bold = true;
                    gvPeople.FooterRow.Cells[r + 2].Text = milestoneFinancials.HoursBilled.ToString("##0.00");
                    gvPeople.FooterRow.Cells[r + 3].Font.Bold = true;
                    gvPeople.FooterRow.Cells[r + 3].Text = string.Format("<div class='Revenue fl-right'>&nbsp;{0}</div>", milestoneFinancials.Revenue);
                    gvPeople.FooterRow.Cells[r + 4].Font.Bold = true;
                    gvPeople.FooterRow.Cells[r + 4].Text = string.Format("<div class='Margin fl-right'> &nbsp;{0}</div>", milestoneFinancials.GrossMargin);


                }
            }

            // Calculate and display totals
            if (Milestone.ComputedFinancials != null)
            {
                lblTotalCogs.Text = Milestone.ComputedFinancials.Cogs.ToString(PersonListSeniorityAnalyzer.GreaterSeniorityExists);
                lblTotalRevenue.Text = Milestone.ComputedFinancials.Revenue.ToString();
                lblTotalRevenueNet.Text = Milestone.ComputedFinancials.RevenueNet.ToString();

                lblClientDiscount.Text = Milestone.Project.Discount.ToString("##0.00");

                lblClientDiscountAmount.Text =
                    (Milestone.ComputedFinancials.Revenue * Milestone.Project.Discount / 100).ToString();
            }
            else
            {
                lblClientDiscountAmount.Text = lblTotalRevenueNet.Text = lblTotalRevenue.Text = lblTotalCogs.Text = "-";
                lblClientDiscount.Text = "";
                lblTargetMargin.Style["background-color"] = "";
            }
        }

        private void SetFooterLabelText(string labelValue, string labelId)
        {
            var lbl = this.FindControl(labelId) as Label;
            if (lbl != null)
                lbl.Text = labelValue;
        }

        private void UpdateRevenueState()
        {
            txtFixedRevenue.Enabled = compFixedRevenue.Enabled = lnkbtnMonthlyRevenue.Visible = chbLockFFAmount.Enabled =
                rbtnFixedRevenue.Checked;

            lnkbtnMonthlyRevenue.Enabled = rbtnFixedRevenue.Checked && Milestone != null;

            FixedMilestoneDetails.Style["display"] = rbtnFixedRevenue.Checked ? "" : "none";
        }

        private int SaveData(MilestoneUpdateObject milestoneUpdateObj = null)
        {
            Milestone milestone = new Milestone();
            PopulateData(milestone);

            if (milestoneUpdateObj == null)
            {
                milestoneUpdateObj = new MilestoneUpdateObject()
                {
                    IsEndDateChangeReflectedForMilestoneAndPersons = null,
                    IsStartDateChangeReflectedForMilestoneAndPersons = null,
                    IsExtendedORCompleteOutOfRange = null
                };
            }

            Milestone m = new Milestone()
            {
                Id = milestone.Id,
                StartDate = milestone.StartDate,
                ProjectedDeliveryDate = milestone.ProjectedDeliveryDate,
                PremiumDiscount = milestone.PremiumDiscount
            };
            MilestoneObject = m;
            MilestoneUpdateObject = milestoneUpdateObj;

            using (MilestoneServiceClient serviceClient = new MilestoneServiceClient())
            {
                try
                {
                    IsAttributionPanelDisplayed = false;
                    MilestoneCSATAttributionCount = null;
                    return serviceClient.SaveMilestoneDetail(milestone, User.Identity.Name);
                }
                catch (FaultException<ExceptionDetail>)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        private void DeleteRecord()
        {
            var milestone = new Milestone { Id = MilestoneId };
            Person currentPerson = DataHelper.CurrentPerson;

            using (var serviceClient = new MilestoneServiceClient())
            {
                try
                {
                    serviceClient.DeleteMilestone(milestone, User.Identity.Name);
                    foreach (var person in Milestone.MilestonePersons)
                    {
                        if (person.Person.IsStrawMan)
                            continue;
                        ServiceCallers.Custom.Person(p => p.UpdateMSBadgeDetailsByPersonId(person.Person.Id.Value, currentPerson.Id.Value));
                    }
                }
                catch (FaultException<ExceptionDetail>)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        private void PopulateControls(Milestone milestone, bool fillComputedFinancials)
        {
            txtMilestoneName.Text = milestone.Description;
            dtpPeriodFrom.DateValue = milestone.StartDate;
            dtpPeriodTo.DateValue = milestone.ProjectedDeliveryDate;
            hdnPeriodFrom.Value = dtpPeriodFrom.TextValue;
            hdnPeriodTo.Value = dtpPeriodTo.TextValue;
            rbtnFixedRevenue.Checked = !milestone.IsHourlyAmount;
            rbtnHourlyRevenue.Checked = milestone.IsHourlyAmount;
            chbIsChargeable.Checked = milestone.IsChargeable;
            chbConsultantsCanAdjust.Checked = milestone.ConsultantsCanAdjust;
            ddlMilestoneType.SelectedValue = milestone.milestoneType.ToString();
            lnkbtnMonthlyRevenue.Visible = rbtnFixedRevenue.Checked;

            if (rbtnFixedRevenue.Checked)
            {
                txtFixedRevenue.Text = milestone.Amount != null ? milestone.Amount.Value.Value.ToString() : string.Empty;
                txtMilestoneDiscount.Text = lblMilestoneDiscount.Text = milestone.PremiumDiscount.HasValue ? milestone.PremiumDiscount.Value.ToString("###,###,###,###,##0.##") : string.Empty;
                ddlDiscountType.SelectedValue = DiscountType;
                if (DiscountType == "1")
                {
                    lblDoller.Attributes.Add("style", "display: inline");
                    lblPercentage.Attributes.Add("style", "display: none");
                }
                else
                {
                    lblDoller.Attributes.Add("style", "display: none");
                    lblPercentage.Attributes.Add("style", "display: inline");
                }
                chbLockFFAmount.Checked = milestone.IsAmountAtMilestone;
                ShowOrHideDiscount();


                decimal? blendedRate = null;

                blendedRate = milestone.ComputedFinancials != null ? milestone.ComputedFinancials.BillRate : null;

                lblBlendedRate.Text = blendedRate != null ? blendedRate.Value.ToString(CurrencyFormat) : "-";
            }

            PopulateProjectControls(milestone.Project);

            if (fillComputedFinancials)
                FillComputedFinancials(milestone);

            SetControlsChangebility();
        }


        private void ShowOrHideDiscount()
        {
            if (chbLockFFAmount.Checked)
            {
                txtMilestoneDiscount.Attributes.Add("style", "display: none");
                lblMilestoneDiscount.Attributes.Add("style", "display: inline");
                ddlDiscountType.Attributes.Add("style", "display: none");
            }
            else
            {
                txtMilestoneDiscount.Attributes.Add("style", "display: inline");
                lblMilestoneDiscount.Attributes.Add("style", "display: none");
                ddlDiscountType.Attributes.Add("style", "display: inline");
            }
        }

        public void FillComputedFinancials(Milestone milestone)
        {
            if (milestone == null)
                return;

            PersonListSeniorityAnalyzer = null;

            var variance = GetBudgetToProjectedVar(milestone);

            //Fill Total Margin Cell
            SetFooterLabelWithSeniority(milestone.ComputedFinancials == null ? null : (PracticeManagementCurrency?)milestone.ComputedFinancials.GrossMargin, lblTotalMargin);
            SetFooterLabelWithSeniority(milestone.BudgetFinancials == null ? null : (PracticeManagementCurrency?)milestone.BudgetFinancials.GrossMargin, lblBudgetGrossMargin);
            SetFooterLabelWithSeniority(variance == null ? null : (PracticeManagementCurrency?)variance.GrossMargin, lblVarcGrossMargin);


            //Fill Target Margin Cell
            SetFooterLabelWithSeniority(milestone.ComputedFinancials == null ? "0.00%" : string.Format(Constants.Formatting.PercentageFormat, milestone.ComputedFinancials.TargetMargin), lblTargetMargin);
            SetFooterLabelWithSeniority(milestone.BudgetFinancials == null ? "0.00%" : string.Format(Constants.Formatting.PercentageFormat, milestone.BudgetFinancials.TargetMargin), lblBudgetMarginPerc);
            SetFooterLabelWithSeniority(variance == null ? "0.00%" : string.Format(Constants.Formatting.PercentageFormat, variance.TargetMargin), lblVarcMarginPerc);

            if (milestone.Project.Client.Id.HasValue && milestone != null)
            {
                CMList = DataHelper.GetClientMarginColorInfo(milestone.Project.Client.Id.Value, Milestone.StartDate, Milestone.EndDate, SelectedProjectId.Value);

                if (milestone.ComputedFinancials != null)
                {
                    SetBackgroundColorForMargin(milestone.Project.Client.Id.Value, milestone.ComputedFinancials.TargetMargin, milestone.Project.Client.IsMarginColorInfoEnabled, lblTargetMargin, CMList);
                }

                if (milestone.BudgetFinancials != null)
                {
                    SetBackgroundColorForMargin(milestone.Project.Client.Id.Value, milestone.BudgetFinancials.TargetMargin, milestone.Project.Client.IsMarginColorInfoEnabled, lblBudgetMarginPerc, CMList);
                }

                if (variance != null)
                {
                    SetBackgroundColorForMargin(milestone.Project.Client.Id.Value, variance.TargetMargin, milestone.Project.Client.IsMarginColorInfoEnabled, lblVarcMarginPerc, CMList);
                }
            }
            if (Milestone.ComputedFinancials != null)
            {
                lblTotalCogs.Text = Milestone.ComputedFinancials.Cogs.ToString(PersonListSeniorityAnalyzer.GreaterSeniorityExists);
                lblTotalRevenue.Text = Milestone.ComputedFinancials.Revenue.ToString();
                lblTotalRevenueNet.Text = Milestone.ComputedFinancials.RevenueNet.ToString();

                var discountAmt = (Milestone.ComputedFinancials.Revenue.Value * Project.Discount / 100);
                var nfi = new NumberFormatInfo { CurrencyDecimalDigits = 0, CurrencySymbol = "$" };
                lblClientDiscountAmount.Text = discountAmt >= 0 ?
                    String.Format(nfi, "{0:c}", discountAmt) : ((PracticeManagementCurrency)discountAmt).ToString();

                lblClientDiscount.Text = Milestone.Project.Discount.ToString("##0.00");
                lblTotalExpenses.Text = PersonListSeniorityAnalyzer.GreaterSeniorityExists ? "(Hidden)" : (Milestone.ComputedFinancials.Cogs + Milestone.ComputedFinancials.Expenses).ToString();
            }

            if (Milestone.BudgetFinancials != null)
            {
                lblBudgetCogs.Text = Milestone.BudgetFinancials.Cogs.ToString(PersonListSeniorityAnalyzer.GreaterSeniorityExists);
                lblBudgetRevenue.Text = Milestone.BudgetFinancials.Revenue.ToString();
                lblBudgetRevenueNet.Text = Milestone.BudgetFinancials.RevenueNet.ToString();

                var nfi = new NumberFormatInfo { CurrencyDecimalDigits = 0, CurrencySymbol = "$" };
                var discountAmt = (Milestone.BudgetFinancials.Revenue.Value * Project.Discount / 100);
                lblBudgetDiscount.Text = discountAmt >= 0 ?
                    String.Format(nfi, "{0:c}", discountAmt) : ((PracticeManagementCurrency)discountAmt).ToString();

                lblClientDiscount.Text = Milestone.Project.Discount.ToString("##0.00");

                lblBudgetTotalExpenes.Text = PersonListSeniorityAnalyzer.GreaterSeniorityExists ? "(Hidden)" : (Milestone.BudgetFinancials.Cogs + Milestone.BudgetFinancials.Expenses).ToString();

            }

            if (variance != null)
            {
                lblVarcCogs.Text = variance.Cogs.ToString(PersonListSeniorityAnalyzer.GreaterSeniorityExists);
                lblVarcRevenue.Text = variance.Revenue.ToString();
                lblVarRevenueNet.Text = variance.RevenueNet.ToString();

                var nfi = new NumberFormatInfo { CurrencyDecimalDigits = 0, CurrencySymbol = "$" };
                var discountAmt = (variance.Revenue.Value * Project.Discount / 100);
                lblVarcDiscount.Text = discountAmt >= 0 ?
                    String.Format(nfi, "{0:c}", discountAmt) : ((PracticeManagementCurrency)discountAmt).ToString();

                lblClientDiscount.Text = Milestone.Project.Discount.ToString("##0.00");
                lblVarcTotalExpenses.Text = PersonListSeniorityAnalyzer.GreaterSeniorityExists ? "(Hidden)" : (variance.Cogs + variance.Expenses).ToString();
            }


            //Fill Expenses Cell
            SetFooterLabelWithSeniority(
                milestone.ComputedFinancials == null ? "-" :
                        ((PracticeManagementCurrency)milestone.ComputedFinancials.Expenses).ToString(),
                lblExpenses);


            SetFooterLabelWithSeniority(
                milestone.ComputedFinancials == null ? "-" :
                        ((PracticeManagementCurrency)milestone.ComputedFinancials.ReimbursedExpenses).ToString(),
                lblReimbursedExpenses);


            SetFooterLabelWithSeniority(
                milestone.BudgetFinancials == null ? "-" :
                        ((PracticeManagementCurrency)milestone.BudgetFinancials.Expenses).ToString(),
                lblBudgetExpense);


            SetFooterLabelWithSeniority(
                milestone.BudgetFinancials == null ? "-" :
                        ((PracticeManagementCurrency)milestone.BudgetFinancials.ReimbursedExpenses).ToString(),
                lblBudgetReimExpenses);

            SetFooterLabelWithSeniority(
                variance == null ? "-" :
                        ((PracticeManagementCurrency)variance.Expenses).ToString(),
                lblVarcExpenses);


            SetFooterLabelWithSeniority(
                variance == null ? "-" :
                        ((PracticeManagementCurrency)variance.ReimbursedExpenses).ToString(),
                lblVarcReimExpenses);
        }

        private List<ClientMarginColorInfo> CMList
        {
            get; set;
        }
        public ComputedFinancials GetBudgetToProjectedVar(Milestone milestone)
        {
            ComputedFinancials varc = null;
            var budget = milestone.BudgetFinancials;
            var projected = milestone.ComputedFinancials;
            if (budget == null && projected == null)
            {
                return varc;
            }
            if (budget == null && projected != null)
            {
                varc = new ComputedFinancials();
                varc.FinanceType = 6;//for EAC
                varc.Revenue = -1 * projected.Revenue;
                varc.RevenueNet = -1 * projected.RevenueNet;
                varc.Cogs = -1 * projected.Cogs;
                varc.Expenses = -1 * projected.Expenses;
                varc.ReimbursedExpenses = -1 * projected.ReimbursedExpenses;
                varc.GrossMargin = -1 * projected.GrossMargin;
            }
            else if (budget != null && projected == null)
            {
                varc = budget;
            }

            else
            {
                varc = new ComputedFinancials();
                varc.Revenue = budget.Revenue - projected.Revenue;
                varc.RevenueNet = Math.Round(budget.RevenueNet - projected.RevenueNet, 2, MidpointRounding.AwayFromZero);
                varc.Cogs = budget.Cogs - projected.Cogs;
                varc.Expenses = budget.Expenses - projected.Expenses;
                varc.ReimbursedExpenses = budget.ReimbursedExpenses - projected.ReimbursedExpenses;
                varc.GrossMargin = Math.Round(budget.GrossMargin - projected.GrossMargin, 2, MidpointRounding.AwayFromZero);
            }

            return varc;
        }

        private void SetBackgroundColorForMargin(int clientId, decimal targetMargin, bool? individualClientMarginColorInfoEnabled, Label marginLable, List<ClientMarginColorInfo> cmciList)
        {
            if (targetMargin != 0)
            {
                if (cmciList != null)
                {
                    foreach (var item in cmciList)
                    {
                        if (targetMargin >= item.StartRange - 1 && targetMargin < item.EndRange)
                        {
                            marginLable.Style["background-color"] = item.ColorInfo.ColorValue;
                            break;
                        }
                    }
                }
            }
        }

        private void SetControlsChangebility()
        {
            // Security
            var isReadOnly =
                !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.AdministratorRoleName) &&
                !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.OperationsRoleName) &&
                !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.SalespersonRoleName) &&
                !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.PracticeManagerRoleName) &&
                !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.BusinessUnitManagerRoleName) &&
                !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.DirectorRoleName) &&// #2817: DirectorRoleName is added as per the requirement.
                !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.SeniorLeadershipRoleName) &&// #2913: userIsSeniorLeadership is added as per the requirement.
                !Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.ProjectLead);// #2941: userIsProjectLead is added as per the requirement.

            txtMilestoneName.ReadOnly = dtpPeriodFrom.ReadOnly =
                dtpPeriodTo.ReadOnly = isReadOnly;

            txtFixedRevenue.ReadOnly = isReadOnly && (tierOneExceptionStatus == 1 || tierTwoExceptionStatus == 1);

            rbtnFixedRevenue.Enabled = rbtnHourlyRevenue.Enabled = !isReadOnly;
            btnSave.Visible = btnDelete.Visible = btnClone.Visible = btnMoveMilestone.Visible = (!isReadOnly && EnableEdit);
        }

        private void SetFooterLabelWithSeniority(string labelValue, ITextControl label)
        {
            label.Text =
                PersonListSeniorityAnalyzer != null && PersonListSeniorityAnalyzer.GreaterSeniorityExists ?
                    Resources.Controls.HiddenCellText : labelValue;
        }

        private void SetFooterLabelWithSeniority(PracticeManagementCurrency? value, Label label)
        {
            label.Text = value.HasValue ? value.Value.ToString(PersonListSeniorityAnalyzer != null && PersonListSeniorityAnalyzer.GreaterSeniorityExists) : "-";
        }

        private void PopulateProjectControls(Project project)
        {
            pdProjectInfo.Populate(project);

            if (project != null && gvPeople.FooterRow != null) lblClientDiscount.Text = project.Discount.ToString();
        }

        private void PopulateData(Milestone milestone)
        {
            milestone.Project = new Project { Id = SelectedProjectId };

            milestone.Id = MilestoneId;
            milestone.Description = txtMilestoneName.Text;
            milestone.StartDate = dtpPeriodFrom.DateValue;
            milestone.ProjectedDeliveryDate = dtpPeriodTo.DateValue;
            milestone.IsHourlyAmount = rbtnHourlyRevenue.Checked;
            milestone.Amount =
                rbtnFixedRevenue.Checked && txtFixedRevenue.Text != string.Empty ? (decimal?)decimal.Parse(txtFixedRevenue.Text) : null;
            milestone.IsChargeable = chbIsChargeable.Checked;
            milestone.ConsultantsCanAdjust = chbConsultantsCanAdjust.Checked;
            int type = 0;
            int.TryParse(ddlMilestoneType.SelectedValue, out type);
            milestone.milestoneType = type;
            if (IsDiscountAtMilestoneLevel == 0 && Milestone != null)
            {
                IsDiscountAtMilestoneLevel = Milestone.IsDiscountAtMilestoneLevel;
            }
            milestone.IsDiscountAtMilestoneLevel = IsDiscountAtMilestoneLevel;
            milestone.IsAmountAtMilestone = chbLockFFAmount.Checked;
            if (rbtnFixedRevenue.Checked)
            {
                milestone.PremiumDiscount = !string.IsNullOrEmpty(txtMilestoneDiscount.Text) ? (decimal?)decimal.Parse(txtMilestoneDiscount.Text) : null;
                int premiumType = 0;
                int.TryParse(ddlDiscountType.SelectedValue, out premiumType);
                milestone.PremiumType = premiumType;
            }
            else
            {
                milestone.PremiumDiscount = null;
                milestone.PremiumType = 0; //0 for hourly milestone
            }

        }

        private string MilestoneRedirrectUrl(int milestoneId)
        {
            var redirrectUrl = string.Format(
                Constants.ApplicationPages.MilestonePrevNextRedirectFormat,
                Constants.ApplicationPages.MilestoneDetail,
                milestoneId, Project.Id.Value);

            return Generic.GetTargetUrlWithReturn(redirrectUrl, Request.Url.AbsoluteUri);
        }

        protected void gvPeople_OnDataBound(object sender, EventArgs e)
        {
            gvPeople.FooterStyle.BackColor =
                gvPeople.Rows.Count % 2 == 0 ?
                    Color.White : Color.FromArgb(0xf9, 0xfa, 0xff);
        }

        protected void btnView_Command(object sender, CommandEventArgs e)
        {
            int viewIndex = int.Parse((string)e.CommandArgument);
            SelectView((Control)sender, viewIndex, false);

            LoadActiveTabIndex(viewIndex);
        }

        protected void odsMilestoneExpenses_OnSelecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (MilestoneId.HasValue)
            {
                e.InputParameters["milestoneId"] = MilestoneId.Value;
            }
        }

        protected void gvMilestoneExpenses_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            var row = e.Row;
            switch (row.RowType)
            {
                case DataControlRowType.DataRow:
                    var expense = row.DataItem as ProjectExpense;

                    if (expense != null)
                    {
                        _totalAmount += expense.Amount != null ? expense.Amount.Value : 0;
                        _totalReimbursed += expense.Reimbursement;
                        _totalReimbursementAmount += expense.ReimbursementAmount;

                        _expensesCount++;

                        // Hide rows with null values.
                        // These are special rows that are used not to show
                        //      empty data grid message
                        if (!expense.Id.HasValue)
                            row.Visible = false;
                    }

                    break;

                case DataControlRowType.Footer:
                    SetRowValue(row, LblTotalamount, _totalAmount);
                    SetRowValue(row, LblTotalreimbursement, string.Format("{0:0}%", (_totalReimbursed / _expensesCount)));
                    SetRowValue(row, LblTotalreimbursementamount, _totalReimbursementAmount);

                    break;
            }
        }

        private static void SetRowValue(Control row, string ctrlName, decimal number)
        {
            SetRowValue(row, ctrlName, ((PracticeManagementCurrency)number).ToString());
        }

        private static void SetRowValue(Control row, string ctrlName, string text)
        {
            var totalAmountCtrl = row.FindControl(ctrlName) as Label;
            if (totalAmountCtrl != null)
                totalAmountCtrl.Text = text;
        }

        private void LoadActiveTabIndex(int viewIndex)
        {
            switch (viewIndex)
            {
                case 0:
                    FillComputedFinancials(Milestone);
                    //PopulatePeopleGrid();
                    break;
                case 1: PopulatePeopleGrid(); break;
                case 7: activityLog.Update(); break;
            }
        }

        private void SelectView(Control sender, int viewIndex, bool selectOnly)
        {
            mvMilestoneDetailTab.ActiveViewIndex = viewIndex;

            foreach (TableCell cell in tblMilestoneDetailTabViewSwitch.Rows[0].Cells)
            {
                cell.CssClass = string.Empty;
            }

            ((WebControl)sender.Parent).CssClass = "SelectedSwitch";
        }

        private void SetDefaultValuesInPopup()
        {
            rbtnRemovePersonsStartDate.Checked = true;
            rbtnRemovePersonsEndDate.Checked = true;
            rbtnCancelStartDate.Checked = false;
            rbtnCancelEndDate.Checked = false;

            rbtnchangeMileStoneAndPersonsStartDate.Checked = true;
            rbtnchangeMileStoneAndPersonsEndDate.Checked = true;
            rbtnchangeMileStoneEndDate.Checked = false;
            rbtnchangeMileStoneStartDate.Checked = false;

            tblHasTimeentriesTowardsMileStone.Visible = false;
            tblchangeMilestonePersonsForStartDate.Visible = false;
            tblchangeMilestonePersonsForEndDate.Visible = false;
            tblRemoveMilestonePersonsForStartDate.Visible = false;
            tblRemoveMilestonePersonsForEndDate.Visible = false;
            hdnCanShowPopup.Value = "false";
            hdnCanShowMonthlyRevenue.Value = "false";
        }

        protected bool GetVisibleValue()
        {
            return MilestoneId.HasValue;
        }

        protected void vwResources_OnActivate(object sender, EventArgs e)
        {
            MilestonePersonEntryListControl.AddEmptyRow();
        }

        private void SetPopupMessageTextAndAssignHiddenFieldValues()
        {
            SetDefaultValuesInPopup();

            if (MilestoneId.HasValue && Milestone != null && Milestone.MilestonePersons.Count() > 0)
            {
                DateTime PeriodFrom = Convert.ToDateTime(hdnPeriodFrom.Value);
                DateTime PeriodTo = Convert.ToDateTime(hdnPeriodTo.Value);

                if (PeriodFrom.Date == dtpPeriodFrom.DateValue.Date && PeriodTo.Date == dtpPeriodTo.DateValue.Date)
                {
                    hdnCanShowPopup.Value = "false";
                }
                else
                {
                    try
                    {
                        bool hasTimeEntries = TimeEntryHelper.HasTimeEntriesForMilestone(MilestoneId.Value, dtpPeriodFrom.DateValue, dtpPeriodTo.DateValue);

                        if (hasTimeEntries == true)
                        {
                            tblHasTimeentriesTowardsMileStone.Visible = true;
                            trShowSaveandCancel.Visible = false;

                            hdnCanShowPopup.Value = "true";
                        }
                        else
                        {
                            hrBetweenCMSDandCMED.Visible = false;
                            hrBetweenCMEDandRMSD.Visible = false;
                            hrBetweenRMSDandRMED.Visible = false;

                            if (PeriodFrom > dtpPeriodFrom.DateValue)
                            {
                                if (PeriodFrom < dtpPeriodTo.DateValue)
                                {
                                    lblchangeMilestonePersonsPopupMessageForStartDate.Text = string.Format(changeMilestonePersonsPopupMessageForStartDate, dtpPeriodFrom.TextValue, PeriodFrom.ToString("MM/dd/yyyy"), dtpPeriodFrom.TextValue);
                                    tblchangeMilestonePersonsForStartDate.Visible = true;
                                    hdnCanShowPopup.Value = "true";
                                }
                            }

                            if (PeriodTo < dtpPeriodTo.DateValue)
                            {
                                if (PeriodTo > dtpPeriodFrom.DateValue)
                                {
                                    lblchangeMilestonePersonsForEndDate.Text = string.Format(changeMilestonePersonsPopupMessageForEndDate, dtpPeriodTo.TextValue, PeriodTo.ToString("MM/dd/yyyy"), dtpPeriodTo.TextValue);
                                    tblchangeMilestonePersonsForEndDate.Visible = true;
                                    hdnCanShowPopup.Value = "true";
                                }
                            }

                            if (PeriodFrom < dtpPeriodFrom.DateValue)
                            {
                                List<Person> personlist = new List<Person>();

                                personlist = Milestone.MilestonePersons.Where(mp => mp.Person.LastTerminationDate.HasValue && mp.Person.LastTerminationDate.Value < dtpPeriodFrom.DateValue).Select(mp => mp.Person).ToList();

                                if (personlist.Count > 0)
                                {
                                    string personDetail = string.Empty;
                                    foreach (var person in personlist)
                                    {
                                        personDetail += "<br/> <b>" + string.Format(format, person.FirstName, person.LastName, person.TerminationDate.Value.ToString("MM/dd/yyyy")) + "</b> <br/>";
                                    }
                                    lblRemoveMilestonePersonsForStartDate.Text = string.Format(RemoveMilestonePersonsPopupMessageForStartDate, dtpPeriodFrom.TextValue, personDetail);

                                    tblRemoveMilestonePersonsForStartDate.Visible = true;
                                    hdnCanShowPopup.Value = "true";
                                }
                            }

                            if (PeriodTo > dtpPeriodTo.DateValue)
                            {
                                List<Person> personlist = new List<Person>();

                                personlist = Milestone.MilestonePersons.Where(mp => mp.Person.FirstHireDate > dtpPeriodTo.DateValue).Select(mp => mp.Person).ToList();

                                if (personlist.Count > 0)
                                {
                                    string personDetail = string.Empty;
                                    foreach (var person in personlist)
                                    {
                                        personDetail += "<br/> <b>" + string.Format(format, person.FirstName, person.LastName, person.HireDate.ToString("MM/dd/yyyy")) + "</b> <br/>";
                                    }
                                    lblRemoveMilestonePersonsForEndDate.Text = string.Format(RemoveMilestonePersonsPopupMessageForEndDate, dtpPeriodTo.TextValue, personDetail);

                                    tblRemoveMilestonePersonsForEndDate.Visible = true;
                                    hdnCanShowPopup.Value = "true";
                                }
                            }

                            if (hdnCanShowPopup.Value == "true")
                            {
                                trShowSaveandCancel.Visible = true;
                            }

                            if (tblchangeMilestonePersonsForStartDate.Visible && (tblchangeMilestonePersonsForEndDate.Visible || tblRemoveMilestonePersonsForStartDate.Visible || tblRemoveMilestonePersonsForEndDate.Visible))
                            {
                                hrBetweenCMSDandCMED.Visible = true;
                            }

                            if (tblchangeMilestonePersonsForEndDate.Visible && (tblRemoveMilestonePersonsForStartDate.Visible || tblRemoveMilestonePersonsForEndDate.Visible))
                            {
                                hrBetweenCMEDandRMSD.Visible = true;
                            }

                            if (tblRemoveMilestonePersonsForStartDate.Visible && tblRemoveMilestonePersonsForEndDate.Visible)
                            {
                                hrBetweenRMSDandRMED.Visible = true;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        protected string GetDateFormat(DateTime date)
        {
            return date.ToString(Constants.Formatting.EntryDateFormat);
        }

        protected override object LoadPageStateFromPersistenceMedium()
        {
            System.Web.UI.PageStatePersister pageStatePersister1 = this.PageStatePersister;
            pageStatePersister1.Load();
            String vState = pageStatePersister1.ViewState.ToString();
            byte[] pBytes = System.Convert.FromBase64String(vState);
            pBytes = SettingsHelper.Decompress(pBytes);
            LosFormatter mFormat = new LosFormatter();
            Object ViewState = mFormat.Deserialize(System.Convert.ToBase64String(pBytes));
            return new Pair(pageStatePersister1.ControlState, ViewState);
        }

        protected override void SavePageStateToPersistenceMedium(Object pViewState)
        {
            Pair pair1;
            System.Web.UI.PageStatePersister pageStatePersister1 = this.PageStatePersister;
            Object ViewState;
            if (pViewState is Pair)
            {
                pair1 = ((Pair)pViewState);
                pageStatePersister1.ControlState = pair1.First;
                ViewState = pair1.Second;
            }
            else
            {
                ViewState = pViewState;
            }
            LosFormatter mFormat = new LosFormatter();
            StringWriter mWriter = new StringWriter();
            mFormat.Serialize(mWriter, ViewState);
            String mViewStateStr = mWriter.ToString();
            byte[] pBytes = System.Convert.FromBase64String(mViewStateStr);
            pBytes = SettingsHelper.Compress(pBytes);
            String vStateStr = System.Convert.ToBase64String(pBytes);
            pageStatePersister1.ViewState = vStateStr;
            pageStatePersister1.Save();
        }

        public void PopulateMonthlyRevenue(List<MonthlyRevenue> monthlyRevenues = null)
        {
            if (monthlyRevenues == null)
            {
                var startDate = dtpPeriodFrom.DateValue;
                var endDate = dtpPeriodTo.DateValue;
                int monthsCount = Math.Abs((endDate.Month - startDate.Month) + 12 * (endDate.Year - startDate.Year));
                if (monthsCount > 0)
                {
                    lblMilestoneName.Text = txtMilestoneName.Text;
                    lblPeriod.Text = startDate.Date.ToShortDateString() + "-" + endDate.Date.ToShortDateString();
                    decimal monthlyRevenue = Convert.ToDecimal(txtFixedRevenue.Text) / (monthsCount + 1);

                    List<MonthlyRevenue> list = new List<MonthlyRevenue>();

                    for (int i = 0; i <= monthsCount; i++)
                    {
                        MonthlyRevenue temp = new MonthlyRevenue();
                        temp.StartDate = i == 0 ? startDate : Utils.Calendar.MonthStartDate(startDate.AddMonths(i));
                        temp.EndDate = i == monthsCount ? endDate : Utils.Calendar.MonthEndDate(temp.StartDate);
                        temp.Amount = monthlyRevenue;

                        list.Add(temp);
                    }

                    repMonthlyRevenue.DataSource = list;
                    repMonthlyRevenue.DataBind();
                    mpeMonthlyRevenue.Show();
                }
            }
            else if (monthlyRevenues.Count > 0)
            {
                repMonthlyRevenue.DataSource = monthlyRevenues;
                repMonthlyRevenue.DataBind();
                mpeMonthlyRevenue.Show();
            }
            hdnCanShowMonthlyRevenue.Value = "true";

        }

        private void SaveMonthlyRevenue()
        {
            var revenues = new List<MonthlyRevenue>();

            CustomValidator cvRevenueSum = new CustomValidator();

            foreach (RepeaterItem item in repMonthlyRevenue.Items)
            {
                if (item.ItemType == ListItemType.Header)
                {
                    cvRevenueSum = item.FindControl("cvRevenueSum") as CustomValidator;
                }
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    var month = item.FindControl("lblMonth") as Label;
                    var monthRevenue = item.FindControl("txtMonthRevenue") as TextBox;

                    MonthlyRevenue temp = new MonthlyRevenue();

                    temp.Id = !string.IsNullOrEmpty(month.Attributes["RevenueId"]) ? (int?)Convert.ToInt32(month.Attributes["RevenueId"]) : null;
                    temp.StartDate = Convert.ToDateTime(month.Attributes["StartDate"]);
                    temp.EndDate = Convert.ToDateTime(month.Attributes["EndDate"]);
                    temp.Amount = Convert.ToDecimal(monthRevenue.Text);
                    temp.MilestoneId = !string.IsNullOrEmpty(month.Attributes["MilestoneId"]) ? (int?)Convert.ToInt32(month.Attributes["MilestoneId"]) : null;
                    revenues.Add(temp);
                }
            }
            var sum = revenues.Sum(r => r.Amount);
            var revenue = Convert.ToDecimal(txtFixedRevenue.Text);
            cvRevenueSum.IsValid = (sum == revenue) || (Math.Abs(sum - revenue) < 1);
            if (cvRevenueSum.IsValid)
            {
                lblSumError.Visible = false;
                if (revenues.Count() > 0)
                {
                    ServiceCallers.Custom.Milestone(m => m.SaveMonthlyRevenueForFixedMilestone(MilestoneId.Value, revenues.ToArray()));
                }
            }
            else
            {
                lblSumError.Visible = true;
                mpeMonthlyRevenue.Show();
            }




        }

        #region Implementation of IPostBackEventHandler

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String"/> that represents an optional event argument
        /// to be passed to the event handler. </param>
        public void RaisePostBackEvent(string eventArgument)
        {
            SaveAndRedirect(eventArgument);
        }



        #endregion Implementation of IPostBackEventHandler

        protected void btnSaveMonthlyRevenue_Click(object sender, EventArgs e)
        {
            Page.Validate("MonthlyRevenue");
            if (Page.IsValid)
            {
                saveMonthlyRevenue = true;
                if (hdnUpdateOnlyMonthlyRevenue.Value == "true")
                {
                    SaveMonthlyRevenue();
                }
                else
                {
                    SaveMilestoneData();
                }
            }
            else
            {
                mpeMonthlyRevenue.Show();
            }
            UpnlBody.Update();
        }

        protected void lnkMonthlyRevenue_Click(object sender, EventArgs e)
        {
            if (MilestoneId.HasValue && Milestone != null && !Milestone.IsHourlyAmount)
            {
                var monthlyData = ServiceCallers.Custom.Milestone(m => m.GetMonthlyRevenuesForMilestone(MilestoneId.Value));
                if (monthlyData.Count() > 0)
                {
                    var startDate = dtpPeriodFrom.DateValue;
                    var endDate = dtpPeriodTo.DateValue;
                    lblMilestoneName.Text = txtMilestoneName.Text;
                    lblPeriod.Text = startDate.Date.ToShortDateString() + "-" + endDate.Date.ToShortDateString();
                    hdnUpdateOnlyMonthlyRevenue.Value = "true";
                    repMonthlyRevenue.DataSource = monthlyData;
                    repMonthlyRevenue.DataBind();
                    mpeMonthlyRevenue.Show();
                }
            }
        }

        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {
            DataHelper.InsertExportActivityLogMessage("Milestone Forecast worksheet");
            List<SheetStyles> sheetStylesList = new List<SheetStyles>();
            var dataSetList = new List<DataSet>();

            var ReportData = ServiceCallers.Custom.Milestone(m => m.GetForecastDataForMilestone(MilestoneId.Value)).ToList();

            var fileName = string.Format("MilestoneForecast.xls");

            if (ReportData.Count > 0)
            {
                DataTable header1 = new DataTable();
                header1.Columns.Add(string.Format("Milestone Resource Forecast Worksheet"));

                List<object> row1 = new List<object>();
                row1.Add(Milestone.Description + " (" + Milestone.StartDate.ToString("MM/dd/yyy") + " - " + Milestone.EndDate.ToString("MM/dd/yyy") + ")");
                header1.Rows.Add(row1.ToArray());
                headerRowsCount = header1.Rows.Count + 3;

                var data = PrepareDataTable(ReportData);
                coloumnsCount = data.Columns.Count;
                sheetStylesList.Add(HeaderSheetStyle);
                DataStyles.IsAutoResize = true;
                sheetStylesList.Add(DataStyles);
                var dataset = new DataSet();
                dataset.DataSetName = "Milestone Forecast worksheet";
                dataset.Tables.Add(header1);
                dataset.Tables.Add(data);
                dataSetList.Add(dataset);
            }
            else
            {
                string dateRangeTitle = "There are no Records the selected parameters.";
                DataTable header = new DataTable();
                header.Columns.Add(dateRangeTitle);
                sheetStylesList.Add(HeaderSheetStyle);
                var dataset = new DataSet();
                dataset.DataSetName = "Milestone Forecast worksheet";
                dataset.Tables.Add(header);
                dataSetList.Add(dataset);
            }
            NPOIExcel.Export(fileName, dataSetList, sheetStylesList);
        }

        public DataTable PrepareDataTable(List<PersonBudgetComparison> report)
        {
            var dataStyle = new SheetStyles(new RowStyles[0]);
            var rowStylesList = dataStyle.rowStyles.ToList();

            DataTable data = new DataTable();

            int numberOfPeriods = 0;
            int rowCount = 5;

            foreach (var repData in report)
            {
                if (repData.ProjectedAndActualBillRate != null)
                {
                    numberOfPeriods = repData.ProjectedAndActualBillRate.Count > numberOfPeriods ? repData.ProjectedAndActualBillRate.Count : numberOfPeriods;
                }
            }

            int monthsInPeriod = Math.Abs((Milestone.StartDate.Month - Milestone.EndDate.Month) + 12 * (Milestone.StartDate.Year - Milestone.EndDate.Year)) + 1;


            var row = new List<object>();
            DateTime periodStart = Utils.Calendar.MonthStartDate(Milestone.StartDate);

            var dataCellStyle = new CellStyles();
            dataCellStyle.WrapText = true;

            CellStyles dataDateCellStyle = new CellStyles();
            dataDateCellStyle.DataFormat = "mm/dd/yy;@";

            CellStyles dataCurrencyStyle = new CellStyles();
            dataCurrencyStyle.DataFormat = "$#,##0.00_);($#,##0.00)";

            CellStyles dataPercentageStyle = new CellStyles();
            dataPercentageStyle.DataFormat = "0%";

            CellStyles headerWrapCellStyle = new CellStyles();
            headerWrapCellStyle.IsBold = true;
            headerWrapCellStyle.HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            headerWrapCellStyle.WrapText = true;

            var dataTotalHoursCellStyle = new CellStyles();
            dataTotalHoursCellStyle.WrapText = true;

            List<CellStyles> headerCellStyleList = new List<CellStyles>();
            headerCellStyleList.Add(headerWrapCellStyle);
            var headerrowStyle = new RowStyles(headerCellStyleList.ToArray());
            rowStylesList.Add(headerrowStyle);

            List<string> alpha = new List<string> { "A","B","C","D","E","F","G","H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "AA","AB","AC","AD","AE","AF","AG","AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ",
            "BA","BB","BC","BD","BE","BF","BG","BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ",
            "CA","CB","CC","CD","CE","CF","CG","CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ"};

            int monthStartCol = 1 + (numberOfPeriods * 3);
            int monthEndcol = (numberOfPeriods * 3) + monthsInPeriod;

            string monthStartColumnAlpha = alpha[monthStartCol];
            string monthEndColumnAlpha = alpha[monthEndcol];
            string revenueColumn = alpha[monthEndcol + 2];
            string costColumn = alpha[monthEndcol + 3];
            string marginColumn = alpha[monthEndcol + 4];

            int numberOfColumns = (numberOfPeriods * 3) + monthsInPeriod + 6;

            var dataEmptyRowStyle = new List<CellStyles>();



            data.Columns.Add("Resource");

            for (int i = 1; i <= numberOfPeriods; i++)
            {
                data.Columns.Add("Period-" + i.ToString() + " Start Date");
                data.Columns.Add("BillRate/Hr (Period-" + i.ToString() + ")");
                data.Columns.Add("Cost/Hr (Period-" + i.ToString() + ")");
            }

            for (int k = 0; k < monthsInPeriod; k++)
            {
                data.Columns.Add(periodStart.ToString(Constants.Formatting.CompPerfMonthYearFormat));

                periodStart = periodStart.AddMonths(1);
            }

            data.Columns.Add("Total Hours");
            data.Columns.Add("Revenue");
            data.Columns.Add("Cost");
            data.Columns.Add("Margin");
            data.Columns.Add("Margin %");



            var totalHours = new Dictionary<DateTime, decimal>();
            decimal totalServiceHours = 0, totalRevenue = 0, totalMargin = 0, totalPersonCost = 0;

            foreach (var personData in report)
            {

                var dataCellStylearray = new List<CellStyles>() { dataCellStyle };

                DateTime monthBegin = Utils.Calendar.MonthStartDate(Milestone.StartDate);

                row = new List<object>();
                row.Add(personData.Person.LastName + ", " + personData.Person.FirstName + (personData.Person.Title != null ? "(" + personData.Person.Title.TitleName + ")" : ""));
                int tempPeriodCount = 0;


                if (personData.ProjectedAndActualBillRate != null)
                {
                    tempPeriodCount = personData.ProjectedAndActualBillRate.Count;
                    foreach (var billRates in personData.ProjectedAndActualBillRate)
                    {
                        row.Add(billRates.Key.ToString("MM/dd/yyy"));
                        row.Add(billRates.Value.BillRate.Value);
                        row.Add(billRates.Value.PersonCost.Value);

                        dataCellStylearray.Add(dataDateCellStyle);
                        dataCellStylearray.Add(dataCurrencyStyle);
                        dataCellStylearray.Add(dataCurrencyStyle);
                    }
                }

                for (int i = 0; i < numberOfPeriods - tempPeriodCount; i++)
                {
                    row.Add(string.Empty);
                    row.Add(string.Empty);
                    row.Add(string.Empty);
                    dataCellStylearray.Add(dataDateCellStyle);
                    dataCellStylearray.Add(dataCurrencyStyle);
                    dataCellStylearray.Add(dataCurrencyStyle);
                }

                var colValue = -1M;

                for (int k = 0; k < monthsInPeriod; k++, monthBegin = monthBegin.AddMonths(1))
                {
                    DateTime monthEnd = Utils.Calendar.MonthEndDate(monthBegin);
                    colValue = -1M;

                    if (personData.ProjectedHours != null)
                    {
                        foreach (KeyValuePair<DateTime, decimal> interestValue in personData.ProjectedHours)
                        {
                            if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                            {
                                colValue = interestValue.Value;
                                decimal temp = colValue;
                                if (totalHours.Any(t => t.Key == interestValue.Key))
                                {
                                    temp += totalHours.FirstOrDefault(t => t.Key == interestValue.Key).Value;
                                    totalHours.Remove(interestValue.Key);
                                    totalHours.Add(interestValue.Key, temp);
                                }
                                else
                                {
                                    totalHours.Add(interestValue.Key, colValue);
                                }
                            }
                        }
                    }
                    row.Add(colValue > -1 ? colValue.ToString() : (object)"-");
                    dataCellStylearray.Add(dataCellStyle);
                }


                totalServiceHours += personData.TotalProjectedHours;
                totalRevenue += personData.ProjectedRevenue.Value;
                totalPersonCost += personData.ProjectedCost.Value;
                totalMargin += personData.ProjectedMargin.Value;
                row.Add(personData.TotalProjectedHours);

                row.Add(personData.ProjectedRevenue.Value);
                row.Add(personData.ProjectedCost.Value);
                row.Add(personData.ProjectedMargin.Value);
                row.Add(personData.ProjectedRevenue != 0 ? personData.ProjectedMargin.Value / personData.ProjectedRevenue.Value : 0);

                dataTotalHoursCellStyle = new CellStyles();
                dataTotalHoursCellStyle.WrapText = true;
                dataTotalHoursCellStyle.CellFormula = string.Format("SUM({0}{1}: {2}{3})", monthStartColumnAlpha, rowCount, monthEndColumnAlpha, rowCount);
                dataCellStylearray.Add(dataTotalHoursCellStyle);

                //if (monthsInPeriod != personData.ProjectedAndActualBillRate.Count)
                //{
                var _billRates = personData.ProjectedAndActualBillRate.OrderBy(b => b.Key);
                var projectedHours = personData.ProjectedHours.OrderBy(p => p.Key);
                string equation = "";
                string costEquation = "";
                int j = 0;

                monthBegin = Utils.Calendar.MonthStartDate(Milestone.StartDate);
                for (int k = 0; k < monthsInPeriod; k++, monthBegin = monthBegin.AddMonths(1))
                {
                    DateTime monthEnd = Utils.Calendar.MonthEndDate(monthBegin);
                    colValue = -1M;

                    if (personData.ProjectedHours != null)
                    {
                        foreach (KeyValuePair<DateTime, decimal> interestValue in personData.ProjectedHours)
                        {
                            if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                            {
                                //string monthCol = alpha[monthStartCol + j];
                                var b = _billRates.ToList();
                                KeyValuePair<DateTime, PayRate> last = new KeyValuePair<DateTime, PayRate>(new DateTime(2029, 01, 01), null);
                                b.Add(last);
                                for (int i = 0; i < b.Count - 1; i++)
                                {
                                    var first = new DateTime(b[i].Key.Year, b[i].Key.Month, 01);
                                    var second = new DateTime(b[i + 1].Key.Year, b[i + 1].Key.Month, 01);
                                    if (first <= interestValue.Key && second > interestValue.Key)
                                    {
                                        if (j != 0)
                                        {
                                            equation += "+";
                                            costEquation += "+";
                                        }
                                        equation += string.Format("{0}{1}*{2}{3}", alpha[2 + (i * 3)], rowCount, alpha[monthStartCol + j], rowCount);
                                        costEquation += string.Format("{0}{1}*{2}{3}", alpha[3 + (i * 3)], rowCount, alpha[monthStartCol + j], rowCount);
                                    }
                                }

                                break;
                            }

                        }
                    }
                    j++;
                }

                var dataCurrencyRevStyle = new CellStyles();
                dataCurrencyRevStyle.DataFormat = "$#,##0_);($#,##0)";
                dataCurrencyRevStyle.CellFormula = equation;
                dataCellStylearray.Add(dataCurrencyRevStyle);

                var dataCurrencyRevCostStyle = new CellStyles();
                dataCurrencyRevCostStyle.DataFormat = "$#,##0_);($#,##0)";
                dataCurrencyRevCostStyle.CellFormula = costEquation;
                dataCellStylearray.Add(dataCurrencyRevCostStyle);

                //dataCellStylearray.Add(dataCurrencyRevCostStyle);

                var dataCurrencyMarginStyle = new CellStyles();
                dataCurrencyMarginStyle.DataFormat = "$#,##0_);($#,##0)";
                dataCurrencyMarginStyle.CellFormula = string.Format("{0}{1}-{2}{3}", revenueColumn, rowCount, costColumn, rowCount);
                dataCellStylearray.Add(dataCurrencyMarginStyle);

                CellStyles dataMarginPercentageStyle = new CellStyles();
                dataMarginPercentageStyle.DataFormat = "0%";
                dataMarginPercentageStyle.CellFormula = string.Format("{0}{1}/{2}{3}", marginColumn, rowCount, revenueColumn, rowCount);
                dataCellStylearray.Add(dataMarginPercentageStyle);

                data.Rows.Add(row.ToArray());
                var datarowStyle = new RowStyles(dataCellStylearray.ToArray());
                rowStylesList.Add(datarowStyle);
                rowCount++;
            }

            row = new List<object>();
            var dataTotalStylearray = new List<CellStyles>() { dataCellStyle };
            row.Add(string.Format(NPOIExcel.CustomColorWithBoldKey, "black", "Services Total"));
            for (int i = 0; i < numberOfPeriods; i++)
            {
                row.Add(string.Empty);
                row.Add(string.Empty);
                row.Add(string.Empty);
                dataTotalStylearray.Add(dataDateCellStyle);
                dataTotalStylearray.Add(dataCurrencyStyle);
                dataTotalStylearray.Add(dataCurrencyStyle);
            }

            if (totalHours != null)
            {
                var colValue = -1M;
                DateTime monthBegin = Utils.Calendar.MonthStartDate(Milestone.StartDate);
                for (int k = 0; k < monthsInPeriod; k++, monthBegin = monthBegin.AddMonths(1))
                {
                    DateTime monthEnd = Utils.Calendar.MonthEndDate(monthBegin);
                    foreach (KeyValuePair<DateTime, decimal> interestValue in totalHours)
                    {
                        if (IsInMonth(interestValue.Key, monthBegin, monthEnd))
                        {
                            colValue = interestValue.Value;
                        }
                    }
                    row.Add(colValue > -1 ? colValue.ToString() : (object)"-");
                    dataTotalStylearray.Add(dataCellStyle);
                }
            }

            row.Add(totalServiceHours);
            row.Add(totalRevenue);
            row.Add(totalPersonCost);
            row.Add(totalMargin);
            row.Add(totalRevenue != 0M ? totalMargin / totalRevenue : 0);

            dataTotalHoursCellStyle = new CellStyles();
            dataTotalHoursCellStyle.WrapText = true;
            dataTotalHoursCellStyle.CellFormula = string.Format("SUM({0}{1}: {2}{3})", monthStartColumnAlpha, rowCount, monthEndColumnAlpha, rowCount);
            dataTotalStylearray.Add(dataTotalHoursCellStyle);

            var dataTotalRevenueStyle = new CellStyles();
            dataTotalRevenueStyle.DataFormat = "$#,##0_);($#,##0)";
            dataTotalRevenueStyle.CellFormula = string.Format("SUM({0}{1}: {2}{3})", revenueColumn, 5, revenueColumn, rowCount - 1);
            dataTotalStylearray.Add(dataTotalRevenueStyle);

            dataTotalRevenueStyle = new CellStyles();
            dataTotalRevenueStyle.DataFormat = "$#,##0_);($#,##0)";
            dataTotalRevenueStyle.CellFormula = string.Format("SUM({0}{1}: {2}{3})", costColumn, 5, costColumn, rowCount - 1);
            dataTotalStylearray.Add(dataTotalRevenueStyle);


            var dataTotalMarginStyle = new CellStyles();
            dataTotalMarginStyle.DataFormat = "$#,##0_);($#,##0)";
            dataTotalMarginStyle.CellFormula = string.Format("{0}{1}-{2}{3}", revenueColumn, rowCount, costColumn, rowCount);
            dataTotalStylearray.Add(dataTotalMarginStyle);

            CellStyles dataTotalMarginPercentageStyle = new CellStyles();
            dataTotalMarginPercentageStyle.DataFormat = "0%";
            dataTotalMarginPercentageStyle.CellFormula = string.Format("{0}{1}/{2}{3}", marginColumn, rowCount, revenueColumn, rowCount);
            dataTotalStylearray.Add(dataTotalMarginPercentageStyle);

            rowStylesList.Add(new RowStyles(dataTotalStylearray.ToArray()));
            data.Rows.Add(row.ToArray());

            DataStyles = new SheetStyles(rowStylesList.ToArray());
            DataStyles.TopRowNo = headerRowsCount;
            return data;
        }

        private static bool IsInMonth(DateTime date, DateTime periodStart, DateTime periodEnd)
        {
            var result =
                (date.Year > periodStart.Year ||
                (date.Year == periodStart.Year && date.Month >= periodStart.Month)) &&
                (date.Year < periodEnd.Year || (date.Year == periodEnd.Year && date.Month <= periodEnd.Month));

            return result;
        }

        protected void btnCancelMonthlyRevenue_Click(object sender, EventArgs e)
        {
            SetDefaultValuesInPopup();
            //if (MilestoneId.HasValue && Milestone != null)
            //{
            //    Project = Milestone.Project;
            //    PopulateControls(Milestone, true);
            //    lblResult.ClearMessage();
            //    ClearDirty();
            //    hdnCanShowPopup.Value = "false";
            //}

            LoadActiveTabIndex(mvMilestoneDetailTab.ActiveViewIndex);
        }
    }

}

