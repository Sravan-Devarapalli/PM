using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using AjaxControlToolkit;
using System.Web.UI.WebControls;
using System.ComponentModel;


[assembly: WebResource("PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender.js", "text/javascript")]

namespace PraticeManagement.Controls.Generic.BillableNonBillableAndTotal
{
    [ClientScriptResource(
       "PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender",
       "PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender.js")]
    [TargetControlType(typeof(Control))]
    public class BillableNonBillableAndTotalExtender : ExtenderControlBase
    {

        [ExtenderControlProperty]
        [DefaultValue("")]
        [ClientPropertyName("controlsToCheck")]
        public string ControlsToCheck
        {
            get { return GetPropertyValue("controlsToCheck", string.Empty); }
            set { SetPropertyValue("controlsToCheck", value); }
        }

        [ExtenderControlProperty]
        [DefaultValue("")]
        [ClientPropertyName("targetContolsToCheck")]
        public string TargetControlsToCheck
        {
            get { return GetPropertyValue("targetContolsToCheck", string.Empty); }
            set { SetPropertyValue("targetContolsToCheck", value); }
        }

    }
}