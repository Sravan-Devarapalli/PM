﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel.Activation;
using System.Text;
using System.Web;
using System.Xml;
using DataAccess;
using DataAccess.Other;
using DataTransferObjects;
using DataTransferObjects.ContextObjects;
using DataTransferObjects.TimeEntry;

namespace PracticeManagementService
{
    // NOTE: If you change the class name "ProjectService" here, you must also update the reference to "ProjectService" in Web.config.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [System.Web.Script.Services.ScriptService()]
    public class ProjectService : IProjectService
    {
        #region IProjectService Members

        public Project ProjectGetById(int projectId)
        {
            try
            {
                var project = ProjectDAL.GetById(projectId);
                return project;
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "ProjectGetById", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public ComputedFinancials GetProjectsComputedFinancials(int projectId)
        {
            try
            {
                return ComputedFinancialsDAL.FinancialsGetByProject(projectId);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetProjectsComputedFinancials", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public DataSet GetProjectMilestonesFinancials(int projectId)
        {
            try
            {
                return ProjectDAL.GetProjectMilestonesFinancials(projectId);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetProjectMilestonesFinancials", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        /// <summary>
        /// Enlists number of requested projects by client.
        /// </summary>
        public int ProjectCountByClient(int clientId)
        {
            try
            {
                return ProjectDAL.ProjectCountByClient(clientId);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "ProjectCountByClient", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> ListProjectsByClient(int? clientId, string viewerUsername)
        {
            try
            {
                return clientId != null ? ProjectDAL.ProjectListByClient(clientId.Value, viewerUsername) : null;
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "ListProjectsByClient", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> ListProjectsByClientShort(int? clientId, bool IsOnlyActiveAndProjective, bool IsOnlyActiveAndInternal, bool IsOnlyEnternalProjects)
        {
            try
            {
                return clientId != null ? ProjectDAL.ListProjectsByClientShort(clientId.Value, IsOnlyActiveAndProjective, IsOnlyActiveAndInternal, IsOnlyEnternalProjects) : null;
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "ListProjectsByClientShort", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> ListProjectsByClientAndPersonInPeriod(int clientId, bool isOnlyActiveAndInternal, bool isOnlyEnternalProjects, int personId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return ProjectDAL.ListProjectsByClientAndPersonInPeriod(clientId, isOnlyActiveAndInternal, isOnlyEnternalProjects, personId, startDate, endDate);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "ListProjectsByClientAndPersonInPeriod", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> ListProjectsByClientWithSort(int? clientId, string viewerUsername, string sortBy)
        {
            try
            {
                return clientId != null ? ProjectDAL.ProjectListByClientWithSorting(clientId.Value, viewerUsername, sortBy) : null;
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "ListProjectsByClientWithSort", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public int CloneProject(ProjectCloningContext context)
        {
            try
            {
                return ProjectDAL.CloneProject(context);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "CloneProject", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        /// <summary>
        /// Enlists the requested projects.
        /// </summary>
        public List<Project> GetProjectListCustom(bool projected, bool completed, bool active, bool experimantal, bool proposed)
        {
            try
            {
                return ProjectDAL.ProjectListAll(
                projected,
                completed,
                active,
                experimantal,
                proposed,
                false);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetProjectListCustom", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

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
        public List<Project> ProjectListAllMultiParameters(
            string clientIds,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showInternal,
            bool showExperimental,
            bool showProposed,
            bool showInactive,
            bool showAtRisk,
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
            bool getFinancialsFromCache)
        {
            try
            {
                List<Project> result =
               ProjectRateCalculator.GetProjectListMultiParameters(
                   clientIds,
                   showProjected,
                   showCompleted,
                   showActive,
                   showInternal,
                   showExperimental,
                   showProposed,
                   showInactive,
                   showAtRisk,
                   periodStart,
                   periodEnd,
                   salespersonIdsList,
                   projectOwnerIdsList,
                   practiceIdsList,
                   divisionIdsList,
                   channelIdsList,
                   revenueTypeIdsList,
                   offeringIdsList,
                   projectGroupIdsList,
                   includeCurentYearFinancials,
                   excludeInternalPractices,
                   userLogin,
                   useActuals,
                   getFinancialsFromCache);

                return result;
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, !getFinancialsFromCache ? "ProjectListAllMultiParameters" : "ProjectListAllMultiParametersFromCache", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public bool IsProjectSummaryCachedToday()
        {
            try
            {
                return ProjectDAL.IsProjectSummaryCachedToday();
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "IsProjectSummaryCachedToday", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> GetProjectListByDateRange(
                                                    bool showProjected,
                                                    bool showCompleted,
                                                    bool showActive,
                                                    bool showInternal,
                                                    bool showExperimental,
                                                    bool showInactive,
                                                    DateTime periodStart,
                                                    DateTime periodEnd
                                                    )
        {
            try
            {
                List<Project> result =
               ProjectDAL.GetProjectListByDateRange(
                                                   showProjected,
                                                   showCompleted,
                                                   showActive,
                                                   showInternal,
                                                   showExperimental,
                                                   showInactive,
                                                   periodStart,
                                                   periodEnd
                                                   );

                return result;
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetProjectListByDateRange", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> GetProjectListWithFinancials(
            string clientIds,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showInternal,
            bool showExperimental,
            bool showInactive,
            bool showAtRisk,
            DateTime periodStart,
            DateTime periodEnd,
            string salespersonIdsList,
            string projectOwnerIdsList,
            string practiceIdsList,
            string projectGroupIdsList,
            bool excludeInternalPractices
            )
        {
            try
            {
                List<Project> result =
                 ProjectDAL.GetProjectListWithFinancials(
                  clientIds,
              showProjected,
              showCompleted,
              showActive,
              showInternal,
              showExperimental,
              showInactive,
              showAtRisk,
              periodStart,
              periodEnd,
              salespersonIdsList,
              projectOwnerIdsList,
              practiceIdsList,
              projectGroupIdsList,
              excludeInternalPractices);
                return result;
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetProjectListWithFinancials", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<MilestonePerson> GetProjectListGroupByPracticeManagers(
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
            )
        {
            try
            {
                return ProjectDAL.GetProjectListGroupByPracticeManagers(clientIds,
                                                                    showProjected,
                                                                    showCompleted,
                                                                    showActive,
                                                                    showInternal,
                                                                    showExperimental,
                                                                    showProposed,
                                                                    showInactive,
                                                                    periodStart,
                                                                    periodEnd,
                                                                    salespersonIdsList,
                                                                    projectOwnerIdsList,
                                                                    practiceIdsList,
                                                                    projectGroupIdsList,
                                                                    excludeInternalPractices
                                                                    );
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetProjectListGroupByPracticeManagers", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> GetBenchList(BenchReportContext context)
        {
            try
            {
                return ProjectRateCalculator.GetBenchAndAdmin(context);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetBenchList", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> GetBenchListWithoutBenchTotalAndAdminCosts(BenchReportContext context)
        {
            try
            {
                return ProjectRateCalculator.GetBenchListWithoutBenchTotalAndAdminCosts(context);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetBenchListWithoutBenchTotalAndAdminCosts", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        /// <summary>
        /// Retrives a list of the projects by the specified conditions.
        /// </summary>
        /// <param name="looked">A text to be looked for.</param>
        /// <param name="personId"></param>
        /// <returns>A list of the <see cref="Project"/> objects.</returns>
        public List<Project> ProjectSearchText(
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
             bool showAtRisk,
            string salespersonIdsList,
            string projectManagerIdsList,
            string practiceIdsList,
            string divisionIdsList,
            string channelIdsList,
            string revenueTypeIdsList,
            string offeringIdsList,
            string projectGroupIdsList)
        {
            try
            {
                return ProjectDAL.ProjectSearchText(
                    looked,
                    personId,
                    clientIdsList,
                    showProjected,
                    showCompleted,
                    showActive,
                    showInternal,
                    showExperimental,
                    showProposed,
                    showInactive,
                    showAtRisk,
                    salespersonIdsList,
                    projectManagerIdsList,
                    practiceIdsList,
                    divisionIdsList,
                    channelIdsList,
                    revenueTypeIdsList,
                    offeringIdsList,
                    projectGroupIdsList);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "ProjectSearchText", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public Project GetProjectDetailWithoutMilestones(int projectId, string userName)
        {
            try
            {
                return ProjectRateCalculator.GetProjectDetail(projectId, userName);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetProjectDetailWithoutMilestones", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        /// <summary>
        /// Saves the <see cref="Project"/> data into the database.
        /// </summary>
        /// <param name="project">The <see cref="Project"/> to be saved.</param>
        /// <param name="userName">A current user.</param>
        /// <returns>An ID of the saved project.</returns>
        public int SaveProjectDetail(Project project, string userName)
        {
            try
            {
                using (var connection = new SqlConnection(DataSourceHelper.DataConnection))
                {
                    connection.Open();
                    SqlTransaction currentTransaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                    if (!project.Id.HasValue)
                    {
                        ProjectDAL.InsertProject(project, userName, connection, currentTransaction);
                    }
                    else
                    {
                        ProjectDAL.UpdateProject(project, userName, connection, currentTransaction);
                    }

                    //Save ProjectTimetypes
                    if (!String.IsNullOrEmpty(project.ProjectWorkTypesList))
                    {
                        try
                        {
                            if (project.Id != null)
                                ProjectDAL.SetProjectTimeTypes(project.Id.Value, project.ProjectWorkTypesList, connection, currentTransaction);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("Time has already been entered for the following Work Type(s). The Work Type(s) cannot be unassigned from this project.") ||
                            ex.InnerException.Message.Contains("Time has already been entered for the following Work Type(s). The Work Type(s) cannot be unassigned from this project."))
                            {
                                var message = ex.Message.Contains("Time has already been entered for the following Work Type(s). The Work Type(s) cannot be unassigned from this project.") ? ex.Message : ex.InnerException.Message;
                                currentTransaction.Rollback();
                                throw new Exception(message);
                            }
                            throw ex;
                        }
                    }

                    currentTransaction.Commit();
                }

                if (project.Id != null) return project.Id.Value;
            }
            catch (Exception e)
            {
                throw e;
            }
            return -1;
        }

        public void SaveInternalProject(string projectName, string projectNumberSeries, int divisionId, int practiceId)
        {
            ProjectDAL.SaveInternalProject(projectName, projectNumberSeries, divisionId, practiceId);
        }

        /// <summary>
        /// Provides an info for the month mini report.
        /// </summary>
        /// <param name="month">The month to the data be provided for.</param>
        /// <param name="userName">The user (by email) to retrive the result for.</param>
        /// <returns>A well-formed XML document with the report data.</returns>
        public string MonthMiniReport(
            DateTime month,
            string userName,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showExperimental,
            bool showProposed,
            bool showInternal,
            bool showInactive,
            bool useActuals)
        {
            try
            {
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));

                var res = PersonStartsReport(monthStart, monthEnd, userName, null, null, showProjected, showCompleted, showActive, showExperimental, showProposed, showInternal, showInactive, useActuals);

                if (res.Count != 1)
                {
                    throw new Exception("'PersonStartsReport' method must return just one 'PesonStat' element for the month mini report");
                }

                var result = new StringBuilder();
                using (var writer = XmlWriter.Create(result))
                {
                    writer.WriteStartElement("Report");
                    writer.WriteAttributeString("Date", month.ToString("yyyy-MM-ddTHH:mm:ss"));
                    writer.WriteAttributeString("Revenue", res[0].Revenue.Value.ToString());
                    writer.WriteAttributeString("EmployeesCount", res[0].EmployeesCount.ToString());
                    writer.WriteAttributeString("ConsultantsCount", res[0].ConsultantsCount.ToString());
                }

                return result.ToString();
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "MonthMiniReport", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        /// <summary>
        /// Retrives the data for the person stats report.
        /// </summary>
        /// <param name="startDate">The period start.</param>
        /// <param name="endDate">The period end.</param>
        /// <param name="userName">The user (by email) to retrive the result for.</param>
        /// <param name="salespersonId">Determines an ID of the salesperson to filter the list for.</param>
        /// <param name="practiceManagerId">Determines an ID of the practice manager to filter the list for.</param>
        /// <returns>The list of the <see cref="PersonStats"/> objects.</returns>
        public List<PersonStats> PersonStartsReport(DateTime startDate,
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
            bool useActuals)
        {
            try
            {
                // ProjectRateCalculator.VerifyPrivileges(userName, ref salespersonId, ref practiceManagerId);
                var result
                    = ComputedFinancialsDAL.PersonStatsByDateRange(
                        startDate, //startDate.AddMonths(-1),
                        endDate,
                        salespersonId,
                        practiceManagerId,
                        showProjected,
                        showCompleted,
                        showActive,
                        showExperimental,
                        showProposed,
                        showInternal,
                        showInactive,
                        useActuals);

                for (var i = 1; i < result.Count; i++)
                {
                    var prevVirtualConsultants = result[i - 1].VirtualConsultants;
                    result[i].VirtualConsultantsChange = result[i].VirtualConsultants - prevVirtualConsultants;
                }

                // The data for a previous month was retrived to calculate the Virtual Consultants Change
                if (result.Count > 0 && result[0].Date < startDate)
                {
                    result.RemoveAt(0);
                }

                // Admin costs
                var allAdmin = ProjectRateCalculator.GetAdminCosts(startDate, endDate, userName);
                if (allAdmin != null)
                    foreach (var stats in result)
                    {
                        PersonStats stats1 = stats;
                        foreach (var financials in allAdmin.ProjectedFinancialsByMonth)
                        {
                            if (stats1.Date.Month == financials.Key.Month && stats1.Date.Year == financials.Key.Year) stats.AdminCosts = financials.Value.Cogs;
                        }
                    }

                return result;
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "PersonStartsReport", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public int? GetProjectId(string projectNumber)
        {
            try
            {
                return ProjectDAL.GetProjectId(projectNumber);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetProjectId", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public Project GetProjectShortByProjectNumber(string projectNumber, int? milestoneId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                return ProjectDAL.GetProjectShortByProjectNumber(projectNumber, milestoneId, startDate, endDate);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DateTime GetProjectLastChangeDateFortheGivenStatus(int projectId, int projectStatusId)
        {
            return ProjectDAL.GetProjectLastChangeDateFortheGivenStatus(projectId, projectStatusId);
        }

        public List<ProjectsGroupedByPerson> PersonBudgetListByYear(int year, BudgetCategoryType categoryType)
        {
            try
            {
                return ProjectDAL.PersonBudgetListByYear(year, categoryType);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "PersonBudgetListByYear", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<ProjectsGroupedByPractice> PracticeBudgetListByYear(int year)
        {
            try
            {
                return ProjectDAL.PracticeBudgetListByYear(year);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "PracticeBudgetListByYear", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public void CategoryItemBudgetSave(int itemId, BudgetCategoryType categoryType, DateTime monthStartDate, PracticeManagementCurrency amount)
        {
            try
            {
                ProjectDAL.CategoryItemBudgetSave(itemId, categoryType, monthStartDate, amount);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "CategoryItemBudgetSave", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<ProjectsGroupedByPerson> CalculateBudgetForPersons(
            DateTime startDate,
            DateTime endDate,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showInternal,
            bool showExperimental,
            bool showProposed,
            bool showInactive,
            bool showAtRisk,
            string practiceIdsList,
            bool excludeInternalPractices,
            string personIds,
            BudgetCategoryType categoryType)
        {
            try
            {
                return ProjectDAL.CalculateBudgetForPersons(
                                                             startDate,
                                                             endDate,
                                                             showProjected,
                                                             showCompleted,
                                                             showActive,
                                                             showInternal,
                                                             showExperimental,
                                                             showProposed,
                                                             showInactive,
                                                             showAtRisk,
                                                             practiceIdsList,
                                                             excludeInternalPractices,
                                                             personIds,
                                                             categoryType);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "CalculateBudgetForPersons", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<ProjectsGroupedByPractice> CalculateBudgetForPractices
            (DateTime startDate,
             DateTime endDate,
            bool showProjected,
            bool showCompleted,
            bool showActive,
            bool showInternal,
            bool showExperimental,
            bool showProposed,
            bool showInactive,
            bool showAtRisk,
            string practiceIdsList,
            bool excludeInternalPractices)
        {
            try
            {
                return ProjectDAL.CalculateBudgetForPractices(
                                                          startDate,
                                                          endDate,
                                                          showProjected,
                                                          showCompleted,
                                                          showActive,
                                                          showInternal,
                                                          showExperimental,
                                                          showProposed,
                                                          showInactive,
                                                          showAtRisk,
                                                          practiceIdsList,
                                                          excludeInternalPractices);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "CalculateBudgetForPractices", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public void CategoryItemsSaveFromXML(List<CategoryItemBudget> categoryItems, int year)
        {
            try
            {
                ProjectDAL.CategoryItemsSaveFromXML(categoryItems, year);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "CategoryItemsSaveFromXML", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public void ProjectDelete(int projectId, string userName)
        {
            try
            {
                ProjectDAL.ProjectDelete(projectId, userName);//It will delete only Inactive and Experimental Projects as per #2702.
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ProjectExpense[] GetProjectExpensesForProject(ProjectExpense entity)
        {
            try
            {
                return (new ProjectExpenseDal()).GetForProject(entity);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "GetProjectExpensesForProject", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> AllProjectsWithFinancialTotalsAndPersons()
        {
            try
            {
                ProjectDAL.InsertTodayProjectsIntoCache();

                var projectsList = ProjectDAL.ProjectsAll();

                ComputedFinancialsDAL.LoadTotalFinancialsPeriodForProjectsFromCache(projectsList, null, null);

                MilestonePersonDAL.LoadMilestonePersonListForProject(projectsList);

                return projectsList;
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "AllProjectsWithFinancialTotalsAndPersons", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public bool IsUserHasPermissionOnProject(string user, int id, bool isProjectId)
        {
            try
            {
                return ProjectDAL.IsUserHasPermissionOnProject(user, id, isProjectId);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "IsUserHasPermissionOnProject", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public bool IsUserIsOwnerOfProject(string user, int id, bool isProjectId)
        {
            try
            {
                return ProjectDAL.IsUserIsOwnerOfProject(user, id, isProjectId);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "IsUserHasPermissionOnProject", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public bool IsUserIsProjectOwner(string user, int id)
        {
            try
            {
                return ProjectDAL.IsUserIsProjectOwner(user, id);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "IsUserIsProjectOwner", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> GetProjectsListByProjectGroupId(int projectGroupId, bool isInternal, int personId, DateTime startDate, DateTime endDate)
        {
            return ProjectDAL.GetProjectsListByProjectGroupId(projectGroupId, isInternal, personId, startDate, endDate);
        }

        public List<Project> GetBusinessDevelopmentProject()
        {
            return ProjectDAL.GetBusinessDevelopmentProject();
        }

        public Project GetProjectByIdShort(int projectId)
        {
            return ProjectDAL.GetProjectByIdShort(projectId);
        }

        public List<TimeTypeRecord> GetTimeTypesByProjectId(int projectId, bool IsOnlyActive, DateTime? startDate, DateTime? endDate)
        {
            return ProjectDAL.GetTimeTypesByProjectId(projectId, IsOnlyActive, startDate, endDate);
        }

        public void SetProjectTimeTypes(int projectId, string projectTimeTypesList)
        {
            ProjectDAL.SetProjectTimeTypes(projectId, projectTimeTypesList, null, null);
        }

        public Dictionary<DateTime, bool> GetIsHourlyRevenueByPeriod(int projectId, int personId, DateTime startDate, DateTime endDate)
        {
            return ProjectDAL.GetIsHourlyRevenueByPeriod(projectId, personId, startDate, endDate);
        }

        public List<TimeTypeRecord> GetTimeTypesInUseDetailsByProject(int projectId, string timeTypeIds)
        {
            return ProjectDAL.GetTimeTypesInUseDetailsByProject(projectId, timeTypeIds);
        }

        public string AttachOpportunityToProject(int projectId, int opportunityId, string userLogin, int? pricingListId, bool link)
        {
            return ProjectDAL.AttachOpportunityToProject(projectId, opportunityId, userLogin, pricingListId, link);
        }

        public int CSATInsert(ProjectCSAT projectCSAT, string userLogin)
        {
            return ProjectCSATDAL.CSATInsert(projectCSAT, userLogin);
        }

        public void CSATDelete(int projectCSATId, string userLogin)
        {
            ProjectCSATDAL.CSATDelete(projectCSATId, userLogin);
        }

        public void CSATUpdate(ProjectCSAT projectCSAT, string userLogin)
        {
            ProjectCSATDAL.CSATUpdate(projectCSAT, userLogin);
        }

        public List<ProjectCSAT> CSATList(int? projectId)
        {
            return ProjectCSATDAL.CSATList(projectId);
        }

        public List<Project> CSATSummaryReport(DateTime startDate, DateTime endDate, string practiceIds, string accountIds, bool isExport)
        {
            return ProjectCSATDAL.CSATSummaryReport(startDate, endDate, practiceIds, accountIds, isExport);
        }

        public List<int> CSATReportHeader(DateTime startDate, DateTime endDate, string practiceIds, string accountIds)
        {
            return ProjectCSATDAL.CSATReportHeader(startDate, endDate, practiceIds, accountIds);
        }

        public List<Attribution> GetProjectAttributionValues(int projectId)
        {
            return ProjectDAL.GetProjectAttributionValues(projectId);
        }

        public void SetProjectAttributionValues(int projectId, string attributionXML, string userLogin)
        {
            try
            {
                ProjectDAL.SetProjectAttributionValues(projectId, attributionXML, userLogin);
            }
            catch (Exception e)
            {
                string logData = string.Format(Constants.Formatting.ErrorLogMessage, "SetProjectAttributionValues", "ProjectService.svc", string.Empty,
                    HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }

        public List<Project> GetAttributionForGivenIds(string attributionIds)
        {
            return ProjectDAL.GetAttributionForGivenIds(attributionIds);
        }

        public List<ProjectFeedback> GetProjectFeedbackByProjectId(int projectId)
        {
            return ProjectDAL.GetProjectFeedbackByProjectId(projectId);
        }

        public List<ProjectFeedbackStatus> GetAllFeedbackStatuses()
        {
            return ProjectDAL.GetAllFeedbackStatuses();
        }

        public void SaveFeedbackCancelationDetails(int feedbackId, int? statusId, bool isCanceled, string cancelationReason, string userLogin, bool sendReactivationMail)
        {
            ProjectDAL.SaveFeedbackCancelationDetails(feedbackId, statusId, isCanceled, cancelationReason, userLogin);
            if (sendReactivationMail)
                SendReactivationMail((int?)feedbackId);
        }

        public bool CheckIfFeedbackExists(int? milestonePersonId, int? milestoneId, DateTime? startDate, DateTime? endDate)
        {
            return ProjectDAL.CheckIfFeedbackExists(milestonePersonId, milestoneId, startDate, endDate);
        }

        public void SendReactivationMail(int? feedbackId)
        {
            var feedbacks = ProjectDAL.GetPersonsForIntialMailForProjectFeedback(feedbackId);
            var now = DateTime.Now;
            if (feedbacks.Count > 0 && now.Date >= feedbacks[0].Resources[0].ReviewEndDate.Date)
            {
                MailUtil.SendProjectFeedbackInitialMailNotification(feedbacks[0]);
            }
        }

        public bool CheckIfProjectNumberExists(string projectNumber)
        {
            return ProjectDAL.CheckIfProjectNumberExists(projectNumber);
        }

        public Project ProjectGetShortById(int projectId)
        {
            return ProjectDAL.ProjectGetShortById(projectId);
        }

        public List<Project> PersonsByProjectReport(string accountIds, string payTypeIds, string personStatusIds, string practices, string projectStatusIds, bool excludeInternal)
        {
            return ProjectDAL.PersonsByProjectReport(accountIds, payTypeIds, personStatusIds, practices, projectStatusIds, excludeInternal);
        }

        public List<Offering> GetProjectOfferingList()
        {
            return ProjectDAL.GetProjectOfferingList();
        }

        public List<Revenue> GetProjectRevenueTypeList()
        {
            return ProjectDAL.GetProjectRevenueTypeList();
        }

        public List<Channel> GetChannelList()
        {
            return ProjectDAL.GetChannelList();
        }

        public Channel GetChannelById(int channelId)
        {
            return ProjectDAL.GetChannelById(channelId);
        }

        public List<ProjectDivision> GetProjectDivisions()
        {
            return ProjectDAL.GetProjectDivisions();
        }

        public List<Project> GetProjectsForClients(string clientIds)
        {
            return ProjectDAL.GetProjectsForClients(clientIds);
        }

        #endregion IProjectService Members
    }
}

