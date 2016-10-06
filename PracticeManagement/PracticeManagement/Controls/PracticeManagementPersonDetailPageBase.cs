﻿using DataTransferObjects;

namespace PraticeManagement.Controls
{
    public abstract class PracticeManagementPersonDetailPageBase : PracticeManagementPageBase
    {
        protected override void Display()
        {

        }

        /// <summary>
        /// To transfer the unsaved Person object to other(CompensationDetail) page
        /// </summary>
        public abstract Person PersonUnsavedData
        {
            get;
            set;
        }

        public abstract PersonPermission Permissions { get; set; }
    }
}

