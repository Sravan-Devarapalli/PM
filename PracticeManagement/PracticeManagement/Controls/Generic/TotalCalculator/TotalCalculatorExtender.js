Type.registerNamespace("PraticeManagement.Controls.Generic.TotalCalculator");

PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorBehavior = function (element) {
    PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorBehavior.initializeBase(this, [element]);
}

PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorBehavior.prototype = {
    initialize: function () {
        PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorBehavior.callBaseMethod(this, 'initialize');
        var controlIds = this.getControlIdList();
        for (i = 0; i < controlIds.length; i++) {
            var control = document.getElementById(controlIds[i]);
            if (control) {
                $addHandlers(
                     control,
                     {
                         'change': this._onChange
                     },
                     this);
            }
        }
        this.update();
    },

    dispose: function () {
        // Cleanup code
        PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorBehavior.callBaseMethod(this, 'dispose');
    },

    _onChange: function () {
        this.update();
    },
    update: function () {

        var target = this.get_element();
        var isVerticalDaytotalTarget = (target.getAttribute("DayTotal") != null && target.getAttribute("DayTotal") != "undefined");

        var sum = 0;
        var controlIds = this.getControlIdList();

        for (i = 0; i < controlIds.length; i++) {
            var control = document.getElementById(controlIds[i]);
            if (control && !isNaN(Number(control.value))) {
                sum += Number(control.value);
            }
        }


        if (isVerticalDaytotalTarget) {
            var hdnHours = document.getElementById(target.getAttribute("DayTotal"));
            hdnHours.value = sum.toFixed(2);
            if (sum > 24) {
                target.style.backgroundColor = "red";
                target.title = "Day Total hours must be lessthan or equals to 24.";

            }
            else {
                target.style.backgroundColor = "white";
                target.title = "";
            }
        }

        target.innerHTML = target.value = sum.toFixed(2);

    },

    getControlIdList: function () {
        var contolsToCheckList = this.controlsToCheck;
        if (contolsToCheckList)
            return contolsToCheckList.split(';');
        else
            return [];
    }
}

PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorBehavior.registerClass('PraticeManagement.Controls.Generic.TotalCalculator.TotalCalculatorBehavior', AjaxControlToolkit.BehaviorBase);
