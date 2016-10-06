﻿using System;
using System.Linq;
using System.ServiceModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using DataTransferObjects;
using PraticeManagement.Controls;
using PraticeManagement.PersonService;

namespace PraticeManagement
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Web;
    using System.Web.Security;
    using Resources;
    using Utils;

    public partial class CompensationDetail : PracticeManagementPageBase
    {
        #region Constants

        private const string StartDateArgument = "StartDate";
        private const string IsStawman = "Isstrawman";

        #endregion

        #region Fields

        private ExceptionDetail internalException;
        private int _saveCode;
        private bool? _userIsAdministratorValue;
        private bool? _userIsHRValue;
        private bool IsErrorPanelDisplay;
        private bool IsOtherPanelDisplay;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value from the StartDate query string argument.
        /// </summary>
        protected DateTime? SelectedStartDate
        {
            get
            {
                return GetArgumentDateTime(StartDateArgument);
            }
        }

        protected bool? SelectedStawman
        {
            get
            {
                return GetArgumentInt32(IsStawman) != null ? (bool?)(GetArgumentInt32(IsStawman) == 1) : null;
            }
        }

        public bool ValidateAttribution
        {
            get
            {
                if (ViewState["ValidateAttribution_Key"] == null)
                    ViewState["ValidateAttribution_Key"] = true;
                return (bool)ViewState["ValidateAttribution_Key"];
            }
            set
            {
                ViewState["ValidateAttribution_Key"] = value;
            }
        }

        private List<Pay> PayHistory
        {
            get
            {
                return ViewState["PAY_HISTORY"] as List<Pay>;
            }
            set
            {
                ViewState["PAY_HISTORY"] = value;
            }
        }

        protected Person PersonDetailData
        {
            get
            {
                return (Person)ViewState["PersonDetailPageData"];
            }
            set
            {
                ViewState["PersonDetailPageData"] = value;
            }
        }

        protected PersonPermission Permissions
        {
            get
            {
                return (PersonPermission)ViewState["PersonPermissions_ViewState"];
            }
            set
            {
                ViewState["PersonPermissions_ViewState"] = value;
            }
        }

        private bool isRehire
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the current user is in the Administrator role.
        /// </summary>
        protected bool UserIsAdministrator
        {
            get
            {
                if (!_userIsAdministratorValue.HasValue)
                {
                    _userIsAdministratorValue =
                        Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.AdministratorRoleName);
                }

                return _userIsAdministratorValue.Value;
            }
        }

        protected bool UserIsHR
        {
            get
            {
                if (!_userIsHRValue.HasValue)
                {
                    _userIsHRValue =
                        Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.HRRoleName);
                }

                return _userIsHRValue.Value;
            }
        }

        #endregion

        #region Events

        #region Page Events

        protected void Page_Load(object sender, EventArgs e)
        {
            mlConfirmation.ClearMessage();

            if (SelectedStawman.HasValue && SelectedStawman.Value)
            {
                personnelCompensation.IsStrawmanMode = SelectedStawman.Value;
            }
        }

        protected void Page_Prerender(object sender, EventArgs e)
        {
            if (SelectedStawman.HasValue && SelectedStawman.Value)
            {
                personnelCompensation.ShowDates();
                personnelCompensation.StartDateReadOnly = true;
                personnelCompensation.EndDateReadOnly = true;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            IsOtherPanelDisplay = personnelCompensation.SLTApprovalPopupDisplayed ? true : IsOtherPanelDisplay;
            if (IsErrorPanelDisplay && !IsOtherPanelDisplay)
            {
                PopulateErrorPanel();
            }
        }

        #endregion

        #region Validation

        protected void cvEmployeePayTypeChangeViolation_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            var validator = ((CustomValidator)sender);
            DateTime enddate = personnelCompensation.EndDate.HasValue ? personnelCompensation.EndDate.Value : new DateTime(2029, 12, 31);
            e.IsValid = !ServiceCallers.Custom.Person(p => p.IsPersonTimeOffExistsInSelectedRangeForOtherthanGivenTimescale(SelectedId.Value, personnelCompensation.StartDate.Value, enddate, (int)personnelCompensation.Timescale));
            if (!e.IsValid)
            {
                validator.Text = validator.ToolTip = validator.ErrorMessage = PersonDetail.EmployeePayTypeChangeVoilationMessage;
                mpeEmployeePayTypeChange.Show();
                IsOtherPanelDisplay = true;
            }
        }

        protected void custUserName_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = _saveCode == default(int);

            string message;
            switch (-_saveCode)
            {
                case (int)MembershipCreateStatus.DuplicateEmail:
                    message = Messages.DuplicateEmail;
                    break;
                case (int)MembershipCreateStatus.DuplicateUserName:
                    //  Because we're using email as username in the system,
                    //      DuplicateUserName is equal to our PersonEmailUniquenesViolation
                    message = Messages.DuplicateEmail;
                    break;
                case (int)MembershipCreateStatus.InvalidAnswer:
                    message = Messages.InvalidAnswer;
                    break;
                case (int)MembershipCreateStatus.InvalidEmail:
                    message = Messages.InvalidEmail;
                    break;
                case (int)MembershipCreateStatus.InvalidPassword:
                    message = Messages.InvalidPassword;
                    break;
                case (int)MembershipCreateStatus.InvalidQuestion:
                    message = Messages.InvalidQuestion;
                    break;
                case (int)MembershipCreateStatus.InvalidUserName:
                    message = Messages.InvalidUserName;
                    break;
                case (int)MembershipCreateStatus.ProviderError:
                    message = Messages.ProviderError;
                    break;
                case (int)MembershipCreateStatus.UserRejected:
                    message = Messages.UserRejected;
                    break;
                default:
                    message = custUserName.ErrorMessage;
                    return;
            }
            custUserName.ErrorMessage = custUserName.ToolTip = message;
        }

        protected void cvRehireConfirmation_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            e.IsValid = true;
            var validator = (CustomValidator)sender;
            validator.ErrorMessage = PersonDetail.ReHireMessage;
            var person = ServiceCallers.Custom.Person(p => p.GetPersonDetailsShort(SelectedId.Value));
            var payHistory = ServiceCallers.Custom.Person(p => p.GetHistoryByPerson(SelectedId.Value)).ToList();
            if (payHistory != null && payHistory.Any(p => p.StartDate >= person.HireDate))
            {
                payHistory = payHistory.OrderBy(p => p.StartDate).ToList();
                PopulateUnCommitedPay(payHistory);
                Pay firstPay = payHistory.Where(p => p.StartDate >= person.HireDate).FirstOrDefault();
                if (firstPay != null && (firstPay.Timescale == TimescaleType.PercRevenue || firstPay.Timescale == TimescaleType._1099Ctc))
                {
                    if (payHistory.Any(p => p.StartDate > firstPay.StartDate && p.StartDate <= DateTime.Now.Date && (p.Timescale == TimescaleType.Salary || p.Timescale == TimescaleType.Hourly)))
                    {
                        e.IsValid = false;
                    }
                }
            }

            if (!e.IsValid)
            {
                mpeRehireConfirmation.Show();
                IsOtherPanelDisplay = true;
            }

            validator.Text = validator.ToolTip = validator.ErrorMessage;
        }

        #endregion

        #region mpeRehireConfirmation Events

        protected void btnRehireConfirmationOk_Click(object sender, EventArgs e)
        {
            cvRehireConfirmation.Enabled = false;
            isRehire = true;
            btnSave_Click(btnSave, new EventArgs());
            mpeRehireConfirmation.Hide();
            cvRehireConfirmation.Enabled = true;
        }

        protected void btnRehireConfirmationCancel_Click(object source, EventArgs args)
        {
            if (SelectedStartDate.HasValue)
            {
                Person person = ServiceCallers.Custom.Person(p => p.GetPersonDetail(SelectedId.Value));
                Pay pay = person.PaymentHistory.First(pa => pa.StartDate.Date == SelectedStartDate.Value.Date);
                SetSaveButtonVisibility(person, pay);
                PopulateControls(pay);
            }
            cvRehireConfirmation.Enabled = true;
            mpeRehireConfirmation.Hide();
        }

        #endregion
        protected void btnOkConsultantToContract_Click(object sender, EventArgs e)
        {
            ValidateAttribution = false;
            btnSave_Click(btnSave, new EventArgs());
            mpeConsultantToContract.Hide();
        }

        protected void btnCloseConsultantToContract_Click(object sender, EventArgs e)
        {
            mpeConsultantToContract.Hide();
        }


        #region personnelCompensation Events

        protected void personnelCompensation_CompensationMethodChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        protected void personnelCompensation_SaveDetails(object sender, EventArgs e)
        {
            btnSave_Click(btnSave, null);
        }

        protected void personnelCompensation_PeriodChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        #endregion

        #region mpeEmployeePayTypeChange Events

        protected void btnEmployeePayTypeChangeViolationOk_Click(object sender, EventArgs e)
        {
            cvEmployeePayTypeChangeViolation.Enabled = false;
            btnSave_Click(btnSave, new EventArgs());
            mpeEmployeePayTypeChange.Hide();
            cvEmployeePayTypeChangeViolation.Enabled = true;
        }

        protected void btnEmployeePayTypeChangeViolationCancel_Click(object source, EventArgs args)
        {
            if (SelectedStartDate.HasValue)
            {

                Person person = ServiceCallers.Custom.Person(p => p.GetPersonDetail(SelectedId.Value));
                Pay pay = person.PaymentHistory.First(pa => pa.StartDate.Date == SelectedStartDate.Value.Date);
                SetSaveButtonVisibility(person, pay);
                PopulateControls(pay);
            }
            cvEmployeePayTypeChangeViolation.Enabled = true;
            mpeEmployeePayTypeChange.Hide();
        }

        #endregion

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateAndSave())
            {
                if (PersonDetailData != null)
                {
                    if (_saveCode == default(int))
                    {
                        ClearDirty();

                        //var returnUrl = Request.Url.AbsoluteUri.Substring(Request.Url.AbsoluteUri.LastIndexOf("&returnTo="));
                        var returnUrl = Request.UrlReferrer.ToString();
                        if (returnUrl.LastIndexOf("&returnTo=") != -1)
                            returnUrl = returnUrl.Substring(returnUrl.LastIndexOf("&returnTo="));
                        string redirectUrl = "PersonDetail.aspx?id=" + PersonDetailData.Id + "&ShowConfirmMessage=1";
                        redirectUrl = redirectUrl + (returnUrl.Contains("persons.aspx") ? returnUrl : string.Empty);

                        Response.Redirect(redirectUrl);

                        //Server.Transfer("~" + ReturnUrl.Substring(ReturnUrl.IndexOf("/PersonDetail.aspx?id="), ReturnUrl.Length - ReturnUrl.IndexOf("/PersonDetail.aspx?id=")));
                    }
                }
                else
                {
                    ClearDirty();
                    if (SelectedId.HasValue)
                    {
                        ReturnToPreviousPage();
                    }
                }
            }
            else
            {
                if (cvEmployeePayTypeChangeViolation.IsValid && personnelCompensation.IsDivisionOrPracticeOwner)
                {
                    Page.Validate(vsumCompensation.ValidationGroup);
                    if (Page.IsValid)
                    {
                        if (internalException != null)
                        {
                            string data = internalException.ToString();
                            string innerexceptionMessage = internalException.InnerException.Message;
                            if (data.Contains("CK_Pay_DateRange"))
                            {
                                mlConfirmation.ShowErrorMessage("Compensation for the same period already exists.");
                                IsErrorPanelDisplay = true;
                            }
                            else if (data.Contains("Attribution Error:"))
                            {
                                mpeConsultantToContract.Show();
                                IsOtherPanelDisplay = true;
                                lblPersonName.Text = personInfo.LastName + ", " + personInfo.FirstName;
                                int length = "Attribution Error:".Length;
                                string attributionIds = innerexceptionMessage.Substring(length);
                                dlCommissions.DataSource =
                                    ServiceCallers.Custom.Project(p => p.GetAttributionForGivenIds(attributionIds));
                                dlCommissions.DataBind();
                            }
                            else if (innerexceptionMessage == PersonDetail.StartDateIncorrect || innerexceptionMessage == PersonDetail.EndDateIncorrect || innerexceptionMessage == PersonDetail.PeriodIncorrect || innerexceptionMessage == PersonDetail.HireDateInCorrect || innerexceptionMessage == PersonDetail.SalaryToContractException)
                            {
                                if (innerexceptionMessage == PersonDetail.SalaryToContractException)
                                    mlConfirmation.ShowErrorMessage(PersonDetail.SalaryToContractMessage);
                                else
                                    mlConfirmation.ShowErrorMessage(innerexceptionMessage);
                                IsErrorPanelDisplay = true;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void SetSaveButtonVisibility(Person person, Pay pay)
        {
            var now = Utils.Generic.GetNowWithTimeZone();
            DateTime? _editablePayStartDate = null;
            //if person status is active or terminated pending or contigent and does not have current pay then we need to show last pay as editable.
            if ((person.Status.Id != (int)PersonStatusType.Terminated)
                && !person.PaymentHistory.Any(p => p.StartDate <= now.Date && (!p.EndDate.HasValue || (p.EndDate.HasValue && now.Date <= p.EndDate.Value.AddDays(-1)))))
            {
                Pay editPay = person.PaymentHistory.OrderByDescending(p => p.StartDate).FirstOrDefault(p => p.StartDate < now.Date);
                _editablePayStartDate = editPay != null && editPay.StartDate >= person.HireDate.Date ? editPay.StartDate : (DateTime?)null;
            }

            btnSave.Visible = (pay.EndDate.HasValue) ? !((pay.EndDate.Value.AddDays(-1) < now.Date) || (person.Status.Id == (int)PersonStatusType.Terminated)) || (_editablePayStartDate.HasValue && _editablePayStartDate.Value == pay.StartDate) : true;

        }
        protected override void Display()
        {
            using (PersonServiceClient serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person person;
                    if (PreviousPage != null && PreviousPage.PersonUnsavedData != null)
                    {
                        person = PersonDetailData = PreviousPage.PersonUnsavedData;
                        Permissions = PreviousPage.Permissions;
                    }
                    else
                    {
                        person = serviceClient.GetPersonDetail(SelectedId.Value);
                    }

                    if (SelectedStartDate.HasValue)
                    {
                        Pay pay = person.PaymentHistory.First(pa => pa.StartDate.Date == SelectedStartDate.Value.Date);
                        SetSaveButtonVisibility(person, pay);
                        PopulateControls(pay);
                    }
                    else
                    {
                        if (person.PaymentHistory.Count == 0 || (PreviousPage != null && PersonDetailData != null))
                        {
                            personnelCompensation.StartDate = person.HireDate;
                            if (person.Title != null)
                            {
                                personnelCompensation.TitleId = person.Title.TitleId;
                            }
                            if ((int)person.DivisionType != 0)
                            {
                                personnelCompensation.DivisionId = (int)person.DivisionType;
                            }
                            if (person.DefaultPractice != null)
                            {
                                personnelCompensation.PracticeId = person.DefaultPractice.Id;
                            }

                            personnelCompensation.StartDateReadOnly = true;
                        }
                        else
                        {
                            personnelCompensation.StartDate = DateTime.Today;
                        }
                    }

                    personInfo.Person = person;
                }
                catch (FaultException<ExceptionDetail>)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        protected override bool ValidateAndSave()
        {
            bool result = false;
            Page.Validate(vsumCompensation.ValidationGroup);
            if (Page.IsValid && cvEmployeePayTypeChangeViolation.Enabled && SelectedId.HasValue && PersonDetailData == null)
            {
                cvEmployeePayTypeChangeViolation.Validate();
            }
            if (Page.IsValid && cvRehireConfirmation.Enabled && SelectedId.HasValue && PersonDetailData == null)
            {
                cvRehireConfirmation.Validate();
            }
            if (Page.IsValid)
            {
                result = SaveData();
            }
            IsErrorPanelDisplay = !Page.IsValid;
            return result;
        }

        private void PopulateErrorPanel()
        {
            mpeErrorPanel.Show();
        }

        private void PopulateControls(Pay pay)
        {
            personnelCompensation.StartDate = pay.StartDate;
            personnelCompensation.EndDate = pay.EndDate;
            personnelCompensation.Timescale = pay.Timescale;
            personnelCompensation.Amount = pay.Amount;
            personnelCompensation.VacationDays = pay.VacationDays;
            personnelCompensation.IsYearBonus = pay.IsYearBonus;
            personnelCompensation.BonusAmount = pay.BonusAmount;
            personnelCompensation.BonusHoursToCollect = pay.BonusHoursToCollect;
            personnelCompensation.DivisionId = pay.DivisionId;
            personnelCompensation.PracticeId = pay.PracticeId;
            personnelCompensation.TitleId = pay.TitleId;
            personnelCompensation.SLTApproval = pay.SLTApproval;
            personnelCompensation.SLTPTOApproval = pay.SLTPTOApproval;
            personnelCompensation.VendorId = pay.vendor!=null?pay.vendor.Id:null;
        }

        private bool SaveData()
        {
            Pay pay = personnelCompensation.Pay;

            pay.PersonId = SelectedId.Value;
            pay.ValidateAttribution = ValidateAttribution;
            using (PersonServiceClient serviceClient = new PersonServiceClient())
            {
                try
                {
                    //Pay oldPay = null;
                    Person oldPersonPay = serviceClient.GetPersonDetail(SelectedId.Value);
                    PayHistory = oldPersonPay.PaymentHistory;
                    PayHistory = PayHistory.OrderBy(p => p.StartDate.Date).ToList();
                    if (SelectedStartDate.HasValue)
                    {
                        oldPersonPay.CurrentPay = PayHistory.First(p => p.StartDate.Date == SelectedStartDate.Value.Date);
                    }
                    else if (PayHistory.Count > 0)
                    {
                        oldPersonPay.CurrentPay = PayHistory.Last();
                    }

                    if (oldPersonPay.CurrentPay.Timescale == TimescaleType.Salary && pay.Timescale != TimescaleType.Salary)
                    {
                        personnelCompensation.IsDivisionOrPracticeOwner = serviceClient.CheckIfPersonIsOwnerForDivisionAndOrPractice(SelectedId.Value) == null;
                    }
                    else {
                        personnelCompensation.IsDivisionOrPracticeOwner = true;
                    }

                    if (!Page.IsValid)
                    {
                        return false;
                    }
                    if (SelectedStawman.HasValue && SelectedStawman.Value)
                    {
                        var person = new Person { Id = pay.PersonId, FirstName = personInfo.FirstName, LastName = personInfo.LastName };
                        serviceClient.SaveStrawman(person, pay, HttpContext.Current.User.Identity.Name);
                    }
                    else
                    {
                        if (PersonDetailData != null)
                        {
                            var person = PersonDetailData;
                            person.CurrentPay = pay;
                            Person oldPerson = null;
                            if (person.Id.HasValue)
                            {
                                oldPerson = serviceClient.GetPersonDetailsShort(person.Id.Value);
                            }
                            string[] currentRoles = Roles.GetRolesForUser(person.Alias);
                            if (oldPerson != null)
                            {
                                if (currentRoles.Length == 0)
                                    currentRoles = Roles.GetRolesForUser(oldPerson.Alias);
                                oldPerson.RoleNames = currentRoles;
                            }
                            int? personId = serviceClient.SavePersonDetail(person, User.Identity.Name, LoginPageUrl, true, Page.User.Identity.Name);

                            PersonDetail.SaveRoles(person, currentRoles);
                            serviceClient.SendAdministratorAddedEmail(person, oldPerson);
                            if (personId.Value < 0)
                            {
                                // Creating User error
                                _saveCode = personId.Value;
                                Page.Validate(custUserName.ValidationGroup);

                                return true;
                            }

                            SavePersonsPermissions(person, serviceClient);

                            if (!PersonDetailData.Id.HasValue)
                                PersonDetailData.Id = personId.Value;
                            IsDirty = false;
                            personnelCompensation.SaveAllLockdownViewStates();
                        }
                        else
                        {
                            serviceClient.SavePay(pay, LoginPageUrl, HttpContext.Current.User.Identity.Name);
                        }

                        ValidateAttribution = true;
                        personnelCompensation.StartDate = personnelCompensation.StartDate;
                        personnelCompensation.EndDate = personnelCompensation.EndDate;
                    }
                    //if (oldPersonPay.Status.ToStatusType() != PersonStatusType.Terminated)
                    //{
                        
                    //    serviceClient.SendCompensationChangeEmail(oldPersonPay, oldPersonPay.CurrentPay, pay, isRehire);
                    //}
                    return true;

                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    internalException = ex.Detail;
                    string data = internalException.ToString();
                    serviceClient.Abort();
                    string exceptionMessage = internalException.InnerException != null ? internalException.InnerException.Message : string.Empty;
                    if (!(data.Contains("CK_Pay_DateRange") || exceptionMessage == PersonDetail.StartDateIncorrect || exceptionMessage == PersonDetail.EndDateIncorrect || exceptionMessage == PersonDetail.PeriodIncorrect || exceptionMessage == PersonDetail.HireDateInCorrect || exceptionMessage == PersonDetail.SalaryToContractException || exceptionMessage.Contains("Attribution Error:")))
                    {
                        Logging.LogErrorMessage(
                            ex.Message,
                            ex.Source,
                            internalException.InnerException != null ? internalException.InnerException.Message : string.Empty,
                            string.Empty,
                            HttpContext.Current.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped),
                            string.Empty,
                            Thread.CurrentPrincipal.Identity.Name);
                    }
                    return false;
                }
            }
        }

        private void SavePersonsPermissions(Person person, PersonServiceClient serviceClient)
        {
            if (UserIsAdministrator || UserIsHR)
            {
                serviceClient.SetPermissionsForPerson(person, Permissions);
            }
        }

        private void PopulateUnCommitedPay(List<Pay> payHistory)
        {
            Pay pay = (Pay)personnelCompensation.Pay.Clone();
            if (SelectedStartDate.HasValue)
            {
                int index = payHistory.FindIndex(p => p.StartDate.Date == SelectedStartDate.Value.Date);
                payHistory[index] = pay;
            }
            else
            {
                payHistory.Add(pay);
            }
        }

        #endregion
    }
}

