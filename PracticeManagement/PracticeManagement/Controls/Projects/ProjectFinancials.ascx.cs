using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.MobileControls;
using DataTransferObjects;
using PraticeManagement.ProjectService;
using PraticeManagement.Security;
using PraticeManagement.Utils;
using System.Linq;

namespace PraticeManagement.Controls.Projects
{
    public partial class ProjectFinancials : UserControl
    {
        #region Constants

        private const string BenchCssClass = "Bench";
        private const string MarginCssClass = "Margin";

        #endregion Constants

        #region Fields

        private SeniorityAnalyzer _milestonesSeniorityAnalyzer;
        private List<ComputedFinancials> _financials;

        #endregion Fields

        public Project Project { get; set; }

        public ComputedFinancials EAC
        {
            get
            {
                return FindEAC();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            InitSeniorityAnalyzers();
            DisplayInterestValues(Project);
        }

        private void InitSeniorityAnalyzers()
        {
            var currentPerson = DataHelper.CurrentPerson;
            _milestonesSeniorityAnalyzer = new SeniorityAnalyzer(currentPerson);
            if (Project != null)
            {
                foreach (var milestone in Project.Milestones)
                {
                    if (_milestonesSeniorityAnalyzer.OneWithGreaterSeniorityExists(
                                DataHelper.GetPersonsInMilestone(milestone)))
                    {
                        break;
                    }
                }
            }
        }

        private void DisplayInterestValues(Project project)
        {
            // Interest values
            if (Financials != null && Financials.Count > 0)
            {
                FindEAC();
                FindBudgetToEACVariance();
                SetBudgetSummary(Financials.Find(f => f.FinanceType == 1));
                SetProjectedSummary(Financials.Find(f => f.FinanceType == 2));
                SetProjectedRemaining(Financials.Find(f => f.FinanceType == 3));
                SetActualsSummary(Financials.Find(f => f.FinanceType == 4));
                SetEAC(Financials.Find(f => f.FinanceType == 5));
                SetVarc(Financials.Find(f => f.FinanceType == 6));
            }
        }

        private void SetBackgroundColorForMargin(decimal margin, System.Web.UI.WebControls.Label lblMargin)
        {
            if (margin != 0)
            {
                int clientId = Project.Client.Id.Value;
                bool? individualClientMarginColorInfoEnabled = Project.Client.IsMarginColorInfoEnabled;

                List<ClientMarginColorInfo> cmciList = new List<ClientMarginColorInfo>();

                cmciList = DataHelper.GetClientMarginColorInfo(clientId, Project.StartDate.Value, Project.EndDate.Value, Project.Id.Value);

                if (cmciList != null)
                {
                    foreach (var item in cmciList)
                    {
                        if (margin >= item.StartRange - 1 && margin < item.EndRange)
                        {
                            lblMargin.Style["background-color"] = item.ColorInfo.ColorValue;
                            break;
                        }
                    }
                }
            }
        }

        public List<ComputedFinancials> Financials
        {
            get
            {
                return _financials ?? (_financials = Project != null && Project.Id.HasValue
                                                         ? ServiceCallers.Custom.Project(p => p.GetProjectsComputedFinancials(Project.Id.Value)).ToList()
                                                         : null);
            }
        }

        private void SetBudgetSummary(ComputedFinancials budgetFinancial)
        {
            if (budgetFinancial != null)
            {
                var hide = _milestonesSeniorityAnalyzer.GreaterSeniorityExists;
                var ht = Resources.Controls.HiddenCellText;

                var nfi = new NumberFormatInfo { CurrencyDecimalDigits = 0, CurrencySymbol = "$" };
                if (budgetFinancial.Revenue.Value >= 0)
                {
                    lblBudgetRevenue.Text = String.Format(nfi, "{0:c}", budgetFinancial.Revenue.Value);
                }
                else
                {
                    lblBudgetRevenue.Text = budgetFinancial.Revenue.ToString();
                }
                lblDiscount.Text = String.Format(nfi, "{0}", Project.Discount);
                var discountAmt = (budgetFinancial.Revenue.Value * Project.Discount / 100);
                lblBudgetDiscount.Text = discountAmt >= 0 ?
                    String.Format(nfi, "{0:c}", discountAmt) : ((PracticeManagementCurrency)discountAmt).ToString();
                lblBudgetRevenueNet.Text = budgetFinancial.RevenueNet.Value >= 0 ?
                    (String.Format(nfi, "{0:c}", budgetFinancial.RevenueNet.Value)) :
                    budgetFinancial.RevenueNet.ToString();
                lblBudgetCogs.Text = hide ? ht : String.Format(nfi, "{0:c}", budgetFinancial.Cogs.Value);
                lblBudgetExpense.Text = hide ? ht : String.Format(nfi, "{0:c}", budgetFinancial.Expenses);
                lblBudgetReimExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", budgetFinancial.ReimbursedExpenses);
                lblBudgetGrossMargin.Text = hide ? ht : String.Format(nfi, "{0:c}", budgetFinancial.GrossMargin.Value);
                lblBudgetGrossMargin.CssClass =
                    budgetFinancial.GrossMargin.Value < 0 ? BenchCssClass : MarginCssClass;
                lblBudgetMarginPerc.Text = hide
                                           ? ht
                                           : string.Format(Constants.Formatting.PercentageFormat,
                                                           budgetFinancial.TargetMargin);
                lblBudgetTotalExpenes.Text = hide ? ht : String.Format(nfi, "{0:c}", budgetFinancial.Cogs.Value + budgetFinancial.Expenses);

                if (Project.Client.Id.HasValue)
                {
                    SetBackgroundColorForMargin(budgetFinancial.TargetMargin, lblBudgetMarginPerc);
                }
            }
            else
            {
                lblBudgetMarginPerc.Text = "0.00%";
            }
        }

        private void SetProjectedSummary(ComputedFinancials ProjFinancial)
        {
            if (ProjFinancial != null)
            {

                var hide = _milestonesSeniorityAnalyzer.GreaterSeniorityExists;
                var ht = Resources.Controls.HiddenCellText;

                var nfi = new NumberFormatInfo { CurrencyDecimalDigits = 0, CurrencySymbol = "$" };
                if (ProjFinancial.Revenue.Value >= 0)
                {
                    lblEstimatedRevenue.Text = String.Format(nfi, "{0:c}", ProjFinancial.Revenue.Value);
                }
                else
                {
                    lblEstimatedRevenue.Text = ProjFinancial.Revenue.ToString();
                }
                lblDiscount.Text = String.Format(nfi, "{0}", Project.Discount);
                var discountAmt = (ProjFinancial.Revenue.Value * Project.Discount / 100);
                lblDiscountAmount.Text = discountAmt >= 0 ?
                    String.Format(nfi, "{0:c}", discountAmt) : ((PracticeManagementCurrency)discountAmt).ToString();
                lblRevenueNet.Text = ProjFinancial.RevenueNet.Value >= 0 ?
                    (String.Format(nfi, "{0:c}", ProjFinancial.RevenueNet.Value)) :
                    ProjFinancial.RevenueNet.ToString();
                lblEstimatedCogs.Text = hide ? ht : String.Format(nfi, "{0:c}", ProjFinancial.Cogs.Value);
                lblExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", ProjFinancial.Expenses);
                lblReimbursedExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", ProjFinancial.ReimbursedExpenses);
                lblGrossMargin.Text = hide ? ht : String.Format(nfi, "{0:c}", ProjFinancial.GrossMargin.Value);
                lblGrossMargin.CssClass =
                    ProjFinancial.GrossMargin.Value < 0 ? BenchCssClass : MarginCssClass;
                lblMarginPerc.Text = hide
                                           ? ht
                                           : string.Format(Constants.Formatting.PercentageFormat,
                                                           ProjFinancial.TargetMargin);

                lblTotalExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", ProjFinancial.Cogs.Value + ProjFinancial.Expenses);

                if (Project.Client.Id.HasValue)
                {
                    SetBackgroundColorForMargin(ProjFinancial.TargetMargin, lblMarginPerc);
                }

            }
            else
            {
                lblMarginPerc.Text = "0.00%";
            }
        }

        private void SetActualsSummary(ComputedFinancials ActualFinancial)
        {
            if (ActualFinancial != null)
            {

                var hide = _milestonesSeniorityAnalyzer.GreaterSeniorityExists;
                var ht = Resources.Controls.HiddenCellText;

                var nfi = new NumberFormatInfo { CurrencyDecimalDigits = 0, CurrencySymbol = "$" };
                if (ActualFinancial.Revenue.Value >= 0)
                {
                    lblActualRevenue.Text = String.Format(nfi, "{0:c}", ActualFinancial.Revenue.Value);
                }
                else
                {
                    lblActualRevenue.Text = ActualFinancial.Revenue.ToString();
                }
                lblDiscount.Text = String.Format(nfi, "{0}", Project.Discount);
                var discountAmt = (ActualFinancial.Revenue.Value * Project.Discount / 100);
                lblActDiscount.Text = discountAmt >= 0 ?
                    String.Format(nfi, "{0:c}", discountAmt) : ((PracticeManagementCurrency)discountAmt).ToString();
                lblActRevenueNet.Text = ActualFinancial.RevenueNet.Value >= 0 ?
                    (String.Format(nfi, "{0:c}", ActualFinancial.RevenueNet.Value)) :
                    ActualFinancial.RevenueNet.ToString();
                lblActCogs.Text = hide ? ht : String.Format(nfi, "{0:c}", ActualFinancial.Cogs.Value);
                lblActExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", ActualFinancial.Expenses);
                lblActualReimExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", ActualFinancial.ReimbursedExpenses);
                lblActGrossMargin.Text = hide ? ht : String.Format(nfi, "{0:c}", ActualFinancial.GrossMargin.Value);
                lblActGrossMargin.CssClass =
                    ActualFinancial.GrossMargin.Value < 0 ? BenchCssClass : MarginCssClass;
                lblActMarginPerc.Text = hide
                                           ? ht
                                           : string.Format(Constants.Formatting.PercentageFormat,
                                                           ActualFinancial.TargetMargin);
                lblActTotalExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", ActualFinancial.Cogs.Value + ActualFinancial.Expenses);

                if (Project.Client.Id.HasValue)
                {
                    SetBackgroundColorForMargin(ActualFinancial.TargetMargin, lblActMarginPerc);
                }

            }
            else
            {
                lblActMarginPerc.Text = "0.00%";
            }
        }

        private void SetProjectedRemaining(ComputedFinancials remFinancial)
        {
            if (remFinancial != null)
            {

                var hide = _milestonesSeniorityAnalyzer.GreaterSeniorityExists;
                var ht = Resources.Controls.HiddenCellText;

                var nfi = new NumberFormatInfo { CurrencyDecimalDigits = 0, CurrencySymbol = "$" };
                if (remFinancial.Revenue.Value >= 0)
                {
                    lblProjRemRevenue.Text = String.Format(nfi, "{0:c}", remFinancial.Revenue.Value);
                }
                else
                {
                    lblProjRemRevenue.Text = remFinancial.Revenue.ToString();
                }
                lblDiscount.Text = String.Format(nfi, "{0}", Project.Discount);
                var discountAmt = (remFinancial.Revenue.Value * Project.Discount / 100);
                lblProjRemDiscount.Text = discountAmt >= 0 ?
                    String.Format(nfi, "{0:c}", discountAmt) : ((PracticeManagementCurrency)discountAmt).ToString();
                lblProjRemRevenueNet.Text = remFinancial.RevenueNet.Value >= 0 ?
                    (String.Format(nfi, "{0:c}", remFinancial.RevenueNet.Value)) :
                    remFinancial.RevenueNet.ToString();
                lblProjRemCogs.Text = hide ? ht : String.Format(nfi, "{0:c}", remFinancial.Cogs.Value);
                lblProjRemExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", remFinancial.Expenses);
                lblProjRemReimExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", remFinancial.ReimbursedExpenses);
                lblProjRemGrossMargin.Text = hide ? ht : String.Format(nfi, "{0:c}", remFinancial.GrossMargin.Value);
                lblProjRemGrossMargin.CssClass =
                    remFinancial.GrossMargin.Value < 0 ? BenchCssClass : MarginCssClass;
                lblProjRemMarginPer.Text = hide
                                           ? ht
                                           : string.Format(Constants.Formatting.PercentageFormat,
                                                           remFinancial.TargetMargin);
                lblProjRemTotalExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", remFinancial.Cogs.Value + remFinancial.Expenses);

                if (Project.Client.Id.HasValue)
                {
                    SetBackgroundColorForMargin(remFinancial.TargetMargin, lblProjRemMarginPer);
                }

            }
            else
            {
                lblProjRemMarginPer.Text = "0.00%";
            }
        }

        private void SetEAC(ComputedFinancials financial)
        {
            if (financial != null)
            {

                var hide = _milestonesSeniorityAnalyzer.GreaterSeniorityExists;
                var ht = Resources.Controls.HiddenCellText;

                var nfi = new NumberFormatInfo { CurrencyDecimalDigits = 0, CurrencySymbol = "$" };
                if (financial.Revenue.Value >= 0)
                {
                    lblEACRevenue.Text = String.Format(nfi, "{0:c}", financial.Revenue.Value);
                }
                else
                {
                    lblEACRevenue.Text = financial.Revenue.ToString();
                }
                lblDiscount.Text = String.Format(nfi, "{0}", Project.Discount);
                var discountAmt = (financial.Revenue.Value * Project.Discount / 100);
                lblEACDiscount.Text = discountAmt >= 0 ?
                    String.Format(nfi, "{0:c}", discountAmt) : ((PracticeManagementCurrency)discountAmt).ToString();
                lblEACRevenueNet.Text = financial.RevenueNet.Value >= 0 ?
                    (String.Format(nfi, "{0:c}", financial.RevenueNet.Value)) :
                    financial.RevenueNet.ToString();
                lblEACCogs.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.Cogs.Value);
                lblEACExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.Expenses);
                lblEACReimExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.ReimbursedExpenses);
                lblEACGrossMargin.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.GrossMargin.Value);
                lblEACGrossMargin.CssClass =
                    financial.GrossMargin.Value < 0 ? BenchCssClass : MarginCssClass;
                lblEACMarginPerc.Text = hide
                                           ? ht
                                           : string.Format(Constants.Formatting.PercentageFormat,
                                                           financial.TargetMargin);
                lblEACTotalExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.Cogs.Value + financial.Expenses);

                if (Project.Client.Id.HasValue)
                {
                    SetBackgroundColorForMargin(financial.TargetMargin, lblEACMarginPerc);
                }

            }
            else
            {
                lblEACMarginPerc.Text = "0.00%";
            }
        }


        private void SetVarc(ComputedFinancials financial)
        {
            if (financial != null)
            {
                var hide = _milestonesSeniorityAnalyzer.GreaterSeniorityExists;
                var ht = Resources.Controls.HiddenCellText;

                var nfi = new NumberFormatInfo { CurrencyDecimalDigits = 0, CurrencySymbol = "$" };
                if (financial.Revenue.Value >= 0)
                {
                    lblVarcRevenue.Text = String.Format(nfi, "{0:c}", financial.Revenue.Value);
                }
                else
                {
                    lblVarcRevenue.Text = financial.Revenue.ToString();
                }
                lblDiscount.Text = String.Format(nfi, "{0}", Project.Discount);
                var discountAmt = (financial.Revenue.Value * Project.Discount / 100);
                lblVarcDiscount.Text = discountAmt >= 0 ?
                    String.Format(nfi, "{0:c}", discountAmt) : ((PracticeManagementCurrency)discountAmt).ToString();
                lblVarRevenueNet.Text = financial.RevenueNet.Value >= 0 ?
                    (String.Format(nfi, "{0:c}", financial.RevenueNet.Value)) :
                    financial.RevenueNet.ToString();
                lblVarcCogs.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.Cogs.Value);
                lblVarcExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.Expenses);
                lblVarcReimExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.ReimbursedExpenses);
                lblVarcGrossMargin.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.GrossMargin.Value);
                lblVarcGrossMargin.CssClass =
                    financial.GrossMargin.Value < 0 ? BenchCssClass : MarginCssClass;
                lblVarcMarginPerc.Text = hide
                                           ? ht
                                           : string.Format(Constants.Formatting.PercentageFormat,
                                                           financial.TargetMargin);
                lblVarcTotalExpenses.Text = hide ? ht : String.Format(nfi, "{0:c}", financial.Cogs.Value + financial.Expenses);

                if (Project.Client.Id.HasValue)
                {
                    SetBackgroundColorForMargin(financial.TargetMargin, lblVarcMarginPerc);
                }
            }
            else
            {
                lblVarcMarginPerc.Text = "0.00%";
            }
        }

        private ComputedFinancials FindEAC()
        {
            ComputedFinancials eac = null;
            if (Financials != null)
            {
                var actFinancials = Financials.Find(f => f.FinanceType == 4);
                var remProjFinancials = Financials.Find(f => f.FinanceType == 3);


                if (actFinancials == null && remProjFinancials != null)
                {
                    eac = remProjFinancials;
                    eac.FinanceType = 5;//for EAC
                }
                else if (actFinancials != null && remProjFinancials == null)
                {
                    eac = new ComputedFinancials();
                    eac.FinanceType = 5;
                    eac.Revenue = actFinancials.Revenue;
                    eac.RevenueNet = actFinancials.RevenueNet;
                    eac.Cogs = actFinancials.Cogs;
                    eac.Expenses = actFinancials.Expenses;
                    eac.ReimbursedExpenses = actFinancials.ReimbursedExpenses;
                    eac.GrossMargin = actFinancials.GrossMargin;

                }
                else
                {
                    eac = new ComputedFinancials();
                    eac.FinanceType = 5;
                    eac.Revenue = actFinancials.Revenue + remProjFinancials.Revenue;
                    eac.RevenueNet = actFinancials.RevenueNet + remProjFinancials.RevenueNet;
                    eac.Cogs = actFinancials.Cogs + remProjFinancials.Cogs;
                    eac.Expenses = actFinancials.Expenses + remProjFinancials.Expenses;
                    eac.ReimbursedExpenses = actFinancials.ReimbursedExpenses + remProjFinancials.ReimbursedExpenses;
                    eac.GrossMargin = actFinancials.GrossMargin + remProjFinancials.GrossMargin;
                }
                Financials.Add(eac);
            }
            return eac;
        }

        private void FindBudgetToEACVariance()
        {
            if (Financials != null)
            {
                var budgtetFinancials = Financials.Find(f => f.FinanceType == 1);
                var EACFinancials = Financials.Find(f => f.FinanceType == 5);
                ComputedFinancials varc = null;

                if (budgtetFinancials == null && EACFinancials != null)
                {
                    varc = new ComputedFinancials();
                    varc.FinanceType = 6;//for Variance
                    varc.Revenue = EACFinancials.Revenue;
                    varc.RevenueNet = EACFinancials.RevenueNet;
                    varc.Cogs = EACFinancials.Cogs;
                    varc.Expenses = EACFinancials.Expenses;
                    varc.ReimbursedExpenses = EACFinancials.ReimbursedExpenses;
                    varc.GrossMargin = EACFinancials.GrossMargin;

                }
                else if (budgtetFinancials != null && EACFinancials == null)
                {
                    varc = new ComputedFinancials();
                    varc.FinanceType = 6;//for Variance
                    varc.Revenue = -1 * budgtetFinancials.Revenue;
                    varc.RevenueNet = -1 * budgtetFinancials.RevenueNet;
                    varc.Cogs = -1 * budgtetFinancials.Cogs;
                    varc.Expenses = -1 * budgtetFinancials.Expenses;
                    varc.ReimbursedExpenses = -1 * budgtetFinancials.ReimbursedExpenses;
                    varc.GrossMargin = -1 * budgtetFinancials.GrossMargin;
                }
                else
                {
                    varc = new ComputedFinancials();
                    varc.FinanceType = 6;
                    varc.Revenue = EACFinancials.Revenue - budgtetFinancials.Revenue;
                    varc.RevenueNet = Math.Round(EACFinancials.RevenueNet - budgtetFinancials.RevenueNet, 2, MidpointRounding.AwayFromZero);
                    varc.Cogs = EACFinancials.Cogs - budgtetFinancials.Cogs;
                    varc.Expenses = EACFinancials.Expenses - budgtetFinancials.Expenses;
                    varc.ReimbursedExpenses = EACFinancials.ReimbursedExpenses - budgtetFinancials.ReimbursedExpenses;
                    varc.GrossMargin = Math.Round(EACFinancials.GrossMargin - budgtetFinancials.GrossMargin, 2, MidpointRounding.AwayFromZero);
                }
                Financials.Add(varc);

            }
        }
    }
}

