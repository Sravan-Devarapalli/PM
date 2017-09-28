﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using DataAccess.Other;
using DataTransferObjects;

namespace DataAccess
{
    /// <summary>
    /// Access pay data in database
    /// </summary>
    public static class PayDAL
    {
        #region Constants

        #region Parameters

        private const string AmountParam = "@Amount";
        private const string TimescaleParam = "@Timescale";
        private const string VacationDaysParam = "@VacationDays";
        private const string BonusAmountParam = "@BonusAmount";
        private const string BonusHoursToCollectParam = "@BonusHoursToCollect";
        private const string OldStartDateParam = "@OLD_StartDate";
        private const string OldEndDateParam = "@OLD_EndDate";
        private const string PracticeIdParam = "@PracticeId";
        private const string TitleIdParam = "@TitleId";
        private const string SLTApprovalParam = "@SLTApproval";
        private const string SLTPTOApprovalParam = "@SLTPTOApproval";

        #endregion Parameters

        #region Columns

        private const string PersonIdColumn = "PersonId";
        private const string StartDateColumn = "StartDate";
        private const string EndDateColumn = "EndDate";
        private const string AmountColumn = "Amount";
        private const string TimescaleNameColumn = "TimescaleName";
        private const string AmountHourlyColumn = "AmountHourly";
        private const string VacationDaysColumn = "VacationDays";
        private const string BonusAmountColumn = "BonusAmount";
        private const string BonusHoursToCollectColumn = "BonusHoursToCollect";
        private const string IsYearBonusColumn = "IsYearBonus";
        private const string PracticeIdColumn = "PracticeId";
        private const string PracticeNameColumn = "PracticeName";
        private const string TitleColumn = "Title";
        private const string TitleIdColumn = "TitleId";
        private const string SLTApprovalColumn = "SLTApproval";
        private const string SLTPTOApprovalColumn = "SLTPTOApproval";

        #endregion Columns

        #endregion Constants

        #region Methods

        /// <summary>
        /// Retrieves a current pay for the specified person.
        /// </summary>
        /// <param name="personId">An ID of the <see cref="Person"/> to retrieve the data for.</param>
        /// <returns>The <see cref="Pay"/> object if found any or null otherwise.</returns>
        public static Pay GetCurrentByPerson(int personId)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.Pay.PayGetCurrentByPersonProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonIdParam, personId);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    List<Pay> result = new List<Pay>();
                    ReadPay(reader, result);

                    return result.Count > 0 ? result[0] : null;
                }
            }
        }

        /// <summary>
        /// Retrives a payment history for the specified person.
        /// </summary>
        /// <param name="personId">An ID of the <see cref="Person"/> to retrieve the data for.</param>
        /// <returns>The list of the <see cref="Pay"/> objects.</returns>
        public static List<Pay> GetHistoryByPerson(int personId)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.Pay.PayGetHistoryByPersonProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonIdParam, personId);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    List<Pay> result = new List<Pay>();
                    ReadPay(reader, result);

                    return result;
                }
            }
        }

        public static List<Pay> GetPayHistoryShortByPerson(int personId)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.Pay.GetPayHistoryShortByPersonProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonIdParam, personId);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    List<Pay> result = new List<Pay>();
                    ReadPayShort(reader, result);

                    return result;
                }
            }
        }

        /// <summary>
        /// Saves a <see cref="Pay"/> object to the database.
        /// </summary>
        /// <param name="pay">The <see cref="Pay"/> object to be saved.</param>
        public static bool SavePayDatail(Pay pay, SqlConnection connection = null, SqlTransaction activeTransaction = null, string user = null)
        {
            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }

            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.Pay.PaySaveProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonIdParam, pay.PersonId);
                command.Parameters.AddWithValue(AmountParam, pay.Amount.Value);
                command.Parameters.AddWithValue(TimescaleParam, pay.Timescale);
                command.Parameters.AddWithValue(VacationDaysParam, pay.VacationDays.HasValue ? (object)pay.VacationDays.Value : DBNull.Value);
                command.Parameters.AddWithValue(BonusAmountParam, pay.BonusAmount.Value);
                command.Parameters.AddWithValue(BonusHoursToCollectParam,
                    pay.BonusHoursToCollect.HasValue ? (object)pay.BonusHoursToCollect.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.StartDateParam, pay.StartDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.ValidateAttribution, pay.ValidateAttribution);
                command.Parameters.AddWithValue(Constants.ParameterNames.EndDateParam,
                    pay.EndDate.HasValue ? (object)pay.EndDate.Value : DBNull.Value);
                command.Parameters.AddWithValue(OldStartDateParam,
                    pay.OldStartDate.HasValue ? (object)pay.OldStartDate.Value : DBNull.Value);
                command.Parameters.AddWithValue(OldEndDateParam,
                    pay.OldEndDate.HasValue ? (object)pay.OldEndDate.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.DivisionId, pay.DivisionId.HasValue ? (object)pay.DivisionId.Value : DBNull.Value);
                command.Parameters.AddWithValue(PracticeIdParam,
                    pay.PracticeId.HasValue ? (object)pay.PracticeId.Value : DBNull.Value);
                command.Parameters.AddWithValue(TitleIdParam, pay.TitleId.HasValue ? (object)pay.TitleId : DBNull.Value);
                command.Parameters.AddWithValue(SLTApprovalParam, pay.SLTApproval);
                command.Parameters.AddWithValue(SLTPTOApprovalParam, pay.SLTPTOApproval);
                command.Parameters.AddWithValue(Constants.ParameterNames.VendorId, pay.vendor != null ? (object)pay.vendor.Id : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.UserLoginParam, string.IsNullOrEmpty(user) ? DBNull.Value : (object)user);
                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    if (activeTransaction != null)
                    {
                        command.Transaction = activeTransaction;
                    }

                    return ((bool)command.ExecuteScalar());
                }
                catch (SqlException ex)
                {
                    throw new DataAccessException(ex);
                }
            }
        }

        public static void DeletePay(int personId, DateTime startDate)
        {
            var connection = new SqlConnection(DataSourceHelper.DataConnection);

            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.Pay.PayDeleteProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonIdParam, personId);
                command.Parameters.AddWithValue(Constants.ParameterNames.StartDateParam, startDate);

                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw new DataAccessException(ex);
                }
            }
        }

        private static void ReadPayShort(DbDataReader reader, List<Pay> result)
        {
            if (!reader.HasRows) return;
            int startDateIndex = reader.GetOrdinal(StartDateColumn);
            int endDateIndex = reader.GetOrdinal(EndDateColumn);
            int timescaleIndex = reader.GetOrdinal(Constants.ColumnNames.TimescaleColumn);

            while (reader.Read())
            {
                Pay pay = new Pay
                {
                    Timescale = (TimescaleType)reader.GetInt32(timescaleIndex),
                    StartDate = reader.GetDateTime(startDateIndex),
                    EndDate = !reader.IsDBNull(endDateIndex) ? (DateTime?)reader.GetDateTime(endDateIndex) : null
                };

                result.Add(pay);
            }
        }

        private static void ReadPay(DbDataReader reader, List<Pay> result)
        {
            if (!reader.HasRows) return;
            int personIdIndex = reader.GetOrdinal(PersonIdColumn);
            int startDateIndex = reader.GetOrdinal(StartDateColumn);
            int endDateIndex = reader.GetOrdinal(EndDateColumn);
            int amountIndex = reader.GetOrdinal(AmountColumn);
            int timescaleIndex = reader.GetOrdinal(Constants.ColumnNames.TimescaleColumn);
            int timescaleNameIndex = reader.GetOrdinal(TimescaleNameColumn);
            int amountHourlyIndex = reader.GetOrdinal(AmountHourlyColumn);
            int vacationDaysIndex = reader.GetOrdinal(VacationDaysColumn);
            int bonusAmountIndex = reader.GetOrdinal(BonusAmountColumn);
            int bonusHoursToCollectIndex = reader.GetOrdinal(BonusHoursToCollectColumn);
            int isYearBonusIndex = reader.GetOrdinal(IsYearBonusColumn);
            int practiceIdIndex = reader.GetOrdinal(PracticeIdColumn);
            int practiceNameIndex = reader.GetOrdinal(PracticeNameColumn);
            int titleColumnIndex = reader.GetOrdinal(TitleColumn);
            int titleIdColumnIndex = reader.GetOrdinal(TitleIdColumn);
            int sLTApprovalColumnIndex = reader.GetOrdinal(SLTApprovalColumn);
            int sLTPTOApprovalColumnIndex = reader.GetOrdinal(SLTPTOApprovalColumn);
            int divisionIdIndex = reader.GetOrdinal(Constants.ColumnNames.DivisionId);
            int divisionNameIndex = reader.GetOrdinal(Constants.ColumnNames.DivisionName);
            int vendorIdIndex = reader.GetOrdinal(Constants.ColumnNames.VendorId);
            int vendorNameIndex = reader.GetOrdinal(Constants.ColumnNames.VendorName);
            int isRtpIndex = -1;
            try
            {
                isRtpIndex = reader.GetOrdinal(Constants.ColumnNames.IsRightToPresent);
            }
            catch
            {
                isRtpIndex = -1;
            }
            while (reader.Read())
            {
                Pay pay = new Pay
                {
                    PersonId = reader.GetInt32(personIdIndex),
                    Timescale = (TimescaleType)reader.GetInt32(timescaleIndex),
                    TimescaleName = reader.GetString(timescaleNameIndex),
                    Amount = reader.GetDecimal(amountIndex),
                    StartDate = reader.GetDateTime(startDateIndex),
                    EndDate =
                            !reader.IsDBNull(endDateIndex) ? (DateTime?)reader.GetDateTime(endDateIndex) : null,
                    AmountHourly = reader.GetDecimal(amountHourlyIndex),
                    VacationDays =
                            !reader.IsDBNull(vacationDaysIndex) ? (int?)reader.GetInt32(vacationDaysIndex) : null,
                    BonusAmount = reader.GetDecimal(bonusAmountIndex),
                    BonusHoursToCollect = !reader.IsDBNull(bonusHoursToCollectIndex)
                                                  ? (int?)reader.GetInt32(bonusHoursToCollectIndex)
                                                  : null,
                    IsYearBonus = reader.GetBoolean(isYearBonusIndex),
                    PracticeId =
                            !reader.IsDBNull(practiceIdIndex) ? (int?)reader.GetInt32(practiceIdIndex) : null,
                    PracticeName =
                            !reader.IsDBNull(practiceNameIndex) ? reader.GetString(practiceNameIndex) : string.Empty
                };

                if (!reader.IsDBNull(titleIdColumnIndex))
                {
                    pay.TitleId = reader.GetInt32(titleIdColumnIndex);
                    pay.TitleName = reader.GetString(titleColumnIndex);
                }

                if (!reader.IsDBNull(divisionIdIndex))
                {
                    pay.DivisionId = reader.GetInt32(divisionIdIndex);
                    pay.DivisionName = reader.GetString(divisionNameIndex);
                }
                pay.SLTApproval = reader.GetBoolean(sLTApprovalColumnIndex);
                pay.SLTPTOApproval = reader.GetBoolean(sLTPTOApprovalColumnIndex);
                if (vendorIdIndex > 0 && vendorNameIndex > 0 && !reader.IsDBNull(vendorIdIndex))
                {
                    pay.vendor = new Vendor { Id = reader.GetInt32(vendorIdIndex), Name = reader.GetString(vendorNameIndex) };
                }
                if (isRtpIndex > -1)
                {
                    pay.IsRighttoPresent = reader.GetBoolean(isRtpIndex);
                }
                result.Add(pay);
            }
        }

        public static List<Triple<DateTime, bool, bool>> IsPersonSalaryTypeListByPeriod(int personId, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.Pay.IsPersonSalaryTypeListByPeriodProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonIdParam, personId);
                command.Parameters.AddWithValue(Constants.ParameterNames.StartDateParam, startDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.EndDateParam, endDate);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    List<Triple<DateTime, bool, bool>> result = new List<Triple<DateTime, bool, bool>>();
                    if (reader.HasRows)
                    {
                        int dateIndex = reader.GetOrdinal(Constants.ColumnNames.DateColumn);
                        int isSalaryTypeIndex = reader.GetOrdinal(Constants.ColumnNames.IsSalaryType);
                        int isHourlyTypeIndex = reader.GetOrdinal(Constants.ColumnNames.IsHourlyType);

                        while (reader.Read())
                        {
                            Triple<DateTime, bool, bool> resultLocal = new Triple<DateTime, bool, bool>(DateTime.Now, true, true)
                            {
                                First = reader.GetDateTime(dateIndex),
                                Second = reader.GetBoolean(isSalaryTypeIndex),
                                Third = reader.GetBoolean(isHourlyTypeIndex)
                            };
                            result.Add(resultLocal);
                        }
                    }
                    return result;
                }
            }
        }

        #endregion Methods
    }
}

