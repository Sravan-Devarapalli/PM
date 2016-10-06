﻿using System;
using PraticeManagement.Utils;

namespace PraticeManagement.Controls.Projects
{
    public partial class ProjectPersons : System.Web.UI.UserControl
    {
        public const string PERSON_TARGET = "person";

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected string GetDoubleFormat(decimal value)
        {
            return value.ToString(Constants.Formatting.DoubleValue);
        }

        #region Redirects

        protected string GetMilestonePersonRedirectUrl(object milestoneId, object milestonePersonId)
        {
            return Urls.GerMilestonePersonDetailsUrlWithReturn(milestoneId, milestonePersonId, Request.Url.AbsoluteUri);
        }

        #endregion
    }
}
