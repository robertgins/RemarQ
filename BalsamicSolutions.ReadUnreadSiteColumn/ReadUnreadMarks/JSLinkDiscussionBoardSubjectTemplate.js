//specialized page for marking up the "Subject" view of
//a SharePoint 2013 disucssion board, aka AllItems.aspx
//necessary because discussion boards have different 
//rendering processes then document libraries and lists
var remarQIsRegistered_LISTID_ = false;
var remarQListName_LISTID_ = "_LISTGUID_";
var remarQFieldGuid_LISTID_ = "_FIELDGUID_";
var remarQListId_LISTID_ = "_LISTID_";
var remarQAlertDialog_LISTID_ = "_ALERT_";
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

function remarQReminder_LISTID_() {
    var dialogOpts = SP.UI.$create_DialogOptions();
    dialogOpts.width = 500;
    dialogOpts.height = 250;
    dialogOpts.title = "_REMINDERTITLE_";
    dialogOpts.url = "_LICENSEREMINDERURL_";
    SP.UI.ModalDialog.showModalDialog(dialogOpts);
}

function remarQReplaceAll_LISTID_(sourceText, findThis, replaceWith) {
    while (sourceText.indexOf(findThis) > -1) {
        sourceText = sourceText.replace(findThis, replaceWith);
    }
    return sourceText;
}
function remarQEncoredUri_LISTID_(encodeMe) {
    //cant do a reg expression replace because of the ( and )
    var retVal = encodeURIComponent(encodeMe);
    retVal = remarQReplaceAll_LISTID_(retVal,'\'', '%27');
    retVal = remarQReplaceAll_LISTID_(retVal,'(', '%28');
    retVal = remarQReplaceAll_LISTID_(retVal,')', '%29');
    retVal = remarQReplaceAll_LISTID_(retVal, '!', '%21');
    retVal = remarQReplaceAll_LISTID_(retVal, '-', '%2D');
    retVal = remarQReplaceAll_LISTID_(retVal, '_', '%5F');
    return retVal;
}

function renderReadMarks_LISTID_(renderCtx) {
      if (remarQIsNotNullOrUndefined_LISTID_(remarQRenderTimer_LISTID_)) {
            window.clearInterval(remarQRenderTimer_LISTID_);
            remarQRenderTimer_LISTID_ = null;
      }

    var remarQReadMarks = getReadUnreadMarks_LISTID_(renderCtx);
    var allTags = document.getElementsByTagName("a");
    for (var rowIdx = 0; rowIdx < renderCtx.ListData.Row.length; rowIdx++) {
        var isRead = remarQReadMarks[renderCtx.ListData.Row[rowIdx].ID];
        if (false === isRead) {
            //we need to know which group this points to so  we will find it by FileRef, since its not acutally
            //tagged in the genrated html, it matchs becuase they are genrated from the same source
            var fileRef = "RootFolder=" + remarQEncoredUri_LISTID_(renderCtx.ListData.Row[rowIdx].FileRef) + "&";
            for (var tagIdx = 0; tagIdx < allTags.length; tagIdx++) {
                var ancTag = allTags[tagIdx];
                if (null !== ancTag) {
                    if (ancTag.href.indexOf(fileRef) > -1) {
                        //we only expect one in th list with this match
                        ancTag.setAttribute("class", "unreadLink");
                        break;
                    }
                }
            }
        }
    }
    var refreshInterval = parseInt("_REFRESHINTERVAL_") || 0;
    if (refreshInterval > 0) {
        if (remarQIsNullOrUndefined_LISTID_(remarQRenderTimer_LISTID_)) {
            remarQRenderTimer_LISTID_ = window.setInterval(function () { renderReadMarks_LISTID_(renderCtx); }, refreshInterval * 1000);
        }
    }
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
        //add a style that supports marking teh links with our highlight color
        var unreadStyle = document.createElement('style')
        unreadStyle.type = 'text/css'
        unreadStyle.innerHTML = 'a.unreadLink:link {font-weight:bolder;} a.unreadLink:link {color:_COLRENDERCOLOR_;} a.unreadLink:visited {color:_COLRENDERCOLOR_;} a.unreadLink:hover {color:_COLRENDERCOLOR_;} a.unreadLink:active {color:_COLRENDERCOLOR_;}';
        document.getElementsByTagName('head')[0].appendChild(unreadStyle)
    }
}

function remarQ_LISTID_PostRender(renderCtx) {
    if (renderCtx.listName === remarQListName_LISTID_) {
        renderReadMarks_LISTID_(renderCtx);
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
        overrideCtx_LISTID_.BaseViewID = 3;
        overrideCtx_LISTID_.ListTemplateType = 108;
        overrideCtx_LISTID_.OnPostRender = remarQ_LISTID_PostRender;
        overrideCtx_LISTID_.OnPreRender = remarQ_LISTID_PreRender;
        SPClientTemplates.TemplateManager.RegisterTemplateOverrides(overrideCtx_LISTID_);
        remarQIsRegistered_LISTID_ = true;
    }
};

//RegisterModuleInit("_PATHANDQUERY_", remarQRegisterTemplates_LISTID_);
if (window.XMLHttpRequest) {
    remarQRegisterTemplates_LISTID_();
}
