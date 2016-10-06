﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PraticeManagement.Controls;
using PraticeManagement.Utils;
using System.ComponentModel;
using DataTransferObjects;
using DataTransferObjects.Skills;
using System.Xml;
using AjaxControlToolkit;
using System.Web.Security;
using System.Net;
using System.Xml.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using PraticeManagement.ConfigurationService;

namespace PraticeManagement
{
    public partial class SkillsEntry : PracticeManagementPageBase, IPostBackEventHandler
    {
        #region Constants

        private const string SessionPersonWithSkills = "PersonWithSkills";
        private const string ViewStatePreviousActiveTabIndex = "PreviousActiveTabIndex";
        private const string ViewStatePreviousCategoryIndex = "PreviousCategoryIndex";
        private const string ValidationPopUpMessage = "Please select a value for ‘Level’, ‘Experience’, and ‘Last Used’ for the below skill(s):";
        private const string SuccessMessage = "Skills Saved Successfully.";
        private const string ProfileXml = @"<Profile Id=""{0}"" ProfileName=""{1}"" ProfileURL=""{2}"" IsDefault=""{3}"" > </Profile>";
        private const string SuccessMessageForAddedPicture = "Consultant Profile picture added successfully.";
        private const string SuccessMessageForUpdatedPicture = "Consultant Profile picture updated successfully.";
        private const string SuccessMessageForDeletedPicture = "Consultant Profile picture deleted successfully.";
        private const string Update = "Update";
        private const string Add = "Add";



        //scripts
        private const string ANIMATION_SHOW_SCRIPT =
                     @"<OnClick>
                        	<Sequence>
                        		<Parallel Duration=""0"" AnimationTarget=""{0}"">
                        			<Discrete Property=""style"" propertyKey=""border"" ValuesScript=""['thin solid navy']""/>
                        		</Parallel>
                        		<Parallel Duration="".4"" Fps=""20"" AnimationTarget=""{0}"">
                        			<Resize  Width=""350"" Height=""{1}"" Unit=""px"" />
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

        //Ids
        private const string ddlLevelId = "ddlLevel";
        private const string ddlExperienceId = "ddlExperience";
        private const string ddlLastUsedId = "ddlLastUsed";
        private const string hdnChangedId = "hdnChanged";
        private const string hdnIdId = "hdnId";
        private const string hdnDescriptionId = "hdnDescription";
        private const string lnkbtnClearId = "lnkbtnClear";

        //Validator Ids
        private const string cvSkillsId = "cvSkills";
        private const string cvLastUsedId = "cvLastUsed";
        private const string cvExperienceId = "cvExperience";
        private const string cvLevelId = "cvLevel";

        //Xml
        private const string Root = "Skills";
        private const string SkillTag = "Skill";
        private const string IndustrySkillTag = "IndustrySkill";
        private const string IdAttribute = "Id";
        private const string LevelAttribute = "Level";
        private const string ExperienceAttribute = "Experience";
        private const string LastUsedAttribute = "LastUsed";

        #endregion

        #region fields

        public bool IsFirst = true;
        private bool IsPreviousTabValid = true;
        private bool UnsavedSkillsEntriesExists = false;

        #endregion

        #region Properties

        public int? PersonId
        {
            get
            {
                int personId;
                if (!string.IsNullOrEmpty(Page.Request.QueryString["id"]) && int.TryParse(Page.Request.QueryString["id"], out personId))
                {
                    return personId;
                }
                else
                {
                    return null;
                }
            }
        }

        public Person Person
        {
            get
            {
                int personId;
                if (PersonId.HasValue)
                {
                    personId = PersonId.Value;
                }
                else
                {
                    personId = DataHelper.CurrentPerson.Id.Value;
                }
                if (ViewState[SessionPersonWithSkills] == null)
                {
                    using (var serviceClient = new PersonSkillService.PersonSkillServiceClient())
                    {
                        ViewState[SessionPersonWithSkills] = serviceClient.GetPersonProfilesWithSkills(personId);
                    }
                }
                return (Person)ViewState[SessionPersonWithSkills];
            }
            set
            {
                ViewState[SessionPersonWithSkills] = value;
            }
        }

        public List<Profile> PersonProfiles
        {
            get
            {
                if (ViewState["PersonProfiles"] == null)
                {
                    ViewState["PersonProfiles"] = Person.Profiles.Count > 0 ? Person.Profiles : new List<Profile> { new Profile() { ProfileId = -1 } };
                }
                return (List<Profile>)ViewState["PersonProfiles"];
            }
            set
            {
                ViewState["PersonProfiles"] = value;
            }
        }

        public int SelectedBusinessCategory
        {
            get
            {
                return Convert.ToInt32(ddlBusinessCategory.SelectedValue);
            }
        }

        public int SelectedTechnicalCategory
        {
            get
            {
                return Convert.ToInt32(ddlTechnicalCategory.SelectedValue);
            }
        }

        public int PreviousActiveTabIndex
        {
            get
            {
                if (ViewState[ViewStatePreviousActiveTabIndex] == null)
                {
                    ViewState[ViewStatePreviousActiveTabIndex] = 0;
                }
                return (int)ViewState[ViewStatePreviousActiveTabIndex];
            }
            set
            {
                ViewState[ViewStatePreviousActiveTabIndex] = value;
            }
        }

        public int PreviousCategoryIndex
        {
            get
            {
                if (ViewState[ViewStatePreviousCategoryIndex] == null)
                {
                    ViewState[ViewStatePreviousCategoryIndex] = 0;
                }
                return (int)ViewState[ViewStatePreviousCategoryIndex];
            }
            set
            {
                ViewState[ViewStatePreviousCategoryIndex] = value;
            }
        }

        #endregion

        #region Control Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (PersonId.HasValue && PersonId != DataHelper.CurrentPerson.Id && !(Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.AdministratorRoleName)))
                {
                    Response.Redirect(Constants.ApplicationPages.AccessDeniedPage);
                }

                ltrlPicturePopupPersonname.Text = ltrlPersonname.Text = lblUserName.Text = Person.LastName + " " + Person.FirstName;
                repProfiles.DataSource = PersonProfiles;
                repProfiles.DataBind();
                RenderSkills(tcSkillsEntry.ActiveTabIndex);

                if (ddlTechnicalCategory.DataSource == null && ddlTechnicalCategory.Items.Count == 0)
                {
                    ddlTechnicalCategory.DataSource = Utils.SettingsHelper.GetSkillCategoriesByType(2);//Load TechnicalSkills Categories also.
                    ddlTechnicalCategory.DataBind();
                    ddlTechnicalCategory.SelectedIndex = 0;
                }
            }
            lblValidationMessage.Text = "";
            hdnIsValid.Value = false.ToString();
            lblMessage.Text = "";
        }

        protected override void Display()
        {

        }

        protected void tcSkillsEntry_ActiveTabChanged(object sender, EventArgs e)
        {
            if (IsFirst)
            {
                if (IsDirty)
                {
                    if (!ValidateAndSave(PreviousActiveTabIndex))
                    {
                        tcSkillsEntry.ActiveTabIndex = PreviousActiveTabIndex;
                        IsFirst = false;
                        IsPreviousTabValid = false;
                        return;
                    }
                }

                RenderSkills(tcSkillsEntry.ActiveTabIndex);
                IsFirst = false;
            }

            if (!IsFirst && !IsPreviousTabValid)
            {
                tcSkillsEntry.ActiveTabIndex = PreviousActiveTabIndex;
            }
        }

        protected void ddlCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            int activeTabIndex = tcSkillsEntry.ActiveTabIndex;
            if (IsDirty)
            {
                if (!ValidateAndSave(activeTabIndex))
                {
                    var ddlCategory = sender as DropDownList;
                    ddlCategory.SelectedIndex = PreviousCategoryIndex;
                    return;
                }
            }

            BindSkills(activeTabIndex);
        }

        protected void gvSkills_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var ddlLevel = e.Row.FindControl(ddlLevelId) as DropDownList;
                var ddlExperience = e.Row.FindControl(ddlExperienceId) as DropDownList;
                var ddlLastUsed = e.Row.FindControl(ddlLastUsedId) as DropDownList;
                var hdnId = e.Row.FindControl(hdnIdId) as HiddenField;
                var clearLink = e.Row.FindControl(lnkbtnClearId) as LinkButton;
                ApplyClearLinkStyle(clearLink, false);
                if (Person.Skills.Count > 0)
                {
                    if (Person.Skills.Where(s => s.Skill.Id == Convert.ToInt32(hdnId.Value)).Count() > 0)
                    {
                        var skill = Person.Skills.Where(s => s.Skill.Id == Convert.ToInt32(hdnId.Value)).First();

                        if (skill != null)
                        {
                            ddlLevel.SelectedValue = skill.SkillLevel.Id.ToString();
                            ddlExperience.SelectedValue = skill.YearsExperience.Value.ToString();
                            ddlLastUsed.SelectedValue = skill.LastUsed.ToString();
                            ApplyClearLinkStyle(clearLink, true);
                        }
                    }
                }
            }
        }

        private GridView ActiveGridView(bool includeIndustries)
        {
            int activeIndex = tcSkillsEntry.ActiveTabIndex;
            switch (activeIndex)
            {
                case 0:
                    return gvBusinessSkills;
                case 1:
                    return gvTechnicalSkills;
                case 3:
                    if (includeIndustries)
                    {
                        return gvIndustrySkills;
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            GridView gvSkills = ActiveGridView(false);
            if (gvSkills != null)
            {
                var animHide = gvSkills.HeaderRow.FindControl("animHide") as AnimationExtender;
                var animShow = gvSkills.HeaderRow.FindControl("animShow") as AnimationExtender;
                var pnlLevel = gvSkills.HeaderRow.FindControl("pnlLevel") as Panel;
                var btnClosePriority = gvSkills.HeaderRow.FindControl("btnCloseLevel") as Button;
                var dtlSkillLevels = gvSkills.HeaderRow.FindControl("dtlSkillLevels") as DataList;
                var img = gvSkills.HeaderRow.FindControl("imgLevelyHint") as Image;
                animShow.Animations = string.Format(ANIMATION_SHOW_SCRIPT, pnlLevel.ID, 175);
                animHide.Animations = string.Format(ANIMATION_HIDE_SCRIPT, pnlLevel.ID);
                img.Attributes["onclick"]
                       = string.Format("setHintPosition('{0}', '{1}');", img.ClientID, pnlLevel.ClientID);
                dtlSkillLevels.DataSource = SettingsHelper.GetSkillLevels();
                dtlSkillLevels.DataBind();
            }
            //enable delete button if person has a picture.
            btnPictureDelete.Enabled = Person.HasPicture;
            btnUpdatePictureLink.Text = btnUpdatePictureLink.ToolTip = (Person.HasPicture) ? Update : Add;
        }

        protected void gvIndustrySkills_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var ddlExperience = e.Row.FindControl(ddlExperienceId) as DropDownList;
                var hdnId = e.Row.FindControl(hdnIdId) as HiddenField;
                if (Person.Industries.Count > 0)
                {
                    if (Person.Industries.Where(i => i.Industry.Id == Convert.ToInt32(hdnId.Value)).Count() > 0)
                    {
                        var industry = Person.Industries.Where(i => i.Industry.Id == Convert.ToInt32(hdnId.Value)).First();

                        if (industry != null)
                        {
                            ddlExperience.SelectedValue = industry.YearsExperience.ToString();
                        }
                    }
                }
            }
        }

        protected void cvSkills_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            var validator = sender as CustomValidator;
            var row = validator.Parent.Parent as GridViewRow;

            var hdnChanged = row.FindControl(hdnChangedId) as HiddenField;
            if (hdnChanged != null && hdnChanged.Value == "1")
            {
                var ddlLevel = row.FindControl(ddlLevelId) as DropDownList;
                var ddlExperience = row.FindControl(ddlExperienceId) as DropDownList;
                var ddlLastUsed = row.FindControl(ddlLastUsedId) as DropDownList;
                var hdnDescription = row.FindControl(hdnDescriptionId) as HiddenField;

                if (!(ddlLevel.SelectedIndex == 0 && ddlExperience.SelectedIndex == 0 && ddlLastUsed.SelectedIndex == 0))
                {
                    if (ddlLevel.SelectedIndex == 0 || ddlExperience.SelectedIndex == 0 || ddlLastUsed.SelectedIndex == 0)
                    {
                        lblValidationMessage.Text = (lblValidationMessage.Text == "") ? "- " + hdnDescription.Value
                                                                                    : lblValidationMessage.Text + ",<br />" + "- " + hdnDescription.Value;
                        e.IsValid = false;
                        var cvLevel = row.FindControl(cvLevelId) as CustomValidator;
                        var cvExperience = row.FindControl(cvExperienceId) as CustomValidator;
                        var cvLastUsed = row.FindControl(cvLastUsedId) as CustomValidator;

                        cvLevel.Validate();
                        cvExperience.Validate();
                        cvLastUsed.Validate();

                        cvLevel.IsValid = !(ddlLevel.SelectedIndex == 0);
                        cvExperience.IsValid = !(ddlExperience.SelectedIndex == 0);
                        cvLastUsed.IsValid = !(ddlLastUsed.SelectedIndex == 0);
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int activeTabIndex = tcSkillsEntry.ActiveTabIndex;
            if (ValidateAndSave(activeTabIndex))
            {
                lblMessage.Text = SuccessMessage;
                mpeErrorPanel.Show();
                lblMessage.Focus();
            }
        }

        protected new void btnCancel_Click(object sender, EventArgs e)
        {
            BindSkills(tcSkillsEntry.ActiveTabIndex);
            ClearDirty();
        }

        protected void btnCancelPicture_Click(object sender, EventArgs e)
        {
            EnableClickLinkButton();
            EnableSaveAndCancelButtons(UnsavedSkillsEntriesExists);
        }

        protected void btnUpdatePictureLink_OnClick(object sender, EventArgs e)
        {
            EnableClickLinkButton();
            EnableSaveAndCancelButtons(UnsavedSkillsEntriesExists);
            if (fuPersonPicture.HasFile)
            {
                PraticeManagement.AttachmentService.AttachmentService svc = PraticeManagement.Utils.WCFClientUtility.GetAttachmentService();
                svc.SavePersonPicture(Person.Id.Value, fuPersonPicture.FileBytes, User.Identity.Name, fuPersonPicture.FileName);

                lblMessage.Text = !Person.HasPicture ? SuccessMessageForAddedPicture : SuccessMessageForUpdatedPicture;
                Person.HasPicture = true;
                hdnTargetErrorPanel.Value = false.ToString().ToLower();
                lblMessage.Focus();
            }
        }

        protected void btnPictureDelete_OnClick(object sender, EventArgs e)
        {
            EnableClickLinkButton();
            EnableSaveAndCancelButtons(UnsavedSkillsEntriesExists);
            PraticeManagement.AttachmentService.AttachmentService svc = PraticeManagement.Utils.WCFClientUtility.GetAttachmentService();
            svc.SavePersonPicture(Person.Id.Value, null, User.Identity.Name, null);

            Person.HasPicture = false;
            lblMessage.Text = SuccessMessageForDeletedPicture;
            mpeErrorPanel.Show();
            lblMessage.Focus();
        }

        protected void btnProfilePopupUpdate_OnClick(object sender, EventArgs e)
        {
            EnableClickLinkButton();
            EnableSaveAndCancelButtons(UnsavedSkillsEntriesExists);
            UpdatePersonProfilesFromRep(false);
            Page.Validate("ProfileValidationGroup");
            if (Page.IsValid)
            {
                string profilesXml = GetProfilesLinksXml();
                ServiceCallers.Custom.PersonSkill(p => p.SavePersonProfiles(Person.Id.Value, profilesXml, DataHelper.CurrentPerson.Alias));
                var personProfiles = ServiceCallers.Custom.PersonSkill(p => p.GetPersonProfiles(Person.Id.Value)).ToList();
                Person.Profiles = personProfiles;
                BindProfilesRepeater(null);
            }
            else
            {
                mpeProfilePopUp.Show();
            }
        }

        protected void btnCancelProfile_OnClick(object sender, EventArgs e)
        {
            EnableClickLinkButton();
            EnableSaveAndCancelButtons(UnsavedSkillsEntriesExists);
            BindProfilesRepeater(null);
        }

        private void BindProfilesRepeater(List<Profile> personProfiles)
        {
            PersonProfiles = personProfiles;
            repProfiles.DataSource = PersonProfiles;
            repProfiles.DataBind();
        }

        protected void ibtnAddProfile_Click(object sender, EventArgs e)
        {
            EnableClickLinkButton();
            EnableSaveAndCancelButtons(UnsavedSkillsEntriesExists);
            UpdatePersonProfilesFromRep(true);
            BindProfilesRepeater(PersonProfiles);
            mpeProfilePopUp.Show();
        }

        protected void cvProfileName_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
            var validator = source as CustomValidator;
            var row = validator.NamingContainer;
            var txtProfileName = row.FindControl("txtProfileName") as TextBox;
            var txtProfileLink = row.FindControl("txtProfileLink") as TextBox;
            var rbprofileIsDefault = row.FindControl("rbprofileIsDefault") as RadioButton;
            RepeaterItemCollection items = repProfiles.Items;

            if (string.IsNullOrEmpty(txtProfileName.Text) && (!string.IsNullOrEmpty(txtProfileLink.Text) || rbprofileIsDefault.Checked))
            {
                args.IsValid = false;
                if (items.Count == 1 && string.IsNullOrEmpty(txtProfileLink.Text))
                {
                    args.IsValid = true;
                }
            }
        }

        protected void cvProfileNameDuplicate_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
            var validator = source as CustomValidator;
            var row = validator.NamingContainer;
            var currentRowProfileName = row.FindControl("txtProfileName") as TextBox;
            var currentRowProfileId = row.FindControl("hdProfileId") as HiddenField;
            if (string.IsNullOrEmpty(currentRowProfileName.Text))
            {
                return;
            }
            RepeaterItemCollection items = repProfiles.Items;
            foreach (RepeaterItem item in items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    var hdProfileId = item.FindControl("hdProfileId") as HiddenField;
                    var txtProfileName = item.FindControl("txtProfileName") as TextBox;
                    if (!string.IsNullOrEmpty(txtProfileName.Text) && currentRowProfileId.Value != hdProfileId.Value && txtProfileName.Text == currentRowProfileName.Text)
                    {
                        args.IsValid = false;
                        break;
                    }
                }
            }
        }

        protected void cvProfileLink_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
            var validator = source as CustomValidator;
            var row = validator.NamingContainer;
            var txtProfileName = row.FindControl("txtProfileName") as TextBox;
            var txtProfileLink = row.FindControl("txtProfileLink") as TextBox;
            var rbprofileIsDefault = row.FindControl("rbprofileIsDefault") as RadioButton;
            RepeaterItemCollection items = repProfiles.Items;

            if (string.IsNullOrEmpty(txtProfileLink.Text) && (!string.IsNullOrEmpty(txtProfileName.Text) || rbprofileIsDefault.Checked))
            {
                args.IsValid = false;
                if (items.Count == 1 && string.IsNullOrEmpty(txtProfileName.Text))
                {
                    args.IsValid = true;
                }
            }
        }

        protected void cvIsDefault_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = false;
            RepeaterItemCollection items = repProfiles.Items;
            int i = 0;
            foreach (RepeaterItem item in items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    var txtProfileName = item.FindControl("txtProfileName") as TextBox;
                    var txtProfileLink = item.FindControl("txtProfileLink") as TextBox;
                    var rbprofileIsDefault = item.FindControl("rbprofileIsDefault") as RadioButton;
                    if (!string.IsNullOrEmpty(txtProfileName.Text) || !string.IsNullOrEmpty(txtProfileName.Text))
                    {
                        i++;
                    }
                    if (rbprofileIsDefault.Checked)
                    {
                        args.IsValid = true;
                        break;
                    }
                }
            }

            if (!args.IsValid && i == 0)
            {
                args.IsValid = true;
            }
        }

        protected void repProfiles_OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {

            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var rbprofileIsDefault = e.Item.FindControl("rbprofileIsDefault") as RadioButton;
                rbprofileIsDefault.Attributes["value"] = rbprofileIsDefault.UniqueID;
            }
        }

        #endregion

        #region Methods

        private bool ValidateAndSave(int activeTabIndex)
        {
            bool result = false;
            switch (activeTabIndex)
            {
                case 0:
                    Page.Validate(valSummaryBusiness.ValidationGroup);
                    if (Page.IsValid)
                    {
                        result = Page.IsValid;
                        SaveBusinessSkills();
                    }
                    break;
                case 1:
                    Page.Validate(valSummaryTechnical.ValidationGroup);
                    if (Page.IsValid)
                    {
                        result = Page.IsValid;
                        SaveTechnicalSkills();
                    }
                    break;
                case 2:
                    SaveIndustrySkills();
                    result = true;
                    break;
            }
            EnableSaveAndCancelButtons(!result);
            EnableClickLinkButton();
            hdnIsValid.Value = result.ToString().ToLower();
            return result;
        }

        private void EnableClickLinkButton()
        {
            var grid = ActiveGridView(false);
            if (grid != null)
            {
                var rows = grid.Rows;
                foreach (GridViewRow row in rows)
                {
                    var hdnChanged = row.FindControl(hdnChangedId) as HiddenField;
                    if (hdnChanged != null && hdnChanged.Value == "1")
                    {
                        UnsavedSkillsEntriesExists = true;
                        var ddlLevel = row.FindControl(ddlLevelId) as DropDownList;
                        var ddlExperience = row.FindControl(ddlExperienceId) as DropDownList;
                        var ddlLastUsed = row.FindControl(ddlLastUsedId) as DropDownList;
                        var clearLink = row.FindControl(lnkbtnClearId) as LinkButton;

                        ApplyClearLinkStyle(clearLink, !(ddlLevel.SelectedIndex == 0 && ddlExperience.SelectedIndex == 0 && ddlLastUsed.SelectedIndex == 0));
                    }
                }
            }
        }

        private void EnableSaveAndCancelButtons(bool enable)
        {
            btnSave.Enabled = enable;
            btnCancel.Enabled = enable;
        }

        private void RenderSkills(int activeTabIndex)
        {
            if (activeTabIndex == 0 || activeTabIndex == 1)
            {
                var categories = Utils.SettingsHelper.GetSkillCategoriesByType(activeTabIndex + 1);

                switch (activeTabIndex)
                {
                    case 0:
                        if (ddlBusinessCategory.DataSource == null && ddlBusinessCategory.Items.Count == 0)
                        {
                            ddlBusinessCategory.DataSource = categories;
                            ddlBusinessCategory.DataBind();
                            ddlBusinessCategory.SelectedIndex = 0;
                        }
                        break;

                    case 1:
                        if (ddlTechnicalCategory.DataSource == null && ddlTechnicalCategory.Items.Count == 0)
                        {
                            ddlTechnicalCategory.DataSource = categories;
                            ddlTechnicalCategory.DataBind();
                            ddlTechnicalCategory.SelectedIndex = 0;
                        }
                        break;
                }
            }

            BindSkills(activeTabIndex);
        }

        private void BindSkills(int activeTabIndex)
        {
            switch (activeTabIndex)
            {
                case 0:
                    gvBusinessSkills.DataSource = Utils.SettingsHelper.GetSkillsByCategory(SelectedBusinessCategory);
                    gvBusinessSkills.DataBind();
                    PreviousCategoryIndex = ddlBusinessCategory.SelectedIndex;
                    break;
                case 1:
                    gvTechnicalSkills.DataSource = Utils.SettingsHelper.GetSkillsByCategory(SelectedTechnicalCategory);
                    gvTechnicalSkills.DataBind();
                    PreviousCategoryIndex = ddlTechnicalCategory.SelectedIndex;
                    break;
                case 2:
                    gvIndustrySkills.DataSource = Utils.SettingsHelper.GetIndustrySkillsAll();
                    gvIndustrySkills.DataBind();
                    break;
            }
            PreviousActiveTabIndex = activeTabIndex;
        }

        private void SaveTechnicalSkills()
        {
            var rows = gvTechnicalSkills.Rows;
            SaveBusinessORTechnicalSkills(rows);
        }

        private void SaveBusinessSkills()
        {
            var rows = gvBusinessSkills.Rows;
            SaveBusinessORTechnicalSkills(rows);
        }

        private void ApplyClearLinkStyle(LinkButton clearLink, bool enable)
        {
            if (enable)
            {
                clearLink.Enabled = true;
                clearLink.Attributes["disable"] = false.ToString();
                clearLink.CssClass = "fontUnderline linkEnableStyle";
            }
            else
            {
                clearLink.Enabled = false;
                clearLink.Attributes["disable"] = true.ToString();
                clearLink.CssClass = "fontUnderline linkDisableStyle";
            }
        }

        private void SaveBusinessORTechnicalSkills(GridViewRowCollection rows)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement(Root);

            foreach (GridViewRow row in rows)
            {
                var hdnChanged = row.FindControl(hdnChangedId) as HiddenField;
                if (hdnChanged != null && hdnChanged.Value == "1")
                {
                    var ddlLevel = row.FindControl(ddlLevelId) as DropDownList;
                    var ddlExperience = row.FindControl(ddlExperienceId) as DropDownList;
                    var ddlLastUsed = row.FindControl(ddlLastUsedId) as DropDownList;
                    var clearLink = row.FindControl(lnkbtnClearId) as LinkButton;
                    var hdnId = row.FindControl(hdnIdId) as HiddenField;
                    int skillId = Convert.ToInt32(hdnId.Value);
                    bool isModified = true;
                    ApplyClearLinkStyle(clearLink, !(ddlLevel.SelectedIndex == 0 && ddlExperience.SelectedIndex == 0 && ddlLastUsed.SelectedIndex == 0));

                    if (Person.Skills.Count > 0 && Person.Skills.Where(skill => skill.Skill != null && skill.Skill.Id == Convert.ToInt32(hdnId.Value)).Count() > 0)
                    {
                        if (!(ddlLevel.SelectedIndex == 0 && ddlExperience.SelectedIndex == 0 && ddlLastUsed.SelectedIndex == 0))
                        {
                            var personSkill = Person.Skills.Where(skill => skill.Skill.Id == Convert.ToInt32(hdnId.Value)).First();
                            if (personSkill.SkillLevel.Id == Convert.ToInt32(ddlLevel.SelectedValue)
                                    && personSkill.YearsExperience.Value == Convert.ToInt32(ddlExperience.SelectedValue)
                                    && personSkill.LastUsed == Convert.ToInt32(ddlLastUsed.SelectedValue)
                                )
                            {
                                isModified = false;
                            }
                        }
                    }

                    if (isModified)
                    {
                        XmlElement skillTag = doc.CreateElement(SkillTag);

                        skillTag.SetAttribute(IdAttribute, hdnId.Value);
                        skillTag.SetAttribute(LevelAttribute, ddlLevel.SelectedValue);
                        skillTag.SetAttribute(ExperienceAttribute, ddlExperience.SelectedValue);
                        skillTag.SetAttribute(LastUsedAttribute, ddlLastUsed.SelectedValue);

                        root.AppendChild(skillTag);
                    }

                }
            }

            doc.AppendChild(root);

            if (root.HasChildNodes)
            {
                string skillsXml = doc.InnerXml;
                using (var serviceClient = new PersonSkillService.PersonSkillServiceClient())
                {
                    try
                    {
                        serviceClient.SavePersonSkills(Person.Id.Value, skillsXml, User.Identity.Name);
                        Person = null;

                        EnableSaveAndCancelButtons(false);
                        ClearDirty();
                    }
                    catch
                    {
                        serviceClient.Abort();
                    }
                }
            }
        }

        private void SaveIndustrySkills()
        {
            var rows = gvIndustrySkills.Rows;

            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement(Root);

            foreach (GridViewRow row in rows)
            {
                var hdnChanged = row.FindControl(hdnChangedId) as HiddenField;
                if (hdnChanged != null && hdnChanged.Value == "1")
                {
                    var ddlExperience = row.FindControl(ddlExperienceId) as DropDownList;
                    var hdnId = row.FindControl(hdnIdId) as HiddenField;
                    int industryId = Convert.ToInt32(hdnId.Value);
                    bool isModified = true;

                    if (Person.Industries.Count > 0 && Person.Industries.Where(industry => industry.Industry != null && industry.Industry.Id == Convert.ToInt32(hdnId.Value)).Count() > 0)
                    {
                        if (ddlExperience.SelectedIndex != 0)
                        {
                            var personIndustry = Person.Industries.Where(industry => industry.Industry.Id == Convert.ToInt32(hdnId.Value)).First();
                            if (personIndustry.YearsExperience == Convert.ToInt32(ddlExperience.SelectedValue))
                            {
                                isModified = false;
                            }
                        }
                    }

                    if (isModified)
                    {
                        XmlElement industryTag = doc.CreateElement(IndustrySkillTag);

                        industryTag.SetAttribute(IdAttribute, hdnId.Value);
                        industryTag.SetAttribute(ExperienceAttribute, ddlExperience.SelectedValue);

                        root.AppendChild(industryTag);
                    }
                }
            }

            doc.AppendChild(root);

            if (root.HasChildNodes)
            {
                string skillsXml = doc.InnerXml;
                using (var serviceClient = new PersonSkillService.PersonSkillServiceClient())
                {
                    try
                    {
                        serviceClient.SavePersonIndustrySkills(Person.Id.Value, skillsXml, User.Identity.Name);
                        Person = null;

                        EnableSaveAndCancelButtons(false);
                        ClearDirty();
                    }
                    catch
                    {
                        serviceClient.Abort();
                    }
                }
            }
        }

        /*
            <Profiles>
            <Profile Id='' ProfileName='' ProfileURL='' > </Profile>
            <Profile Id='' ProfileName='' ProfileURL='' > </Profile>
            ....
            </Profiles>
        */
        private string GetProfilesLinksXml()
        {
            StringBuilder profilesXml = new StringBuilder();
            profilesXml.Append("<Profiles>");
            foreach (var profile in PersonProfiles)
            {
                if (!string.IsNullOrEmpty(profile.ProfileName) && !string.IsNullOrEmpty(profile.ProfileUrl))
                {
                    int profileId = profile.ProfileId.HasValue && profile.ProfileId.Value > 0 ? profile.ProfileId.Value : -1;
                    string profileXml = string.Format(ProfileXml, profileId, HttpUtility.HtmlEncode(profile.ProfileName), HttpUtility.HtmlEncode(profile.ProfileUrl).Trim(), profile.IsDefault);
                    profilesXml.Append(profileXml);
                }
            }
            profilesXml.Append("</Profiles>");
            return profilesXml.ToString();

        }

        private void UpdatePersonProfilesFromRep(bool addNewRow)
        {
            var personProfiles = PersonProfiles;
            RepeaterItemCollection items = repProfiles.Items;

            foreach (RepeaterItem item in items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    var hdProfileId = item.FindControl("hdProfileId") as HiddenField;
                    var txtProfileName = item.FindControl("txtProfileName") as TextBox;
                    var txtProfileLink = item.FindControl("txtProfileLink") as TextBox;
                    var rbprofileIsDefault = item.FindControl("rbprofileIsDefault") as RadioButton;
                    int profileId = 0;
                    if (!string.IsNullOrEmpty(hdProfileId.Value) && int.TryParse(hdProfileId.Value, out profileId) && personProfiles.Any(p => p.ProfileId.HasValue && p.ProfileId.Value == profileId))
                    {
                        Profile profile = personProfiles.First(p => p.ProfileId.Value == profileId);
                        profile.ProfileName = txtProfileName.Text;
                        profile.ProfileUrl = txtProfileLink.Text;
                        profile.IsDefault = rbprofileIsDefault.Checked;
                    }
                }
            }
            if (addNewRow)
            {
                Profile profile = new Profile();
                profile.ProfileId = personProfiles.Count == 0 ? -1 : personProfiles.Any(p => p.ProfileId.HasValue && p.ProfileId.Value < 0) ? personProfiles.Min(p => p.ProfileId.HasValue ? p.ProfileId.Value : 0) - 1 : -1;
                personProfiles.Add(profile);
            }
            PersonProfiles = personProfiles;
        }

        #endregion

        #region ObjectDataSource Select Methods

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static List<NameValuePair> GetExperiences()
        {
            var experience = new List<NameValuePair>();
            for (var index = 0; index <= 30; index++)
            {
                var item = new NameValuePair();
                item.Id = index;
                if (index == 1)
                {
                    item.Name = "1 Year";
                }
                else if (index > 1)
                {
                    item.Name = index.ToString() + " Years";
                }

                experience.Add(item);
            }
            return experience;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static List<NameValuePair> GetLastUsedYears()
        {
            var years = new List<NameValuePair>();
            var currentYear = SettingsHelper.GetCurrentPMTime().Year;
            var emptyItem = new NameValuePair();
            emptyItem.Id = 0;
            years.Add(emptyItem);
            for (var index = currentYear; index >= 1990; index--)
            {
                var item = new NameValuePair();
                item.Id = index;
                item.Name = index.ToString();
                years.Add(item);
            }
            return years;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static List<SkillLevel> GetSkillLevels()
        {
            var Skills = new List<SkillLevel>();
            Skills.Add(new SkillLevel { Id = 0 });
            Skills.AddRange(SettingsHelper.GetSkillLevels());
            return Skills;
        }

        #endregion

        #region RaisePostBackEvents

        public void RaisePostBackEvent(string eventArgument)
        {
            if (IsDirty)
            {
                if (SaveDirty && !ValidateAndSave(PreviousActiveTabIndex))
                {
                    return;
                }
            }
            Redirect(eventArgument == string.Empty ? Constants.ApplicationPages.OpportunityList : eventArgument);
        }

        #endregion
    }
}

