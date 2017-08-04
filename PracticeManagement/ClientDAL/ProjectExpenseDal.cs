using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using DataAccess.Generic;
using DataAccess.Readers;
using DataTransferObjects;
using System.Collections.Generic;
using DataAccess.Other;
using System;

namespace DataAccess
{
    public class ProjectExpenseDal : DalBase<ProjectExpense, ProjectExpenseReader>
    {
        #region Overrides of DalBase<ProjectExpense>

        #region Procedure names

        protected override string GetByIdProcedure
        {
            get { return Constants.ProcedureNames.ProjectExpenses.GetById; }
        }

        protected override string AddProcedure
        {
            get { return Constants.ProcedureNames.ProjectExpenses.Insert; }
        }

        protected override string UpdateProcedure
        {
            get { return Constants.ProcedureNames.ProjectExpenses.Update; }
        }

        protected override string RemoveProcedure
        {
            get { return Constants.ProcedureNames.ProjectExpenses.Delete; }
        }

        #endregion Procedure names

        #region Base initializers

        protected override SqlParameter InitAddCommand(ProjectExpense entity, SqlCommand command)
        {
            InitPropertiesNoId(command, entity);
            command.Parameters.AddWithValue(Constants.ParameterNames.ProjectId, entity.ProjectId);
            command.Parameters.AddWithValue(Constants.ParameterNames.MilestoneId, entity.Milestone.Id);

            var outParam =
                new SqlParameter(Constants.ParameterNames.ExpenseId, SqlDbType.Int) { Direction = ParameterDirection.Output };

            command.Parameters.Add(outParam);

            return outParam;
        }

        protected override void InitRemoveCommand(ProjectExpense entity, SqlCommand command)
        {
            InitSingleId(command, entity);
        }

        protected override void InitUpdateCommand(ProjectExpense entity, SqlCommand command)
        {
            InitSingleId(command, entity);
            InitPropertiesNoId(command, entity);
        }

        protected override void InitGetById(ProjectExpense entity, SqlCommand command)
        {
            InitSingleId(command, entity);
        }

        protected override EntityReaderBase<ProjectExpense> InitEntityReader(DbDataReader reader)
        {
            return new ProjectExpenseReader(reader);
        }

        #endregion Base initializers

        #region Derived initializers

        private static void InitPropertiesNoId(SqlCommand command, ProjectExpense entity)
        {
            command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseName, entity.Name);
            command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseAmount, entity.Amount == null ? DBNull.Value : (object)entity.Amount.Value);
            command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseReimbursement, entity.Reimbursement);
            command.Parameters.AddWithValue(Constants.ParameterNames.StartDate, entity.StartDate);
            command.Parameters.AddWithValue(Constants.ParameterNames.EndDate, entity.EndDate);
            command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseTypeId, entity.Type.Id);
            command.Parameters.AddWithValue(Constants.ParameterNames.ExpectedAmount, entity.ExpectedAmount);
        }

        private static void InitSingleId(SqlCommand command, IIdNameObject entity)
        {
            command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseId, entity.Id.Value);
        }

        #endregion Derived initializers

        #region Custom data access

        public ProjectExpense[] GetForMilestone(ProjectExpense projectExpense)
        {
            return ExecuteReader(
                        projectExpense,
                        Constants.ProcedureNames.ProjectExpenses.GetAllForMilestone,
                        GetForMilestoneInitializer);
        }

        public ProjectExpense[] GetForProject(ProjectExpense projectExpense)
        {
            return ExecuteReader(
                        projectExpense,
                        Constants.ProcedureNames.ProjectExpenses.GetAllForProject,
                        GetForProjectInitializer);
        }

        private static void GetForMilestoneInitializer(ProjectExpense projectExpense, SqlCommand sqlCommand)
        {
            sqlCommand.Parameters.AddWithValue(Constants.ParameterNames.MilestoneId, projectExpense.Milestone.Id);
        }

        private static void GetForProjectInitializer(ProjectExpense projectExpense, SqlCommand sqlCommand)
        {
            sqlCommand.Parameters.AddWithValue(Constants.ParameterNames.ProjectId, projectExpense.ProjectId);
        }

        public static List<ExpenseType> GetAllExpenseTypesList()
        {
            var result = new List<ExpenseType>();
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.ProjectExpenses.GetAllExpenseTypes, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ReadExpenseTypes(reader, result);
                    return result;
                }
            }
        }

        private static void ReadExpenseTypes(SqlDataReader reader, List<ExpenseType> result)
        {
            try
            {
                int idIndex = reader.GetOrdinal(Constants.ColumnNames.Id);
                int nameIndex = reader.GetOrdinal(Constants.ColumnNames.Name);
                if (!reader.HasRows) return;
                while (reader.Read())
                {
                    var expenseType = new ExpenseType()
                    {
                        Id = reader.GetInt32(idIndex),
                        Name = reader.GetString(nameIndex),
                    };
                    result.Add(expenseType);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertProjectMonthlyExpenses(int expenseId, List<PeriodicalExpense> monthlyExpenses)
        {
            DeleteProjectMonthlyExpenses(expenseId);
            if (monthlyExpenses != null && monthlyExpenses.Count > 0)
            {
                foreach (var monthlyExpense in monthlyExpenses)
                {
                    using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
                    using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.ProjectExpenses.InsertProjectMonthlyExpenses, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = connection.ConnectionTimeout;

                        command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseId, expenseId);
                        command.Parameters.AddWithValue(Constants.ParameterNames.StartDate, monthlyExpense.StartDate);
                        command.Parameters.AddWithValue(Constants.ParameterNames.EndDate, monthlyExpense.EndDate);
                        command.Parameters.AddWithValue(Constants.ParameterNames.EstimatedAmount, monthlyExpense.EstimatedExpense);
                        command.Parameters.AddWithValue(Constants.ParameterNames.ActualAmount, monthlyExpense.ActualExpense == null ? DBNull.Value : (object)monthlyExpense.ActualExpense.Value);

                        connection.Open();
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                        connection.Close();
                    }
                }
            }
        }

        public static void DeleteProjectMonthlyExpenses(int expenseId)
        {

            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.ProjectExpenses.DeleteProjectMonthlyExpenses, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseId, expenseId);

                connection.Open();
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);

                }
                connection.Close();
            }
        }




        public static void UpdateProjectMonthlyExpenses(int expenseId, List<PeriodicalExpense> monthlyExpenses)
        {
            if (monthlyExpenses != null && monthlyExpenses.Count > 0)
            {
                foreach (var monthlyExpense in monthlyExpenses)
                {
                    using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
                    using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.ProjectExpenses.UpdateProjectMonthlyExpenses, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = connection.ConnectionTimeout;

                        command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseId, expenseId);
                        command.Parameters.AddWithValue(Constants.ParameterNames.Id, monthlyExpense.Id);
                        command.Parameters.AddWithValue(Constants.ParameterNames.StartDate, monthlyExpense.StartDate);
                        command.Parameters.AddWithValue(Constants.ParameterNames.EndDate, monthlyExpense.EndDate);
                        command.Parameters.AddWithValue(Constants.ParameterNames.EstimatedAmount, monthlyExpense.EstimatedExpense);
                        command.Parameters.AddWithValue(Constants.ParameterNames.ActualAmount, monthlyExpense.ActualExpense == null ? DBNull.Value : (object)monthlyExpense.ActualExpense.Value);

                        connection.Open();
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                        connection.Close();
                    }
                }

                using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
                using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.ProjectExpenses.UpdateProjectExpenseActualAmount, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseId, expenseId);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    connection.Close();
                }
            }
        }

        public static List<PeriodicalExpense> GetMonthlyExpensesForProjectExpense(int expenseId)
        {
            var result = new List<PeriodicalExpense>();
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.ProjectExpenses.GetMonthlyExpensesByExpenseId, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ExpenseId, expenseId);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ReadMonthlyExpenses(reader, result);
                    return result;
                }
            }
        }

        private static void ReadMonthlyExpenses(SqlDataReader reader, List<PeriodicalExpense> result)
        {
            try
            {
                if (!reader.HasRows) return;
                int idIndex = reader.GetOrdinal(Constants.ColumnNames.Id);
                int expenseIdIndex = reader.GetOrdinal(Constants.ColumnNames.ExpenseId);
                int startDateIndex = reader.GetOrdinal(Constants.ColumnNames.StartDate);
                int endDateIndex = reader.GetOrdinal(Constants.ColumnNames.EndDate);
                int estimatedAmountIndex = reader.GetOrdinal(Constants.ColumnNames.EstimatedAmount);
                int actualAmountIndex = reader.GetOrdinal(Constants.ColumnNames.ActualAmount);

                while (reader.Read())
                {
                    var monthExpense = new PeriodicalExpense()
                    {
                        Id = reader.GetInt32(idIndex),
                        ProjectExpenseId = reader.GetInt32(expenseIdIndex),
                        StartDate = reader.GetDateTime(startDateIndex),
                        EndDate = reader.GetDateTime(endDateIndex),
                        EstimatedExpense = reader.GetDecimal(estimatedAmountIndex),
                        ActualExpense = reader.IsDBNull(actualAmountIndex) ? null : (decimal?)reader.GetDecimal(actualAmountIndex)
                    };
                    result.Add(monthExpense);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Custom data access

        #endregion Overrides of DalBase<ProjectExpense>
    }
}

