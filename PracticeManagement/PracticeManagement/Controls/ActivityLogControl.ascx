﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ActivityLogControl.ascx.cs"
    Inherits="PraticeManagement.Controls.ActivityLogControl" %>
<%@ Register Src="~/Controls/Generic/LoadingProgress.ascx" TagName="LoadingProgress"
    TagPrefix="uc" %>
<%@ Register Src="~/Controls/Generic/Filtering/DateInterval.ascx" TagPrefix="uc"
    TagName="DateInterval" %>
<script type="text/javascript" language="javascript">
    function EnableResetButtonForDateIntervalChange(sender, args) {
        var btnreset = document.getElementById('<%= btnResetFilter.ClientID %>');
        var hdnResetFilter = document.getElementById('<%= hdnResetFilter.ClientID %>');
        if (btnreset != null && btnreset != "undefined") {
            hdnResetFilter.value = 'true';
            btnreset.disabled = '';
        }
    }

    function EnableResetButton() {
        var btnreset = document.getElementById('<%= btnResetFilter.ClientID %>');
        var hdnResetFilter = document.getElementById('<%= hdnResetFilter.ClientID %>');
        if (btnreset != null && btnreset != "undefined") {
            btnreset.disabled = '';
            hdnResetFilter.value = 'true';
        }
    }

    function CheckIfDatesValid() {

        txtStartDate = document.getElementById('<%= diRange.ClientID %>_tbFrom');
        txtEndDate = document.getElementById('<%= diRange.ClientID %>_tbTo');
        var startDate = new Date(txtStartDate.value);
        var endDate = new Date(txtEndDate.value);
        if (txtStartDate.value != '' && txtEndDate.value != ''
            && startDate <= endDate) {
            var btnCustDatesClose = document.getElementById('<%= btnCustDatesClose.ClientID %>');
            hdnStartDate = document.getElementById('<%= hdnStartDate.ClientID %>');
            hdnEndDate = document.getElementById('<%= hdnEndDate.ClientID %>');
            lblCustomDateRange = document.getElementById('<%= lblCustomDateRange.ClientID %>');
            var startDate = new Date(txtStartDate.value);
            var endDate = new Date(txtEndDate.value);
            var startDateStr = startDate.format("MM/dd/yyyy");
            var endDateStr = endDate.format("MM/dd/yyyy");
            hdnStartDate.value = startDateStr;
            hdnEndDate.value = endDateStr;
            lblCustomDateRange.innerHTML = '(' + startDateStr + '&nbsp;-&nbsp;' + endDateStr + ')';
            btnCustDatesClose.click();

        }
        return false;
    }

    function CheckAndShowCustomDatesPoup(ddlPeriod) {
        imgCalender = document.getElementById('<%= imgCalender.ClientID %>');
        lblCustomDateRange = document.getElementById('<%= lblCustomDateRange.ClientID %>');
        if (ddlPeriod.value == '0') {
            imgCalender.attributes["class"].value = "";
            lblCustomDateRange.attributes["class"].value = "fontBold";
            if (imgCalender.fireEvent) {
                imgCalender.style.display = "";
                lblCustomDateRange.style.display = "";
                imgCalender.click();
            }
            if (document.createEvent) {
                var event = document.createEvent('HTMLEvents');
                event.initEvent('click', true, true);
                imgCalender.dispatchEvent(event);
            }
        }
        else {
            imgCalender.attributes["class"].value = "displayNone";
            lblCustomDateRange.attributes["class"].value = "displayNone";
            if (imgCalender.fireEvent) {
                imgCalender.style.display = "none";
                lblCustomDateRange.style.display = "none";
            }
        }
    }
    function ReAssignStartDateEndDates() {
        hdnStartDate = document.getElementById('<%= hdnStartDate.ClientID %>');
        hdnEndDate = document.getElementById('<%= hdnEndDate.ClientID %>');
        txtStartDate = document.getElementById('<%= diRange.ClientID %>_tbFrom');
        txtEndDate = document.getElementById('<%= diRange.ClientID %>_tbTo');
        hdnStartDateCalExtenderBehaviourId = document.getElementById('<%= hdnStartDateCalExtenderBehaviourId.ClientID %>');
        hdnEndDateCalExtenderBehaviourId = document.getElementById('<%= hdnEndDateCalExtenderBehaviourId.ClientID %>');

        var endDateCalExtender = $find(hdnEndDateCalExtenderBehaviourId.value);
        var startDateCalExtender = $find(hdnStartDateCalExtenderBehaviourId.value);
        if (startDateCalExtender != null) {
            startDateCalExtender.set_selectedDate(hdnStartDate.value);
        }
        if (endDateCalExtender != null) {
            endDateCalExtender.set_selectedDate(hdnEndDate.value);
        }
        CheckIfDatesValid();
    }

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandle);

    function endRequestHandle(sender, Args) {
        imgCalender = document.getElementById('<%= imgCalender.ClientID %>');
        lblCustomDateRange = document.getElementById('<%= lblCustomDateRange.ClientID %>');
        ddlPeriod = document.getElementById('<%=  ddlPeriod.ClientID %>');
        if (imgCalender != null && lblCustomDateRange != null && ddlPeriod != null) {
            if (imgCalender.fireEvent && ddlPeriod.value != '0') {
                imgCalender.style.display = "none";
                lblCustomDateRange.style.display = "none";
            }
        }
    }

    function disableProjectsDropDown() {

        var eventSource = document.getElementById('<%= ddlEventSource.ClientID %>');
        projectList = document.getElementById('<%= ddlProjects.ClientID %>');
        if (eventSource != null && projectList != null && eventSource.value != 'undefind') {
            if ((eventSource.value >= 3 && eventSource.value <= 5) || (eventSource.value >= 21 && eventSource.value <= 27)
                 || (eventSource.value >= 32 && eventSource.value <= 42)) {
                projectList.disabled = 'true';
                projectList[0].selected = 'true';
            }
            else {
                projectList.disabled = '';
            }
        }
    }

</script>
<uc:LoadingProgress ID="lpActivityLog" runat="server" />
<asp:UpdatePanel ID="updActivityLog" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <table cellpadding="5" class="WholeWidth">
            <tr>
                <td>
                    <div id="divActivitylog" class="Padding10" runat="server">
                        <table id="tblActivitylog" runat="server" class="no-wrap">
                            <tr>
                                <td id="tdEventSource" runat="server" class="padLeft0 padRight0">
                                    <asp:Label ID="lblDisplay" runat="server" Text="Show "></asp:Label><asp:DropDownList
                                        ID="ddlEventSource" runat="server" EnableViewState="true">
                                        <asp:ListItem Text="ALL Events" Value="1" Selected="True"></asp:ListItem>
                                        <asp:ListItem Text="ALL Errors" Value="2"></asp:ListItem>
                                        <asp:ListItem Text="ALL Project Events" Value="7"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Projects" Value="8"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Projects" Value="9"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Projects" Value="10"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Project Attachment" Value="11"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Project Attachment" Value="12"></asp:ListItem>
                                        <asp:ListItem Text="ALL Milestone Events" Value="13"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Milestones" Value="14"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Milestones" Value="15"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Milestones" Value="16"></asp:ListItem>
                                        <asp:ListItem Text="ALL Opportunity Events" Value="17"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Opportunities" Value="18"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Opportunities" Value="19"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Opportunities" Value="20"></asp:ListItem>
                                        <asp:ListItem Text="ALL Report Events" Value="21"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Exported Project Summary" Value="22"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Exported Opportunity Summary" Value="23"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Exported Time Entry By Project" Value="24"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Exported Time Entry By Person" Value="25"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Exported Bench Report" Value="26"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Exported Consultants Util. Table" Value="27"></asp:ListItem>
                                        <asp:ListItem Text="ALL Time Entry Events" Value="28"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added TimeEntry" Value="29"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed TimeEntry" Value="30"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted TimeEntry" Value="31"></asp:ListItem>
                                        <asp:ListItem Text="ALL Person Events" Value="3"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Persons" Value="4"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Persons" Value="5"></asp:ListItem>
                                        <asp:ListItem Text="ALL Strawmen Events" Value="43"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Strawmen" Value="44"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Strawmen" Value="45"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Strawmen" Value="46"></asp:ListItem>
                                        <asp:ListItem Text="Target Person" Value="6"></asp:ListItem>
                                        <asp:ListItem Text="ALL Logon Events" Value="32"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Successfull Logins" Value="33"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Rejected Logins" Value="34"></asp:ListItem>
                                        <asp:ListItem Text="ALL Security Events" Value="35"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Account Lockouts" Value="36"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Password Reset Requests" Value="37"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Become User" Value="38"></asp:ListItem>
                                        <asp:ListItem Text="ALL Skill Events" Value="39"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Skills" Value="40"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Skills" Value="41"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Skills" Value="42"></asp:ListItem>
                                        <asp:ListItem Text="ALL Practice Events" Value="47"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Practice" Value="48"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Practice" Value="49"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Practice" Value="50"></asp:ListItem>
                                        <asp:ListItem Text="ALL Practice Capability Events" Value="51"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Practice Capability" Value="52"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Practice Capability" Value="53"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Practice Capability" Value="54"></asp:ListItem>
                                        <asp:ListItem Text="ALL Title Events" Value="55"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Title" Value="56"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Title" Value="57"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Title" Value="58"></asp:ListItem>
                                        <asp:ListItem Text="ALL Calendar Events" Value="59"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Calendar Events" Value="60"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Calendar Events" Value="61"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Calendar Events" Value="62"></asp:ListItem>
                                        <asp:ListItem Text="ALL PerformanceManagement Events" Value="63"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Added Project Feedback Events" Value="64"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Changed Project Feedback Events" Value="65"></asp:ListItem>
                                        <asp:ListItem Text="&nbsp; Deleted Project Feedback Events" Value="66"></asp:ListItem>
                                        <asp:ListItem Text="Vendor" Value="67"></asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                                <td id="tdYear" runat="server" class="padLeft2">
                                    <asp:Label ID="Label3" runat="server" Text="&nbsp;&nbsp;over&nbsp;&nbsp;"></asp:Label><asp:DropDownList
                                        ID="ddlPeriod" runat="server" EnableViewState="true">
                                        <asp:ListItem Text="Last Day" Value="-1"></asp:ListItem>
                                        <asp:ListItem Text="Last Week" Selected="True" Value="-7"></asp:ListItem>
                                        <asp:ListItem Text="Last Month" Value="-30"></asp:ListItem>
                                        <asp:ListItem Text="Last 3 months" Value="-90"></asp:ListItem>
                                        <asp:ListItem Text="Last 6 months" Value="-180"></asp:ListItem>
                                        <asp:ListItem Text="Last Year" Value="-360"></asp:ListItem>
                                        <asp:ListItem Text="Custom Dates" Value="0"></asp:ListItem>
                                    </asp:DropDownList>
                                    <AjaxControlToolkit:ModalPopupExtender ID="mpeCustomDates" runat="server" TargetControlID="imgCalender"
                                        EnableViewState="true" OkControlID="btnCustDatesClose" CancelControlID="btnCustDatesCancel"
                                        BackgroundCssClass="modalBackground" PopupControlID="pnlCustomDates" BehaviorID="bhCustomDates"
                                        DropShadow="false" OnCancelScript="ReAssignStartDateEndDates();" OnOkScript="return false;" />
                                    <asp:HiddenField ID="hdnStartDate" EnableViewState="true" runat="server" Value="" />
                                    <asp:HiddenField ID="hdnEndDate" EnableViewState="true" runat="server" Value="" />
                                    <asp:HiddenField ID="hdnStartDateCalExtenderBehaviourId" EnableViewState="true" runat="server"
                                        Value="" />
                                    <asp:HiddenField ID="hdnEndDateCalExtenderBehaviourId" EnableViewState="true" runat="server"
                                        Value="" />
                                    <asp:Label ID="lblCustomDateRange" EnableViewState="true" runat="server" Text=""></asp:Label>
                                    <asp:Image ID="imgCalender" EnableViewState="true" runat="server" ImageUrl="~/Images/calendar.gif" />
                                </td>
                                <td id="spnPersons" runat="server">
                                    <asp:Label ID="Label1" runat="server" Text="&nbsp;for&nbsp;"></asp:Label><asp:DropDownList
                                        ID="ddlPersonName" runat="server" DataSourceID="odsPersons" DataTextField="PersonLastFirstName"
                                        DataValueField="Id" OnDataBound="ddlPersonName_OnDataBound" />
                                    &nbsp;
                                </td>
                                <td id="spnProjects" runat="server">
                                    <asp:Label ID="Label2" runat="server" Text="&nbsp;on "></asp:Label><asp:DropDownList
                                        ID="ddlProjects" runat="server" DataSourceID="odsProjects" DataTextField="Name"
                                        DataValueField="Id" OnDataBound="ddlProjects_OnDataBound" />
                                </td>
                                <td id="tdBtnList" runat="server">
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Button ID="btnUpdateView" runat="server" Text="Update" ToolTip="Update" OnClick="btnUpdateView_Click" />
                                            </td>
                                            <td class="padLeft4">
                                                <asp:Button ID="btnResetFilter" runat="server" Text="Reset " ToolTip="Reset" OnClick="btnResetFilter_Click"
                                                    Visible="false" />
                                            </td>
                                        </tr>
                                    </table>
                                    <asp:HiddenField ID="hdnResetFilter" runat="server" />
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;
                </td>
            </tr>
            <tr>
                <td>
                    <asp:GridView ID="gvActivities" runat="server" AutoGenerateColumns="False" EmptyDataText="No activity for given parameters."
                        DataSourceID="odsActivities" AllowPaging="True" PageSize="20" CssClass="CompPerfTable gvActivitiesActivityLog"
                        OnRowDataBound="gvActivities_OnRowDataBound" OnDataBound="gvActivities_OnDataBound">
                        <AlternatingRowStyle CssClass="alterrow" />
                        <PagerSettings Mode="NumericFirstLast" />
                        <PagerStyle CssClass="cssPager" />
                        <RowStyle CssClass="al-row" />
                        <Columns>
                            <asp:TemplateField>
                                <ItemStyle CssClass="Width15Percent" />
                                <HeaderTemplate>
                                    <div class="ie-bg">
                                        Modified / User</div>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblCreateDate" runat="server" Text='<%# ((DateTime)Eval("LogDate")).ToString("MM/dd/yyyy") + " " + ((DateTime)Eval("LogDate")).ToShortTimeString() %>' />
                                    by
                                    <asp:Label ID="lblUserName" runat="server" Text='<%# GetModifiedByDetails( Eval("Person.Id"), Eval("Person.PersonLastFirstName"), Eval("SystemUser").ToString(), Eval("LogData")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <ItemStyle CssClass="WordWrapActivityLog Width15Percent" />
                                <HeaderTemplate>
                                    <div class="ie-bg">
                                        Activity</div>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblActivityType" runat="server" Text='<%# NoNeedToShowActivityType(Eval("ActivityName")) %>'></asp:Label>
                                    <asp:Xml ID="xmlActivityItem" runat="server" DocumentContent='<%# AddDefaultProjectAndMileStoneInfo(Eval("LogData")) %>'
                                        TransformSource="~/Reports/Xslt/ActivityLogItem.xslt"></asp:Xml>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <ItemStyle CssClass="WordWrapActivityLog Width70Per" />
                                <HeaderTemplate>
                                    <div class="ie-bg">
                                        Changes</div>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Xml ID="xmlChanges" runat="server" DocumentContent='<%# AddDefaultProjectAndMileStoneInfo(Eval("LogData")) %>'
                                        TransformSource="~/Reports/Xslt/ActivityLogChanges.xslt"></asp:Xml>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <asp:ObjectDataSource ID="odsProjects" runat="server" SelectMethod="GetProjectListCustom"
                        TypeName="PraticeManagement.ProjectService.ProjectServiceClient" OnSelecting="odsProjects_Selecting">
                        <SelectParameters>
                            <asp:Parameter DefaultValue="true" Name="projected" Type="Boolean" />
                            <asp:Parameter DefaultValue="true" Name="completed" Type="Boolean" />
                            <asp:Parameter DefaultValue="true" Name="active" Type="Boolean" />
                            <asp:Parameter DefaultValue="true" Name="experimantal" Type="Boolean" />
                            <asp:Parameter DefaultValue="true" Name="proposed" Type="Boolean" />
                            <asp:Parameter DefaultValue="false" Name="IsMilestone" Type="Boolean" />
                        </SelectParameters>
                    </asp:ObjectDataSource>
                    <asp:ObjectDataSource ID="odsPersons" runat="server" SelectMethod="GetAllPersons"
                        TypeName="PraticeManagement.Controls.DataHelper"></asp:ObjectDataSource>
                    <asp:ObjectDataSource ID="odsActivities" runat="server" TypeName="PraticeManagement.Utils.ActivityLogHelper"
                        SelectCountMethod="GetActivitiesCount" SelectMethod="GetActivities" StartRowIndexParameterName="startRow"
                        MaximumRowsParameterName="maxRows" EnablePaging="true" OnSelecting="odsActivities_Selecting">
                        <SelectParameters>
                            <asp:Parameter Name="startDateFilter" Type="DateTime" />
                            <asp:Parameter Name="endDateFilter" Type="DateTime" />
                            <asp:Parameter Name="personId" />
                            <asp:Parameter Name="sourceFilter" />
                            <asp:Parameter Name="projectId" />
                            <asp:Parameter Name="vendorId" />
                            <asp:Parameter Name="opportunityId" />
                            <asp:Parameter Name="milestoneId" />
                            <asp:Parameter Name="practiceAreas" Type="Boolean" />
                            <asp:Parameter Name="sowBudget" Type="Boolean" />
                            <asp:Parameter Name="director" Type="Boolean" />
                            <asp:Parameter Name="poAmount" Type="Boolean" />
                            <asp:Parameter Name="capabilities" Type="Boolean" />
                            <asp:Parameter Name="newOrExtension" Type="Boolean" />
                            <asp:Parameter Name="poNumber" Type="Boolean" />
                            <asp:Parameter Name="projectStatus" Type="Boolean" />
                            <asp:Parameter Name="salesPerson" Type="Boolean" />
                            <asp:Parameter Name="projectOwner" Type="Boolean" />
                            <asp:Parameter Name="recordPerChange" Type="Boolean" />
                            <asp:Parameter Name="division" Type="Boolean" />
                            <asp:Parameter Name="channel" Type="Boolean" />
                            <asp:Parameter Name="offering" Type="Boolean" />
                            <asp:Parameter Name="revenueType" Type="Boolean" />
                            <asp:Parameter Name="budget" Type="Boolean" />
                            <asp:Parameter Name="margin" Type="Boolean" />
                        </SelectParameters>
                    </asp:ObjectDataSource>
                </td>
            </tr>
        </table>
        <asp:Panel ID="pnlCustomDates" runat="server" CssClass="ConfirmBoxClass CustomDatesPopUp"
            Style="display: none;">
            <table class="WholeWidth">
                <tr>
                    <td align="center">
                        <uc:DateInterval ID="diRange" runat="server" IsFromDateRequired="true" IsToDateRequired="true"
                            EnableViewState="true" FromToDateFieldWidth="70" />
                    </td>
                </tr>
                <tr>
                    <td class="textCenter PaddingTop10 PaddingBottom10">
                        <asp:Button ID="btnCustDatesOK" runat="server" Text="OK" ValidationGroup="<%# ClientID %>"
                            OnClientClick="return CheckIfDatesValid();" CausesValidation="true" />
                        <asp:Button ID="btnCustDatesClose" runat="server" Style="display: none;" CausesValidation="true" />
                        &nbsp; &nbsp;
                        <asp:Button ID="btnCustDatesCancel" CausesValidation="false" runat="server" Text="Cancel" />
                    </td>
                </tr>
                <tr>
                    <td class="textCenter">
                        <asp:ValidationSummary ID="valSum" runat="server" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="btnUpdateView" EventName="Click" />
        <asp:AsyncPostBackTrigger ControlID="btnResetFilter" EventName="Click" />
    </Triggers>
</asp:UpdatePanel>

