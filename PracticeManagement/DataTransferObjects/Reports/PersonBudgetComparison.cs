using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataTransferObjects.Reports
{
    [DataContract]
    [Serializable]
    public class PersonBudgetComparison
    {
        [DataMember]
        public Person Person
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<DateTime, decimal> ActualHours
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<DateTime, decimal> ProjectedHours
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<DateTime, decimal> BudgetHours
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<DateTime, decimal> ProjectedRemainingHours
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<DateTime, decimal> EACHours
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<DateTime, PayRate> ProjectedAndActualBillRate
        {
            get;
            set;
        }

        [DataMember]
        public Dictionary<DateTime, PayRate> BudgetBillRate
        {
            get;
            set;
        }

        public decimal TotalBudgetHours
        {
            get
            {
                if (BudgetHours != null)
                {
                    return BudgetHours.Sum(b => b.Value);
                }
                return 0;
            }
        }

        public decimal TotalProjectedHours
        {
            get
            {
                if (ProjectedHours != null)
                {
                    return ProjectedHours.Sum(p => p.Value);
                }
                return 0;
            }
        }

        public decimal TotalActualHours
        {
            get
            {
                if (ActualHours != null)
                {
                    return ActualHours.Sum(a => a.Value);
                }
                return 0;
            }
        }

        public decimal TotalEACHours
        {
            get
            {
                if (EACHours != null)
                {
                    return EACHours.Sum(e => e.Value);
                }
                return 0;
            }
        }

        public PracticeManagementCurrency BudgetRevenue
        {
            get
            {
                PracticeManagementCurrency _revenue = 0M;
                if (BudgetHours != null && BudgetBillRate != null)
                {
                    foreach (var hoursData in BudgetHours)
                    {
                        var b = BudgetBillRate.ToList();
                        KeyValuePair<DateTime, PayRate> last = new KeyValuePair<DateTime, PayRate>(new DateTime(2029, 01, 01), null);
                        b.Add(last);
                        for (int i = 0; i < b.Count - 1; i++)
                        {
                            var first = new DateTime(b[i].Key.Year, b[i].Key.Month, 01);
                            var second = new DateTime(b[i + 1].Key.Year, b[i + 1].Key.Month, 01);
                            if (first <= hoursData.Key && second > hoursData.Key)
                            {
                                _revenue += hoursData.Value * b[i].Value.BillRate;
                            }
                        }
                    }
                }
                return _revenue;
            }
        }

        public PracticeManagementCurrency BudgetCost
        {
            get
            {
                PracticeManagementCurrency _revenue = 0M;
                if (BudgetHours != null && BudgetBillRate != null)
                {
                    foreach (var hoursData in BudgetHours)
                    {
                        var b = BudgetBillRate.ToList();
                        KeyValuePair<DateTime, PayRate> last = new KeyValuePair<DateTime, PayRate>(new DateTime(2029, 01, 01), null);
                        b.Add(last);
                        for (int i = 0; i < b.Count - 1; i++)
                        {
                            var first = new DateTime(b[i].Key.Year, b[i].Key.Month, 01);
                            var second = new DateTime(b[i + 1].Key.Year, b[i + 1].Key.Month, 01);
                            if (first <= hoursData.Key && second > hoursData.Key)
                            {
                                _revenue += hoursData.Value * b[i].Value.PersonCost;
                            }
                        }
                    }
                }
                return _revenue;
            }
        }

        public PracticeManagementCurrency BudgetMargin
        {
            get
            {
                return BudgetRevenue - BudgetCost;
            }
        }

        public PracticeManagementCurrency ActualRevenue
        {
            get
            {
                PracticeManagementCurrency _revenue = 0M;
                if (ActualHours != null && ProjectedAndActualBillRate != null)
                {
                    foreach (var hoursData in ActualHours)
                    {
                        var b = ProjectedAndActualBillRate.ToList();
                        KeyValuePair<DateTime, PayRate> last = new KeyValuePair<DateTime, PayRate>(new DateTime(2029, 01, 01), null);
                        b.Add(last);
                        for (int i = 0; i < b.Count - 1; i++)
                        {
                            var first = new DateTime(b[i].Key.Year, b[i].Key.Month, 01);
                            var second = new DateTime(b[i + 1].Key.Year, b[i + 1].Key.Month, 01);
                            if (first <= hoursData.Key && second > hoursData.Key)
                            {
                                _revenue += hoursData.Value * b[i].Value.BillRate;
                            }
                        }
                    }
                }
                return _revenue;
            }
        }

        public PracticeManagementCurrency ActualCost
        {
            get
            {
                PracticeManagementCurrency _revenue = 0M;
                if (ActualHours != null && ProjectedAndActualBillRate != null)
                {
                    foreach (var hoursData in ActualHours)
                    {
                        var b = ProjectedAndActualBillRate.ToList();
                        KeyValuePair<DateTime, PayRate> last = new KeyValuePair<DateTime, PayRate>(new DateTime(2029, 01, 01), null);
                        b.Add(last);
                        for (int i = 0; i < b.Count - 1; i++)
                        {
                            var first = new DateTime(b[i].Key.Year, b[i].Key.Month, 01);
                            var second = new DateTime(b[i + 1].Key.Year, b[i + 1].Key.Month, 01);
                            if (first <= hoursData.Key && second > hoursData.Key)
                            {
                                _revenue += hoursData.Value * b[i].Value.PersonCost;
                            }
                        }
                    }
                }
                return _revenue;
            }
        }

        public PracticeManagementCurrency ActualMargin
        {
            get
            {
                return ActualRevenue - ActualCost;
            }
        }

        public PracticeManagementCurrency ProjectedRevenue
        {
            get
            {
                PracticeManagementCurrency _revenue = 0M;

                if (ProjectedHours != null && ProjectedAndActualBillRate != null)
                {
                    foreach (var hoursData in ProjectedHours)
                    {
                        var b = ProjectedAndActualBillRate.ToList();
                        KeyValuePair<DateTime, PayRate> last = new KeyValuePair<DateTime, PayRate>(new DateTime(2029, 01, 01), null);
                        b.Add(last);
                        for (int i = 0; i < b.Count - 1; i++)
                        {
                            var first = new DateTime(b[i].Key.Year, b[i].Key.Month, 01);
                            var second = new DateTime(b[i + 1].Key.Year, b[i + 1].Key.Month, 01);
                            if (first <= hoursData.Key && second > hoursData.Key)
                            {
                                _revenue += hoursData.Value * b[i].Value.BillRate;
                            }
                        }
                    }
                }
                return _revenue;
            }
        }

        public PracticeManagementCurrency ProjectedCost
        {
            get
            {
                PracticeManagementCurrency _revenue = 0M;
                if (ProjectedHours != null && ProjectedAndActualBillRate != null)
                {
                    foreach (var hoursData in ProjectedHours)
                    {
                        var b = ProjectedAndActualBillRate.ToList();
                        KeyValuePair<DateTime, PayRate> last = new KeyValuePair<DateTime, PayRate>(new DateTime(2029, 01, 01), null);
                        b.Add(last);
                        for (int i = 0; i < b.Count - 1; i++)
                        {
                            var first = new DateTime(b[i].Key.Year, b[i].Key.Month, 01);
                            var second = new DateTime(b[i + 1].Key.Year, b[i + 1].Key.Month, 01);
                            if (first <= hoursData.Key && second > hoursData.Key)
                            {
                                _revenue += hoursData.Value * b[i].Value.PersonCost;
                            }
                        }
                    }
                }
                return _revenue;
            }
        }

        public PracticeManagementCurrency ProjectedMargin
        {
            get
            {
                return ProjectedRevenue - ProjectedCost;
            }
        }

        public PracticeManagementCurrency EACRevenue
        {
            get
            {
                PracticeManagementCurrency _revenue = 0M;
                if (EACHours != null && ProjectedAndActualBillRate != null)
                {
                    foreach (var hoursData in EACHours)
                    {
                        var b = ProjectedAndActualBillRate.ToList();
                        KeyValuePair<DateTime, PayRate> last = new KeyValuePair<DateTime, PayRate>(new DateTime(2029, 01, 01), null);
                        b.Add(last);
                        for (int i = 0; i < b.Count - 1; i++)
                        {
                            var first = new DateTime(b[i].Key.Year, b[i].Key.Month, 01);
                            var second = new DateTime(b[i + 1].Key.Year, b[i + 1].Key.Month, 01);
                            if (first <= hoursData.Key && second > hoursData.Key)
                            {
                                _revenue += hoursData.Value * b[i].Value.BillRate;
                            }
                        }
                    }
                }
                return _revenue;
            }
        }

        public PracticeManagementCurrency EACCost
        {
            get
            {
                PracticeManagementCurrency _revenue = 0M;
                if (EACHours != null && ProjectedAndActualBillRate != null)
                {
                    foreach (var hoursData in EACHours)
                    {
                        var b = ProjectedAndActualBillRate.ToList();
                        KeyValuePair<DateTime, PayRate> last = new KeyValuePair<DateTime, PayRate>(new DateTime(2029, 01, 01), null);
                        b.Add(last);
                        for (int i = 0; i < b.Count - 1; i++)
                        {
                            var first = new DateTime(b[i].Key.Year, b[i].Key.Month, 01);
                            var second = new DateTime(b[i + 1].Key.Year, b[i + 1].Key.Month, 01);
                            if (first <= hoursData.Key && second > hoursData.Key)
                            {
                                _revenue += hoursData.Value * b[i].Value.PersonCost;
                            }
                        }
                    }
                }
                return _revenue;
            }
        }

        public PracticeManagementCurrency EACMargin
        {
            get
            {
                return EACRevenue - EACCost;
            }
        }

        [DataMember]
        public ComputedFinancials Financials
        {
            get;
            set;
        }
    }
}

