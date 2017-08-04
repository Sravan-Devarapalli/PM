﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using DataAccess;
using DataTransferObjects;
using DataTransferObjects.ContextObjects;
using DataTransferObjects.Financials;

namespace PracticeManagementService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class MilestonePersonService : IMilestonePersonService
    {
        #region IMilestonePersonService Members

        /// <summary>
        /// Gets milestones info for given person
        /// </summary>
        public List<MilestonePersonEntry> GetConsultantMilestones(ConsultantMilestonesContext context)
        {
            return MilestonePersonDAL.GetConsultantMilestones(context);
        }

        /// <summary>
        /// Retrives the list of the <see cref="Person"/>s for the specified <see cref="Project"/>.
        /// </summary>
        /// <param name="projectId">An ID of the project to the data be retrieved for.</param>
        /// <returns>The list of the <see cref="MilestonePerson"/> objects.</returns>
        public List<MilestonePerson> GetMilestonePersonListByProject(int projectId)
        {
            List<MilestonePerson> result = MilestonePersonDAL.MilestonePersonListByProject(projectId, false);
            foreach (MilestonePerson milestonePerson in result)
                if (milestonePerson.Person.Id != null)
                    milestonePerson.Person.CurrentPay = result.Any(mp => mp.Person.Id == milestonePerson.Person.Id && mp.Person.CurrentPay != null)
                                                            ? result.First(mp => mp.Person.Id == milestonePerson.Person.Id && mp.Person.CurrentPay != null).Person.CurrentPay
                                                            : PayDAL.GetCurrentByPerson(milestonePerson.Person.Id.Value);

            return result;
        }

        /// <summary>
        /// Retrives the list of the <see cref="Person"/>s for the specified <see cref="Project"/>.
        /// </summary>
        /// <param name="projectId">An ID of the project to the data be retrieved for.</param>
        /// <returns>The list of the <see cref="MilestonePerson"/> objects.</returns>
        public List<MilestonePerson> GetMilestonePersonListByProjectWithoutPay(int projectId)
        {
            return MilestonePersonDAL.MilestonePersonListByProject(projectId);
        }

        /// <summary>
        /// Retrives the list of the <see cref="Person"/>s for the specified <see cref="Milestone"/>.
        /// </summary>
        /// <param name="milestoneId">An ID of the milestone to the data be retrieved for.</param>
        /// <returns>The list of the <see cref="MilestonePerson"/> objects.</returns>
        public List<MilestonePerson> GetMilestonePersonListByMilestone(int milestoneId)
        {
            return PersonRateCalculator.GetMilestonePersonListByMilestone(milestoneId);
        }

        /// <summary>
        /// Retrives the list of the <see cref="Person"/>s for the specified <see cref="Milestone"/>.
        /// </summary>
        /// <param name="milestoneId">An ID of the milestone to the data be retrieved for.</param>
        /// <returns>The list of the <see cref="MilestonePerson"/> objects.</returns>
        public List<MilestonePerson> GetMilestonePersonListByMilestoneNoFinancials(int milestoneId)
        {
            return PersonRateCalculator.GetMilestonePersonListByMilestoneNoFinancials(milestoneId);
        }

        public List<MilestonePerson> MilestonePersonsByMilestoneForTEByProject(int milestoneId)
        {
            return MilestonePersonDAL.MilestonePersonsByMilestoneForTEByProject(milestoneId);
        }

        public MilestonePersonComputedFinancials CalculateMilestonePersonFinancials(int milestonePersonId)
        {
            return ComputedFinancialsDAL.CalculateMilestonePersonFinancials(milestonePersonId);
        }

        /// <summary>
        /// Retrives the milestone-person link details for the specified <see cref="Milestone"/> and
        /// <see cref="Person"/>
        /// </summary>
        /// <param name="milestonePersonId">An ID of the milestone-person association.</param>
        /// <returns>The <see cref="MilestonePerson"/> object if found and null otherwise.</returns>
        public MilestonePerson GetMilestonePersonDetail(int milestonePersonId)
        {
            MilestonePerson result = MilestonePersonDAL.MilestonePersonGetByMilestonePersonId(milestonePersonId);
            if (result != null)
            {
                result.Entries = MilestonePersonDAL.MilestonePersonEntryListByMilestonePersonId(milestonePersonId);

                if (result.Milestone != null && result.Milestone.Id.HasValue &&
                    result.Person != null && result.Person.Id.HasValue)
                {
                    // Financials for each entry
                    foreach (MilestonePersonEntry entry in result.Entries)
                    {
                        entry.ComputedFinancials =
                            ComputedFinancialsDAL.FinancialsGetByMilestonePersonEntry(entry.Id);
                    }
                }

                // Financials for the milestone person assignment
                if (result.Milestone != null && result.Person != null && result.Milestone.Id != null && result.Person.Id != null)
                    result.ComputedFinancials =
                        ComputedFinancialsDAL.FinancialsGetByMilestonePerson(
                            result.Milestone.Id.Value,
                            result.Person.Id.Value);
            }
            return result;
        }

        public MilestonePersonEntry GetMilestonePersonEntry(int mpeid)
        {
            return MilestonePersonDAL.LoadMilestonePersonEntryWithFinancials(mpeid);
        }

        public List<MilestonePerson> GetMilestonePersonsDetailsByMileStoneId(int milestoneId)
        {
            List<MilestonePerson> result = MilestonePersonDAL.MilestonePersonsGetByMilestoneId(milestoneId);

            if (result != null)
            {
                MilestonePersonDAL.LoadMilestonePersonEntriesWithFinancials(result, milestoneId);
            }

            return result;
        }

        /// <summary>
        /// Checks whether there are any time entries for a given MilestonepersonId in between the given startdate and enddate.
        /// </summary>
        /// <param name="milestonePersonId"></param>
        /// <returns></returns>
        public bool CheckTimeEntriesForMilestonePerson(int milestonePersonId, DateTime? startDate, DateTime? endDate,
                                                        bool checkStartDateEquality, bool checkEndDateEquality)
        {
            return MilestonePersonDAL.CheckTimeEntriesForMilestonePerson(milestonePersonId, startDate, endDate,
                                                                            checkStartDateEquality, checkEndDateEquality);
        }

        /// <summary>
        /// Checks whether there are any time entries for a given MilestonepersonId for the given MilestonePersonRoleId.
        /// </summary>
        /// <param name="milestonePersonId"></param>
        /// <param name="milestonePersonRoleId"></param>
        /// <returns></returns>
        public bool CheckTimeEntriesForMilestonePersonWithGivenRoleId(int milestonePersonId, int? milestonePersonRoleId)
        {
            return MilestonePersonDAL.CheckTimeEntriesForMilestonePersonWithGivenRoleId(milestonePersonId, milestonePersonRoleId);
        }

        /// <summary>
        /// Saves the specified <see cref="Milestone"/>-<see cref="Person"/> link to the database.
        /// </summary>
        /// <param name="milestonePerson">The data to be saved to.</param>
        public void SaveMilestonePerson(ref MilestonePerson milestonePerson, string userName)
        {
            MilestonePersonDAL.SaveMilestonePersonWrapper(milestonePerson, userName);
        }

        public void SaveMilestonePersons(List<MilestonePerson> milestonePersons, string userName)
        {
            MilestonePersonDAL.SaveMilestonePersonsWrapper(milestonePersons, userName);
        }

        /// <summary>
        /// Deletes the specified <see cref="Milestone"/>-<see cref="Person"/> link from the database.
        /// </summary>
        /// <param name="milestonePerson">The data to be deleted from.</param>
        public void DeleteMilestonePerson(MilestonePerson milestonePerson)
        {
            MilestonePersonDAL.MilestonePersonDelete(milestonePerson);
        }

        public void DeleteMilestonePersonEntry(int milestonePersonEntryId, string userName, int? milestoneId)
        {
            MilestonePersonDAL.DeleteMilestonePersonEntry(milestonePersonEntryId, userName);
        }

        public int UpdateMilestonePersonEntry(MilestonePersonEntry entry, string userName)
        {
            var result = MilestonePersonDAL.UpdateMilestonePersonEntry(entry, userName);
            return result;
        }

        public int MilestonePersonEntryInsert(MilestonePersonEntry entry, string userName)
        {
            var result = MilestonePersonDAL.MilestonePersonEntryInsert(entry, userName);
            return result;
        }

        public int MilestonePersonAndEntryInsert(MilestonePerson milestonePerson, string userName)
        {
            var result = MilestonePersonDAL.MilestonePersonAndEntryInsert(milestonePerson, userName);
            return result;
        }

        public bool IsPersonAlreadyAddedtoMilestone(int mileStoneId, int personId)
        {
            return MilestonePersonDAL.IsPersonAlreadyAddedtoMilestone(mileStoneId, personId);
        }

        public void MilestoneResourceUpdate(Milestone milestone, MilestoneUpdateObject milestoneUpdateObj, string userName)
        {
            MilestonePersonDAL.MilestoneResourceUpdate(milestone, milestoneUpdateObj, userName);
            MilestoneDAL.UpdateMilestoneDiscount(milestone.Id.Value);

        }

        #endregion IMilestonePersonService Members
    }
}
