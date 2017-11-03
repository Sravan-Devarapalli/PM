﻿using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using DataTransferObjects;
using DataTransferObjects.ContextObjects;
using DataTransferObjects.Reports;
using DataTransferObjects.TimeEntry;

namespace PracticeManagementService
{
    [ServiceKnownType(typeof(Timescale))]
    [ServiceContract]
    public interface IPersonService
    {
        /// <summary>
        /// Person Milestone With Financials
        /// </summary>
        [OperationContract]
        DataSet GetPersonMilestoneWithFinancials(int personId);

        /// <summary>
        /// Set the person as default manager
        /// </summary>
        [OperationContract]
        void SetAsDefaultManager(int personId);

        /// <summary>
        /// Retrieves consultants report
        /// </summary>
        /// <returns>An <see cref="Opportunity"/> object if found and null otherwise.</returns>
        [OperationContract]
        List<ConsultantUtilizationPerson> GetConsultantUtilizationWeekly(ConsultantTimelineReportContext context);

        /// <summary>
        /// Retrieves a consultant's  daily report whose Oersin Id is given by personId.
        /// </summary>
        /// <returns>An <see cref="Opportunity"/> object if found and null otherwise.</returns>
        [OperationContract]
        List<ConsultantUtilizationPerson> ConsultantUtilizationDailyByPerson(int personId, ConsultantTimelineReportContext context);

        /// <summary>
        /// Check's if there's compensation record covering milestone/
        /// See #886 for details.
        /// </summary>
        /// <param name="person">Person to check against</param>
        /// <returns>True if there's such record, false otherwise</returns>
        [OperationContract]
        bool IsCompensationCoversMilestone(Person person, DateTime? start, DateTime? end);

        /// <summary>
        /// Verifies whether a user has compensation at this moment
        /// </summary>
        /// <param name="personId">Id of the person</param>
        /// <returns>True if a person has active compensation, false otherwise</returns>
        [OperationContract]
        bool CurrentPayExists(int personId);

        /// <summary>
        /// Retrieves a list of the persons for excel export.
        /// </summary>
        /// <returns>A list of the <see cref="Opportunity"/> objects.</returns>
        [OperationContract]
        System.Data.DataSet PersonGetExcelSet();


        [OperationContract]
        System.Data.DataSet PersonGetExcelSetWithFilters(
            string practiceIdsSelected,
           string divisionIdsSelected,
           string looked,
           string recruiterIdsSelected,
           string timeScaleIdsSelected,
           bool Active,
           bool projected,
           bool terminated,
           bool terminatedPending
            );
        /// <summary>
        /// Gets all permissions for the given person
        /// </summary>
        /// <param name="person">Person to get permissions for</param>
        /// <returns>Object with the list of permissions</returns>
        [OperationContract]
        PersonPermission GetPermissions(Person person);

        [OperationContract]
        List<Person> GetPersonListWithCurrentPayByCommaSeparatedIdsList(
            string practiceIdsSelected,
            string divisionIdsSelected,
            bool active,
            int pageSize,
            int pageNo,
            string looked,
            string recruiterIdsSelected,
            string userName,
            string sortBy,
            string timeScaleIdsSelected,
            bool projected,
            bool terminated,
            bool terminatedPending,
            bool righttoPresent,
            char? alphabet);

        /// <summary>
        /// Retrieves a short info on persons.
        /// </summary>
        /// <param name="practice">Practice filter, null meaning all practices</param>
        /// <param name="statusId">Person status id</param>
        /// <param name="startDate">Determines a start date when persons in the list must are available.</param>
        /// <param name="endDate">Determines an end date when persons in the list must are available.</param>
        /// <returns>A list of the <see cref="Person"/> objects.</returns>
        [OperationContract]
        List<Person> PersonListAllShort(int? practice, string statusIds, DateTime startDate, DateTime endDate);

        [OperationContract]
        List<Person> OwnerListAllShort(string statusIds);

        /// <summary>
        ///  Retrieves a short info on persons.
        /// </summary>
        /// <param name="statusId">Person status id</param>
        /// <param name="roleName">Person role</param>
        /// <returns>A list of the <see cref="Person"/> objects</returns>
        [OperationContract]
        List<Person> PersonListShortByRoleAndStatus(string statusIds, string roleName);

        [OperationContract]
        List<Person> PersonListShortByTitleAndStatus(string statusIds, string titleNames);

        /// <summary>
        /// Retrieves a short info on persons who are not in the Administration practice.
        /// </summary>
        /// <param name="milestonePersonId">An ID of existing milestone-person association or null.</param>
        /// <param name="startDate">Determines a start date when persons in the list must are available.</param>
        /// <param name="endDate">Determines an end date when persons in the list must are available.</param>
        /// <returns>A list of the <see cref="Person"/> objects.</returns>
        [OperationContract]
        List<Person> PersonListAllForMilestone(int? milestonePersonId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculates a number of <see cref="Person"/>s match with the specified conditions.
        /// </summary>
        /// <param name="practiceIds">List of practice Ids</param>
        /// <param name="active">Is active or not</param>
        /// <param name="looked">search text</param>
        /// <param name="recruiterIds">List of recruiter Ids</param>
        /// <param name="userName">Logged in user</param>
        /// <param name="timeScaleIds">List of Time scale Ids</param>
        /// <param name="projected">Is Contingent or not</param>
        /// <param name="terminated">Is terminatied or not</param>
        /// <param name="terminatedPending">Is Termination Pending or not</param>
        /// <param name="alphabet">person starts with the letter</param>
        /// <returns></returns>
        [OperationContract]
        int GetPersonCountByCommaSeperatedIdsList(string practiceIds, string divisionIdsSelected, bool active, string looked, string recruiterIds, string userName, string timeScaleIds, bool projected, bool terminated, bool terminationpending, bool righttoPresent, char? alphabet);

        /// <summary>
        /// Calculates a number of <see cref="Person"/>s working days in period.
        /// </summary>
        /// <param name="personId">Id of the person to get</param>
        /// <param name="startDate">mileStone start date </param>
        /// <param name="endDate">mileStone end date</param>
        /// <returns>The number of the persons working days in period.</returns>
        [OperationContract]
        PersonWorkingHoursDetailsWithinThePeriod GetPersonWorkingHoursDetailsWithinThePeriod(int personId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lists all active persons who have recruiter role.
        /// </summary>
        /// <returns>The list of <see cref="Person"/> objects.</returns>
        [OperationContract]
        List<Person> GetRecruiterList();

        /// <summary>
        /// List the persons who recieve the sales commissions
        /// </summary>
        /// <param name="includeInactive">Determines whether inactive persons will are included into the results.</param>
        /// <returns>The list of the <see cref="Person"/> objects.</returns>
        [OperationContract]
        List<Person> GetSalespersonList(bool includeInactive);

        /// <summary>
        /// List the persons who recieve the sales commissions
        /// </summary>
        /// <param name="person">Person to restrict permissions to</param>
        /// <param name="inactives">Determines whether inactive persons will are included into the results.</param>
        /// <returns>The list of the <see cref="Person"/> objects.</returns>
        [OperationContract]
        List<Person> PersonListSalesperson(Person person, bool inactives);

        /// <summary>
        /// List the persons who recieve the Practice Management commissions
        /// </summary>
        /// <param name="endDate">An end date of the project the Practice Manager be selected for.</param>
        /// <param name="includeInactive">Determines whether inactive persons will are included into the results.</param>
        /// <param name="person"></param>
        /// <returns>
        /// The list of <see cref="Person"/> objects applicable to be a practice manager for the project.
        /// </returns>
        [OperationContract]
        List<Person> PersonListProjectOwner(bool includeInactive, Person person);

        /// <summary>
        /// Read All persons firstname and last name  except having inactive status and must have compensation for today or in future.
        /// </summary>
        /// <param name="today"></param>
        /// <returns>List<Person/></returns>
        [OperationContract]
        List<Person> GetOneOffList(DateTime today);

        /// <summary>
        /// Get a person
        /// </summary>
        /// <param name="personId">Id of the person to get</param>
        /// <returns>
        /// Person matching <paramref name="personId"/>, or <value>null</value> if the person is not in the system
        /// </returns>
        /// <remarks>
        /// Presumably the id is obtained form a previous call to GetPersonList but
        /// there is no system restriction on the value for the identifier in this call.
        /// </remarks>
        [OperationContract]
        Person GetPersonDetail(int personId);

        /// <summary>
        /// Retrieves a <see cref="Person"/> by the Alias (email).
        /// </summary>
        /// <param name="alias">The EMail to search for.</param>
        /// <returns>The <see cref="Person"/> object if found and null otherwise.</returns>
        [OperationContract]
        Person GetPersonByAlias(string alias);

        // TODO: better define "if the person exists"  The id is new or zero, maybe?
        /// <summary>
        /// Commit data about a <see cref="Person"/> to the system store
        /// </summary>
        /// <param name="person">Person information to store</param>
        /// <remarks>
        /// If the person exists in the system then this information overwirtes information already in
        /// the store, otherwise a new person is created, and an identifier is placed
        /// in the <paramref name="person"/>
        /// </remarks>
        /// <param name="currentUser">current logged in user name</param>
        [OperationContract]
        [FaultContract(typeof(DataAccessFault))]
        int SavePersonDetail(Person person, string currentUser, string loginPageUrl, bool saveCurrentPay, string userLogin);

        [OperationContract]
        void PersonValidations(Person person);

        /// <summary>
        /// Retrieves a list of overheads for the specified <see cref="Timescale"/>.
        /// </summary>
        /// <param name="timescale">The <see cref="Timescale"/> to retrive the data for.</param>
        /// <returns>The list of the <see cref="PersonOverhead"/> objects.</returns>
        [OperationContract]
        List<PersonOverhead> GetPersonOverheadByTimescale(TimescaleType timescale, DateTime? effectiveDate);

        /// <summary>
        /// Retrieves the person's rate.
        /// </summary>
        /// <param name="milestonePerson">The milestone-person association to the rate be calculated for.</param>
        /// <returns>The <see cref="MilestonePerson"/> object with calculated data.</returns>
        [OperationContract]
        MilestonePerson GetPersonRate(MilestonePerson milestonePerson);

        /// <summary>
        /// Calculates the person's rate.
        /// </summary>
        /// <param name="person">A <see cref="Person"/> object to calculate the data for.</param>
        /// <param name="proposedHoursPerWeek">A proposed work week duration.</param>
        /// <param name="proposedRate">A proposed person's hourly rate.</param>
        /// <returns>The <see cref="ComputedRate"/> object with the calculation results.</returns>
        [OperationContract]
        ComputedFinancialsEx CalculateProposedFinancialsPerson(Person person, decimal proposedRate, decimal proposedHoursPerWeek, decimal clientDiscount, bool isMarginTestPage, DateTime? effectiveDate);

        /// <summary>
        /// Saves a payment data.
        /// </summary>
        /// <param name="pay">The <see cref="Pay"/> object to be saved.</param>
        [OperationContract]
        void SavePay(Pay pay, string loginPageUrl, string user);

        [OperationContract]
        void DeletePay(int personId, DateTime startDate);

        /// <summary>
        /// Selects a list of the seniorities.
        /// </summary>
        /// <returns>A list of the <see cref="Seniority"/> objects.</returns>
        [OperationContract]
        List<Seniority> ListSeniorities();

        /// <summary>
        /// Sets permissions for user
        /// </summary>
        /// <param name="person">Person to set permissions for</param>
        /// <param name="permissions">Permissions to set for the user</param>
        [OperationContract]
        void SetPermissionsForPerson(Person person, PersonPermission permissions);

        /// <summary>
        /// Get person by it's ID
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>Person</returns>
        [OperationContract]
        Person GetPersonById(int personId);

        /// <summary>
        /// Retrives a short info on persons.
        /// </summary>
        /// <param name="statusList">Comma seperated statusIds</param>
        /// <returns></returns>
        [OperationContract]
        List<Person> GetPersonListByStatusList(string statusList, int? personId);

        /// <summary>
        /// Retrives a short info of persons specified by personIds.
        /// </summary>
        /// <param name="statusList">Comma seperated PersonIds</param>
        /// <returns></returns>
        [OperationContract]
        List<Person> GetPersonListByPersonIdList(string PersonIds);

        [OperationContract]
        List<Person> GetPersonListByPersonIdsAndPayTypeIds(string personIds, string paytypeIds, string practiceIds, DateTime startDate, DateTime endDate);

        [OperationContract]
        bool SaveUserTemporaryCredentials(string userName, string PMLoginPageUrl, string PMChangePasswordPageUrl);

        [OperationContract]
        bool CheckIfTemporaryCredentialsValid(string userName, string password);

        [OperationContract]
        void SetNewPasswordForUser(string userName, string newPassword);

        [OperationContract]
        List<Person> PersonListByCategoryTypeAndPeriod(BudgetCategoryType categoryType, DateTime startDate, DateTime endDate);

        [OperationContract]
        bool CheckPersonTimeEntriesAfterTerminationDate(int personId, DateTime terminationDate);

        [OperationContract]
        bool CheckPersonTimeEntriesAfterHireDate(int personId);

        [OperationContract]
        Owner CheckIfPersonStatusCanChangeFromActiveToContingent(int personId);

        [OperationContract]
        List<Milestone> GetPersonMilestonesAfterTerminationDate(int personId, DateTime terminationDate);

        [OperationContract]
        List<UserPasswordsHistory> GetPasswordHistoryByUserName(string userName);

        [OperationContract]
        string GetEncodedPassword(string password, string passwordSalt);

        [OperationContract]
        void RestartCustomMembershipProvider();

        [OperationContract]
        void SendLockedOutNotificationEmail(string userName, string loginPageUrl);

        [OperationContract]
        Dictionary<DateTime, bool> GetIsNoteRequiredDetailsForSelectedDateRange(DateTime start, DateTime end, int personId);

        [OperationContract]
        List<Project> GetOwnerProjectsAfterTerminationDate(int personId, DateTime terminationDate);

        [OperationContract]
        List<Opportunity> GetActiveOpportunitiesByOwnerId(int personId);

        [OperationContract]
        int? SaveStrawman(Person person, Pay currentPay, string userLogin);

        [OperationContract]
        void DeleteStrawman(int personId, string userLogin);

        [OperationContract]
        Person GetStrawmanDetailsById(int personId);

        [OperationContract]
        List<Person> GetStrawmenListAll();

        [OperationContract]
        List<Person> GetStrawmenListAllShort(bool includeInactive);

        [OperationContract]
        int SaveStrawManFromExisting(int existingPersonId, Person person, string userLogin);

        [OperationContract]
        List<ConsultantDemandItem> GetConsultantswithDemand(DateTime startDate, DateTime endDate);

        [OperationContract]
        bool IsPersonHaveActiveStatusDuringThisPeriod(int personId, DateTime startDate, DateTime? endDate);

        [OperationContract]
        List<Person> PersonsListHavingActiveStatusDuringThisPeriod(DateTime startDate, DateTime endDate);

        [OperationContract]
        List<Person> GetApprovedByManagerList();

        [OperationContract]
        List<Person> GetPersonListBySearchKeyword(String looked);

        [OperationContract]
        Person GetPayHistoryShortByPerson(int personId);

        [OperationContract]
        List<Triple<DateTime, bool, bool>> IsPersonSalaryTypeListByPeriod(int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        Person GetPersonDetailsShort(int personId);

        [OperationContract]
        Person GetStrawmanDetailsByIdWithCurrentPay(int id);

        [OperationContract]
        List<Pay> GetHistoryByPerson(int personId);

        [OperationContract]
        List<Person> GetStrawmanListShortFilterWithTodayPay();

        [OperationContract]
        List<TerminationReason> GetTerminationReasonsList();

        [OperationContract]
        Person GetPersonHireAndTerminationDate(int personId);

        [OperationContract]
        List<SeniorityCategory> ListAllSeniorityCategories();

        [OperationContract]
        List<Person> GetPersonListWithRole(string rolename);

        [OperationContract]
        List<Employment> GetPersonEmploymentHistoryById(int personId);

        [OperationContract]
        List<TimeTypeRecord> GetPersonAdministrativeTimeTypesInRange(int personId, DateTime startDate, DateTime endDate, bool includePTO, bool includeHoliday, bool includeUnpaid, bool includeSickLeave, bool includeFloatingHoliday);

        [OperationContract]
        bool IsPersonTimeOffExistsInSelectedRangeForOtherthanGivenTimescale(int personId, DateTime startDate, DateTime endDate, int timeScaleId);

        [OperationContract]
        void DeletePersonEncodedPassword(int personId);

        [OperationContract]
        bool CheckIfPersonPasswordValid(string alias, string password);

        [OperationContract]
        void UpdateUserPassword(int personId, string userName, string newPassword);

        [OperationContract]
        Pay GetCurrentByPerson(int personId);

        [OperationContract]
        void SendAdministratorAddedEmail(Person person, Person oldPerson);

        [OperationContract]
        List<Person> GetActivePersonsByProjectId(int projectId);

        [OperationContract]
        Title GetPersonTitleByRange(int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        bool CheckIfRangeWithinHireAndTermination(int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        bool CheckIfPersonConsultantTypeInAPeriod(int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        List<Project> GetCommissionsValidationByPersonId(int personId, DateTime hireDate,
                                                                DateTime? terminationDate, int personStatusId,
                                                                int divisionId, bool IsReHire);

        [OperationContract]
        Person CheckIfValidDivision(int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        bool CheckIfPersonEntriesOverlapps(int milestoneId, int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        List<Person> GetPersonsByPayTypesAndByStatusIds(string statusIds, string payTypeIds);

        [OperationContract]
        List<CohortAssignment> GetAllCohortAssignments();

        [OperationContract]
        List<Person> GetPTOReport(DateTime startDate, DateTime endDate, bool includeCompanyHolidays);

        [OperationContract]
        List<MSBadge> GetBadgeDetailsByPersonId(int personId);

        [OperationContract]
        List<MSBadge> GetLogic2020BadgeHistory(int personId);

        [OperationContract]
        void SaveBadgeDetailsByPersonId(MSBadge msBadge);

        [OperationContract]
        void UpdateMSBadgeDetailsByPersonId(int personId, int updatedBy);

        [OperationContract]
        List<bool> CheckIfDatesInDeactivationHistory(int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        bool CheckIfPersonInProjectForDates(int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        bool CheckIfPersonIsRestrictedByProjectId(int personId, int projectId, DateTime chargeDate);

        [OperationContract]
        PersonBadgeHistories GetBadgeHistoryByPersonId(int personId);

        [OperationContract]
        bool CheckIfPersonInProjectsForThisPeriod(DateTime? modifiedEndDate, DateTime? oldEndDate, DateTime? modifiedStartDate, DateTime? oldStartDate, int personId);

        [OperationContract]
        List<MSBadge> GetBadgeRecordsAfterDeactivatedDate(int personId, DateTime deactivatedDate);

        [OperationContract]
        List<MSBadge> GetBadgeRecordsByProjectId(int projectId);

        [OperationContract]
        bool IsPersonSalaryTypeInGivenRange(int personId, DateTime startDate, DateTime endDate);

        [OperationContract]
        List<Person> GetPracticeLeaderships(int? divisionId);

        [OperationContract]
        List<PersonDivision> GetPersonDivisions();

        [OperationContract]
        PersonDivision GetPersonDivisionById(int divisioId);

        [OperationContract]
        void UpdatePersonDivision(PersonDivision division);

        [OperationContract]
        List<Owner> CheckIfPersonIsOwnerForDivisionAndOrPractice(int personId);

        [OperationContract]
        void SaveReportFilterValues(int currentUserId, int reportId, string data, int previousUserId, string sessionId);

        [OperationContract]
        string GetReportFilterValues(int currentUserId, int reportId, int previousUserId, string sessionId);

        [OperationContract]
        void DeleteReportFilterValues(int currentUserId, int previousUserId, string sessionId);

        //[OperationContract]
        //void SendCompensationChangeEmail(Person person, Pay oldPay, Pay newPay, bool isRehire);

        [OperationContract]
        List<ConsultantPTOHours> GetConsultantPTOEntries(DateTime startDate, DateTime endDate, bool includeActivePersons, bool includeContingentPersons, bool isW2Salary, bool isW2Hourly, string practiceIds, string divisionIds, string titleIds, int sortId, string sortDirection);
    }
}

