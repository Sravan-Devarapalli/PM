﻿using System;
using PraticeManagement.ActivityLogService;
using PraticeManagement.CalendarService;
using PraticeManagement.ClientService;
using PraticeManagement.MilestonePersonService;
using PraticeManagement.MilestoneService;
using PraticeManagement.OpportunityService;
using PraticeManagement.PersonService;
using PraticeManagement.ProjectGroupService;
using PraticeManagement.ProjectService;
using PraticeManagement.MembershipService;
using PraticeManagement.AuthService;
using PraticeManagement.TimeEntryService;
using PraticeManagement.TimeTypeService;
using PraticeManagement.ReportService;
using PraticeManagement.PersonStatusService;
using PraticeManagement.PersonSkillService;
using PraticeManagement.PracticeService;
using PraticeManagement.TitleService;

namespace PraticeManagement
{
    public class ServiceCallers
    {
        #region Generic

        public static TResult Invoke<TServ, TResult>(Func<TServ, TResult> func)
            where TServ : IDisposable, new()
        {
            using (var client = new TServ())
                return func(client);
        }

        public static void Invoke<TServ>(Action<TServ> action)
            where TServ : IDisposable, new()
        {
            using (var client = new TServ())
                action(client);
        }

        #endregion

        public class Custom
        {
            public static TResult Milestone<TResult>(Func<MilestoneServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Milestone(Action<MilestoneServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult Client<TResult>(Func<ClientServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Client(Action<ClientServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult MilestonePerson<TResult>(Func<MilestonePersonServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void MilestonePerson(Action<MilestonePersonServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult Group<TResult>(Func<ProjectGroupServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static TResult Project<TResult>(Func<ProjectServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Project(Action<ProjectServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult Person<TResult>(Func<PersonServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Person(Action<PersonServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult Opportunity<TResult>(Func<OpportunityServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Opportunity(Action<OpportunityServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult ActivityLog<TResult>(Func<ActivityLogServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void ActivityLog(Action<ActivityLogServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult Membership<TResult>(Func<MembershipServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Membership(Action<MembershipServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult Auth<TResult>(Func<AuthServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Auth(Action<AuthServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult TimeEntry<TResult>(Func<TimeEntryServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void TimeEntry(Action<TimeEntryServiceClient> action)
            {
                Invoke(action);
            }

            public static void TimeType(Action<TimeTypeServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult TimeType<TResult>(Func<TimeTypeServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Report(Action<ReportServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult Report<TResult>(Func<ReportServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static TResult Calendar<TResult>(Func<CalendarServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Calendar(Action<CalendarServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult PersonStatus<TResult>(Func<PersonStatusServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void PersonStatus(Action<PersonStatusServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult PersonSkill<TResult>(Func<PersonSkillServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void PersonSkill(Action<PersonSkillServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult Practice<TResult>(Func<PracticeServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Practice(Action<PracticeServiceClient> action)
            {
                Invoke(action);
            }

            public static TResult Title<TResult>(Func<TitleServiceClient, TResult> func)
            {
                return Invoke(func);
            }

            public static void Title(Action<TitleServiceClient> action)
            {
                Invoke(action);
            }
        }
    }
}

