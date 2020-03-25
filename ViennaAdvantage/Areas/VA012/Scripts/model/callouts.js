; VA012 = window.VA012 || {};
; (function (VA012, $) {
    var Level = VIS.Logging.Level;
    var Util = VIS.Utility.Util;
    function VA012_Contra() {
        VIS.CalloutEngine.call(this, "VA012.VA012_Contra");
    };
    VIS.Utility.inheritPrototype(VA012_Contra, VIS.CalloutEngine);
    VA012_Contra.prototype.SetContra = function (ctx, windowNo, mTab, mField, value, oldValue) {
        if (this.isCalloutActive()) {
            return "";
        }
        this.setCalloutActive(true);
        if (value == null) {
            mTab.setValue("VA012_IsContra", false);
        }
        else {
            var _contra = Util.getValueOfString(VIS.DB.executeScalar("SELECT DTD001_ChargeType FROM C_Charge WHERE  C_Charge_ID=" + value.toString()));
            if (_contra == "CON") {
                mTab.setValue("VA012_IsContra", true);
            }
            else {
                mTab.setValue("VA012_IsContra", false);
            }
        }
        this.setCalloutActive(false);
        return "";

    };
    VA012.Model = VA012.Model || {};
    VA012.Model.VA012_Contra = VA012_Contra;
})(VA012, jQuery);