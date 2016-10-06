using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using DataAccess.Other;
using DataTransferObjects;
using DataTransferObjects.ContextObjects;

namespace DataAccess
{
    /// <summary>
    /// 	Provides an access to the Opportunity database table.
    /// </summary>
    public static class OpportunityDAL
    {
        public const string GetActiveOpportunitiesByOwnerIdProcedure = "dbo.GetActiveOpportunitiesByOwnerId";

        #region Methods

        /// <summary>
        /// 	Retrieves a list of the opportunities by the specified conditions.
        /// </summary>
        /// <param name = "activeOnly">Determines whether only active opportunities must are retrieved.</param>
        /// <param name = "looked">Determines a text to be searched within the opportunity name.</param>
        /// <param name = "clientId">Determines a client to retrieve the opportunities for.</param>
        /// <param name = "salespersonId">Determines a salesperson to retrieve the opportunities for.</param>
        /// <param name = "currentId"></param>
        /// <returns>A list of the <see cref = "Opportunity" /> objects.</returns>
        public static List<Opportunity> OpportunityListAll(OpportunityListContext context)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityListAll, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ActiveOnlyParam, context.ActiveClientsOnly);
                command.Parameters.AddWithValue(Constants.ParameterNames.LookedParam,
                                                !string.IsNullOrEmpty(context.SearchText) ? (object)context.SearchText : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ClientIdParam,
                                                context.ClientId.HasValue ? (object)context.ClientId.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.SalespersonIdParam,
                                                context.SalespersonId.HasValue ? (object)context.SalespersonId.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.CurrentId,
                                                context.CurrentId.HasValue ? (object)context.CurrentId.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.TargetPerson,
                                                (object)context.TargetPersonId ?? DBNull.Value);
                if (context.IsDiscussionReview2)
                {
                    command.Parameters.AddWithValue(Constants.ParameterNames.IsDiscussionReview2, context.IsDiscussionReview2);
                }

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<Opportunity>();
                    ReadOpportunities(reader, result);
                    return result;
                }
            }
        }

        public static List<OpportunityPriority> GetOpportunityPrioritiesListAll()
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityPrioritiesListAll, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<OpportunityPriority>();
                    ReadOpportunityPriorityListAll(reader, result);
                    return result;
                }
            }
        }

        private static void ReadOpportunityPriorityListAll(DbDataReader reader, List<OpportunityPriority> result)
        {
            if (!reader.HasRows) return;
            var priorityIndex = reader.GetOrdinal(Constants.ColumnNames.PriorityColumn);
            var descriptionIdIndex = reader.GetOrdinal(Constants.ColumnNames.DescriptionColumn);
            var displayNameIdIndex = reader.GetOrdinal(Constants.ColumnNames.DisplayNameColumn);
            var priorityIdIndex = reader.GetOrdinal(Constants.ColumnNames.Id);
            int inUseIndex;

            try
            {
                inUseIndex = reader.GetOrdinal(Constants.ColumnNames.InUse);
            }
            catch
            {
                inUseIndex = -1;
            }

            while (reader.Read())
            {
                // Reading the item
                var opportunityPriority =
                    new OpportunityPriority
                        {
                            Id = reader.GetInt32(priorityIdIndex),
                            Priority = reader.GetString(priorityIndex),
                            Description = reader.IsDBNull(descriptionIdIndex) ? null : reader.GetString(descriptionIdIndex),
                            DisplayName = reader.IsDBNull(displayNameIdIndex) ? null : reader.GetString(displayNameIdIndex)
                        };

                if (inUseIndex >= 0)
                {
                    try
                    {
                        opportunityPriority.InUse = reader.GetBoolean(inUseIndex);
                    }
                    catch
                    {
                    }
                }

                result.Add(opportunityPriority);
            }
        }

        /// <summary>
        /// 	Retruves an <see cref = "Opportunity" /> by a specified ID.
        /// </summary>
        /// <param name = "opportunityId">An ID of the record to be retrieved.</param>
        /// <returns>An <see cref = "Opportunity" /> object if found and null otherwise.</returns>
        public static Opportunity OpportunityGetById(int opportunityId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityGetById, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdParam, opportunityId);

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    var result = new List<Opportunity>(1);
                    ReadOpportunities(reader, result);
                    return result.Count > 0 ? result[0] : null;
                }
            }
        }

        /// <summary>
        /// 	Retrives <see cref = "Opportunity" /> data to be exported to excel.
        /// </summary>
        /// <returns>An <see cref = "Opportunity" /> object if found and null otherwise.</returns>
        public static DataSet OpportunityGetExcelSet()
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityGetExcelSet, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                connection.Open();
                var adapter = new SqlDataAdapter(command);
                var dataset = new DataSet();
                adapter.Fill(dataset, "excelDataTable");
                return dataset;
            }
        }

        /// <summary>
        /// 	Inserts a new <see cref = "Opportunity" /> into the database.
        /// </summary>
        /// <param name = "userName">The name of the current user.</param>
        /// <param name = "opportunity">The data to be inserted.</param>
        public static void OpportunityInsert(Opportunity opportunity, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityInsert, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.NameParam,
                                                !string.IsNullOrEmpty(opportunity.Name)
                                                    ? (object)opportunity.Name
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ClientIdParam,
                                                opportunity.Client != null && opportunity.Client.Id.HasValue
                                                    ? (object)opportunity.Client.Id.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.SalespersonIdParam,
                                                opportunity.Salesperson != null && opportunity.Salesperson.Id.HasValue
                                                    ? (object)opportunity.Salesperson.Id.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityStatusIdParam,
                                                opportunity.Status != null
                                                    ? (object)opportunity.Status.Id
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.PriorityIdParam,
                                                opportunity.Priority != null ? (object)opportunity.Priority.Id
                                                : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectedStartDateParam,
                                                opportunity.ProjectedStartDate.HasValue
                                                    ? (object)opportunity.ProjectedStartDate
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectedEndDateParam,
                                                opportunity.ProjectedEndDate.HasValue
                                                    ? (object)opportunity.ProjectedEndDate.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.DescriptionParam,
                                                !string.IsNullOrEmpty(opportunity.Description)
                                                    ? (object)opportunity.Description
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.PracticeIdParam,
                                                opportunity.Practice != null
                                                    ? (object)opportunity.Practice.Id
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.BuyerNameParam,
                                                !string.IsNullOrEmpty(opportunity.BuyerName)
                                                    ? (object)opportunity.BuyerName
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.PipelineParam,
                                                !string.IsNullOrEmpty(opportunity.Pipeline)
                                                    ? (object)opportunity.Pipeline
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ProposedParam,
                                                !string.IsNullOrEmpty(opportunity.Proposed)
                                                    ? (object)opportunity.Proposed
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.SendOutParam,
                                                !string.IsNullOrEmpty(opportunity.SendOut)
                                                    ? (object)opportunity.SendOut
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.UserLoginParam,
                                                !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectIdParam,
                                                opportunity.Project != null && opportunity.Project.Id.HasValue
                                                    ? (object)opportunity.Project.Id.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIndexParam,
                                                opportunity.OpportunityIndex.HasValue
                                                    ? (object)opportunity.OpportunityIndex.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.EstimatedRevenueParam,
                                                opportunity.EstimatedRevenue != null && opportunity.EstimatedRevenue.HasValue
                                                    ? (object)opportunity.EstimatedRevenue
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.CloseDateParam,
                                                 opportunity.CloseDate.HasValue
                                                    ? (object)opportunity.CloseDate
                                                    : DBNull.Value);

                command.Parameters.AddWithValue(
                    Constants.ParameterNames.OwnerId,
                    opportunity.Owner == null ? (object)DBNull.Value : opportunity.Owner.Id);

                command.Parameters.AddWithValue(Constants.ParameterNames.PricingListId,
                            opportunity.PricingList != null && opportunity.PricingList.PricingListId.HasValue ?
                            (object)opportunity.PricingList.PricingListId.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.BusinessTypeId,
                        ((int)opportunity.BusinessType) != 0 ? (object)((int)opportunity.BusinessType) : DBNull.Value);

                command.Parameters.AddWithValue(Constants.ParameterNames.GroupIdParam,
                opportunity.Group == null ? (object)DBNull.Value : opportunity.Group.Id);

                command.Parameters.AddWithValue(
                   Constants.ParameterNames.PersonIdListParam,
                   opportunity.ProposedPersonIdList ?? (object)DBNull.Value);

                command.Parameters.AddWithValue(
                    Constants.ParameterNames.StrawManListParam,
                    opportunity.StrawManList ?? (object)DBNull.Value);

                var idParam = new SqlParameter(Constants.ParameterNames.OpportunityIdParam, SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(idParam);

                try
                {
                    connection.Open();

                    SqlTransaction trn = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    command.Transaction = trn;

                    command.ExecuteNonQuery();

                    trn.Commit();

                    opportunity.Id = (int)idParam.Value;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 	Updates a new <see cref = "Opportunity" /> in the database.
        /// </summary>
        /// <param name = "userName">The name of the current user.</param>
        /// <param name = "opportunity">The data to be updated.</param>
        public static void OpportunityUpdate(Opportunity opportunity, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityUpdate, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.NameParam,
                                                !string.IsNullOrEmpty(opportunity.Name)
                                                    ? (object)opportunity.Name
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ClientIdParam,
                                                opportunity.Client != null && opportunity.Client.Id.HasValue
                                                    ? (object)opportunity.Client.Id.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.SalespersonIdParam,
                                                opportunity.Salesperson != null && opportunity.Salesperson.Id.HasValue
                                                    ? (object)opportunity.Salesperson.Id.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityStatusIdParam,
                                                opportunity.Status != null
                                                    ? (object)opportunity.Status.Id
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.PriorityIdParam,
                                                opportunity.Priority != null ? (object)opportunity.Priority.Id
                                                : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectedStartDateParam,
                                                opportunity.ProjectedStartDate.HasValue
                                                    ? (object)opportunity.ProjectedStartDate
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectedEndDateParam,
                                                opportunity.ProjectedEndDate.HasValue
                                                    ? (object)opportunity.ProjectedEndDate.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.DescriptionParam,
                                                !string.IsNullOrEmpty(opportunity.Description)
                                                    ? (object)opportunity.Description
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.PracticeIdParam,
                                                opportunity.Practice != null
                                                    ? (object)opportunity.Practice.Id
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.BuyerNameParam,
                                                !string.IsNullOrEmpty(opportunity.BuyerName)
                                                    ? (object)opportunity.BuyerName
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.PipelineParam,
                                                !string.IsNullOrEmpty(opportunity.Pipeline)
                                                    ? (object)opportunity.Pipeline
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ProposedParam,
                                                !string.IsNullOrEmpty(opportunity.Proposed)
                                                    ? (object)opportunity.Proposed
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.SendOutParam,
                                                !string.IsNullOrEmpty(opportunity.SendOut)
                                                    ? (object)opportunity.SendOut
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdParam,
                                                opportunity.Id.HasValue ? (object)opportunity.Id.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.UserLoginParam,
                                                !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectIdParam,
                                                opportunity.Project != null && opportunity.Project.Id.HasValue
                                                    ? (object)opportunity.Project.Id.Value
                                                    : DBNull.Value);

                command.Parameters.AddWithValue(Constants.ParameterNames.EstimatedRevenueParam,
                                                opportunity.EstimatedRevenue != null && opportunity.EstimatedRevenue.HasValue
                                                    ? (object)opportunity.EstimatedRevenue
                                                    : DBNull.Value);

                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIndexParam,
                                                opportunity.OpportunityIndex.HasValue
                                                    ? (object)opportunity.OpportunityIndex.Value
                                                    : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.CloseDateParam,
                                                 opportunity.CloseDate.HasValue
                                                    ? (object)opportunity.CloseDate
                                                    : DBNull.Value);

                command.Parameters.AddWithValue(
                    Constants.ParameterNames.OwnerId,
                    opportunity.Owner == null ? (object)DBNull.Value : opportunity.Owner.Id);

                command.Parameters.AddWithValue(
                    Constants.ParameterNames.GroupIdParam,
                    opportunity.Group == null ? (object)DBNull.Value : opportunity.Group.Id);

                command.Parameters.AddWithValue(Constants.ParameterNames.PricingListId,
                     opportunity.PricingList != null && opportunity.PricingList.PricingListId.HasValue ?
                     (object)opportunity.PricingList.PricingListId.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.BusinessTypeId,
                        ((int)opportunity.BusinessType) != 0 ? (object)((int)opportunity.BusinessType) : DBNull.Value);
                command.Parameters.AddWithValue(
                    Constants.ParameterNames.PersonIdListParam,
                    opportunity.ProposedPersonIdList ?? (object)DBNull.Value);
                command.Parameters.AddWithValue(
                    Constants.ParameterNames.StrawManListParam,
                    opportunity.StrawManList ?? (object)DBNull.Value);

                try
                {
                    connection.Open();

                    SqlTransaction trn = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    command.Transaction = trn;

                    command.ExecuteNonQuery();

                    trn.Commit();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static void ReadOpportunities(DbDataReader reader, List<Opportunity> result)
        {
            if (!reader.HasRows) return;
            var opportunityIdIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityIdColumn);
            var nameIndex = reader.GetOrdinal(Constants.ColumnNames.NameColumn);
            var clientIdIndex = reader.GetOrdinal(Constants.ColumnNames.ClientIdColumn);
            var salespersonIdIndex = reader.GetOrdinal(Constants.ColumnNames.SalespersonIdColumn);
            var opportunityStatusIdIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityStatusIdColumn);
            var priorityIndex = reader.GetOrdinal(Constants.ColumnNames.PriorityColumn);
            var projectedStartDateIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectedStartDateColumn);
            var projectedEndDateIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectedEndDateColumn);
            var opportunityNumberIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityNumberColumn);
            var descriptionIndex = reader.GetOrdinal(Constants.ColumnNames.DescriptionColumn);
            var clientNameIndex = reader.GetOrdinal(Constants.ColumnNames.ClientNameColumn);
            var salespersonFirstNameIndex = reader.GetOrdinal(Constants.ColumnNames.SalespersonFirstNameColumn);
            var salespersonLastNameIndex = reader.GetOrdinal(Constants.ColumnNames.SalespersonLastNameColumn);
            var salespersonStatusIndex = reader.GetOrdinal(Constants.ColumnNames.SalespersonStatusColumn);
            var opportunityStatusNameIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityStatusNameColumn);
            var practiceIdIndex = reader.GetOrdinal(Constants.ColumnNames.PracticeIdColumn);
            var practiceNameIndex = reader.GetOrdinal(Constants.ColumnNames.PracticeNameColumn);
            var buyerNameIndex = reader.GetOrdinal(Constants.ColumnNames.BuyerNameColumn);
            var createDateIndex = reader.GetOrdinal(Constants.ColumnNames.CreateDateColumn);
            var pipelineIndex = reader.GetOrdinal(Constants.ColumnNames.PipelineColumn);
            var proposedIndex = reader.GetOrdinal(Constants.ColumnNames.ProposedColumn);
            var sendOutIndex = reader.GetOrdinal(Constants.ColumnNames.SendOutColumn);
            var projectId = reader.GetOrdinal(Constants.ColumnNames.ProjectIdColumn);
            var projectNumber = reader.GetOrdinal(Constants.ColumnNames.ProjectNumberColumn);
            var opportunityIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityIndexColumn);
            var revenueTypeIndex = reader.GetOrdinal(Constants.ColumnNames.RevenueTypeColumn);
            var ownerIdIndex = reader.GetOrdinal(Constants.ColumnNames.OwnerIdColumn);
            var ownerFirstNameIndex = reader.GetOrdinal(Constants.ColumnNames.OwnerFirstNameColumn);
            var ownerLastNameIndex = reader.GetOrdinal(Constants.ColumnNames.OwnerLastNameColumn);
            var ownerStatusIndex = reader.GetOrdinal(Constants.ColumnNames.OwnerStatusColumn);
            var groupIdIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectGroupIdColumn);
            var lastUpdateIndex = reader.GetOrdinal(Constants.ColumnNames.LastUpdateColumn);
            var groupNameIndex = reader.GetOrdinal(Constants.ColumnNames.ProjectGroupNameColumn);
            var practManagerIdIndex = reader.GetOrdinal(Constants.ColumnNames.PracticeManagerIdColumn);
            var closeDateIndex = -1;
            var priorityDisplaynameIndex = -1;

            int priorityIdIndex = -1;
            int pricingListIdIndex = -1;
            int pricingListNameIndex = -1;
            var prioritySortOrderIndex = -1;
            int EstimatedRevenueiIndex = -1;
            int opportunityBusinessTypesIndex = -1;
            try
            {
                opportunityBusinessTypesIndex = reader.GetOrdinal(Constants.ColumnNames.BusinessTypeId);
            }
            catch
            { }

            try
            {
                priorityDisplaynameIndex = reader.GetOrdinal(Constants.ColumnNames.DisplayNameColumn);
            }
            catch
            {
                priorityDisplaynameIndex = -1;
            }
            try
            {
                closeDateIndex = reader.GetOrdinal(Constants.ColumnNames.CloseDateColumn);
            }
            catch
            {
                closeDateIndex = -1;
            }

            try
            {
                EstimatedRevenueiIndex = reader.GetOrdinal(Constants.ColumnNames.EstimatedRevenueColumn);
            }
            catch
            {
                EstimatedRevenueiIndex = -1;
            }

            try
            {
                priorityIdIndex = reader.GetOrdinal(Constants.ColumnNames.PriorityIdColumn);
            }
            catch
            {
                priorityIdIndex = -1;
            }
            try
            {
                prioritySortOrderIndex = reader.GetOrdinal(Constants.ColumnNames.PrioritySortOrderColumn);
            }
            catch
            {
                prioritySortOrderIndex = -1;
            }
            try
            {
                pricingListIdIndex = reader.GetOrdinal(Constants.ColumnNames.PricingListId);
            }
            catch
            {
                pricingListIdIndex = -1;
            }

            try
            {
                pricingListNameIndex = reader.GetOrdinal(Constants.ColumnNames.PricingListNameColumn);
            }
            catch
            {
                pricingListNameIndex = -1;
            }

            while (reader.Read())
            {
                // Reading the item
                var opportunity =
                    new Opportunity
                        {
                            Id = reader.GetInt32(opportunityIdIndex),
                            Name = reader.GetString(nameIndex),
                            Description =
                                !reader.IsDBNull(descriptionIndex) ? reader.GetString(descriptionIndex) : null,
                            OpportunityNumber = reader.GetString(opportunityNumberIndex),
                            ProjectedStartDate =
                                !reader.IsDBNull(projectedStartDateIndex)
                                    ? (DateTime?)reader.GetDateTime(projectedStartDateIndex)
                                    : null,
                            ProjectedEndDate =
                                !reader.IsDBNull(projectedEndDateIndex)
                                    ? (DateTime?)reader.GetDateTime(projectedEndDateIndex)
                                    : null,
                            BuyerName =
                                !reader.IsDBNull(buyerNameIndex) ? reader.GetString(buyerNameIndex) : null,
                            CreateDate = reader.GetDateTime(createDateIndex),
                            Priority = new OpportunityPriority { Priority = reader.GetString(priorityIndex) },
                            Pipeline =
                                !reader.IsDBNull(pipelineIndex) ? reader.GetString(pipelineIndex) : null,
                            Proposed =
                                !reader.IsDBNull(proposedIndex) ? reader.GetString(proposedIndex) : null,
                            SendOut =
                                !reader.IsDBNull(sendOutIndex) ? reader.GetString(sendOutIndex) : null,
                            Client = new Client
                                {
                                    Id = reader.GetInt32(clientIdIndex),
                                    Name = reader.GetString(clientNameIndex)
                                },
                            Status = new OpportunityStatus
                                {
                                    Id = reader.GetInt32(opportunityStatusIdIndex),
                                    Name = reader.GetString(opportunityStatusNameIndex)
                                },
                            Salesperson =
                                !reader.IsDBNull(salespersonIdIndex)
                                    ? new Person
                                        {
                                            Id = reader.GetInt32(salespersonIdIndex),
                                            FirstName = reader.GetString(salespersonFirstNameIndex),
                                            LastName = reader.GetString(salespersonLastNameIndex),
                                            Status = !reader.IsDBNull(salespersonStatusIndex) ? new PersonStatus { Name = reader.GetString(salespersonStatusIndex) } : null
                                        }
                                    : null,
                            Practice =
                                new Practice
                                    {
                                        Id = reader.GetInt32(practiceIdIndex),
                                        Name = reader.GetString(practiceNameIndex)
                                        //PracticeOwner = new Person
                                        //    {
                                        //        Id = reader.GetInt32(practManagerIdIndex)
                                        //    }
                                    },
                            LastUpdate = reader.GetDateTime(lastUpdateIndex),
                            Project =
                                !reader.IsDBNull(projectId) ?
                                    new Project
                                        {
                                            Id = (int?)reader.GetInt32(projectId),
                                            ProjectNumber = reader.GetString(projectNumber)
                                        }
                                    : null,
                            OpportunityIndex =
                                !reader.IsDBNull(opportunityIndex) ? (int?)reader.GetInt32(opportunityIndex) : null,
                            OpportunityRevenueType = (RevenueType)reader.GetInt32(revenueTypeIndex),
                            Owner =
                                !reader.IsDBNull(ownerIdIndex)
                                    ? new Person
                                        {
                                            Id = reader.GetInt32(ownerIdIndex),
                                            LastName = !reader.IsDBNull(ownerLastNameIndex) ? reader.GetString(ownerLastNameIndex) : null,
                                            FirstName = !reader.IsDBNull(ownerFirstNameIndex) ? reader.GetString(ownerFirstNameIndex) : null,
                                            Status = !reader.IsDBNull(ownerStatusIndex) ? new PersonStatus() { Name = reader.GetString(ownerStatusIndex) } : null
                                        }
                                    : null,
                            Group = !reader.IsDBNull(groupIdIndex)
                                        ? new ProjectGroup
                                            {
                                                Id = reader.GetInt32(groupIdIndex),
                                                Name = reader.GetString(groupNameIndex)
                                            }
                                        : null
                        };
                if(!reader.IsDBNull(practManagerIdIndex))
                {
                    opportunity.Practice.PracticeOwner = new Person() { Id = reader.GetInt32(practManagerIdIndex) };
                }
                if (opportunityBusinessTypesIndex > -1)
                {
                    opportunity.BusinessType = reader.IsDBNull(opportunityBusinessTypesIndex) ? (BusinessType)Enum.Parse(typeof(BusinessType), "0") : (BusinessType)Enum.Parse(typeof(BusinessType), reader.GetInt32(opportunityBusinessTypesIndex).ToString());
                }

                if (closeDateIndex > -1)
                {
                    if (!reader.IsDBNull(closeDateIndex))
                    {
                        opportunity.CloseDate = !reader.IsDBNull(closeDateIndex)
                                                    ? (DateTime?)reader.GetDateTime(closeDateIndex)
                                                    : null;
                    }
                }

                if (pricingListIdIndex > -1)
                {
                    opportunity.PricingList = !reader.IsDBNull(pricingListIdIndex) ?
                                                  new PricingList
                                                      {
                                                          PricingListId = reader.GetInt32(pricingListIdIndex),
                                                          Name = pricingListNameIndex > -1 ? reader.GetString(pricingListNameIndex) : string.Empty
                                                      }
                                                  : null;
                }

                if (EstimatedRevenueiIndex > -1)
                {
                    if (!reader.IsDBNull(EstimatedRevenueiIndex))
                    {
                        opportunity.EstimatedRevenue = reader.GetDecimal(EstimatedRevenueiIndex);
                    }
                }

                if (priorityIdIndex > -1)
                {
                    if (!reader.IsDBNull(priorityIdIndex))
                    {
                        opportunity.Priority.Id = reader.GetInt32(priorityIdIndex);
                    }
                }

                if (priorityDisplaynameIndex > -1)
                {
                    if (!reader.IsDBNull(priorityDisplaynameIndex))
                    {
                        opportunity.Priority.DisplayName = reader.GetString(priorityDisplaynameIndex);
                    }
                }
                if (prioritySortOrderIndex > -1)
                {
                    if (!reader.IsDBNull(prioritySortOrderIndex))
                    {
                        opportunity.Priority.SortOrder = reader.GetInt32(prioritySortOrderIndex);
                    }
                }

                result.Add(opportunity);
            }
        }

        #endregion Methods

        public static int? GetOpportunityId(string opportunityNumber)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityGetByNumber, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ColumnNames.OpportunityNumberColumn, opportunityNumber);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            SqlInt32 opportunityId = reader.GetSqlInt32(0);
                            if (!opportunityId.IsNull)
                            {
                                return opportunityId.Value;
                            }
                        }
                    }

                    return null;
                }
            }
        }

        public static void FillProposedPersons(List<Opportunity> opportunities)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.GetPersonsByOpportunityIds, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;
                    var opportunityIds = DataTransferObjects.Utils.Generic.EnumerableToCsv
                                    (from id in opportunities where id.Id.HasValue select id, id => id.Id.Value);

                    command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdsParam, opportunityIds);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        ReadAndAttachPersonsToOpportunities(reader, opportunities);
                    }
                }
            }
        }

        private static void ReadAndAttachPersonsToOpportunity(SqlDataReader reader, List<OpportunityPerson> opportunityPersons)
        {
            if (!reader.HasRows) return;
            int personIdIndex = reader.GetOrdinal(Constants.ColumnNames.PersonId);
            int firstNameIndex = reader.GetOrdinal(Constants.ColumnNames.FirstName);
            int lastNameIndex = reader.GetOrdinal(Constants.ColumnNames.LastName);
            int opportunityPersonTypeIdIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityPersonTypeId);
            int opportunityPersonRelationTypeIdIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityPersonRelationTypeId);
            int opportunityPersonQuantityIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityPersonQuantity);
            int needByIndex = reader.GetOrdinal(Constants.ColumnNames.NeedBy);
            int personStatusIdIndex = reader.GetOrdinal(Constants.ColumnNames.PersonStatusId);

            while (reader.Read())
            {
                var opportunityPerson = new OpportunityPerson
                    {
                        Person = new Person
                            {
                                Id = reader.GetInt32(personIdIndex),
                                FirstName = reader.GetString(firstNameIndex),
                                LastName = reader.GetString(lastNameIndex),
                                Status = new PersonStatus
                                    {
                                        Id = reader.GetInt32(personStatusIdIndex)
                                    }
                            },
                        PersonType = reader.GetInt32(opportunityPersonTypeIdIndex),
                        RelationType = reader.GetInt32(opportunityPersonRelationTypeIdIndex),
                        Quantity = !reader.IsDBNull(opportunityPersonQuantityIndex) ? reader.GetInt32(opportunityPersonQuantityIndex) : 0,
                        NeedBy = !reader.IsDBNull(needByIndex) ? reader.GetDateTime(needByIndex) : (DateTime?)null
                    }
                    ;

                opportunityPersons.Add(opportunityPerson);
            }
        }

        private static void ReadAndAttachPersonsToOpportunities(SqlDataReader reader, List<Opportunity> opportunities)
        {
            if (!reader.HasRows) return;
            int personIdIndex = reader.GetOrdinal(Constants.ColumnNames.PersonId);
            int firstNameIndex = reader.GetOrdinal(Constants.ColumnNames.FirstName);
            int lastNameIndex = reader.GetOrdinal(Constants.ColumnNames.LastName);
            int OpportunityIdIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityIdColumn);
            int opportunityPersonTypeIdIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityPersonTypeId);
            int opportunityPersonRelationTypeIdIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityPersonRelationTypeId);
            int opportunityPersonQuantityIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityPersonQuantity);
            int needByIndex = reader.GetOrdinal(Constants.ColumnNames.NeedBy);
            int personStatusIdIndex = reader.GetOrdinal(Constants.ColumnNames.PersonStatusId);
            Opportunity CurrentOpportunity = null;

            while (reader.Read())
            {
                var opportunityId = reader.GetInt32(OpportunityIdIndex);
                if (CurrentOpportunity == null || CurrentOpportunity.Id.Value != opportunityId)
                {
                    CurrentOpportunity = opportunities.Find(opty => opty.Id.HasValue && opty.Id.Value == opportunityId);
                    CurrentOpportunity.ProposedPersons = new List<OpportunityPerson>();
                }

                var personId = reader.GetInt32(personIdIndex);
                var opportunityPerson = new OpportunityPerson
                    {
                        Person = new Person
                            {
                                Id = personId,
                                FirstName = reader.GetString(firstNameIndex),
                                LastName = reader.GetString(lastNameIndex),
                                Status = new PersonStatus
                                    {
                                        Id = reader.GetInt32(personStatusIdIndex)
                                    }
                            },
                        PersonType = reader.GetInt32(opportunityPersonTypeIdIndex),
                        RelationType = reader.GetInt32(opportunityPersonRelationTypeIdIndex),
                        Quantity = !reader.IsDBNull(opportunityPersonQuantityIndex) ? reader.GetInt32(opportunityPersonQuantityIndex) : 0,
                        NeedBy = !reader.IsDBNull(needByIndex) ? reader.GetDateTime(needByIndex) : (DateTime?)null
                    };
                CurrentOpportunity.ProposedPersons.Add(opportunityPerson);
            }
        }

        public static List<OpportunityPerson> GetOpportunityPersons(int opportunityId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.GetOpportunityPersons, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ColumnNames.OpportunityIdColumn, opportunityId);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        var result = new List<OpportunityPerson>();
                        ReadAndAttachPersonsToOpportunity(reader, result);
                        return result;
                    }
                }
            }
        }

        public static int ConvertOpportunityToProject(int opportunityId, string userName, bool hasPersons)
        {
            int res;
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.ConvertOpportunityToProject, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdParam, opportunityId);
                command.Parameters.AddWithValue(Constants.ParameterNames.UserLoginParam,
                                                !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.HasPersons, hasPersons);

                var idParam = new SqlParameter(Constants.ParameterNames.ProjectId, SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(idParam);

                try
                {
                    connection.Open();

                    SqlTransaction trn = connection.BeginTransaction();
                    command.Transaction = trn;

                    command.ExecuteNonQuery();

                    trn.Commit();

                    res = (int)idParam.Value;
                }
                catch (SqlException ex)
                {
                    throw new DataAccessException(ex);
                }
            }

            return res;
        }

        public static void OpportunityPersonInsert(int opportunityId, string personIdList, int relationTypeId, string outSideResources)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityPersonInsert, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdParam, opportunityId);
                command.Parameters.AddWithValue(Constants.ParameterNames.PersonIdListParam, personIdList);
                command.Parameters.AddWithValue(Constants.ParameterNames.OutSideResourcesParam, outSideResources);
                command.Parameters.AddWithValue(Constants.ParameterNames.RelationTypeIdParam, relationTypeId);

                try
                {
                    connection.Open();

                    SqlTransaction trn = connection.BeginTransaction();
                    command.Transaction = trn;

                    command.ExecuteNonQuery();

                    trn.Commit();
                }
                catch (SqlException ex)
                {
                    throw new DataAccessException(ex);
                }
            }
        }

        public static void OpportunityPersonDelete(int opportunityId, string personIdList)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityPersonDelete, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdParam, opportunityId);
                command.Parameters.AddWithValue(Constants.ParameterNames.PersonIdListParam, personIdList);

                try
                {
                    connection.Open();

                    SqlTransaction trn = connection.BeginTransaction();
                    command.Transaction = trn;

                    command.ExecuteNonQuery();

                    trn.Commit();
                }
                catch (SqlException ex)
                {
                    throw new DataAccessException(ex);
                }
            }
        }

        public static List<OpportunityPriority> GetOpportunityPriorities(bool isinserted)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityPriorities, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;
                command.Parameters.AddWithValue(Constants.ParameterNames.IsInserted, isinserted);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<OpportunityPriority>();
                    ReadOpportunityPriorityListAll(reader, result);
                    return result;
                }
            }
        }

        public static void InsertOpportunityPriority(OpportunityPriority opportunityPriority)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityPriorityInsert, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PriorityIdParam, opportunityPriority.Id);
                command.Parameters.AddWithValue(Constants.ParameterNames.DescriptionParam,
                    opportunityPriority.Description != null ? (object)opportunityPriority.Description : DBNull.Value);

                command.Parameters.AddWithValue(Constants.ParameterNames.DisplayNameParam,
                    opportunityPriority.DisplayName != null ? (object)opportunityPriority.DisplayName : DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void UpdateOpportunityPriority(int oldPriorityId, OpportunityPriority opportunityPriority, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityPriorityUpdate, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.OldPriorityIdParam, oldPriorityId);
                command.Parameters.AddWithValue(Constants.ParameterNames.PriorityIdParam, opportunityPriority.Id);
                command.Parameters.AddWithValue(Constants.ParameterNames.DescriptionParam,
                    opportunityPriority.Description != null ? (object)opportunityPriority.Description : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.DisplayNameParam,
                    opportunityPriority.Description != null ? (object)opportunityPriority.DisplayName : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.UserLoginParam,
                                            !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteOpportunityPriority(int? updatedPriorityId, int deletedPriorityId, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityPriorityDelete, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.UpdatedPriorityIdParam,
                                                updatedPriorityId != null ? (object)updatedPriorityId.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.DeletedPriorityIdParam, deletedPriorityId);
                command.Parameters.AddWithValue(Constants.ParameterNames.UserLoginParam,
                                             !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void OpportunityDelete(int opportunityId, string userName)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityDelete, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdParam, opportunityId);
                    command.Parameters.AddWithValue(Constants.ParameterNames.UserLoginParam, userName);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static bool IsOpportunityPriorityInUse(int priorityId)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.Opportunitites.IsOpportunityPriorityInUse, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.PriorityIdParam, priorityId);

                    connection.Open();
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        public static List<Opportunity> GetActiveOpportunitiesByOwnerId(int personId)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (SqlCommand command = new SqlCommand(GetActiveOpportunitiesByOwnerIdProcedure, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.PersonId, personId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    List<Opportunity> result = new List<Opportunity>();

                    ReadOpportunitiesByOwner(reader, result);

                    return result;
                }
            }
        }

        private static void ReadOpportunitiesByOwner(SqlDataReader reader, List<Opportunity> result)
        {
            if (!reader.HasRows) return;
            int opportunityIdIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityIdColumn);
            int opportunityNameIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityName);
            int OpportunityNumberIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityNumberColumn);

            while (reader.Read())
            {
                var opportunity = new Opportunity
                    {
                        Id = reader.GetInt32(opportunityIdIndex),
                        Name = reader.GetString(opportunityNameIndex),
                        OpportunityNumber = reader.GetString(OpportunityNumberIndex)
                    };
                result.Add(opportunity);
            }
        }

        public static IDictionary<string, int> GetOpportunityPriorityTransitionCount(int daysPrevious)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.GetOpportunityPriorityTransitionCount, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.DaysPrevious, daysPrevious);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        var result = new Dictionary<string, int>();
                        ReadOpportunityPriorityTrend(reader, result);
                        return result;
                    }
                }
            }
        }

        private static void ReadOpportunityPriorityTrend(SqlDataReader reader, Dictionary<string, int> result)
        {
            if (!reader.HasRows) return;
            int priorityTrendTypeIndex = reader.GetOrdinal(Constants.ColumnNames.PriorityTrendTypeColumn);
            int priorityTrendCountIndex = reader.GetOrdinal(Constants.ColumnNames.PriorityTrendCountColumn);
            while (reader.Read())
            {
                string key = reader.GetString(priorityTrendTypeIndex);
                int value = reader.GetInt32(priorityTrendCountIndex);

                result.Add(key, value);
            }
        }

        public static IDictionary<string, int> GetOpportunityStatusChangeCount(int daysPrevious)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.GetOpportunityStatusChangeCount, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.DaysPrevious, daysPrevious);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        var result = new Dictionary<string, int>();
                        ReadOpportunityStatusChangesRecord(reader, result);
                        return result;
                    }
                }
            }
        }

        private static void ReadOpportunityStatusChangesRecord(SqlDataReader reader, Dictionary<string, int> result)
        {
            if (!reader.HasRows) return;
            int statusIndex = reader.GetOrdinal(Constants.ColumnNames.StatusColumn);
            int statusCountIndex = reader.GetOrdinal(Constants.ColumnNames.StatusCountColumn);
            while (reader.Read())
            {
                string key = reader.GetString(statusIndex);
                int value = reader.GetInt32(statusCountIndex);

                result.Add(key, value);
            }
        }

        public static void UpdatePriorityIdForOpportunity(int opportunityId, int priorityId, string userName)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.UpdatePriorityIdForOpportunity, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdParam, opportunityId);
                command.Parameters.AddWithValue(Constants.ParameterNames.PriorityIdParam, priorityId);
                command.Parameters.AddWithValue(Constants.ParameterNames.UserLoginParam,
                                              !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void AttachProjectToOpportunity(int opportunityId, int projectId, int priorityId, string userName, bool isOpportunityDescriptionSelected)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.AttachProjectToOpportunity, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdParam, opportunityId);
                command.Parameters.AddWithValue(Constants.ParameterNames.ProjectIdParam, projectId);
                command.Parameters.AddWithValue(Constants.ParameterNames.PriorityIdParam, priorityId);
                command.Parameters.AddWithValue(Constants.ParameterNames.isOpportunityDescriptionSelected, isOpportunityDescriptionSelected);
                command.Parameters.AddWithValue(Constants.ParameterNames.UserLoginParam,
                                              !string.IsNullOrEmpty(userName) ? (object)userName : DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static List<Opportunity> FilteredOpportunityListAll(bool showActive, bool showExperimental, bool showInactive, bool showLost, bool showWon, string clientIdsList, string opportunityGroupIdsList, string opportunityOwnerIdsList, string salespersonIdsList)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.FilteredOpportunityListAll, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ShowActiveParam, showActive);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowExperimentalParam, showExperimental);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowInactiveParam, showInactive);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowLostParam, showLost);
                command.Parameters.AddWithValue(Constants.ParameterNames.ShowWonParam, showWon);
                command.Parameters.AddWithValue(Constants.ParameterNames.ClientIdsParam, clientIdsList);
                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityGroupIdsParam, opportunityGroupIdsList);
                command.Parameters.AddWithValue(Constants.ParameterNames.SalespersonIdsParam, salespersonIdsList);
                command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityOwnerIdsParam, opportunityOwnerIdsList);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<Opportunity>();
                    ReadOpportunities(reader, result);
                    return result;
                }
            }
        }

        public static List<Opportunity> OpportunitySearchText(string looked, int personId)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunitySearchText, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.LookedParam,
                    !string.IsNullOrEmpty(looked) ? (object)looked : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.PersonId, personId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<Opportunity>();
                    ReadOpportunities(reader, result);
                    return result;
                }
            }
        }

        public static bool IsOpportunityHaveTeamStructure(int opportunityId)
        {
            using (SqlConnection connection = new SqlConnection(DataSourceHelper.DataConnection))
            {
                using (SqlCommand command = new SqlCommand(Constants.ProcedureNames.Opportunitites.IsOpportunityHaveTeamStructure, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = connection.ConnectionTimeout;

                    command.Parameters.AddWithValue(Constants.ParameterNames.OpportunityIdParam, opportunityId);

                    connection.Open();
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        public static List<Opportunity> OpportunityListWithMinimumDetails(int? clientId, bool? attach)
        {
            using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
            using (var command = new SqlCommand(Constants.ProcedureNames.Opportunitites.OpportunityListWithMinimumDetails, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = connection.ConnectionTimeout;

                command.Parameters.AddWithValue(Constants.ParameterNames.ClientIdParam, clientId.HasValue ? (object)clientId.Value : DBNull.Value);
                command.Parameters.AddWithValue(Constants.ParameterNames.LinkParam, attach.HasValue ? (object)attach.Value : DBNull.Value);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<Opportunity>();
                    ReadOpportunityBasicDetails(reader, result);
                    return result;
                }
            }
        }

        private static void ReadOpportunityBasicDetails(SqlDataReader reader, List<Opportunity> result)
        {
            int opportunityIdIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityIdColumn);
            int opportunityNameIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityName);
            int opportunityNumberIndex = reader.GetOrdinal(Constants.ColumnNames.OpportunityNumberColumn);
            int clientNameIndex = reader.GetOrdinal(Constants.ColumnNames.ClientNameColumn);

            if (!reader.HasRows) return;
            while (reader.Read())
            {
                var opportunity = new Opportunity
                    {
                        Id = reader.GetInt32(opportunityIdIndex),
                        Name = reader.GetString(opportunityNameIndex),
                        OpportunityNumber = reader.GetString(opportunityNumberIndex),
                        Client = new Client
                            {
                                Name = reader.GetString(clientNameIndex)
                            }
                    };

                result.Add(opportunity);
            }
        }
    }
}

