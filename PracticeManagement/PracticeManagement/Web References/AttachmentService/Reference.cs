﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.296.
// 
#pragma warning disable 1591

namespace PraticeManagement.AttachmentService {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.ComponentModel;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="AttachmentServiceSoap", Namespace="http://tempuri.org/")]
    public partial class AttachmentService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback SaveProjectAttachmentOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetProjectAttachmentDataOperationCompleted;
        
        private System.Threading.SendOrPostCallback DeleteProjectAttachmentByProjectIdOperationCompleted;
        
        private System.Threading.SendOrPostCallback SavePersonPictureOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetPersonPictureOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public AttachmentService() {
            this.Url = global::PraticeManagement.Properties.Settings.Default.PraticeManagement_AttachmentService_AttachmentService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event SaveProjectAttachmentCompletedEventHandler SaveProjectAttachmentCompleted;
        
        /// <remarks/>
        public event GetProjectAttachmentDataCompletedEventHandler GetProjectAttachmentDataCompleted;
        
        /// <remarks/>
        public event DeleteProjectAttachmentByProjectIdCompletedEventHandler DeleteProjectAttachmentByProjectIdCompleted;
        
        /// <remarks/>
        public event SavePersonPictureCompletedEventHandler SavePersonPictureCompleted;
        
        /// <remarks/>
        public event GetPersonPictureCompletedEventHandler GetPersonPictureCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/SaveProjectAttachment", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void SaveProjectAttachment(ProjectAttachment sow, int projectId, string userName) {
            this.Invoke("SaveProjectAttachment", new object[] {
                        sow,
                        projectId,
                        userName});
        }
        
        /// <remarks/>
        public void SaveProjectAttachmentAsync(ProjectAttachment sow, int projectId, string userName) {
            this.SaveProjectAttachmentAsync(sow, projectId, userName, null);
        }
        
        /// <remarks/>
        public void SaveProjectAttachmentAsync(ProjectAttachment sow, int projectId, string userName, object userState) {
            if ((this.SaveProjectAttachmentOperationCompleted == null)) {
                this.SaveProjectAttachmentOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSaveProjectAttachmentOperationCompleted);
            }
            this.InvokeAsync("SaveProjectAttachment", new object[] {
                        sow,
                        projectId,
                        userName}, this.SaveProjectAttachmentOperationCompleted, userState);
        }
        
        private void OnSaveProjectAttachmentOperationCompleted(object arg) {
            if ((this.SaveProjectAttachmentCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SaveProjectAttachmentCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetProjectAttachmentData", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
        public byte[] GetProjectAttachmentData(int projectId, int attachmentId) {
            object[] results = this.Invoke("GetProjectAttachmentData", new object[] {
                        projectId,
                        attachmentId});
            return ((byte[])(results[0]));
        }
        
        /// <remarks/>
        public void GetProjectAttachmentDataAsync(int projectId, int attachmentId) {
            this.GetProjectAttachmentDataAsync(projectId, attachmentId, null);
        }
        
        /// <remarks/>
        public void GetProjectAttachmentDataAsync(int projectId, int attachmentId, object userState) {
            if ((this.GetProjectAttachmentDataOperationCompleted == null)) {
                this.GetProjectAttachmentDataOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetProjectAttachmentDataOperationCompleted);
            }
            this.InvokeAsync("GetProjectAttachmentData", new object[] {
                        projectId,
                        attachmentId}, this.GetProjectAttachmentDataOperationCompleted, userState);
        }
        
        private void OnGetProjectAttachmentDataOperationCompleted(object arg) {
            if ((this.GetProjectAttachmentDataCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetProjectAttachmentDataCompleted(this, new GetProjectAttachmentDataCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/DeleteProjectAttachmentByProjectId", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void DeleteProjectAttachmentByProjectId([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] System.Nullable<int> attachmentId, int projectId, string userName) {
            this.Invoke("DeleteProjectAttachmentByProjectId", new object[] {
                        attachmentId,
                        projectId,
                        userName});
        }
        
        /// <remarks/>
        public void DeleteProjectAttachmentByProjectIdAsync(System.Nullable<int> attachmentId, int projectId, string userName) {
            this.DeleteProjectAttachmentByProjectIdAsync(attachmentId, projectId, userName, null);
        }
        
        /// <remarks/>
        public void DeleteProjectAttachmentByProjectIdAsync(System.Nullable<int> attachmentId, int projectId, string userName, object userState) {
            if ((this.DeleteProjectAttachmentByProjectIdOperationCompleted == null)) {
                this.DeleteProjectAttachmentByProjectIdOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeleteProjectAttachmentByProjectIdOperationCompleted);
            }
            this.InvokeAsync("DeleteProjectAttachmentByProjectId", new object[] {
                        attachmentId,
                        projectId,
                        userName}, this.DeleteProjectAttachmentByProjectIdOperationCompleted, userState);
        }
        
        private void OnDeleteProjectAttachmentByProjectIdOperationCompleted(object arg) {
            if ((this.DeleteProjectAttachmentByProjectIdCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DeleteProjectAttachmentByProjectIdCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/SavePersonPicture", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void SavePersonPicture(int personId, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] pictureData, string userLogin, string pictureFileName) {
            this.Invoke("SavePersonPicture", new object[] {
                        personId,
                        pictureData,
                        userLogin,
                        pictureFileName});
        }
        
        /// <remarks/>
        public void SavePersonPictureAsync(int personId, byte[] pictureData, string userLogin, string pictureFileName) {
            this.SavePersonPictureAsync(personId, pictureData, userLogin, pictureFileName, null);
        }
        
        /// <remarks/>
        public void SavePersonPictureAsync(int personId, byte[] pictureData, string userLogin, string pictureFileName, object userState) {
            if ((this.SavePersonPictureOperationCompleted == null)) {
                this.SavePersonPictureOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSavePersonPictureOperationCompleted);
            }
            this.InvokeAsync("SavePersonPicture", new object[] {
                        personId,
                        pictureData,
                        userLogin,
                        pictureFileName}, this.SavePersonPictureOperationCompleted, userState);
        }
        
        private void OnSavePersonPictureOperationCompleted(object arg) {
            if ((this.SavePersonPictureCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SavePersonPictureCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetPersonPicture", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
        public byte[] GetPersonPicture(int personId) {
            object[] results = this.Invoke("GetPersonPicture", new object[] {
                        personId});
            return ((byte[])(results[0]));
        }
        
        /// <remarks/>
        public void GetPersonPictureAsync(int personId) {
            this.GetPersonPictureAsync(personId, null);
        }
        
        /// <remarks/>
        public void GetPersonPictureAsync(int personId, object userState) {
            if ((this.GetPersonPictureOperationCompleted == null)) {
                this.GetPersonPictureOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetPersonPictureOperationCompleted);
            }
            this.InvokeAsync("GetPersonPicture", new object[] {
                        personId}, this.GetPersonPictureOperationCompleted, userState);
        }
        
        private void OnGetPersonPictureOperationCompleted(object arg) {
            if ((this.GetPersonPictureCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetPersonPictureCompleted(this, new GetPersonPictureCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class ProjectAttachment {
        
        private int attachmentIdField;
        
        private string attachmentFileNameField;
        
        private ProjectAttachmentCategory categoryField;
        
        private byte[] attachmentDataField;
        
        private int attachmentSizeField;
        
        private System.Nullable<System.DateTime> uploadedDateField;
        
        private string uploaderField;
        
        /// <remarks/>
        public int AttachmentId {
            get {
                return this.attachmentIdField;
            }
            set {
                this.attachmentIdField = value;
            }
        }
        
        /// <remarks/>
        public string AttachmentFileName {
            get {
                return this.attachmentFileNameField;
            }
            set {
                this.attachmentFileNameField = value;
            }
        }
        
        /// <remarks/>
        public ProjectAttachmentCategory Category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
        public byte[] AttachmentData {
            get {
                return this.attachmentDataField;
            }
            set {
                this.attachmentDataField = value;
            }
        }
        
        /// <remarks/>
        public int AttachmentSize {
            get {
                return this.attachmentSizeField;
            }
            set {
                this.attachmentSizeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public System.Nullable<System.DateTime> UploadedDate {
            get {
                return this.uploadedDateField;
            }
            set {
                this.uploadedDateField = value;
            }
        }
        
        /// <remarks/>
        public string Uploader {
            get {
                return this.uploaderField;
            }
            set {
                this.uploaderField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.233")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public enum ProjectAttachmentCategory {
        
        /// <remarks/>
        Undefined,
        
        /// <remarks/>
        SOW,
        
        /// <remarks/>
        MSA,
        
        /// <remarks/>
        ChangeRequest,
        
        /// <remarks/>
        Proposal,
        
        /// <remarks/>
        ProjectEstimate,
        
        /// <remarks/>
        PurchaseOrder,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void SaveProjectAttachmentCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void GetProjectAttachmentDataCompletedEventHandler(object sender, GetProjectAttachmentDataCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetProjectAttachmentDataCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetProjectAttachmentDataCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public byte[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void DeleteProjectAttachmentByProjectIdCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void SavePersonPictureCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    public delegate void GetPersonPictureCompletedEventHandler(object sender, GetPersonPictureCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetPersonPictureCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetPersonPictureCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public byte[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591
