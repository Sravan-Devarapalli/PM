﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PraticeManagement.TimescaleService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="TimescaleService.ITimescaleService")]
    public interface ITimescaleService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITimescaleService/GetById", ReplyAction="http://tempuri.org/ITimescaleService/GetByIdResponse")]
        DataTransferObjects.Timescale GetById(DataTransferObjects.TimescaleType timescaleId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITimescaleService/GetAll", ReplyAction="http://tempuri.org/ITimescaleService/GetAllResponse")]
        DataTransferObjects.Timescale[] GetAll();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ITimescaleServiceChannel : PraticeManagement.TimescaleService.ITimescaleService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class TimescaleServiceClient : System.ServiceModel.ClientBase<PraticeManagement.TimescaleService.ITimescaleService>, PraticeManagement.TimescaleService.ITimescaleService {
       
        public TimescaleServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public TimescaleServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TimescaleServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public TimescaleServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public DataTransferObjects.Timescale GetById(DataTransferObjects.TimescaleType timescaleId) {
            return base.Channel.GetById(timescaleId);
        }
        
        public DataTransferObjects.Timescale[] GetAll() {
            return base.Channel.GetAll();
        }
    }
}

