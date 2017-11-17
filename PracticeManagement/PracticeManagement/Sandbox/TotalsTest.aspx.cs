using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PraticeManagement.Controls.Generic.TotalCalculator;

namespace PraticeManagement.Sandbox
{
    public partial class TotalsTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var randomizer = new Random();

            for (int i = 0; i < 10; i++)
            {
                HtmlTableRow row = new HtmlTableRow();
                List<string> ids = new List<string>();

                for (int j = 0; j < 5; j++)
                {
                    HtmlTableCell cell = new HtmlTableCell();

                    var id = "cell_" + i + "_" + j;
                    HtmlInputText text = new HtmlInputText();
                    text.ID = id;
                    text.Value = randomizer.Next(1, 100).ToString();
                    cell.Controls.Add(text);
                    row.Cells.Add(cell);
                    ids.Add("ctl00_body_" + id);
                }

                HtmlInputText sumText = new HtmlInputText();
                sumText.ID = "sumCell_" + i;                

                HtmlTableCell sumCell = new HtmlTableCell();
                sumCell.Controls.Add(sumText);
                row.Cells.Add(sumCell);

                TotalCalculatorExtender ext = new TotalCalculatorExtender();
                ext.TargetControlID = sumText.ID;
                ext.ControlsToCheck = string.Join(";", ids.ToArray());
                div.Controls.Add(ext);

                testTable.Rows.Add(row);
            }
        }
    }
}
