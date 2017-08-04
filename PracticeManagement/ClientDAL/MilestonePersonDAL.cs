﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using DataAccess.Other;
using DataTransferObjects;
using DataTransferObjects.ContextObjects;

namespace DataAccess
{
    public static class MilestonePersonDAL
    {
        #region Constants

        #region Parameters

        private const string ProjectIdParam = "@ProjectId";
        private const string IncludeStrawmanParam = "@IncludeStrawman";
        private const string MilestoneIdParam = "@MilestoneId";
        private const string ProjectedDeliveryDateParam = "@ProjectedDeliveryDate";
        private const string PersonIdParam = "@PersonId";
        private const string StartDateParam = "@StartDate";
        private const string EndDateParam = "@EndDate";
        private const string CheckStartDateEqualityParam = "@CheckStartDateEquality";
        private const string CheckEndDateEqualityParam = "@CheckEndDateEquality";
        private const string HoursPerDayParam = "@HoursPerDay";
        private const string PersonRoleIdParam = "@PersonRoleId";
        private const string AmountParam = "@Amount";
        private const string MilestonePersonIdParam = "@MilestonePersonId";
        private const string LocationParam = "@Location";
        private const string UserLoginParam = "@UserLogin";

        #endregion Parameters

        #region Columns

        private const string MilestonePersonIdColumn = "MilestonePersonId";
        private const string MilestoneIdColumn = "MilestoneId";
        private const string PersonIdColumn = "PersonId";
        private const string StartDateColumn = "StartDate";
        private const string EndDateColumn = "EndDate";
        private const string FirstNameColumn = "FirstName";
        private const string LastNameColumn = "LastName";
        private const string ProjectIdColumn = "ProjectId";
        private const string ProjectStatusIdColumn = "ProjectStatusId";
        private const string ProjectNumberColumn = "ProjectNumber";
        private const string ProjectNameColumn = "ProjectName";
        private const string ProjectStartDateColumn = "ProjectStartDate";
        private const string ProjectEndDateColumn = "ProjectEndDate";
        private const string ClientIdColumn = "ClientId";
        private const string ClientNameColumn = "ClientName";
        private const string MilestoneNameColumn = "MilestoneName";
        private const string MilestoneStartDateColumn = "MilestoneStartDate";
        private const string MilestoneProjectedDeliveryDateColumn = "MilestoneProjectedDeliveryDate";
        private const string HoursPerDayColumn = "HoursPerDay";
        private const string ExpectedHoursWithVacationDaysColumn = "ExpectedHoursWithVacationDays";
        private const string PersonRoleIdColumn = "PersonRoleId";
        private const string PersonRoleNameColumn = "RoleName";
        private const string AmountColumn = "Amount";
        private const string IsHourlyAmountColumn = "IsHourlyAmount";
        private const string DiscountColumn = "Discount";
        private const string MilestoneExpectedHoursColumn = "MilestoneExpectedHours";
        private const string MilestoneHourlyRevenueColumn = "MilestoneHourlyRevenue";
        private const string PersonVacationsOnMilestoneColumn = "VacationDays";
        private const string PersonSeniorityIdColumn = "SeniorityId";
        private const string LocationColumn = "Location";
        private const string HasTimeEntriesColumn = "HasTimeEntries";

        #endregion Columns

        #endregion Constants

        #region Methods

        /// <summary>
        /// 	Gets milestones info for given person
        /// </summary>
        public static List<MilestonePersonEntry> GetConsultantMilestones(ConsultantMilestonesContext context)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (
                var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.ConsultantMilestones, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonId, context.PersonId);
                command.Parameters.AddWithValue(Constants.ParameterNames.StartDate, context.StartDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.EndDate, context.EndDate);

                command.Parameters.AddWithValue(Constants.ParameterNames.IncludeActive, context.IncludeActiveProjects);
                command.Parameters.AddWithValue(Constants.ParameterNames.IncludeProjected, context.IncludeProjectedProjects);
                command.Parameters.AddWithValue(Constants.ParameterNames.IncludeInactive, context.IncludeInactiveProjects);
                command.Parameters.AddWithValue(Constants.ParameterNames.IncludeCompleted, context.IncludeCompletedProjects);
                command.Parameters.AddWithValue(Constants.ParameterNames.IncludeInternal, context.IncludeInternalProjects);
                command.Parameters.AddWithValue(Constants.ParameterNames.IncludeExperimental, context.IncludeExperimentalProjects);
                command.Parameters.AddWithValue(Constants.ParameterNames.IncludeDefaultMileStone, context.IncludeDefaultMileStone);
                command.Parameters.AddWithValue(Constants.ParameterNames.IncludeAtRisk, context.IncludeAtRiskProjects);

                connection.Open();
                using (var reader = command.ExecuteReader())
                    return ReadDetailedMilestonePersonEntries(reader);
            }
        }

        /// <summary>
        /// 	Retrives the list of the <see cref = "Person" />s for the specified <see cref = "Project" />.
        /// </summary>
        /// <param name = "projects">Projects list</param>
        /// <returns>The list of the <see cref = "MilestonePerson" /> objects.</returns>
        public static void LoadMilestonePersonListForProject(List<Project> projects)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.MilestonePerson.MilestonePersonListByProjectShort, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(ProjectIdParam,
                                                DataTransferObjects.Utils.Generic.IdsListToString(projects));

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    projects.ForEach(delegate (Project project) { project.ProjectPersons = new List<MilestonePerson>(); });
                    ReadMilestonePersonsShort(reader, projects);
                }
            }
        }

        /// <summary>
        /// 	Retrives the list of the <see cref = "Person" />s for the specified <see cref = "Project" />.
        /// </summary>
        /// <param name = "projectId">An ID of the project to the data be retrieved for.</param>
        /// <returns>The list of the <see cref = "MilestonePerson" /> objects.</returns>
        public static List<MilestonePerson> MilestonePersonListByProject(int projectId, bool includeStrawman = true)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (
                var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonListByProject,
                                             connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(ProjectIdParam, projectId);
                command.Parameters.AddWithValue(IncludeStrawmanParam, includeStrawman);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<MilestonePerson>();

                    ReadMilestonePersons(reader, result);
                    ReadMilestonePersonEntriesTimeOffs(reader, result);
                    return result;
                }
            }
        }

        /// <summary>
        /// 	Retrives the list of the <see cref = "Person" />s for the specified <see cref = "Milestone" />.
        /// </summary>
        /// <param name = "milestoneId">An ID of the milestone to the data be retrieved for.</param>
        /// <returns>The list of the <see cref = "MilestonePerson" /> objects.</returns>
        public static List<MilestonePerson> MilestonePersonListByMilestone(int milestoneId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (
                var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonListByMilestone,
                                             connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestoneId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<MilestonePerson>();

                    ReadMilestonePersons(reader, result);
                    ReadMilestonePersonEntriesTimeOffs(reader, result);

                    return result;
                }
            }
        }

        public static List<MilestonePerson> MilestonePersonsByMilestoneForTEByProject(int milestoneId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (
                var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonsByMilestoneForTEByProject,
                                             connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestoneId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<MilestonePerson>();

                    ReadMilestonePersonsForTEbyProjectReport(reader, result);

                    return result;
                }
            }
        }

        /// <summary>
        /// 	Retrives the milestone-person link details.
        /// </summary>
        /// <param name = "milestonePersonId">An ID of the milestone-person association.</param>
        /// <returns>The <see cref = "MilestonePerson" /> object if found and null otherwise.</returns>
        public static MilestonePerson MilestonePersonGetByMilestonePersonId(int milestonePersonId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (
                var command =
                    new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonGetByMilestonePersonId,
                                   connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestonePersonIdParam, milestonePersonId);

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    var result = new List<MilestonePerson>(1);

                    ReadMilestonePersonAssociations(reader, result);

                    return result.Count > 0 ? result[0] : null;
                }
            }
        }

        /// <summary>
        /// 	Retrieves the milestone-person entries for the specified milestone-person association.
        /// </summary>
        /// <param name = "milestonePersonId">An ID of the milestone-person association.</param>
        /// <returns>A list of the <see cref = "MilestonePersonEntry" /> objects.</returns>
        public static List<MilestonePersonEntry> MilestonePersonEntryListByMilestonePersonId(int milestonePersonId
                                                                                               , SqlConnection connection = null
                                                                                               , SqlTransaction activeTransaction = null
                                                                                             )
        {
            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }

            using (var command =
                new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonEntryListByMilestonePersonId,
                               connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestonePersonIdParam, milestonePersonId);

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                if (activeTransaction != null)
                {
                    command.Transaction = activeTransaction;
                }

                using (var reader = command.ExecuteReader())
                {
                    var result = new List<MilestonePersonEntry>();

                    ReadMilestonePersonEntries(reader, result);
                    ReadMilestonePersonEntriesTimeOffs(reader, result);
                    return result;
                }
            }
        }

        private static void ReadMilestonePersonEntriesTimeOffs(SqlDataReader reader, List<MilestonePersonEntry> result)
        {
            if (!reader.NextResult() || !reader.HasRows) return;
            var personIdIndex = reader.GetOrdinal(PersonIdColumn);
            var dateIndex = reader.GetOrdinal(Constants.ColumnNames.Date);
            var actualHoursIndex = reader.GetOrdinal(Constants.ColumnNames.ActualHours);

            while (reader.Read())
            {
                var personId = reader.GetInt32(personIdIndex);
                if (result.All(mp => mp.ThisPerson.Id != personId)) continue;
                MilestonePersonEntry mpe = result.First(mp => mp.ThisPerson.Id == personId);
                if (mpe.PersonTimeOffList == null)
                {
                    mpe.PersonTimeOffList = new Dictionary<DateTime, Decimal>();
                }
                mpe.PersonTimeOffList.Add(reader.GetDateTime(dateIndex), reader.GetDecimal(actualHoursIndex));
            }
        }

        private static void ReadMilestonePersonEntriesTimeOffs(SqlDataReader reader, List<MilestonePerson> result)
        {
            if (!reader.NextResult() || !reader.HasRows) return;
            var personIdIndex = reader.GetOrdinal(PersonIdColumn);
            var dateIndex = reader.GetOrdinal(Constants.ColumnNames.Date);
            var actualHoursIndex = reader.GetOrdinal(Constants.ColumnNames.ActualHours);

            while (reader.Read())
            {
                var personId = reader.GetInt32(personIdIndex);
                var date = reader.GetDateTime(dateIndex);
                var hours = reader.GetDecimal(actualHoursIndex);

                if (result.All(mp => mp.Person.Id != personId)) continue;
                List<MilestonePerson> milestonePersons = result.Where(mp => mp.Person.Id == personId).ToList();
                foreach (MilestonePerson mp in milestonePersons)
                {
                    if (mp.Entries == null) continue;
                    foreach (var mpe in mp.Entries)
                    {
                        if (mpe.PersonTimeOffList == null)
                        {
                            mpe.PersonTimeOffList = new Dictionary<DateTime, Decimal>();
                        }
                        mpe.PersonTimeOffList.Add(date, hours);
                    }
                }
            }
        }

        public static bool CheckTimeEntriesForMilestonePerson(int milestonePersonId, DateTime? startDate, DateTime? endDate
                , bool checkStartDateEquality, bool checkEndDateEquality)
        {
            bool result;
            try
            {
                using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
                using (var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.CheckTimeEntriesForMilestonePerson
                                                    , connection
                                                   )
                      )
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(MilestonePersonIdParam, milestonePersonId);

                    if (startDate.HasValue)
                    {
                        command.Parameters.AddWithValue(StartDateParam, startDate.Value);
                        command.Parameters.AddWithValue(CheckStartDateEqualityParam, checkStartDateEquality);
                    }
                    if (endDate.HasValue)
                    {
                        command.Parameters.AddWithValue(EndDateParam, endDate.Value);
                        command.Parameters.AddWithValue(CheckEndDateEqualityParam, checkEndDateEquality);
                    }

                    connection.Open();
                    result = Convert.ToBoolean(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                result = true;
            }

            return result;
        }

        public static bool CheckTimeEntriesForMilestonePersonWithGivenRoleId(int milestonePersonId, int? milestonePersonRoleId)
        {
            bool result;
            try
            {
                using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
                using (var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.CheckTimeEntriesForMilestonePersonWithGivenRoleId, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(MilestonePersonIdParam, milestonePersonId);
                    if (milestonePersonRoleId.HasValue)
                    {
                        command.Parameters.AddWithValue(PersonRoleIdParam, milestonePersonRoleId);
                    }
                    connection.Open();
                    result = Convert.ToBoolean(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 	Inserts the specified <see cref = "Milestone" />-<see cref = "Person" /> link to the database.
        /// </summary>
        /// <param name = "milestonePerson">The data to be saved to.</param>
        public static void MilestonePersonInsert(MilestonePerson milestonePerson, SqlConnection connection = null, SqlTransaction activeTransaction = null)
        {
            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }

            using (var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonInsert, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam,
                                                milestonePerson.Milestone != null &&
                                                milestonePerson.Milestone.Id.HasValue
                                                    ? (object)milestonePerson.Milestone.Id.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(PersonIdParam,
                                                milestonePerson.Person != null && milestonePerson.Person.Id.HasValue
                                                    ? (object)milestonePerson.Person.Id.Value
                                                    : DBNull.Value);

                var milestonePersonId = new SqlParameter(MilestonePersonIdParam, SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(milestonePersonId);

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

                    command.ExecuteNonQuery();
                    milestonePerson.Id = (int)milestonePersonId.Value;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static bool IsPersonAlreadyAddedtoMilestone(int mileStoneId, int personId, SqlConnection connection = null, SqlTransaction activeTransaction = null)
        {
            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }

            using (var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.IsPersonAlreadyAddedtoMilestone, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, mileStoneId);
                command.Parameters.AddWithValue(PersonIdParam, personId);

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

                    return (int)command.ExecuteScalar() > 0;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 	Inserts person-milestone details for the specified milestone and person.
        /// </summary>
        /// <param name = "entry">The data to be inserted.</param>
        /// <param name = "userName">A current user.</param>
        public static int MilestonePersonEntryInsert(MilestonePersonEntry entry, string userName, SqlConnection connection = null, SqlTransaction activeTransaction = null)
        {
            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }

            using (var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonEntryInsert, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(PersonIdParam,
                                                entry.ThisPerson.Id.HasValue
                                                    ? (object)entry.ThisPerson.Id.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(MilestonePersonIdParam, entry.MilestonePersonId);
                command.Parameters.AddWithValue(LocationParam, entry.Location);
                command.Parameters.AddWithValue(StartDateParam, entry.StartDate);
                command.Parameters.AddWithValue(EndDateParam,
                                                entry.EndDate.HasValue ? (object)entry.EndDate.Value : DBNull.Value);
                command.Parameters.AddWithValue(HoursPerDayParam, entry.HoursPerDay);
                command.Parameters.AddWithValue(PersonRoleIdParam,
                                                entry.Role != null ? (object)entry.Role.Id : DBNull.Value);
                command.Parameters.AddWithValue(AmountParam,
                                                entry.HourlyAmount.HasValue
                                                    ? (object)entry.HourlyAmount.Value.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsBadgeRequired, entry.MSBadgeRequired);
                command.Parameters.AddWithValue(Constants.ParameterNames.BadgeStartDate, entry.BadgeStartDate.HasValue ? (object)entry.BadgeStartDate.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.BadgeEndDate, entry.BadgeEndDate.HasValue ? (object)entry.BadgeEndDate.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsBadgeException, entry.BadgeException);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsApproved, entry.IsApproved);
                command.Parameters.AddWithValue(UserLoginParam,
                                                !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsNewtoBudget, entry.IsNewToBudget);
                command.Parameters.AddWithValue(Constants.ParameterNames.DiscountParam, entry.Discount);
                command.Parameters.AddWithValue(Constants.ParameterNames.LockDiscount, entry.DiscountLocked);

                command.Parameters.AddWithValue(Constants.ParameterNames.IsDiscountAtResourceLevel, entry.IsDiscountAtResourceLevel);

                var milestonePersonEntryId = new SqlParameter(Constants.ParameterNames.IdParam, SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(milestonePersonEntryId);

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                if (activeTransaction != null)
                {
                    command.Transaction = activeTransaction;
                }

                command.ExecuteNonQuery();

                return (int)milestonePersonEntryId.Value;
            }
        }

        /// <summary>
        /// 	Removes persons-milestones details for the specified milestone and person.
        /// </summary>
        /// <param name = "milestonePersonId">An ID of the milestone-person association.</param>
        public static void MilestonePersonDeleteEntries(int milestonePersonId, SqlConnection connection = null, SqlTransaction activeTransaction = null)
        {
            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }

            using (var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonDeleteEntries, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestonePersonIdParam, milestonePersonId);

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (activeTransaction != null)
                {
                    command.Transaction = activeTransaction;
                }

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 	Deletes the specified <see cref = "Milestone" />-<see cref = "Person" /> link from the database.
        /// </summary>
        /// <param name = "milestonePerson">The data to be deleted from.</param>
        public static void MilestonePersonDelete(MilestonePerson milestonePerson)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (
                var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonDelete, connection)
                )
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestonePersonIdParam,
                                                milestonePerson.Id.HasValue
                                                    ? (object)milestonePerson.Id.Value
                                                    : DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void SaveMilestonePersonWrapper(MilestonePerson milestonePerson, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                connection.Open();
                var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                SaveMilestonePerson(milestonePerson, userName, connection, transaction);

                transaction.Commit();
            }
        }

        /// <summary>
        /// 	Saves the specified <see cref = "Milestone" />-<see cref = "Person" /> link to the database.
        /// </summary>
        /// <param name = "milestonePerson">The data to be saved to.</param>
        /// <param name = "userName">A current user.</param>
        /// <remarks>
        /// 	Must be run within a transaction.
        /// </remarks>
        public static void SaveMilestonePerson(MilestonePerson milestonePerson, string userName, SqlConnection connection = null, SqlTransaction activeTransaction = null)
        {
            if (!milestonePerson.Id.HasValue)
            {
                MilestonePersonInsert(milestonePerson, connection, activeTransaction);
            }

            MilestonePersonDeleteEntries(milestonePerson.Id.Value, connection, activeTransaction);

            foreach (var entry in milestonePerson.Entries)
            {
                entry.ThisPerson = milestonePerson.Person;
                entry.MilestonePersonId = milestonePerson.Id.Value;
                MilestonePersonEntryInsert(entry, userName, connection, activeTransaction);
            }
        }

        private static void ReadMilestonePersons(DbDataReader reader, List<MilestonePerson> result)
        {
            if (!reader.HasRows) return;
            var milestonePersonIdIndex = reader.GetOrdinal(MilestonePersonIdColumn);
            var milestoneIdIndex = reader.GetOrdinal(MilestoneIdColumn);
            var personIdIndex = reader.GetOrdinal(PersonIdColumn);
            var startDateIndex = reader.GetOrdinal(StartDateColumn);
            var endDateIndex = reader.GetOrdinal(EndDateColumn);
            var firstNameIndex = reader.GetOrdinal(FirstNameColumn);
            var lastNameIndex = reader.GetOrdinal(LastNameColumn);
            var projectIdIndex = reader.GetOrdinal(ProjectIdColumn);
            var projectNameIndex = reader.GetOrdinal(ProjectNameColumn);
            var projectStartDateIndex = reader.GetOrdinal(ProjectStartDateColumn);
            var projectEndDateIndex = reader.GetOrdinal(ProjectEndDateColumn);
            var clientIdIndex = reader.GetOrdinal(ClientIdColumn);
            var clientNameIndex = reader.GetOrdinal(ClientNameColumn);
            var milestoneNameIndex = reader.GetOrdinal(MilestoneNameColumn);
            var milestoneStartDateIndex = reader.GetOrdinal(MilestoneStartDateColumn);
            var milestoneProjectedDeliveryDateIndex = reader.GetOrdinal(MilestoneProjectedDeliveryDateColumn);
            var hoursPerDayIndex = reader.GetOrdinal(HoursPerDayColumn);
            var expectedHoursWithVacationDaysIndex = reader.GetOrdinal(ExpectedHoursWithVacationDaysColumn);

            var personRoleIdIndex = reader.GetOrdinal(PersonRoleIdColumn);
            var personRoleNameIndex = reader.GetOrdinal(PersonRoleNameColumn);
            var amountIndex = reader.GetOrdinal(AmountColumn);
            var isHourlyAmountIndex = reader.GetOrdinal(IsHourlyAmountColumn);
            var discountIndex = reader.GetOrdinal(DiscountColumn);
            var milestoneExpectedHoursIndex = reader.GetOrdinal(MilestoneExpectedHoursColumn);
            var milestoneHourlyRevenueIndex = reader.GetOrdinal(MilestoneHourlyRevenueColumn);
            var personSeniorityIdIndex = reader.GetOrdinal(PersonSeniorityIdColumn);

            int IsBadgeRequiredIndex;
            int BadgeStartDateIndex;
            int BadgeEndDateIndex;
            int IsBadgeExceptionIndex;
            int IsApprovedIndex;
            int ConsultantEndDateIndex;
            int personDiscountIndex;
            int isNewtoBudgetIndex;
            int discountLockedIndex;

            try {
                discountLockedIndex = reader.GetOrdinal(Constants.ColumnNames.LockDiscount);
            }
            catch {
                discountLockedIndex = -1;
            }
            try
            {
                personDiscountIndex = reader.GetOrdinal(Constants.ColumnNames.MilestonePersonDiscount);
            }
            catch
            {
                personDiscountIndex = -1;
            }

            try
            {
                isNewtoBudgetIndex = reader.GetOrdinal(Constants.ColumnNames.IsNewToBudget);
            }
            catch
            {
                isNewtoBudgetIndex = -1;
            }

            try
            {
                IsBadgeRequiredIndex = reader.GetOrdinal(Constants.ColumnNames.IsBadgeRequired);
            }
            catch
            {
                IsBadgeRequiredIndex = -1;
            }

            try
            {
                BadgeStartDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeStartDate);
            }
            catch
            {
                BadgeStartDateIndex = -1;
            }

            try
            {
                BadgeEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeEndDate);
            }
            catch
            {
                BadgeEndDateIndex = -1;
            }
            try
            {
                IsBadgeExceptionIndex = reader.GetOrdinal(Constants.ColumnNames.IsBadgeException);
            }
            catch
            {
                IsBadgeExceptionIndex = -1;
            }
            try
            {
                IsApprovedIndex = reader.GetOrdinal(Constants.ColumnNames.IsApproved);
            }
            catch
            {
                IsApprovedIndex = -1;
            }
            try
            {
                ConsultantEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.ConsultantEndDate);
            }
            catch
            {
                ConsultantEndDateIndex = -1;
            }
            int firstHireDateIndex;
            try
            {
                firstHireDateIndex = reader.GetOrdinal(Constants.ColumnNames.FirstHireDate);
            }
            catch
            {
                firstHireDateIndex = -1;
            }

            int lastTerminationDateIndex;
            try
            {
                lastTerminationDateIndex = reader.GetOrdinal(Constants.ColumnNames.LastTerminationDate);
            }
            catch
            {
                lastTerminationDateIndex = -1;
            }

            int milestonePersonEntryIdIndex;
            try
            {
                milestonePersonEntryIdIndex = reader.GetOrdinal(Constants.ColumnNames.EntryId);
            }
            catch
            {
                milestonePersonEntryIdIndex = -1;
            }

            int projectStatusIdIndex;
            try
            {
                projectStatusIdIndex = reader.GetOrdinal(ProjectStatusIdColumn);
            }
            catch
            {
                projectStatusIdIndex = -1;
            }

            int hireDateIndex, terminationDateIndex;
            try
            {
                hireDateIndex = reader.GetOrdinal(Constants.ColumnNames.HireDateColumn);
                terminationDateIndex = reader.GetOrdinal(Constants.ColumnNames.TerminationDateColumn);
            }
            catch
            {
                hireDateIndex = -1;
                terminationDateIndex = -1;
            }

            while (reader.Read())
            {
                var milestonePerson = new MilestonePerson { Id = reader.GetInt32(milestonePersonIdIndex) };

                // Person on milestone
                var entry = new MilestonePersonEntry
                {
                    StartDate = reader.GetDateTime(startDateIndex),
                    EndDate =
                            !reader.IsDBNull(endDateIndex)
                                ? (DateTime?)reader.GetDateTime(endDateIndex)
                                : null,
                    HoursPerDay = reader.GetDecimal(hoursPerDayIndex),
                    ProjectedWorkloadWithVacation = reader.GetDecimal(expectedHoursWithVacationDaysIndex),
                    HourlyAmount = !reader.IsDBNull(milestoneHourlyRevenueIndex)
                                           ? reader.GetDecimal(milestoneHourlyRevenueIndex)
                                           : 0,
                    MilestonePersonId = reader.GetInt32(milestonePersonIdIndex),
                    ThisPerson = new Person()
                    {
                        Id = reader.GetInt32(personIdIndex),
                        FirstName = reader.GetString(firstNameIndex),
                        LastName = reader.GetString(lastNameIndex)
                    },
                };

                if (IsBadgeRequiredIndex > -1)
                {
                    entry.MSBadgeRequired = !reader.IsDBNull(IsBadgeRequiredIndex) ? reader.GetBoolean(IsBadgeRequiredIndex) : false;
                }
                if (BadgeStartDateIndex > -1)
                {
                    entry.BadgeStartDate = !reader.IsDBNull(BadgeStartDateIndex) ? (DateTime?)reader.GetDateTime(BadgeStartDateIndex) : null;
                }
                if (BadgeEndDateIndex > -1)
                {
                    entry.BadgeEndDate = !reader.IsDBNull(BadgeEndDateIndex) ? (DateTime?)reader.GetDateTime(BadgeEndDateIndex) : null;
                }
                if (IsBadgeExceptionIndex > -1)
                {
                    entry.BadgeException = !reader.IsDBNull(IsBadgeExceptionIndex) ? reader.GetBoolean(IsBadgeExceptionIndex) : false;
                }
                if (IsApprovedIndex > -1)
                {
                    entry.MSBadgeRequired = !reader.IsDBNull(IsBadgeRequiredIndex) ? reader.GetBoolean(IsBadgeRequiredIndex) : false;
                }
                if (ConsultantEndDateIndex > -1)
                {
                    entry.ConsultantEndDate = !reader.IsDBNull(ConsultantEndDateIndex) ? (DateTime?)reader.GetDateTime(ConsultantEndDateIndex) : null;
                }
                if (milestonePersonEntryIdIndex > -1)
                {
                    entry.Id = reader.GetInt32(milestonePersonEntryIdIndex);
                }

                if (personDiscountIndex > -1)
                {
                    entry.Discount = !reader.IsDBNull(personDiscountIndex) ? (decimal?)reader.GetDecimal(personDiscountIndex) : null;
                }

                if (isNewtoBudgetIndex > -1)
                {
                    entry.IsNewToBudget = reader.GetBoolean(isNewtoBudgetIndex);
                }

                if (discountLockedIndex > -1)
                {
                    entry.DiscountLocked = reader.GetBoolean(discountLockedIndex);
                }

                if (!reader.IsDBNull(personRoleIdIndex))
                {
                    entry.Role = new PersonRole
                    {
                        Id = reader.GetInt32(personRoleIdIndex),
                        Name = reader.GetString(personRoleNameIndex)
                    };
                    entry.StartDate = !reader.IsDBNull(startDateIndex) ? reader.GetDateTime(startDateIndex) : DateTime.MinValue;
                    entry.EndDate = !reader.IsDBNull(endDateIndex) ? reader.GetDateTime(endDateIndex) : DateTime.MaxValue;
                }

                milestonePerson.Entries = new List<MilestonePersonEntry>(1) { entry };

                // Person details
                milestonePerson.Person = new Person
                {
                    Id = reader.GetInt32(personIdIndex),
                    FirstName = reader.GetString(firstNameIndex),
                    LastName = reader.GetString(lastNameIndex)
                };

                if (hireDateIndex >= 0 && terminationDateIndex >= 0)
                {
                    milestonePerson.Person.HireDate = reader.GetDateTime(hireDateIndex);
                    milestonePerson.Person.TerminationDate = !reader.IsDBNull(terminationDateIndex)
                                                                 ? (DateTime?)reader.GetDateTime(terminationDateIndex)
                                                                 : null;
                }

                if (firstHireDateIndex > -1)
                {
                    milestonePerson.Person.FirstHireDate = reader.GetDateTime(firstHireDateIndex);
                }

                if (lastTerminationDateIndex > -1)
                {
                    milestonePerson.Person.LastTerminationDate = reader.GetDateTime(lastTerminationDateIndex);
                }

                // Seniority
                if (!reader.IsDBNull(personSeniorityIdIndex))
                {
                    milestonePerson.Person.Seniority = new Seniority { Id = reader.GetInt32(personSeniorityIdIndex) };
                }

                // Milestone details
                var project = new Project
                {
                    Id = reader.GetInt32(projectIdIndex),
                    Name = reader.GetString(projectNameIndex),
                    StartDate = reader.GetDateTime(projectStartDateIndex),
                    EndDate = reader.GetDateTime(projectEndDateIndex),
                    Discount = reader.GetDecimal(discountIndex)
                };
                milestonePerson.Milestone = new Milestone
                {
                    Id = reader.GetInt32(milestoneIdIndex),
                    Description = reader.GetString(milestoneNameIndex),
                    Amount =
                            !reader.IsDBNull(amountIndex)
                                ? (decimal?)reader.GetDecimal(amountIndex)
                                : null,
                    StartDate = reader.GetDateTime(milestoneStartDateIndex),
                    ProjectedDeliveryDate =
                            reader.GetDateTime(milestoneProjectedDeliveryDateIndex),
                    IsHourlyAmount = reader.GetBoolean(isHourlyAmountIndex),
                    ExpectedHours = reader.GetDecimal(milestoneExpectedHoursIndex),
                    Project = project
                };

                // Project details

                if (projectStatusIdIndex >= 0)
                    milestonePerson.Milestone.Project.Status = new ProjectStatus { Id = reader.GetInt32(projectStatusIdIndex) };

                // Client details
                milestonePerson.Milestone.Project.Client = new Client
                {
                    Id = reader.GetInt32(clientIdIndex),
                    Name = reader.GetString(clientNameIndex)
                };
                result.Add(milestonePerson);
            }
        }

        private static void ReadMilestonePersonsForTEbyProjectReport(DbDataReader reader, List<MilestonePerson> result)
        {
            if (!reader.HasRows) return;
            var milestonePersonIdIndex = reader.GetOrdinal(MilestonePersonIdColumn);
            var personIdIndex = reader.GetOrdinal(PersonIdColumn);
            var firstNameIndex = reader.GetOrdinal(FirstNameColumn);
            var lastNameIndex = reader.GetOrdinal(LastNameColumn);

            while (reader.Read())
            {
                var milestonePerson = new MilestonePerson
                {
                    Id = reader.GetInt32(milestonePersonIdIndex),
                    Person = new Person
                    {
                        Id = reader.GetInt32(personIdIndex),
                        FirstName = reader.GetString(firstNameIndex),
                        LastName = reader.GetString(lastNameIndex)
                    }
                };

                // Person details

                result.Add(milestonePerson);
            }
        }

        private static void ReadMilestonePersonsShort(DbDataReader reader, List<Project> projects)
        {
            if (!reader.HasRows) return;
            var milestonePersonIdIndex = reader.GetOrdinal(MilestonePersonIdColumn);
            var personIdIndex = reader.GetOrdinal(PersonIdColumn);
            var firstNameIndex = reader.GetOrdinal(FirstNameColumn);
            var lastNameIndex = reader.GetOrdinal(LastNameColumn);
            var projectIdIndex = reader.GetOrdinal(ProjectIdColumn);
            var seniorityIdIndex = reader.GetOrdinal(PersonSeniorityIdColumn);
            int preferredFirstNameIndex = reader.GetOrdinal(Constants.ColumnNames.PreferredFirstName);
            while (reader.Read())
            {
                var project = new Project { Id = reader.GetInt32(projectIdIndex) };

                var milestonePerson =
                    new MilestonePerson
                    {
                        Id = reader.GetInt32(milestonePersonIdIndex),
                        Person = new Person
                        {
                            Id = reader.GetInt32(personIdIndex),
                            FirstName = reader.GetString(firstNameIndex),
                            LastName = reader.GetString(lastNameIndex),
                            PrefferedFirstName = reader.IsDBNull(preferredFirstNameIndex) ? string.Empty : reader.GetString(preferredFirstNameIndex),
                            Seniority = !reader.IsDBNull(seniorityIdIndex) ?
                                                    new Seniority
                                                    {
                                                        Id = reader.GetInt32(seniorityIdIndex)
                                                    } : null
                        }
                    };

                var i = projects.IndexOf(project);
                projects[i].ProjectPersons.Add(milestonePerson);
            }
        }

        private static List<MilestonePersonEntry> ReadDetailedMilestonePersonEntries(DbDataReader reader)
        {
            var result = new List<MilestonePersonEntry>();

            if (reader.HasRows)
            {
                var hoursPerDayIndex = reader.GetOrdinal(HoursPerDayColumn);
                var milestoneIdIndex = reader.GetOrdinal(MilestoneIdColumn);
                var milestoneNameIndex = reader.GetOrdinal(MilestoneNameColumn);
                var milestonePersonIdIndex = reader.GetOrdinal(MilestonePersonIdColumn);
                var endDateIndex = reader.GetOrdinal(EndDateColumn);
                var startDateIndex = reader.GetOrdinal(StartDateColumn);
                var projectIdIndex = reader.GetOrdinal(ProjectIdColumn);
                var projectNameIndex = reader.GetOrdinal(ProjectNameColumn);
                var clientIdIndex = reader.GetOrdinal(ClientIdColumn);
                var clientNameIndex = reader.GetOrdinal(ClientNameColumn);
                var projectStatusIdIndex = reader.GetOrdinal(ProjectStatusIdColumn);
                var projectNumberIndex = reader.GetOrdinal(ProjectNumberColumn);
                int pmIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectManagersIdFirstNameLastName);

                while (reader.Read())
                {
                    // Client details
                    var client = new Client
                    {
                        Id = reader.GetInt32(clientIdIndex),
                        Name = reader.GetString(clientNameIndex)
                    };

                    // Project details
                    var project = new Project
                    {
                        Id = reader.GetInt32(projectIdIndex),
                        Name = reader.GetString(projectNameIndex),
                        ProjectNumber = reader.GetString(projectNumberIndex),
                        Status = new ProjectStatus
                        {
                            Id = reader.GetInt32(projectStatusIdIndex)
                        },
                        Client = client,
                        ProjectManagers = Utils.stringToProjectManagersList(reader.GetString(pmIndex))
                    };

                    // Milestone details
                    var milestone = new Milestone
                    {
                        Id = reader.GetInt32(milestoneIdIndex),
                        Description = reader.GetString(milestoneNameIndex),
                        Project = project
                    };

                    // Person on milestone
                    var entry = new MilestonePersonEntry
                    {
                        MilestonePersonId = reader.GetInt32(milestonePersonIdIndex),
                        StartDate = reader.GetDateTime(startDateIndex),
                        EndDate = !reader.IsDBNull(endDateIndex)
                                                      ? (DateTime?)reader.GetDateTime(endDateIndex)
                                                      : null,
                        HoursPerDay = reader.GetDecimal(hoursPerDayIndex),
                        ParentMilestone = milestone
                    };

                    result.Add(entry);
                }
            }

            return result;
        }

        private static void ReadMilestonePersonAssociations(DbDataReader reader, List<MilestonePerson> result)
        {
            if (!reader.HasRows) return;
            var milestonePersonIdIndex = reader.GetOrdinal(MilestonePersonIdColumn);
            var milestoneIdIndex = reader.GetOrdinal(MilestoneIdColumn);
            var personIdIndex = reader.GetOrdinal(PersonIdColumn);
            var isHourlyAmountIndex = reader.GetOrdinal(IsHourlyAmountColumn);
            var milestoneNameIndex = reader.GetOrdinal(MilestoneNameColumn);
            var milestoneStartDateIndex = reader.GetOrdinal(MilestoneStartDateColumn);
            var milestoneProjectedDeliveryDateIndex = reader.GetOrdinal(MilestoneProjectedDeliveryDateColumn);
            var projectIdIndex = reader.GetOrdinal(ProjectIdColumn);
            var projectNameIndex = reader.GetOrdinal(ProjectNameColumn);
            var projectStartDateIndex = reader.GetOrdinal(ProjectStartDateColumn);
            var projectEndDateIndex = reader.GetOrdinal(ProjectEndDateColumn);
            var clientIdIndex = reader.GetOrdinal(ClientIdColumn);
            var clientNameIndex = reader.GetOrdinal(ClientNameColumn);
            var personSeniorityIdIndex = reader.GetOrdinal(PersonSeniorityIdColumn);

            while (reader.Read())
            {
                var project = new Project
                {
                    Id = reader.GetInt32(projectIdIndex),
                    Name = reader.GetString(projectNameIndex),
                    Client = new Client
                    {
                        Id = reader.GetInt32(clientIdIndex),
                        Name = reader.GetString(clientNameIndex)
                    },
                    StartDate = !reader.IsDBNull(projectStartDateIndex)
                                        ? (DateTime?)
                                          reader.GetDateTime(projectStartDateIndex)
                                        : null,
                    EndDate = !reader.IsDBNull(projectEndDateIndex)
                                      ? (DateTime?)
                                        reader.GetDateTime(projectEndDateIndex)
                                      : null
                };

                var milestone = new Milestone
                {
                    Id = reader.GetInt32(milestoneIdIndex),
                    IsHourlyAmount = reader.GetBoolean(isHourlyAmountIndex),
                    Description = reader.GetString(milestoneNameIndex),
                    StartDate = reader.GetDateTime(milestoneStartDateIndex),
                    ProjectedDeliveryDate =
                            reader.GetDateTime(milestoneProjectedDeliveryDateIndex),
                    Project = project
                };

                var association = new MilestonePerson
                {
                    Id = reader.GetInt32(milestonePersonIdIndex),
                    Milestone = milestone,
                    Person = new Person { Id = reader.GetInt32(personIdIndex) }
                };

                if (!reader.IsDBNull(personSeniorityIdIndex))
                {
                    association.Person.Seniority = new Seniority { Id = reader.GetInt32(personSeniorityIdIndex) };
                }

                result.Add(association);
            }
        }

        private static void ReadMilestonePersonEntries(SqlDataReader reader, List<MilestonePersonEntry> result)
        {
            if (!reader.HasRows) return;
            var milestonePersonEntryIdIndex = reader.GetOrdinal(Constants.ColumnNames.EntryId);
            var milestonePersonIdIndex = reader.GetOrdinal(MilestonePersonIdColumn);
            var startDateIndex = reader.GetOrdinal(StartDateColumn);
            var endDateIndex = reader.GetOrdinal(EndDateColumn);
            var personRoleIdIndex = reader.GetOrdinal(PersonRoleIdColumn);
            var personRoleNameIndex = reader.GetOrdinal(PersonRoleNameColumn);
            var amountIndex = reader.GetOrdinal(AmountColumn);
            var hoursPerDayIndex = reader.GetOrdinal(HoursPerDayColumn);
            var personVacationsOnMilestoneIndex = reader.GetOrdinal(PersonVacationsOnMilestoneColumn);
            var expectedHoursWithVacationDaysIndex = reader.GetOrdinal(ExpectedHoursWithVacationDaysColumn);
            var personSeniorityIdIndex = reader.GetOrdinal(PersonSeniorityIdColumn);
            var personIdIndex = reader.GetOrdinal(PersonIdColumn);
            var locationIndex = reader.GetOrdinal(LocationColumn);
            var firstNameIndex = reader.GetOrdinal(FirstNameColumn);
            var lastNameIndex = reader.GetOrdinal(LastNameColumn);

            while (reader.Read())
            {
                var entry =
                    new MilestonePersonEntry
                    {
                        Id = reader.GetInt32(milestonePersonEntryIdIndex),
                        MilestonePersonId = reader.GetInt32(milestonePersonIdIndex),
                        StartDate = reader.GetDateTime(startDateIndex),
                        EndDate =
                                !reader.IsDBNull(endDateIndex)
                                    ? (DateTime?)reader.GetDateTime(endDateIndex)
                                    : null,
                        HourlyAmount =
                                !reader.IsDBNull(amountIndex)
                                    ? (decimal?)reader.GetDecimal(amountIndex)
                                    : null,
                        HoursPerDay = reader.GetDecimal(hoursPerDayIndex),
                        VacationDays = reader.GetInt32(personVacationsOnMilestoneIndex),
                        ProjectedWorkloadWithVacation = reader.GetDecimal(expectedHoursWithVacationDaysIndex),
                        ThisPerson = new Person
                        {
                            Id = reader.GetInt32(personIdIndex),
                            FirstName = reader.GetString(firstNameIndex),
                            LastName = reader.GetString(lastNameIndex),
                            Seniority = !reader.IsDBNull(personSeniorityIdIndex) ? new Seniority
                            {
                                Id = reader.GetInt32(personSeniorityIdIndex)
                            } : null
                        },
                        Location = !reader.IsDBNull(locationIndex)
                                           ? reader.GetString(locationIndex)
                                           : null
                    };

                if (!reader.IsDBNull(personRoleIdIndex))
                    entry.Role =
                        new PersonRole
                        {
                            Id = reader.GetInt32(personRoleIdIndex),
                            Name = reader.GetString(personRoleNameIndex)
                        };

                result.Add(entry);
            }
        }

        #endregion Methods

        public static List<MilestonePerson> MilestonePersonsGetByMilestoneId(int milestoneId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (
                var command =
                    new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonsGetByMilestoneId,
                                   connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneIdParam, milestoneId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<MilestonePerson>();

                    ReadMilestonePersonAssociations(reader, result);

                    return result;
                }
            }
        }

        public static MilestonePersonEntry LoadMilestonePersonEntryWithFinancials(int mpeId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command =
                        new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonEntryWithFinancials, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.IdParam, mpeId);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        var entry = LoadMilestonePersonEntry(reader);
                        LoadMilestonePersonEntryFinancials(reader, entry);
                        ReadMilestonePersonEntriesTimeOffs(reader, new List<MilestonePersonEntry>() { entry });
                        return entry;
                    }
                }
            }
        }

        public static void LoadMilestonePersonEntriesWithFinancials(List<MilestonePerson> milestonePersons, int milestoneId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (
                var command =
                    new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestonePersonEntriesWithFinancialsByMilestoneId,
                                   connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneIdParam, milestoneId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    LoadMilestonePersonEntries(reader, milestonePersons);
                    LoadMilestonePersonEntriesFinancials(reader, milestonePersons);
                    ReadMilestonePersonEntriesTimeOffs(reader, milestonePersons);
                }
            }
        }

        private static void LoadMilestonePersonEntriesFinancials(SqlDataReader reader, List<MilestonePerson> milestonePersons)
        {
            if (!reader.NextResult() || !reader.HasRows) return;
            int revenueIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueColumn);
            int revenueNetIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueNetColumn);
            int grossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.GrossMarginColumn);
            var milestonePersonEntryIdIndex = reader.GetOrdinal(Constants.ColumnNames.EntryId);
            var personIdIndex = reader.GetOrdinal(PersonIdColumn);

            while (reader.Read())
            {
                var personId = reader.GetInt32(personIdIndex);
                var entryId = reader.GetInt32(milestonePersonEntryIdIndex);
                var entry = (milestonePersons.Find(mp => mp.Person.Id.Value == personId)).Entries.Find(e => e.Id == entryId);
                entry.ComputedFinancials
                    = new ComputedFinancials
                    {
                        Revenue = reader.GetDecimal(revenueIndex),
                        GrossMargin = reader.GetDecimal(grossMarginIndex),
                        RevenueNet = reader.GetDecimal(revenueNetIndex)
                    };
            }
        }

        private static MilestonePersonEntry LoadMilestonePersonEntry(SqlDataReader reader)
        {
            if (reader.HasRows)
            {
                var milestonePersonEntryIdIndex = reader.GetOrdinal(Constants.ColumnNames.Id);
                var milestonePersonIdIndex = reader.GetOrdinal(MilestonePersonIdColumn);
                var startDateIndex = reader.GetOrdinal(StartDateColumn);
                var endDateIndex = reader.GetOrdinal(EndDateColumn);
                var personRoleIdIndex = reader.GetOrdinal(PersonRoleIdColumn);
                var personRoleNameIndex = reader.GetOrdinal(PersonRoleNameColumn);
                var amountIndex = reader.GetOrdinal(AmountColumn);
                var hoursPerDayIndex = reader.GetOrdinal(HoursPerDayColumn);
                var personVacationsOnMilestoneIndex = reader.GetOrdinal(PersonVacationsOnMilestoneColumn);
                var expectedHoursWithVacationDaysIndex = reader.GetOrdinal(ExpectedHoursWithVacationDaysColumn);
                var personSeniorityIdIndex = reader.GetOrdinal(PersonSeniorityIdColumn);
                var personIdIndex = reader.GetOrdinal(PersonIdColumn);
                var firstNameIndex = reader.GetOrdinal(FirstNameColumn);
                var lastNameIndex = reader.GetOrdinal(LastNameColumn);
                var hasTimeEntriesIndex = reader.GetOrdinal(HasTimeEntriesColumn);
                var isStrawManIndex = reader.GetOrdinal(Constants.ColumnNames.IsStrawmanColumn);
                var isBadgeRequiredIndex = reader.GetOrdinal(Constants.ColumnNames.IsBadgeRequired);
                var badgeStartDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeStartDate);
                var badgeEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeEndDate);
                var isBadgeExceptionIndex = reader.GetOrdinal(Constants.ColumnNames.IsBadgeException);
                var isApprovedIndex = reader.GetOrdinal(Constants.ColumnNames.IsApproved);
                var consultantEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.ConsultantEndDate);
                var personDiscountIndex = reader.GetOrdinal(Constants.ColumnNames.MilestonePersonDiscount);
                var isNewtoBudgetIndex = reader.GetOrdinal(Constants.ColumnNames.IsNewToBudget);
                var discountLockedIndex = reader.GetOrdinal(Constants.ColumnNames.LockDiscount);

                while (reader.Read())
                {
                    var entry =
                        new MilestonePersonEntry
                        {
                            Id = reader.GetInt32(milestonePersonEntryIdIndex),
                            MilestonePersonId = reader.GetInt32(milestonePersonIdIndex),
                            StartDate = reader.GetDateTime(startDateIndex),
                            EndDate =
                                !reader.IsDBNull(endDateIndex)
                                    ? (DateTime?)reader.GetDateTime(endDateIndex)
                                    : null,
                            HourlyAmount =
                                !reader.IsDBNull(amountIndex)
                                    ? (decimal?)reader.GetDecimal(amountIndex)
                                    : null,
                            HoursPerDay = reader.GetDecimal(hoursPerDayIndex),
                            VacationDays = reader.GetInt32(personVacationsOnMilestoneIndex),
                            ProjectedWorkloadWithVacation = reader.GetDecimal(expectedHoursWithVacationDaysIndex),
                            ThisPerson = new Person
                            {
                                Id = reader.GetInt32(personIdIndex),
                                FirstName = reader.GetString(firstNameIndex),
                                LastName = reader.GetString(lastNameIndex),
                                IsStrawMan = reader.GetBoolean(isStrawManIndex),
                                Seniority = !reader.IsDBNull(personSeniorityIdIndex) ? new Seniority
                                {
                                    Id = reader.GetInt32(personSeniorityIdIndex)
                                } : null
                            },
                            HasTimeEntries = reader.GetBoolean(hasTimeEntriesIndex),
                            MSBadgeRequired = !reader.IsDBNull(isBadgeRequiredIndex) ? reader.GetBoolean(isBadgeRequiredIndex) : false,
                            BadgeException = !reader.IsDBNull(isBadgeExceptionIndex) ? reader.GetBoolean(isBadgeExceptionIndex) : false,
                            IsApproved = !reader.IsDBNull(isApprovedIndex) ? reader.GetBoolean(isApprovedIndex) : false,
                            BadgeStartDate = !reader.IsDBNull(badgeStartDateIndex) ? (DateTime?)reader.GetDateTime(badgeStartDateIndex) : null,
                            BadgeEndDate = !reader.IsDBNull(badgeEndDateIndex) ? (DateTime?)reader.GetDateTime(badgeEndDateIndex) : null,
                            ConsultantEndDate = !reader.IsDBNull(consultantEndDateIndex) ? (DateTime?)reader.GetDateTime(consultantEndDateIndex) : null,
                            Discount = !reader.IsDBNull(personDiscountIndex) ? (decimal?)reader.GetDecimal(personDiscountIndex) : null,
                            IsNewToBudget = reader.GetBoolean(isNewtoBudgetIndex),
                            DiscountLocked=reader.GetBoolean(discountLockedIndex)
                        };

                    if (!reader.IsDBNull(personRoleIdIndex))
                        entry.Role =
                            new PersonRole
                            {
                                Id = reader.GetInt32(personRoleIdIndex),
                                Name = reader.GetString(personRoleNameIndex)
                            };

                    return entry;
                }
            }

            return null;
        }

        private static void LoadMilestonePersonEntryFinancials(SqlDataReader reader, MilestonePersonEntry mpe)
        {
            if (!reader.NextResult() || !reader.HasRows) return;
            int revenueIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueColumn);
            int revenueNetIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueNetColumn);
            int grossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.GrossMarginColumn);

            while (reader.Read())
            {
                mpe.ComputedFinancials
                    = new ComputedFinancials
                    {
                        Revenue = reader.GetDecimal(revenueIndex),
                        RevenueNet = reader.GetDecimal(revenueNetIndex),
                        GrossMargin = reader.GetDecimal(grossMarginIndex),
                    };
            }
        }

        private static void LoadMilestonePersonEntries(SqlDataReader reader, List<MilestonePerson> milestonePersons)
        {
            if (!reader.HasRows) return;
            var milestonePersonEntryIdIndex = reader.GetOrdinal(Constants.ColumnNames.Id);
            var milestonePersonIdIndex = reader.GetOrdinal(MilestonePersonIdColumn);
            var startDateIndex = reader.GetOrdinal(StartDateColumn);
            var endDateIndex = reader.GetOrdinal(EndDateColumn);
            var personRoleIdIndex = reader.GetOrdinal(PersonRoleIdColumn);
            var personRoleNameIndex = reader.GetOrdinal(PersonRoleNameColumn);
            var amountIndex = reader.GetOrdinal(AmountColumn);
            var hoursPerDayIndex = reader.GetOrdinal(HoursPerDayColumn);
            var isBadgeRequiredIndex = reader.GetOrdinal(Constants.ColumnNames.IsBadgeRequired);
            var badgeStartDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeStartDate);
            var badgeEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeEndDate);
            var isBadgeExceptionIndex = reader.GetOrdinal(Constants.ColumnNames.IsBadgeException);
            var isApprovedIndex = reader.GetOrdinal(Constants.ColumnNames.IsApproved);
            var consultantEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.ConsultantEndDate);

            var personVacationsOnMilestoneIndex = reader.GetOrdinal(PersonVacationsOnMilestoneColumn);
            var expectedHoursWithVacationDaysIndex = reader.GetOrdinal(ExpectedHoursWithVacationDaysColumn);
            var personSeniorityIdIndex = reader.GetOrdinal(PersonSeniorityIdColumn);
            var personIdIndex = reader.GetOrdinal(PersonIdColumn);
            var firstNameIndex = reader.GetOrdinal(FirstNameColumn);
            var lastNameIndex = reader.GetOrdinal(LastNameColumn);
            var hasTimeEntriesIndex = reader.GetOrdinal(HasTimeEntriesColumn);
            var isStrawManIndex = reader.GetOrdinal(Constants.ColumnNames.IsStrawmanColumn);
            var personDiscountIndex = reader.GetOrdinal(Constants.ColumnNames.MilestonePersonDiscount);
            var isNewtoBudgetIndex = reader.GetOrdinal(Constants.ColumnNames.IsNewToBudget);
            var discountLockedIndex = reader.GetOrdinal(Constants.ColumnNames.LockDiscount);

            while (reader.Read())
            {
                var entry =
                    new MilestonePersonEntry
                    {
                        Id = reader.GetInt32(milestonePersonEntryIdIndex),
                        MilestonePersonId = reader.GetInt32(milestonePersonIdIndex),
                        StartDate = reader.GetDateTime(startDateIndex),
                        EndDate =
                                !reader.IsDBNull(endDateIndex)
                                    ? (DateTime?)reader.GetDateTime(endDateIndex)
                                    : null,
                        HourlyAmount =
                                !reader.IsDBNull(amountIndex)
                                    ? (decimal?)reader.GetDecimal(amountIndex)
                                    : null,
                        HoursPerDay = reader.GetDecimal(hoursPerDayIndex),
                        VacationDays = reader.GetInt32(personVacationsOnMilestoneIndex),
                        ProjectedWorkloadWithVacation = reader.GetDecimal(expectedHoursWithVacationDaysIndex),
                        ThisPerson = new Person
                        {
                            Id = reader.GetInt32(personIdIndex),
                            FirstName = reader.GetString(firstNameIndex),
                            LastName = reader.GetString(lastNameIndex),
                            IsStrawMan = reader.GetBoolean(isStrawManIndex),
                            Seniority = !reader.IsDBNull(personSeniorityIdIndex)
                                                    ? new Seniority
                                                    {
                                                        Id = reader.GetInt32(personSeniorityIdIndex)
                                                    } : null
                        },
                        HasTimeEntries = reader.GetBoolean(hasTimeEntriesIndex),
                        MSBadgeRequired = !reader.IsDBNull(isBadgeRequiredIndex) ? reader.GetBoolean(isBadgeRequiredIndex) : false,
                        BadgeException = !reader.IsDBNull(isBadgeExceptionIndex) ? reader.GetBoolean(isBadgeExceptionIndex) : false,
                        IsApproved = !reader.IsDBNull(isApprovedIndex) ? reader.GetBoolean(isApprovedIndex) : false,
                        BadgeStartDate = !reader.IsDBNull(badgeStartDateIndex) ? (DateTime?)reader.GetDateTime(badgeStartDateIndex) : null,
                        BadgeEndDate = !reader.IsDBNull(badgeEndDateIndex) ? (DateTime?)reader.GetDateTime(badgeEndDateIndex) : null,
                        ConsultantEndDate = !reader.IsDBNull(consultantEndDateIndex) ? (DateTime?)reader.GetDateTime(consultantEndDateIndex) : null,
                        Discount = !reader.IsDBNull(personDiscountIndex) ? (decimal?)reader.GetDecimal(personDiscountIndex) : null,
                        IsNewToBudget = reader.GetBoolean(isNewtoBudgetIndex),
                        DiscountLocked=reader.GetBoolean(discountLockedIndex)
                    };

                if (!reader.IsDBNull(personRoleIdIndex))
                    entry.Role =
                        new PersonRole
                        {
                            Id = reader.GetInt32(personRoleIdIndex),
                            Name = reader.GetString(personRoleNameIndex)
                        };

                var mperson = milestonePersons.Find(mp => mp.Person != null && mp.Person.Id == entry.ThisPerson.Id);
                if (mperson.Entries == null)
                {
                    mperson.Entries = new List<MilestonePersonEntry>();
                }
                mperson.Entries.Add(entry);
            }
        }

        public static void SaveMilestonePersonsWrapper(List<MilestonePerson> milestonePersons, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                connection.Open();
                var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                foreach (var mp in milestonePersons)
                {
                    SaveMilestonePerson(mp, userName, connection, transaction);
                }

                transaction.Commit();
            }
        }

        public static void DeleteMilestonePersonEntry(int milestonePersonEntryId, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.DeleteMilestonePersonEntry, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.Id, milestonePersonEntryId);
                    command.Parameters.AddWithValue(UserLoginParam,
                                                !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static int MilestonePersonAndEntryInsert(MilestonePerson milestonePerson, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                connection.Open();
                var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                if (!milestonePerson.Id.HasValue)
                {
                    MilestonePersonInsert(milestonePerson, connection, transaction);
                }

                int mpid = 0;
                foreach (var entry in milestonePerson.Entries)
                {
                    entry.MilestonePersonId = milestonePerson.Id.Value;
                    mpid = MilestonePersonEntryInsert(entry, userName, connection, transaction);
                }

                transaction.Commit();

                return mpid;
            }
        }

        public static int UpdateMilestonePersonEntry(MilestonePersonEntry entry, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.UpdateMilestonePersonEntry, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.Id, entry.Id);
                    command.Parameters.AddWithValue(PersonIdParam,
                                               entry.ThisPerson.Id.HasValue
                                                   ? (object)entry.ThisPerson.Id.Value
                                                   : DBNull.Value);

                    var milestonePersonId = new SqlParameter
                    {
                        ParameterName = MilestonePersonIdParam,
                        SqlDbType = SqlDbType.Int,
                        Value = entry.MilestonePersonId,
                        Direction = ParameterDirection.InputOutput
                    };
                    command.Parameters.Add(milestonePersonId);

                    command.Parameters.AddWithValue(StartDateParam, entry.StartDate);
                    command.Parameters.AddWithValue(EndDateParam,
                                                    entry.EndDate.HasValue ? (object)entry.EndDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue(HoursPerDayParam, entry.HoursPerDay);
                    command.Parameters.AddWithValue(PersonRoleIdParam,
                                                    entry.Role != null ? (object)entry.Role.Id : DBNull.Value);
                    command.Parameters.AddWithValue(AmountParam,
                                                    entry.HourlyAmount.HasValue
                                                        ? (object)entry.HourlyAmount.Value.Value
                                                        : DBNull.Value);
                    command.Parameters.AddWithValue(Constants.ParameterNames.IsBadgeRequired, entry.MSBadgeRequired);
                    command.Parameters.AddWithValue(Constants.ParameterNames.BadgeStartDate, entry.BadgeStartDate.HasValue ? (object)entry.BadgeStartDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue(Constants.ParameterNames.BadgeEndDate, entry.BadgeEndDate.HasValue ? (object)entry.BadgeEndDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue(Constants.ParameterNames.IsBadgeException, entry.BadgeException);
                    command.Parameters.AddWithValue(Constants.ParameterNames.IsApproved, entry.IsApproved);
                    command.Parameters.AddWithValue(Constants.ParameterNames.RequestDate, entry.RequestDate.HasValue ? (object)entry.RequestDate.Value : DBNull.Value);
                    command.Parameters.AddWithValue(UserLoginParam,
                                                    !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);
                    command.Parameters.AddWithValue(Constants.ParameterNames.DiscountParam, entry.Discount);
                    command.Parameters.AddWithValue(Constants.ParameterNames.LockDiscount, entry.DiscountLocked);

                    command.Parameters.AddWithValue(Constants.ParameterNames.IsDiscountAtResourceLevel, entry.IsDiscountAtResourceLevel);

                    connection.Open();

                    command.ExecuteNonQuery();

                    return Convert.ToInt32(milestonePersonId.Value);
                }
            }
        }

        public static void MilestoneResourceUpdate(Milestone milestone, MilestoneUpdateObject milestoneUpdateObj, string userName)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.MilestoneResourceUpdateProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestone.Id.Value);

                command.Parameters.AddWithValue(StartDateParam, milestone.StartDate);
                command.Parameters.AddWithValue(ProjectedDeliveryDateParam, milestone.ProjectedDeliveryDate);

                command.Parameters.AddWithValue(Constants.ParameterNames.IsStartDateChangeReflectedForMilestoneAndPersons,
                    milestoneUpdateObj.IsStartDateChangeReflectedForMilestoneAndPersons.HasValue ?
                    (object)milestoneUpdateObj.IsStartDateChangeReflectedForMilestoneAndPersons.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsEndDateChangeReflectedForMilestoneAndPersons,
                     milestoneUpdateObj.IsEndDateChangeReflectedForMilestoneAndPersons.HasValue ?
                    (object)milestoneUpdateObj.IsEndDateChangeReflectedForMilestoneAndPersons.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsExtendedORCompleteOutOfRange,
                    milestoneUpdateObj.IsExtendedORCompleteOutOfRange.HasValue ?
                   (object)milestoneUpdateObj.IsExtendedORCompleteOutOfRange.Value : DBNull.Value);
                command.Parameters.AddWithValue(UserLoginParam, userName);
                connection.Open();

                SqlTransaction trn = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Transaction = trn;

                command.ExecuteNonQuery();

                trn.Commit();
            }
        }

    }
}

