using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObjects
{
    [DataContract]
    [Serializable]
    public class ProjectRevenue
    {
        private PracticeManagementCurrency _revenueValue;
        private PracticeManagementCurrency _summaryRevenueValue;
        private PracticeManagementCurrency _marginValue;

        public PracticeManagementCurrency BillRate
        {
            get {
                return (Hours != 0) ? Revenue / Hours : 0;
            }
        }

        [DataMember]
        public decimal Hours
        {
            get;
            set;
        }

        [DataMember]
        public PracticeManagementCurrency Revenue
        {
            get { return _revenueValue; }
            set
            {
                _revenueValue = value;
                _revenueValue.FormatStyle = NumberFormatStyle.Revenue;
                _revenueValue.DoNotShowDecimals = true;
            }
        }

        [DataMember]
        public PracticeManagementCurrency Margin
        {
            get { return _marginValue; }
            set
            {
                _marginValue = value;
                _marginValue.FormatStyle = NumberFormatStyle.Margin;
                _marginValue.DoNotShowDecimals = true;
            }
        }

        public PracticeManagementCurrency MarginRate
        {
            get
            {
                return (Hours != 0) ? Margin / Hours : 0;
            }
        }

        //[DataMember]
        //public PracticeManagementCurrency ServiceRevenue
        //{
        //    get { return _summaryRevenueValue; }
        //    set
        //    {
        //        _summaryRevenueValue = value;
        //        _summaryRevenueValue.FormatStyle = NumberFormatStyle.Revenue;
        //    }
        //}

        public PracticeManagementCurrency TotalRevenue
        {
            get
            {
                return Revenue + Expenses;
            }
        }

        public PracticeManagementCurrency TotalMargin
        {
            get
            {
                return Margin + Expenses;
            }
        }

       
        [DataMember]
        public decimal Expenses { get; set; }
    }
}

