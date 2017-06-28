/*
 * Author       : Raj K
 * Date         : 03-06-2017
 * Description  : All common methods are available in this js file
 */
function FixedDecimal(value) {
    return ((value * 100) / 100).toFixed(2);
}

function RoundFixedDecimal(value) {
    return Math.round((value * 100) / 100).toFixed(2);
}

function ConvertToProperCase(value) {
    var aStr = value.split(' ');
    var aProp = [];
    for (str in aStr) {
        aProp.push(aStr[str].charAt(0).toUpperCase() + aStr[str].slice(1));
    }
    return aProp.join(' ');
};

function ToProperCase(control) {
    var aStr = control.value.split(' ');
    var aProp = [];
    for (str in aStr) {
        aProp.push(aStr[str].charAt(0).toUpperCase() + aStr[str].slice(1));
    }
    control.value = aProp.join(' ');
};

function SetDropDownItemByText(dropdownId, itemText) {
    var DropDown = document.getElementById(dropdownId);
    for (var i = 0; i < DropDown.options.length; i++) {
        if (DropDown.options[i].text === itemText) {
            DropDown.selectedIndex = i;
            break;
        }
    }
}