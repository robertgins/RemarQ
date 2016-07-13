var unreadImage_LISTID_Url = "_UNREADIMAGEURL_";
var readImage_LISTID_Url = "_READIMAGEURL_";
var errorImage_LISTID_Url = "_ERRORIMAGEURL_";
var colRenderMode_LISTID_ = "_COLRENDERMODE_";
var colRenderColor_LISTID_ = "_COLRENDERCOLOR_";
var threadRenderColor_LISTID_ = "_THREADRENDERCOLOR_";
var ruService_LISTID_Url = "_RUSERVICEURL_";
var ruVarPrefix_LISTID_ = "rum__LISTID__";
var ruOnClickFuncName_LISTID_ = "DispEx_LISTID_";
var ruListId_LISTID_ = "_LISTID_";
var ruFieldGuid_LISTID_ = "_FIELDGUID_";
var ruModuleActive_LISTID_ = "_HTTPMODULE_";
var ruAlertDialog_LISTID_ = "_ALERT_";
var remarQReportTitle_LISTID_ = "_REPORTTITLE_";
var remarQReminderTitle_LISTID_ = "_REMINDERTITLE_";
var ruUnReadItemsArray_LISTID_ = [];
var ruDispEx_LISTID_ = null;

 

function remarQ_LISTID_PreRender(renderCtx) {
    //Place holder, might be able to remove this
   
 }


function remarQ_LISTID_PostRender(renderCtx) {

    for (var rowIdx = 0; rowIdx < renderCtx.ListData.LastRow; rowIdx++) {
        var readOrUnread = renderCtx.ListData.Row[rowIdx].ReadOrUnread;
        alert(readOrUnread);
    }
    debugger;
 }

function readUnread_LISTID_ItemRender(renderCtx) {
    
    return "";
}


(function () {
    
    if (typeof SPClientTemplates === "undefined") return;

    var overrideCtx_LISTID_ = {};
    overrideCtx_LISTID_.Templates = {};
    overrideCtx_LISTID_.Templates.Item = readUnread_LISTID_ItemRender;
    overrideCtx_LISTID_.OnPreRender = remarQ_LISTID_PreRender;
    overrideCtx_LISTID_.OnPostRender = remarQ_LISTID_PostRender;
    SPClientTemplates.TemplateManager.RegisterTemplateOverrides(overrideCtx_LISTID_);

})();