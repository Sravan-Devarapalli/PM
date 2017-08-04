using DataTransferObjects;
using PraticeManagement.ConfigurationService;
using PraticeManagement.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PraticeManagement.Config
{
    public partial class MarginThreshold : System.Web.UI.Page
    {

        private bool ShowPopup;

        public List<ClientMarginThreshold> MarginThresholds
        {
            get
            {
                using (var serviceClient = new ConfigurationServiceClient())
                {
                    try
                    {
                        var result = serviceClient.GetMarginThresholds();
                        return result.ToList();
                    }
                    catch
                    {
                        serviceClient.Abort();
                        throw;
                    }
                }
            }
        }

        public List<ClientMarginException> MarginExceptionThresholds
        {
            get
            {
                using (var serviceClient = new ConfigurationServiceClient())
                {
                    try
                    {
                        var result = serviceClient.GetMarginExceptionThresholds();
                        return result.ToList();
                    }
                    catch
                    {
                        serviceClient.Abort();
                        throw;
                    }
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindMarginThresholdData();
                BindMarginExceptionData();
            }
        }

        private void BindMarginThresholdData()
        {
            repMarginThresholdHistory.DataSource = MarginThresholds;
            repMarginThresholdHistory.DataBind();
        }

        private void BindMarginExceptionData()
        {
            repMarginExceptionHistory.DataSource = MarginExceptionThresholds;
            repMarginExceptionHistory.DataBind();
        }

        protected void imgbtnUpdate_OnClick(object sender, EventArgs e)
        {
            Page.Validate(valsMarginTheshold.ValidationGroup);
            if (Page.IsValid)
            {
                var threshold = new ClientMarginThreshold
                {
                    StartDate = dtpPeriodFrom.DateValue,
                    EndDate = dtpPeriodTo.DateValue,
                    ThresholdVariance = int.Parse(tbVariance.Text)
                };

                using (var serviceClient = new ConfigurationServiceClient())
                {
                    serviceClient.InsertMarginThreshold(threshold);
                }
                BindMarginThresholdData();
                dtpPeriodFrom.TextValue = string.Empty;
                dtpPeriodTo.TextValue = string.Empty;
                tbVariance.Text = string.Empty;
            }
            else
            {
                ShowPopup = true;
                return;
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {

        }

        protected void custThresholdPeriod_ServerValidate(object source, ServerValidateEventArgs args)
        {
            bool isExists = true;
            var newThreshold = new ClientMarginThreshold
            {
                StartDate = dtpPeriodFrom.DateValue,
                EndDate = dtpPeriodTo.DateValue
            };

            foreach (var threshold in MarginThresholds)
            {
                if (threshold.StartDate <= newThreshold.EndDate && threshold.EndDate >= newThreshold.StartDate)
                {
                    args.IsValid = isExists = false;
                    return;
                }
            }
            args.IsValid = isExists;
        }

        protected void repMarginThresholdHistory_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var imgEditThreshold = e.Item.FindControl("imgEditThreshold") as ImageButton;
                var imgDelete = e.Item.FindControl("imgDeleteThreshold") as ImageButton;

                var today = Utils.Generic.GetNowWithTimeZone();
                var threshold = (ClientMarginThreshold)e.Item.DataItem;

                imgEditThreshold.Visible = (threshold.StartDate <= today && threshold.EndDate >= today) || (threshold.StartDate >= today && threshold.EndDate >= today);
                imgDelete.Visible = (threshold.StartDate > today && threshold.EndDate > today);
            }
        }

        protected void imgUpdateThreshold_Click(object sender, ImageClickEventArgs e)
        {
            var imgEdit = sender as ImageButton;
            var row = imgEdit.NamingContainer as RepeaterItem;
            var dtpPeriodFrom = row.FindControl("dtpPeriodFrom") as DatePicker;
            var dtpPeriodTo = row.FindControl("dtpPeriodTo") as DatePicker;
            var tbVariance = row.FindControl("tbVariance") as TextBox;
            var cv = row.FindControl("custThresholdPeriodUpdate") as CustomValidator;

            Page.Validate(valsMarginThresholdEdit.ValidationGroup);
            if (!Page.IsValid)
            {
                ShowPopup = true;
                return;
            }

            var newThreshold = new ClientMarginThreshold
            {
                Id = Convert.ToInt32(dtpPeriodFrom.Attributes["ThresholdId"]),
                StartDate = dtpPeriodFrom.DateValue,
                EndDate = dtpPeriodTo.DateValue,
                ThresholdVariance = Convert.ToInt32(tbVariance.Text)
            };

            bool isExists = true;

            foreach (var threshold in MarginThresholds)
            {
                if (threshold.Id.Value != newThreshold.Id.Value && threshold.StartDate <= newThreshold.EndDate && threshold.EndDate >= newThreshold.StartDate)
                {
                    isExists = false;
                    break;
                }
            }
            if (!isExists)
            {
                cv.IsValid = false;
                ShowPopup = true;
                return;
            }

            using (var serviceClient = new ConfigurationServiceClient())
            {
                serviceClient.UpdateMarginThreshold(newThreshold);
            }
            imgEdit.CommandName = "";
            BindMarginThresholdData();
        }

        protected void imgCancel_Click(object sender, ImageClickEventArgs e)
        {
            BindMarginThresholdData();
        }

        protected void repMarginThresholdHistory_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var imgUpdateThreshold = e.Item.FindControl("imgUpdateThreshold");
            var imgCancel = e.Item.FindControl("imgCancel");
            var dtpPeriodFrom = e.Item.FindControl("dtpPeriodFrom");
            var dtpPeriodTo = e.Item.FindControl("dtpPeriodTo");
            var tbVariance = e.Item.FindControl("tbVariance");
            var imgEditThreshold = e.Item.FindControl("imgEditThreshold");
            var lblPer = e.Item.FindControl("lblPer");

            var lblStartDate = e.Item.FindControl("lblStartDate");
            var lblEndDate = e.Item.FindControl("lblEndDate");
            var lblThreshold = e.Item.FindControl("lblThreshold");

            if (e.CommandName == "edit")
            {
                imgUpdateThreshold.Visible = imgCancel.Visible = dtpPeriodFrom.Visible = dtpPeriodTo.Visible = tbVariance.Visible = lblPer.Visible = true;
                lblStartDate.Visible = lblEndDate.Visible = lblThreshold.Visible = imgEditThreshold.Visible = false;
                return;
            }
            imgUpdateThreshold.Visible = imgCancel.Visible = dtpPeriodFrom.Visible = dtpPeriodTo.Visible = tbVariance.Visible = false;
            lblStartDate.Visible = lblEndDate.Visible = lblThreshold.Visible = imgEditThreshold.Visible = true;
        }

        protected void imgbtnAddExceptionThreshold_Click(object sender, EventArgs e)
        {
            Page.Validate(valsMarginExceptionThreshold.ValidationGroup);
            if (Page.IsValid)
            {
                var threshold = new ClientMarginException
                {
                    StartDate = dtpAddExcFrom.DateValue,
                    EndDate = dtpAddExcTo.DateValue,
                    Level = new ApprovalLevel { Id = Convert.ToInt32(ddlAddApprovalLevel.SelectedValue) },
                    MarginThreshold = Convert.ToInt32(txtAddMarginGoal.Text),
                    Revenue = Convert.ToDecimal(txtAddRevenue.Text)
                };

                using (var serviceClient = new ConfigurationServiceClient())
                {
                    serviceClient.InsertMarginExceptionThreshold(threshold);
                }
                BindMarginExceptionData();
                dtpAddExcFrom.TextValue = dtpAddExcTo.TextValue = txtAddMarginGoal.Text = txtAddRevenue.Text = string.Empty;
                ddlAddApprovalLevel.SelectedValue = "-1";
            }
            else
            {
                ShowPopup = true;
                return;
            }
        }

        protected void imgUpdateExceptionThreshold_Click(object sender, ImageClickEventArgs e)
        {
            var imgEdit = sender as ImageButton;
            var row = imgEdit.NamingContainer as RepeaterItem;
            var dtpExcFrom = row.FindControl("dtpExcFrom") as DatePicker;
            var dtpExcTo = row.FindControl("dtpExcTo") as DatePicker;
            var ddlApprovalLevel = row.FindControl("ddlApprovalLevel") as DropDownList;
            var txtMarginGoal = row.FindControl("txtMarginGoal") as TextBox;
            var txtRevenue = row.FindControl("txtRevenue") as TextBox;
            var cv = row.FindControl("custExceptionPeriod") as CustomValidator;

            Page.Validate(valsMarginExceptionThresholdEdit.ValidationGroup);
            if (!Page.IsValid)
            {
                ShowPopup = true;
                return;
            }

            var newThreshold = new ClientMarginException
            {
                Id = Convert.ToInt32(dtpExcFrom.Attributes["ExcThresholdId"]),
                StartDate = dtpExcFrom.DateValue,
                EndDate = dtpExcTo.DateValue,
                Level = new ApprovalLevel { Id = Convert.ToInt32(ddlApprovalLevel.SelectedValue) },
                MarginThreshold = Convert.ToInt32(txtMarginGoal.Text),
                Revenue = Convert.ToDecimal(txtRevenue.Text)
            };


            bool isExists = true;


            foreach (var threshold in MarginExceptionThresholds)
            {
                if (threshold.Id.Value != newThreshold.Id.Value && threshold.StartDate <= newThreshold.EndDate && threshold.EndDate >= newThreshold.StartDate && newThreshold.Level.Id == threshold.Level.Id)
                {
                    isExists = false;
                    break;
                }
            }
            if (!isExists)
            {
                cv.IsValid = false;
                ShowPopup = true;
                return;
            }

            using (var serviceClient = new ConfigurationServiceClient())
            {
                serviceClient.UpdateMarginExceptionThreshold(newThreshold);
            }
            imgEdit.CommandName = "";
            BindMarginExceptionData();
        }

        protected void imgExceptionCancel_Click(object sender, ImageClickEventArgs e)
        {
            BindMarginExceptionData();
        }

        protected void repMarginExceptionHistory_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var imgEditThreshold = e.Item.FindControl("imgEditExceptionThreshold") as ImageButton;
                var imgDelete = e.Item.FindControl("imgDeleteExceptionThreshold") as ImageButton;
                var today = Utils.Generic.GetNowWithTimeZone();
                var ddlApprovalLevel = e.Item.FindControl("ddlApprovalLevel") as DropDownList;
                var threshold = (ClientMarginException)e.Item.DataItem;

                ddlApprovalLevel.SelectedValue = threshold.Level.Id.ToString();
                imgEditThreshold.Visible = (threshold.StartDate <= today && threshold.EndDate >= today) || (threshold.StartDate >= today && threshold.EndDate >= today);
                imgDelete.Visible = (threshold.StartDate > today && threshold.EndDate > today);
            }
        }

        protected void repMarginExceptionHistory_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var imgUpdateExceptionThreshold = e.Item.FindControl("imgUpdateExceptionThreshold");
            var imgExceptionCancel = e.Item.FindControl("imgExceptionCancel");
            var imgEditExceptionThreshold = e.Item.FindControl("imgEditExceptionThreshold");
            var dtpExcFrom = e.Item.FindControl("dtpExcFrom");
            var dtpExcTo = e.Item.FindControl("dtpExcTo");
            var ddlApprovalLevel = e.Item.FindControl("ddlApprovalLevel");
            var txtMarginGoal = e.Item.FindControl("txtMarginGoal");
            var txtRevenue = e.Item.FindControl("txtRevenue");
            var lblDoller = e.Item.FindControl("lblDoller");
            var lblPer = e.Item.FindControl("lblPer");


            var lblStartDate = e.Item.FindControl("lblStartDate");
            var lblEndDate = e.Item.FindControl("lblEndDate");
            var lblLevel = e.Item.FindControl("lblLevel");
            var lblMarginGoal = e.Item.FindControl("lblMarginGoal");
            var lblRevenue = e.Item.FindControl("lblRevenue");

            if (e.CommandName == "edit")
            {
                imgUpdateExceptionThreshold.Visible = imgExceptionCancel.Visible = dtpExcFrom.Visible = dtpExcTo.Visible = ddlApprovalLevel.Visible = txtMarginGoal.Visible = txtRevenue.Visible = lblDoller.Visible = lblPer.Visible = true;
                lblStartDate.Visible = lblEndDate.Visible = lblLevel.Visible = lblMarginGoal.Visible = lblRevenue.Visible = imgEditExceptionThreshold.Visible = false;
                return;
            }
            imgUpdateExceptionThreshold.Visible = imgExceptionCancel.Visible = dtpExcFrom.Visible = dtpExcTo.Visible = ddlApprovalLevel.Visible = txtMarginGoal.Visible = txtRevenue.Visible = false;
            lblStartDate.Visible = lblEndDate.Visible = lblLevel.Visible = lblMarginGoal.Visible = lblRevenue.Visible = imgEditExceptionThreshold.Visible = true;
        }

        protected void custExceptionPeriod_ServerValidate(object source, ServerValidateEventArgs args)
        {
            bool isExists = true;
            var newThreshold = new ClientMarginException
            {
                StartDate = dtpAddExcFrom.DateValue,
                EndDate = dtpAddExcTo.DateValue,
                Level = new ApprovalLevel { Id = Convert.ToInt32(ddlAddApprovalLevel.SelectedValue) }
            };

            foreach (var threshold in MarginExceptionThresholds)
            {
                if (threshold.StartDate <= newThreshold.EndDate && threshold.EndDate >= newThreshold.StartDate && newThreshold.Level.Id == threshold.Level.Id)
                {
                    args.IsValid = isExists = false;
                    return;
                }
            }
            args.IsValid = isExists;
        }

        protected void imgDeleteThreshold_Click(object sender, ImageClickEventArgs e)
        {
            var imgDelete = sender as ImageButton;
            var row = imgDelete.NamingContainer as RepeaterItem;

            int id = int.Parse(((HiddenField)row.FindControl("hidKey")).Value);

            using (var serviceClient = new ConfigurationServiceClient())
            {
                serviceClient.DeleteMarginThreshold(id);
            }
            BindMarginThresholdData();
        }

        protected void imgDeleteExceptionThreshold_Click(object sender, ImageClickEventArgs e)
        {
            var imgDelete = sender as ImageButton;
            var row = imgDelete.NamingContainer as RepeaterItem;

            int id = int.Parse(((HiddenField)row.FindControl("hidKey")).Value);

            using (var serviceClient = new ConfigurationServiceClient())
            {
                serviceClient.DeleteMarginExceptionThreshold(id);
            }
            BindMarginExceptionData();
        }

        protected void custThresholdPeriodUpdate_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var custVal = source as CustomValidator;
            var row = custVal.NamingContainer as RepeaterItem;
            var dtpPeriodFrom = row.FindControl("dtpPeriodFrom") as DatePicker;
            var dtpPeriodTo = row.FindControl("dtpPeriodTo") as DatePicker;

            var newThreshold = new ClientMarginThreshold
            {
                Id = Convert.ToInt32(dtpPeriodFrom.Attributes["ThresholdId"]),
                StartDate = dtpPeriodFrom.DateValue,
                EndDate = dtpPeriodTo.DateValue,
            };

            bool isExists = true;

            foreach (var threshold in MarginThresholds)
            {
                if (threshold.StartDate <= newThreshold.EndDate && threshold.EndDate >= newThreshold.StartDate)
                {
                    custVal.IsValid = isExists = false;
                    return;
                }
            }
            custVal.IsValid = isExists;
        }
    }
}
