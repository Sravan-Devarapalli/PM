<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ClientMarginGoals.ascx.cs"
    Inherits="PraticeManagement.Controls.Clients.ClientMarginGoals" %>
<%@ Register Src="~/Controls/DatePicker.ascx" TagName="DatePicker" TagPrefix="uc2" %>
<%@ Import Namespace="PraticeManagement.Utils" %>
<%@ Import Namespace="DataTransferObjects" %>

<table class="Width100Per">
    <tr>
        <td>
            <table class="CompPerfTable gvCommissionsAndRates Width55Percent Border0">
                <thead>
                    <tr class="no-wrap">
                        <th class="Width5Percent ie-bg"></th>
                        <th class="Width20PerImp">
                            <div class='ie-bg'>
                                Start Date
                            </div>
                        </th>
                        <th class="Width20PerImp">
                            <div class='ie-bg'>
                                End Date
                            </div>
                        </th>
                        <th class="Width20PerImp">
                            <div class='ie-bg'>
                                Margin Goal %
                            </div>
                        </th>
                        <th class="Width35Percent">
                            <div class='ie-bg'>
                                Comments
                            </div>
                        </th>
                    </tr>
                </thead>
            </table>
            <asp:Repeater ID="repMarginGoal" runat="server" OnItemDataBound="repMarginGoal_ItemDataBound" OnItemCommand="repMarginGoal_ItemCommand">
                <HeaderTemplate>
                    <div>
                        <table id="tblMarginGoal" class="CompPerfTable Width55Percent BackGroundColorWhite">
                            <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr class="no-wrap">
                        <td class="Width5Percent">
                            <asp:ImageButton ID="imgEditMargin" ToolTip="Edit Compensation" runat="server" CommandName="edit"
                                ImageUrl="~/Images/icon-edit.png" />
                            <asp:ImageButton ID="imgUpdateMargin" Visible="false"
                                ToolTip="Save" runat="server" ImageUrl="~/Images/icon-check.png" OnClick="imgUpdateMargin_Click" />
                            <asp:ImageButton ID="imgCancel" ToolTip="Cancel" runat="server" ImageUrl="~/Images/no.png" Visible="false"
                                OnClick="imgCancel_Click" /></td>
                        <td class="textCenter Width20PerImp">
                            <table class="WholeWidth">
                                <tr>
                                    <td class="Width90Percent">
                                        <asp:Label ID="lblStartDate" runat="server" Text='<%#((DateTime)Eval("StartDate")).ToString("MM/dd/yyyy") %>'></asp:Label>

                                        <uc2:DatePicker ID="dtpPeriodFrom" runat="server" ValidationGroup="MarginGoal" Visible="false" DateValue='<%#((DateTime)Eval("StartDate")).Date %>'
                                            MarginId='<%#Eval("Id")%>' />
                                    </td>
                                    <td>
                                        <asp:RequiredFieldValidator ID="reqPeriodFrom" runat="server" ControlToValidate="dtpPeriodFrom"
                                            ErrorMessage="The Start Date is required." ToolTip="The Start Date is required."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginGoal"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compPeriodFrom" runat="server" ControlToValidate="dtpPeriodFrom"
                                            ErrorMessage="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                            ToolTip="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                            Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginGoal"></asp:CompareValidator>
                                        <asp:CustomValidator ID="custGoalPeriodUpdate" runat="server" ControlToValidate="dtpPeriodFrom"
                                            ErrorMessage="A Margin Goal already exists for this period. Margin Goal dates cannot overlap."
                                            ToolTip="A Margin Goal already exists for this period. Margin Goal dates cannot overlap."
                                            ValidationGroup="MarginGoal" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                            Display="Dynamic"></asp:CustomValidator>
                                    </td>
                                </tr>
                            </table>


                        </td>
                        <td class="textCenter Width20PerImp">
                            <table class="WholeWidth">
                                <tr>
                                    <td class="Width90Percent">
                                        <asp:Label ID="lblEndDate" runat="server" Text='<%#((DateTime)Eval("EndDate")).ToString("MM/dd/yyyy") %>'></asp:Label>

                                        <uc2:DatePicker ID="dtpPeriodTo" runat="server" ValidationGroup="MarginGoal" Visible="false" DateValue='<%#((DateTime)Eval("EndDate")).Date%>' />
                                    </td>
                                    <td>
                                        <asp:RequiredFieldValidator ID="reqPeriodTo" runat="server" ControlToValidate="dtpPeriodTo"
                                            ErrorMessage="The End Date is required." ToolTip="The End Date is required."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="MarginGoal"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compPeriodTo" runat="server" ControlToValidate="dtpPeriodTo"
                                            ErrorMessage="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                            ToolTip="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                            Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                            Operator="DataTypeCheck" Type="Date" ValidationGroup="MarginGoal"></asp:CompareValidator>
                                        <asp:CompareValidator ID="compPeriod" runat="server" ControlToCompare="dtpPeriodTo"
                                            ControlToValidate="dtpPeriodFrom" Display="Dynamic" EnableClientScript="False"
                                            ErrorMessage="The End date must be greater than the Start date."
                                            Operator="LessThanEqual" SetFocusOnError="True" Type="Date" ToolTip="The End date must be greater than the Start date."
                                            ValidationGroup="MarginGoal">*</asp:CompareValidator>
                                    </td>
                                </tr>
                            </table>


                        </td>
                        <td class="textCenter Width20PerImp">
                            <table class="WholeWidth">
                                <tr>
                                    <td class="Width90Percent">
                                        <asp:Label ID="lblMarginGoal" runat="server" Text='<%#Eval("MarginGoal")+"%"  %>'></asp:Label>

                                        <asp:TextBox ID="txtMarginGoal" runat="server" Text='<%#Eval("MarginGoal") %>' Visible="false"></asp:TextBox>
                                    </td>
                                    <td>
                                        <asp:RequiredFieldValidator ID="rfvMarginThresholdVariance" runat="server" ControlToValidate="txtMarginGoal"
                                            EnableClientScript="false" Text="*" ErrorMessage="Margin Goal is Required." ToolTip="Margin Goal is Required."
                                            ValidationGroup="MarginGoal"></asp:RequiredFieldValidator>
                                        <asp:CompareValidator ID="compVarianceGrid" runat="server" Operator="DataTypeCheck" Type="Integer" Display="Dynamic"
                                            ControlToValidate="txtMarginGoal" ErrorMessage="The Margin Goal must be a whole number." ToolTip="The Margin Goal must be a whole number."
                                            EnableClientScript="false" Text="*" ValidationGroup="MarginGoal" />
                                    </td>
                                </tr>
                            </table>


                        </td>
                        <td class="width30P">
                            <table class="WholeWidth">
                                <tr>
                                    <td class="Width90Percent no-wrap">
                                        <asp:Label ID="lblComments" runat="server" Text='<%#Eval("Comments") %>'></asp:Label>

                                        <asp:TextBox ID="txtComments" runat="server" Text='<%#Eval("Comments") %>' Visible="false" Width="100%"></asp:TextBox>
                                    </td>
                                    <td>
                                        <asp:RequiredFieldValidator ID="rfvCommentsGrid" runat="server" ControlToValidate="txtComments"
                                            EnableClientScript="false" Text="*" ErrorMessage="Margin Goal Comments is Required." ToolTip="Margin Goal Comments is Required."
                                            ValidationGroup="MarginGoal"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                            </table>


                        </td>
                        <td class="Width5Percent">
                            <asp:HiddenField ID="hidKey" runat="server" Value='<%# Eval("Id") %>' />
                            <asp:ImageButton ID="imgDelete" runat="server" ImageUrl="~/Images/icon-delete.png" Visible="false"
                                OnClick="imgDelete_Click" ToolTip="Delete MarginGoal" />
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </tbody></table></div>
                </FooterTemplate>
            </asp:Repeater>
            <table class="CompPerfTable gvCommissionsAndRates Width55Percent Border0">
                <tr class="alterrow no-wrap">
                    <td class="Width5Percent PaddingTop10Px TextAlignCenter">
                        <asp:ImageButton ID="btnPlus" runat="server" ImageUrl="~/Images/add_16.png" OnClick="btnPlus_Click"
                            ToolTip="Add Pricing List" Visible="false" />
                        <asp:ImageButton ID="btnAddMarginGoal" runat="server" ValidationGroup="AddMarginGoal"
                            ImageUrl="~/Images/icon-check.png" ToolTip="Confirm" Visible="false" OnClick="btnAddMarginGoal_Click" />
                        <asp:ImageButton ID="btnCancel" runat="server" ImageUrl="~/Images/no.png" OnClick="btnCancel_Click"
                            ToolTip="Cancel" Visible="false" />
                    </td>
                    <td class="textCenter Width20PerImp">
                        <table class="WholeWidth">
                            <tr>
                                <td class="Width90Percent">
                                    <uc2:DatePicker ID="dtpAddPeriodFrom" runat="server" ValidationGroup="AddMarginGoal" Visible="false" />
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="reqPeriodFrom" runat="server" ControlToValidate="dtpAddPeriodFrom"
                                        ErrorMessage="The Start Date is required." ToolTip="The Start Date is required."
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="AddMarginGoal"></asp:RequiredFieldValidator>
                                    <asp:CompareValidator ID="compPeriodFrom" runat="server" ControlToValidate="dtpAddPeriodFrom"
                                        ErrorMessage="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                        ToolTip="The StartDate has an incorrect format. It must be 'MM/dd/yyyy'."
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                        Operator="DataTypeCheck" Type="Date" ValidationGroup="AddMarginGoal"></asp:CompareValidator>
                                    <asp:CustomValidator ID="custGoalPeriod" runat="server" ControlToValidate="dtpAddPeriodFrom"
                                        ErrorMessage="A Margin Goal already exists for this period. Margin Goal dates cannot overlap."
                                        ToolTip="A Margin Goal already exists for this period. Margin Goal dates cannot overlap."
                                        ValidationGroup="AddMarginGoal" Text="*" EnableClientScript="false" SetFocusOnError="true"
                                        Display="Dynamic" OnServerValidate="custGoalPeriod_ServerValidate" Enabled="false"></asp:CustomValidator>
                                </td>
                            </tr>
                        </table>


                    </td>
                    <td class="textCenter Width20PerImp">
                        <table class="WholeWidth">
                            <tr>
                                <td class="Width90Percent">
                                    <uc2:DatePicker ID="dtpAddPeriodTo" runat="server" ValidationGroup="AddMarginGoal" Visible="false" />
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="reqPeriodTo" runat="server" ControlToValidate="dtpAddPeriodTo"
                                        ErrorMessage="The End Date is required." ToolTip="The End Date is required."
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" ValidationGroup="AddMarginGoal"></asp:RequiredFieldValidator>
                                    <asp:CompareValidator ID="compPeriodTo" runat="server" ControlToValidate="dtpAddPeriodTo"
                                        ErrorMessage="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                        ToolTip="The End Date has an incorrect format. It must be 'MM/dd/yyyy'."
                                        Text="*" EnableClientScript="false" SetFocusOnError="true" Display="Dynamic"
                                        Operator="DataTypeCheck" Type="Date" ValidationGroup="AddMarginGoal"></asp:CompareValidator>
                                    <asp:CompareValidator ID="compPeriod" runat="server" ControlToCompare="dtpAddPeriodTo"
                                        ControlToValidate="dtpAddPeriodFrom" Display="Dynamic" EnableClientScript="False"
                                        ErrorMessage="The End date must be greater than the Start date."
                                        Operator="LessThanEqual" SetFocusOnError="True" Type="Date" ToolTip="The End date must be greater than the Start date."
                                        ValidationGroup="AddMarginGoal">*</asp:CompareValidator>
                                </td>
                            </tr>
                        </table>


                    </td>
                    <td class="textCenter Width20PerImp">
                        <table class="WholeWidth">
                            <tr>
                                <td class="Width90Percent">
                                    <asp:TextBox ID="txtAddMarginGoal" runat="server" Visible="false"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="rfvMarginThresholdVarianceAdd" runat="server" ControlToValidate="txtAddMarginGoal" Display="Dynamic"
                                        EnableClientScript="false" Text="*" ErrorMessage="Margin Goal is Required." ToolTip="Margin Goal is Required."
                                        ValidationGroup="AddMarginGoal"></asp:RequiredFieldValidator>
                                    <asp:CompareValidator ID="compVariance" runat="server" Operator="DataTypeCheck" Type="Integer" Display="Dynamic" Enabled="false"
                                        ControlToValidate="txtAddMarginGoal" ErrorMessage="The Margin Goal must be a whole number." ToolTip="The Margin Goal must be a whole number."
                                        EnableClientScript="false" Text="*" ValidationGroup="AddMarginGoal" />
                                </td>
                            </tr>
                        </table>


                    </td>
                    <td class="Width35Percent">
                        <table class="WholeWidth">
                            <tr>
                                <td class="Width90Percent no-wrap">
                                    <asp:TextBox ID="txtAddComments" runat="server" Visible="false" Width="100%"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:RequiredFieldValidator ID="rfvComments" runat="server" ControlToValidate="txtAddComments" Display="Dynamic"
                                        EnableClientScript="false" Text="*" ErrorMessage="Margin Goal Comments is Required." ToolTip="Margin Goal Comments is Required."
                                        ValidationGroup="AddMarginGoal"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<div>
    <asp:ValidationSummary ID="valSumGroups" runat="server" ValidationGroup="AddMarginGoal" />
    <asp:ValidationSummary ID="valSumUpdation" runat="server" ValidationGroup="MarginGoal" />
</div>
<div class="Width50Percent textRight">
    <asp:LinkButton ID="lnkMarginGoalsHistory" runat="server" Text="View History" OnClick="lnkMarginGoalsHistory_Click"></asp:LinkButton>
</div>
<asp:HiddenField ID="hdnTempField" runat="server" />
<AjaxControlToolkit:ModalPopupExtender ID="mpeHistory" runat="server"
    TargetControlID="hdnTempField" CancelControlID="btnOKHistoryPanel"
    BackgroundCssClass="modalBackground" PopupControlID="pnlHistoryPanel"
    DropShadow="false" />

<asp:Panel ID="pnlHistoryPanel" runat="server" Style="display: none;" CssClass="MarginHistoryPopUp">
    <table class="WholeWidth Padding5">
        <tr>
            <td>
                <asp:Repeater ID="repMarginGoalsHistory" runat="server">
                    <HeaderTemplate>
                        <div style="max-height: 200px; overflow: auto;">
                            <table id="tblHisrotyMarginGoal" class="CompPerfTable WholeWidth BackGroundColorWhite">
                                <thead>
                                    <tr>
                                        <th>
                                            <div class='ie-bg'>
                                                Action
                                            </div>
                                        </th>
                                        <th>
                                            <div class='ie-bg'>
                                                Old Start Date
                                            </div>
                                        </th>
                                        <th>
                                            <div class='ie-bg'>
                                                New Start Date
                                            </div>
                                        </th>
                                        <th>
                                            <div class='ie-bg'>
                                                Old End Date
                                            </div>
                                        </th>
                                        <th>
                                            <div class='ie-bg'>
                                                New End Date
                                            </div>
                                        </th>
                                        <th>
                                            <div class='ie-bg'>
                                                Old Margin Goal
                                            </div>
                                        </th>
                                        <th>
                                            <div class='ie-bg'>
                                                New Margin Goal
                                            </div>
                                        </th>
                                        <th>
                                            <div class='ie-bg'>
                                                Comments
                                            </div>
                                        </th>
                                        <th>
                                            <div class='ie-bg'>
                                                Updated By
                                            </div>
                                        </th>

                                        <th>
                                            <div class='ie-bg'>
                                                Updated On
                                            </div>
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="">
                                <%#Eval("ActivityName") %>
                            </td>
                            <td class="textCenter ">
                                <%#(DateTime?)Eval("OldStartDate")!=null? ((DateTime)Eval("OldStartDate")).ToString("MM/dd/yyyy"):"" %>
                            </td>
                            <td class="textCenter ">
                                <%#(DateTime?)Eval("NewStartDate")!=null? ((DateTime)Eval("NewStartDate")).ToString("MM/dd/yyyy"):"" %>
                            </td>
                            <td class="textCenter ">
                                <%#(DateTime?)Eval("OldEndDate")!=null? ((DateTime)Eval("OldEndDate")).ToString("MM/dd/yyyy"):"" %>
                            </td>
                            <td class="textCenter ">
                                <%#(DateTime?)Eval("NewEndDate")!=null? ((DateTime)Eval("NewEndDate")).ToString("MM/dd/yyyy"):"" %>
                            </td>
                            <td class="textCenter ">
                                <%#(int?)Eval("OldMarginGoal")!=null? Eval("OldMarginGoal")+"%":"" %>
                            </td>
                            <td class="textCenter ">
                                <%#(int?)Eval("NewMarginGoal")!=null? Eval("NewMarginGoal")+"%":"" %>
                            </td>

                            <td class=""><%#Eval("Comments") %>
                            </td>

                            <td class=""><%#Eval("PersonName") %>
                            </td>
                            <td class="textCenter ">
                                <%#((DateTime)Eval("LogTime")).ToString("MM/dd/yyyy h:mm tt") %>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </tbody>
                        </table>
                        </div>
                    </FooterTemplate>
                </asp:Repeater>

            </td>
        </tr>
        <tr>
            <td class="Padding10Px TextAlignCenterImp">
                <asp:Button ID="btnOKHistoryPanel" runat="server" Text="Close" Width="100" />
            </td>
        </tr>
    </table>
</asp:Panel>





