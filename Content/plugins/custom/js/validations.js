/*
 * Author       : Raj K
 * Date         : 03-06-2017
 * Description  : All common validations are available in this js file
 */

$(function () {
    $(".numeric").keypress(function (e) {
        //if the letter is not digit then display error and don't type anything
        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
            return false;
        }
    });
});



function SetFocus(validatecontrol) {
    //    window.setTimeout(function () { // Use Timeout for IE8
    //        validatecontrol.focus();
    //    }, 0);
    // The above lines are commented as this is not working when using custom alert
    validatecontrol.focus();
}

// Title       : Alpha Numeric Expression Description
// About       : regexDecimal = ^[\w ]*[^\W_][\w ]*$;
// Description : This regexCurrency validates Value with decimals upto 4 or with out decimal, zero will not be allowed first for integer values
//               Valid Ex: 100 , 100.0000 , 0.250 --- Not Valid Ex: 100.00005 , 01 , 01.00 , .00
// Author      : Raj K
var regexAlphaNumeric = /^([\w]+\s)*[\w]+$/;
function ValidateAlphaNumericControl(control, controlDesc) {
    var iErrors = 0;
    var controlValue = control.value;
    if (controlValue != '') {
        if (!regexAlphaNumeric.test(controlValue)) {
            iErrors++;
            SetFocus(control);
            alert('Please Enter Only Alpha-Numerics in ' + controlDesc);
        }
    }
    // Finally return Result
    if (iErrors == 0) {
        return true;
    }
    else {
        control.value = '';
        return false;
    }
}

function ValidateAlphaNumericValue(value) {
    if (regexAlphaNumeric.test(value)) {
        return true;
    }
    else {
        return false;
    }
}

// Title       : Numeric Expression Description
// About       : regexDecimal = ^[\w ]*[^\W_][\w ]*$;
// Description : This regexCurrency validates Value with Numbers only, No Spaces//               
// Author      : Raj K
var regexNumeric = /^[0-9]+$/;
function ValidateNumericControl(control, controlDesc) {
    var iErrors = 0;
    var controlValue = control.value;
    if (controlValue != '') {
        if (!regexNumeric.test(controlValue)) {
            iErrors++;
            SetFocus(control);
            alert('Please Enter Only Numbers in ' + controlDesc);
        }
    }
    // Finally return Result
    if (iErrors == 0) {
        return true;
    }
    else {
        control.value = '';
        return false;
    }
}

// Title       : NumericDecimal Regular Expression Description
// About       : regexDecimal = /^(?!0\d|$)\d*(\.\d{1,4})?$/;
// Description : Allows Negative Values .This regexCurrency validates Value with decimals upto 4 or with out decimal, zero will not be allowed first for integer values
//               Valid Ex: 100 , 100.0000 , 0.250 --- Not Valid Ex: 100.00005 , 01 , 01.00 , .00
// Author      : Indrajith Reddy
var regexNumericDecimal = /^[-]?(?!0\d|$)\d*(\.\d{1,4})?$/;

// Title       : Numeric Decimal Validator Description
// About       : ValidateDecimalControl
// Description : Validates value to be a Valid Expression, Accepts Zero
// Author      : Raj K
function ValidateNumericDecimalControl(control, controlDesc) {
    var iErrors = 0;
    var controlValue = control.value;
    if (controlValue != '') {
        if (!regexNumericDecimal.test(controlValue)) {
            iErrors++;
            SetFocus(control);
            alert('Please Enter Valid ' + controlDesc);
        }
    }
    // Finally return Result
    if (iErrors == 0) {
        return true;
    }
    else {
        control.value = '';
        return false;
    }
}

// Title       : Date Expression Description for dd-Mon-yyyy format
// About       : 
// Description : This regexDate validates date format with seperaters (/ . -)
//               works for Leap Year, months wihh 28,29,30,31 dates also.                       
// Author      : Raj K
// var regexDate = /^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$/;
var regexDate = /^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{2})$/;
function ValidateDateControl(control) {
    var iErrors = 0;
    var controlValue = control.value;
    if (controlValue != '' && controlValue != "dd-MM-yyyy") {
        if (!regexDate.test(controlValue)) {
            iErrors++;
            control.value = '';
            SetFocus(control);
            alert('Please Enter Valid Date (dd-MM-yyyy)');
        }
    }
    // Finally return Result
    if (iErrors == 0) {
        return true;
    }
    else {
        return false;
    }
}

function ValidateFromDateToDate(FromDate, ToDate) {
    if (FromDate != '' && ToDate != '') {
        var FromDateConv = customDateParse2(FromDate);
        var ToDateConv = customDateParse2(ToDate);
        if (FromDateConv >= ToDateConv) {
            return false;
        }
        else {
            return true;
        }
    }
}

function DaysBetween(FromDate, ToDate) {
    var oneDay = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds
    var firstDate = new Date(FromDate);
    var secondDate = new Date(ToDate);
    var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime()) / (oneDay)));
    return diffDays;
}

function ValidateCheckBoxList(control, ControlDesc) {
    var checked = $("#" + control.id + " input:checked").length > 0;
    if (!checked) {
        alert("Please check at least one checkbox in " + ControlDesc);
        return false;
    }
    else {
        return true;
    }
}

// Title       : ValidateEmail Description
// About       : regexDecimal = ^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$
// Description : This regexEmail validates Email Format//               
// Author      : Raj K
var regexEmail = /^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$/;
function ValidateEmailControl(control) {
    var controlValue = control.value;
    if (!regexEmail.test(controlValue)) {
        SetFocus(control);
        alert('Please Enter Valid Email');
        control.value = '';
        return false;
    }
    else {
        return true;
    }
}

// Title       : Decimal Validator Description
// About       : ValidateDecimalControl
// Description : Validates value to be a Valid Expression, Accepts Zero
// Author      : Raj K
function ValidateDecimalControl(control, controlDesc) {
    var iErrors = 0;
    var controlValue = control.value;
    if (controlValue != '') {
        if (!regexDecimal.test(controlValue)) {
            iErrors++;
            SetFocus(control);
            alert('Please Enter Valid ' + controlDesc);
        }
    }
    // Finally return Result
    if (iErrors == 0) {
        return true;
    }
    else {
        control.value = '';
        return false;
    }
}

function ValidatePercentageControl(control, controlDesc) {
    var iErrors = 0;
    var controlValue = control.value;
    if (controlValue != '') {
        if (regexDecimal.test(controlValue)) {
            if (controlValue < 0 || controlValue > 100) {
                iErrors++;
                SetFocus(control);
                alert('Please Enter ' + controlDesc + ' between 0 to 100');
            }
        }
        else {
            iErrors++;
            SetFocus(control);
            alert('Please Enter Valid ' + controlDesc);
        }
    }
    // Finally return Result
    if (iErrors == 0) {
        return true;
    }
    else {
        control.value = '';
        return false;
    }
}

// Title       : Alphabets Expression Description
// About       : regexAlphabets = /^[a-zA-Z]+$/;
// Description : This regexCurrency validates Value with only Alphabets
// Author      : Raj K
var regexAlphabets = /^[a-zA-Z ]+$/;
function ValidateAlphabetControl(control, controlDesc) {
    var iErrors = 0;
    var controlValue = control.value;
    if (controlValue != '') {
        if (!regexAlphabets.test(controlValue)) {
            iErrors++;
            SetFocus(control);
            alert('Please Enter Only Albhabets in ' + controlDesc);
        }
        else {
            ToProperCase(control);
        }
    }
    // Finally return Result
    if (iErrors == 0) {
        return true;
    }
    else {
        control.value = '';
        return false;
    }
}