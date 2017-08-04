﻿using System;
using System.Runtime.Serialization;

namespace DataTransferObjects
{
    /// <summary>
    /// Provides formatting for the currency ammount.
    /// </summary>
    [DataContract]
    [Serializable]
    public struct PracticeManagementCurrency
    {
        #region Constants

        public const string CurrencyLargeDisplayFormat = "$###,###,###,###,##0";
        public const string CurrencySmallDisplayFormat = "$##0.00";
        public const string CurrencyNegativeFormat = "<span class=\"Bench\">({0})</span>";
        public const string BenchFormat = "<span class=\"Bench\">{0}</span>";
        public const string RevenueFormat = "<span class=\"Revenue\">{0}</span>";
        public const string MarginFormat = "<span class=\"Margin\">{0}</span>";

        private const string TotalFormat = "<b>{0}</b>";
        private const string CogsFormat = "<span class=\"Cogs\">{0}</span>";
        private const decimal SmallFormatLimit = 1000M;
        private const string PercentFormat = "{0}%";
        private const string HideText = "(Hidden)";

        #endregion Constants

        #region Properties

        /// <summary>
        /// Gets or sets a currency value
        /// </summary>
        [DataMember]
        public decimal Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the formatting style
        /// </summary>
        [DataMember]
        public NumberFormatStyle FormatStyle
        {
            get;
            set;
        }

        public bool DoNotShowDecimals
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        public string FormattedValue()
        {
            decimal absValue = Math.Abs(Value);
            string result =
                absValue >= SmallFormatLimit || DoNotShowDecimals ?
                absValue.ToString(CurrencyLargeDisplayFormat) : absValue.ToString(CurrencySmallDisplayFormat);
            return "(" + result + ")";
        }

        /// <summary>
        /// Converts a Value into the text representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            decimal absValue = Math.Abs(Value);
            string result =
                absValue >= SmallFormatLimit || DoNotShowDecimals ?
                absValue.ToString(CurrencyLargeDisplayFormat) : absValue.ToString(CurrencySmallDisplayFormat);

            if (Value < -0.49M || (FormatStyle & NumberFormatStyle.Negative) == NumberFormatStyle.Negative)
            {
                result = string.Format(CurrencyNegativeFormat, result);
            }
            if ((FormatStyle & NumberFormatStyle.Cogs) == NumberFormatStyle.Cogs)
            {
                result = string.Format(CogsFormat, result);
            }
            if ((FormatStyle & NumberFormatStyle.Margin) == NumberFormatStyle.Margin)
            {
                result = string.Format(MarginFormat, result);
            }
            if ((FormatStyle & NumberFormatStyle.Revenue) == NumberFormatStyle.Revenue)
            {
                result = string.Format(RevenueFormat, result);
            }
            if ((FormatStyle & NumberFormatStyle.Total) == NumberFormatStyle.Total)
            {
                result = string.Format(TotalFormat, result);
            }
            if ((FormatStyle & NumberFormatStyle.Percent) == NumberFormatStyle.Percent)
            {
                result = string.Format(PercentFormat, absValue);
            }

            return result;
        }

        public string ToString(bool personsWithHigherSeniorityExists)
        {
            decimal absValue = Math.Abs(Value);
            string result = absValue.ToString(CurrencyLargeDisplayFormat);

            if (Value < 0 || (FormatStyle & NumberFormatStyle.Negative) == NumberFormatStyle.Negative)
            {
                result = string.Format(CurrencyNegativeFormat, personsWithHigherSeniorityExists ? HideText : result);
            }
            if ((FormatStyle & NumberFormatStyle.Cogs) == NumberFormatStyle.Cogs)
            {
                result = string.Format(CogsFormat, personsWithHigherSeniorityExists ? HideText : result);
            }
            if ((FormatStyle & NumberFormatStyle.Margin) == NumberFormatStyle.Margin)
            {
                result = string.Format(Value < 0 ? BenchFormat : MarginFormat, personsWithHigherSeniorityExists ? HideText : result);
            }
            if ((FormatStyle & NumberFormatStyle.Revenue) == NumberFormatStyle.Revenue)
            {
                result = string.Format(RevenueFormat, personsWithHigherSeniorityExists ? HideText : result);
            }
            if ((FormatStyle & NumberFormatStyle.Total) == NumberFormatStyle.Total)
            {
                result = string.Format(TotalFormat, personsWithHigherSeniorityExists ? HideText : result);
            }
            if ((FormatStyle & NumberFormatStyle.Percent) == NumberFormatStyle.Percent)
            {
                result = string.Format(PercentFormat, personsWithHigherSeniorityExists ? HideText : absValue.ToString());
            }

            return result;
        }

        /// <summary>
        /// Returns a hash code for the value.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Determines an equality to the specified value.
        /// </summary>
        /// <param name="obj">The value to compare with.</param>
        /// <returns>true if equal and false otherwise.</returns>
        public override bool Equals(object obj)
        {
            bool result;
            try
            {
                result = obj != null && Value == Convert.ToDecimal(obj);
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        #endregion Methods

        #region Operators

        #region Conversion

        public static implicit operator decimal(PracticeManagementCurrency value)
        {
            return value.Value;
        }

        public static implicit operator PracticeManagementCurrency(decimal value)
        {
            PracticeManagementCurrency result = new PracticeManagementCurrency { Value = value };
            return result;
        }

        #endregion Conversion

        #region Arithmetic

        public static PracticeManagementCurrency operator -(PracticeManagementCurrency value)
        {
            return -value.Value;
        }

        public static PracticeManagementCurrency operator +(PracticeManagementCurrency v1,
            PracticeManagementCurrency v2)
        {
            return v1.Value + v2.Value;
        }

        public static PracticeManagementCurrency operator -(PracticeManagementCurrency v1,
            PracticeManagementCurrency v2)
        {
            return v1.Value - v2.Value;
        }

        public static PracticeManagementCurrency operator *(PracticeManagementCurrency v1,
            PracticeManagementCurrency v2)
        {
            return v1.Value * v2.Value;
        }

        public static PracticeManagementCurrency operator /(PracticeManagementCurrency v1,
            PracticeManagementCurrency v2)
        {
            return v1.Value / v2.Value;
        }

        #endregion Arithmetic

        #region Boolean

        public static bool operator ==(PracticeManagementCurrency v1, PracticeManagementCurrency v2)
        {
            return v1.Value == v2.Value;
        }

        public static bool operator !=(PracticeManagementCurrency v1, PracticeManagementCurrency v2)
        {
            return v1.Value != v2.Value;
        }

        public static bool operator <(PracticeManagementCurrency v1, PracticeManagementCurrency v2)
        {
            return v1.Value < v2.Value;
        }

        public static bool operator >(PracticeManagementCurrency v1, PracticeManagementCurrency v2)
        {
            return v1.Value > v2.Value;
        }

        public static bool operator <=(PracticeManagementCurrency v1, PracticeManagementCurrency v2)
        {
            return v1.Value <= v2.Value;
        }

        public static bool operator >=(PracticeManagementCurrency v1, PracticeManagementCurrency v2)
        {
            return v1.Value >= v2.Value;
        }

        #endregion Boolean

        #endregion Operators
    }
}
