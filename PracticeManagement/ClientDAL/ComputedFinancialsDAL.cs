﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using DataAccess.Other;
using DataTransferObjects;
using DataTransferObjects.Financials;

namespace DataAccess
{
    /// <summary>
    /// Provides an access to the computed financials
    /// </summary>
    public static class ComputedFinancialsDAL
    {
        #region Methods

        /// <summary>
        /// Retrives the list of the computed financials for the project and the specified period
        /// grouped by months.
        /// </summary>
        /// <param name="projects">Projects list</param>
        /// <param name="startDate">A period start.</param>
        /// <param name="endDate">A period end.</param>
        /// <returns>The list of the <see cref="ComputedFinancials"/> objects.</returns>
        public static void LoadFinancialsPeriodForProjects(
            List<Project> projects, DateTime startDate, DateTime endDate, bool useActuals)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.FinancialsListByProjectPeriod, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectIdParam, DataTransferObjects.Utils.Generic.IdsListToString(projects));
                command.Parameters.AddWithValue(Constants.ParameterNames.StartDateParam, startDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.EndDateParam, endDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.UseActualsParam, useActuals);

                connection.Open();

                projects.ForEach(delegate(Project project)
                                     {
                                         if (project.ProjectedFinancialsByMonth == null)
                                             project.ProjectedFinancialsByMonth =
                                                 new Dictionary<DateTime, ComputedFinancials>();
                                     });

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ReadMonthlyFinancialsForListOfProjects(reader, projects);
                }
            }
        }

        /// <summary>
        /// Retrives the list of the computed financials for the project and the specified period.
        /// </summary>
        /// <param name="projects">Projects list</param>
        /// <param name="startDate">A period start.</param>
        /// <param name="endDate">A period end.</param>
        /// <returns>The <see cref="ComputedFinancials"/> object if found and null otherwise.</returns>
        public static void LoadTotalFinancialsPeriodForProjects(List<Project> projects, DateTime? startDate, DateTime? endDate, bool useActuals)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.FinancialsListByProjectPeriodTotal, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectIdParam, DataTransferObjects.Utils.Generic.IdsListToString(projects));
                if (startDate.HasValue && endDate.HasValue)
                {
                    command.Parameters.AddWithValue(Constants.ParameterNames.StartDateParam, startDate);
                    command.Parameters.AddWithValue(Constants.ParameterNames.EndDateParam, endDate);
                }
                command.Parameters.AddWithValue(Constants.ParameterNames.UseActualsParam, useActuals);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ReadTotalFinancialsForListOfProjects(reader, projects);
                }
            }
        }

        /// <summary>
        /// Retrives the list of the computed financials for the project and the specified period From Cache table.
        /// grouped by months.
        /// </summary>
        /// <param name="projects">Projects list</param>
        /// <param name="startDate">A period start.</param>
        /// <param name="endDate">A period end.</param>
        /// <returns>The list of the <see cref="ComputedFinancials"/> objects.</returns>
        public static void LoadFinancialsPeriodForProjectsFromCache(List<Project> projects, DateTime startDate, DateTime endDate, bool isAttainmentReport)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.GetProjectSummaryCacheValue, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectIdParam, DataTransferObjects.Utils.Generic.IdsListToString(projects));
                command.Parameters.AddWithValue(Constants.ParameterNames.StartDateParam, startDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.EndDateParam, endDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsMonthlyReportParam, true);
                command.Parameters.AddWithValue(Constants.ParameterNames.IsAttainmentReportParam, isAttainmentReport);

                connection.Open();

                if (!isAttainmentReport)
                {
                    projects.ForEach(delegate(Project project)
                    {
                        if (project.ProjectedFinancialsByMonth == null)
                            project.ProjectedFinancialsByMonth =
                                new Dictionary<DateTime, ComputedFinancials>();
                    });

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ReadMonthlyFinancialsForListOfProjects(reader, projects);
                    }
                }
                else
                {
                    projects.ForEach(delegate(Project project)
                    {
                        if (project.ProjectedFinancialsByRange == null)
                            project.ProjectedFinancialsByRange =
                                new Dictionary<RangeType, ComputedFinancials>();
                    });

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ReportDAL.ReadMonthlyFinancialsForListOfProjects(reader, projects);
                    }
                }
            }
        }

        /// <summary>
        /// Retrives the list of the computed financials for the project and the specified period From Cache table.
        /// </summary>
        /// <param name="projects">Projects list</param>
        /// <param name="startDate">A period start.</param>
        /// <param name="endDate">A period end.</param>
        /// <returns>The <see cref="ComputedFinancials"/> object if found and null otherwise.</returns>
        public static void LoadTotalFinancialsPeriodForProjectsFromCache(List<Project> projects, DateTime? startDate, DateTime? endDate)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.GetProjectSummaryCacheValue, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectIdParam, DataTransferObjects.Utils.Generic.IdsListToString(projects));
                if (startDate.HasValue && endDate.HasValue)
                {
                    command.Parameters.AddWithValue(Constants.ParameterNames.StartDateParam, startDate);
                    command.Parameters.AddWithValue(Constants.ParameterNames.EndDateParam, endDate);
                }
                command.Parameters.AddWithValue(Constants.ParameterNames.IsMonthlyReportParam, false);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ReadTotalFinancialsForListOfProjects(reader, projects);
                }
            }
        }

        /// <summary>
        /// Retrieves the list of the computed financials for the milestone person
        /// </summary>
        /// <returns>The <see cref="ComputedFinancials"/> object if found and null otherwise.</returns>
        public static MilestonePersonComputedFinancials CalculateMilestonePersonFinancials(int milestonePersonId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.CalculateMilestonePersonFinancials, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.MilestonePersonId, milestonePersonId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                    return ReadMilestonePersonComputedFinancials(reader);
            }
        }

        /// <summary>
        /// Retrives the list of the computed financials for the milestone persons association
        /// and the specified period grouped by months.
        /// </summary>
        /// <param name="milestoneId">An ID of the milestone to retrive the data for.</param>
        /// <param name="milestonePersons">Persons to init financials with</param>
        /// <returns>The list of the <see cref="ComputedFinancials"/> objects.</returns>
        public static void FinancialsGetByMilestonePersonsMonthly(int milestoneId, List<MilestonePerson> milestonePersons)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.FinancialsGetByMilestonePersonsMonthly, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneIdParam, milestoneId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    ReadFinancialsForPersons(reader, milestonePersons);
                }
            }
        }

        /// <summary>
        /// Retrives the list of the total computed financials for the milestone persons association
        /// and the specified period grouped by months.
        /// </summary>
        /// <param name="milestoneId">An ID of the milestone to retrive the data for.</param>
        /// <param name="milestonePersons">Persons to init financials with</param>
        /// <returns>The list of the <see cref="ComputedFinancials"/> objects.</returns>
        public static void FinancialsGetByMilestonePersonsTotal(int milestoneId, List<MilestonePerson> milestonePersons)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.FinancialsGetByMilestonePersonsTotal, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneIdParam, milestoneId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    ReadTotalFinancialsForPersons(reader, milestonePersons);
                }
            }
        }

        /// <summary>
        /// Retrives the financials for a specified project.
        /// </summary>
        /// <param name="projectId">An ID of the project to retrive the data for.</param>
        /// <returns>The <see cref="ComputedFinancials"/> object.</returns>
        public static ComputedFinancials FinancialsGetByProject(int projectId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.FinancialsGetByProject, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectIdParam, projectId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    var result = new Dictionary<DateTime, ComputedFinancials>(1);
                    ReadFinancials(reader, result);
                    foreach (var pair in result)
                    {
                        return pair.Value;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Retrives the list of the computed financials for the milestone.
        /// </summary>
        /// <param name="milestoneId">An ID of the milestone to retrive the data for.</param>
        /// <returns>The <see cref="ComputedFinancials"/> object.</returns>
        public static ComputedFinancials FinancialsGetByMilestone(int milestoneId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.FinancialsGetByMilestone, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneIdParam, milestoneId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    var result =
                        new Dictionary<DateTime, ComputedFinancials>(1);
                    ReadFinancials(reader, result);
                    foreach (var pair in result)
                    {
                        return pair.Value;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Retrives the list of the computed financials for the milestone-person association.
        /// </summary>
        /// <param name="milestoneId">An ID of the milestone to retrive the data for.</param>
        /// <param name="personId">An ID of the person to retrive the data for.</param>
        /// <returns>The <see cref="ComputedFinancials"/> object.</returns>
        public static ComputedFinancials FinancialsGetByMilestonePerson(int milestoneId, int personId, SqlConnection connection = null, SqlTransaction activeTransaction = null)
        {
            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }
            using (var command = new SqlCommand(Constants.ProcedureNames.ComputedFinancials.FinancialsGetByMilestonePerson, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneIdParam, milestoneId);
                command.Parameters.AddWithValue(Constants.ParameterNames.PersonIdParam, personId);

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                if (activeTransaction != null)
                {
                    command.Transaction = activeTransaction;
                }

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    var result =
                        new Dictionary<DateTime, ComputedFinancials>(1);
                    ReadFinancials(reader, result);
                    foreach (var pair in result)
                    {
                        return pair.Value;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Retrives the data for the person stats report.
        /// </summary>
        /// <param name="startDate">The period start.</param>
        /// <param name="endDate">The period end.</param>
        /// <param name="salespersonId">Determines an ID of the salesperson to filter the list for.</param>
        /// <param name="practiceManagerId">Determines an ID of the practice manager to filter the list for.</param>
        /// <returns>The list of the <see cref="PersonStats"/> objects.</returns>
        public static List<PersonStats> PersonStatsByDateRange(
            DateTime startDate,
            DateTime endDate,
            int? salespersonId,
            int? practiceManagerId,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showExperimental,
            bool showProposed,
            bool showInternal,
            bool showInactive,
            bool useActuals)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.PersonStatsByDate, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.StartDateParam, startDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.EndDateParam, endDate);
                command.Parameters.AddWithValue(Constants.ParameterNames.SalespersonIdParam,
                                                salespersonId.HasValue ? (object)salespersonId.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.PracticeManagerIdParam,
                                                practiceManagerId.HasValue
                                                    ? (object)practiceManagerId.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowProjectedParam, showProjected);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowCompletedParam, showCompleted);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowActiveParam, showActive);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowInternalParam, showInternal);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowExperimentalParam, showExperimental);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowProposedParam, showProposed);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowInactiveParam, showInactive);
                command.Parameters.AddWithValue(Constants.ParameterNames.UseActualsParam, useActuals);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var result = new List<PersonStats>();
                    ReadPersonStats(reader, result);
                    return result;
                }
            }
        }

        private static void ReadFinancialsForPersons(SqlDataReader reader, List<MilestonePerson> milestonePersons)
        {
            foreach (var persFin in ReadFinancialsByOneForPerson(reader))
            {
                var fin = persFin;
                var foundPersons = milestonePersons.FindAll(milestonePerson => milestonePerson.Person.Id.Value == fin.Key.Person.Id.Value
                                                                                && milestonePerson.Entries[0].Id == fin.Key.Entries[0].Id

                                                                                );
                foreach (var found in foundPersons)
                {
                    if (found.Person.ProjectedFinancialsByMonth == null)
                        found.Person.ProjectedFinancialsByMonth = new Dictionary<DateTime, ComputedFinancials>();

                    found.Person.ProjectedFinancialsByMonth.Add(fin.Value.FinancialDate.Value, fin.Value);
                }
            }
        }

        private static void ReadTotalFinancialsForPersons(SqlDataReader reader, List<MilestonePerson> milestonePersons)
        {
            foreach (var persFin in ReadFinancialsByOneForPerson(reader))
            {
                var fin = persFin;
                if (milestonePersons.Any(milestonePerson => milestonePerson.Person.Id.Value == fin.Key.Person.Id.Value
                                                            && milestonePerson.Entries[0].Id == fin.Key.Entries[0].Id

                                        )
                    )
                    milestonePersons.Find(milestonePerson => milestonePerson.Person.Id.Value == fin.Key.Person.Id.Value
                                                                && milestonePerson.Entries[0].Id == fin.Key.Entries[0].Id

                                            ).Entries[0].ComputedFinancials = fin.Value;
            }
        }

        private static void ReadMonthlyFinancialsForListOfProjects(DbDataReader reader, List<Project> projects)
        {
            if (!reader.HasRows) return;
            int financialDateIndex = reader.GetOrdinal(Constants.ColumnNames.FinancialDateColumn);
            int revenueIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueColumn);
            int revenueNetIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueNetColumn);
            int cogsIndex = reader.GetOrdinal(Constants.ColumnNames.CogsColumn);
            int grossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.GrossMarginColumn);
            int hoursIndex = reader.GetOrdinal(Constants.ColumnNames.HoursColumn);
            int projectIdIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectId);
            int actualRevenueIndex = -1;
            int actualGrossMarginIndex = -1;
            int previousMonthsActualRevenueIndex = -1;
            int previousMonthsActualGrossMarginIndex = -1;

            try
            {
                actualRevenueIndex = reader.GetOrdinal(Constants.ColumnNames.ActualRevenue);
                actualGrossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.ActualGrossMargin);
            }
            catch { }
            try
            {
                previousMonthsActualRevenueIndex = reader.GetOrdinal(Constants.ColumnNames.PreviousMonthActualRevenue);
                previousMonthsActualGrossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.PreviousMonthActualGrossMargin);
            }
            catch { }


            while (reader.Read())
            {
                var project = new Project { Id = reader.GetInt32(projectIdIndex) };
                var financials =
                    ReadComputedFinancials(
                        reader,
                        financialDateIndex,
                        revenueIndex,
                        revenueNetIndex,
                        cogsIndex,
                        grossMarginIndex,
                        hoursIndex,
                        -1,
                        -1,
                        actualRevenueIndex,
                        actualGrossMarginIndex,
                        previousMonthsActualRevenueIndex,
                        previousMonthsActualGrossMarginIndex);

                var i = projects.IndexOf(project);
                projects[i].ProjectedFinancialsByMonth.Add(financials.FinancialDate.Value, financials);
            }
        }

        private static void ReadTotalFinancialsForListOfProjects(DbDataReader reader, List<Project> projects)
        {
            if (!reader.HasRows) return;
            int financialDateIndex = reader.GetOrdinal(Constants.ColumnNames.FinancialDateColumn);
            int revenueIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueColumn);
            int revenueNetIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueNetColumn);
            int cogsIndex = reader.GetOrdinal(Constants.ColumnNames.CogsColumn);
            int grossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.GrossMarginColumn);
            int hoursIndex = reader.GetOrdinal(Constants.ColumnNames.HoursColumn);
            int projectIdIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectId);
            int actualRevenueIndex = -1;
            int actualGrossMarginIndex = -1;
            int previousMonthsActualRevenueIndex = -1;
            int previousMonthsActualGrossMarginIndex = -1;

            try
            {
                actualRevenueIndex = reader.GetOrdinal(Constants.ColumnNames.ActualRevenue);
                actualGrossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.ActualGrossMargin);
            }
            catch { }
            try
            {
                previousMonthsActualRevenueIndex = reader.GetOrdinal(Constants.ColumnNames.PreviousMonthActualRevenue);
                previousMonthsActualGrossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.PreviousMonthActualGrossMargin);
            }
            catch { }

            while (reader.Read())
            {
                var project = new Project { Id = reader.GetInt32(projectIdIndex) };
                var financials =
                    ReadComputedFinancials(
                        reader,
                        financialDateIndex,
                        revenueIndex,
                        revenueNetIndex,
                        cogsIndex,
                        grossMarginIndex,
                        hoursIndex,
                        -1,
                        -1,
                        actualRevenueIndex,
                        actualGrossMarginIndex,
                        previousMonthsActualRevenueIndex,
                        previousMonthsActualGrossMarginIndex);

                var i = projects.IndexOf(project);
                projects[i].ComputedFinancials = financials;
            }
        }

        private static void ReadFinancials(DbDataReader reader, Dictionary<DateTime, ComputedFinancials> result)
        {
            foreach (var computedFinancialse in ReadFinancialsByOne(reader))
                result.Add(computedFinancialse.FinancialDate.Value, computedFinancialse);
        }

        private static MilestonePersonComputedFinancials ReadMilestonePersonComputedFinancials(SqlDataReader reader)
        {
            var hoursInPeriodIndex = reader.GetOrdinal(Constants.ColumnNames.HoursInPeriod);
            var projectDiscountIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectDiscount);
            var grossHourlyBillRateIndex = reader.GetOrdinal(Constants.ColumnNames.GrossHourlyBillRate);
            var loadedHourlyPayRateIndex = reader.GetOrdinal(Constants.ColumnNames.LoadedHourlyPayRate);

            if (reader.HasRows)
                while (reader.Read())
                    return new MilestonePersonComputedFinancials
                               {
                                   ProjectDiscount = GetDecimalValueFromReader(reader, projectDiscountIndex),
                                   GrossHourlyBillRate = GetDecimalValueFromReader(reader, grossHourlyBillRateIndex),
                                   HoursInPeriod = GetDecimalValueFromReader(reader, hoursInPeriodIndex),
                                   LoadedHourlyPayRate = GetDecimalValueFromReader(reader, loadedHourlyPayRateIndex)
                               };

            return null;
        }

        private static decimal GetDecimalValueFromReader(SqlDataReader reader, int index)
        {
            try
            {
                return reader.GetDecimal(index);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private static IEnumerable<KeyValuePair<MilestonePerson, ComputedFinancials>> ReadFinancialsByOneForPerson(DbDataReader reader)
        {
            if (!reader.HasRows) yield break;
            var milestonePersonEntryIdIndex = reader.GetOrdinal(Constants.ColumnNames.EntryId);
            int financialDateIndex = reader.GetOrdinal(Constants.ColumnNames.FinancialDateColumn);
            int revenueIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueColumn);
            int revenueNetIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueNetColumn);
            int cogsIndex = reader.GetOrdinal(Constants.ColumnNames.CogsColumn);
            int grossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.GrossMarginColumn);
            int hoursIndex = reader.GetOrdinal(Constants.ColumnNames.HoursColumn);
            int personIdIndex =
                reader.GetOrdinal(Constants.ColumnNames.PersonId);
            int startDateIndex = reader.GetOrdinal(Constants.ColumnNames.StartDate);
            int endDateIndex = reader.GetOrdinal(Constants.ColumnNames.EndDate);
            int expenseIndex;
            int expenseReimbIndex;

            try
            {
                expenseIndex = reader.GetOrdinal(Constants.ColumnNames.Expense);
                expenseReimbIndex = reader.GetOrdinal(Constants.ColumnNames.ReimbursedExpense);
            }
            catch (IndexOutOfRangeException)
            {
                expenseIndex = -1;
                expenseReimbIndex = -1;
            }

            while (reader.Read())
            {
                yield return
                    new KeyValuePair<MilestonePerson, ComputedFinancials>(
                        new MilestonePerson()
                            {
                                Person = new Person() { Id = reader.GetInt32(personIdIndex) },
                                Entries = new List<MilestonePersonEntry>(1)
                                    {
                                        new MilestonePersonEntry{
                                            Id = reader.GetInt32(milestonePersonEntryIdIndex),
                                            StartDate = reader.GetDateTime(startDateIndex),
                                            EndDate =  reader.GetDateTime(endDateIndex)
                                        }
                                    }
                            },
                        ReadComputedFinancials(
                            reader,
                            financialDateIndex,
                            revenueIndex,
                            revenueNetIndex,
                            cogsIndex,
                            grossMarginIndex,
                            hoursIndex,
                            expenseIndex,
                            expenseReimbIndex));
            }
        }

        private static IEnumerable<ComputedFinancials> ReadFinancialsByOne(DbDataReader reader)
        {
            if (!reader.HasRows) yield break;
            int financialDateIndex = reader.GetOrdinal(Constants.ColumnNames.FinancialDateColumn);
            int revenueIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueColumn);
            int revenueNetIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueNetColumn);
            int cogsIndex = reader.GetOrdinal(Constants.ColumnNames.CogsColumn);
            int grossMarginIndex = reader.GetOrdinal(Constants.ColumnNames.GrossMarginColumn);
            int hoursIndex = reader.GetOrdinal(Constants.ColumnNames.HoursColumn);

            int expenseIndex;
            int expenseReimbIndex;

            try
            {
                expenseIndex = reader.GetOrdinal(Constants.ColumnNames.Expense);
                expenseReimbIndex = reader.GetOrdinal(Constants.ColumnNames.ReimbursedExpense);
            }
            catch (IndexOutOfRangeException)
            {
                expenseIndex = -1;
                expenseReimbIndex = -1;
            }

            while (reader.Read())
            {
                yield return
                    ReadComputedFinancials(
                        reader,
                        financialDateIndex,
                        revenueIndex,
                        revenueNetIndex,
                        cogsIndex,
                        grossMarginIndex,
                        hoursIndex,
                        expenseIndex,
                        expenseReimbIndex);
            }
        }

        private static ComputedFinancials ReadComputedFinancials(
            DbDataReader reader,
            int financialDateIndex,
            int revenueIndex,
            int revenueNetIndex,
            int cogsIndex,
            int grossMarginIndex,
            int hoursIndex,
            int expenseIndex,
            int expenseReimbIndex,
            int actualRevenueIndex = -1,
            int actualGrossMarginIndex = -1,
            int previousRevenueIndex = -1,
            int previousGrossMarginIndex = -1)
        {
            return new ComputedFinancials
                       {
                           FinancialDate = reader.IsDBNull(financialDateIndex) ? (DateTime?)null : reader.GetDateTime(financialDateIndex),
                           Revenue = reader.GetDecimal(revenueIndex),
                           RevenueNet = reader.GetDecimal(revenueNetIndex),
                           Cogs = reader.GetDecimal(cogsIndex),
                           GrossMargin = reader.GetDecimal(grossMarginIndex),
                           HoursBilled = reader.GetDecimal(hoursIndex),
                           Expenses = expenseIndex < 0 ? 0 : reader.GetDecimal(expenseIndex),
                           ReimbursedExpenses = expenseReimbIndex < 0 ? 0 : reader.GetDecimal(expenseReimbIndex),
                           ActualRevenue = actualRevenueIndex > -1 && !reader.IsDBNull(actualRevenueIndex) ? reader.GetDecimal(actualRevenueIndex) : 0M,
                           ActualGrossMargin = actualGrossMarginIndex > -1 && !reader.IsDBNull(actualGrossMarginIndex) ? reader.GetDecimal(actualGrossMarginIndex) : 0M,
                           PreviousMonthsActualRevenueValue = previousRevenueIndex > -1 && !reader.IsDBNull(previousRevenueIndex) ? reader.GetDecimal(previousRevenueIndex) : 0M,
                           PreviousMonthsActualMarginValue = previousGrossMarginIndex > -1 && !reader.IsDBNull(previousGrossMarginIndex) ? reader.GetDecimal(previousGrossMarginIndex) : 0M
                       };
        }

        private static void ReadPersonStats(DbDataReader reader, List<PersonStats> result)
        {
            if (!reader.HasRows) return;
            int dateIndex = reader.GetOrdinal(Constants.ColumnNames.DateColumn);
            int revenueIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueColumn);
            int virtualConsultantsIndex = reader.GetOrdinal(Constants.ColumnNames.VirtualConsultantsColumn);
            int employeesNumberIndex = reader.GetOrdinal(Constants.ColumnNames.EmployeesNumberColumn);
            int consultantsNumberIndex = reader.GetOrdinal(Constants.ColumnNames.ConsultantsNumberColumn);

            while (reader.Read())
            {
                var stats = new PersonStats
                    {
                        Date = reader.GetDateTime(dateIndex),
                        Revenue = reader.GetDecimal(revenueIndex),
                        VirtualConsultants = reader.GetDecimal(virtualConsultantsIndex),
                        EmployeesCount = reader.GetInt32(employeesNumberIndex),
                        ConsultantsCount = reader.GetInt32(consultantsNumberIndex)
                    };

                result.Add(stats);
            }
        }

        public static ComputedFinancials FinancialsGetByMilestonePersonEntry(int mpeId, SqlConnection connection = null, SqlTransaction activeTransaction = null)
        {
            if (connection == null)
            {
                connection = new SqlConnection(DataSourceHelper.DataConnection);
            }

            using (var command = new SqlCommand(
                Constants.ProcedureNames.ComputedFinancials.FinancialsGetByMilestonePersonEntry, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.IdParam, mpeId);

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (activeTransaction != null)
                {
                    command.Transaction = activeTransaction;
                }

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    var result =
                        new Dictionary<DateTime, ComputedFinancials>(1);
                    ReadFinancials(reader, result);
                    foreach (var pair in result)
                    {
                        return pair.Value;
                    }
                    return null;
                }
            }
        }

        #endregion Methods
    }
}

