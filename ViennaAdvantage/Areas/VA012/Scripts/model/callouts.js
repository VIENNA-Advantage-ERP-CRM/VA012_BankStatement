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

    //Callout to SetConversionType on statement Line
    VA012_Contra.prototype.SetConversionType = function (ctx, windowNo, mTab, mField, value, oldValue) {
        //handled exception when value is null
        if (this.isCalloutActive() || value == null || value.toString() == "") {
            //clear Values
            mTab.setValue("StmtAmt", 0);
            mTab.setValue("TrxAmt", 0);
            mTab.setValue("ChargeAmt", 0);//clear chargeAmt 
            mTab.setValue("C_ConversionType_ID", 0);
            //avoid looping Commented C_CashLine_ID set as Zero
            //mTab.setValue("C_CashLine_ID", 0);
            //setCalloutActive as false
            //this.setCalloutActive(false);
            return "";
        }

        this.setCalloutActive(true);

        if (VIS.Utility.Util.getValueOfInt(mTab.getValue("C_CashLine_ID")) > 0) {

            var DateAcct = mTab.getValue("DateAcct");
            if (DateAcct == null) {
                mTab.setValue("C_CashLine_ID", 0);
                this.setCalloutActive(false);
                return "PlzSelectStmtDate";
            }

            //defining variable to pass as a parameter which is holding some individual Values
            var paramString = mTab.getValue("C_CashLine_ID").toString() + "," + mTab.getValue("C_Currency_ID").toString() + "," + DateAcct.toString() + "," + mTab.getValue("AD_Org_ID").toString();
            //changed the variable name dateAcct as data 
            //Updating the DateAcct, TrxAmt and Stmt Amount according to conditions
            var data = VIS.dataContext.getJSONRecord("VA012/Statement/GetCashDetails", paramString);
            if (data != null) {
                if (data.Amount != 0) {
                    /* Now, statement line date and Account date are same, so no to overwrite with Cash Journal Account date
                    mTab.setValue("DateAcct", data.DateAcct);*/
                    //rearrage the fields
                    mTab.setValue("StmtAmt", data.Amount);
                    mTab.setValue("TrxAmt", data.Amount);
                    mTab.setValue("C_ConversionType_ID", data.C_ConversionType_ID);
                }
                else {
                    //reset stmtAmt
                    mTab.setValue("StmtAmt", 0);
                    mTab.setValue("TrxAmt", 0);
                    mTab.setValue("C_ConversionType_ID", 0);
                    mTab.setValue("C_CashLine_ID", 0);
                    VIS.ADialog.info("VA012_ConversionRateNotFound");
                }
            }
        }
        else
        {
            //clear the ConversionType value when Cash Journal is zero or null
            mTab.setValue("C_ConversionType_ID", 0);
        }
        //Update the Charge Amount
        var stmt = VIS.Utility.Util.getValueOfDecimal(mTab.getValue("StmtAmt"));
        var trx = VIS.Utility.Util.getValueOfDecimal(mTab.getValue("TrxAmt"));

        var bd = stmt - trx;
        // Calculate Charge
        var interest = VIS.Utility.Util.getValueOfDecimal(mTab.getValue("InterestAmt"));

        bd = bd - interest;
        mTab.setValue("ChargeAmt", bd);

        this.setCalloutActive(false);
        ctx = windowNo = mTab = mField = value = oldValue = null;
        return "";
    };

    VA012.Model = VA012.Model || {};
    VA012.Model.VA012_Contra = VA012_Contra;
})(VA012, jQuery);
