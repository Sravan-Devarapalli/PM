﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using DataTransferObjects;
using DataTransferObjects.ContextObjects;
using System.Net.Mail;

namespace PracticeManagementService
{
    [ServiceContract]
    public interface IConfigurationService
    {
        [OperationContract]
        List<EmailTemplate> GetAllEmailTemplates();

        [OperationContract]
        EmailTemplate EmailTemplateGetByName(string emailTemplateName);

        [OperationContract]
        bool UpdateEmailTemplate(EmailTemplate template);

        [OperationContract]
        bool AddEmailTemplate(EmailTemplate template);

        [OperationContract]
        bool DeleteEmailTemplate(int templateId);

        [OperationContract]
        EmailData GetEmailData(EmailContext emailContext);

        [OperationContract]
        CompanyLogo GetCompanyLogoData();

        [OperationContract]
        void SaveCompanyLogoData(string title, string imagename, string imagePath, Byte[] data);

        [OperationContract]
        void SaveResourceKeyValuePairs(SettingsType settingType, Dictionary<string, string> dictionary);

        [OperationContract]
        bool SaveResourceKeyValuePairItem(SettingsType settingType, string key, string value);

        [OperationContract]
        Dictionary<string, string> GetResourceKeyValuePairs(SettingsType settingType);

        [OperationContract]
        Boolean VerifySMTPSettings(string MailServer, int PortNumber, bool EnableSSl, string UserName, string Password, string PMSupportEmailAddress);

        [OperationContract]
        void SaveMarginInfoDetail(List<Triple<DefaultGoalType, Triple<SettingsType, string, string>, List<ClientMarginColorInfo>>> marginInfoList);

        [OperationContract]
        List<ClientMarginColorInfo> GetMarginColorInfoDefaults(DefaultGoalType goalType);

        [OperationContract]
        void SavePracticesIsNotesRequiredDetails(string isNotesRequiredPracticeIdsList, string isNotesExemptedPracticeIdsList);

        [OperationContract]
        void SaveQuickLinksForDashBoard(string linkNameList, string virtualPathList, DashBoardType dashBoardType);

        [OperationContract]
        List<QuickLinks> GetQuickLinksByDashBoardType(DashBoardType dashBoardtype);

        [OperationContract]
        void DeleteQuickLinkById(int id);

        [OperationContract]
        void SaveAnnouncement(string text, string richText);

        [OperationContract]
        string GetLatestAnnouncement();

        [OperationContract]
        List<string> GetAllDomains();

        [OperationContract]
        void SendResourceExceptionReportsEmail(DateTime startDate, DateTime endDate, byte[] attachmentByteArray);

        [OperationContract]
        List<RecruitingMetrics> GetRecruitingMetrics(int? recruitingMetricsTypeId);

        [OperationContract]
        void SaveRecruitingMetrics(RecruitingMetrics metric);

        [OperationContract]
        void RecruitingMetricsDelete(int recruitingMetricId);

        [OperationContract]
        void RecruitingMetricsInsert(RecruitingMetrics metrics);

        [OperationContract]
        void SendRecruitingMetricsReportEmail(DateTime startDate, DateTime endDate, byte[] attachmentByteArray);

        [OperationContract]
        List<Lockout> GetLockoutDetails(int? lockoutPageId);

        [OperationContract]
        void SaveLockoutDetails(string lockoutXML);


        [OperationContract]
        List<Location> GetLocations();
    }
}

