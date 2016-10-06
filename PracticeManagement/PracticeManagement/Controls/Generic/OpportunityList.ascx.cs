﻿using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using DataTransferObjects;
using PraticeManagement.Utils;
using PraticeManagement.Controls.Generic.Filtering;
using DataTransferObjects.ContextObjects;
using System.Web.UI.HtmlControls;
using AjaxControlToolkit;
using PraticeManagement.Controls.Opportunities;
using System.Collections.Generic;
using System.Web;

namespace PraticeManagement.Controls.Generic
{
    public enum OpportunityListFilterMode
    {
        GenericFilter,
        ByTargetPerson 
    }

    public partial class OpportunityList : PracticeManagementFilterControl<OpportunitySortingContext>
    {
        private const string ViewStateSortOrder = "SortOrder";
        private const string ViewStateSortDirection = "SortDirection";
        private const string CssArrowClass = "arrow";
        private const string WordBreak = "<wbr />";
        private const string Description = "<b>Description : </b>{0}";

        private const string ANIMATION_SHOW_SCRIPT =
                       @"<OnClick>
                        	<Sequence>
                        		<Parallel Duration=""0"" AnimationTarget=""{0}"">
                        			<Discrete Property=""style"" propertyKey=""border"" ValuesScript=""['thin solid navy']""/>
                        		</Parallel>
                        		<Parallel Duration="".4"" Fps=""20"" AnimationTarget=""{0}"">
                        			<Resize  Width=""350"" Height=""{1}"" Unit=""px"" />
                        		</Parallel>
                        	</Sequence>
                        </OnClick>";

        private const string ANIMATION_HIDE_SCRIPT =
                        @"<OnClick>
                        	<Sequence>
                        		<Parallel Duration="".4"" Fps=""20"" AnimationTarget=""{0}"">
                        			<Resize Width=""0"" Height=""0"" Unit=""px"" />
                        		</Parallel>
                        		<Parallel Duration=""0"" AnimationTarget=""{0}"">
                        			<Discrete Property=""style"" propertyKey=""border"" ValuesScript=""['none']""/>
                        		</Parallel>
                        	</Sequence>
                        </OnClick>";


        public bool AllowAutoRedirectToDetails { get; set; }
        public OpportunityListFilterMode FilterMode { get; set; }
        public int? TargetPersonId { get; set; }

        protected string OrderBy
        {
            get
            {
                return GetViewStateValue<string>(ViewStateSortOrder, null);
            }
            set
            {
                SetViewStateValue(ViewStateSortOrder, value);
            }
        }

        protected SortDirection SortDirection
        {
            get
            {
                return GetViewStateValue(ViewStateSortDirection, SortDirection.Ascending);
            }
            set
            {
                SetViewStateValue(ViewStateSortDirection, value);
            }
        }

        public Opportunity[] GetOpportunities(OpportunityFilterSettings filter = null)
        {
            return
                FilterMode == OpportunityListFilterMode.GenericFilter ?
                DataHelper.GetFilteredOpportunities(filter)
                :
                DataHelper.GetOpportunitiesForTargetPerson(TargetPersonId);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void PopulatePriorityHint()
        {
            var opportunityPriorities = OpportunityPriorityHelper.GetOpportunityPriorities(true);
            var row = lvOpportunities.FindControl("lvHeader") as System.Web.UI.HtmlControls.HtmlTableRow;
            if (row != null)
            {
                var lvOp = row.FindControl("lvOpportunityPriorities") as ListView;
                lvOp.DataSource = opportunityPriorities;
                lvOp.DataBind();
            }

        }

        protected void lvOpportunities_Sorting(object sender, ListViewSortEventArgs e)
        {
            var newOrder = e.SortExpression;

            if (newOrder == OrderBy)
            {
                SortDirection =
                    SortDirection == SortDirection.Ascending ?
                        SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                OrderBy = newOrder;
                SortDirection = SortDirection.Ascending;
            }

            SetHeaderIconsAccordingToSordOrder();
            FireFilterOptionsChanged();
        }

        protected override void ResetControls()
        {
            OrderBy = null;
            SortDirection = SortDirection.Ascending;

            SetHeaderIconsAccordingToSordOrder();
        }

        protected override void InitControls()
        {

            OrderBy = Filter.OrderBy;
            SortDirection = DataTransferObjects.Utils.Generic.ToEnum<SortDirection>(Filter.SortDirection, SortDirection.Ascending);

            SetHeaderIconsAccordingToSordOrder();
        }

        protected override OpportunitySortingContext InitFilter()
        {
            return new OpportunitySortingContext
                        {
                            SortDirection = SortDirection.ToString(),
                            OrderBy = OrderBy
                        };
        }

        protected void Page_Prerender(object sender, EventArgs e)
        {
            PopulatePriorityHint();
            PreparePrioritiesWithAnimations();
        }

        private void PreparePrioritiesWithAnimations()
        {
            var row = lvOpportunities.FindControl("lvHeader") as System.Web.UI.HtmlControls.HtmlTableRow;

            if (row != null)
            {
                var img = row.FindControl("imgPriorityHint") as Image;
                var pnlPriority = lvOpportunities.FindControl("pnlPriority") as Panel;
                var btnClosePriority = lvOpportunities.FindControl("btnClosePriority") as Button;

                var animHide = row.FindControl("animHide") as AnimationExtender;
                var animShow = row.FindControl("animShow") as AnimationExtender;

                int height = 205;

                animShow.Animations = string.Format(ANIMATION_SHOW_SCRIPT, pnlPriority.ID, height);
                animHide.Animations = string.Format(ANIMATION_HIDE_SCRIPT, pnlPriority.ID);

                img.Attributes["onclick"]
                   = string.Format("setHintPosition('{0}', '{1}');", img.ClientID, pnlPriority.ClientID);
            }
        }

        private void SetHeaderIconsAccordingToSordOrder()
        {
            var row = lvOpportunities.FindControl("lvHeader") as System.Web.UI.HtmlControls.HtmlTableRow;

            if (row != null)
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    var cell = row.Cells[i];

                    if (cell.HasControls())
                    {
                        foreach (var ctrl in cell.Controls)
                        {
                            if (ctrl is LinkButton)
                            {
                                var lb = ctrl as LinkButton;
                                lb.CssClass = CssArrowClass;
                                if (lb.CommandArgument == OrderBy)
                                    lb.CssClass = GetCssClass();
                            }
                        }
                    }
                }
        }

        protected string GetCssClass()
        {
            return string.Format("{0} sort-{1}",
                                CssArrowClass,
                                SortDirection.ToString() == SortDirection.Ascending.ToString() ? "up" : "down");
        }

        public void DatabindOpportunities(OpportunityFilterSettings filter = null)
        {
           var opportunities = GetOpportunities(filter);
            DataBindLookedOpportunities(opportunities);
        }

        public void DataBindLookedOpportunities(Opportunity[] opportunities)
        {
            lvOpportunities.DataSource = opportunities;
            lvOpportunities.DataBind();

            //  IsPostBack here means that method is called on postback
            //      so it means that it's coming from search and we should redirect if there's the only result
            if (IsPostBack && lvOpportunities.Items.Count == 1 && AllowAutoRedirectToDetails)
            {
                var detailsLink =
                    Urls.OpportunityDetailsLink(opportunities[0].Id.Value);

                PraticeManagement.Utils.Generic.RedirectWithReturnTo(detailsLink, Request.Url.AbsoluteUri, Response);
            }
        }
  
        protected string GetOpportunityDetailsLink(int opportunityId, int index)
        {
            return Utils.Generic.GetTargetUrlWithReturn(Urls.OpportunityDetailsLink(opportunityId), Request.Url.AbsoluteUri);
        }

        protected string GetPersonDetailsLink(int personId, int index)
        {
            return Urls.GetPersonDetailsUrl(
                     new Person(personId),
                     Request.Url.AbsoluteUri);
        }

        protected static string GetDaysOld(DateTime date, bool IsCreateDate)
        {
            var span = (Utils.Generic.GetNowWithTimeZone()).Date.Subtract(date.Date);

            var days = span.Days;

            if (IsCreateDate)
            {
                return days > 0 ? string.Format("{0}", days) : "Current";
            }
            else
            {
                return days > 0 ? string.Format("{0} day{1}", days, days == 1 ? string.Empty : "s") : "Current";
            }
        }

        protected static string GetFormattedEstimatedRevenue(Decimal? estimatedRevenue)
        {
            return estimatedRevenue.GetFormattedEstimatedRevenue();
        }

        protected static string GetRevenueTypeCaption(RevenueType type)
        {
            if (type == RevenueType.Undefined || type == RevenueType.Unknown)
            {
                return Constants.Formatting.UnknownValue;
            }
            return type.ToString();
        }

        protected static bool IsNeedToShowPerson(Person person)
        {
            if (person == null)
            {
                return false;
            }
            return true;
        }

        protected static string GetWrappedText(String descriptionText)
        {
            if (descriptionText == null)
            {
                return string.Format(Description,string.Empty);
            }

            descriptionText = descriptionText.Trim();

            if (descriptionText.Length > 500)
            {
                descriptionText = descriptionText.Substring(0, 500) + ".....";
            }

            for (int i = 0; i < descriptionText.Length; i = i + 15)
            {
                descriptionText = descriptionText.Insert(i, WordBreak);
            }

            descriptionText = HttpUtility.HtmlEncode(descriptionText);

            descriptionText = descriptionText.Replace("&lt;wbr /&gt;", "<wbr />");

            return string.Format(Description,descriptionText); ;
        }

        public string GetProjectDetailUrl(Opportunity opty)
        {
            if (opty.Project != null)
            {
                return Utils.Generic.GetTargetUrlWithReturn(string.Format(Constants.ApplicationPages.DetailRedirectFormat,
                                 Constants.ApplicationPages.ProjectDetail,
                                 opty.Project.Id.ToString())
                                 , Request.Url.AbsoluteUri
                                 );
            }
            return string.Empty;
        }
    }
}

