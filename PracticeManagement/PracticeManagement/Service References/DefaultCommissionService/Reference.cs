﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PraticeManagement.DefaultCommissionService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="DefaultCommissionService.IDefaultCommissionService")]
    public interface IDefaultCommissionService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDefaultCommissionService/DefaultSalesCommissionByPerson", ReplyAction="http://tempuri.org/IDefaultCommissionService/DefaultSalesCommissionByPersonRespon" +
            "se")]
        DataTransferObjects.DefaultCommission DefaultSalesCommissionByPerson(int personId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDefaultCommissionService/DefaultManagementCommissionByPerson", ReplyAction="http://tempuri.org/IDefaultCommissionService/DefaultManagementCommissionByPersonR" +
            "esponse")]
        DataTransferObjects.DefaultCommission DefaultManagementCommissionByPerson(int personId);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDefaultCommissionServiceChannel : PraticeManagement.DefaultCommissionService.IDefaultCommissionService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DefaultCommissionServiceClient : System.ServiceModel.ClientBase<PraticeManagement.DefaultCommissionService.IDefaultCommissionService>, PraticeManagement.DefaultCommissionService.IDefaultCommissionService {
        
        public DefaultCommissionServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DefaultCommissionServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DefaultCommissionServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DefaultCommissionServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public DataTransferObjects.DefaultCommission DefaultSalesCommissionByPerson(int personId) {
            return base.Channel.DefaultSalesCommissionByPerson(personId);
        }
        
        public DataTransferObjects.DefaultCommission DefaultManagementCommissionByPerson(int personId) {
            return base.Channel.DefaultManagementCommissionByPerson(personId);
        }
    }
}
