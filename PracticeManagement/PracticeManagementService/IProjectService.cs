﻿using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using DataTransferObjects;
using DataTransferObjects.ContextObjects;
using DataTransferObjects.TimeEntry;

namespace PracticeManagementService
{
    [ServiceContract]
    public interface IProjectService
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [OperationContract]
        Project ProjectGetById(int projectId);

        [OperationContract]
        DateTime GetProjectLastChangeDateFortheGivenStatus(int projectId, int projectStatusId);

        /// <summary>
        /// Projects Computed Financials
        /// </summary>
        [OperationContract]
        ComputedFinancials GetProjectsComputedFinancials(int projectId);

        /// <summary>
        /// Person Milestone With Financials
        /// </summary>
        [OperationContract]
        DataSet GetProjectMilestonesFinancials(int projectId);

        /// <summary>
        /// Enlists number of requested projects by client.
        /// </summary>
        [OperationContract]
        int ProjectCountByClient(int clientId);

        [OperationContract]
        List<Project> GetProjectListByDateRange(bool showProjected,
                                                bool showCompleted,
                                                bool showActive,
                                                bool showInternal,
                                                bool showExperimental,
                                                bool showInactive,
                                                DateTime periodStart,
                                                DateTime periodEnd
                                                );

        /// <summary>
        /// Enlists the requested projects.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="viewerUsername"></param>
        [OperationContract]
        List<Project> ListProjectsByClient(int? clientId, string viewerUsername);

        [OperationContract]
        List<Project> ListProjectsByClientShort(int? clientId, bool IsOnlyActiveAndProjective, bool IsOnlyActiveAndInternal, bool IsOnlyEnternalProjects);

        [OperationContract]
        List<Project> ListProjectsByClientAndPersonInPeriod(int clientId, bool isOnlyActiveAndInternal, bool isOnlyEnternalProjects, int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        List<Project> ListProjectsByClientWithSort(int? clientId, string viewerUsername, string sortBy);

        /// <summary>
        /// Clones project specified
        /// </summary>
        [OperationContract]
        int CloneProject(ProjectCloningContext context);

        /// <summary>
        /// Enlists the requested projects.
        /// </summary>
        [OperationContract]
        List<Project> GetProjectListCustom(bool projected, bool completed, bool active, bool experimantal, bool proposed);

        /// <summary>
        /// Enlists the requested projects.
        /// </summary>
        /// <param name="clientIds">Comma separated list of client ids. Null value means all clients.</param>
        /// <param name="showProjected">If true - the projected projects will be included in the results.</param>
        /// <param name="showCompleted">If true - the completed projects will be included in the results.</param>
        /// <param name="showActive">If true - the active (statusName=Active) projects will be included in the results.</param>
        /// <param name="showExperimental">If true - the experimantal projects will are included in the results.</param>
        /// <param name="periodStart">The start of the period to enlist the projects within.</param>
        /// <param name="periodEnd">The end of the period to enlist the projects within.</param>
        /// <param name="userName">The user (by email) to retrive the result for.</param>
        /// <param name="projectGroupIdsList"></param>
        /// <param name="includeCurentYearFinancials">
        /// Determines the financial indexes for the current year need to be included into the result.
        /// </param>
        /// <param name="salespersonIdsList"></param>
        /// <param name="projectOwnerIdsList"></param>
        /// <param name="practiceIdsList"></param>
        /// <returns>The list of the projects are match with the specified conditions.</returns>
        [OperationContract]
        List<Project> ProjectListAllMultiParameters(
            string clientIds,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showInternal,
            bool showExperimental,
            bool showProposed,
            bool showInactive,
            DateTime periodStart,
            DateTime periodEnd,
            string salespersonIdsList,
            string projectOwnerIdsList,
            string practiceIdsList,
            string divisionIdsList,
            string channelIdsList,
            string revenueTypeIdsList,
            string offeringIdsList,
            string projectGroupIdsList,
            ProjectCalculateRangeType includeCurentYearFinancials,
            bool excludeInternalPractices,
            string userLogin,
            bool useActuals,
            bool getFinancialsFromCache);

        [OperationContract]
        bool IsProjectSummaryCachedToday();

        [OperationContract]
        List<Project> GetProjectListWithFinancials(
            string clientIds,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showInternal,
            bool showExperimental,
            bool showInactive,
            DateTime periodStart,
            DateTime periodEnd,
            string salespersonIdsList,
            string projectOwnerIdsList,
            string practiceIdsList,
            string projectGroupIdsList,
            bool excludeInternalPractices
            );

        [OperationContract]
        List<MilestonePerson> GetProjectListGroupByPracticeManagers(
            string clientIds,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showInternal,
            bool showExperimental,
            bool showProposed,
            bool showInactive,
            DateTime periodStart,
            DateTime periodEnd,
            string salespersonIdsList,
            string projectOwnerIdsList,
            string practiceIdsList,
            string projectGroupIdsList,
            bool excludeInternalPractices
            );

        [OperationContract]
        List<Project> GetBenchList(BenchReportContext context);

        [OperationContract]
        List<Project> GetBenchListWithoutBenchTotalAndAdminCosts(BenchReportContext context);

        /// <summary>
        /// Retrives a list of the projects by the specified conditions.
        /// </summary>
        /// <param name="looked">A text to be looked for.</param>
        /// <param name="personId"></param>
        /// <returns>A list of the <see cref="Project"/> objects.</returns>
        [OperationContract]
        List<Project> ProjectSearchText(
            string looked,
            int personId,
            string clientIdsList,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showInternal,
            bool showExperimental,
            bool showProposed,
            bool showInactive,
            string salespersonIdsList,
            string projectManagerIdsList,
            string practiceIdsList,
            string divisionIdsList,
            string channelIdsList,
            string revenueTypeIdsList,
            string offeringIdsList,
            string projectGroupIdsList);

        /// <summary>
        /// Reatrives a project with a specified ID.
        /// </summary>
        /// <param name="projectId">The ID of the requested project.</param>
        /// <param name="userName">The user (by email) to retrive the result for.</param>
        /// <returns>The <see cref="Project"/> record if found and null otherwise.</returns>
        [OperationContract]
        Project GetProjectDetailWithoutMilestones(int projectId, string userName);

        /// <summary>
        /// Saves the <see cref="Project"/> data into the database.
        /// </summary>
        /// <param name="project">The <see cref="Project"/> to be saved.</param>
        /// <param name="userName">A current user.</param>
        /// <returns>An ID of the saved project.</returns>
        [OperationContract]
        int SaveProjectDetail(Project project, string userName);

        [OperationContract]
        void SaveInternalProject(string projectName, string projectNumberSeries, int divisionId, int practiceId);
        /// <summary>
        /// Provides an info for the month mini report.
        /// </summary>
        /// <param name="month">The month to the data be provided for.</param>
        /// <param name="userName">The user (by email) to retrive the result for.</param>
        /// <returns>An XML document with the report data.</returns>
        [OperationContract]
        string MonthMiniReport(
            DateTime month,
            string userName,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showExperimental,
            bool showProposed,
            bool showInternal,
            bool showInactive,
            bool useActuals);

        /// <summary>
        /// Retrives the data for the person stats report.
        /// </summary>
        /// <param name="startDate">The period start.</param>
        /// <param name="endDate">The period end.</param>
        /// <param name="userName">The user (by email) to retrive the result for.</param>
        /// <param name="salespersonId">Determines an ID of the salesperson to filter the list for.</param>
        /// <param name="practiceManagerId">Determines an ID of the practice manager to filter the list for.</param>
        /// <returns>The list of the <see cref="PersonStats"/> objects.</returns>
        [OperationContract]
        List<PersonStats> PersonStartsReport(
            DateTime startDate,
            DateTime endDate,
            string userName,
            int? salespersonId,
            int? practiceManagerId,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showExperimental,
            bool showProposed,
            bool showInternal,
            bool showInactive,
            bool useActuals);

        /// <summary>
        /// Reatrives a projectID with a specified ProjectNumber.
        /// </summary>
        /// <param name="projectNumber">The number of the requested project.</param>
        /// <returns>The projectID if found and null otherwise.</returns>
        [OperationContract]
        int? GetProjectId(string projectNumber);

        [OperationContract]
        List<ProjectsGroupedByPerson> PersonBudgetListByYear(int year, BudgetCategoryType categoryType);

        [OperationContract]
        List<ProjectsGroupedByPractice> PracticeBudgetListByYear(int year);

        [OperationContract]
        void CategoryItemBudgetSave(int itemId, BudgetCategoryType categoryType, DateTime monthStartDate, PracticeManagementCurrency amount);

        [OperationContract]
        List<ProjectsGroupedByPerson> CalculateBudgetForPersons(
                                                                DateTime startDate,
                                                                DateTime endDate,
                                                                bool showProjected,
                                                                bool showCompleted,
                                                                bool showActive,
                                                                bool showInternal,
                                                                bool showExperimental,
            bool showProposed,
                                                                bool showInactive,
                                                                string practiceIdsList,
                                                                bool excludeInternalPractices,
                                                                string personIds,
                                                                BudgetCategoryType categoryType);

        [OperationContract]
        List<ProjectsGroupedByPractice> CalculateBudgetForPractices(
                                                                    DateTime startDate,
                                                                    DateTime endDate,
                                                                    bool showProjected,
                                                                    bool showCompleted,
                                                                    bool showActive,
                                                                    bool showInternal,
                                                                    bool showExperimental,
                                                                    bool showProposed,
                                                                    bool showInactive,
                                                                    string practiceIdsList,
                                                                    bool excludeInternalPractices);

        [OperationContract]
        void CategoryItemsSaveFromXML(List<CategoryItemBudget> categoryItems, int year);

        [OperationContract]
        void ProjectDelete(int projectId, string userName);

        [OperationContract]
        ProjectExpense[] GetProjectExpensesForProject(ProjectExpense entity);

        [OperationContract]
        List<Project> AllProjectsWithFinancialTotalsAndPersons();

        [OperationContract]
        bool IsUserHasPermissionOnProject(string user, int id, bool isProjectId);

        [OperationContract]
        bool IsUserIsOwnerOfProject(string user, int id, bool isProjectId);

        [OperationContract]
        bool IsUserIsProjectOwner(string user, int id);

        [OperationContract]
        List<Project> GetProjectsListByProjectGroupId(int projectGroupId, bool isInternal, int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        List<Project> GetBusinessDevelopmentProject();

        [OperationContract]
        Project GetProjectByIdShort(int projectId);

        [OperationContract]
        List<TimeTypeRecord> GetTimeTypesByProjectId(int projectId, bool IsOnlyActive, DateTime? startDate, DateTime? endDate);

        [OperationContract]
        void SetProjectTimeTypes(int projectId, string projectTimeTypesList);

        [OperationContract]
        Dictionary<DateTime, bool> GetIsHourlyRevenueByPeriod(int projectId, int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        Project GetProjectShortByProjectNumber(string projectNumber, int? milestoneId, DateTime? startDate, DateTime? endDate);

        [OperationContract]
        List<TimeTypeRecord> GetTimeTypesInUseDetailsByProject(int projectId, string timeTypeIds);

        [OperationContract]
        string AttachOpportunityToProject(int projectId, int opportunityId, string userLogin, int? pricingListId, bool link);

        [OperationContract]
        int CSATInsert(ProjectCSAT projectCSAT, string userLogin);

        [OperationContract]
        void CSATDelete(int projectCSATId, string userLogin);

        [OperationContract]
        void CSATUpdate(ProjectCSAT projectCSAT, string userLogin);

        [OperationContract]
        List<ProjectCSAT> CSATList(int? projectId);

        [OperationContract]
        List<Project> CSATSummaryReport(DateTime startDate, DateTime endDate, string practiceIds, string accountIds, bool isExport = false);

        [OperationContract]
        List<int> CSATReportHeader(DateTime startDate, DateTime endDate, string practiceIds, string accountIds);

        [OperationContract]
        List<Attribution> GetProjectAttributionValues(int projectId);

        [OperationContract]
        void SetProjectAttributionValues(int projectId, string attributionXML, string userLogin);

        [OperationContract]
        List<Project> GetAttributionForGivenIds(string attributionIds);

        [OperationContract]
        List<ProjectFeedback> GetProjectFeedbackByProjectId(int projectId);

        [OperationContract]
        List<ProjectFeedbackStatus> GetAllFeedbackStatuses();

        [OperationContract]
        void SaveFeedbackCancelationDetails(int feedbackId, int? statusId, bool isCanceled, string cancelationReason, string userLogin, bool sendReactivationMail);

        [OperationContract]
        bool CheckIfFeedbackExists(int? milestonePersonId, int? milestoneId, DateTime? startDate, DateTime? endDate);

        [OperationContract]
        bool CheckIfProjectNumberExists(string projectNumber);

        [OperationContract]
        Project ProjectGetShortById(int projectId);

        [OperationContract]
        List<Project> PersonsByProjectReport(string accountIds, string payTypeIds, string personStatusIds, string practices, string projectStatusIds, bool excludeInternal);

        [OperationContract]
        List<Offering> GetProjectOfferingList();

        [OperationContract]
        List<Revenue> GetProjectRevenueTypeList();

        [OperationContract]
        List<Channel> GetChannelList();

        [OperationContract]
        Channel GetChannelById(int channelId);

        [OperationContract]
        List<ProjectDivision> GetProjectDivisions();

        [OperationContract]
        List<Project> GetProjectsForClients(string clientIds);

    }
}

