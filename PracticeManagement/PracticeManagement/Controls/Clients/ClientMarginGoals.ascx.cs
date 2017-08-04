using DataTransferObjects;
using PraticeManagement.ClientService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PraticeManagement.Controls.Clients
{
    public partial class ClientMarginGoals : System.Web.UI.UserControl
    {

        protected int? ClientId
        {
            get
            {
                try
                {
                    return Convert.ToInt32(Request.QueryString[Constants.QueryStringParameterNames.Id]);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private bool userIsAdministrator { get { return Roles.IsUserInRole(DataTransferObjects.Constants.RoleNames.AdministratorRoleName); ; } }

        public List<ClientMarginGoal> MarginGoals
        {
            get
            {
                if (ClientId.HasValue && ClientId.Value != 0)
                {
                    return ServiceCallers.Custom.Client(g => g.GetClientMarginGoals(ClientId.Value)).ToList();
                }
                return null;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            btnPlus.Visible = userIsAdministrator;
        }

        protected void repMarginGoal_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var imgUpdateMargin = e.Item.FindControl("imgUpdateMargin");
            var imgCancel = e.Item.FindControl("imgCancel");
            var imgEditMargin = e.Item.FindControl("imgEditMargin");
            var dtpPeriodFrom = e.Item.FindControl("dtpPeriodFrom");
            var dtpPeriodTo = e.Item.FindControl("dtpPeriodTo");
            var txtMarginGoal = e.Item.FindControl("txtMarginGoal");
            var txtComments = e.Item.FindControl("txtComments");

            var lblStartDate = e.Item.FindControl("lblStartDate");
            var lblEndDate = e.Item.FindControl("lblEndDate");
            var lblComments = e.Item.FindControl("lblComments");
            var lblMarginGoal = e.Item.FindControl("lblMarginGoal");

            if (e.CommandName == "edit")
            {
                imgUpdateMargin.Visible = imgCancel.Visible = dtpPeriodFrom.Visible = dtpPeriodTo.Visible = txtMarginGoal.Visible = txtComments.Visible = true;
                lblStartDate.Visible = lblEndDate.Visible = lblComments.Visible = lblMarginGoal.Visible = imgEditMargin.Visible = false;
                return;
            }
            imgUpdateMargin.Visible = imgCancel.Visible = dtpPeriodFrom.Visible = dtpPeriodTo.Visible = txtMarginGoal.Visible = txtComments.Visible = false;
            lblStartDate.Visible = lblEndDate.Visible = lblComments.Visible = lblMarginGoal.Visible = imgEditMargin.Visible = true;
        }

        protected void repMarginGoal_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var imgDelete = e.Item.FindControl("imgDelete") as ImageButton;
                var imgEditMargin = e.Item.FindControl("imgEditMargin") as ImageButton;
                var today = Utils.Generic.GetNowWithTimeZone();
                var marginGoal = (ClientMarginGoal)e.Item.DataItem;
                imgEditMargin.Visible = (userIsAdministrator) && (marginGoal.EndDate >= today);
                imgDelete.Visible = (marginGoal.StartDate > today && marginGoal.EndDate > today) && (userIsAdministrator);
            }
        }

        protected void imgUpdateMargin_Click(object sender, ImageClickEventArgs e)
        {
            Page.Validate("MarginGoal");

            var imgEdit = sender as ImageButton;
            var row = imgEdit.NamingContainer as RepeaterItem;
            if (Page.IsValid)
            {

                var dtpPeriodFrom = row.FindControl("dtpPeriodFrom") as DatePicker;
                var dtpPeriodTo = row.FindControl("dtpPeriodTo") as DatePicker;
                var txtMarginGoal = row.FindControl("txtMarginGoal") as TextBox;
                var txtComments = row.FindControl("txtComments") as TextBox;
             
                var custGoalPeriodUpdate = row.FindControl("custGoalPeriodUpdate") as CustomValidator;
                var rfvCommentsGrid = row.FindControl("rfvCommentsGrid") as RequiredFieldValidator;

                var defaultMarginGoal = DataHelper.GetDefaultMargin();
                rfvCommentsGrid.Enabled = Convert.ToInt32(txtMarginGoal.Text) != defaultMarginGoal.MarginGoal;

                if (Page.IsValid)
                {
                    var marginGoal = new ClientMarginGoal
                    {
                        Id = Convert.ToInt32(dtpPeriodFrom.Attributes["MarginId"]),
                        ClientId = ClientId.Value,
                        StartDate = dtpPeriodFrom.DateValue,
                        EndDate = dtpPeriodTo.DateValue,
                        MarginGoal = Convert.ToInt32(txtMarginGoal.Text),
                        Comments = txtComments.Text
                    };

                    bool isExists = true;
                    custGoalPeriodUpdate.Enabled = true;
                    foreach (var goal in MarginGoals)
                    {
                        if (goal.StartDate <= marginGoal.EndDate && goal.EndDate >= marginGoal.StartDate && goal.Id != marginGoal.Id)
                        {
                            isExists = false;
                            break;
                        }
                    }
                    if (!isExists)
                    {
                        custGoalPeriodUpdate.IsValid = false;
                    }
                    if (custGoalPeriodUpdate.IsValid)
                    {
                        using (var serviceClient = new ClientServiceClient())
                        {
                            serviceClient.UpdateClientMarginGoal(marginGoal, HttpContext.Current.User.Identity.Name);
                        }

                        DisplayMarginGoals(false);
                    }
                    else
                    {
                        imgEdit.CommandName = "edit";

                        btnPlus.Visible = !btnAddMarginGoal.Visible;
                        return;
                    }
                }
                else
                {
                    imgEdit.CommandName = "edit";

                    btnPlus.Visible = !btnAddMarginGoal.Visible;
                    return;
                }
            }
            else
            {

                imgEdit.CommandName = "edit";

                btnPlus.Visible = !btnAddMarginGoal.Visible;
                return;
            }

        }

        protected void imgCancel_Click(object sender, ImageClickEventArgs e)
        {
            DisplayMarginGoals(false);
        }

        protected void btnPlus_Click(object sender, ImageClickEventArgs e)
        {
            plusMakeVisible(false);
        }

        protected void btnAddMarginGoal_Click(object sender, ImageClickEventArgs e)
        {
            Page.Validate(valSumGroups.ValidationGroup);
            compVariance.Validate();
            if (Page.IsValid)
            {
                custGoalPeriod.Enabled = true;
                custGoalPeriod.Validate();
                if (custGoalPeriod.IsValid)
                {
                    if (ClientId.HasValue && ClientId.Value != 0)
                    {
                        var marginGoal = new ClientMarginGoal
                        {
                            ClientId = ClientId.Value,
                            StartDate = dtpAddPeriodFrom.DateValue,
                            EndDate = dtpAddPeriodTo.DateValue,
                            MarginGoal = Convert.ToInt32(txtAddMarginGoal.Text),
                            Comments = txtAddComments.Text
                        };

                        using (var serviceClient = new ClientServiceClient())
                        {
                            try
                            {
                                serviceClient.InsertClientMargin(marginGoal, HttpContext.Current.User.Identity.Name);
                                plusMakeVisible(true);
                            }
                            catch
                            {
                                serviceClient.Abort();
                            }
                        }
                    }
                    DisplayMarginGoals(false);
                }
                else
                {
                    btnPlus.Visible = !btnAddMarginGoal.Visible;
                }
            }
            else
            {
                btnPlus.Visible = !btnAddMarginGoal.Visible;
            }
        }



        protected void btnCancel_Click(object sender, ImageClickEventArgs e)
        {
            plusMakeVisible(true);
        }

        private void plusMakeVisible(bool isplusVisible)
        {
            if (isplusVisible && userIsAdministrator)
            {
                btnPlus.Visible = true;
                btnAddMarginGoal.Visible = false;
                btnCancel.Visible = false;
                dtpAddPeriodFrom.Visible = dtpAddPeriodTo.Visible = txtAddMarginGoal.Visible = txtAddComments.Visible = false;
            }
            else
            {
                btnPlus.Visible = false;
                btnAddMarginGoal.Visible = true;
                btnCancel.Visible = true;
                dtpAddPeriodFrom.Visible = dtpAddPeriodTo.Visible = txtAddMarginGoal.Visible = txtAddComments.Visible = true;
                dtpAddPeriodFrom.TextValue = dtpAddPeriodTo.TextValue = txtAddMarginGoal.Text = txtAddComments.Text = string.Empty;
                DisplayMarginGoals(false);
            }
        }

        public void DisplayMarginGoals(bool isFromMainPage)
        {
            if (isFromMainPage)
            {
                plusMakeVisible(true);
            }
            var today = Utils.Generic.GetNowWithTimeZone();

            var currentGoals = MarginGoals.OrderBy(m => m.StartDate);

            repMarginGoal.DataSource = currentGoals;
            repMarginGoal.DataBind();
        }

        protected void lnkMarginGoalsHistory_Click(object sender, EventArgs e)
        {
            btnPlus.Visible = !btnAddMarginGoal.Visible;
            var today = Utils.Generic.GetNowWithTimeZone();
            List<ClientMarginGoalLog> historyGoals = null;
            if (ClientId.HasValue && ClientId.Value != 0)
            {
                historyGoals = ServiceCallers.Custom.Client(c => c.GetMarginGoalLogForClient(ClientId.Value)).ToList();
                historyGoals = historyGoals.OrderBy(h => h.LogTime).ToList();
            }
            repMarginGoalsHistory.DataSource = historyGoals;
            repMarginGoalsHistory.DataBind();
            mpeHistory.Show();
        }

        protected void imgDelete_Click(object sender, ImageClickEventArgs e)
        {
            var imgDelete = sender as ImageButton;
            var row = imgDelete.NamingContainer as RepeaterItem;

            int id = int.Parse(((HiddenField)row.FindControl("hidKey")).Value);

            using (var serviceClient = new ClientServiceClient())
            {
                try
                {
                    serviceClient.DeleteClientMarginGoal(id, HttpContext.Current.User.Identity.Name);
                    plusMakeVisible(true);
                }
                catch
                {
                    serviceClient.Abort();
                }
            }
            DisplayMarginGoals(false);
        }

        protected void custGoalPeriod_ServerValidate(object source, ServerValidateEventArgs args)
        {
            bool isExists = true;
            var newThreshold = new ClientMarginGoal
            {
                StartDate = dtpAddPeriodFrom.DateValue,
                EndDate = dtpAddPeriodTo.DateValue,
                MarginGoal = int.Parse(txtAddMarginGoal.Text)
            };

            foreach (var goal in MarginGoals)
            {
                if (goal.StartDate <= newThreshold.EndDate && goal.EndDate >= newThreshold.StartDate)
                {
                    args.IsValid = isExists = false;
                    return;
                }
            }
            args.IsValid = isExists;
        }
    }
}
