using System.Web.UI;
using AjaxControlToolkit;

[assembly: WebResource("PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorExtender.js", "text/javascript")]

namespace PraticeManagement.Controls.Generic.TotalCalculator
{
    using System.ComponentModel;
   
    [ClientScriptResource("PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorBehavior", "PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorExtender.js")]
    [TargetControlType(typeof(Control))]
    public class TotalCalculatorExtender : ExtenderControlBase
    {
        [ExtenderControlProperty]
        [DefaultValue("")]
        [ClientPropertyName("controlsToCheck")]
        public string ControlsToCheck
        {
            get { return GetPropertyValue("controlsToCheck", string.Empty); }
            set { SetPropertyValue("controlsToCheck", value); }
        }
    }
}