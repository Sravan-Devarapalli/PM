﻿using System.Collections.Generic;
using System.ServiceModel;
using DataTransferObjects;
using DataTransferObjects.Skills;

namespace PracticeManagementService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPersonSkillService" in both code and config file together.
    [ServiceContract]
    public interface IPersonSkillService
    {
        //[OperationContract]
        //List<SkillType> GetSkillStatuses();

        [OperationContract]
        List<SkillCategory> SkillCategoriesAll();

        [OperationContract]
        List<Skill> SkillsAll();

        [OperationContract]
        List<SkillLevel> SkillLevelsAll();

        [OperationContract]
        List<SkillType> SkillTypesAll();

        [OperationContract]
        List<Industry> GetIndustrySkillsAll();

        [OperationContract]
        Person GetPersonProfilesWithSkills(int personId);

        [OperationContract]
        List<Profile> GetPersonProfiles(int personId);

        [OperationContract]
        Person GetPersonWithSkills(int personId);

        [OperationContract]
        void SavePersonSkills(int personId, string skillsXml, string userLogin);

        [OperationContract]
        void SavePersonIndustrySkills(int personId, string industrySkillsXml, string userLogin);

        [OperationContract]
        List<Person> PersonsSearchBySkills(string skillsSearchXML);

        [OperationContract]
        void SavePersonProfiles(int personId, string profilesXml, string userLogin);
    }
}
