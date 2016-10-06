﻿using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Export using GridView control.
/// </summary>
public class GridViewExportUtil
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="gv"></param>
    public static void Export(string fileName, GridView gv)
    {
        Export(fileName, gv, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="gv"></param>
    public static void Export(string fileName, GridView gv, string title, string cellMaxHeight = "20px")
    {
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.AddHeader(
            "content-disposition", string.Format("attachment; filename={0}", fileName));
        HttpContext.Current.Response.ContentType = "application/ms-excel";

        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter htw = new HtmlTextWriter(sw))
            {
                if (title != null)
                {
                    Table titleTable = new Table();
                    titleTable.GridLines = GridLines.None;

                    TableCell titleCell = new TableCell();
                    titleCell.Text = title;
                    titleCell.Font.Bold = true;
                    titleCell.Font.Size = new FontUnit(FontSize.Medium);
                    if (gv.HeaderRow != null)
                        titleCell.ColumnSpan = gv.HeaderRow.Cells.Count;

                    TableRow titleRow = new TableRow();
                    titleRow.Cells.Add(titleCell);
                    titleTable.Rows.Add(titleRow);

                    titleTable.RenderControl(htw);
                }

                Table reportTable = PrepareReportTable(gv, cellMaxHeight);
                //  render the table into the htmlwriter
                reportTable.RenderControl(htw);

                //  render the htmlwriter into the response
                HttpContext.Current.Response.Write(sw.ToString());
                HttpContext.Current.Response.End();
            }
        }
    }

    public static void Export(string fileName, GridView gv, Table summaryTable, string title, string cellMaxHeight = "20px")
    {
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.AddHeader(
            "content-disposition", string.Format("attachment; filename={0}", fileName));
        HttpContext.Current.Response.ContentType = "application/ms-excel";

        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter htw = new HtmlTextWriter(sw))
            {
                if (title != null)
                {
                    Table titleTable = new Table();
                    titleTable.GridLines = GridLines.None;

                    TableCell titleCell = new TableCell();
                    titleCell.Text = title;
                    titleCell.Font.Bold = true;
                    titleCell.Font.Size = new FontUnit(FontSize.Medium);
                    if (gv.HeaderRow != null)
                        titleCell.ColumnSpan = gv.HeaderRow.Cells.Count;
                    titleCell.RowSpan = 2;
                    titleCell.HorizontalAlign = HorizontalAlign.Center;
                    titleCell.Style.Add("border-bottom", "1px solid black");
                    titleCell.Style.Add("vertical-align", "middle");

                    TableRow titleRow = new TableRow();
                    titleRow.Cells.Add(titleCell);
                    titleTable.Rows.Add(titleRow);

                    titleTable.RenderControl(htw);
                }

                summaryTable.RenderControl(htw);

                Table reportTable = PrepareReportTable(gv, cellMaxHeight);
                //  render the table into the htmlwriter
                reportTable.RenderControl(htw);

                //  render the htmlwriter into the response
                int chunkSize = 256;

                char[] data = sw.ToString().ToCharArray();
                for (int index = 0; index < data.Length; index = index + chunkSize)
                {
                    if (data.Length - index < chunkSize)
                        chunkSize = data.Length - index;

                    HttpContext.Current.Response.Write(data, index, chunkSize);
                }

                HttpContext.Current.Response.End();
            }
        }
    }

    private static Table PrepareReportTable(GridView gv, string cellMaxHeight)
    {
        //  Create a table to contain the grid
        Table table = new Table();

        //  include the gridline settings
        table.GridLines = gv.GridLines;

        //  add the header row to the table
        if (gv.HeaderRow != null)
        {
            GridViewExportUtil.PrepareControlForExport(gv.HeaderRow);
            table.Rows.Add(gv.HeaderRow);
        }

        //  add each of the data rows to the table
        foreach (GridViewRow row in gv.Rows)
        {
            GridViewExportUtil.PrepareControlForExport(row);
            table.Rows.Add(row);
        }

        //  add the footer row to the table
        if (gv.FooterRow != null)
        {
            GridViewExportUtil.PrepareControlForExport(gv.FooterRow);
            table.Rows.Add(gv.FooterRow);
        }

        if (cellMaxHeight != null)
        {
            foreach (TableRow trow in table.Rows)
                trow.Style.Add(HtmlTextWriterStyle.Height, cellMaxHeight);
        }

        return table;
    }

    /// <summary>
    /// Replace any of the contained controls with literals
    /// </summary>
    /// <param name="control"></param>
    private static void PrepareControlForExport(Control control)
    {
        for (int i = 0; i < control.Controls.Count; i++)
        {
            Control current = control.Controls[i];
            if (current is LinkButton)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as LinkButton).Text));
            }
            else if (current is ImageButton)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as ImageButton).AlternateText));
            }
            else if (current is HyperLink)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as HyperLink).Text));
            }
            else if (current is DropDownList)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as DropDownList).SelectedItem.Text));
            }
            else if (current is CheckBox)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as CheckBox).Checked ? "True" : "False"));
            }

            if (current.HasControls())
            {
                GridViewExportUtil.PrepareControlForExport(current);
            }
        }
    }

    public static void Export(string fileName, StringBuilder data)
    {
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.AddHeader(
            "content-disposition", string.Format("attachment; filename={0}", fileName));
        HttpContext.Current.Response.ContentType = "application/ms-excel";

        using (StringWriter sw = new StringWriter(data))
        {
            HttpContext.Current.Response.Write(sw.ToString());
            HttpContext.Current.Response.End();
        }
    }

}
