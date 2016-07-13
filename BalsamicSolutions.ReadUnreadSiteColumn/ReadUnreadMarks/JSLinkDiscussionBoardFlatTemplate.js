//specialized page for marking up the "Subject" view of
//a SharePoint 2013 disucssion board, aka AllItems.aspx
//necessary because discussion boards have different 
//rendering processes then document libraries and lists
var remarQIsRegistered_LISTID_ = false;
var remarQListName_LISTID_ = "_LISTGUID_";
var remarQFieldGuid_LISTID_ = "_FIELDGUID_";
var remarQListId_LISTID_ = "_LISTID_";
var remarQAlertDialog_LISTID_ = "_ALERT_";
var remarQLoadingImage_LISTID_ = "_LOADINGIMGURL_";
var remarQMenuImage_LISTID_ = "_MENUIMGURL_";
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


function getReadUnreadMarks_LISTID_(renderCtx) {

    //we are going to get all of our read unread information
    //from the query service, if its not there or errors out
    //we will use the inline information, otherwise 
    //we will use the udpated information from the query
    var returnValue = new Array();
    var requestURL = "_RUQUERY_&userId=" + renderCtx.CurrentUserId + "&itemIds=";
    for (var rowIdx = 0; rowIdx < renderCtx.ListData.Row.length; rowIdx++) {
        requestURL += renderCtx.ListData.Row[rowIdx].ID;
        requestURL += "%2C";
    }
    requestURL += "&cc=" + Math.random();
    var httpRequest = new XMLHttpRequest();
    //we dont want any other processing until we are done
    //so this request is synchronous but if it times
    //out we will simply move on
    httpRequest.open("GET", requestURL, false);
    try {
        httpRequest.timeout = 4500;
    }
    catch (e) {
        if (e.name !== "InvalidAccessError") {
            throw e;
        }
    }
    httpRequest.send(null);
    if (httpRequest.status === 200) {
        var respParts = httpRequest.responseText.split(",");
        for (var respIdx = 0; respIdx < respParts.length; respIdx++) {
            var resVal = respParts[respIdx].split("=");
            if (resVal.length === 2) {
                var isRead = (resVal[1] === "true");
                returnValue[resVal[0]] = isRead;
            }
        }
    }
    return returnValue;
}

function toggleReadUnreadMark_LISTID_(itemId) {
    //toggle the value 
    var requestURL = "_RUSERVICEURL_?itemId=" + itemId + "&listId=_LISTID_&markUnread=toggle";
    var imageId = "menuImg_LISTID__" + itemId;
    var imgTag = document.getElementById(imageId);
    if (null !== imgTag) {
        imgTag.setAttribute("src", remarQLoadingImage_LISTID_);
    }
    var httpRequest = new XMLHttpRequest();
    httpRequest.onreadystatechange = function () {
        if (4 === httpRequest.readyState) {
            //update the screen on completion
            renderReadMarksAndMenu_LISTID_(remarQRenderContext_LISTID_, false);
            if (null !== imgTag) {
                imgTag.setAttribute("src", remarQMenuImage_LISTID_);
            }
        }
    };
    httpRequest.open("GET", requestURL, true);
    httpRequest.send(null);
}

function renderReadMarksAndMenu_LISTID_(renderCtx, updateMenu) {

    if (remarQIsNotNullOrUndefined_LISTID_(remarQRenderTimer_LISTID_)) {
        window.clearInterval(remarQRenderTimer_LISTID_);
        remarQRenderTimer_LISTID_ = null;
    }

    //get our read marks
    var remarQReadMarks = getReadUnreadMarks_LISTID_(renderCtx);
    for (var rowIdx = 0; rowIdx < renderCtx.ListData.Row.length; rowIdx++) {
        var isRead = remarQReadMarks[renderCtx.ListData.Row[rowIdx].ID];
        //again there is no tag info in the html but the row Body has the
        //ExternalClassXXXXX assigned to it so we can use that to locate
        //the tag to markup
        var bodyTag = renderCtx.ListData.Row[rowIdx].Body;
        var quoteChar = "\"";
        var posClass = bodyTag.indexOf("class=\"ExternalClass");
        if (posClass === -1) {
            posClass = bodyTag.indexOf("class='ExternalClass");
            quoteChar = "'";
        }
        if (posClass > -1) {
            var posTag = bodyTag.indexOf("ExternalClass", posClass);
            var posEndTag = bodyTag.indexOf(quoteChar, posTag);
            var classTag = bodyTag.substring(posTag, posEndTag);
            var divTags = document.getElementsByClassName(classTag);
            if (null !== divTags) {
                for (var divIdx = 0; divIdx < divTags.length; divIdx++) {
                    var divTag = divTags[divIdx];
                    if (false === isRead) {
                        divTag.style.color = "_COLRENDERCOLOR_";
                        divTag.style.fontWeight = "bolder";
                    } else {
                        divTag.style.color = "";
                        divTag.style.fontWeight = "";
                    }
                    if (updateMenu) {
                        //Now add the menu html
                        if (null !== divTag.parentNode && null !== divTag.parentNode.parentNode && null !== divTag.parentNode.parentNode.parentNode) {
                            var topTag = divTag.parentNode.parentNode.parentNode;
                            var childSpans = topTag.getElementsByTagName("span");
                            for (var spanIdx = 0; spanIdx < childSpans.length; spanIdx++) {
                                var menuSpan = childSpans[spanIdx];
                                if (menuSpan.hasAttribute("id")) {
                                    var menuId = menuSpan.getAttribute("id");
                                    if (menuId.toLowerCase().indexOf("commandbar") === 0) {
                                        addReadUnreadMenu_LISTID_(menuSpan, menuId, renderCtx.ListData.Row[rowIdx].ID, isRead)
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    var refreshInterval = parseInt("_REFRESHINTERVAL_") || 0;
    if (refreshInterval > 0) {
        if (remarQIsNullOrUndefined_LISTID_(remarQRenderTimer_LISTID_)) {
            remarQRenderTimer_LISTID_ = window.setInterval(function () { renderReadMarksAndMenu_LISTID_(renderCtx, false); }, refreshInterval * 1000);
        }
    }
}

function addReadUnreadMenu_LISTID_(menuElem, menuId, itemId, isRead) {
    var menuNum = menuId.substring(10);
    var htmlList = menuElem.getElementsByTagName("ul");
    if (null !== htmlList && htmlList.length === 1) {
        var listElem = htmlList[0];
        addReadUnreadMenuItem_LISTID_(listElem, menuNum, itemId);
    }
}

function addReadUnreadMenuItem_LISTID_(listElem, menuNum, itemId) {
    //add an LI element to ListElem with the  
    var commandId = "commandBar" + menuNum;
    var newMenu = document.createElement("li");
    newMenu.setAttribute("class", "ms-comm-cmdSpaceListItem");
    newMenu.setAttribute("style", "cursor: pointer;");
    newMenu.setAttribute("id", commandId + "-togglereadunread-link");
    var anchorTag = document.createElement("a");
    anchorTag.setAttribute("onclick", "javascript:toggleReadUnreadMark_LISTID_(" + itemId + ");return false;");
    anchorTag.setAttribute("style", "cursor: pointer;");

    var imgTag = document.createElement("img");
    imgTag.setAttribute("id", "menuImg_LISTID__" + itemId);
    imgTag.setAttribute("src", "_MENUIMGURL_");
    imgTag.setAttribute("alt", remarQMenuImage_LISTID_);
    imgTag.setAttribute("style", "cursor: pointer;border:0px,none;height:16px;width:16px;");

    anchorTag.appendChild(imgTag);
    newMenu.appendChild(anchorTag);
    listElem.appendChild(newMenu);
}

function remarQReminder_LISTID_() {
    var dialogOpts = SP.UI.$create_DialogOptions();
    dialogOpts.width = 500;
    dialogOpts.height = 250;
    dialogOpts.title = "_REMINDERTITLE_";
    dialogOpts.url = "_LICENSEREMINDERURL_";
    SP.UI.ModalDialog.showModalDialog(dialogOpts);
}

function remarQ_LISTID_PreRender(renderCtx) {
    //first check to see if we have a RemarQ field we have to do this because the view has
    //our jslink and not the field
    if (remarQIsNotNullOrUndefined_LISTID_(renderCtx) && remarQIsNotNullOrUndefined_LISTID_(renderCtx.ListSchema)
       && remarQIsNotNullOrUndefined_LISTID_(renderCtx.ListSchema.Field) && remarQIsNotNullOrUndefined_LISTID_(renderCtx.ListSchema.Field.length)) {
        if (renderCtx.listName === remarQListName_LISTID_) {
            //our list so chek it (we dont want one)
            var fieldCount = renderCtx.ListSchema.Field.length;
            var fieldExists = false;
            for (var fieldIdx = 0; fieldIdx < fieldCount; fieldIdx++) {
                if (renderCtx.ListSchema.Field[fieldIdx].ID === remarQFieldGuid_LISTID_) {
                    fieldExists = true;
                    break;
                }
            }
            if (!fieldExists) {
                //clear our match so that nothing else will render
                remarQListName_LISTID_ = "{NOT-SUPPORTED}";
            }
        }
    }
    if (renderCtx.listName === remarQListName_LISTID_) {
        //capture the render context in case we need it later
        remarQRenderContext_LISTID_ = renderCtx;
    }
}

function remarQ_LISTID_PostRender(renderCtx) {
    if (renderCtx.listName === remarQListName_LISTID_) {

        renderReadMarksAndMenu_LISTID_(remarQRenderContext_LISTID_, true);

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
        overrideCtx_LISTID_.BaseViewID = 2;
        overrideCtx_LISTID_.ListTemplateType = 108;
        overrideCtx_LISTID_.OnPostRender = remarQ_LISTID_PostRender;
        overrideCtx_LISTID_.OnPreRender = remarQ_LISTID_PreRender;
        SPClientTemplates.TemplateManager.RegisterTemplateOverrides(overrideCtx_LISTID_);
        remarQIsRegistered_LISTID_ = true;
    }
}

//RegisterModuleInit("_PATHANDQUERY_", remarQRegisterTemplates_LISTID_);
if (window.XMLHttpRequest) {
    remarQRegisterTemplates_LISTID_();
}