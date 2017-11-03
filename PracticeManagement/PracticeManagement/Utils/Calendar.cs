﻿using System;
using System.Collections.Generic;
using DataTransferObjects;

namespace PraticeManagement.Utils
{
    public class Calendar
    {
        public const int DefaultHoursPerWeek = 40;

        public static string GetCssClassByCalendarItem(CalendarItem calendarItem)
        {
            if (calendarItem == null)
                return string.Empty;

            return calendarItem.DayOff
                        ? (calendarItem.CompanyDayOff
                        ? (calendarItem.Date.DayOfWeek == DayOfWeek.Sunday || calendarItem.Date.DayOfWeek == DayOfWeek.Saturday ? Resources.Controls.CssWeekEndDayOff : Resources.Controls.CssDayOff)
                            : (calendarItem.Date.DayOfWeek == DayOfWeek.Sunday || calendarItem.Date.DayOfWeek == DayOfWeek.Saturday ? Resources.Controls.CssWeekEndDayOff : calendarItem.IsFloatingHoliday ? Resources.Controls.CssCompanyDayOnFloatingDay : calendarItem.IsFloatingVacation?Resources.Controls.CSSFloatingHoliday: Resources.Controls.CssCompanyDayOn)
                          )
                        : (calendarItem.CompanyDayOff
                            ? (calendarItem.Date.DayOfWeek == DayOfWeek.Sunday || calendarItem.Date.DayOfWeek == DayOfWeek.Saturday ? Resources.Controls.CssWeekEndDayOn : Resources.Controls.CssCompanyDayOff)
                            : (calendarItem.Date.DayOfWeek == DayOfWeek.Sunday || calendarItem.Date.DayOfWeek == DayOfWeek.Saturday ? Resources.Controls.CssWeekEndDayOn : Resources.Controls.CssDayOn)
                          );
        }

        public static DateTime WeekStartDate(DateTime now)
        {
            return now.AddDays(-(int)now.DayOfWeek);
        }

        public static DateTime WeekEndDate(DateTime now)
        {
            return now.AddDays(6 - (int)now.DayOfWeek);
        }

        public static DateTime MonthStartDate(DateTime now)
        {
            return now.AddDays(1 - now.Day);
        }

        public static DateTime PayrollCurrentStartDate(DateTime now)
        {
            return now.Day < 16 ? MonthStartDate(now) : CurrentMonthSecondHalfStartDate(now);
        }

        public static DateTime PayrollCurrentEndDate(DateTime now)
        {
            return now.Day < 16 ? CurrentMonthFirstHalfEndDate(now) : MonthEndDate(now);
        }

        public static DateTime PayrollPerviousStartDate(DateTime now)
        {
            return now.Day < 16 ? LastMonthSecondHalfStartDate(now) : MonthStartDate(now);
        }

        public static DateTime PayrollPerviousEndDate(DateTime now)
        {
            return now.Day < 16 ? LastMonthEndDate(now) : CurrentMonthFirstHalfEndDate(now);
        }

        public static DateTime MonthEndDate(DateTime now)
        {
            return now.AddMonths(1).AddDays(-now.AddMonths(1).Day);
        }

        public static DateTime YearStartDate(DateTime now)
        {
            return now.AddDays(1 - now.DayOfYear);
        }

        public static DateTime MonthStartDateByMonthNumber(DateTime now, int MonthNumber)
        {
            return now.AddMonths(MonthNumber - 1).AddDays(1 - now.AddMonths(-MonthNumber).Day);
        }

        public static DateTime YearEndDate(DateTime now)
        {
            return now.AddYears(1).AddDays(-now.AddYears(1).DayOfYear);
        }

        public static DateTime LastWeekStartDate(DateTime now)
        {
            return now.AddDays(-(int)now.DayOfWeek - 7);
        }

        public static DateTime LastWeekEndDate(DateTime now)
        {
            return now.AddDays(-(int)now.DayOfWeek - 1);
        }

        public static DateTime LastMonthStartDate(DateTime now)
        {
            return now.AddMonths(-1).AddDays(1 - now.AddMonths(-1).Day);
        }

        public static DateTime Last3MonthStartDate(DateTime now)
        {
            return MonthStartDate(now.AddMonths(-3));
        }

        public static DateTime Last6MonthStartDate(DateTime now)
        {
            return MonthStartDate(now.AddMonths(-6));
        }

        public static DateTime Last9MonthStartDate(DateTime now)
        {
            return MonthStartDate(now.AddMonths(-9));
        }

        public static DateTime Last12MonthStartDate(DateTime now)
        {
            return MonthStartDate(now.AddMonths(-12));
        }

        //Current month + next 3 months
        public static DateTime Next4MonthEndDate(DateTime now)
        {
            return MonthEndDate(now.AddMonths(3));
        }

        //Current month + next 2 months
        public static DateTime Next3MonthEndDate(DateTime now)
        {
            return MonthEndDate(now.AddMonths(2));
        }

        //Current month + next 1 months
        public static DateTime Next2MonthEndDate(DateTime now)
        {
            return MonthEndDate(now.AddMonths(1));
        }

        //return 16th of the last month
        public static DateTime LastMonthSecondHalfStartDate(DateTime now)
        {
            return now.AddMonths(-1).AddDays(16 - now.AddMonths(-1).Day);
        }

        //return 16th of the Current month
        public static DateTime CurrentMonthSecondHalfStartDate(DateTime now)
        {
            return now.AddDays(16 - now.AddMonths(-1).Day);
        }

        //returns 15th of the current month
        public static DateTime CurrentMonthFirstHalfEndDate(DateTime now)
        {
            return now.AddDays(15 - now.Day);
        }

        public static DateTime LastMonthEndDate(DateTime now)
        {
            return now.AddDays(-now.Day);
        }

        public static DateTime LastYearStartDate(DateTime now)
        {
            return now.AddYears(-1).AddDays(1 - now.AddYears(-1).DayOfYear);
        }

        public static DateTime LastYearEndDate(DateTime now)
        {
            return now.AddDays(-now.DayOfYear);
        }

        public static DateTime QuarterStartDate(DateTime now, int quater)
        {
            switch (quater)
            {
                case 1:
                    return YearStartDate(now);
                case 2:
                    return new DateTime(now.Year, 4, 1);
                case 3:
                    return new DateTime(now.Year, 7, 1);
                default:
                    return new DateTime(now.Year, 10, 1);
            }
        }

        public static DateTime QuarterEndDate(DateTime now, int quater)
        {
            switch (quater)
            {
                case 1:
                    return new DateTime(now.Year, 3, 31);
                case 2:
                    return new DateTime(now.Year, 6, 30);
                case 3:
                    return new DateTime(now.Year, 9, 30);
                default:
                    return YearEndDate(now);
            }
        }

        public static List<string> GetMonthYearWithInThePeriod(DateTime startDate, DateTime endDate)
        {
            startDate = MonthStartDate(startDate).Date;
            endDate = MonthStartDate(endDate).Date;

            List<string> result = new List<string>();
            if (startDate <= endDate)
            {
                while (startDate <= endDate)
                {
                    result.Add(startDate.ToString(Constants.Formatting.FullMonthYearFormat));
                    startDate = startDate.AddMonths(1);
                }
            }
            return result;
        }

        public static int GetMonths(DateTime startDate, DateTime endDate)
        {
            int months = ((endDate.Year * 12) + endDate.Month) - ((startDate.Year * 12) + startDate.Month);

            if (endDate.Day < startDate.Day)
            {
                months--;
            }

            return months;
        }

        public static DateTime CurrentQuarterStartDate(DateTime now)
        {
            int quater = (int)Math.Ceiling(now.Month / 3.0);

            return QuarterStartDate(now, quater);
        }

        public static DateTime CurrentQuarterEndDate(DateTime now)
        {
            int quater = (int)Math.Ceiling(now.Month / 3.0);

            return QuarterEndDate(now, quater);
        }

        public static DateTime NextQuarterStartDate(DateTime now)
        {
            int quater = (int)Math.Ceiling(now.Month / 3.0)+1;
            if (quater == 5)
            {
                now = now.AddYears(1);
                quater = 1;
            }

            return QuarterStartDate(now, quater);
        }

        public static DateTime NextQuarterEndDate(DateTime now)
        {
            int quater = (int)Math.Ceiling(now.Month / 3.0)+1;
            if (quater == 5)
            {
                now = now.AddYears(1);
                quater = 1;
            }

            return QuarterEndDate(now, quater);
        }

        public static DateTime CurrentHalfStartDate(DateTime now)
        {
            if (now.Month <= 6)
            {
                return new DateTime(now.Year, 1, 1);
            }
            else
            {
                return new DateTime(now.Year, 7, 1);
            }
        }
        public static DateTime CurrentHalfEndDate(DateTime now)
        {
            if (now.Month <= 6)
            {
                return new DateTime(now.Year, 6, 30);
            }
            else
            {
                return new DateTime(now.Year, 12, 31);
            }
        }

        public static DateTime NextHalfStartDate(DateTime now)
        {
            if (now.Month <= 6)
            {
                return new DateTime(now.Year, 7, 1);
            }
            else
            {
                return new DateTime(now.Year + 1, 1, 1);
            }
        }
        public static DateTime NextHalfEndDate(DateTime now)
        {
            if (now.Month <= 6)
            {
                return new DateTime(now.Year, 12, 31);
            }
            else
            {
                return new DateTime(now.Year + 1, 6, 30);
            }
        }
    }
}

