var remarQIsRegistered_LISTID_ = false;
var remarQListName_LISTID_ = "_LISTGUID_";
var remarQListId_LISTID_ = "_LISTID_";
var remarQAlertDialog_LISTID_ = "_ALERT_";
var remarQErrorImage_LISTID_ = "_ERRORIMAGEURL_";
var remarQFieldGuid_LISTID_ = "_FIELDGUID_";
var remarQRenderMode_LISTID_ = "_COLRENDERMODE_";
var remarQServiceUrl_LISTID_ = "_RUSERVICEURL_";
var remarQVarPrefix_LISTID_ = "rum__LISTID__";
var remarQModuleActive_LISTID_ = "_HTTPMODULE_";
var remarQRenderContext_LISTID_ = null;
var remarQRenderTimer_LISTID_ = null;

function remarQIsNullOrUndefined_LISTID_(checkMe) {
    if (null === checkMe) return true;
    if (undefined === checkMe) return true;
    return false;
}

function remarQIsNotNullOrUndefined_LISTID_(checkMe) {
    return !remarQIsNullOrUndefined_LISTID_(checkMe);
}

function remarQDispEx_LISTID_(ele, objEvent, fTransformServiceOn, fShouldTransformExtension, fTransformHandleUrl, strHtmlTrProgId, iDefaultItemOpen, strProgId, strHtmlType, strServerFileRedirect, strCheckoutUser, strCurrentUser, strRequireCheckout, strCheckedoutTolocal, strPermmask) {
    
    if (remarQIsNullOrUndefined_LISTID_(ele) || remarQIsNullOrUndefined_LISTID_(objEvent)) {
        //this is an error condition so just fall through to SharePoint
        return DispEx(ele, objEvent, fTransformServiceOn, fShouldTransformExtension, fTransformHandleUrl, strHtmlTrProgId, iDefaultItemOpen, strProgId, strHtmlType, strServerFileRedirect, strCheckoutUser, strCurrentUser, strRequireCheckout, strCheckedoutTolocal, strPermmask);
    }
    else {
        var itemId = ele.getAttribute("remarQId");
        if (remarQIsNullOrUndefined_LISTID_(itemId)) {
            //this is not an item we care about so just fall through to SharePoint
            return DispEx(ele, objEvent, fTransformServiceOn, fShouldTransformExtension, fTransformHandleUrl, strHtmlTrProgId, iDefaultItemOpen, strProgId, strHtmlType, strServerFileRedirect, strCheckoutUser, strCurrentUser, strRequireCheckout, strCheckedoutTolocal, strPermmask);
        }
        else {
            //if the Http module is running then we dont need to process the local intercept but we dont
            //know for sure when the page will be ready with an update so we let the remarQRenderTimer handle
            //that ui update
            if (remarQModuleActive_LISTID_ === "true") {
                var retunValue = DispEx(ele, objEvent, fTransformServiceOn, fShouldTransformExtension, fTransformHandleUrl, strHtmlTrProgId, iDefaultItemOpen, strProgId, strHtmlType, strServerFileRedirect, strCheckoutUser, strCurrentUser, strRequireCheckout, strCheckedoutTolocal, strPermmask);
                //if its an open in window we will repaint anyway on when the page refreshs
                //but if its an office open then DispEx has set cancelBubble and we need
                //to try to repaint the screen in about 10 seconds (after the document has opened)
                if (remarQIsNotNullOrUndefined_LISTID_(objEvent.cancelBubble)) {
                    if (objEvent.cancelBubble) {
                        window.setTimeout(function () { renderReadMarksAndMenu_LISTID_(remarQRenderContext_LISTID_, false); }, 10000);
                    }
                }
                return retunValue;
            }
            else {
                //setup a callback to SharePoint's DispEx to be triggered after our marking
                //happens, unfortunately that marking event is synchronous to avoid falling
                //throught to the Anchor click that usually pins the document
                var callbackFunc = function () { DispEx(ele, objEvent, fTransformServiceOn, fShouldTransformExtension, fTransformHandleUrl, strHtmlTrProgId, iDefaultItemOpen, strProgId, strHtmlType, strServerFileRedirect, strCheckoutUser, strCurrentUser, strRequireCheckout, strCheckedoutTolocal, strPermmask); };
                return remarQCallRemoteMarkingService_LISTID_(itemId, false, callbackFunc, true);
            }
        }
    }
}


function remarQCallRemoteMarkingService_LISTID_(itemId, markUnread, callbackFunc, callIsSynchronous) {

    //make REST call to our marking service
    var requestURL = "_RUSERVICEURL_?returnMarks=true&itemId=" + itemId + "&listId=_LISTID_&markUnread=" + markUnread;
    requestURL += "&userId=" + remarQRenderContext_LISTID_.CurrentUserId + "&itemIds=";
    for (var rowIdx = 0; rowIdx < remarQRenderContext_LISTID_.ListData.Row.length; rowIdx++) {
        requestURL += remarQRenderContext_LISTID_.ListData.Row[rowIdx].ID;
        requestURL += "%2C";
    }
    //if we dont do this some of the browsers will cache
    requestURL += "&cc=" + Math.random();
    var httpRequest = new XMLHttpRequest();
    httpRequest.onreadystatechange = function () {
        if (4 === httpRequest.readyState) {
            if (200 === httpRequest.status) {
                //we asked for read marks in one query so we dont have to call again
                //this way we can mark up the page  now 
                var respParts = httpRequest.responseText.split(",");
                var remarQReadMarks = new Array();
                for (var respIdx = 0; respIdx < respParts.length; respIdx++) {
                    var resVal = respParts[respIdx].split("=");
                    if (resVal.length === 2) {
                        var isRead = (resVal[1] === "true");
                        remarQReadMarks[resVal[0]] = isRead;
                    }
                }
                renderReadMarksAndMenuEx_LISTID_(remarQRenderContext_LISTID_, false, remarQReadMarks);
            }
            if (remarQIsNotNullOrUndefined_LISTID_(callbackFunc)) {
                callbackFunc();
            }
        }
    };
    if (callIsSynchronous) {
         httpRequest.open("GET", requestURL, false);
        try {
            httpRequest.timeout = 2000;
        }
        catch (e) {
            //some browsers dont support synchronous timeouts
            if (e.name !== "InvalidAccessError") {
                throw e;
            }
            else {
                httpRequest = new XMLHttpRequest();
                httpRequest.open("GET", requestURL, false);
            }
        }
    }
    else {
        httpRequest.open("GET", requestURL, true);
    }
    httpRequest.send(null);
}

function renderReadMarksAndMenuEx_LISTID_(renderCtx, updateMenu, remarQReadMarks) {
    //set the page flags and if necessary update the DispEx intercept
    if (remarQIsNotNullOrUndefined_LISTID_(renderCtx)) {
        for (var rowIdx = 0; rowIdx < renderCtx.ListData.Row.length; rowIdx++) {
            var isRead = remarQReadMarks[renderCtx.ListData.Row[rowIdx].ID];
            var divName = remarQVarPrefix_LISTID_ + renderCtx.ListData.Row[rowIdx].ID;
            var rumDiv = document.getElementById(divName);
            if (null !== rumDiv) {
                setRemarQDisplay_LISTID_(rumDiv, isRead);
                if (updateMenu) {
                    var tblRow = remarQFindTableRowFromDiv_LISTID_(rumDiv);
                    //mark up DispEx onclick events to use our interceptor
                    var allTags = tblRow.getElementsByTagName("a");
                    for (var tagIdx = 0; tagIdx < allTags.length; tagIdx++) {
                        var ancTag = allTags[tagIdx];
                        if (ancTag.hasAttribute("onclick")) {
                            var clickFunc = ancTag.getAttribute("onclick");
                            if (clickFunc.indexOf("DispEx(") > -1) {
                                clickFunc = clickFunc.replace("DispEx(", "remarQDispEx_LISTID_(");
                                ancTag.setAttribute("onclick", clickFunc);
                                ancTag.setAttribute("remarQId", renderCtx.ListData.Row[rowIdx].ID);
                            }
                        }
                    }
                }
            }
        }
    }
}

function renderReadMarksAndMenu_LISTID_(renderCtx, updateMenu) {
    //set the page flags and if necessary update the DispEx intercept
    if (remarQIsNotNullOrUndefined_LISTID_(renderCtx)) {
        //we are going to get all of our read unread information
        //from the query service, if its not there or errors out
        //we will use the inline information, otherwise 
        //we will use the udpated information from the query

        if (remarQIsNotNullOrUndefined_LISTID_(remarQRenderTimer_LISTID_)) {
            window.clearInterval(remarQRenderTimer_LISTID_);
            remarQRenderTimer_LISTID_ = null;
        }

        var requestURL = "_RUQUERY_&userId=" + renderCtx.CurrentUserId + "&itemIds=";
        for (var rowIdx = 0; rowIdx < renderCtx.ListData.Row.length; rowIdx++) {
            requestURL += renderCtx.ListData.Row[rowIdx].ID;
            requestURL += "%2C";
        }
        //if we dont do this some of the browsers will cache
        requestURL += "&cc=" + Math.random();
        var httpRequest = new XMLHttpRequest();
        httpRequest.open("GET", requestURL, true);
        httpRequest.onreadystatechange = function () {
            if (4 === httpRequest.readyState) {
                if (200 === httpRequest.status) {
                    var remarQReadMarks = new Array();
                    var respParts = httpRequest.responseText.split(",");
                    for (var respIdx = 0; respIdx < respParts.length; respIdx++) {
                        var resVal = respParts[respIdx].split("=");
                        if (resVal.length === 2) {
                            var isRead = (resVal[1] === "true");
                            remarQReadMarks[resVal[0]] = isRead;
                        }
                    }
                    renderReadMarksAndMenuEx_LISTID_(renderCtx, updateMenu, remarQReadMarks);
                    var refreshInterval = parseInt("_REFRESHINTERVAL_") || 0;
                    if (refreshInterval > 0) {
                        if (remarQIsNullOrUndefined_LISTID_(remarQRenderTimer_LISTID_)) {
                            remarQRenderTimer_LISTID_ = window.setInterval(function () { renderReadMarksAndMenu_LISTID_(renderCtx, false); }, refreshInterval * 1000);
                        }
                    }
                }
            }
        };
        httpRequest.open("GET", requestURL, true);
        httpRequest.send(null);

    }
}

function setRemarQDisplay_LISTID_(rumDiv, isRead) {

    if (remarQRenderMode_LISTID_ === "2") {
        var tblRow = remarQFindTableRowFromDiv_LISTID_(rumDiv);
        if (isRead) {
            remarQMarkColorOnAllChildElemenets_LISTID_(tblRow, "", "normal");
        }
        else {
            remarQMarkColorOnAllChildElemenets_LISTID_(tblRow, "_COLRENDERCOLOR_", "bolder");
        }
    }
    else {
        if (isRead) {
            remarQSetItemImage_LISTID_(rumDiv, "_READIMAGEURL_");
        }
        else {
            remarQSetItemImage_LISTID_(rumDiv, "_UNREADIMAGEURL_");
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

function remarQSetItemImage_LISTID_(rumDiv, imgUrl) {
    var imgId = 'img_' + rumDiv.id.substring(4);
    var imgTag = document.getElementById(imgId);
    if (remarQIsNotNullOrUndefined_LISTID_(imgTag)) {
        imgTag.setAttribute("src", imgUrl);
    }
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

function remarQContextMenu_LISTID_(itemId, markUnread) {
    remarQCallRemoteMarkingService_LISTID_(itemId, markUnread, null,false);
}

function remarQReminder_LISTID_() {
    var dialogOpts = SP.UI.$create_DialogOptions();
    dialogOpts.width = 500;
    dialogOpts.height = 250;
    dialogOpts.title = "_REMINDERTITLE_";
    dialogOpts.url = "_LICENSEREMINDERURL_";
    SP.UI.ModalDialog.showModalDialog(dialogOpts);
}

function remarQReport_LISTID_(itemId) {

    var dialogOpts = SP.UI.$create_DialogOptions();
    dialogOpts.width = 500;
    dialogOpts.height = 450;
    dialogOpts.title = "_REPORTTITLE_";
    for (var rowIdx = 0; rowIdx < remarQRenderContext_LISTID_.ListData.Row.length; rowIdx++) {
        if (itemId === remarQRenderContext_LISTID_.ListData.Row[rowIdx].ID) {
            dialogOpts.title = remarQRenderContext_LISTID_.ListData.Row[rowIdx].FileLeafRef;
            break;
        }
    }
    dialogOpts.url = "_REPORTURL_?listId=_LISTID_&itemId=" + itemId;
    SP.UI.ModalDialog.showModalDialog(dialogOpts);
}

function remarQ_LISTID_PreRender(renderCtx) {

    //We neeed all these null checks because some
    //of the rendering modes dont actually provide 
    //field schema (e.g. GettingStartedWebpart)
    if (remarQIsNotNullOrUndefined_LISTID_(renderCtx) && remarQIsNotNullOrUndefined_LISTID_(renderCtx.ListSchema)
        && remarQIsNotNullOrUndefined_LISTID_(renderCtx.ListSchema.Field) && remarQIsNotNullOrUndefined_LISTID_(renderCtx.ListSchema.Field.length)) {
        if (renderCtx.listName === remarQListName_LISTID_) {
            //Remove up the header text (we dont want one)
            var fieldCount = renderCtx.ListSchema.Field.length;
            for (var fieldIdx = 0; fieldIdx < fieldCount; fieldIdx++) {
                if (renderCtx.ListSchema.Field[fieldIdx].ID === remarQFieldGuid_LISTID_) {
                    renderCtx.ListSchema.Field[fieldIdx].DisplayName = null;
                    renderCtx.ListSchema.Field[fieldIdx].AllowGridEditing = "FALSE";
                    break;
                }
            }
        }
    }
}

function remarQ_LISTID_PostRender(renderCtx) {

    if (renderCtx.listName === remarQListName_LISTID_) {
        //some of the render modes (GRID) do not respect the .DisplayName = null;
        //that we set in remarQ_LISTID_PreRender, so we will clean up the 
        //offending Html and clear the column header clicks and displays
        var tableId = renderCtx.listName.toUpperCase() + "-" + renderCtx.view.toUpperCase();
        var listTable = document.getElementById(tableId);
        if (remarQIsNotNullOrUndefined_LISTID_(listTable)) {
            tableId = tableId.toLowerCase();
            listTable = document.getElementById(tableId);
        }
        if (remarQIsNotNullOrUndefined_LISTID_(listTable)) {
            var tableHeaders = listTable.getElementsByTagName("th");
            if (null !== tableHeaders) {
                var headerCount = tableHeaders.length;
                for (var headerIdx = 0; headerIdx < headerCount; headerIdx++) {
                    var headerCell = tableHeaders[headerIdx];
                    var headerTitle = headerCell.getAttribute("title");
                    if (remarQIsNotNullOrUndefined_LISTID_(headerTitle) && headerTitle === "_FIELDNAME_") {
                        headerCell.innerHTML = "";
                        break;
                    }
                }
            }
        }

        //some of the code here is overly complex, this is because our
        //support script may not be ready yet, if we try to wait on it
        //then the page rendering order is fubar so we take some added
        //complexity here for markup of the read/unread indicators
        remarQRenderContext_LISTID_ = renderCtx;
        renderReadMarksAndMenu_LISTID_(renderCtx, true);


        if (remarQAlertDialog_LISTID_ === "true") {
            SP.SOD.executeFunc('sp.js', 'SP.ClientContext', remarQReminder_LISTID_);
            remarQAlertDialog_LISTID_ = "false";
        }

    }
}

function remarQRegisterTemplates_LISTID_() {
    if (typeof SPClientTemplates === "undefined") return;
    if (!remarQIsRegistered_LISTID_) {

        var overrideCtx_LISTID_ = {};
        overrideCtx_LISTID_.Templates = {};
        //overrideCtx_LISTID_.BaseViewID = ; 
        //overrideCtx_LISTID_.ListTemplateType = 108;
        overrideCtx_LISTID_.OnPreRender = remarQ_LISTID_PreRender;
        overrideCtx_LISTID_.OnPostRender = remarQ_LISTID_PostRender;
        SPClientTemplates.TemplateManager.RegisterTemplateOverrides(overrideCtx_LISTID_);
        remarQIsRegistered_LISTID_ = true;

    }
};

//RegisterModuleInit("_PATHANDQUERY_", remarQRegisterTemplates_LISTID_);
if (window.XMLHttpRequest) {
    remarQRegisterTemplates_LISTID_();
}