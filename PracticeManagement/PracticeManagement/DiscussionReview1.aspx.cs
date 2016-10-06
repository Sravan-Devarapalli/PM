﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Web.Security;
using System.Web.UI.WebControls;
using DataTransferObjects;
using PraticeManagement.Controls;
using PraticeManagement.OpportunityService;
using PraticeManagement.ProjectService;
using PraticeManagement.Utils;
using System.Web.UI;
using System.Linq;
using System.Collections;
using System.Data;
using DataTransferObjects.ContextObjects;
using System.Drawing;
using System.Web.UI.HtmlControls;
using PraticeManagement.Controls.Opportunities;

namespace PraticeManagement
{
    public partial class DiscussionReview1 : PracticeManagementPageBase, IPostBackEventHandler
    {
        #region Events

        public event EventHandler NoteAdded;

        #endregion

        #region Fields

        private bool _userIsAdministrator;
        private bool _userIsRecruiter;
        private bool _userIsSalesperson;
        private bool _userIsHR;

        #endregion

        #region Properties

        ////private Project selectedProjectValue;
        private const int WonProjectId = 4;
        private const string OPPORTUNITY_KEY = "OPPORTUNITY_KEY";
        private const string OPPORTUNITIES_LIST_KEY = "OPPORTUNITIES_LIST_KEY";
        private const string PreviousReportContext_Key = "PREVIOUSREPORTCONTEXT_KEY";
        private const string DistinctPotentialBoldPersons_Key = "DISTINCTPOTENTIALBOLDPERSONS_KEY";
        private const string EstRevenueFormat = "Est. Revenue - {0}";
        private const string WordBreak = "<wbr />";


        private const string ANIMATION_SHOW_SCRIPT =
                       @"<OnClick>
                        	<Sequence>
                        		<Parallel Duration=""0"" AnimationTarget=""{0}"">
                        			<Discrete Property=""style"" propertyKey=""border"" ValuesScript=""['thin solid navy']""/>
                        		</Parallel>
                        		<Parallel Duration="".4"" Fps=""20"" AnimationTarget=""{0}"">
                        			<Resize  Width=""260"" Height=""{1}"" Unit=""px"" />
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

        /// <summary>
        /// 	Gets a selected opportunity
        /// </summary>
        private Opportunity Opportunity
        {
            get
            {
                if (ViewState[OPPORTUNITY_KEY] != null && OpportunityId.HasValue)
                {
                    if ((ViewState[OPPORTUNITY_KEY] as Opportunity).Id == OpportunityId)
                    {
                        return ViewState[OPPORTUNITY_KEY] as Opportunity;
                    }
                }

                if (OpportunityId.HasValue)
                {
                    using (var serviceClient = new OpportunityServiceClient())
                    {
                        try
                        {
                            var currentOpportunity = serviceClient.GetById(OpportunityId.Value);
                            ViewState[OPPORTUNITY_KEY] = currentOpportunity;
                            return currentOpportunity;
                        }
                        catch (CommunicationException)
                        {
                            serviceClient.Abort();
                            throw;
                        }
                    }
                }
                return null;
            }
        }

        private Opportunity[] OpportunitiesList
        {
            get
            {
                if (ViewState[OPPORTUNITIES_LIST_KEY] != null)
                {
                    return ViewState[OPPORTUNITIES_LIST_KEY] as Opportunity[];
                }

                else
                {
                    var result = ServiceCallers.Custom.Opportunity(c => c.OpportunityListAllShort(new OpportunityListContext { ActiveClientsOnly = true }));
                    ViewState[OPPORTUNITIES_LIST_KEY] = result;
                    return result;
                }

            }
        }

        private int? OpportunityId
        {
            get
            {
                int id;
                if (Int32.TryParse(hdnOpportunityId.Value, out id))
                {
                    return id;
                }

                return null;
            }
            set
            {
                hdnOpportunityId.Value = value.ToString();
            }
        }

        private bool HasProposedPersonsOfTypeNormal
        {
            get
            {
                return ucProposedResources.HasProposedPersonsOfTypeNormal;
            }
        }

        #endregion

        #region Methods

        #region Page Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState.Remove(OPPORTUNITY_KEY);
                ViewState.Remove(OPPORTUNITIES_LIST_KEY);
            }

            // Security
            InitSecurity();

            BindOpportunitiesData();

            if (!IsPostBack)
            {
                DataHelper.FillClientList(ddlClient, string.Empty);
                DataHelper.FillSalespersonListOnlyActive(ddlSalesperson, string.Empty);
                DataHelper.FillOpportunityStatusList(ddlStatus, string.Empty);
                DataHelper.FillPracticeListOnlyActive(ddlPractice, string.Empty);
                DataHelper.FillOpportunityPrioritiesList(ddlPriority, string.Empty);
                DataHelper.FillOwnersList(ddlOpportunityOwner, "-- Select Owner --");
                PopulatePriorityHint();

                LoadOpportunityDetails();
            }

            mlConfirmation.ClearMessage();

            if (hdnValueChanged.Value == "false")
            {
                btnSave.Attributes.Add("disabled", "true");
            }
            else
            {
                btnSave.Attributes.Remove("disabled");
            }

            lblSaved.Text = string.Empty;
            lblError.Text = string.Empty;
            divResultDescription.Style["display"] = "none";

            if (tcOpportunityDetails.ActiveTabIndex == 1)
            {
                LoadActivityLogControl();
            }
        }

        private void LoadActivityLogControl()
        {
            phActivityLog.Controls.Clear();
            ActivityLogControl activityLog = LoadControl("~/Controls/ActivityLogControl.ascx") as ActivityLogControl;
            activityLog.ID = "activityLog";
            activityLog.EnableViewState = true;
            activityLog.IsFreshRequest = true;
            activityLog.OpportunityId = OpportunityId;
            activityLog.DisplayDropDownValue = ActivityEventSource.Opportunity;
            activityLog.FromDateFilterValue = SettingsHelper.GetCurrentPMTime().AddYears(-1);
            activityLog.ToDateFilterValue = SettingsHelper.GetCurrentPMTime();
            activityLog.ShowDisplayDropDown = false;
            activityLog.ShowProjectDropDown = false;
            phActivityLog.Controls.Add(activityLog);

        }

        private void FillGroupAndProjectDropDown()
        {
            if (OpportunityId.HasValue)
            {
                var groups = ServiceCallers.Custom.Group(client => client.GroupListAll(Opportunity.Client.Id, null));
                groups = groups.AsQueryable().Where(g => (g.IsActive == true)).ToArray();
                DataHelper.FillListDefault(ddlClientGroup, string.Empty, groups, false);

                var projects = ServiceCallers.Custom.Project(client => client.ListProjectsByClientShort(Opportunity.Client.Id, true, false, false));
                DataHelper.FillListDefault(ddlProjects, "Select Project ...", projects, false, "Id", "DetailedProjectTitle");
                if (ddlProjects.Items.Count == 1)
                {
                    ddlProjects.Items[0].Enabled = true;
                }
                upAttachToProject.Update();
                upOpportunityDetail.Update();
            }
        }

        protected void Page_Prerender(object sender, EventArgs e)
        {
            if (ddlClientGroup.Items.Count > 0)
                ddlClientGroup.SortByText();

            animHide.Animations = GetAnimationsScriptForAnimHide();
            animShow.Animations = GetAnimationsScriptForAnimShow();

            if (lvOpportunities.SelectedIndex <= 0)
            {
                imgBtnFirst.Visible = false;
                imgBtnPrevious.Visible = false;
                imgFirst.Visible = true;
                imgPrevious.Visible = true;

                imgBtnNext.Visible = true;
                imgBtnLast.Visible = true;
                imgNext.Visible = false;
                imgLast.Visible = false;
            }
            else if (lvOpportunities.SelectedIndex >= lvOpportunities.Items.Count - 1)
            {
                imgBtnFirst.Visible = true;
                imgBtnPrevious.Visible = true;
                imgFirst.Visible = false;
                imgPrevious.Visible = false;

                imgBtnNext.Visible = false;
                imgBtnLast.Visible = false;
                imgNext.Visible = true;
                imgLast.Visible = true;
            }
            else
            {
                imgBtnFirst.Visible = true;
                imgBtnPrevious.Visible = true;
                imgFirst.Visible = false;
                imgPrevious.Visible = false;

                imgBtnNext.Visible = true;
                imgBtnLast.Visible = true;
                imgNext.Visible = false;
                imgLast.Visible = false;
            }
            lblTotalOpportunities.Text = lvOpportunities.Items.Count.ToString();
            lblCurrentOpportunity.Text = Convert.ToString(lvOpportunities.SelectedIndex + 1);
            lvOpportunities.DataBind();

            upTopBarPane.Update();
            UpdatePanel1.Update();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "EnableOrDisableConvertOrAttachToProj", "EnableOrDisableConvertOrAttachToProj();", true);
        }

        #endregion

        #region topandleft

        public void imgBtnPrevious_OnClick(object sender, EventArgs e)
        {
            if (!CheckForDirtyBehaviour())
            {
                return;
            }
            lvOpportunities.SelectedIndex = lvOpportunities.SelectedIndex - 1;


            if (IsPostBack && Page.IsValid)
            {
                LoadOpportunityDetails();
            }

            FocusToSelectedItem();
            clearActivityLogControls();


        }

        public void imgBtnNext_OnClick(object sender, EventArgs e)
        {
            if (!CheckForDirtyBehaviour())
            {
                return;
            }
            lvOpportunities.SelectedIndex = lvOpportunities.SelectedIndex + 1;

            if (IsPostBack && Page.IsValid)
            {
                LoadOpportunityDetails();
            }

            FocusToSelectedItem();
            clearActivityLogControls();

        }

        public void lvOpportunities_SelectedIndexChanging(object sender, ListViewSelectEventArgs e)
        {
            if (!CheckForDirtyBehaviour())
            {
                return;
            }
            lvOpportunities.SelectedIndex = e.NewSelectedIndex;

            if (IsPostBack && Page.IsValid)
            {
                LoadOpportunityDetails();
            }
            SetScrollPosition();

            clearActivityLogControls();
        }

        public void imgBtnFirst_OnClick(object sender, EventArgs e)
        {
            if (!CheckForDirtyBehaviour())
            {
                return;
            }

            if (lvOpportunities.Items.Count > 0)
                lvOpportunities.SelectedIndex = 0;

            if (IsPostBack && Page.IsValid)
            {
                LoadOpportunityDetails();
            }
            FocusToSelectedItem();
            clearActivityLogControls();

        }

        public void imgBtnLast_OnClick(object sender, EventArgs e)
        {
            if (!CheckForDirtyBehaviour())
            {
                return;
            }

            if (lvOpportunities.Items.Count > 0)
                lvOpportunities.SelectedIndex = lvOpportunities.Items.Count - 1;

            if (IsPostBack && Page.IsValid)
            {
                LoadOpportunityDetails();
            }
            FocusToSelectedItem();
            clearActivityLogControls();
        }

        #endregion

        private void clearActivityLogControls()
        {
            phActivityLog.Controls.Clear();
            upActivityLog.Update();
        }



        public void LoadOpportunityDetails()
        {
            lvOpportunities.DataBind();
            hdnOpportunityId.Value = lvOpportunities.SelectedValue.ToString();
            ucProposedResources.OpportunityId = OpportunityId;
            FillControls();
            BindNotesData();
            PopulateProposedResources();
        }

        private void PopulateProposedResources()
        {
            ucProposedResources.FillProposedResources();
            ucProposedResources.FillPotentialResources();
            upProposedResources.Update();
        }

        private void BindOpportunitiesData()
        {
            lvOpportunities.DataSource = OpportunitiesList;
            upOpportunityList.Update();
        }

        private void BindNotesData()
        {
            var notes = ServiceCallers.Custom.Milestone(c => c.NoteListByTargetId(Convert.ToInt32(OpportunityId), 4));
            lvNotes.DataSource = notes;
            lvNotes.DataBind();
            upNotes.Update();
        }

        private void InitSecurity()
        {
            var roles = new List<string>(Roles.GetRolesForUser());

            _userIsAdministrator =
                roles.Contains(DataTransferObjects.Constants.RoleNames.AdministratorRoleName);
            roles.Contains(DataTransferObjects.Constants.RoleNames.PracticeManagerRoleName);
            _userIsSalesperson =
                roles.Contains(DataTransferObjects.Constants.RoleNames.SalespersonRoleName);
            _userIsRecruiter =
                roles.Contains(DataTransferObjects.Constants.RoleNames.RecruiterRoleName);
            _userIsHR =
                roles.Contains(DataTransferObjects.Constants.RoleNames.HRRoleName);
        }

        protected void btnAddNote_Click(object sender, EventArgs e)
        {
            Page.Validate(tbNote.ValidationGroup);

            if (Page.IsValid)
            {
                var note = new Note
                {
                    Author = new Person
                    {
                        Id = DataHelper.CurrentPerson.Id
                    },
                    CreateDate = DateTime.Now,
                    NoteText = tbNote.Text,
                    Target = (NoteTarget)4,
                    TargetId = (int)OpportunityId
                };

                ServiceCallers.Custom.Milestone(client => client.NoteInsert(note));

                lvNotes.DataSource = ServiceCallers.Custom.Milestone(c => c.NoteListByTargetId(Convert.ToInt32(OpportunityId), 4));
                lvNotes.DataBind();

                tbNote.Text = string.Empty;

                Utils.Generic.InvokeEventHandler(NoteAdded, this, e);
            }
        }

        /// <summary>
        /// 	Creates a project from the opportunity.
        /// </summary>
        protected void btnConvertToProject_Click(object sender, EventArgs e)
        {
            Page.Validate(vsumOpportunity.ValidationGroup);
            if (Page.IsValid)
            {
                Page.Validate(vsumWonConvert.ValidationGroup);
                if (!Page.IsValid)
                {
                    return;
                }

                var opportunity = Opportunity;

                if (opportunity == null)
                {
                    custOpportunityNotSaved.IsValid = false;
                }
                else if (CanUserEditOpportunity(opportunity))
                {
                    if (!CheckForDirtyBehaviour())
                    {
                        return;
                    }

                    Page.Validate(vsumWonConvert.ValidationGroup);
                    if (Page.IsValid)
                    {
                        ucProposedResources.FillProposedResources();
                        upProposedResources.Update();
                        if (HasProposedPersonsOfTypeNormal)
                        {
                            Page.Validate(vsumHasPersons.ValidationGroup);
                            if (Page.IsValid)
                            {
                                ConvertToProject();
                            }
                        }
                        else
                        {
                            ConvertToProject();
                        }
                    }
                }
            }
        }


        public bool CheckForDirtyBehaviour()
        {
            bool result = true;
            if (CanUserEditOpportunity(Opportunity))
            {
                if (IsDirty)
                {
                    if (!SaveDirty)
                    {
                        ClearDirty();
                        Display();
                    }
                    else if (!ValidateAndSave())
                    {
                        return false;
                    }
                }
            }
            return result;
        }

        private void ConvertToProject()
        {
            using (var serviceClient = new OpportunityServiceClient())
            {
                try
                {
                    var projectId = serviceClient.ConvertOpportunityToProject(OpportunityId.Value,
                                                                              User.Identity.Name, HasProposedPersonsOfTypeNormal);

                    Response.Redirect(
                            Urls.GetProjectDetailsUrl(projectId, Request.Url.AbsoluteUri));
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        protected void ddlClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlClientGroup.Items.Clear();
            ddlProjects.Items.Clear();

            if (!(ddlClient.SelectedIndex == 0))
            {
                int clientId;

                if (int.TryParse(ddlClient.SelectedValue, out clientId))
                {
                    var groups = ServiceCallers.Custom.Group(client => client.GroupListAll(clientId, null));
                    groups = groups.AsQueryable().Where(g => (g.IsActive == true)).ToArray();
                    DataHelper.FillListDefault(ddlClientGroup, string.Empty, groups, false);

                    var projects = ServiceCallers.Custom.Project(client => client.ListProjectsByClientShort(clientId, true, false, false));
                    DataHelper.FillListDefault(ddlProjects, "Select Project ...", projects, false, "Id", "DetailedProjectTitle");

                    if (ddlProjects.Items.Count == 1)
                    {
                        ddlProjects.Items[0].Enabled = true;
                    }
                }
                else if (ddlProjects.Items != null && ddlProjects.Items.Count == 0)
                {
                    ddlProjects.Items.Add(new ListItem() { Text = "Select Project ...", Value = "" });
                }
            }
            upOpportunityDetail.Update();
            upAttachToProject.Update();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            divResultDescription.Style["display"] = "inline";
            upAttachToProject.Update();

            bool IsSavedWithoutErrors = ValidateAndSave();
            if (IsSavedWithoutErrors)
            {
                mlConfirmation.ShowInfoMessage(string.Format(Resources.Messages.SavedDetailsConfirmation, "Opportunity"));
                ClearDirty();
                if (IsPostBack && Page.IsValid)
                {
                    LoadOpportunityDetails();
                }

                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "", "FadeOutLabel()", true);
            }

            if (Page.IsValid)
            {
                BindOpportunitiesData();
                lvOpportunities.DataBind();
                lvOpportunities.SelectedIndex = GetSelectedIndex(OpportunityId.Value);

            }
        }

        private int GetSelectedIndex(int OpportunityId)
        {
            foreach (ListViewDataItem item in lvOpportunities.Items)
            {
                Label lblOpportunityName = item.FindControl("lblOpportunityName") as Label;

                if (lblOpportunityName.Attributes["OpportunityID"] == OpportunityId.ToString())
                {
                    return item.DisplayIndex;
                }
            }

            return 0;
        }

        protected void btnCancelChanges_Click(object sender, EventArgs e)
        {
            if (IsDirty)
            {
                ClearDirty();
                LoadOpportunityDetails();
                tbNote.Text = "";
            }
            btnSave.Enabled = false;
        }

        protected void btnAttachToProject_Click(object sender, EventArgs e)
        {
            Page.Validate(vsumOpportunity.ValidationGroup);
            if (Page.IsValid)
            {
                mpeAttachToProject.Show();
            }
        }

        public void lvOpportunities_OnDataBound(object sender, EventArgs e)
        {
        }

        private void FocusToSelectedItem()
        {
            ScriptManager.RegisterClientScriptBlock(upOpportunityList, upOpportunityList.GetType(), "ScrollToImage", string.Format("ScrollToImage('{0}');", lvOpportunities.SelectedIndex), true);
        }

        private void SetScrollPosition()
        {
            ScriptManager.RegisterClientScriptBlock(upOpportunityList, upOpportunityList.GetType(), "setScrollPos", "setScrollPos();", true);
        }

        public string GetAnimationsScriptForAnimShow()
        {
            int lvCount = lvOpportunityPriorities.Items.Count;

            int height = ((lvCount + 1) * (35)) - 10;

            if (height > 150)
            {
                height = 180;
            }

            return string.Format(ANIMATION_SHOW_SCRIPT, pnlPriority.ID, 180);
        }

        public string GetAnimationsScriptForAnimHide()
        {
            return string.Format(ANIMATION_HIDE_SCRIPT, pnlPriority.ID);
        }


        protected override bool ValidateAndSave()
        {
            var retValue = false;

            Page.Validate(vsumOpportunity.ValidationGroup);
            if (Page.IsValid)
            {
                var opportunity = new Opportunity();
                PopulateData(opportunity);

                using (var serviceClient = new OpportunityServiceClient())
                {
                    try
                    {
                        int? id = serviceClient.OpportunitySave(opportunity, User.Identity.Name);

                        if (id.HasValue)
                        {
                            OpportunityId = id;
                        }

                        retValue = true;
                        ClearDirty();
                        lblSaved.Text = "Saved";

                        ViewState.Remove(OPPORTUNITY_KEY);
                        ViewState.Remove(PreviousReportContext_Key);
                        ViewState.Remove(DistinctPotentialBoldPersons_Key);
                        btnSave.Enabled = false;
                        UpdateOpportunitiesList();
                    }
                    catch (CommunicationException ex)
                    {
                        serviceClient.Abort();
                        throw;
                    }
                }
            }
            else
            {
                dpEndDate.ErrorMessage = string.Empty;
                dpStartDate.ErrorMessage = string.Empty;
                lblError.Text = "Error";
            }

            return retValue;
        }

        private void UpdateOpportunitiesList()
        {
            foreach (Opportunity item in OpportunitiesList)
            {
                if (item.Id.Value == Convert.ToInt32(lvOpportunities.SelectedValue))
                {
                    item.Priority = Opportunity.Priority;
                    item.Client.Name = Opportunity.Client.Name;

                    if (item.Group != null)
                    {
                        item.Group.Name = Opportunity.Group != null ? Opportunity.Group.Name : string.Empty;
                    }
                    else
                    {
                        item.Group = new ProjectGroup();
                        item.Group.Name = Opportunity.Group != null ? Opportunity.Group.Name : string.Empty;
                    }
                    item.EstimatedRevenue = Opportunity.EstimatedRevenue;
                    item.Name = Opportunity.Name;
                    item.BuyerName = Opportunity.BuyerName;
                    item.Salesperson.LastName = Opportunity.Salesperson != null ? Opportunity.Salesperson.LastName : string.Empty;
                    break;
                }
            }
        }

        protected override void Display()
        {
        }

        private void FillControls()
        {
            if (OpportunityId.HasValue)
            {
                var opportunity = Opportunity;

                if (opportunity != null)
                {

                    PopulateControls(opportunity);

                    UpdateState(opportunity);

                    upOpportunityDetail.Update();
                    upDescription.Update();
                    upAttachToProject.Update();
                }
            }
        }

        /// <summary>
        /// 	Updates control state for the security reasons.
        /// </summary>
        /// <param name = "opportunity">An opportunity is being displayed.</param>
        private void UpdateState(Opportunity opportunity)
        {
            // Security
            var canEdit = CanUserEditOpportunity(opportunity);

            var isStatusReadOnly =
                 !canEdit ||
                 (!_userIsAdministrator && !_userIsSalesperson && _userIsRecruiter) ||
                 (!_userIsAdministrator && !_userIsSalesperson && _userIsHR); //#2817: this line is added asper requirement.


            txtOpportunityName.ReadOnly =
                  txtBuyerName.ReadOnly =
                  txtDescription.ReadOnly =
                  txtEstRevenue.ReadOnly =
                                         !canEdit;

            ddlClient.Enabled =
                ddlPriority.Enabled =
                ddlSalesperson.Enabled =
                ddlPractice.Enabled =
                btnSave.Enabled =
                ddlOpportunityOwner.Enabled =
                ucProposedResources.Enabled = canEdit;

            btnConvertToProject.Enabled =
                btnAttachToProject.Enabled = canEdit && (opportunity.Project == null);
            hdnHasProjectIdOrPermission.Value = canEdit && (opportunity.Project == null) ? "false" : "true";

            ddlClientGroup.Visible = canEdit;

            ddlStatus.Enabled = !isStatusReadOnly;

            if (!canEdit)
                btnConvertToProject.OnClientClick = Resources.Controls.OpportunityInappropriatePersonMessage;

            lblReadOnlyWarning.Visible = !canEdit;
        }

        /// <summary>
        /// 	Determine whether the current user can convert the specified opportunity
        /// </summary>
        /// <param name = "opportunity">The <see cref = "Opportunity" /> to be checked for.</param>
        /// <returns>true if the user can convert the <see cref = "Opportunity" /> and false otherwise.</returns>
        private bool CanUserEditOpportunity(Opportunity opportunity)
        {
            var current = DataHelper.CurrentPerson;

            var isOwnerOrPracticeManagerOrTheSamePractice =
                current != null && current.Id.HasValue && // Current is not null
                // Current is Opportunity Owner
                (opportunity.Owner != null && current.Id == opportunity.Owner.Id
                    ||
                // or current is practice manager
                opportunity.Practice.PracticeOwner != null && opportunity.Practice.PracticeOwner.Id == current.Id
                    ||
                // or current is of the same practice
                current.DefaultPractice != null && opportunity.Practice != null && current.DefaultPractice == opportunity.Practice);

            return _userIsAdministrator || isOwnerOrPracticeManagerOrTheSamePractice || _userIsRecruiter || _userIsSalesperson || _userIsHR;//#2817: _userIsHR is added as per requirement.
        }

        private void PopulateControls(Opportunity opportunity)
        {
            txtEstRevenue.Text = opportunity.EstimatedRevenue != null ? opportunity.EstimatedRevenue.Value.ToString("###,###,###,###,##0") : string.Empty;
            txtOpportunityName.Text = opportunity.Name;
            lblOpportunityNumber.Text = opportunity.OpportunityNumber;

            dpStartDate.DateValue = opportunity.ProjectedStartDate.HasValue ? opportunity.ProjectedStartDate.Value : DateTime.MinValue;
            dpEndDate.DateValue = opportunity.ProjectedEndDate.HasValue ? opportunity.ProjectedEndDate.Value : DateTime.MinValue;

            txtDescription.Text = opportunity.Description;
            txtBuyerName.Text = opportunity.BuyerName;
            lblLastUpdate.Text = opportunity.LastUpdate.ToString("MM/dd/yyyy");

            ddlStatus.SelectedIndex =
                ddlStatus.Items.IndexOf(
                    ddlStatus.Items.FindByValue(
                        opportunity.Status != null ? opportunity.Status.Id.ToString() : string.Empty));
            ddlClient.SelectedIndex =
                ddlClient.Items.IndexOf(
                    ddlClient.Items.FindByValue(
                        opportunity.Client != null && opportunity.Client.Id.HasValue
                            ? opportunity.Client.Id.Value.ToString()
                            : string.Empty));

            ddlPriority.SelectedIndex =
                ddlPriority.Items.IndexOf(
                    ddlPriority.Items.FindByValue(opportunity.Priority.Id.ToString()));

            PopulateSalesPersonDropDown();

            PopulatePracticeDropDown();

            PopulateOwnerDropDown();

            FillGroupAndProjectDropDown();

            PopulateClientGroupDropDown();

            PopulateProjectsDropDown();

            hdnValueChanged.Value = "false";
            btnSave.Attributes.Add("disabled", "true");
        }

        private void PopulateProjectsDropDown()
        {
            ListItem selectedProject = ddlProjects.Items.FindByValue(Opportunity.Project.Id.ToString());
            if (selectedProject != null)
            {
                ddlProjects.SelectedValue = selectedProject.Value;
            }
        }

        private void PopulatePriorityHint()
        {
            var opportunityPriorities = OpportunityPriorityHelper.GetOpportunityPriorities(true);
            lvOpportunityPriorities.DataSource = opportunityPriorities;
            lvOpportunityPriorities.DataBind();
        }

        private void PopulateSalesPersonDropDown()
        {
            if (Opportunity != null && Opportunity.Salesperson != null)
            {
                ListItem selectedSalesPerson = ddlSalesperson.Items.FindByValue(Opportunity.Salesperson.Id.ToString());
                if (selectedSalesPerson == null)
                {
                    selectedSalesPerson = new ListItem(Opportunity.Salesperson.Name, Opportunity.Salesperson.Id.ToString());
                    ddlSalesperson.Items.Add(selectedSalesPerson);
                    ddlSalesperson.SortByText();
                }

                ddlSalesperson.SelectedValue = selectedSalesPerson.Value;
            }
        }

        private void PopulatePracticeDropDown()
        {

            if (Opportunity != null && Opportunity.Practice != null)
            {
                ListItem selectedPractice = ddlPractice.Items.FindByValue(Opportunity.Practice.Id.ToString());
                // For situation, when disabled practice is assigned to Opportunity.
                if (selectedPractice == null)
                {
                    selectedPractice = new ListItem(Opportunity.Practice.Name, Opportunity.Practice.Id.ToString());
                    ddlPractice.Items.Add(selectedPractice);
                    ddlPractice.SortByText();
                }

                ddlPractice.SelectedValue = selectedPractice.Value;
            }
            else
            {
                ddlPractice.SelectedIndex = 0;
            }
        }

        private void PopulateOwnerDropDown()
        {
            if (Opportunity.Owner != null)
            {
                var ownerId = Opportunity.Owner.Id.Value.ToString();
                ListItem selectedOwner = ddlOpportunityOwner.Items.FindByValue(ownerId);
                if (selectedOwner == null)
                {
                    selectedOwner = new ListItem(Opportunity.Owner.PersonLastFirstName, ownerId);
                    ddlOpportunityOwner.Items.Add(selectedOwner);
                    ddlOpportunityOwner.SortByText();
                }

                ddlOpportunityOwner.SelectedValue = selectedOwner.Value;
            }
            else
            {
                ddlOpportunityOwner.SelectedValue = string.Empty;
            }
        }

        private void PopulateClientGroupDropDown()
        {
            if (Opportunity.Group != null && Opportunity.Group.Id.HasValue)
            {
                ListItem selectedGroup = ddlClientGroup.Items.FindByValue(Opportunity.Group.Id.ToString());
                if (selectedGroup == null)
                {
                    selectedGroup = new ListItem(Opportunity.Group.Name, Opportunity.Group.Id.ToString());
                    ddlClientGroup.Items.Add(selectedGroup);
                    ddlClientGroup.SortByText();
                }

                ddlClientGroup.SelectedValue = selectedGroup.Value;
            }
        }

        private void PopulateData(Opportunity opportunity)
        {
            opportunity.Id = OpportunityId;
            opportunity.Name = txtOpportunityName.Text;
            opportunity.ProjectedStartDate = dpStartDate.DateValue != DateTime.MinValue
                        ? (DateTime?)dpStartDate.DateValue
                        : null;
            opportunity.ProjectedEndDate =
                dpEndDate.DateValue != DateTime.MinValue
                        ? (DateTime?)dpEndDate.DateValue
                        : null;
            int priorityId;
            if (int.TryParse(ddlPriority.SelectedValue, out priorityId))
            {
                opportunity.Priority = new OpportunityPriority { Id = priorityId };
            }
            opportunity.Description = txtDescription.Text;
            opportunity.BuyerName = txtBuyerName.Text;

            opportunity.Status =
                new OpportunityStatus { Id = int.Parse(ddlStatus.SelectedValue) };
            opportunity.Client =
                new Client { Id = int.Parse(ddlClient.SelectedValue) };

            if (ddlClientGroup.Items.Count > 0 && ddlClientGroup.SelectedValue != string.Empty)
                opportunity.Group = new ProjectGroup { Id = Convert.ToInt32(ddlClientGroup.SelectedValue) };
            else
                opportunity.Group = null;

            opportunity.Salesperson =
                !string.IsNullOrEmpty(ddlSalesperson.SelectedValue)
                    ? new Person { Id = int.Parse(ddlSalesperson.SelectedValue) }
                    : null;
            opportunity.Practice =
                new Practice { Id = int.Parse(ddlPractice.SelectedValue) };

            opportunity.EstimatedRevenue = Convert.ToDecimal(txtEstRevenue.Text);

            if (!(string.IsNullOrEmpty(ddlProjects.SelectedValue)
                 || ddlProjects.SelectedValue == "-1")
               )
            {
                opportunity.Project = new Project
                {
                    Id =
                        int.Parse(ddlProjects.SelectedValue)
                };
            }

            var selectedValue = ddlOpportunityOwner.SelectedValue;
            opportunity.Owner = string.IsNullOrEmpty(selectedValue) ?
                null :
                new Person(Convert.ToInt32(selectedValue));

            opportunity.ProposedPersonIdList = ucProposedResources.GetProposedPersonsIdsList();
        }

        protected static string GetFormattedEstimatedRevenue(Decimal? estimatedRevenue)
        {
            return estimatedRevenue.GetFormattedEstimatedRevenue();
        }

        protected static string GetTruncatedOpportunityName(String Name)
        {
            if (Name.Length > 32)
            {
                Name = Name.Substring(0, 30) + "..";
            }

            return Name;
        }

        protected static string GetWrappedText(String NoteText)
        {
            if (NoteText.Length > 70)
            {
                for (int i = 10; i < NoteText.Length; i = i + 10)
                {
                    NoteText = NoteText.Insert(i, WordBreak);
                }
            }

            return NoteText;
        }

        #region Validations

        protected void custTransitionStatus_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            int statusId;
            e.IsValid =
                !int.TryParse(e.Value, out statusId) || statusId != (int)OpportunityTransitionStatusType.Lost ||
                // Only Administratos, Salesperson or Practice Manager can set the status to Lost.
                _userIsAdministrator || _userIsSalesperson;
        }

        protected void custOppDescription_ServerValidation(object source, ServerValidateEventArgs args)
        {
            args.IsValid = txtDescription.Text.Length <= 2000;
        }

        protected void custWonConvert_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid =
                ddlStatus.SelectedValue != WonProjectId.ToString();
        }

        protected void cvLen_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            var length = tbNote.Text.Length;
            args.IsValid = length > 0 && length <= 2000;
        }

        protected void custEstRevenue_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            e.IsValid = true;
            Decimal result;
            bool isDecimal = Decimal.TryParse(txtEstRevenue.Text, out result);

            if (isDecimal)
            {
                if (result < 1000)
                {
                    e.IsValid = false;
                }
            }
        }

        protected void custEstimatedRevenue_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            e.IsValid = true;
            Decimal result;
            string revenueText = txtEstRevenue.Text;

            bool isDecimal = Decimal.TryParse(revenueText, out result);

            if (!isDecimal)
            {
                e.IsValid = false;
            }
        }

        #endregion

        #endregion

        #region IPostBackEventHandler Members

        public void RaisePostBackEvent(string eventArgument)
        {
            if (ValidateAndSave())
            {
                Redirect(eventArgument == string.Empty ? Constants.ApplicationPages.OpportunityList : eventArgument);
            }
        }

        #endregion
    }
}

