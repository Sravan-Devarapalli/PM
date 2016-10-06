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
    /// Access milestone data in database
    /// </summary>
    public static class MilestoneDAL
    {
        #region Constants

        #region Parameters

        private const string ProjectIdParam = "@ProjectId";
        private const string MilestoneIdParam = "@MilestoneId";
        private const string MilestonePersonIdParam = "@MilestonePersonId";
        private const string DescriptionParam = "@Description";
        private const string AmountParam = "@Amount";
        private const string StartDateParam = "@StartDate";
        private const string EndDateParam = "@EndDate";
        private const string ProjectedDeliveryDateParam = "@ProjectedDeliveryDate";
        private const string IsHourlyAmountParam = "@IsHourlyAmount";
        private const string ShiftDaysParam = "@ShiftDays";
        private const string MoveFutureMilestonesParam = "@MoveFutureMilestones";
        private const string CloneDurationParam = "@CloneDuration";
        private const string MilestoneCloneIdParam = "@MilestoneCloneId";
        private const string UserLoginParam = "@UserLogin";
        private const string ClientIdParam = "@ClientId";
        private const string LowerBoundParam = "@LowerBound";
        private const string UpperBoundParam = "@UpperBound";

        #endregion Parameters

        #region Stored Procedures

        private const string MilestoneListByProjectProcedure = "dbo.MilestoneListByProject";

        private const string MilestoneListByProjectForTimeEntryByProjectReportProcedure =
            "dbo.MilestoneListByProjectForTimeEntryByProjectReport";

        private const string MilestoneGetByIdProcedure = "dbo.MilestoneGetById";
        private const string MilestoneInsertProcedure = "dbo.MilestoneInsert";
        private const string MilestoneUpdateProcedure = "dbo.MilestoneUpdate";
        private const string MilestoneUpdateShortDetailsProcedure = "dbo.MilestoneUpdateShortDetails";
        private const string MilestoneDeleteProcedure = "dbo.MilestoneDelete";
        private const string MilestoneMoveProcedure = "dbo.MilestoneMove";
        private const string MilestoneMoveEndProcedure = "dbo.MilestoneMoveEnd";
        private const string MilestoneCloneProcedure = "dbo.MilestoneClone";
        private const string GetMilestoneAndCSATCountsByProjectProcedure = "GetMilestoneAndCSATCountsByProject";
        private const string DefaultMileStoneInsertProcedure = "dbo.DefaultMilestoneSettingInsert";
        private const string DefaultMileStoneGetProcedure = "dbo.GetDefaultMilestoneSetting";

        public const string GetPersonMilestonesAfterTerminationDateProcedure =
            "dbo.GetPersonMilestonesAfterTerminationDate";

        public const string CheckIfExpensesExistsForMilestonePeriodProcedure =
            "dbo.CheckIfExpensesExistsForMilestonePeriod";

        public const string CanMoveFutureMilestonesProcedure = "dbo.CanMoveFutureMilestones";

        #endregion Stored Procedures

        #region Columns

        private const string ProjectStartDateColumn = "ProjectStartDate";
        private const string ProjectEndDateColumn = "ProjectEndDate";
        private const string IsHourlyAmountColumn = "IsHourlyAmount";
        private const string ExpectedHoursColumn = "ExpectedHours";
        private const string DiscountColumn = "Discount";
        private const string PersonCountColumn = "PersonCount";
        private const string ProjectedDurationColumn = "ProjectedDuration";
        private const string ClientIdColumn = "ClientId";
        private const string ProjectIdColumn = "ProjectId";
        private const string MilestoneIdColumn = "MilestoneId";
        private const string ModifiedDateColumn = "ModifiedDate";
        private const string LowerBoundColumn = "LowerBound";
        private const string UpperBoundColumn = "UpperBound";

        #endregion Columns

        #endregion Constants

        /// <summary>
        /// Saves Default Project-milestone details into DB. Persons not assigned to any Project-Milestone
        /// can enter time entery for this default Project Milestone.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="ProjectId"></param>
        /// <param name="MileStoneId"></param>
        public static void SaveDefaultMilestone(int? clientId, int? ProjectId, int? MilestoneId, int? lowerBound,
                                                int? upperBound)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(DefaultMileStoneInsertProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;
                if (clientId.HasValue)
                    command.Parameters.AddWithValue(ClientIdParam, clientId.Value);
                if (ProjectId.HasValue)
                    command.Parameters.AddWithValue(ProjectIdParam, ProjectId.Value);
                if (MilestoneId.HasValue)
                    command.Parameters.AddWithValue(MilestoneIdParam, MilestoneId.Value);
                if (lowerBound.HasValue)
                    command.Parameters.AddWithValue(LowerBoundParam, lowerBound.Value);
                if (lowerBound.HasValue)
                    command.Parameters.AddWithValue(UpperBoundParam, upperBound.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static DefaultMilestone GetDefaultMilestone()
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(DefaultMileStoneGetProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var result = ReadDefaultMilestone(reader);

                    return result;
                }
            }
        }

        /// <summary>
        /// Lists <see cref="Milestones"/> by the specified Project.
        /// </summary>
        /// <param name="projectId">An ID of the project to the data be retrived for.</param>
        /// <returns>The list of the <see cref="Milestone"/> objects.</returns>
        public static List<Milestone> MilestoneListByProject(int projectId)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(MilestoneListByProjectProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(ProjectIdParam, projectId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    List<Milestone> result = new List<Milestone>();

                    ReadMilestones(reader, result);

                    return result;
                }
            }
        }

        public static List<Milestone> MilestoneListByProjectForTimeEntryByProjectReport(int projectId)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (
                SqlCommand command = new SqlCommand(MilestoneListByProjectForTimeEntryByProjectReportProcedure,
                                                    connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(ProjectIdParam, projectId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    List<Milestone> result = new List<Milestone>();

                    ReadMilestoneShort(reader, result);

                    return result;
                }
            }
        }

        public static List<Milestone> GetPersonMilestonesAfterTerminationDate(int personId, DateTime terminationDate)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(GetPersonMilestonesAfterTerminationDateProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonId, personId);
                command.Parameters.AddWithValue(Constants.ParameterNames.TerminationDate, terminationDate);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    List<Milestone> result = new List<Milestone>();

                    ReadMilestonesForPersonTermination(reader, result);

                    return result;
                }
            }
        }

        private static void ReadMilestonesForPersonTermination(SqlDataReader reader, List<Milestone> result)
        {
            if (!reader.HasRows) return;
            int projectIdIndex = reader.GetOrdinal("ProjectId");
            int milestoneIdIndex = reader.GetOrdinal("MilestoneId");
            int descriptionIndex = reader.GetOrdinal("MilestoneName");
            int projectNameIndex = reader.GetOrdinal("ProjectName");
            int projectNumberIndex = reader.GetOrdinal("ProjectNumber");

            while (reader.Read())
            {
                Milestone milestone = new Milestone
                    {
                        Id = reader.GetInt32(milestoneIdIndex),
                        Description = reader.GetString(descriptionIndex),
                        Project =
                            new Project
                                {
                                    Id = reader.GetInt32(projectIdIndex),
                                    Name = reader.GetString(projectNameIndex),
                                    ProjectNumber = reader.GetString(projectNumberIndex)
                                }
                    };

                result.Add(milestone);
            }
        }

        public static List<Milestone> GetPersonMilestonesOnPreviousHireDate(int personId, DateTime previousHireDate)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.GetPersonMilestonesOnPreviousHireDate, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonId, personId);
                command.Parameters.AddWithValue(Constants.ParameterNames.PreviousHireDate, previousHireDate);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    List<Milestone> result = new List<Milestone>();

                    ReadPersonMilestonesOnPreviousHireDate(reader, result);

                    return result;
                }
            }
        }

        private static void ReadPersonMilestonesOnPreviousHireDate(SqlDataReader reader, List<Milestone> result)
        {
            if (!reader.HasRows) return;
            int projectIdIndex = reader.GetOrdinal("ProjectId");
            int milestoneIdIndex = reader.GetOrdinal("MilestoneId");
            int descriptionIndex = reader.GetOrdinal("Description");
            int projectNameIndex = reader.GetOrdinal("ProjectName");
            int projectNumberIndex = reader.GetOrdinal("ProjectNumber");

            while (reader.Read())
            {
                Milestone milestone = new Milestone
                {
                    Id = reader.GetInt32(milestoneIdIndex),
                    Description = reader.GetString(descriptionIndex),
                    Project =
                        new Project
                        {
                            Id = reader.GetInt32(projectIdIndex),
                            Name = reader.GetString(projectNameIndex),
                            ProjectNumber = reader.GetString(projectNumberIndex)
                        }
                };

                result.Add(milestone);
            }
        }

        /// <summary>
        /// Retrieves a <see cref="Milestone"/> by the specified ID.
        /// </summary>
        /// <param name="milestoneId">An ID of the <see cref="Milestone"/> to be retrieved.</param>
        /// <returns>The <see cref="Milestone"/> object when found and null otherwise.</returns>
        public static Milestone GetById(int milestoneId)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(MilestoneGetByIdProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestoneId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    List<Milestone> result = new List<Milestone>(1);

                    ReadMilestones(reader, result);

                    return result.Count > 0 ? result[0] : null;
                }
            }
        }

        /// <summary>
        /// Inserts a <see cref="Milestone"/> into the database.
        /// </summary>
        /// <param name="milestone">The <see cref="Milestine"/> to be inserted to.</param>
        /// <param name="userName">A current user.</param>
        public static void MilestoneInsert(Milestone milestone, string userName)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(MilestoneInsertProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(ProjectIdParam,
                                                (milestone.Project != null && milestone.Project.Id.HasValue)
                                                    ? (object)milestone.Project.Id.Value
                                                    : DBNull.Value
                    );
                command.Parameters.AddWithValue(DescriptionParam,
                                                !string.IsNullOrEmpty(milestone.Description)
                                                    ? (object)milestone.Description
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(AmountParam,
                                                milestone.Amount.HasValue
                                                    ? (object)milestone.Amount.Value.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(StartDateParam, milestone.StartDate);
                command.Parameters.AddWithValue(ProjectedDeliveryDateParam, milestone.ProjectedDeliveryDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsChargeable, milestone.IsChargeable);
                command.Parameters.AddWithValue(Constants.ParameterNames.ConsultantsCanAdjust,
                                                milestone.ConsultantsCanAdjust);
                command.Parameters.AddWithValue(IsHourlyAmountParam, milestone.IsHourlyAmount);
                command.Parameters.AddWithValue(UserLoginParam,
                                                !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);

                SqlParameter milestoneIdParam = new SqlParameter(MilestoneIdParam, SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                command.Parameters.Add(milestoneIdParam);

                connection.Open();

                SqlTransaction trn = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Transaction = trn;

                command.ExecuteNonQuery();

                milestone.Id = (int)milestoneIdParam.Value;

                trn.Commit();
            }
        }

        /// <summary>
        /// Update a <see cref="Milestone"/> in the database.
        /// </summary>
        /// <param name="milestone">The <see cref="Milestine"/> to be updated.</param>
        /// <param name="userName">A current user.</param>
        public static void MilestoneUpdate(Milestone milestone, string userName)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(MilestoneUpdateProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestone.Id.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ConsultantsCanAdjust,
                                                milestone.ConsultantsCanAdjust);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsChargeable, milestone.IsChargeable);
                command.Parameters.AddWithValue(ProjectIdParam,
                                                milestone.Project != null && milestone.Project.Id.HasValue
                                                    ? (object)milestone.Project.Id.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(DescriptionParam,
                                                !string.IsNullOrEmpty(milestone.Description)
                                                    ? (object)milestone.Description
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(AmountParam,
                                                milestone.Amount.HasValue
                                                    ? (object)milestone.Amount.Value.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(StartDateParam, milestone.StartDate);
                command.Parameters.AddWithValue(ProjectedDeliveryDateParam, milestone.ProjectedDeliveryDate);
                command.Parameters.AddWithValue(IsHourlyAmountParam, milestone.IsHourlyAmount);
                command.Parameters.AddWithValue(UserLoginParam,
                                                !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);

                connection.Open();

                SqlTransaction trn = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                command.Transaction = trn;

                command.ExecuteNonQuery();

                trn.Commit();
            }
        }

        public static void MilestoneUpdateShortDetails(Milestone milestone, string userName)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(MilestoneUpdateShortDetailsProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestone.Id.Value);
                command.Parameters.AddWithValue(DescriptionParam,
                                                !string.IsNullOrEmpty(milestone.Description)
                                                    ? (object)milestone.Description
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(UserLoginParam,
                                                !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);

                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deletes a <see cref="Milestone"/> from the database.
        /// </summary>
        /// <param name="milestone">The <see cref="Milestine"/> to be deleted from.</param>
        /// <param name="userName">A current user.</param>
        public static void MilestoneDelete(Milestone milestone, string userName)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(MilestoneDeleteProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestone.Id.Value);
                command.Parameters.AddWithValue(UserLoginParam,
                                                !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Moves the specified milestone and optionally future milestones forward and backward.
        /// </summary>
        /// <param name="milestoneId">An ID of the <see cref="Milestone"/> to be moved.</param>
        /// <param name="shiftDays">A number of days to move.</param>
        /// <param name="moveFutureMilestones">Determines whether future milestones must be moved too.</param>
        public static List<MSBadge> MilestoneMove(int milestoneId, int shiftDays, bool moveFutureMilestones)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(MilestoneMoveProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestoneId);
                command.Parameters.AddWithValue(ShiftDaysParam, shiftDays);
                command.Parameters.AddWithValue(MoveFutureMilestonesParam, moveFutureMilestones);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    List<MSBadge> result = new List<MSBadge>();
                    ReadBadgeRecords(reader, result);
                    return result;
                }
            }
        }

        public static void ReadBadgeRecords(SqlDataReader reader, List<MSBadge> result)
        {
            try
            {
                if (!reader.HasRows) return;
              
                int projectIdIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectId);
                int projectNameIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectName);
                int projectNumberIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectNumber);
                int badgeStartDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeStartDate);
                int badgeEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeEndDate);
                int newBadgeStartDateIndex = reader.GetOrdinal(Constants.ColumnNames.NewBadgeStartDate);
                int newBadgeEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.NewBadgeEndDate);
                int personIdIndex = reader.GetOrdinal(Constants.ColumnNames.PersonId);
                int lastNameIndex = reader.GetOrdinal(Constants.ColumnNames.LastName);
                int firstNameIndex = reader.GetOrdinal(Constants.ColumnNames.FirstName);
                int isBadgeExceptionIndex = reader.GetOrdinal(Constants.ColumnNames.IsBadgeException);

                while (reader.Read())
                {
                    var badgeResource = new MSBadge()
                    {
                        Person = new Person()
                        {
                            Id = reader.GetInt32(personIdIndex),
                            LastName = reader.GetString(lastNameIndex),
                            FirstName = reader.GetString(firstNameIndex)
                        },
                        Project = new Project()
                        {
                            Id = reader.GetInt32(projectIdIndex),
                            Name = reader.GetString(projectNameIndex),
                            ProjectNumber = reader.GetString(projectNumberIndex)
                        },
                        BadgeStartDate = reader.GetDateTime(newBadgeStartDateIndex),
                        BadgeEndDate = reader.GetDateTime(newBadgeEndDateIndex),
                        LastBadgeStartDate = reader.GetDateTime(badgeStartDateIndex),
                        LastBadgeEndDate = reader.GetDateTime(badgeEndDateIndex),
                        IsException = reader.GetBoolean(isBadgeExceptionIndex)
                    };
                    result.Add(badgeResource);
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Moves the specified milestone end date forward and backward.
        /// </summary>
        /// <param name="milestoneId">An ID of the <see cref="Milestone"/> to be moved.</param>
        /// <param name="shiftDays">A number of days to move.</param>
        public static void MilestoneMoveEnd(int milestoneId, int milestonePersonId, int shiftDays)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(MilestoneMoveEndProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestoneId);
                command.Parameters.AddWithValue(ShiftDaysParam, shiftDays);
                command.Parameters.AddWithValue(MilestonePersonIdParam, milestonePersonId);

                connection.Open();

                SqlTransaction trnScope = connection.BeginTransaction();
                command.Transaction = trnScope;

                command.ExecuteNonQuery();

                trnScope.Commit();
            }
        }

        /// <summary>
        /// Clones a specified milestones and set a specified duration to a new one.
        /// </summary>
        /// <param name="milestoneId">An ID of the milestone to be cloned.</param>
        /// <param name="cloneDuration">A clone's duration.</param>
        /// <returns>An ID of a new milestone.</returns>
        public static int MilestoneClone(int milestoneId, int cloneDuration)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(MilestoneCloneProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestoneId);
                command.Parameters.AddWithValue(CloneDurationParam, cloneDuration);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsFromMilestoneDetail, true);

                SqlParameter cloneIdParam =
                    new SqlParameter(MilestoneCloneIdParam, SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(cloneIdParam);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();

                    return (int)cloneIdParam.Value;
                }
                catch (SqlException ex)
                {
                    throw new DataAccessException(ex);
                }
            }
        }

        public static List<int> GetMilestoneAndCSATCountsByProject(int projectId)
        {
            List<int> result = new List<int>();
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(GetMilestoneAndCSATCountsByProjectProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(ProjectIdParam, projectId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    int milestoneCountIndex = reader.GetOrdinal(Constants.ColumnNames.MilestoneCountColumn);
                    int csatCountIndex = reader.GetOrdinal(Constants.ColumnNames.CSATCountColumn);
                    int attributionCountIndex = reader.GetOrdinal(Constants.ColumnNames.AttributionCount);
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt32(milestoneCountIndex));
                        result.Add(reader.GetInt32(csatCountIndex));
                        result.Add(reader.GetInt32(attributionCountIndex));
                    }
                }
            }
            return result;
        }

        public static bool CheckIfExpensesExistsForMilestonePeriod(int milestoneId, DateTime? startDate,
                                                                   DateTime? EndDate)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(CheckIfExpensesExistsForMilestonePeriodProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestoneId);
                if (startDate.HasValue)
                    command.Parameters.AddWithValue(StartDateParam, startDate.Value);
                if (EndDate.HasValue)
                    command.Parameters.AddWithValue(EndDateParam, EndDate.Value);

                connection.Open();

                return Boolean.Parse(command.ExecuteScalar().ToString());
            }
        }

        public static bool CanMoveFutureMilestones(int milestoneId, int shiftDays)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(CanMoveFutureMilestonesProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(MilestoneIdParam, milestoneId);
                command.Parameters.AddWithValue(ShiftDaysParam, shiftDays);
                connection.Open();
                return Boolean.Parse(command.ExecuteScalar().ToString());
            }
        }

        /// <summary>
        /// Reades the records from the DbDataReader
        /// </summary>
        /// <param name="reader">The reader the data be read from.</param>
        /// <param name="result">The list of <see cref="Milestone"/> objects.</param>
        private static void ReadMilestones(DbDataReader reader, List<Milestone> result)
        {
            if (!reader.HasRows) return;
            int clientIdIndex = reader.GetOrdinal("ClientId");
            int projectIdIndex = reader.GetOrdinal("ProjectId");
            int projectStatusIdIndex = -1;
            int projectStatusNameIndex = -1;
            int projectNumberIndex = -1;

            int milestoneIdIndex = reader.GetOrdinal("MilestoneId");
            int descriptionIndex = reader.GetOrdinal("Description");
            int amountIndex = reader.GetOrdinal("Amount");
            int startDateIndex = reader.GetOrdinal("StartDate");
            int projectedDeliveryDateIndex = reader.GetOrdinal("ProjectedDeliveryDate");
            int clientNameIndex = reader.GetOrdinal("ClientName");
            int projectNameIndex = reader.GetOrdinal("ProjectName");
            int projectStartDateIndex = reader.GetOrdinal(ProjectStartDateColumn);
            int projectEndDateIndex = reader.GetOrdinal(ProjectEndDateColumn);
            int isHourlyAmountIndex = reader.GetOrdinal(IsHourlyAmountColumn);
            int expectedHoursIndex = reader.GetOrdinal(ExpectedHoursColumn);
            int discountIndex = reader.GetOrdinal(DiscountColumn);

            int personCountIndex = reader.GetOrdinal(PersonCountColumn);
            int projectedDurationIndex = reader.GetOrdinal(ProjectedDurationColumn);
            int milestoneIsChargeableIndex = reader.GetOrdinal(Constants.ColumnNames.MilestoneIsChargeable);
            int consultantsCanAdjustIndex = reader.GetOrdinal(Constants.ColumnNames.ConsultantsCanAdjust);
            int isMarginColorInfoEnabledIndex = -1;

            try
            {
                projectStatusIdIndex = reader.GetOrdinal("ProjectStatusId");
                projectStatusNameIndex = reader.GetOrdinal("ProjectStatusName");
            }
            catch
            {
                projectStatusIdIndex = -1;
                projectStatusNameIndex = -1;
            }

            try
            {
                isMarginColorInfoEnabledIndex = reader.GetOrdinal(Constants.ColumnNames.IsMarginColorInfoEnabledColumn);
            }
            catch
            {
                isMarginColorInfoEnabledIndex = -1;
            }

            try
            {
                projectNumberIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectNumber);
            }
            catch
            {
                projectNumberIndex = -1;
            }

            while (reader.Read())
            {
                Milestone milestone = new Milestone
                    {
                        Id = reader.GetInt32(milestoneIdIndex),
                        Description = reader.GetString(descriptionIndex),
                        Amount = !reader.IsDBNull(amountIndex) ? (decimal?)reader.GetDecimal(amountIndex) : null,
                        StartDate = reader.GetDateTime(startDateIndex),
                        ProjectedDeliveryDate = reader.GetDateTime(projectedDeliveryDateIndex),
                        IsHourlyAmount = reader.GetBoolean(isHourlyAmountIndex),
                        ExpectedHours = reader.GetDecimal(expectedHoursIndex),
                        PersonCount = reader.GetInt32(personCountIndex),
                        ProjectedDuration = reader.GetInt32(projectedDurationIndex),
                        IsChargeable = reader.GetBoolean(milestoneIsChargeableIndex),
                        ConsultantsCanAdjust = reader.GetBoolean(consultantsCanAdjustIndex),
                        Project = new Project
                            {
                                Id = reader.GetInt32(projectIdIndex),
                                Name = reader.GetString(projectNameIndex),
                                Discount = reader.GetDecimal(discountIndex),
                                StartDate = reader.GetDateTime(projectStartDateIndex),
                                EndDate = reader.GetDateTime(projectEndDateIndex),
                                Client = new Client
                                    {
                                        Id = reader.GetInt32(clientIdIndex),
                                        Name = reader.GetString(clientNameIndex)
                                    }
                            }
                    };

                if (projectStatusIdIndex >= 0 && projectStatusNameIndex >= 0)
                {
                    milestone.Project.Status = new ProjectStatus
                        {
                            Id = reader.GetInt32(projectStatusIdIndex),
                            Name = reader.GetString(projectStatusNameIndex)
                        };
                }
                if (projectNumberIndex >= 0)
                {
                    milestone.Project.ProjectNumber = reader.GetString(projectNumberIndex);
                }
                if (isMarginColorInfoEnabledIndex >= 0)
                {
                    try
                    {
                        milestone.Project.Client.IsMarginColorInfoEnabled =
                            reader.GetBoolean(isMarginColorInfoEnabledIndex);
                    }
                    catch
                    {
                    }
                }

                result.Add(milestone);
            }
        }

        private static void ReadMilestoneShort(SqlDataReader reader, List<Milestone> result)
        {
            int milestoneIdIndex = reader.GetOrdinal("MilestoneId");
            int descriptionIndex = reader.GetOrdinal("Description");
            while (reader.Read())
            {
                Milestone milestone = new Milestone
                    {
                        Id = reader.GetInt32(milestoneIdIndex),
                        Description = reader.GetString(descriptionIndex)
                    };

                result.Add(milestone);
            }
        }

        private static DefaultMilestone ReadDefaultMilestone(SqlDataReader reader)
        {
            DefaultMilestone defaultMilestone = null;
            if (reader.HasRows)
            {
                defaultMilestone = new DefaultMilestone();
                int ClientIdIndex = reader.GetOrdinal(ClientIdColumn);
                int ProjectIdIndex = reader.GetOrdinal(ProjectIdColumn);
                int MilestoneIdIndex = reader.GetOrdinal(MilestoneIdColumn);
                int ModifiedDateIndex = reader.GetOrdinal(ModifiedDateColumn);
                int LowerBoundIndex = reader.GetOrdinal(LowerBoundColumn);
                int UpperBoundIndex = reader.GetOrdinal(UpperBoundColumn);

                reader.Read();

                if (!reader.IsDBNull(ClientIdIndex))
                    defaultMilestone.ClientId = reader.GetInt32(ClientIdIndex);
                if (!reader.IsDBNull(ProjectIdIndex))
                    defaultMilestone.ProjectId = reader.GetInt32(ProjectIdIndex);
                if (!reader.IsDBNull(MilestoneIdIndex))
                    defaultMilestone.MilestoneId = reader.GetInt32(MilestoneIdIndex);
                if (!reader.IsDBNull(ModifiedDateIndex))
                    defaultMilestone.ModifiedDate = reader.GetDateTime(ModifiedDateIndex);
                if (!reader.IsDBNull(LowerBoundIndex))
                    defaultMilestone.LowerBound = reader.GetInt32(LowerBoundIndex);
                if (!reader.IsDBNull(UpperBoundIndex))
                    defaultMilestone.UpperBound = reader.GetInt32(UpperBoundIndex);
            }
            return defaultMilestone;
        }

        public static List<Attribution> IsProjectAttributionConflictsWithMilestoneChanges(int milestoneId, DateTime startDate, DateTime endDate, bool isUpdate)
        {
            var result = new List<Attribution>();
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (
                    var command =
                        new SqlCommand(
                            Constants.ProcedureNames.MilestonePerson.IsProjectAttributionConflictsWithMilestoneChanges,
                            connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneId, milestoneId);
                    command.Parameters.AddWithValue(Constants.ParameterNames.StartDate, startDate);
                    command.Parameters.AddWithValue(Constants.ParameterNames.IsUpdate, isUpdate);
                    command.Parameters.AddWithValue(Constants.ParameterNames.EndDate, endDate);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        ReadProjectAttributionValues(reader,result);
                    }
                }
            }
            return result;
        }

        public static List<bool> ShouldAttributionDateExtend(int projectId, DateTime startDate, DateTime endDate)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (
                    var command =
                        new SqlCommand(
                            Constants.ProcedureNames.MilestonePerson.ShouldAttributionDateExtend,
                            connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    command.Parameters.AddWithValue(Constants.ParameterNames.ProjectId, projectId);
                    command.Parameters.AddWithValue(Constants.ParameterNames.StartDate, startDate);
                    command.Parameters.AddWithValue(Constants.ParameterNames.EndDate, endDate);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        return ReadShouldExtendAttributions(reader);
                    }
                }
            }
        }

        public static void ReadProjectAttributionValues(SqlDataReader reader, List<Attribution> result)
        {
            try
            {
                if (!reader.HasRows) return;
                int attributionIdIndex = reader.GetOrdinal(Constants.ColumnNames.AttributionId);
                int attributionTypeIdIndex = reader.GetOrdinal(Constants.ColumnNames.AttributionTypeId);
                int startDateIndex = reader.GetOrdinal(Constants.ColumnNames.StartDateColumn);
                int endDateIndex = reader.GetOrdinal(Constants.ColumnNames.EndDateColumn);
                int targetIdIndex = reader.GetOrdinal(Constants.ColumnNames.TargetId);
                int targetNameIndex = reader.GetOrdinal(Constants.ColumnNames.TargetName);

                while (reader.Read())
                {
                    int targetId = reader.GetInt32(targetIdIndex);
                    string targetName = reader.GetString(targetNameIndex);
                    var attribution = new Attribution()
                    {
                        Id = reader.GetInt32(attributionIdIndex),
                        AttributionType = (AttributionTypes)reader.GetInt32(attributionTypeIdIndex),
                        EndDate = reader.GetDateTime(endDateIndex),
                        StartDate = reader.GetDateTime(startDateIndex),
                        TargetId = targetId,
                        TargetName = targetName
                    };
                    result.Add(attribution);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<bool> ReadShouldExtendAttributions(SqlDataReader reader)
        {
            List<bool> extendAttributionList = new List<bool>();
            try
            {
                if (reader.HasRows)
                {
                    int extendAttributionStartDateIndex =
                        reader.GetOrdinal(Constants.ColumnNames.ExtendAttributionStartDate);
                    int extendAttributionEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.ExtendAttributionEndDate);

                    while (reader.Read())
                    {
                        extendAttributionList.Add(reader.GetBoolean(extendAttributionStartDateIndex));
                        extendAttributionList.Add(reader.GetBoolean(extendAttributionEndDateIndex));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return extendAttributionList;
        }

        public static List<MSBadge> GetPeopleAssignedInOtherProjectsForGivenRange(DateTime milestoneNewStartDate,DateTime milestoneNewEnddate,int milestoneId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.MilestonePerson.GetPeopleAssignedInOtherProjectsForGivenRange, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;
                command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneNewStartDate, milestoneNewStartDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneNewEndDate, milestoneNewEnddate);
                command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneId, milestoneId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<MSBadge>();
                    ReadPersonBadges(reader, result);
                    return result;
                }
            }
        }

        public static void ReadPersonBadges(SqlDataReader reader, List<MSBadge> result)
        {
            try
            {
                if (!reader.HasRows) return;
                int personIdIndex = reader.GetOrdinal(Constants.ColumnNames.PersonId);
                int lastNameIndex = reader.GetOrdinal(Constants.ColumnNames.LastName);
                int firstNameIndex = reader.GetOrdinal(Constants.ColumnNames.FirstName);

                int badgeStartDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeStartDate);
                int badgeEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.BadgeEndDate);
                int projectIdIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectId);
                int projectNameIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectName);
                int projectNumberIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectNumber);

                while (reader.Read())
                {
                    var badgeResource = new MSBadge()
                    {
                        Person = new Person()
                        {
                            Id = reader.GetInt32(personIdIndex),
                            LastName = reader.GetString(lastNameIndex),
                            FirstName = reader.GetString(firstNameIndex)
                        },
                        BadgeStartDate = reader.IsDBNull(badgeStartDateIndex) ? null : (DateTime?)reader.GetDateTime(badgeStartDateIndex),//reader.GetDateTime(badgeStartDateIndex),
                        BadgeEndDate = reader.IsDBNull(badgeEndDateIndex) ? null : (DateTime?)reader.GetDateTime(badgeEndDateIndex),
                        Project = new Project()
                        {
                            Id = reader.GetInt32(projectIdIndex),
                            Name = reader.GetString(projectNameIndex),
                            ProjectNumber = reader.GetString(projectNumberIndex)
                        }
                    };
                    result.Add(badgeResource);
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

