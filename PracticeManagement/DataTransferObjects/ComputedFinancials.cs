using System;
using System.Runtime.Serialization;
using DataTransferObjects.Financials;

namespace DataTransferObjects
{
    /// <summary>
    /// Represents the results of the rate computing.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ComputedFinancials
    {
        #region Fields

        private PracticeManagementCurrency _cogsValue;
        private PracticeManagementCurrency _marginValue;
        private PracticeManagementCurrency _revenueNetValue;
        private PracticeManagementCurrency _revenueValue;
        private PracticeManagementCurrency _actualMarginValue;
        private PracticeManagementCurrency _actualRevenueValue;
        private PracticeManagementCurrency _previousMonthsActualMarginValue;
        private PracticeManagementCurrency _previousMonthsActualRevenueValue;
        private PracticeManagementCurrency _BudgetMarginValue;
        private PracticeManagementCurrency _BudgetRevenueValue;
        private PracticeManagementCurrency _ETCMarginValue;
        private PracticeManagementCurrency _ETCRevenueValue;

        #endregion Fields

        #region Constructor

        public ComputedFinancials()
        {
            Timescale = TimescaleType.Salary;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets a computed Revenue.
        /// </summary>
        [DataMember]
        public PracticeManagementCurrency ActualRevenue
        {
            get { return _actualRevenueValue; }
            set
            {
                _actualRevenueValue = value;
                _actualRevenueValue.DoNotShowDecimals = true;
                _actualRevenueValue.FormatStyle = NumberFormatStyle.Revenue;
            }
        }

        /// <summary>
        /// Gets or sets a computed margin.
        /// </summary>
        [DataMember]
        public PracticeManagementCurrency ActualGrossMargin
        {
            get { return _actualMarginValue; }
            set
            {
                _actualMarginValue = value;
                _actualMarginValue.DoNotShowDecimals = true;
                _actualMarginValue.FormatStyle = NumberFormatStyle.Margin;
            }
        }

        [DataMember]
        public PracticeManagementCurrency PreviousMonthsActualMarginValue
        {
            get { return _previousMonthsActualMarginValue; }
            set
            {
                _previousMonthsActualMarginValue = value;
                _previousMonthsActualMarginValue.DoNotShowDecimals = true;
                _previousMonthsActualMarginValue.FormatStyle = NumberFormatStyle.Margin;
            }
        }

        [DataMember]
        public PracticeManagementCurrency PreviousMonthsActualRevenueValue
        {
            get { return _previousMonthsActualRevenueValue; }
            set
            {
                _previousMonthsActualRevenueValue = value;
                _previousMonthsActualRevenueValue.DoNotShowDecimals = true;
                _previousMonthsActualRevenueValue.FormatStyle = NumberFormatStyle.Revenue;
            }
        }

        [DataMember]
        public PracticeManagementCurrency BudgetRevenue
        {
            get { return _BudgetRevenueValue; }
            set
            {
                _BudgetRevenueValue = value;
                _BudgetRevenueValue.DoNotShowDecimals = true;
                _BudgetRevenueValue.FormatStyle = NumberFormatStyle.Revenue;
            }
        }

        /// <summary>
        /// Gets or sets a computed margin.
        /// </summary>
        [DataMember]
        public PracticeManagementCurrency BudgetGrossMargin
        {
            get { return _BudgetMarginValue; }
            set
            {
                _BudgetMarginValue = value;
                _BudgetMarginValue.DoNotShowDecimals = true;
                _BudgetMarginValue.FormatStyle = NumberFormatStyle.Margin;
            }
        }

        [DataMember]
        public PracticeManagementCurrency EACRevenue
        {
            get { return _ETCRevenueValue; }
            set
            {
                _ETCRevenueValue = value;
                _ETCRevenueValue.DoNotShowDecimals = true;
                _ETCRevenueValue.FormatStyle = NumberFormatStyle.Revenue;
            }
        }

        /// <summary>
        /// Gets or sets a computed margin.
        /// </summary>
        [DataMember]
        public PracticeManagementCurrency EACGrossMargin
        {
            get { return _ETCMarginValue; }
            set
            {
                _ETCMarginValue = value;
                _ETCMarginValue.DoNotShowDecimals = true;
                _ETCMarginValue.FormatStyle = NumberFormatStyle.Margin;
            }
        }

        /// <summary>
        /// Gets or sets a computed Revenue.
        /// </summary>
        [DataMember]
        public PracticeManagementCurrency Revenue
        {
            get { return _revenueValue; }
            set
            {
                _revenueValue = value;
                _revenueValue.DoNotShowDecimals = true;
                _revenueValue.FormatStyle = NumberFormatStyle.Revenue;
            }
        }

        /// <summary>
        /// Gets or sets a computed Net Revenue.
        /// </summary>
        [DataMember]
        public PracticeManagementCurrency RevenueNet
        {
            get { return _revenueNetValue; }
            set
            {
                _revenueNetValue = value;
                _revenueNetValue.DoNotShowDecimals = true;
                _revenueNetValue.FormatStyle = NumberFormatStyle.Revenue;
            }
        }

        /// <summary>
        /// Gets or sets a computed COGS.
        /// </summary>
        [DataMember]
        public PracticeManagementCurrency Cogs
        {
            get { return _cogsValue; }
            set
            {
                _cogsValue = value;
                _cogsValue.DoNotShowDecimals = true;
                _cogsValue.FormatStyle = NumberFormatStyle.Cogs;
            }
        }

        /// <summary>
        /// Gets or sets a computed margin.
        /// </summary>
        [DataMember]
        public PracticeManagementCurrency GrossMargin
        {
            get { return _marginValue; }
            set
            {
                _marginValue = value;
                _marginValue.DoNotShowDecimals = true;
                _marginValue.FormatStyle = NumberFormatStyle.Margin;
            }
        }

        [DataMember]
        public PracticeManagementCurrency MilestonePersonAmount
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a date the financials tied with.
        /// </summary>
        [DataMember]
        public DateTime? FinancialDate { get; set; }

        [DataMember]
        public RangeType FinancialRange { get; set; }

        /// <summary>
        /// Gets or sets a number of hours billed.
        /// </summary>
        [DataMember]
        public decimal HoursBilled { get; set; }

        /// <summary>
        /// Gets an average hourly bill rate minus discount.
        /// </summary>
        ///
        [DataMember]
        public PracticeManagementCurrency? BillRateMinusDiscount { get; set; }

        /// <summary>
        /// Gets or sets a date since the person is available.
        /// </summary>
        /// <remarks>Uses for bench persons.</remarks>
        [DataMember]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets expenses, $.
        /// </summary>
        /// <remarks>Total expenses, $</remarks>
        [DataMember]
        public decimal Expenses { get; set; }

        /// <summary>
        /// Gets or sets reimbursed expenses, $.
        /// </summary>
        /// <remarks>Total reimbursed expenses, $</remarks>
        [DataMember]
        public decimal ReimbursedExpenses { get; set; }

        /// <summary>
        /// Gets or sets a timescale the financials were calculated for.
        /// </summary>
        /// <remarks>Uses for bench persons.</remarks>
        [DataMember]
        public TimescaleType Timescale { get; set; }

        /// <summary>
        /// Used in bench costs page for bench persons.
        /// </summary>
        [DataMember]
        public int TimescaleChangeStatus { get; set; }


        /// <summary>
        /// 1- budget, 2-Projected, 3- remaining Projected, 4- Actuals
        /// </summary>
        [DataMember]
        public int FinanceType { get; set; }

        [DataMember]
        public decimal ActualHours { get; set; }

        [DataMember]
        public decimal EACHours { get; set; }

        [DataMember]
        public decimal BudgetHours { get; set; }


        #endregion Properties

        #region Calculated Values

        /// <summary>
        /// Gets an average hourly bill rate without a discount.
        /// </summary>
        public PracticeManagementCurrency? GrossBillRate
        {
            get { return HoursBilled > 0 ? RevenueNet / HoursBilled : (PracticeManagementCurrency?)null; }
        }

        /// <summary>
        /// Gets an average hourly bill rate.
        /// </summary>
        public PracticeManagementCurrency? BillRate
        {
            get { return HoursBilled > 0 ? Revenue / HoursBilled : (PracticeManagementCurrency?)null; }
        }

        /// <summary>
        /// Gets an average hourly pay
        /// </summary>
        public PracticeManagementCurrency? LoadedHourlyPay
        {
            get { return HoursBilled > 0 ? Cogs / HoursBilled : (PracticeManagementCurrency?)null; }
        }

        /// <summary>
        /// Gets a margin net
        /// </summary>
        public PracticeManagementCurrency MarginNet
        {
            get
            {
                PracticeManagementCurrency result = GrossMargin;
                result.FormatStyle = NumberFormatStyle.Margin;
                return result;
            }
        }

        /// <summary>
        /// Gets a margin to revenue ratio in percentage.
        /// </summary>
        public decimal TargetMargin
        {
            get { return RevenueNet != 0M ? GrossMargin.Value * 100M / RevenueNet.Value : 0M; }
        }

        #endregion Calculated Values
    }
}

