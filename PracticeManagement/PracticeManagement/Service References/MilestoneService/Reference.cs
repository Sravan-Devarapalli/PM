﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PraticeManagement.MilestoneService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="MilestoneService.IMilestoneService")]
    public interface IMilestoneService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/SaveDefaultMilestone", ReplyAction="http://tempuri.org/IMilestoneService/SaveDefaultMilestoneResponse")]
        void SaveDefaultMilestone(System.Nullable<int> clientId, System.Nullable<int> projectId, System.Nullable<int> milestoneId, System.Nullable<int> lowerBound, System.Nullable<int> upperBound);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetDefaultMilestone", ReplyAction="http://tempuri.org/IMilestoneService/GetDefaultMilestoneResponse")]
        DataTransferObjects.DefaultMilestone GetDefaultMilestone();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/MilestoneListByProject", ReplyAction="http://tempuri.org/IMilestoneService/MilestoneListByProjectResponse")]
        DataTransferObjects.Milestone[] MilestoneListByProject(int projectId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/MilestoneListByProjectForTimeEntryByProjectR" +
            "eport", ReplyAction="http://tempuri.org/IMilestoneService/MilestoneListByProjectForTimeEntryByProjectR" +
            "eportResponse")]
        DataTransferObjects.Milestone[] MilestoneListByProjectForTimeEntryByProjectReport(int projectId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetMilestoneDetail", ReplyAction="http://tempuri.org/IMilestoneService/GetMilestoneDetailResponse")]
        DataTransferObjects.Milestone GetMilestoneDetail(int milestoneId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/SaveMilestoneDetail", ReplyAction="http://tempuri.org/IMilestoneService/SaveMilestoneDetailResponse")]
        int SaveMilestoneDetail(DataTransferObjects.Milestone milestone, string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/DeleteMilestone", ReplyAction="http://tempuri.org/IMilestoneService/DeleteMilestoneResponse")]
        void DeleteMilestone(DataTransferObjects.Milestone milestone, string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/MilestoneMove", ReplyAction="http://tempuri.org/IMilestoneService/MilestoneMoveResponse")]
        DataTransferObjects.MSBadge[] MilestoneMove(int milestoneId, int shiftDays, bool moveFutureMilestones);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/MilestoneMoveEnd", ReplyAction="http://tempuri.org/IMilestoneService/MilestoneMoveEndResponse")]
        void MilestoneMoveEnd(int milestoneId, int milestonePersonId, int shiftDays);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/MilestoneClone", ReplyAction="http://tempuri.org/IMilestoneService/MilestoneCloneResponse")]
        int MilestoneClone(int milestoneId, int cloneDuration);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetMilestoneById", ReplyAction="http://tempuri.org/IMilestoneService/GetMilestoneByIdResponse")]
        DataTransferObjects.Milestone GetMilestoneById(int milestoneId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetMilestoneDataById", ReplyAction="http://tempuri.org/IMilestoneService/GetMilestoneDataByIdResponse")]
        DataTransferObjects.Milestone GetMilestoneDataById(int milestoneId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/CheckIfExpensesExistsForMilestonePeriod", ReplyAction="http://tempuri.org/IMilestoneService/CheckIfExpensesExistsForMilestonePeriodRespo" +
            "nse")]
        bool CheckIfExpensesExistsForMilestonePeriod(int milestoneId, System.Nullable<System.DateTime> startDate, System.Nullable<System.DateTime> endDate);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/CanMoveFutureMilestones", ReplyAction="http://tempuri.org/IMilestoneService/CanMoveFutureMilestonesResponse")]
        bool CanMoveFutureMilestones(int milestoneId, int shiftDays);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/AddProjectExpense", ReplyAction="http://tempuri.org/IMilestoneService/AddProjectExpenseResponse")]
        int AddProjectExpense(DataTransferObjects.ProjectExpense entity);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/RemoveProjectExpense", ReplyAction="http://tempuri.org/IMilestoneService/RemoveProjectExpenseResponse")]
        void RemoveProjectExpense(DataTransferObjects.ProjectExpense entity);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/UpdateProjectExpense", ReplyAction="http://tempuri.org/IMilestoneService/UpdateProjectExpenseResponse")]
        void UpdateProjectExpense(DataTransferObjects.ProjectExpense entity);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/NoteListByTargetId", ReplyAction="http://tempuri.org/IMilestoneService/NoteListByTargetIdResponse")]
        DataTransferObjects.Note[] NoteListByTargetId(int targetId, int noteTargetId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/NoteInsert", ReplyAction="http://tempuri.org/IMilestoneService/NoteInsertResponse")]
        int NoteInsert(DataTransferObjects.Note note);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/NoteUpdate", ReplyAction="http://tempuri.org/IMilestoneService/NoteUpdateResponse")]
        void NoteUpdate(DataTransferObjects.Note note);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/NoteDelete", ReplyAction="http://tempuri.org/IMilestoneService/NoteDeleteResponse")]
        void NoteDelete(int noteId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetProjectExpenseById", ReplyAction="http://tempuri.org/IMilestoneService/GetProjectExpenseByIdResponse")]
        DataTransferObjects.ProjectExpense GetProjectExpenseById(int id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetProjectExpensesForMilestone", ReplyAction="http://tempuri.org/IMilestoneService/GetProjectExpensesForMilestoneResponse")]
        DataTransferObjects.ProjectExpense[] GetProjectExpensesForMilestone(DataTransferObjects.ProjectExpense entity);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/MilestoneUpdateShortDetails", ReplyAction="http://tempuri.org/IMilestoneService/MilestoneUpdateShortDetailsResponse")]
        void MilestoneUpdateShortDetails(DataTransferObjects.Milestone milestone, string userName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetMilestoneAndCSATCountsByProject", ReplyAction="http://tempuri.org/IMilestoneService/GetMilestoneAndCSATCountsByProjectResponse")]
        int[] GetMilestoneAndCSATCountsByProject(int projectId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/IsProjectAttributionConflictsWithMilestoneCh" +
            "anges", ReplyAction="http://tempuri.org/IMilestoneService/IsProjectAttributionConflictsWithMilestoneCh" +
            "angesResponse")]
        DataTransferObjects.Attribution[] IsProjectAttributionConflictsWithMilestoneChanges(int milestoneId, System.DateTime startDate, System.DateTime endDate, bool isUpdate);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/ShouldAttributionDateExtend", ReplyAction="http://tempuri.org/IMilestoneService/ShouldAttributionDateExtendResponse")]
        bool[] ShouldAttributionDateExtend(int milestoneId, System.DateTime startDate, System.DateTime endDate);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetPersonMilestonesOnPreviousHireDate", ReplyAction="http://tempuri.org/IMilestoneService/GetPersonMilestonesOnPreviousHireDateRespons" +
            "e")]
        DataTransferObjects.Milestone[] GetPersonMilestonesOnPreviousHireDate(int personId, System.DateTime previousHireDate);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/SendBadgeRequestMail", ReplyAction="http://tempuri.org/IMilestoneService/SendBadgeRequestMailResponse")]
        void SendBadgeRequestMail(DataTransferObjects.Project project, int milestoneId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/SendBadgeRequestApprovedMail", ReplyAction="http://tempuri.org/IMilestoneService/SendBadgeRequestApprovedMailResponse")]
        void SendBadgeRequestApprovedMail(string personName, string toAddress);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetPeopleAssignedInOtherProjectsForGivenRang" +
            "e", ReplyAction="http://tempuri.org/IMilestoneService/GetPeopleAssignedInOtherProjectsForGivenRang" +
            "eResponse")]
        DataTransferObjects.MSBadge[] GetPeopleAssignedInOtherProjectsForGivenRange(System.DateTime milestoneNewStartDate, System.DateTime milestoneNewEnddate, int milestoneId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMilestoneService/GetAllExpenseTypesList", ReplyAction="http://tempuri.org/IMilestoneService/GetAllExpenseTypesListResponse")]
        DataTransferObjects.ExpenseType[] GetAllExpenseTypesList();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IMilestoneServiceChannel : PraticeManagement.MilestoneService.IMilestoneService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MilestoneServiceClient : System.ServiceModel.ClientBase<PraticeManagement.MilestoneService.IMilestoneService>, PraticeManagement.MilestoneService.IMilestoneService {
        
        public MilestoneServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MilestoneServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MilestoneServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MilestoneServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void SaveDefaultMilestone(System.Nullable<int> clientId, System.Nullable<int> projectId, System.Nullable<int> milestoneId, System.Nullable<int> lowerBound, System.Nullable<int> upperBound) {
            base.Channel.SaveDefaultMilestone(clientId, projectId, milestoneId, lowerBound, upperBound);
        }
        
        public DataTransferObjects.DefaultMilestone GetDefaultMilestone() {
            return base.Channel.GetDefaultMilestone();
        }
        
        public DataTransferObjects.Milestone[] MilestoneListByProject(int projectId) {
            return base.Channel.MilestoneListByProject(projectId);
        }
        
        public DataTransferObjects.Milestone[] MilestoneListByProjectForTimeEntryByProjectReport(int projectId) {
            return base.Channel.MilestoneListByProjectForTimeEntryByProjectReport(projectId);
        }
        
        public DataTransferObjects.Milestone GetMilestoneDetail(int milestoneId) {
            return base.Channel.GetMilestoneDetail(milestoneId);
        }
        
        public int SaveMilestoneDetail(DataTransferObjects.Milestone milestone, string userName) {
            return base.Channel.SaveMilestoneDetail(milestone, userName);
        }
        
        public void DeleteMilestone(DataTransferObjects.Milestone milestone, string userName) {
            base.Channel.DeleteMilestone(milestone, userName);
        }
        
        public DataTransferObjects.MSBadge[] MilestoneMove(int milestoneId, int shiftDays, bool moveFutureMilestones) {
            return base.Channel.MilestoneMove(milestoneId, shiftDays, moveFutureMilestones);
        }
        
        public void MilestoneMoveEnd(int milestoneId, int milestonePersonId, int shiftDays) {
            base.Channel.MilestoneMoveEnd(milestoneId, milestonePersonId, shiftDays);
        }
        
        public int MilestoneClone(int milestoneId, int cloneDuration) {
            return base.Channel.MilestoneClone(milestoneId, cloneDuration);
        }
        
        public DataTransferObjects.Milestone GetMilestoneById(int milestoneId) {
            return base.Channel.GetMilestoneById(milestoneId);
        }
        
        public DataTransferObjects.Milestone GetMilestoneDataById(int milestoneId) {
            return base.Channel.GetMilestoneDataById(milestoneId);
        }
        
        public bool CheckIfExpensesExistsForMilestonePeriod(int milestoneId, System.Nullable<System.DateTime> startDate, System.Nullable<System.DateTime> endDate) {
            return base.Channel.CheckIfExpensesExistsForMilestonePeriod(milestoneId, startDate, endDate);
        }
        
        public bool CanMoveFutureMilestones(int milestoneId, int shiftDays) {
            return base.Channel.CanMoveFutureMilestones(milestoneId, shiftDays);
        }
        
        public int AddProjectExpense(DataTransferObjects.ProjectExpense entity) {
            return base.Channel.AddProjectExpense(entity);
        }
        
        public void RemoveProjectExpense(DataTransferObjects.ProjectExpense entity) {
            base.Channel.RemoveProjectExpense(entity);
        }
        
        public void UpdateProjectExpense(DataTransferObjects.ProjectExpense entity) {
            base.Channel.UpdateProjectExpense(entity);
        }
        
        public DataTransferObjects.Note[] NoteListByTargetId(int targetId, int noteTargetId) {
            return base.Channel.NoteListByTargetId(targetId, noteTargetId);
        }
        
        public int NoteInsert(DataTransferObjects.Note note) {
            return base.Channel.NoteInsert(note);
        }
        
        public void NoteUpdate(DataTransferObjects.Note note) {
            base.Channel.NoteUpdate(note);
        }
        
        public void NoteDelete(int noteId) {
            base.Channel.NoteDelete(noteId);
        }
        
        public DataTransferObjects.ProjectExpense GetProjectExpenseById(int id) {
            return base.Channel.GetProjectExpenseById(id);
        }
        
        public DataTransferObjects.ProjectExpense[] GetProjectExpensesForMilestone(DataTransferObjects.ProjectExpense entity) {
            return base.Channel.GetProjectExpensesForMilestone(entity);
        }
        
        public void MilestoneUpdateShortDetails(DataTransferObjects.Milestone milestone, string userName) {
            base.Channel.MilestoneUpdateShortDetails(milestone, userName);
        }
        
        public int[] GetMilestoneAndCSATCountsByProject(int projectId) {
            return base.Channel.GetMilestoneAndCSATCountsByProject(projectId);
        }
        
        public DataTransferObjects.Attribution[] IsProjectAttributionConflictsWithMilestoneChanges(int milestoneId, System.DateTime startDate, System.DateTime endDate, bool isUpdate) {
            return base.Channel.IsProjectAttributionConflictsWithMilestoneChanges(milestoneId, startDate, endDate, isUpdate);
        }
        
        public bool[] ShouldAttributionDateExtend(int milestoneId, System.DateTime startDate, System.DateTime endDate) {
            return base.Channel.ShouldAttributionDateExtend(milestoneId, startDate, endDate);
        }
        
        public DataTransferObjects.Milestone[] GetPersonMilestonesOnPreviousHireDate(int personId, System.DateTime previousHireDate) {
            return base.Channel.GetPersonMilestonesOnPreviousHireDate(personId, previousHireDate);
        }
        
        public void SendBadgeRequestMail(DataTransferObjects.Project project, int milestoneId) {
            base.Channel.SendBadgeRequestMail(project, milestoneId);
        }
        
        public void SendBadgeRequestApprovedMail(string personName, string toAddress) {
            base.Channel.SendBadgeRequestApprovedMail(personName, toAddress);
        }
        
        public DataTransferObjects.MSBadge[] GetPeopleAssignedInOtherProjectsForGivenRange(System.DateTime milestoneNewStartDate, System.DateTime milestoneNewEnddate, int milestoneId) {
            return base.Channel.GetPeopleAssignedInOtherProjectsForGivenRange(milestoneNewStartDate, milestoneNewEnddate, milestoneId);
        }
        
        public DataTransferObjects.ExpenseType[] GetAllExpenseTypesList() {
            return base.Channel.GetAllExpenseTypesList();
        }
    }
}

