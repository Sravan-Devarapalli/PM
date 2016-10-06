﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Web;
using DataTransferObjects;
using DataTransferObjects.Skills;
using DataTransferObjects.TimeEntry;
using Microsoft.WindowsAzure.ServiceRuntime;
using PraticeManagement.ConfigurationService;
using PraticeManagement.TimeTypeService;
using System.IO;
using System.IO.Compression;

namespace PraticeManagement.Utils
{
    public class SettingsHelper
    {
        private const string ApplicationSettingskey = "ApplicationSettings";
        private const string CLIENT_MARGIN_COLORINFO_DEFAULT_THRESHOLDS_LIST_KEY = "CLIENT_MARGIN_COLORINFO_DEFAULT_THRESHOLDS_LIST_KEY";
        private const string PERSON_MARGIN_COLORINFO_THRESHOLDS_LIST_KEY = "PERSON_MARGIN_COLORINFO_THRESHOLDS_LIST_KEY";
        private const string TimeType_System = "Time_Type_System";
        private const string SkillCategory = "SkillCategory";
        private const string Skill = "Skill";
        private const string SkillLevel = "SkillLevel";
        private const string SkillType = "SkillType";
        private const string TitleTypes = "TitleTypes";
        private const string SkillsIndustry = "SkillIndustry";
        private const string Person_TerminationReasons_List_Key = "Person_TerminationReasons_List_Key";
        private const string Person_Domain_List_Key = "Person_Domain_List_Key";
        private const string OpportunitySalesStages = "OpportunitySalesStages";

        public static Dictionary<int, string> DemandOpportunitySalesStages
        {
            get
            {
                if (HttpContext.Current.Cache[OpportunitySalesStages] == null)
                {
                    using (var serviceClient = new OpportunityService.OpportunityServiceClient())
                    {
                        var list = serviceClient.GetOpportunityPriorities(true).ToList();
                        if (list.Any() && list.Any(op => op.Id == Constants.OpportunityPriorityIds.PriorityIdOfA || op.Id == Constants.OpportunityPriorityIds.PriorityIdOfB))
                        {
                            HttpContext.Current.Cache[OpportunitySalesStages] = list.Where(op => op.Id == Constants.OpportunityPriorityIds.PriorityIdOfA || op.Id == Constants.OpportunityPriorityIds.PriorityIdOfB).ToDictionary(o => o.Id, o => o.DisplayName);// .Select(o => new { key = o.Id, value = o.Priority }).ToDictionary(o =>  o.key);
                        }
                    }
                }
                return HttpContext.Current.Cache[OpportunitySalesStages] as Dictionary<int, string>;
            }
            set
            {
                if (value == null)
                    HttpContext.Current.Cache.Remove(OpportunitySalesStages);
            }
        }

        public static Dictionary<string, string> GetResourceKeyValuePairs(SettingsType settingType)
        {
            using (var serviceClient = new ConfigurationServiceClient())
            {
                try
                {
                    return serviceClient.GetResourceKeyValuePairs(settingType);
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static string GetResourceValueByTypeAndKey(SettingsType settingType, string key)
        {
            if (HttpContext.Current.Cache[ApplicationSettingskey] == null)
            {
                HttpContext.Current.Cache[ApplicationSettingskey] = new Dictionary<SettingsType, Dictionary<string, string>>();
            }
            var mainDictionary = HttpContext.Current.Cache[ApplicationSettingskey] as Dictionary<SettingsType, Dictionary<string, string>>;
            if (!mainDictionary.Any(kvp => kvp.Key == settingType))
            {
                mainDictionary[settingType] = GetResourceKeyValuePairs(settingType);
            }

            if (mainDictionary[settingType].Any(kvp => kvp.Key == key))
            {
                return mainDictionary[settingType][key] as string;
            }
            else
            {
                return string.Empty;
            }
        }

        public static void SaveResourceKeyValuePairs(SettingsType settingType, Dictionary<string, string> dictionary)
        {
            using (var serviceClient = new ConfigurationServiceClient())
            {
                try
                {
                    serviceClient.SaveResourceKeyValuePairs(settingType, dictionary);
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static SMTPSettings GetSMTPSettings()
        {
            var sMTPSettings = new SMTPSettings();
            sMTPSettings.MailServer = SettingsHelper.GetResourceValueByTypeAndKey(SettingsType.SMTP, Constants.ResourceKeys.MailServerKey);

            var sslEnabledString = SettingsHelper.GetResourceValueByTypeAndKey(SettingsType.SMTP, Constants.ResourceKeys.SSLEnabledKey);
            bool sSLEnabled;
            if (!string.IsNullOrEmpty(sslEnabledString) && bool.TryParse(sslEnabledString, out sSLEnabled))
            {
                sMTPSettings.SSLEnabled = sSLEnabled;
            }

            int portNumber;
            var portNumberString = SettingsHelper.GetResourceValueByTypeAndKey(SettingsType.SMTP, Constants.ResourceKeys.PortNumberKey);
            if (!string.IsNullOrEmpty(portNumberString) && Int32.TryParse(portNumberString, out portNumber))
            {
                sMTPSettings.PortNumber = portNumber;
            }
            else
            {
                sMTPSettings.PortNumber = sMTPSettings.SSLEnabled ? 25 : 465;
            }

            sMTPSettings.UserName =
                SettingsHelper.GetResourceValueByTypeAndKey(SettingsType.SMTP, Constants.ResourceKeys.UserNameKey);

            sMTPSettings.Password =
                SettingsHelper.GetResourceValueByTypeAndKey(SettingsType.SMTP, Constants.ResourceKeys.PasswordKey);

            sMTPSettings.PMSupportEmail =
                SettingsHelper.GetResourceValueByTypeAndKey(SettingsType.SMTP, Constants.ResourceKeys.PMSupportEmailAddressKey);

            return sMTPSettings;
        }

        public static void SaveSMTPSettings(SMTPSettings sMTPSettings)
        {
            SaveResourceKeyValuePairItem(SettingsType.SMTP, Constants.ResourceKeys.MailServerKey, sMTPSettings.MailServer);
            SaveResourceKeyValuePairItem(SettingsType.SMTP, Constants.ResourceKeys.PortNumberKey, sMTPSettings.PortNumber.ToString());
            SaveResourceKeyValuePairItem(SettingsType.SMTP, Constants.ResourceKeys.SSLEnabledKey, sMTPSettings.SSLEnabled.ToString());
            SaveResourceKeyValuePairItem(SettingsType.SMTP, Constants.ResourceKeys.UserNameKey, sMTPSettings.UserName);
            SaveResourceKeyValuePairItem(SettingsType.SMTP, Constants.ResourceKeys.PasswordKey, sMTPSettings.Password);
            SaveResourceKeyValuePairItem(SettingsType.SMTP, Constants.ResourceKeys.PMSupportEmailAddressKey, sMTPSettings.PMSupportEmail);
        }

        public static void SaveResourceKeyValuePairItem(SettingsType settingType, string key, string value)
        {
            if (HttpContext.Current.Cache[ApplicationSettingskey] == null)
            {
                HttpContext.Current.Cache[ApplicationSettingskey] = new Dictionary<SettingsType, Dictionary<string, string>>();
            }
            var mainDictionary = HttpContext.Current.Cache[ApplicationSettingskey] as Dictionary<SettingsType, Dictionary<string, string>>;
            if (!mainDictionary.Any(kvp => kvp.Key == settingType))
            {
                mainDictionary[settingType] = GetResourceKeyValuePairs(settingType);
            }

            mainDictionary[settingType][key] = value;

            using (var serviceClient = new ConfigurationServiceClient())
            {
                try
                {
                    serviceClient.SaveResourceKeyValuePairItem(settingType, key, value);
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        public static List<ClientMarginColorInfo> GetMarginColorInfoDefaults(DefaultGoalType goaltype)
        {
            if (goaltype == DefaultGoalType.Client && HttpContext.Current.Cache[CLIENT_MARGIN_COLORINFO_DEFAULT_THRESHOLDS_LIST_KEY] != null)
            {
                return HttpContext.Current.Cache[CLIENT_MARGIN_COLORINFO_DEFAULT_THRESHOLDS_LIST_KEY] as List<ClientMarginColorInfo>;
            }
            else if (HttpContext.Current.Cache[PERSON_MARGIN_COLORINFO_THRESHOLDS_LIST_KEY] != null)
            {
                return HttpContext.Current.Cache[PERSON_MARGIN_COLORINFO_THRESHOLDS_LIST_KEY] as List<ClientMarginColorInfo>;
            }

            using (var serviceClient = new ConfigurationServiceClient())
            {
                try
                {
                    var result = serviceClient.GetMarginColorInfoDefaults(goaltype);

                    if (result != null)
                    {
                        var marginInfoList = result.AsQueryable().ToList();

                        if (goaltype == DefaultGoalType.Client)
                        {
                            HttpContext.Current.Cache[CLIENT_MARGIN_COLORINFO_DEFAULT_THRESHOLDS_LIST_KEY] = marginInfoList;
                        }
                        else
                        {
                            HttpContext.Current.Cache[PERSON_MARGIN_COLORINFO_THRESHOLDS_LIST_KEY] = marginInfoList;
                        }

                        return marginInfoList;
                    }

                    return null;
                }
                catch (FaultException<ExceptionDetail> ex)
                {
                    serviceClient.Abort();
                    throw;
                }
            }
        }

        internal static void RemoveMarginColorInfoDefaults()
        {
            HttpContext.Current.Cache.Remove(CLIENT_MARGIN_COLORINFO_DEFAULT_THRESHOLDS_LIST_KEY);
            HttpContext.Current.Cache.Remove(PERSON_MARGIN_COLORINFO_THRESHOLDS_LIST_KEY);
        }

        public static DateTime GetCurrentPMTime()
        {
            DateTime currentDate;

            var timezone = SettingsHelper.GetResourceValueByTypeAndKey(SettingsType.Application, Constants.ResourceKeys.TimeZoneKey);
            var isDayLightSavingsTimeEffect = SettingsHelper.GetResourceValueByTypeAndKey(SettingsType.Application, Constants.ResourceKeys.IsDayLightSavingsTimeEffectKey);

            if (timezone == "-08:00" && isDayLightSavingsTimeEffect.ToLower() == "true")
            {
                currentDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
            }
            else
            {
                var timezoneWithoutSign = timezone.Replace("+", string.Empty);
                TimeZoneInfo ctz = TimeZoneInfo.CreateCustomTimeZone("cid", TimeSpan.Parse(timezoneWithoutSign), "customzone", "customzone");
                currentDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, ctz);
            }

            return currentDate;
        }

        public static List<TimeTypeRecord> GetSystemTimeTypes()
        {
            if (HttpContext.Current.Cache[TimeType_System] == null)
            {
                using (var serviceClient = new TimeTypeServiceClient())
                {
                    HttpContext.Current.Cache[TimeType_System] = serviceClient.GetAllTimeTypes().Where(tt => tt.IsSystemTimeType).ToList();
                }
            }

            return HttpContext.Current.Cache[TimeType_System] as List<TimeTypeRecord>;
        }

        public static List<Skill> GetSkillsByCategory(int skillCategoryId)
        {
            if (HttpContext.Current.Cache[Skill] == null)
            {
                using (var serviceClient = new PersonSkillService.PersonSkillServiceClient())
                {
                    HttpContext.Current.Cache[Skill] = serviceClient.SkillsAll().ToList();
                }
            }
            var skills = HttpContext.Current.Cache[Skill] as List<Skill>;
            return skills.FindAll(s => s.Category != null && s.Category.Id == skillCategoryId);
        }

        public static List<SkillCategory> GetSkillCategoriesByType(int skillTypeId)
        {
            if (HttpContext.Current.Cache[SkillCategory] == null)
            {
                using (var serviceClient = new PersonSkillService.PersonSkillServiceClient())
                {
                    HttpContext.Current.Cache[SkillCategory] = serviceClient.SkillCategoriesAll().ToList();
                }
            }
            var skillCats = HttpContext.Current.Cache[SkillCategory] as List<SkillCategory>;
            return skillCats.FindAll(s => s.SkillType != null && s.SkillType.Id == skillTypeId);
        }

        public static List<Industry> GetIndustrySkillsAll()
        {
            if (HttpContext.Current.Cache[SkillsIndustry] == null)
            {
                using (var serviceClient = new PersonSkillService.PersonSkillServiceClient())
                {
                    HttpContext.Current.Cache[SkillsIndustry] = serviceClient.GetIndustrySkillsAll().ToList();
                }
            }
            return HttpContext.Current.Cache[SkillsIndustry] as List<Industry>;
        }

        public static List<SkillLevel> GetSkillLevels()
        {
            if (HttpContext.Current.Cache[SkillLevel] == null)
            {
                using (var serviceClient = new PersonSkillService.PersonSkillServiceClient())
                {
                    HttpContext.Current.Cache[SkillLevel] = serviceClient.SkillLevelsAll().ToList();
                }
            }
            return HttpContext.Current.Cache[SkillLevel] as List<SkillLevel>;
        }

        public static List<SkillType> GetSkillTypes()
        {
            if (HttpContext.Current.Cache[SkillType] == null)
            {
                using (var serviceClient = new PersonSkillService.PersonSkillServiceClient())
                {
                    HttpContext.Current.Cache[SkillType] = serviceClient.SkillTypesAll().AsQueryable().ToList();
                }
            }
            return HttpContext.Current.Cache[SkillType] as List<SkillType>;
        }

        public static List<TerminationReason> GetTerminationReasonsList()
        {
            if (HttpContext.Current.Cache[Person_TerminationReasons_List_Key] == null)
            {
                using (var serviceClient = new PersonService.PersonServiceClient())
                {
                    HttpContext.Current.Cache[Person_TerminationReasons_List_Key] = serviceClient.GetTerminationReasonsList().ToList();
                }
            }
            return HttpContext.Current.Cache[Person_TerminationReasons_List_Key] as List<TerminationReason>;
        }

        public static string[] GetDomainsList()
        {
            if (HttpContext.Current.Cache[Person_Domain_List_Key] == null)
            {
                HttpContext.Current.Cache[Person_Domain_List_Key] = ServiceCallers.Invoke<ConfigurationServiceClient, string[]>(c => c.GetAllDomains()).ToArray();
            }
            return HttpContext.Current.Cache[Person_Domain_List_Key] as string[];
        }

        public static TitleType[] GetTitleTypes()
        {
            if (HttpContext.Current.Cache[TitleTypes] == null)
            {
                HttpContext.Current.Cache[TitleTypes] = ServiceCallers.Custom.Title(t => t.GetTitleTypes()).ToArray();
            }
            return HttpContext.Current.Cache[TitleTypes] as TitleType[];
        }

        public static byte[] Compress(byte[] b)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream zs = new GZipStream(ms, CompressionMode.Compress, true);
            zs.Write(b, 0, b.Length);
            zs.Close();
            return ms.ToArray();
        }

        public static byte[] Decompress(byte[] b)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream zs = new GZipStream(new MemoryStream(b),
                                           CompressionMode.Decompress, true);
            byte[] buffer = new byte[4096];
            int size;
            while (true)
            {
                size = zs.Read(buffer, 0, buffer.Length);
                if (size > 0)
                    ms.Write(buffer, 0, size);
                else break;
            }
            zs.Close();
            return ms.ToArray();
        }
    }
}
