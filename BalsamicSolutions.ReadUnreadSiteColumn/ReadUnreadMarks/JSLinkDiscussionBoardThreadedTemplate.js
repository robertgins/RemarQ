//specialized page for marking up the "Threaded" view of
//a SharePoint 2013 disucssion board, necessary
//this script is mated to the specialized Xsl
//and does not use the same rendering controls
//as the other list views
var remarQListName_LISTID_ = "_LISTGUID_";
var remarQFieldGuid_LISTID_ = "_FIELDGUID_";
var remarQListId_LISTID_ = "_LISTID_";
var remarQAlertDialog_LISTID_ = "_ALERT_";
var remarQLoadingImage_LISTID_ = "_LOADINGIMGURL_";
var remarQMenuImage_LISTID_ = "_MENUIMGURL_";
var remarQRenderTimer_LISTID_ = null;

function remarQIsNullOrUndefined_LISTID_(checkMe) {
    if (null === checkMe) return true;
    if (undefined === checkMe) return true;
    return false;
}

function remarQIsNotNullOrUndefined_LISTID_(checkMe) {
    return !remarQIsNullOrUndefined_LISTID_(checkMe);
}

function remarQReminder_LISTID_() {
    var dialogOpts = SP.UI.$create_DialogOptions();
    dialogOpts.width = 500;
    dialogOpts.height = 250;
    dialogOpts.title = "_REMINDERTITLE_";
    dialogOpts.url = "_LICENSEREMINDERURL_";
    SP.UI.ModalDialog.showModalDialog(dialogOpts);
}

function toggleReadUnreadMark_LISTID_(itemId) {
    //toggle the value 
    var requestURL = "_RUSERVICEURL_?itemId=" + itemId + "&listId=_LISTID_&markUnread=toggle";
    var imageId = "remarQMenuImg" + itemId;
    var imgTag = document.getElementById(imageId);
    if (null !== imgTag) {
        imgTag.setAttribute("src", remarQLoadingImage_LISTID_);
    }
    var httpRequest = new XMLHttpRequest();
    httpRequest.open("GET", requestURL, true);
    httpRequest.onreadystatechange = function () {
        if (4 === httpRequest.readyState) {
            //update the screen on completion
            renderReadMarksAndMenu_LISTID_();
            if (null !== imgTag) {
                imgTag.setAttribute("src", remarQMenuImage_LISTID_);
            }
        }
    };
    httpRequest.send(null);
}

function renderReadMarksAndMenu_LISTID_() {

    if (remarQIsNotNullOrUndefined_LISTID_(remarQRenderTimer_LISTID_)) {
        window.clearInterval(remarQRenderTimer_LISTID_);
        remarQRenderTimer_LISTID_ = null;
    }
    var remarQReadMarks = new Array();
    var itemsToMark = [];
    var tableId = "_LISTGUID_-_VIEWGUID_";
    var tableTag = document.getElementById(tableId);
    if (remarQIsNotNullOrUndefined_LISTID_(tableTag)) {
        var renderDirection = tableTag.getAttribute("dir");
        if (remarQIsNullOrUndefined_LISTID_(renderDirection)) {
            renderDirection = "ltr";
        }
        var requestURL = "_RUQUERY_&userId=_JSUSERID_&itemIds=";
        var childAnchorTags = tableTag.getElementsByTagName("a");
        for (var tagIdx = 0; tagIdx < childAnchorTags.length; tagIdx++) {
            var anchorTag = childAnchorTags[tagIdx];
            var remarQItemId = anchorTag.getAttribute("remarQItemId");
            if (remarQIsNotNullOrUndefined_LISTID_(remarQItemId)) {
                requestURL += remarQItemId;
                requestURL += "%2C";
                itemsToMark.push(anchorTag);
            }
        }
        requestURL += "&cc=" + Math.random();
        var httpRequest = new XMLHttpRequest();
        //we dont want any other processing until we are done
        //so this request is synchronous but if it times
        //out we will simply move on
        httpRequest.open("GET", requestURL, true);
        httpRequest.onreadystatechange = function () {
            if (4 === httpRequest.readyState) {
                if (httpRequest.status === 200) {
                    var respParts = httpRequest.responseText.split(",");
                    for (var respIdx = 0; respIdx < respParts.length; respIdx++) {
                        var resVal = respParts[respIdx].split("=");
                        if (resVal.length === 2) {
                            var isRead = (resVal[1] === "true");
                            remarQReadMarks[resVal[0]] = isRead;
                        }
                    }
                    updateDisplayReadMarksAndMenu_LISTID_(remarQReadMarks, itemsToMark, renderDirection);
                    var refreshInterval = parseInt("_REFRESHINTERVAL_") || 0;
                    if (refreshInterval > 0) {
                        if (remarQIsNullOrUndefined_LISTID_(remarQRenderTimer_LISTID_)) {
                            remarQRenderTimer_LISTID_ = window.setInterval(function () { renderReadMarksAndMenu_LISTID_(); }, refreshInterval * 1000);
                        }
                    }
                }
            }
        };
        httpRequest.send(null);

    }

}

function updateDisplayReadMarksAndMenu_LISTID_(remarQReadMarks, itemsToMark, renderDirection) {
    for (var tagIdx = 0; tagIdx < itemsToMark.length; tagIdx++) {
        var anchorTag = itemsToMark[tagIdx];
        var tblRow = remarQFindTableRowFromDiv_LISTID_(anchorTag);
        if (remarQIsNotNullOrUndefined_LISTID_(tblRow)) {
            var tblCell = tblRow.firstChild;
            if (renderDirection === "rtl") {
                tblCell = tblRow.lastChild;
            }
            var remarQItemId = anchorTag.getAttribute("remarQItemId");
            var isRead = remarQReadMarks[remarQItemId];
            if (isRead) {
                remarQMarkColorOnAllChildElemenets_LISTID_(tblCell, "", "normal");
            }
            else {
                remarQMarkColorOnAllChildElemenets_LISTID_(tblCell, "_COLRENDERCOLOR_", "bolder");
            }
        }
    }
}

function remarQFindTableRowFromDiv_LISTID_(rumDiv) {
    //walk "up" until we hit the row
    var rumParent = rumDiv.parentElement;
    while (remarQIsNotNullOrUndefined_LISTID_(rumParent) && remarQIsNotNullOrUndefined_LISTID_(rumParent.tagName) && rumParent.tagName !== 'TR' && rumParent.tagName !== 'tr') {
        rumParent = rumParent.parentElement;
    }
    return rumParent;
}

function remarQMarkColorOnAllChildElemenets_LISTID_(htmlElem, fontColor, fontStyle) {
    if (remarQIsNotNullOrUndefined_LISTID_(htmlElem)) {
        if (remarQIsNotNullOrUndefined_LISTID_(htmlElem.attributes)) {
            var elemClass = htmlElem.getAttribute("class");
            if (null !== elemClass && elemClass.toLowerCase().indexOf("ms-list-itemlink") > -1) {
                //special processing for the item link popup menu
                htmlElem.style.color = "";
                htmlElem.style.fontWeight = "normal";
            }
            else {
                htmlElem.style.color = fontColor;
                htmlElem.style.fontWeight = fontStyle;
                var childCount = htmlElem.childNodes.length;
                for (var childIdx = 0; childIdx < childCount; childIdx++) {
                    var childElem = htmlElem.childNodes[childIdx];
                    if (remarQIsNotNullOrUndefined_LISTID_(childElem.attributes)) {
                        remarQMarkColorOnAllChildElemenets_LISTID_(childElem, fontColor, fontStyle);
                    }
                }
            }
        }
    }
}

function remarQToggleReadUnreadMark(itemId, listId, userId) {
    var callName = "toggleReadUnreadMark" + listId.replace(/{/g, "").replace(/}/g, "").replace(/-/g, "").toLowerCase();
    eval(callName)(itemId);
}


if (remarQAlertDialog_LISTID_ === "true") {
    SP.SOD.executeFunc('sp.js', 'SP.ClientContext', remarQReminder_LISTID_);
    remarQAlertDialog_LISTID_ = "false";
}

renderReadMarksAndMenu_LISTID_();
