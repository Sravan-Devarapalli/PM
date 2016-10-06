﻿using System;
using System.Collections.Generic;
using System.ServiceModel;

using DataTransferObjects;
using DataTransferObjects.ContextObjects;
using DataTransferObjects.Financials;

namespace PracticeManagementService
{
    [ServiceContract]
    public interface IMilestonePersonService
    {
        /// <summary>
        /// Retrives the list of the <see cref="Person"/>s for the specified <see cref="Project"/>.
        /// </summary>
        /// <param name="projectId">An ID of the project to the data be retrieved for.</param>
        /// <returns>The list of the <see cref="MilestonePerson"/> objects.</returns>
        [OperationContract]
        List<MilestonePerson> GetMilestonePersonListByProject(int projectId);

        /// <summary>
        /// Retrives the list of the <see cref="Person"/>s for the specified <see cref="Project"/>.
        /// </summary>
        /// <param name="projectId">An ID of the project to the data be retrieved for.</param>
        /// <returns>The list of the <see cref="MilestonePerson"/> objects.</returns>
        [OperationContract]
        List<MilestonePerson> GetMilestonePersonListByProjectWithoutPay(int projectId);

        /// <summary>
        /// Gets milestones info for given person
        /// </summary>
        [OperationContract]
        List<MilestonePersonEntry> GetConsultantMilestones(ConsultantMilestonesContext context);

        /// <summary>
        /// Retrives the list of the <see cref="Person"/>s for the specified <see cref="Milestone"/>.
        /// </summary>
        /// <param name="milestoneId">An ID of the milestone to the data be retrieved for.</param>
        /// <returns>The list of the <see cref="MilestonePerson"/> objects.</returns>
        [OperationContract]
        List<MilestonePerson> GetMilestonePersonListByMilestone(int milestoneId);

        /// <summary>
        /// Retrives the milestone-person link details
        /// </summary>
        /// <param name="milestonePersonId">An ID of the milestone-person association.</param>
        /// <returns>The <see cref="MilestonePerson"/> object if found and null otherwise.</returns>
        [OperationContract]
        MilestonePerson GetMilestonePersonDetail(int milestonePersonId);

        /// <summary>
        /// Checks whether there are any time entries for a given MilestonepersonId in between the given startdate and enddate.
        /// </summary>
        /// <param name="milestonePersonId"></param>
        /// <returns></returns>
        [OperationContract]
        bool CheckTimeEntriesForMilestonePerson(int milestonePersonId, DateTime? startDate, DateTime? endDate,
                                                bool checkStartDateEquality, bool checkEndDateEquality);

        /// <summary>
        /// Checks whether there are any time entries for a given MilestonepersonId for the given MilestonePersonRoleId.
        /// </summary>
        /// <param name="milestonePersonId"></param>
        /// <returns></returns>
        [OperationContract]
        bool CheckTimeEntriesForMilestonePersonWithGivenRoleId(int milestonePersonId, int? milestonePersonRoleId);

        /// <summary>
        /// Saves the specified <see cref="Milestone"/>-<see cref="Person"/> link to the database.
        /// </summary>
        /// <param name="milestonePerson">The data to be saved to.</param>
        /// <param name="userName">A current user.</param>
        [OperationContract]
        void SaveMilestonePerson(ref MilestonePerson milestonePerson, string userName);

        [OperationContract]
        void SaveMilestonePersons(List<MilestonePerson> milestonePersons, string userName);

        /// <summary>
        /// Deletes the specified <see cref="Milestone"/>-<see cref="Person"/> link from the database.
        /// </summary>
        /// <param name="milestonePerson">The data to be deleted from.</param>
        [OperationContract]
        void DeleteMilestonePerson(MilestonePerson milestonePerson);

        /// <summary>
        /// Retrives the list of the <see cref="Person"/>s for the specified <see cref="Milestone"/>.
        /// </summary>
        /// <param name="milestoneId">An ID of the milestone to the data be retrieved for.</param>
        /// <returns>The list of the <see cref="MilestonePerson"/> objects.</returns>
        [OperationContract]
        List<MilestonePerson> GetMilestonePersonListByMilestoneNoFinancials(int milestoneId);

        [OperationContract]
        List<MilestonePerson> MilestonePersonsByMilestoneForTEByProject(int milestoneId);

        /// <summary>
        /// Computed financials for milestone person
        /// </summary>
        /// <param name="milestonePersonId">An ID of the milestone-person association</param>
        /// <returns>Computed financials</returns>
        [OperationContract]
        MilestonePersonComputedFinancials CalculateMilestonePersonFinancials(int milestonePersonId);

        [OperationContract]
        List<MilestonePerson> GetMilestonePersonsDetailsByMileStoneId(int milestoneId);

        [OperationContract]
        void DeleteMilestonePersonEntry(int milestonePersonEntryId, string userName);

        [OperationContract]
        int UpdateMilestonePersonEntry(MilestonePersonEntry entry, string userName);

        [OperationContract]
        int MilestonePersonEntryInsert(MilestonePersonEntry entry, string userName);

        [OperationContract]
        int MilestonePersonAndEntryInsert(MilestonePerson milestonePerson, string userName);

        [OperationContract]
        MilestonePersonEntry GetMilestonePersonEntry(int mpeid);

        [OperationContract]
        bool IsPersonAlreadyAddedtoMilestone(int mileStoneId, int personId);

        [OperationContract]
        void MilestoneResourceUpdate(Milestone milestone, MilestoneUpdateObject milestoneUpdateObj, string userName);
    }
}
