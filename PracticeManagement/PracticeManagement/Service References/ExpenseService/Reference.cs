﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PraticeManagement.ExpenseService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ExpenseService.IExpenseService")]
    public interface IExpenseService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExpenseService/GetExpenseBasisList", ReplyAction="http://tempuri.org/IExpenseService/GetExpenseBasisListResponse")]
        DataTransferObjects.ExpenseBasis[] GetExpenseBasisList();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExpenseService/GetWeekPaidOptionList", ReplyAction="http://tempuri.org/IExpenseService/GetWeekPaidOptionListResponse")]
        DataTransferObjects.WeekPaidOption[] GetWeekPaidOptionList();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExpenseService/GetExpenseDetail", ReplyAction="http://tempuri.org/IExpenseService/GetExpenseDetailResponse")]
        DataTransferObjects.MonthlyExpense GetExpenseDetail(string itemName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExpenseService/SaveExpenseItemDetail", ReplyAction="http://tempuri.org/IExpenseService/SaveExpenseItemDetailResponse")]
        void SaveExpenseItemDetail(DataTransferObjects.MonthlyExpense itemExpense);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExpenseService/DeleteMonthlyExpense", ReplyAction="http://tempuri.org/IExpenseService/DeleteMonthlyExpenseResponse")]
        void DeleteMonthlyExpense(DataTransferObjects.MonthlyExpense itemExpense);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExpenseService/MonthlyExpenseListAll", ReplyAction="http://tempuri.org/IExpenseService/MonthlyExpenseListAllResponse")]
        DataTransferObjects.MonthlyExpense[] MonthlyExpenseListAll(System.DateTime startDate, System.DateTime endDate);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IExpenseServiceChannel : PraticeManagement.ExpenseService.IExpenseService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ExpenseServiceClient : System.ServiceModel.ClientBase<PraticeManagement.ExpenseService.IExpenseService>, PraticeManagement.ExpenseService.IExpenseService {
  
        public ExpenseServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ExpenseServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ExpenseServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ExpenseServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public DataTransferObjects.ExpenseBasis[] GetExpenseBasisList() {
            return base.Channel.GetExpenseBasisList();
        }
        
        public DataTransferObjects.WeekPaidOption[] GetWeekPaidOptionList() {
            return base.Channel.GetWeekPaidOptionList();
        }
        
        public DataTransferObjects.MonthlyExpense GetExpenseDetail(string itemName) {
            return base.Channel.GetExpenseDetail(itemName);
        }
        
        public void SaveExpenseItemDetail(DataTransferObjects.MonthlyExpense itemExpense) {
            base.Channel.SaveExpenseItemDetail(itemExpense);
        }
        
        public void DeleteMonthlyExpense(DataTransferObjects.MonthlyExpense itemExpense) {
            base.Channel.DeleteMonthlyExpense(itemExpense);
        }
        
        public DataTransferObjects.MonthlyExpense[] MonthlyExpenseListAll(System.DateTime startDate, System.DateTime endDate) {
            return base.Channel.MonthlyExpenseListAll(startDate, endDate);
        }
    }
}

