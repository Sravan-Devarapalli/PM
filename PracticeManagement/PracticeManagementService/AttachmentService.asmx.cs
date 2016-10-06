﻿using System;
using System.Web;
using System.Web.Services;
using DataAccess;
using DataAccess.Skills;
using DataTransferObjects;

namespace PracticeManagementService
{
    /// <summary>
    /// Summary description for AttachmentService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    // [System.Web.Script.Services.ScriptService]
    public class AttachmentService : System.Web.Services.WebService
    {
        [WebMethod]
        public void SaveProjectAttachment(ProjectAttachment sow, int projectId, string userName)
        {
            if (sow != null)
            {
                ProjectDAL.SaveProjectAttachmentData(sow, projectId, userName);
            }
        }

        [WebMethod]
        public byte[] GetProjectAttachmentData(int projectId, int attachmentId)
        {
            return ProjectDAL.GetProjectAttachmentData(projectId, attachmentId);
        }

        [WebMethod]
        public void DeleteProjectAttachmentByProjectId(int? attachmentId, int projectId, string userName)
        {
            ProjectDAL.DeleteProjectAttachmentByProjectId(attachmentId, projectId, userName);
        }

        [WebMethod]
        public void SavePersonPicture(int personId, byte[] pictureData, string userLogin, string pictureFileName)
        {
            PersonSkillDAL.SavePersonPicture(personId, pictureData, userLogin, pictureFileName);
        }

        [WebMethod]
        public byte[] GetPersonPicture(int personId)
        {
            try
            {
                return PersonSkillDAL.GetPersonPicture(personId);
            }
            catch (Exception e)
            {
                string logData = string.Format(DataTransferObjects.Constants.Formatting.ErrorLogMessage, "GetPersonrPicture", "AttachmentService.asmx", string.Empty,
                   HttpUtility.HtmlEncode(e.Message), e.Source, e.InnerException == null ? string.Empty : HttpUtility.HtmlEncode(e.InnerException.Message), e.InnerException == null ? string.Empty : e.InnerException.Source);
                ActivityLogDAL.ActivityLogInsert(20, logData);
                throw e;
            }
        }
    }
}

