﻿using System.Collections.Generic;
using DataTransferObjects;

namespace PraticeManagement.Utils
{
    public static class ProjectHelper
    {
        private static readonly IDictionary<ProjectStatusType, string> _projectStatuses
           = new Dictionary<ProjectStatusType, string>
                 {
                    {ProjectStatusType.Projected, "ProjectedProject"},
                    {ProjectStatusType.Experimental, "ExperimentalProject"},
                    {ProjectStatusType.Completed, "CompletedProject"},
                    {ProjectStatusType.Active, "pr-indicator"},
                    {ProjectStatusType.Internal, "pr-internal"},
                    {ProjectStatusType.Inactive, "InactiveProject"},
                    {ProjectStatusType.Proposed,"ProposedProject"}
                };

        public static string GetIndicatorClassByStatusId(int statusId)
        {
            return !_projectStatuses.ContainsKey((ProjectStatusType)statusId) ? null : _projectStatuses[(ProjectStatusType)statusId];
        }
    }
}
