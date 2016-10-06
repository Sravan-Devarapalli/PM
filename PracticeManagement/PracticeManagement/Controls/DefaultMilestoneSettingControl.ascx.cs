﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PraticeManagement.Controls;
using PraticeManagement.Configuration;
using DataTransferObjects;

namespace PraticeManagement.Controls
{
    public partial class DefaultMilestoneSettingControl : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Display();
        }
        protected  void Display()
        {
            if (!IsPostBack)
            {
                DataHelper.FillClientList(ddlClients, "Please Select an Account");
                this.txtLowerBound.Text = MileStoneConfigurationManager.GetLowerBound().ToString();
                this.txtUpperBound.Text = MileStoneConfigurationManager.GetUpperBound().ToString();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Page.Validate("LowerUpperBounds");
                if (Page.IsValid)
                {
                    //int lowerBound = Math.Abs(Convert.ToInt32(this.txtLowerBound.Text));
                    //int upperBound = Math.Abs(Convert.ToInt32(this.txtUpperBound.Text));
                    Milestone selectedMileStone = DataHelper.GetMileStoneById(Convert.ToInt32(ddlMileStones.SelectedValue));

                    using (var milestoneService = new MilestoneService.MilestoneServiceClient())
                    {
                        int clientId = Int32.Parse(ddlClients.SelectedValue);
                        int projectId = Int32.Parse(ddlProjects.SelectedValue);
                        int milestoneId = Int32.Parse(ddlMileStones.SelectedValue);
                        milestoneService.SaveDefaultMilestone(clientId, projectId, milestoneId, null, null);
                        MileStoneConfigurationManager.ClearDefaultMileStoneSettingsinCache();
                    }

                    lblSuccessMessage.Text = string.Format("Successfully saved \"{0}\".", selectedMileStone.Description);
                    lblSuccessMessage.Visible = true;
                }
            }
            catch (Exception ex)
            {
                cvError.IsValid = false;
                cvError.ErrorMessage = @"There is a problem with accessing resources. Please contact Administrator.";
            }
        }

        #region "Validation"

        protected void cvClients_Validate(object source, ServerValidateEventArgs args)
        {
            if (string.IsNullOrEmpty(ddlClients.SelectedValue.Trim()))
            {
                args.IsValid = false;
            }
        }

        protected void cvProjects_Validate(object source, ServerValidateEventArgs args)
        {
            if (string.IsNullOrEmpty(ddlProjects.SelectedValue.Trim()))
            {
                args.IsValid = false;
            }
        }

        protected void cvMileStones_Validate(object source, ServerValidateEventArgs args)
        {
            if (string.IsNullOrEmpty(ddlMileStones.SelectedValue.Trim()))
            {
                args.IsValid = false;
            }
        }

        protected void btnSaveBounds_OnClick(object sender, EventArgs e)
        {
            try
            {
                Page.Validate("LowerUpperBounds");
                if (Page.IsValid)
                {
                    int lowerBound = Math.Abs(Convert.ToInt32(this.txtLowerBound.Text));
                    int upperBound = Math.Abs(Convert.ToInt32(this.txtUpperBound.Text));

                    using (var milestoneService = new MilestoneService.MilestoneServiceClient())
                    {
                        milestoneService.SaveDefaultMilestone(null, null, null, lowerBound, upperBound);
                        MileStoneConfigurationManager.ClearDefaultMileStoneSettingsinCache();
                    }

                }
            }
            catch (Exception ex)
            {
                cvError.IsValid = false;
                cvError.ErrorMessage = @"There is a problem with accessing resources. Please contact Administrator.";
            }
        }
        #endregion "Validation"
    }
}

