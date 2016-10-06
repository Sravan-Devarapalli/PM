﻿using System;
using System.Collections.Generic;
using DataTransferObjects;
using DataTransferObjects.TimeEntry;

namespace DataAccess
{
    public class Utils
    {
        /// <summary>
        /// 	Converts CSV into int array
        /// </summary>
        /// <param name = "csv">Comma separated int values</param>
        /// <returns>Array of integers</returns>
        public static int[] StringToIntArray(string csv)
        {
            csv = csv.TrimEnd(DataTransferObjects.Utils.Generic.StringListSeparator);
            var values = csv.Split(DataTransferObjects.Utils.Generic.StringListSeparator);

            var res = new int[values.Length];

            for (var i = 0; i < values.Length; i++)
                res[i] = Convert.ToInt32(values[i]);

            return res;
        }

        public static List<Person> stringToProjectManagersList(string csv)
        {
            List<Person> pmanagers = new List<Person>();
            string[] idFirstnameLastNamesList = csv.Split(DataTransferObjects.Utils.Generic.LastNameSeperator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var idFirstnameLastName in idFirstnameLastNamesList)
            {
                string[] pm = idFirstnameLastName.Split(DataTransferObjects.Utils.Generic.Seperators, StringSplitOptions.RemoveEmptyEntries);

                if (pm.Length != 3) continue;
                int id = Convert.ToInt32(pm[0]);
                string firstName = pm[1];
                string lastName = pm[2];

                pmanagers.Add(new Person
                    {
                        Id = id,
                        FirstName = firstName,
                        LastName = lastName
                    });
            }

            return pmanagers.Count > 0 ? pmanagers : null;
        }

        #region Utils

        /// <summary>
        /// 	Converts ReviewStatus to bool?
        /// </summary>
        public static bool? ReviewStatus2Bool(ReviewStatus status)
        {
            switch (status)
            {
                case ReviewStatus.Approved:
                    return true;

                case ReviewStatus.Declined:
                    return false;
            }

            return null;
        }

        /// <summary>
        /// 	Converts ReviewStatus to bool?
        /// </summary>
        public static int? ReviewStatus2Int(string status)
        {
            if (status == null)
                return null;

            var reviewStatus = (ReviewStatus)Enum.Parse(typeof(ReviewStatus), status);

            switch (reviewStatus)
            {
                case ReviewStatus.Approved:
                    return 1;

                case ReviewStatus.Declined:
                    return 0;
            }

            return 2; // Needed to disctinct values in the database
        }

        /// <summary>
        /// 	Converts bool? to ReviewStatus
        /// </summary>
        public static ReviewStatus Bool2ReviewStatus(bool? status)
        {
            if (status.HasValue)
                return status.Value
                           ? ReviewStatus.Approved
                           : ReviewStatus.Declined;

            return ReviewStatus.Pending;
        }

        public static DateTime MonthEndDate(DateTime now)
        {
            return now.AddMonths(1).AddDays(-now.AddMonths(1).Day);
        }

        public static DateTime MonthStartDate(DateTime now)
        {
            return now.AddDays(1 - now.Day);
        }

        public static List<DateTime> GetMonthYearWithInThePeriod(DateTime startDate, DateTime endDate)
        {
            startDate = MonthStartDate(startDate).Date;
            endDate = MonthStartDate(endDate).Date;

            List<DateTime> result = new List<DateTime>();
            if (startDate <= endDate)
            {
                while (startDate <= endDate)
                {
                    result.Add(startDate);
                    startDate = startDate.AddMonths(1);
                }
            }
            return result;
        }

        #endregion Utils
    }
}
