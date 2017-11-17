Type.registerNamespace("PraticeManagement.Controls.Generic.BillableNonBillableAndTotal");

PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender = function (element) {

    PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender.initializeBase(this, [element]);
}

PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender.prototype = {
    _initBase: function () {
        PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender.callBaseMethod(this, 'initialize');
    },
    initialize: function () {
        this._initBase();
        var controlIds = this.getControlIdList();
        for (i = 0; i < controlIds.length; i++) {
            var control = document.getElementById(controlIds[i]);
            if (control) {
                $addHandlers(
                     control,
                     {
                         'click': this._onClick
                     },
                     this);
            }
        }
        this.update();
    },
    _onClick: function () {
        this.update();
    },
    update: function () {

        var controlIds = this.getControlIdList();
        var targetControlIds = this.getTargetControlIdList();

        for (var i = 0; i < controlIds.length; i++) {
            var control = document.getElementById(controlIds[i]);
            if (control && control.checked) {
                var rbtn = control;
                var attribute = rbtn.getAttribute('DisplayValueType');
                for (var j = 0; j < targetControlIds.length; j++) {
                    var targetControl = document.getElementById(targetControlIds[j]);
                    if (targetControl) {
                        var value = targetControl.getAttribute(attribute);
                        targetControl.innerText = value;
                        targetControl.innerHTML = value;
                    }
                }
            }
        }

    },
    dispose: function () {
        PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender.callBaseMethod(this, 'dispose');
    },
    getControlIdList: function () {
        var contolsToCheckList = this.controlsToCheck;
        if (contolsToCheckList)
            return contolsToCheckList.split(';');
        else
            return [];
    }
    ,
    getTargetControlIdList: function () {
        var targetContolsToCheckList = this.targetContolsToCheck;
        if (targetContolsToCheckList)
            return targetContolsToCheckList.split(';');
        else
            return [];
    }

}
PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender.registerClass('PraticeManagement.Controls.Generic.BillableNonBillableAndTotal.BillableNonBillableAndTotalExtender', AjaxControlToolkit.BehaviorBase);
