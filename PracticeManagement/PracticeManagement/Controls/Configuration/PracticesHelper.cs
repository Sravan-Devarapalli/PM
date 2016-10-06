﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using DataTransferObjects;
using PraticeManagement.PracticeService;

namespace PraticeManagement.Controls.Configuration
{
    public class PracticeExtended : Practice
    {
        public PracticeExtended() { }

        public PracticeExtended(Practice practice)
        {
            Id = practice.Id;
            Name = practice.Name;
            PracticeOwner = practice.PracticeOwner;
            IsActive = practice.IsActive;
            IsCompanyInternal = practice.IsCompanyInternal;
            InUse = practice.InUse;
            Abbreviation = practice.Abbreviation;
            IsActiveCapabilitiesExists = practice.IsActiveCapabilitiesExists;
        }

        public int PracticeManagerId
        {
            get { return PracticeOwner.Id.HasValue ? PracticeOwner.Id.Value : -1; }
            set
            {
                PracticeOwner = new Person
                {
                    Id = value
                };
            }
        }

        public Practice BasePractice
        {
            get
            {
                return new Practice
                {
                    Id = Id,
                    Name = Name,
                    Abbreviation = Abbreviation,
                    PracticeOwner = PracticeOwner,
                    IsActive = IsActive,
                    IsCompanyInternal = IsCompanyInternal,
                    IsActiveCapabilitiesExists = IsActiveCapabilitiesExists
                };
            }
        }
    }

    public class PracticesHelper
    {
        public static IEnumerable<PracticeExtended> GetAllPractices()
        {
            var practices = DataHelper.GetPractices(null);

            foreach (var practice in practices)
                yield return new PracticeExtended(practice);
        }

        /// <summary>
        /// Updates practice
        /// </summary>
        /// <returns>A list of <see cref="Practice"/>s in the system</returns>
        public static void UpdatePractice(Practice practice)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    serviceClient.UpdatePractice(practice, DataHelper.CurrentPerson.Alias);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static void UpdatePracticeEx(PracticeExtended practice)
        {
            UpdatePractice(practice.BasePractice);
        }

        /// <summary>
        /// Inserts practice
        /// </summary>
        /// <returns>A list of <see cref="Practice"/>s in the system</returns>
        public static int InsertPractice(string name, string practiceManagerId, bool isActive, bool isInternal, string abbreviation, string personDivisionIds, string projectDivisionIds)
        {
            return InsertPractice(
                new Practice
                {
                    IsActive = isActive,
                    Name = name,
                    Abbreviation = abbreviation,
                    IsCompanyInternal = isInternal,
                    DivisionIds = personDivisionIds,
                    ProjectDivisionIds=projectDivisionIds,
                    PracticeOwner =
                      new Person
                      {
                          Id = Convert.ToInt32(practiceManagerId)
                      },
                      
                });
        }

        /// <summary>
        /// Inserts practice
        /// </summary>
        /// <returns>A list of <see cref="Practice"/>s in the system</returns>
        public static int InsertPractice(Practice practice)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    return serviceClient.InsertPractice(practice,DataHelper.CurrentPerson.Alias);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Removes practice
        /// </summary>
        /// <returns>A list of <see cref="Practice"/>s in the system</returns>
        public static void RemovePracticeEx(PracticeExtended practice)
        {
            RemovePractice(practice.BasePractice);
        }

        /// <summary>
        /// Removes practice
        /// </summary>
        /// <returns>A list of <see cref="Practice"/>s in the system</returns>
        public static void RemovePractice(Practice practice)
        {
            using (var serviceClient = new PracticeServiceClient())
            {
                try
                {
                    serviceClient.RemovePractice(practice, DataHelper.CurrentPerson.Alias);
                }
                catch (CommunicationException)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }
    }
}

