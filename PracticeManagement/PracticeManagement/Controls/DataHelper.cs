﻿#region using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;
using DataTransferObjects;
using DataTransferObjects.ContextObjects;
using DataTransferObjects.Reports;
using PraticeManagement.ActivityLogService;
using PraticeManagement.CalendarService;
using PraticeManagement.ClientService;
using PraticeManagement.ConfigurationService;
using PraticeManagement.Controls.Opportunities;
using PraticeManagement.Controls.Reports;
using PraticeManagement.ExpenseCategoryService;
using PraticeManagement.ExpenseService;
using PraticeManagement.MilestonePersonService;
using PraticeManagement.MilestoneService;
using PraticeManagement.Objects;
using PraticeManagement.OpportunityService;
using PraticeManagement.OverheadService;
using PraticeManagement.PersonRoleService;
using PraticeManagement.PersonService;
using PraticeManagement.PersonStatusService;
using PraticeManagement.PracticeService;
using PraticeManagement.ProjectGroupService;
using PraticeManagement.ProjectService;
using PraticeManagement.ProjectStatusService;
using PraticeManagement.ReportService;
using PraticeManagement.TimeEntryService;
using PraticeManagement.TimescaleService;
using PraticeManagement.Utils;
using PraticeManagement.VendorService;

#endregion using

namespace PraticeManagement.Controls
{
    public class NameValuePair : IIdNameObject
    {
        public int? Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Provides commonly used operations with the data.
    /// </summary>
    public static class DataHelper
    {
        #region Constants

        private const string DefaultIdFieldName = "Id";
        private const string DefaultNameFieldName = "Name";
        private const string DefaultNameFieldEndodedName = "HtmlEncodedName";
        private const string CurrentPersonKey = "CurrentPerson";
        private const string CommaSeperatedFormat = "{0},{1}";
        private const string CommaSeperatedTitles = "{0},{1},{2}";
        private const string UnassignedText = "Unassigned";
        private const string PersonLastFirstNameText = "PersonLastFirstName";
        private const string ItalicStyleText = "font-style: italic";

        #endregion Constants

        public static Opportunity[] GetOpportunitiesPrevNext(int? opportunityId)
        {
            if (opportunityId.HasValue)
            {
                var opportunities = GetFilteredOpportunities();

                var oppCount = opportunities.Length;
                var currentOppIndex = Array.FindIndex(opportunities, opp => opp.Id.Value == opportunityId.Value);

                if (currentOppIndex >= 0)
                {
                    if (oppCount > 2)
                        return new Opportunity[]
                        {
                            opportunities[(oppCount + currentOppIndex - 1) % oppCount],
                            opportunities[(oppCount + currentOppIndex + 1) % oppCount]
                        };

                    if (oppCount == 2)
                        return new Opportunity[]
                        {
                            opportunities[(oppCount + currentOppIndex + 1) % oppCount]
                        };
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a list of <see cref="Opportunity"></see> objects by the specified conditions.
        /// </summary>
        public static Opportunity[] GetFilteredOpportunities(OpportunityFilterSettings filter = null)
        {
            if (filter == null)
            {
                filter = SerializationHelper.DeserializeCookie("OpportunityList") as OpportunityFilterSettings ??
                      new OpportunityFilterSettings();
            }

            Opportunity[] opportunities = ServiceCallers.Custom.Opportunity(c => c.FilteredOpportunityListAll(filter.ShowActive, filter.ShowExperimental, filter.ShowInactive, filter.ShowLost, filter.ShowWon, filter.ClientIdsList, filter.OpportunityGroupIdsList, filter.OpportunityOwnerIdsList, filter.SalespersonIdsList));

            return SortOppotunities(opportunities).AsQueryable().ToArray();
        }

        public static Opportunity[] GetFilteredOpportunitiesForDiscussionReview2(bool isSortingNeed = true)
        {
            var opportunities =
                ServiceCallers.Custom.Opportunity(c => c.OpportunityListAll(new OpportunityListContext { IsDiscussionReview2 = true }
                    ));

            return isSortingNeed ? SortOppotunities(opportunities).AsQueryable().ToArray() : opportunities;
        }

        /// <summary>
        /// Gets a list of <see cref="Opportunity"></see> objects by the specified conditions.
        /// </summary>
        public static Opportunity[] GetOpportunitiesForTargetPerson(int? personId)
        {
            //var comp = new OpportunityComparer(Controls.Generic.OpportunityList.Filter);
            //Array.Sort(opportunities, comp);

            return personId.HasValue ?
                ServiceCallers.Custom.Opportunity(
                    c => c.OpportunityListAll(
                        new OpportunityListContext
                        {
                            TargetPersonId = personId.Value,
                            ActiveClientsOnly = false
                        }))
                        :
                        new Opportunity[] { };
        }

        public static Person CurrentPerson
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Items[CurrentPersonKey] == null)
                    {
                        using (var serviceClient = new PersonServiceClient())
                        {
                            try
                            {
                                HttpContext.Current.Items[CurrentPersonKey] =
                                    serviceClient.GetPersonByAlias(HttpContext.Current.User.Identity.Name);
                            }
                            catch (CommunicationException)
                            {
                                serviceClient.Abort();
                                throw;
                            }
                        }
                    }
                    return HttpContext.Current.Items[CurrentPersonKey] as Person;
                }
                return null;
            }
        }

        public static int GetPersonID(string personAlias)
        {
            if (string.IsNullOrEmpty(personAlias))
            {
                return 0;
            }
            else
            {
                Person person = ServiceCallers.Custom.Person(p => p.GetPersonByAlias(personAlias));
                return (int)person.Id;
            }
        }

        public static List<DatePoint> GetDatePointsForPerson(DateTime startDate, DateTime endDate, Person person)
        {
            CalendarItem[] calendar;
            using (var client = new CalendarServiceClient())
                calendar = client.GetPersonCalendar(startDate, endDate, person.Id, null);

            return calendar.Select(item => DatePoint.Create(item)).ToList();
        }

        public static Person GetPerson(int personId)
        {
            using (var client = new PersonServiceClient())
                return client.GetPersonDetail(personId);
        }

        public static Person GetPersonWithoutFinancials(int personId)
        {
            using (var client = new PersonServiceClient())
                return client.GetPersonById(personId);
        }

        public static Person GetPersonHireAndTerminationDate(int personId)
        {
            using (var client = new PersonServiceClient())
                return client.GetPersonHireAndTerminationDate(personId);
        }

        public static void SetNewDefaultManager(int personId)
        {
            using (var client = new PersonServiceClient())
            {
                client.SetAsDefaultManager(personId);
            }
        }

        /// <summary>
        /// Enlists number of requested projects by client.
        /// </summary>
        public static int ProjectCountByClient(int clientId)
        {
            using (var client = new ProjectServiceClient())
            {
                return client.ProjectCountByClient(clientId);
            }
        }

        private static int Comp(Triple<Person, int[], int> x, Triple<Person, int[], int> y)
        {
            return x.Third.CompareTo(y.Third);
        }

        /// <summary>
        /// Retrives consultans report: Person - load per range - avarage u%
        /// </summary>
        public static List<ConsultantUtilizationPerson> GetConsultantsWeeklyReport
            (DateTime startDate,
            int step,
            int duration,
            bool activePersons,
            bool projectedPersons,
            bool activeProjects,
            bool projectedProjects,
            bool experimentalProjects,
            bool internalProjects,
            bool proposedProjects,
            bool completedProjects,
            bool atRiskProjects,
            string timescaleIds,
            string practiceIdList,
            int avgUtil,
            int sortId,
            string sortDirection,
            bool excludeInternalPractices, int utilizationType, bool includeBadgeStatus, string divisionIds, bool isSampleReport = false)
        {
            var consultants =
                ReportsHelper.GetConsultantsTimelineReport(
                    startDate, duration, step, activePersons, projectedPersons,
                    activeProjects, projectedProjects, experimentalProjects, internalProjects, proposedProjects, completedProjects,atRiskProjects,
                    timescaleIds, practiceIdList, sortId, sortDirection, excludeInternalPractices, utilizationType, includeBadgeStatus, isSampleReport, divisionIds);

            return consultants.FindAll(Q => Q.AverageUtilization < avgUtil);
        }

        public static List<ConsultantUtilizationPerson> ConsultantUtilizationDailyByPerson
            (DateTime startDate,
            int duration,
            bool activeProjects,
            bool projectedProjects,
            bool internalProjects,
            bool experimentalProjects,
            bool proposedProjects,
            bool completedProjects,
            bool atRiskProjects,
            int personId)
        {
            var context = new ConsultantTimelineReportContext
            {
                Start = startDate,
                Period = duration,
                ActiveProjects = activeProjects,
                ProjectedProjects = projectedProjects,
                InternalProjects = internalProjects,
                ExperimentalProjects = experimentalProjects,
                ProposedProjects = proposedProjects,
                CompletedProjects = completedProjects,
                AtRiskProjects= atRiskProjects
            };

            var consultants = ServiceCallers.Custom.Person(
                client => client.ConsultantUtilizationDailyByPerson(personId, context));
            var consultantsList = new List<ConsultantUtilizationPerson>();
            if (consultants != null && consultants.Any())
                consultantsList.AddRange(consultants);

            return consultantsList;
        }

        public static List<DetailedUtilizationReportBaseItem> GetMilestonePersons(int personId, DateTime startDate, DateTime endDate, bool incActive, bool incProjected, bool incInternal, bool incExperimental, bool incProposed, bool incCompleted, bool incAtRisk, bool isCapacityMode = false)
        {
            var result = new List<DetailedUtilizationReportBaseItem>();

            var context = new ConsultantMilestonesContext
            {
                PersonId = personId,
                StartDate = startDate,
                EndDate = endDate,
                IncludeActiveProjects = incActive,
                IncludeProjectedProjects = incProjected,
                IncludeCompletedProjects = incCompleted,
                IncludeInternalProjects = incInternal,
                IncludeExperimentalProjects = incExperimental,
                IncludeProposedProjects = incProposed,
                IncludeAtRiskProjects= incAtRisk,
                IncludeInactiveProjects = false,
                IncludeDefaultMileStone = false
            };

            var personEntries =
                ServiceCallers.Custom.MilestonePerson(
                    client => client.GetConsultantMilestones(context));

            var opportTransition = ServiceCallers.Custom.Opportunity(
                client => client.GetOpportunityTransitionsByPerson(personId));

            foreach (var entry in personEntries)
                result.Add(new DetailedUtilizationReportMilestoneItem(startDate, endDate, entry, isCapacityMode));

            foreach (var transition in opportTransition)
                result.Add(new DetailedUtilizationReportOpportunityItem(startDate, endDate, transition, isCapacityMode));

            return result;
        }

        public static List<DetailedProjectReportItem> GetProjects(DateTime startDate, DateTime endDate)
        {
            var result = new List<DetailedProjectReportItem>();
            var projectsList = new List<Project>();
            using (var serviceClient = new ProjectServiceClient())
            {
                try
                {
                    projectsList = serviceClient.GetProjectListByDateRange(true, false, true, false, false, false, startDate, endDate).AsQueryable().ToList();
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }

            foreach (var project in projectsList)
                result.Add(new DetailedProjectReportItem(startDate, endDate, project));

            return result;
        }

        public static void InsertExportActivityLogMessage(string source)
        {
            using (var logClient = new ActivityLogServiceClient())
            {
                try
                {
                    Person cp = CurrentPerson;
                    logClient.ActivityLogInsert(Constants.ActityLog.TaskPerformedMessageId,
                                                String.Format(Constants.ActityLog.ExportLogMessage,
                                                              cp.LastName + ", " + cp.FirstName, source));
                }
                catch (CommunicationException)
                {
                    logClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns database version
        /// </summary>
        /// <returns>Returns database version</returns>
        public static string GetDatabaseVersion()
        {
            using (var client = new ActivityLogServiceClient())
            {
                try
                {
                    return client.GetDatabaseVersion();
                }
                catch
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Check's if there's compensation record covering milestone/
        /// See #886 for details.
        /// </summary>
        /// <param name="person">Person to check against</param>
        /// <returns>True if there's such record, false otherwise</returns>
        public static bool IsCompensationCoversMilestone(Person person)
        {
            return IsCompensationCoversMilestone(person, null, null);
        }

        /// <summary>
        /// Verifies whether a user has compensation at this moment
        /// </summary>
        /// <param name="personId">Id of the person</param>
        /// <returns>True if a person has active compensation, false otherwise</returns>
        public static bool CurrentPayExists(int personId)
        {
            return ServiceCallers.Custom.Person(c => c.CurrentPayExists(personId));
        }

        /// <summary>
        /// Check's if there's compensation record covering milestone/
        /// See #886 for details.
        /// </summary>
        /// <param name="person">Person to check against</param>
        /// <returns>True if there's such record, false otherwise</returns>
        public static bool IsCompensationCoversMilestone(Person person, DateTime? start, DateTime? end)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    return serviceClient.IsCompensationCoversMilestone(person, start, end);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Moves milestone
        /// </summary>
        /// <param name="shiftDays">Number of days to move milestone</param>
        /// <param name="selectedIdValue">Id of the milestone to move</param>
        /// <param name="moveFutureMilestones">Whether to move future milestones</param>
        public static List<MSBadge> ShiftMilestone(int shiftDays, int selectedIdValue, bool moveFutureMilestones)
        {
            using (var serviceClient = new MilestoneServiceClient())
            {
                try
                {
                    return serviceClient.MilestoneMove(selectedIdValue, shiftDays, moveFutureMilestones).ToList();
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Moves milestone
        /// </summary>
        /// <param name="shiftDays">Number of days to move milestone</param>
        /// <param name="selectedIdValue">Id of the milestone to move</param>
        public static void ShiftMilestoneEnd(int shiftDays, int milestonePersonId, int selectedIdValue)
        {
            using (var serviceClient = new MilestoneServiceClient())
            {
                try
                {
                    serviceClient.MilestoneMoveEnd(selectedIdValue, milestonePersonId, shiftDays);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of practices.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillPracticeList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    Practice[] practices = serviceClient.GetPracticeList();

                    FillListDefaultWithEncodedName(control, firstItemText, practices, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of practices.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillPracticeListOnlyActive(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    // Add separate SP for this later.
                    Practice[] practices = GetActivePractices(serviceClient.GetPracticeList());

                    FillListDefault(control, firstItemText, practices, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillPracticeListForDivsion(ListControl control, string firstItemText, int divisionId, bool isProject = false)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    Practice[] practices = serviceClient.GetPracticeListForDivision(divisionId, isProject);

                    FillListDefault(control, firstItemText, practices, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillOfferingsList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new ProjectServiceClient())
            {
                try
                {
                    Offering[] offerings = serviceClient.GetProjectOfferingList();
                    FillListDefault(control, firstItemText, offerings, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillRevenueTypeList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new ProjectServiceClient())
            {
                try
                {
                    Revenue[] revenue = serviceClient.GetProjectRevenueTypeList();
                    FillListDefault(control, firstItemText, revenue, false);
                    control.SelectedValue = revenue.First(r => r.IsDefault).Id.ToString();
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillChannelList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new ProjectServiceClient())
            {
                try
                {
                    Channel[] channels = serviceClient.GetChannelList();
                    FillListDefault(control, firstItemText, channels, false);
                    control.SelectedValue = channels.First(c => c.IsDefault).Id.ToString();
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillVendors(ListControl control, string firstItemText)
        {
            using (var vendorService = new VendorServiceClient())
            {
                try
                {
                    var vendors = vendorService.GetAllActiveVendors();
                    FillListDefault(control, firstItemText, vendors, false);
                }
                catch (CommunicationException)
                {
                    vendorService.Abort();
                    throw;
                }

            }
        }

        /// <summary>
        /// Fills the list control with the list of Timescale types.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="firstItemText"></param>
        public static void FillTimescaleList(ListControl control, string firstItemText, List<int> excludedPaytypes = null)
        {
            using (var serviceClient = new TimescaleServiceClient())
            {
                try
                {
                    Timescale[] Timescales = serviceClient.GetAll();
                    Timescales = excludedPaytypes != null ? Timescales.Where(p => !excludedPaytypes.Any(g => g == p.Id)).ToArray() : Timescales;
                    FillListDefault(control, firstItemText, Timescales, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillTimescaleList(ListControl control, string firstItemText, bool fillWithDescription)
        {
            using (var serviceClient = new TimescaleServiceClient())
            {
                try
                {
                    Timescale[] Timescales = serviceClient.GetAll();
                    var hourlyType = Timescales.First(t => t.Id == 3);
                    hourlyType.Name = "1099-Hourly";
                    FillListDefault(control, firstItemText, Timescales, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillTerminationReasonsList(ListControl control, string firstItemText)
        {
            var terminationReasons = Utils.SettingsHelper.GetTerminationReasonsList().Where(tr => tr.IsVisible == true).ToArray();
            FillListDefault(control, firstItemText, terminationReasons, false, DefaultIdFieldName, DefaultNameFieldName);
        }

        /// <summary>
        /// Fills the list control with the list of practices.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillPracticeWithOwnerList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    Practice[] practices = serviceClient.GetPracticeList();

                    FillListDefault(
                        control, firstItemText, practices, false,
                        DefaultIdFieldName, "PracticeWithOwner");
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of practices.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillPracticeWithOwnerListOnlyActive(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    // Create separate SP for this later.
                    Practice[] practices = GetActivePractices(serviceClient.GetPracticeList());

                    FillListDefault(
                        control, firstItemText, practices, false,
                        DefaultIdFieldName, "PracticeWithOwner");
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Select only active practices.
        /// </summary>
        /// <param name="practices"></param>
        /// <returns></returns>
        private static Practice[] GetActivePractices(Practice[] practices)
        {
            return practices.AsQueryable().Where(p => p.IsActive).ToArray();
        }

        /// <summary>
        /// Fills the list control with the list of practices.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillPracticeList(Person person, ListControl control, string firstItemText)
        {
            Practice[] practices = GetPractices(person);

            FillListDefaultWithEncodedName(control, firstItemText, practices, false);
        }

        public static Practice[] GetPractices(Person person)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    return serviceClient.PracticeListAll(person);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static Practice[] GetPracticeById(int? id)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    return serviceClient.PracticeGetById(id);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of active persons.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillPersonList(ListControl control, string firstItemText, string statusIds, bool fillWithPersonFirstLastName = false)
        {
            FillPersonList(control, firstItemText, DateTime.MinValue, DateTime.MinValue, statusIds, fillWithPersonFirstLastName);
        }

        /// <summary>
        /// Fills the list control with the list of active persons.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        /// <param name="startDate">mileStone start date</param>
        /// <param name="endDate">mileStone end date</param>
        public static void FillPersonList(ListControl control, string firstItemText, DateTime startDate, DateTime endDate, string statusIds, bool fillWithPersonFirstLastName = false, bool fillWithPersonPreferrdFirstName = false)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person[] persons = serviceClient.PersonListAllShort(null, statusIds, startDate, endDate);

                    Array.Sort(persons);

                    FillPersonList(control, firstItemText, persons, String.Empty, fillWithPersonFirstLastName, fillWithPersonPreferrdFirstName);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static IEnumerable<Person> GetAllPersons()
        {
            return ServiceCallers.Custom.Person(c => c.PersonListAllShort(null, null, DateTime.MinValue, DateTime.MaxValue));
        }

        public static void FillPersonListForImpersonate(ListControl control)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person[] persons = serviceClient.GetPersonListByStatusList("1,5", null);
                    control.Items.Clear();
                    if (persons.Length == 0)
                        control.Items.Add(new ListItem(Resources.Controls.NotAvailableText, null));
                    else
                        foreach (Person person in persons)
                        {
                            control.Items.Add(new ListItem(person.PersonLastFirstName, person.Alias));
                        }
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of active persons who are not in the Administration practice.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        /// <param name="startDate">mileStone start date</param>
        /// <param name="endDate">mileStone end date</param>
        public static void FillPersonListForMilestone(ListControl control, string firstItemText, int? milestonePersonId, DateTime startDate, DateTime endDate)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var persons = serviceClient.PersonListAllForMilestone(milestonePersonId, startDate, endDate);
                    // persons = persons.ToList().FindAll(item => item.HireDate <= startDate).ToArray();

                    Array.Sort(persons);

                    FillPersonList(control, firstItemText, persons, String.Empty);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of One-Off persons.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        /// <param name="today">A date today</param>
        public static void FillOneOffList(ListControl control, string firstItemText, DateTime today)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var persons = serviceClient.GetOneOffList(today);

                    Array.Sort(persons);
                    FillPersonListWithTitle(control, firstItemText, persons, "-1");
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillStrawManList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var persons = serviceClient.GetStrawmanListShortFilterWithTodayPay();

                    Array.Sort(persons);

                    FillPersonList(control, firstItemText, persons, "-1");
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of active recruiters.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        /// <param name="personId">An ID of the <see cref="Person"/> to fill the list for.</param>
        /// <param name="hireDate">A Hire Date of the person.</param>
        public static void FillRecruiterList(ListControl control, string firstItemText, bool fillPreferredFirstName = false)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person[] persons = serviceClient.GetRecruiterList();

                    FillPersonList(control, firstItemText, persons, String.Empty, false, fillPreferredFirstName);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of active salespersons.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        /// <param name="includeInactive">Determines whether inactive persons will are included into the results.</param>
        public static void FillSalespersonList(ListControl control, string firstItemText, bool includeInactive)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person[] persons = serviceClient.GetSalespersonList(includeInactive);

                    FillPersonList(control, firstItemText, persons, String.Empty);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of active persons having  Director Seniority.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="firstItemText"></param>

        public static void FillDirectorsList(ListControl control, string firstItemText, List<int> excludedPersons = null, bool fillPreferredFirstName = false)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    string statusids = (int)DataTransferObjects.PersonStatusType.Active + ", " + (int)DataTransferObjects.PersonStatusType.TerminationPending;
                    Person[] persons = serviceClient.PersonListShortByRoleAndStatus(statusids, DataTransferObjects.Constants.RoleNames.DirectorRoleName);
                    persons = excludedPersons != null ? persons.Where(p => !excludedPersons.Any(g => g == p.Id)).ToArray() : persons;
                    FillPersonList(control, firstItemText, persons, String.Empty, false, fillPreferredFirstName);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillCSATReviewerList(ListControl control, string firstItemText, List<int> excludedPersons)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    string statusids = (int)DataTransferObjects.PersonStatusType.Active + ", " + (int)DataTransferObjects.PersonStatusType.TerminationPending;
                    Person[] persons = serviceClient.PersonListShortByRoleAndStatus(statusids, DataTransferObjects.Constants.RoleNames.SeniorLeadershipRoleName);
                    persons = persons.Where(p => !excludedPersons.Any(g => g == p.Id)).ToArray();
                    FillPersonList(control, firstItemText, persons, String.Empty);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillSeniorManagerList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    string statusids = string.Format(CommaSeperatedFormat, (int)DataTransferObjects.PersonStatusType.Active, (int)DataTransferObjects.PersonStatusType.TerminationPending);
                    string titles = string.Format(CommaSeperatedTitles, DataTransferObjects.Constants.TitleNames.SeniorManagerTitleName, DataTransferObjects.Constants.TitleNames.DirectorTitleName, DataTransferObjects.Constants.TitleNames.SeniorDirectorTitleName);
                    var persons = serviceClient.PersonListShortByTitleAndStatus(statusids, titles);
                    persons = persons.Any() ? persons.OrderBy(p => p.PersonLastFirstName).ToArray() : persons;
                    List<Person> personlist = new List<Person>();
                    personlist.Add(new Person() { Id = -1, LastName = UnassignedText });
                    personlist.AddRange(persons);
                    FillListDefault(control, firstItemText, personlist.ToArray(), false, DefaultIdFieldName, PersonLastFirstNameText);
                    ListItem unasigned = control.Items.FindByValue("-1");
                    unasigned.Text = UnassignedText;
                    unasigned.Attributes["style"] = ItalicStyleText;
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillW2ActivePersons(ListControl control, string firstItemText, bool noFirstItem, bool selectItem)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    string statusIds = ((int)DataTransferObjects.PersonStatusType.Active).ToString();
                    string paytypeIds = ((int)TimescaleType.Salary).ToString();

                    var persons = ServiceCallers.Custom.Person(p => p.GetPersonsByPayTypesAndByStatusIds(statusIds, paytypeIds)).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToArray();

                    FillListDefault(control, firstItemText, persons, noFirstItem, "Id", "PersonLastFirstName");
                    if (selectItem)
                    {
                        if (persons.Any(p => p.Manager != null))
                            control.SelectedValue = persons.First(p => p.Manager != null).Manager.Id.ToString();
                        else
                        {

                            control.SelectedValue = "-1";
                        }
                    }
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillSalespersonListOnlyActive(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person[] persons = GetActivePersons(serviceClient.GetSalespersonList(false));

                    FillPersonList(control, firstItemText, persons, String.Empty);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillSalespersonListOnlyActiveForLoginPerson(ListControl control, Person person, string firstItemText)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person[] persons = GetActivePersons(serviceClient.PersonListSalesperson(person, false));

                    FillPersonList(control, firstItemText, persons, String.Empty);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        private static Person[] GetActivePersons(Person[] persons)
        {
            return persons.AsQueryable().Where(p => p.Status.Id == (int)PersonStatusType.Active || p.Status.Id == (int)PersonStatusType.TerminationPending).ToArray(); // Here Status.Id == 1 means only active person. (Not projected)
        }

        /// <summary>
        /// Fills the list control with the list of active salespersons.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        /// <param name="includeInactive">Determines whether inactive persons will are included into the results.</param>
        public static void FillSalespersonList(Person person, ListControl control, string firstItemText, bool includeInactive)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person[] persons = serviceClient.PersonListSalesperson(person, includeInactive);

                    FillPersonList(control, firstItemText, persons, String.Empty, false, true);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of active practice managers.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        /// <param name="endDate">An End Date of the project to the Practice Maneger be selected for.</param>
        /// <param name="includeInactive">Determines whether inactive persons will are included into the results.</param>
        public static void FillProjectOwnerList(ListControl control, string firstItemText, DateTime? endDate, bool includeInactive)
        {
            FillProjectOwnerList(control, firstItemText, includeInactive, null);
        }

        /// <summary>
        /// Fills the list control with the list of active practice managers.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        /// <param name="endDate">An End Date of the project to the Practice Maneger be selected for.</param>
        /// <param name="includeInactive">Determines whether inactive persons will are included into the results.</param>
        /// <param name="person">Person who requests the info</param>
        public static void FillProjectOwnerList(ListControl control, string firstItemText, bool includeInactive, Person person, bool fillPreferredFirstName = false)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var persons = serviceClient.PersonListProjectOwner(includeInactive, person);

                    FillPersonList(control, firstItemText, persons, String.Empty, false, fillPreferredFirstName);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillProjectManagersList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    string statusids = (int)DataTransferObjects.PersonStatusType.Active + ", " + (int)DataTransferObjects.PersonStatusType.TerminationPending;
                    Person[] persons = serviceClient.OwnerListAllShort(statusids);

                    FillListDefault(control, firstItemText, persons, false, "Id", "PersonLastFirstName");
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillUnlinkedOpportunityList(int? clientId, ListControl control, string firstItemText)
        {
            using (var serviceClient = new OpportunityService.OpportunityServiceClient())
            {
                try
                {
                    Opportunity[] opportunities = serviceClient.OpportunityListWithMinimumDetails(clientId, false);

                    FillListDefault(control, firstItemText, opportunities, false, "Id", "ClientOpportunity");
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillPersonList(ListControl control, string firstItemText, Person[] persons, string firstItemValue, bool fillWithPersonFirstLastName = false, bool fillWithPersonPreferrdFirstName = false)
        {
            control.Items.Clear();

            if (!string.IsNullOrEmpty(firstItemText))
            {
                var listitem = new ListItem() { Text = firstItemText, Value = firstItemValue };
                listitem.Attributes[Constants.Variables.IsStrawMan] = "false";
                control.Items.Add(listitem);
            }

            if (persons.Length > 0)
            {
                if (fillWithPersonPreferrdFirstName)
                {
                    persons = persons.OrderBy(p => p.IsStrawMan).ThenBy(p => p.PreferredOrFirstName).ToArray();
                }
                else
                {
                    if (fillWithPersonFirstLastName)
                    {
                        persons = persons.OrderBy(p => p.IsStrawMan).ThenBy(p => p.PersonFirstLastName).ToArray();
                    }
                    else
                    {
                        persons = persons.OrderBy(p => p.IsStrawMan).ThenBy(p => p.PersonLastFirstName).ToArray();
                    }
                }
                foreach (Person person in persons)
                {
                    var personitem = new ListItem();
                    personitem.Value = person.Id.Value.ToString();
                    if (fillWithPersonPreferrdFirstName)
                    {
                        personitem.Text = person.PreferredOrFirstName;
                    }
                    else
                    {
                        if (fillWithPersonFirstLastName)
                        {
                            personitem.Text = person.PersonFirstLastName;
                        }
                        else
                        {
                            personitem.Text = person.PersonLastFirstName;
                        }
                    }

                    personitem.Attributes[Constants.Variables.IsStrawMan] = person.IsStrawMan.ToString().ToLowerInvariant();
                    personitem.Attributes[Constants.Variables.OptionGroup] = person.IsStrawMan ? "Strawmen" : "Persons";

                    control.Items.Add(personitem);
                }
            }
        }

        public static void FillTimeEntryProjectList(ListControl control, string firstItemText, Project[] projects, string firstItemValue)
        {
            control.Items.Clear();

            if (!string.IsNullOrEmpty(firstItemText))
            {
                var listitem = new ListItem() { Text = firstItemText, Value = firstItemValue };
                control.Items.Add(listitem);
            }

            if (projects != null && projects.Length > 0)
            {
                foreach (Project project in projects)
                {
                    var projectitem = new ListItem(
                                          project.Name,
                                          project.Id.Value.ToString());

                    projectitem.Attributes[Constants.Variables.OptionGroup] = project.IsAssignedProject ? "Assigned to Me" : "All Projects";

                    control.Items.Add(projectitem);
                }
            }
        }

        public static void FillPersonListWithPersonFirstLastName(ListControl control, string firstItemText, string firstItemValue, string statusIds)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person[] persons = serviceClient.PersonListAllShort(null, statusIds, DateTime.MinValue, DateTime.MinValue);
                    control.Items.Clear();
                    if (!string.IsNullOrEmpty(firstItemText))
                    {
                        var listitem = new ListItem() { Text = firstItemText, Value = firstItemValue };
                        listitem.Attributes[Constants.Variables.IsStrawMan] = "false";
                        control.Items.Add(listitem);
                    }
                    if (persons.Length > 0)
                    {
                        persons = persons.OrderBy(p => p.IsStrawMan).ThenBy(p => p.FirstName).ThenBy(p => p.LastName).ToArray();
                        foreach (Person person in persons)
                        {
                            var personitem = new ListItem(
                                                  person.FirstName + ", " + person.LastName,
                                                  person.Id.Value.ToString());
                            personitem.Attributes[Constants.Variables.IsStrawMan] = person.IsStrawMan.ToString().ToLowerInvariant();
                            personitem.Attributes[Constants.Variables.OptionGroup] = person.IsStrawMan ? "Strawmen" : "Persons";
                            control.Items.Add(personitem);
                        }
                    }
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillPersonListWithTitle(ListControl control, string firstItemText, Person[] persons, string firstItemValue)
        {
            control.Items.Clear();
            if (!string.IsNullOrEmpty(firstItemText))
            {
                var listitem = new ListItem() { Text = firstItemText, Value = firstItemValue };
                listitem.Attributes[Constants.Variables.IsStrawMan] = "false";
                control.Items.Add(listitem);
            }
            if (persons.Length > 0)
            {
                persons = persons.OrderBy(p => p.IsStrawMan).ThenBy(p => p.PersonLastFirstName).ToArray();

                foreach (Person person in persons)
                {
                    var personitem = new ListItem();
                    personitem.Value = person.Id.Value.ToString();
                    var personTitle = person.Title.TitleName;
                    personitem.Attributes[Constants.Variables.IsStrawMan] = person.IsStrawMan.ToString().ToLowerInvariant();
                    personitem.Attributes[Constants.Variables.OptionGroup] = person.IsStrawMan ? "Strawmen" : "Persons";
                    personitem.Text = person.PersonLastFirstName + " (" + personTitle + ") ";
                    control.Items.Add(personitem);
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of registered overhead rate types.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillOverheadRateTypeList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new OverheadServiceClient())
            {
                try
                {
                    OverheadRateType[] types = serviceClient.GetRateTypes();

                    FillListDefault(control, firstItemText, types, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of project statuses.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillProjectStatusList(ListControl control, string firstItemText, List<int> excludedStatus = null, bool noFirstItem = false)
        {
            using (var serviceClient = new ProjectStatusServiceClient())
            {
                try
                {
                    ProjectStatus[] statuses = serviceClient.GetProjectStatuses();
                    statuses = excludedStatus != null ? statuses.Where(p => !excludedStatus.Any(g => g == p.Id)).ToArray() : statuses;
                    FillListDefault(control, firstItemText, statuses, noFirstItem);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of time entry projects.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillTimeEntryProjectsList(ListControl control, string firstItemText, int? selectedValue)
        {
            var projects = ServiceCallers.Custom.TimeEntry(c => c.GetAllTimeEntryProjects());
            FillCheckBoxList(control, firstItemText, selectedValue, projects, "-1");
        }

        /// <summary>
        /// Fills the list control with the list of time entry projects.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillTimeEntryPersonList(ListControl control, string firstItemText, int? selectedValue)
        {
            var persons = ServiceCallers.Custom.Person(c => c.PersonListAllShort(null, null, DateTime.MinValue, DateTime.MaxValue));
            FillCheckBoxList(control, firstItemText, selectedValue, persons, "-1");
        }

        public static void FillTimeEntryPersonList(ListControl control, string firstItemText, int? selectedValue, string personStatusIdsList, int? personId)
        {
            var persons = ServiceCallers.Custom.Person(c => c.GetPersonListByStatusList(personStatusIdsList, personId));
            FillCheckBoxList(control, firstItemText, selectedValue, persons, "-1");
        }

        public static void FillTimeEntryPersonListBetweenStartAndEndDates(ListControl control, string firstItemText, int? selectedValue, string personStatusIdsList, int? personId, DateTime? startDate, DateTime? endDate)
        {
            var persons = ServiceCallers.Custom.Person(c => c.GetPersonListByStatusList(personStatusIdsList, personId));
            if (startDate.HasValue && endDate.HasValue)
            {
                persons = persons.Where(p => p.HireDate <= endDate && (!p.TerminationDate.HasValue || p.TerminationDate.Value >= startDate)).ToArray();
            }
            FillCheckBoxList(control, firstItemText, selectedValue, persons, "-1");
        }

        /// <summary>
        /// Fills the list control with the list of persons.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="persons">Persons which needs to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillTimeEntryPersonList(ListControl control, string firstItemText, int? selectedValue, List<Person> persons)
        {
            FillCheckBoxList(control, firstItemText, selectedValue, persons.ToArray(), "-1");
        }

        private static void FillCheckBoxList<T>(ListControl control, string firstItemText, int? selectedValue, T[] items, string firstItemValue) where T : IIdNameObject
        {
            control.Items.Clear();
            control.Items.Add(new ListItem(firstItemText, firstItemValue));

            foreach (var project in items.OrderBy(p => p.Name))
            {
                var listitem = new ListItem(
                    project.ToString(),
                    project.Id.Value.ToString());

                control.Items.Add(listitem);

                if (selectedValue.HasValue && project.Id.Value == selectedValue)
                    listitem.Selected = true;
            }
        }

        public static string FormatDetailedProjectName(Project project)
        {
            return project.DetailedProjectTitle;
        }

        /// <summary>
        /// Fills the list control with the list of person roles.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillPersonRoleList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PersonRoleServiceClient())
            {
                try
                {
                    PersonRole[] roles = serviceClient.GetPersonRoles();

                    FillListDefault(control, firstItemText, roles, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of project statuses.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillPersonStatusList(ListControl control)
        {
            FillPersonStatusList(control, string.Empty, true);
        }

        /// <summary>
        /// Fills the list control with the list of project statuses and if noFirstItem is false then it will insert first item.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="firstItemText"></param>
        /// <param name="noFirstItem"></param>
        private static void FillPersonStatusList(ListControl control, string firstItemText, bool noFirstItem)
        {
            using (var serviceClient = new PersonStatusServiceClient())
            {
                try
                {
                    PersonStatus[] statuses = serviceClient.GetPersonStatuses();

                    FillListDefault(control, firstItemText, statuses, noFirstItem);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of project statuses and if firstItemText exists then it will keep first item.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillPersonStatusList(ListControl control, string firstItemText)
        {
            FillPersonStatusList(control, firstItemText, false);
        }

        /// <summary>
        /// Fills the list control with the list of expense categories.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillExpenseCategoriesList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new ExpenseCategoryServiceClient())
            {
                try
                {
                    ExpenseCategory[] categories = serviceClient.GetCategoryList();

                    FillListDefault(control, firstItemText, categories, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of expense bases.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillExpenseBasesList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new ExpenseServiceClient())
            {
                try
                {
                    ExpenseBasis[] bases = serviceClient.GetExpenseBasisList();

                    FillListDefault(control, firstItemText, bases, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of week paid options.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillWeekPaidOptionsList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new ExpenseServiceClient())
            {
                try
                {
                    WeekPaidOption[] bases = serviceClient.GetWeekPaidOptionList();

                    FillListDefault(control, firstItemText, bases, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of person's seniorities.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        public static void FillSenioritiesList(ListControl control, string firstItemText = null)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var seniorities = serviceClient.ListSeniorities();

                    FillListDefault(control, firstItemText, seniorities, firstItemText == null);
                    if (firstItemText == null)
                        control.SelectedIndex = control.Items.Count - 1;
                    else
                        control.SelectedIndex = 0;
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of person's titles.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        public static void FillTitleList(ListControl control, string firstItemText = null, bool isHtmlEncodedText = false, int titleTypeId = 0)
        {
            var titles = ServiceCallers.Custom.Title(t => t.GetAllTitles());
            if (titleTypeId != 0)
                titles = titles.Where(t => t.TitleType.TitleTypeId == titleTypeId).ToArray();

            control.Items.Clear();

            if (!string.IsNullOrEmpty(firstItemText))
            {
                var listitem = new ListItem() { Text = firstItemText, Value = "" };
                control.Items.Add(listitem);
            }

            if (titles.Length > 0)
            {
                foreach (Title title in titles)
                {
                    var titleitem = new ListItem();
                    titleitem.Value = title.TitleId.ToString();
                    titleitem.Text = (isHtmlEncodedText) ? title.HtmlEncodedTitleName : title.TitleName;
                    titleitem.Attributes[Constants.Variables.OptionGroup] = title.TitleType.TitleTypeName;
                    control.Items.Add(titleitem);
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of Domains.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        public static void FillDomainsList(ListControl control)
        {
            var domains = SettingsHelper.GetDomainsList().Select(p => new { Name = p, Id = p }).ToArray();
            FillListDefault(control, null, domains, true, "Id", "Name");
            control.SelectedIndex = 0;
        }

        /// <summary>
        /// Fills the list control with the list of person's seniorities.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        public static void FillSenioritiesListOrderByName(ListControl control, string firstItemText = null)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var seniorities = serviceClient.ListSeniorities();
                    seniorities = seniorities.OrderBy(p => p.Name).ToArray();

                    FillListDefault(control, firstItemText, seniorities, firstItemText == null);
                    if (firstItemText == null)
                        control.SelectedIndex = control.Items.Count - 1;
                    else
                        control.SelectedIndex = 0;
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillCohortAssignments(ListControl control, string firstItemText = null)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var cohorts = serviceClient.GetAllCohortAssignments();
                    cohorts = cohorts.OrderBy(p => p.Id).ToArray();

                    FillListDefault(control, firstItemText, cohorts, firstItemText == null);
                    if (firstItemText == null)
                        control.SelectedIndex = control.Items.Count - 1;
                    else
                        control.SelectedIndex = 0;
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillPricingLists(ListControl control, PricingList[] pricingList, string firstItemText = null, bool noFirstItem = false, string valueField = "PricingListId", string NameField = "Name")
        {
            pricingList = pricingList.Where(p => p.IsActive).ToArray();
            FillListDefault(control, "-- Select Pricing List --", pricingList, noFirstItem, valueField, NameField);
        }

        public static void FillBusinessTypes(ListControl control)
        {
            using (var serviceClient = new ClientServiceClient())
            {
                try
                {
                    var businessTypes = Enum.GetValues(typeof(BusinessType));

                    control.AppendDataBoundItems = true;
                    control.Items.Clear();

                    control.DataTextField = "Key";
                    control.DataValueField = "Value";

                    Dictionary<string, string> list = new Dictionary<string, string>();

                    foreach (BusinessType item in businessTypes)
                    {
                        string key = GetDescription(item);
                        string value = ((int)item).ToString();
                        if (value == "0") value = "";
                        list.Add(key, value);
                    }

                    control.DataSource = list;
                    control.DataBind();
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillProjectGroupList(ListControl control, int? clientId, int? projectId, string firstItemText = "-- Select Business Unit --", bool noFirstItem = false)
        {
            using (var serviceClient = new ProjectGroupServiceClient())
            {
                try
                {
                    ProjectGroup[] groups = serviceClient.GroupListAll(clientId, projectId);
                    groups = groups.AsQueryable().Where(g => g.IsActive).ToArray();

                    FillListDefault(control, firstItemText, groups, noFirstItem);

                    // control.Items.Insert(0, new ListItem(Resources.Controls.DefaultGroup, string.Empty));
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillProjectGroupListWithInactiveGroups(ListControl control, int? clientId, int? projectId, string firstItemText = null, bool noFirstItem = true)
        {
            using (var serviceClient = new ProjectGroupServiceClient())
            {
                try
                {
                    ProjectGroup[] groups = serviceClient.GroupListAll(clientId, projectId).OrderBy(g => g.Name).ToArray();
                    FillListDefaultWithEncodedName(control, firstItemText, groups, noFirstItem);

                    // control.Items.Insert(0, new ListItem(Resources.Controls.DefaultGroup, string.Empty));
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillBusinessUnitsByClients(ListControl control, string clientIds, string firstItemText = null, bool noFirstItem = true)
        {
            using (var serviceClient = new ClientServiceClient())
            {
                try
                {
                    ProjectGroup[] groups = serviceClient.GetBusinessUnitsForClients(clientIds).OrderBy(g => g.Client.Name).ThenBy(g => g.Name).ToArray();
                    FillListDefault(control, firstItemText, groups, noFirstItem, "Id", "ClientProjectGroupFormat");
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of active clients.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillClientList(ListControl control, string firstItemText)
        {
            FillClientListWithInactive(control, firstItemText, false);
        }

        /// <summary>
        /// Fills the client list control with the list of active clients and groups list control with corresponding groups
        /// </summary>
        /// <param name="clientList">Clients list control</param>
        /// <param name="groupList">Groups list control</param>
        public static void FillClientsAndGroups(CascadingMsdd clientList, ListControl groupList)
        {
            //  If current user is administrator, don't apply restrictions
            Person person =
                (Roles.IsUserInRole(
                    CurrentPerson.Alias,
                    DataTransferObjects.Constants.RoleNames.AdministratorRoleName)
                    || Roles.IsUserInRole(
                    CurrentPerson.Alias,
                    DataTransferObjects.Constants.RoleNames.OperationsRoleName))
                    ? null
                    : CurrentPerson;

            IEnumerable<Client> clients = GetAllClientsSecure(person, true);

            PrepareClientList(clientList, clients);
            PrepareGroupList(clientList, groupList, clients);
        }

        /// <summary>
        /// Fills the client list control with the list of active clients and groups list control with corresponding groups in person detail page.
        /// </summary>
        /// <param name="clientList">Clients list control</param>
        /// <param name="groupList">Groups list control</param>
        public static void FillClientsAndGroupsCheckBoxListInPersonDetailPage(CascadingMsdd clientList, ListControl groupList)
        {
            //  If current user is administrator, don't apply restrictions
            Person person =
                (
                Roles.IsUserInRole(CurrentPerson.Alias, DataTransferObjects.Constants.RoleNames.AdministratorRoleName) ||
                Roles.IsUserInRole(CurrentPerson.Alias, DataTransferObjects.Constants.RoleNames.HRRoleName) ||
                Roles.IsUserInRole(CurrentPerson.Alias, DataTransferObjects.Constants.RoleNames.OperationsRoleName)
                )
                    ? null
                    : CurrentPerson;

            IEnumerable<Client> clients = GetAllClientsSecure(person, false);

            PrepareClientList(clientList, clients);
            PrepareGroupList(clientList, groupList, clients);
        }

        public static IEnumerable<Person> GetPersonsInMilestone(Milestone milestone)
        {
            MilestonePersonServiceClient serviceClient = null;
            MilestonePerson[] milestonePersonList;
            using (serviceClient = new MilestonePersonServiceClient())
            {
                try
                {
                    milestonePersonList = serviceClient.GetMilestonePersonListByMilestone(milestone.Id.Value);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }

            if (milestonePersonList != null)
                foreach (MilestonePerson mp in milestonePersonList)
                    yield return mp.Person;
            else
                yield return null;
        }

        public static IEnumerable<Person> GetPersonsInMilestone(Project project)
        {
            MilestonePersonServiceClient serviceClient = null;
            MilestonePerson[] milestonePersonsList = null;
            using (serviceClient = new MilestonePersonServiceClient())
            {
                try
                {
                    milestonePersonsList = serviceClient.GetMilestonePersonListByProjectWithoutPay(project.Id.Value);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }

            if (milestonePersonsList != null)
            {
                var persons = new List<DataTransferObjects.Person>();
                foreach (MilestonePerson milestonePerson in milestonePersonsList)
                    persons.Add(milestonePerson.Person);
                return persons;
            }
            else
                return null;
        }

        public static Milestone GetMileStoneById(int mileStoneId)
        {
            MilestoneServiceClient serviceClient = null;
            using (serviceClient = new MilestoneServiceClient())
            {
                try
                {
                    return serviceClient.GetMilestoneById(mileStoneId);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Populates groups list and initializes dependencies between source and target controls
        /// </summary>
        /// <param name="clientList">Clients list control</param>
        /// <param name="groupList">Groups list control</param>
        /// <param name="clients">Collection of clients</param>
        private static void PrepareGroupList(CascadingMsdd clientList, ListControl groupList, IEnumerable<Client> clients)
        {
            groupList.Items.Clear();

            int clientIndex = 1; // Client list checkbox index
            int groupIndex = 1; // Group list checkbox index
            //  Iterates through clients and corresponding groups
            foreach (Client client in clients)
            {
                foreach (ProjectGroup group in client.Groups)
                {
                    //  Add item to the groups list
                    string itemText = String.Format(
                        Resources.Controls.GroupNameFormat,
                        client.HtmlEncodedName, group.HtmlEncodedName);
                    groupList.Items.Add(
                        new ListItem(itemText, group.Id.Value.ToString()));

                    // Add dependence between client and group
                    clientList.AddDependence(
                        new CascadingMsdd.ControlDependence(
                            clientList.ClientID, clientIndex,
                            groupList.ClientID, groupIndex));

                    groupIndex++;
                }

                clientIndex++;
            }

            groupList.Items.Insert(0,
                                    new ListItem(Resources.Controls.AllGroupsText, String.Empty));
        }

        /// <summary>
        /// Fills client list with values
        /// </summary>
        /// <param name="clientList">Client list control</param>
        /// <param name="clients">Collection to databind to the list</param>
        private static void PrepareClientList(CascadingMsdd clientList, IEnumerable<Client> clients)
        {
            clientList.Items.Clear();

            clientList.DataTextField = DefaultNameFieldEndodedName;
            clientList.DataValueField = DefaultIdFieldName;
            clientList.DataSource = clients;
            clientList.DataBind();

            clientList.Items.Insert(0,
                                    new ListItem(Resources.Controls.AllClientsText, String.Empty));
        }

        /// <summary>
        /// Fills the list of clients available for the specific project.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        /// <param name="projectId">An ID of the project to retrive the data for.</param>
        public static void FillClientListForProject(ListControl control, string firstItemText, int? projectId)
        {
            using (var serviceClient = new ClientServiceClient())
            {
                try
                {
                    Client[] clients = serviceClient.ClientListAllForProject(projectId, CurrentPerson.Id);

                    FillListDefault(control, firstItemText, clients, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static IEnumerable<Client> GetAllClientsSecure(Person person, bool inactives, bool applyNewRule = false)
        {
            using (var serviceClient = new ClientServiceClient())
            {
                try
                {
                    Client[] clients = serviceClient.ClientListAllSecureByNewRule(person, inactives, applyNewRule);
                    Array.Sort(clients, (c1, c2) => c1.Name.CompareTo(c2.Name));
                    return clients;
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        private static void FillClientListWithInactive(ListControl control, string firstItemText, bool includeInactive)
        {
            using (var serviceClient = new ClientServiceClient())
            {
                try
                {
                    Client[] clients = serviceClient.ClientListAll(includeInactive);

                    FillListDefault(control, firstItemText, clients, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static Project[] ListProjectsByClient(int? clientId, string sortBy)
        {
            return ServiceCallers.Invoke<ProjectServiceClient, Project[]>(
                client => client.ListProjectsByClientWithSort(clientId, /*CurrentPerson.Alias*/null, sortBy));
        }

        public static Project[] GetTimeEntryProjectsByClientId(int? clientId, int? personId = null, bool showActiveAndInternalProjectsOnly = false)
        {
            using (var serviceClient = new TimeEntryServiceClient())
            {
                try
                {
                    Project[] projects = serviceClient.GetTimeEntryProjectsByClientId(clientId, personId, showActiveAndInternalProjectsOnly);

                    return projects;
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of opportunity statuses.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillOpportunityStatusList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new OpportunityServiceClient())
            {
                try
                {
                    OpportunityStatus[] statuses = serviceClient.OpportunityStatusListAll();

                    FillListDefault(control, firstItemText, statuses, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Fills the list control with the list of opportunity transition statuses.
        /// </summary>
        /// <param name="control">The control to be filled.</param>
        /// <param name="firstItemText">The text to be displayed by default.</param>
        public static void FillOpportinityTransitionStatusList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new OpportunityServiceClient())
            {
                try
                {
                    OpportunityTransitionStatus[] statuses = serviceClient.OpportunityTransitionStatusListAll();

                    FillListDefaultWithEncodedName(control, firstItemText, statuses, false);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillListDefault(ListControl control, string firstItemText, object[] statuses, bool noFirstItem)
        {
            FillListDefault(control, firstItemText, statuses, noFirstItem, DefaultIdFieldName, DefaultNameFieldName);
        }

        public static void FillListDefaultWithEncodedName(ListControl control, string firstItemText, object[] statuses, bool noFirstItem)
        {
            FillListDefault(control, firstItemText, statuses, noFirstItem, DefaultIdFieldName, DefaultNameFieldEndodedName);
        }

        public static void FillListDefault(ListControl control, string firstItemText, object[] statuses, bool noFirstItem, string valueField, string NameField)
        {
            control.AppendDataBoundItems = true;
            control.Items.Clear();
            if (!noFirstItem && statuses != null && statuses.Length > 0)
            {
                control.Items.Add(new ListItem(firstItemText, String.Empty));
            }
            if (statuses == null || !statuses.Any())
            {
                var item = new ListItem(firstItemText, String.Empty);
                item.Enabled = false;
                control.Items.Add(item);
            }
            else
            {
                control.DataValueField = valueField;
                control.DataTextField = NameField;
                control.DataSource = statuses;
                control.DataBind();
            }
        }

        public static void FillTerminationReasonsList(ListControl control, string firstItemText, object[] statuses)
        {
            control.AppendDataBoundItems = true;
            control.Items.Clear();
            if (!string.IsNullOrEmpty(firstItemText))
            {
                control.Items.Add(new ListItem(firstItemText, String.Empty));
            }
            if (statuses == null || !statuses.Any())
            {
                var item = new ListItem(firstItemText, String.Empty);
                item.Enabled = false;
                control.Items.Add(item);
            }
            else
            {
                control.DataValueField = "Id";
                control.DataTextField = "Name";
                control.DataSource = statuses;
                control.DataBind();
            }
        }

        public static void FillApprovedManagersList(ListControl control, string firstItemText, Person[] persons, bool noFirstItem)
        {
            control.AppendDataBoundItems = true;
            control.Items.Clear();
            if (!noFirstItem && persons != null && persons.Length > 0)
            {
                control.Items.Add(new ListItem(firstItemText, String.Empty));
            }
            if (persons == null || !persons.Any())
            {
                var item = new ListItem(firstItemText, String.Empty);
                item.Enabled = false;
                control.Items.Add(item);
            }
            else
            {
                persons = persons.OrderBy(p => p.PersonLastFirstName).ToArray();
                foreach (var person in persons)
                {
                    var itemText = person.LastName + ", " + person.FirstName;
                    var item = new ListItem(itemText, person.Id.Value.ToString());
                    var itemAttribute = person.FirstName + " " + person.LastName;
                    item.Attributes["ApprovedByName"] = itemAttribute;
                    control.Items.Add(item);
                }
            }
        }

        public static int CloneProject(ProjectCloningContext context)
        {
            using (var serviceClient = new ProjectServiceClient())
            {
                try
                {
                    return serviceClient.CloneProject(context);
                }
                catch (FaultException<ExceptionDetail>)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static PersonPermission GetPermissions(Person person)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    return serviceClient.GetPermissions(person);
                }
                catch (FaultException<ExceptionDetail>)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void SaveResourceKeyValuePairs(SettingsType settingType, Dictionary<string, string> dictionary)
        {
            using (var serviceClient = new ConfigurationServiceClient())
            {
                try
                {
                    serviceClient.SaveResourceKeyValuePairs(settingType, dictionary); ;
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void SaveQuickLinksForDashBoard(string linkNameList, string virtualPathList, DashBoardType dashBoardType)
        {
            using (var serviceClient = new ConfigurationServiceClient())
            {
                try
                {
                    serviceClient.SaveQuickLinksForDashBoard(linkNameList, virtualPathList, dashBoardType);
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static Dictionary<string, string> GetResourceKeyValuePairs(SettingsType settingType)
        {
            using (var serviceClient = new ConfigurationServiceClient())
            {
                try
                {
                    return serviceClient.GetResourceKeyValuePairs(settingType); ;
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillBusinessDevelopmentManagersList(ListControl control, string firstItemText, DateTime startDate, DateTime endDate)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    Person[] persons = serviceClient.PersonListByCategoryTypeAndPeriod(DataTransferObjects.BudgetCategoryType.BusinessDevelopmentManager,
                                                                                        startDate,
                                                                                        endDate);

                    //FillListDefault(control, firstItemText, persons, false);
                    FillPersonList(control, firstItemText, persons, string.Empty, false, true);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillOpportunityPrioritiesList(DropDownList ddlPriority, string firstItemText)
        {
            OpportunityPriority[] priorities = OpportunityPriorityHelper.GetOpportunityPriorities(true);

            FillListDefault(ddlPriority, firstItemText, priorities, false, "Id", "HtmlEncodedDisplayName");
        }

        public static Dictionary<string, DateTime> GetFiscalYearPeriod(DateTime currentMonth)
        {
            Dictionary<string, DateTime> fPeriod = new Dictionary<string, DateTime>();

            DateTime startMonth = new DateTime(currentMonth.Year, Constants.Dates.FYFirstMonth, 1);
            DateTime endMonth = new DateTime(currentMonth.Year, Constants.Dates.FYLastMonth, 31);

            fPeriod.Add("StartMonth", startMonth);
            fPeriod.Add("EndMonth", endMonth);

            return fPeriod;
        }

        public static void FillColorsList(DropDownList ddlColor, string p)
        {
            using (var serviceClient = new ClientServiceClient())
            {
                try
                {
                    List<ColorInformation> colors = serviceClient.GetAllColorsForMargin().AsQueryable().ToList();

                    ddlColor.Items.Clear();

                    ListItem firstItem = new ListItem()
                    {
                        Text = "-- Select a Color --",
                        Value = "0"
                    };

                    ddlColor.Items.Add(firstItem);
                    firstItem.Attributes.Add("style", string.Format("background-color:{0}", "white"));
                    firstItem.Attributes.Add("colorValue", "white");

                    foreach (var color in colors)
                    {
                        var colorItem = new ListItem()
                        {
                            Text = string.Empty,
                            Value = color.ColorId.ToString()
                        };
                        colorItem.Attributes.Add("style", string.Format("background-color:{0}", color.ColorValue));
                        colorItem.Attributes.Add("colorValue", string.Format(color.ColorValue));
                        colorItem.Attributes.Add("Description", string.Format(color.ColorDescription));
                        colorItem.Attributes["title"] = string.Format(color.ColorDescription);
                        ddlColor.Items.Add(colorItem);
                    }
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static List<ClientMarginColorInfo> GetClientMarginColorInfo(int clientId)
        {
            using (var serviceClient = new ClientServiceClient())
            {
                try
                {
                    var result = serviceClient.GetClientMarginColorInfo(clientId);

                    if (result != null)
                    {
                        var clientInfoList = result.AsQueryable().ToList();

                        return clientInfoList;
                    }
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
            return null;
        }

        public static bool IsUserHasPermissionOnProject(string user, int projectId, bool isProjectNotMilestone = true)
        {
            using (var serviceClient = new ProjectServiceClient())
            {
                return serviceClient.IsUserHasPermissionOnProject(user, projectId, isProjectNotMilestone);
            }
        }

        public static bool IsUserIsOwnerOfProject(string user, int Id, bool isProjectId)
        {
            using (var serviceClient = new ProjectServiceClient())
            {
                return serviceClient.IsUserIsOwnerOfProject(user, Id, isProjectId);
            }
        }

        public static bool IsUserIsProjectOwner(string user, int Id)
        {
            using (var serviceClient = new ProjectServiceClient())
            {
                return serviceClient.IsUserIsProjectOwner(user, Id);
            }
        }

        public static List<UserPasswordsHistory> GetPasswordHistoryByUserName(string userName)
        {
            using (var person = new PersonServiceClient())
            {
                return person.GetPasswordHistoryByUserName(userName).AsQueryable().ToList();
            }
        }

        public static string GetEncodedPassword(string password, string passwordSalt)
        {
            using (var person = new PersonServiceClient())
            {
                return person.GetEncodedPassword(password, passwordSalt);
            }
        }

        public static List<MilestonePerson> GetMilestonePersonListByProjectWithoutPay(int projectId)
        {
            using (var serviceClient = new MilestonePersonService.MilestonePersonServiceClient())
            {
                var milestonePersonList = serviceClient.GetMilestonePersonListByProjectWithoutPay(projectId);
                return milestonePersonList.OrderBy(mp => mp.Person.PersonLastFirstName).ThenBy(mp => mp.StartDate).ToList();
            }
        }

        public static Dictionary<DateTime, bool> GetIsNoteRequiredDetailsForSelectedDateRange(DateTime start, DateTime end, int personId)
        {
            using (var person = new PersonServiceClient())
            {
                return person.GetIsNoteRequiredDetailsForSelectedDateRange(start, end, personId);
            }
        }

        public static void FillOwnersList(DropDownList ddlProjectManager, string firstItemText)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    string statusids = (int)DataTransferObjects.PersonStatusType.Active + ", " + (int)DataTransferObjects.PersonStatusType.TerminationPending;
                    Person[] persons = serviceClient.OwnerListAllShort(statusids);

                    FillListDefault(ddlProjectManager, firstItemText, persons, false, "Id", "PersonLastFirstName");
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static List<Opportunity> GetLookedOpportunities(string looked, int personId)
        {
            using (var serviceClient = new OpportunityServiceClient())
            {
                try
                {
                    var opportunities =
                        serviceClient.OpportunitySearchText(
                            looked,
                            personId);

                    return SortOppotunities(opportunities);
                }
                catch
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static List<Opportunity> SortOppotunities(Opportunity[] opportunities)
        {
            var sortingFilter = Controls.Generic.OpportunityList.Filter;
            var comp = new OpportunityComparer(sortingFilter);

            if (comp.SortOrder != OpportunitySortOrder.None)
                Array.Sort(opportunities, comp);

            return opportunities.ToList();
        }

        public static List<QuickLinks> GetQuickLinksByDashBoardType(DashBoardType dashBoardtype)
        {
            using (var serviceClient = new ConfigurationServiceClient())
            {
                try
                {
                    var quickLinks = serviceClient.GetQuickLinksByDashBoardType(dashBoardtype);

                    return quickLinks.AsQueryable().ToList(); ;
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void DeleteQuickLinkById(int id)
        {
            using (var serviceClient = new ConfigurationServiceClient())
            {
                try
                {
                    serviceClient.DeleteQuickLinkById(id);
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static Project[] ListProjectsByClientAndPersonInPeriod(int clientId, bool isOnlyActiveAndInternal, bool isOnlyEnternalProjects, int personId, DateTime startDate, DateTime endDate)
        {
            using (var serviceClient = new ProjectServiceClient())
            {
                try
                {
                    return serviceClient.ListProjectsByClientAndPersonInPeriod(clientId, isOnlyActiveAndInternal, isOnlyEnternalProjects, personId, startDate, endDate);
                }
                catch (Exception ex)
                {
                    serviceClient.Abort();
                    throw ex;
                }
            }
        }

        public static DateTime GetSubstituteDate(DateTime holidayDate, int personId)
        {
            using (var serviceClient = new CalendarServiceClient())
            {
                return serviceClient.GetSubstituteDate(personId, holidayDate);
            }
        }

        public static KeyValuePair<DateTime, string> GetSubstituteDayDetails(int personId, DateTime substituteDate)
        {
            using (var serviceClient = new CalendarServiceClient())
            {
                return serviceClient.GetSubstituteDayDetails(personId, substituteDate);
            }
        }

        public static Project[] GetProjectsByClientId(int clientId)
        {
            using (var serviceClient = new ReportServiceClient())
            {
                try
                {
                    Project[] projects = serviceClient.GetProjectsByClientId(clientId);

                    return projects;
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillPersonDivisionList(ListControl control, string firstItemText)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var divisions = Enum.GetValues(typeof(PersonDivisionType));

                    control.AppendDataBoundItems = true;
                    control.Items.Clear();

                    control.DataTextField = "Key";
                    control.DataValueField = "Value";

                    Dictionary<string, string> list = new Dictionary<string, string>();
                    list.Add(firstItemText, String.Empty);
                    foreach (PersonDivisionType item in divisions)
                    {
                        if ((int)item == 0 || (int)item == 2)
                            continue;
                        string key = GetDescription(item);
                        string value = ((int)item).ToString();
                        if (value == "0") value = "";
                        list.Add(key, value);
                    }

                    control.DataSource = list;
                    control.DataBind();
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillPersonDivisionList(ListControl control, bool isAllitems = false)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var divisions = serviceClient.GetPersonDivisions();
                    if (isAllitems)
                    {
                        FillListDefault(control, "All Divisions", divisions, false, "DivisionId", "DivisionName");
                    }
                    else
                    {
                        FillListDefault(control, "- - Select Division - -", divisions, false, "DivisionId", "DivisionName");
                    }
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillProjectDivisionList(ListControl control, bool isExternal, bool isAllitems = false)
        {
            using (var serviceClient = new ProjectServiceClient())
            {
                try
                {
                    var projectDivisions = isExternal ? serviceClient.GetProjectDivisions().Where(d => d.IsExternal).ToArray() : serviceClient.GetProjectDivisions();
                    if (isAllitems)
                    {
                        FillListDefault(control, "All Divisions", projectDivisions, false);
                    }
                    else
                    {
                        FillListDefault(control, "-- Select Division --", projectDivisions, false);
                    }
                }

                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }

        }

        public static void FillVendorTypeList(ListControl control, bool isAllitems = false)
        {
            using (var serviceClient = new VendorServiceClient())
            {
                try
                {
                    var vendorTypes = serviceClient.GetListOfVendorTypes();
                    if (isAllitems)
                    {
                        FillListDefault(control, "All Vendor Types", vendorTypes, false);
                    }
                    else
                    {
                        FillListDefault(control, "-- Select Vendor Type --", vendorTypes, false);
                    }
                }

                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillAttachemntCategoryList(ListControl control)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    var categories = Enum.GetValues(typeof(ProjectAttachmentCategory));

                    control.AppendDataBoundItems = true;
                    control.Items.Clear();

                    control.DataTextField = "Key";
                    control.DataValueField = "Value";

                    Dictionary<string, int> list = new Dictionary<string, int>();

                    foreach (ProjectAttachmentCategory item in categories)
                    {
                        string key = GetDescription(item);

                        int value = (int)item;

                        list.Add(key, value);
                    }

                    control.DataSource = list;
                    control.DataBind();
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static string GetDescription(Enum enumerator)
        {
            Type type = enumerator.GetType();

            var memberInfo = type.GetMember(enumerator.ToString());

            if (memberInfo != null && memberInfo.Length > 0)
            {
                //we default to the first member info, as it's for the specific enum value
                object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                //return the description if it's found
                if (attributes != null && attributes.Length > 0)
                    return ((DescriptionAttribute)attributes[0]).Description;
            }

            //if there's no description, return the string value of the enum
            return enumerator.ToString();
        }

        public static bool IsDateInPersonEmployeeHistory(Person person, DateTime date, bool checkWholeHistory, bool dontCheckActiveRecord = false)
        {
            bool check = false;
            if (person != null)
            {
                if (person.EmploymentHistory != null)
                {
                    if (checkWholeHistory)
                    {
                        DateTime? activeHireDate = null;
                        if (dontCheckActiveRecord && (person.Status.Id == (int)PersonStatusType.Active || person.Status.Id == (int)PersonStatusType.TerminationPending))
                        {
                            //person is active
                            activeHireDate = person.EmploymentHistory.Max(e => e.HireDate);
                        }

                        foreach (var emp in person.EmploymentHistory)
                        {
                            if (emp.HireDate <= date &&
                                ((emp.TerminationDate.HasValue && emp.TerminationDate.Value >= date) || (!emp.TerminationDate.HasValue)))
                            {
                                if ((dontCheckActiveRecord && activeHireDate != emp.HireDate) || !dontCheckActiveRecord)
                                {
                                    check = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        check = person.HireDate <= date &&
                                ((person.TerminationDate.HasValue && person.TerminationDate.Value >= date) || (!person.TerminationDate.HasValue));
                    }
                }
            }

            return check;
        }

        public static bool IsReadOnlyInPersonEmployeeHistory(Person person, DateTime[] dates)
        {
            bool check = true;
            if (person != null && person.EmploymentHistory != null && (person.Status.Id == (int)PersonStatusType.Active || person.Status.Id == (int)PersonStatusType.TerminationPending))
            {
                //person is active
                int i = 0;
                foreach (var date in dates)
                {
                    if (!DataHelper.IsDateInPersonEmployeeHistory(person, date, true, true))
                    {
                        i++;
                    }
                }
                if (i == dates.Length)
                {
                    check = false;
                }
            }

            return check;
        }

        public static void FillLocationList(DropDownList ddlLocations, string firstItemText)
        {
            using (var serviceClient = new PersonServiceClient())
            {
                try
                {
                    using (var config = new ConfigurationServiceClient())
                    {
                        var locations = config.GetLocations();
                        FillListDefault(ddlLocations, firstItemText, locations, false, "LocationId", "FormattedLocation");
                    }
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void FillPersonListWithPreferrdFirstName(ListControl control, string firstItemText, string statusIds)
        {
            FillPersonList(control, firstItemText, DateTime.MinValue, DateTime.MinValue, statusIds, false, true);
        }

        public static void FillExpenseTypeList(ListControl control, bool isAllitems = false)
        {
            using (var serviceClient = new MilestoneServiceClient())
            {
                try
                {
                    var expenseTypes = serviceClient.GetAllExpenseTypesList();
                    if (isAllitems)
                    {
                        FillListDefault(control, "All Expense Types", expenseTypes, false);
                    }
                    else
                    {
                        FillListDefault(control, "-- Select Expense Type --", expenseTypes, false);
                    }
                }

                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static string GetTextForHover(List<ExpenseSummary> list, bool isActual, bool isExpenseType = false)
        {
            string heading = isExpenseType ? "Project Number-Expense Name" : "Expense Name";
            string htmlString = "<table><tr><td class=\"fontBold padRight10\">" + heading + "</ td><td class=\"fontBold\">Amount($)</ td></ tr>{0}</ tabel>";
            string row = "<tr><td class=\"padRight10\">{0}</ td><td>{1}</ td></ tr>";
            string abc = string.Empty;
            foreach (var data in list)
            {
                abc += string.Format(row, isExpenseType ? data.Project.ProjectNumber + "-" + data.Expense.HtmlEncodedName : data.Expense.HtmlEncodedName, isActual ? data.Expense.Amount.ToString("###,###,###,###,###,##0.##") : data.Expense.ExpectedAmount.ToString("###,###,###,###,###,##0.##"));
            }
            return string.Format(htmlString, abc);
        }

        public static void FillProjectsForClients(ListControl control, string clientIds)
        {
            var projects = ServiceCallers.Custom.Project(p => p.GetProjectsForClients(clientIds));
            FillListDefault(control, "All Projects", projects, false);
        }
    }
}

