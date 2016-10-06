﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConsultingDemandTReportByTitle.ascx.cs"
    Inherits="PraticeManagement.Controls.Reports.ConsultantDemand.ConsultingDemandTReportByTitle" %>
<asp:HiddenField ID="hdncpeExtendersIds" runat="server" Value="" />
<asp:HiddenField ID="hdnCollapsed" runat="server" Value="true" />
<table class="WholeWidthWithHeight">
    <tr>
        <td colspan="4" class="Width90Percent">
            <asp:Button ID="btnExpandOrCollapseAll" runat="server" Text="Collapse All" UseSubmitBehavior="false"
                CssClass="Width100Px" ToolTip="Collapse All" />
            &nbsp;&nbsp;
        </td>
        <td class=" Width5Percent padRight5">
            <table class="WholeWidth">
                <tr>
                    <td>
                        Export:
                    </td>
                    <td>
                        <asp:Button ID="btnExportToExcel" runat="server" Text="Excel" OnClick="btnExportToExcel_OnClick"
                            UseSubmitBehavior="false" ToolTip="Export To Excel" />
                        <asp:HiddenField ID="hdIsGraphPage" runat="server" />
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<asp:Repeater ID="repTitles" runat="server" OnItemDataBound="repTitles_ItemDataBound">
    <HeaderTemplate>
        <div class="consultngDetailPanel">
            <table class="trConsultngDetailPanel BorderBottom1px">
                <tr>
                    <th>
                        <asp:LinkButton ID="btnTitleOrSkill1" runat="server" CausesValidation="false" CommandArgument="TitleOrSkill1"
                            Style="text-decoration: none; color: Black;" OnCommand="btnTitleOrSkill1_Command">
                            <asp:Label ID="lblTitleSkillFr" runat="server"></asp:Label>
                        </asp:LinkButton>
                    </th>
                    <th>
                        <asp:LinkButton ID="btnTitleOrSkill2" runat="server" CausesValidation="false" CommandArgument="TitleOrSkill2"
                            Style="text-decoration: none; color: Black;" OnCommand="btnTitleOrSkill2_Command">
                            <asp:Label ID="lblTitleSkillSc" runat="server"></asp:Label></asp:LinkButton>
                    </th>
                    <th>
                        <asp:LinkButton ID="btnSalesStage" runat="server" CausesValidation="false" CommandArgument="Sales Stage"
                            Style="text-decoration: none; color: Black;" OnCommand="btnSalesStage_Command">
                        Sales Stage</asp:LinkButton>
                    </th>
                    <th class="Width12PercentImp">
                        <asp:LinkButton ID="btnOpportunityNumber" runat="server" CausesValidation="false"
                            CommandArgument="OpportunityNumber" Style="text-decoration: none; color: Black;"
                            OnCommand="btnOpportunityNumber_Command">
                        Opportunity Number</asp:LinkButton>
                    </th>
                    <th class="Width14PercentImp">
                        <asp:LinkButton ID="btnProjectNumber" runat="server" CausesValidation="false" CommandArgument="ProjectNumber"
                            Style="text-decoration: none; color: Black;" OnCommand="btnProjectNumber_Command">
                        Project Number</asp:LinkButton>
                    </th>
                    <th class="Width15PercentImp">
                        <asp:LinkButton ID="btnAccountName" runat="server" CausesValidation="false" CommandArgument="AccountName"
                            Style="text-decoration: none; color: Black;" OnCommand="btnAccountName_Command">
                        Account Name</asp:LinkButton>
                    </th>
                    <th class="Width15PercentImp">
                        <asp:LinkButton ID="btnProjectName" runat="server" CausesValidation="false" CommandArgument="ProjectName"
                            Style="text-decoration: none; color: Black;" OnCommand="btnProjectName_Command">
                        Project Name</asp:LinkButton>
                    </th>
                    <th>
                        <asp:LinkButton ID="btnResourceStartDate" runat="server" CausesValidation="false"
                            CommandArgument="ResourceStartDate" Style="text-decoration: none; color: Black;"
                            OnCommand="btnResourceStartDate_Command">
                        Resource Start Date</asp:LinkButton>
                    </th>
                </tr>
            </table>
    </HeaderTemplate>
    <ItemTemplate>
        <table class="trConsultngDetailPanelItem">
            <tr>
                <th class="textLeft padLeft10Imp no-wrap">
                    <AjaxControlToolkit:CollapsiblePanelExtender ID="cpeDetail" runat="Server" CollapsedText="Expand Title Details"
                        ExpandedText="Collapse Title Details" EnableViewState="true" BehaviorID="cpeDetail"
                        Collapsed="true" TargetControlID="pnlTitleDetails" ImageControlID="imgDate" CollapsedImage="~/Images/expand.jpg"
                        ExpandedImage="~/Images/collapse.jpg" CollapseControlID="imgDate" ExpandControlID="imgDate"
                        TextLabelID="lbDate" />
                    <asp:Image ID="imgDate" runat="server" ImageUrl="~/Images/collapse.jpg" ToolTip="Expand Date Details" />
                    <asp:Label ID="lbDate" Style="display: none;" runat="server"></asp:Label>
                    <asp:Label ID="lblHeader" runat="server"></asp:Label>
                </th>
                <th>
                </th>
            </tr>
        </table>
        <asp:Panel ID="pnlTitleDetails" runat="server">
            <asp:Repeater ID="repDetails" runat="server" OnItemDataBound="repDetails_ItemDataBound">
                <ItemTemplate>
                    <table class="trConsultngDetailPanelInnerItem">
                        <tr>
                            <td>
                            </td>
                            <td>
                                <asp:Label ID="lblTitleSkillItem" runat="server"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblSalesStage" runat="server"></asp:Label>
                            </td>
                            <td class="Width12PercentImp">
                                <asp:HyperLink ID="hlOpportunityNumber" runat="server" Target="_blank">
                                </asp:HyperLink>
                            </td>
                            <td class="Width14PercentImp">
                                <asp:HyperLink ID="hlProjectNumber" runat="server" Target="_blank">
                                </asp:HyperLink>
                            </td>
                            <td class="Width15PercentImp WS-Normal">
                                <asp:Label ID="lblAccountName" runat="server"></asp:Label>
                            </td>
                            <td class="Width15PercentImp WS-Normal">
                                <asp:Label ID="lblProjectName" runat="server"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblRsrcStartDate" runat="server"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
                <AlternatingItemTemplate>
                    <table class="trConsultngDetailPanelInnerItem bgcolorF9FAFFImp">
                        <tr>
                            <td>
                            </td>
                            <td>
                                <asp:Label ID="lblTitleSkillItem" runat="server"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblSalesStage" runat="server"></asp:Label>
                            </td>
                            <td class="Width12PercentImp">
                                <asp:HyperLink ID="hlOpportunityNumber" runat="server" Target="_blank">
                                </asp:HyperLink>
                            </td>
                            <td class="Width14PercentImp">
                                <asp:HyperLink ID="hlProjectNumber" runat="server" Target="_blank">
                                </asp:HyperLink>
                            </td>
                            <td class="Width15PercentImp WS-Normal">
                                <asp:Label ID="lblAccountName" runat="server"></asp:Label>
                            </td>
                            <td class="Width15PercentImp WS-Normal">
                                <asp:Label ID="lblProjectName" runat="server"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblRsrcStartDate" runat="server"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </AlternatingItemTemplate>
            </asp:Repeater>
        </asp:Panel>
    </ItemTemplate>
    <AlternatingItemTemplate>
        <table class="trConsultngDetailPanelItem bgcolor_ECE9D9">
            <tr>
                <th class="textLeft padLeft10Imp no-wrap">
                    <AjaxControlToolkit:CollapsiblePanelExtender ID="cpeDetail" runat="Server" CollapsedText="Expand Title Details"
                        ExpandedText="Collapse Title Details" EnableViewState="true" BehaviorID="cpeDetail"
                        Collapsed="true" TargetControlID="pnlTitleDetails" ImageControlID="imgDate" CollapsedImage="~/Images/expand.jpg"
                        ExpandedImage="~/Images/collapse.jpg" CollapseControlID="imgDate" ExpandControlID="imgDate"
                        TextLabelID="lbDate" />
                    <asp:Image ID="imgDate" runat="server" ImageUrl="~/Images/collapse.jpg" ToolTip="Expand Date Details" />
                    <asp:Label ID="lbDate" Style="display: none;" runat="server"></asp:Label>
                    <asp:Label ID="lblHeader" runat="server"></asp:Label>
                </th>
                <th>
                </th>
            </tr>
        </table>
        <asp:Panel ID="pnlTitleDetails" runat="server">
            <asp:Repeater ID="repDetails" runat="server" OnItemDataBound="repDetails_ItemDataBound">
                <ItemTemplate>
                    <table class="trConsultngDetailPanelInnerItem">
                        <tr>
                            <td>
                            </td>
                            <td>
                                <asp:Label ID="lblTitleSkillItem" runat="server"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblSalesStage" runat="server"></asp:Label>
                            </td>
                            <td class="Width12PercentImp">
                                <asp:HyperLink ID="hlOpportunityNumber" runat="server" Target="_blank">
                                </asp:HyperLink>
                            </td>
                            <td class="Width14PercentImp">
                                <asp:HyperLink ID="hlProjectNumber" runat="server" Target="_blank">
                                </asp:HyperLink>
                            </td>
                            <td class="Width15PercentImp WS-Normal">
                                <asp:Label ID="lblAccountName" runat="server"></asp:Label>
                            </td>
                            <td class="Width15PercentImp WS-Normal">
                                <asp:Label ID="lblProjectName" runat="server"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblRsrcStartDate" runat="server"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
                <AlternatingItemTemplate>
                    <table class="trConsultngDetailPanelInnerItem bgcolorF9FAFFImp">
                        <tr>
                            <td>
                            </td>
                            <td>
                                <asp:Label ID="lblTitleSkillItem" runat="server"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblSalesStage" runat="server"></asp:Label>
                            </td>
                            <td class="Width12PercentImp">
                                <asp:HyperLink ID="hlOpportunityNumber" runat="server" Target="_blank">
                                </asp:HyperLink>
                            </td>
                            <td class="Width14PercentImp">
                                <asp:HyperLink ID="hlProjectNumber" runat="server" Target="_blank">
                                </asp:HyperLink>
                            </td>
                            <td class="Width15PercentImp WS-Normal">
                                <asp:Label ID="lblAccountName" runat="server"></asp:Label>
                            </td>
                            <td class="Width15PercentImp WS-Normal">
                                <asp:Label ID="lblProjectName" runat="server"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblRsrcStartDate" runat="server"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </AlternatingItemTemplate>
            </asp:Repeater>
        </asp:Panel>
    </AlternatingItemTemplate>
    <FooterTemplate>
        </div>
    </FooterTemplate>
</asp:Repeater>
<asp:Repeater ID="repByMonth" runat="server" OnItemDataBound="repByMonth_ItemDataBound">
    <HeaderTemplate>
        <div class="consultngDetailPanel">
            <table class="ConsultingDemandDetailsByMonth BorderBottom1px">
                <thead>
                    <tr class="headerRow">
                        <th class="FirstTD">
                            <asp:LinkButton ID="btnMonth" runat="server" CausesValidation="false" CommandArgument="Month"
                                Style="text-decoration: none; color: Black;" OnCommand="btnMonth_Command">
                        Month Year
                            </asp:LinkButton>
                        </th>
                        <th class="SecondTD">
                            <asp:LinkButton ID="btnTitleSkill" runat="server" CausesValidation="false" CommandArgument="TitleSkill"
                                Style="text-decoration: none; color: Black;" OnCommand="btnTitleSkill_Command">
                                <asp:Label ID="lblTilteOrSkillHeader" runat="server"></asp:Label>
                            </asp:LinkButton>
                        </th>
                        <th class="ThirdTD">
                            <asp:LinkButton ID="btnMonthSalesStage" runat="server" CausesValidation="false" CommandArgument="MonthSalesStage"
                                Style="text-decoration: none; color: Black;" OnCommand="btnMonthSalesStage_Command">
                        Sales Stage
                            </asp:LinkButton>
                        </th>
                        <th class="ForthTD">
                            <asp:LinkButton ID="btnMonthOpportunityNumber" runat="server" CausesValidation="false"
                                CommandArgument="MonthOpportunityNumber" Style="text-decoration: none; color: Black;"
                                OnCommand="btnMonthOpportunityNumber_Command">
                        Opportunity Number
                            </asp:LinkButton>
                        </th>
                        <th class="FifthTD">
                            <asp:LinkButton ID="btnMonthProjectNumber" runat="server" CausesValidation="false"
                                CommandArgument="MonthProjectNumber" Style="text-decoration: none; color: Black;"
                                OnCommand="btnMonthProjectNumber_Command">
                        Project Number
                            </asp:LinkButton>
                        </th>
                        <th class="ForthTD">
                            <asp:LinkButton ID="btnMonthAccountName" runat="server" CausesValidation="false"
                                CommandArgument="MonthAccountName" Style="text-decoration: none; color: Black;"
                                OnCommand="btnMonthAccountName_Command">
                        Account Name
                            </asp:LinkButton>
                        </th>
                        <th class="SixthTD">
                            <asp:LinkButton ID="btnMonthProjectName" runat="server" CausesValidation="false"
                                CommandArgument="MonthProjectName" Style="text-decoration: none; color: Black;"
                                OnCommand="btnMonthProjectName_Command">
                        Project Name
                            </asp:LinkButton>
                        </th>
                        <th class="ThirdTD">
                            <asp:LinkButton ID="btnMonthResourceStartDate" runat="server" CausesValidation="false"
                                CommandArgument="MonthResourceStartDate" Style="text-decoration: none; color: Black;"
                                OnCommand="btnMonthResourceStartDate_Command">
                        Resource Start Date
                            </asp:LinkButton>
                        </th>
                    </tr>
                </thead>
            </table>
    </HeaderTemplate>
    <ItemTemplate>
        <table class="ConsultingDemandDetailsByMonth">
            <tr class="bgColorD4D0C9 textLeft">
                <td colspan="7" class="padLeft20Imp no-wrap">
                    <AjaxControlToolkit:CollapsiblePanelExtender ID="cpeDetails" runat="Server" CollapsedText="Expand Month Details"
                        ExpandedText="Collapse Month Details" EnableViewState="true" BehaviorID="cpeDetails"
                        Collapsed="true" TargetControlID="pnlMonthDetails" ImageControlID="imgDetails"
                        CollapsedImage="~/Images/expand.jpg" ExpandedImage="~/Images/collapse.jpg" CollapseControlID="imgDetails"
                        ExpandControlID="imgDetails" TextLabelID="lbMonth" />
                    <asp:Image ID="imgDetails" runat="server" ImageUrl="~/Images/collapse.jpg" ToolTip="Expand Month Details" />
                    <asp:Label ID="lbMonth" Style="display: none;" runat="server"></asp:Label>
                    &nbsp;&nbsp;&nbsp;&nbsp;
                    <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantGroupByMonth)Container.DataItem).MonthStartDate.ToString(PraticeManagement.Constants.Formatting.FullMonthYearFormat)%>
                </td>
            </tr>
        </table>
        <asp:Panel ID="pnlMonthDetails" runat="server">
            <asp:Repeater ID="repByMonthDetails" runat="server" OnItemDataBound="repByMonthDetails_ItemDataBound">
                <ItemTemplate>
                    <table class="ConsultingDemandDetailsByMonth">
                        <tr class="bgcolorwhite">
                            <td class="FirstTD">
                            </td>
                            <td class="SecondTD">
                                <asp:Label ID="lblTilteOrSkillItem" runat="server"></asp:Label>
                            </td>
                            <td class="ThirdTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).SalesStage%>
                            </td>
                            <td class="ForthTD">
                                <asp:HyperLink ID="hlOpportunityNumber" runat="server" Text=' <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).OpportunityNumber%>'
                                    Target="_blank" NavigateUrl='<%# GetOpportunityDetailsLink((int?)(((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).OpportunityId)) %>'
                                    ToolTip='<%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectDescription%>'>
                                </asp:HyperLink>
                            </td>
                            <td class="FifthTD">
                                <asp:HyperLink ID="hlProjectNumber" runat="server" Text=' <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectNumber%>'
                                    Target="_blank" NavigateUrl='<%# GetProjectDetailsLink((int?)(((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectId)) %>'
                                    ToolTip='<%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectDescription%>'>
                                </asp:HyperLink>
                            </td>
                            <td class="ForthTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).HtmlEncodedAccountName%>
                            </td>
                            <td class="SixthTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).HtmlEncodedProjectName%>
                            </td>
                            <td class="ThirdTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ResourceStartDate.ToString(PraticeManagement.Constants.Formatting.EntryDateFormat)%>
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
                <AlternatingItemTemplate>
                    <table class="ConsultingDemandDetailsByMonth">
                        <tr class="bgcolorwhite">
                            <td class="FirstTD">
                            </td>
                            <td class="SecondTD">
                                <asp:Label ID="lblTilteOrSkillItem" runat="server"></asp:Label>
                            </td>
                            <td class="ThirdTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).SalesStage%>
                            </td>
                            <td class="ForthTD">
                                <asp:HyperLink ID="hlOpportunityNumber" runat="server" Text=' <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).OpportunityNumber%>'
                                    Target="_blank" NavigateUrl='<%# GetOpportunityDetailsLink((int?)(((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).OpportunityId)) %>'
                                    ToolTip='<%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectDescription%>'>
                                </asp:HyperLink>
                            </td>
                            <td class="FifthTD">
                                <asp:HyperLink ID="hlProjectNumber" runat="server" Text=' <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectNumber%>'
                                    Target="_blank" NavigateUrl='<%# GetProjectDetailsLink((int?)(((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectId)) %>'
                                    ToolTip='<%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectDescription%>'>
                                </asp:HyperLink>
                            </td>
                            <td class="ForthTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).HtmlEncodedAccountName%>
                            </td>
                            <td class="SixthTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).HtmlEncodedProjectName%>
                            </td>
                            <td class="ThirdTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ResourceStartDate.ToString(PraticeManagement.Constants.Formatting.EntryDateFormat)%>
                            </td>
                        </tr>
                    </table>
                </AlternatingItemTemplate>
            </asp:Repeater>
        </asp:Panel>
    </ItemTemplate>
    <AlternatingItemTemplate>
        <table class="ConsultingDemandDetailsByMonth">
            <tr class="bgcolor_ECE9D9 textLeft">
                <td colspan="7" class="padLeft20Imp no-wrap">
                    <AjaxControlToolkit:CollapsiblePanelExtender ID="cpeDetails" runat="Server" CollapsedText="Expand Month Details"
                        ExpandedText="Collapse Month Details" EnableViewState="true" BehaviorID="cpeDetails"
                        Collapsed="true" TargetControlID="pnlMonthDetails" ImageControlID="imgDetails"
                        CollapsedImage="~/Images/expand.jpg" ExpandedImage="~/Images/collapse.jpg" CollapseControlID="imgDetails"
                        ExpandControlID="imgDetails" TextLabelID="lbMonth" />
                    <asp:Image ID="imgDetails" runat="server" ImageUrl="~/Images/collapse.jpg" ToolTip="Expand Month Details" />
                    <asp:Label ID="lbMonth" Style="display: none;" runat="server"></asp:Label>
                    &nbsp;&nbsp;&nbsp;&nbsp;
                    <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantGroupByMonth)Container.DataItem).MonthStartDate.ToString(PraticeManagement.Constants.Formatting.FullMonthYearFormat)%>
                </td>
            </tr>
        </table>
        <asp:Panel ID="pnlMonthDetails" runat="server">
            <asp:Repeater ID="repByMonthDetails" runat="server" OnItemDataBound="repByMonthDetails_ItemDataBound">
                <ItemTemplate>
                    <table class="ConsultingDemandDetailsByMonth">
                        <tr class="bgcolorwhite">
                            <td class="FirstTD">
                            </td>
                            <td class="SecondTD">
                                <asp:Label ID="lblTilteOrSkillItem" runat="server"></asp:Label>
                            </td>
                            <td class="ThirdTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).SalesStage%>
                            </td>
                            <td class="ForthTD">
                                <asp:HyperLink ID="hlOpportunityNumber" runat="server" Text=' <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).OpportunityNumber%>'
                                    Target="_blank" NavigateUrl='<%# GetOpportunityDetailsLink((int?)(((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).OpportunityId)) %>'
                                    ToolTip='<%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectDescription%>'>
                                </asp:HyperLink>
                            </td>
                            <td class="FifthTD">
                                <asp:HyperLink ID="hlProjectNumber" runat="server" Text=' <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectNumber%>'
                                    Target="_blank" NavigateUrl='<%# GetProjectDetailsLink((int?)(((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectId)) %>'
                                    ToolTip='<%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectDescription%>'>
                                </asp:HyperLink>
                            </td>
                            <td class="ForthTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).HtmlEncodedAccountName%>
                            </td>
                            <td class="SixthTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).HtmlEncodedProjectName%>
                            </td>
                            <td class="ThirdTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ResourceStartDate.ToString(PraticeManagement.Constants.Formatting.EntryDateFormat)%>
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
                <AlternatingItemTemplate>
                    <table class="ConsultingDemandDetailsByMonth">
                        <tr class="bgcolorwhite">
                            <td class="FirstTD">
                            </td>
                            <td class="SecondTD">
                                <asp:Label ID="lblTilteOrSkillItem" runat="server"></asp:Label>
                            </td>
                            <td class="ThirdTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).SalesStage%>
                            </td>
                            <td class="ForthTD">
                                <asp:HyperLink ID="hlOpportunityNumber" runat="server" Text=' <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).OpportunityNumber%>'
                                    Target="_blank" NavigateUrl='<%# GetOpportunityDetailsLink((int?)(((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).OpportunityId)) %>'
                                    ToolTip='<%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectDescription%>'>
                                </asp:HyperLink>
                            </td>
                            <td class="FifthTD">
                                <asp:HyperLink ID="hlProjectNumber" runat="server" Text=' <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectNumber%>'
                                    Target="_blank" NavigateUrl='<%# GetProjectDetailsLink((int?)(((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectId)) %>'
                                    ToolTip='<%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ProjectDescription%>'>
                                </asp:HyperLink>
                            </td>
                            <td class="ForthTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).HtmlEncodedAccountName%>
                            </td>
                            <td class="SixthTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).HtmlEncodedProjectName%>
                            </td>
                            <td class="ThirdTD">
                                <%# ((DataTransferObjects.Reports.ConsultingDemand.ConsultantDemandDetailsByMonth)Container.DataItem).ResourceStartDate.ToString(PraticeManagement.Constants.Formatting.EntryDateFormat)%>
                            </td>
                        </tr>
                    </table>
                </AlternatingItemTemplate>
            </asp:Repeater>
        </asp:Panel>
    </AlternatingItemTemplate>
    <FooterTemplate>
        </div>
    </FooterTemplate>
</asp:Repeater>

