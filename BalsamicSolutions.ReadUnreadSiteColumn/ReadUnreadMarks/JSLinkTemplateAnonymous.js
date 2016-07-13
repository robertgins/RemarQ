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


function remarQIsNullOrUndefined_LISTID_(checkMe) {
    if (null === checkMe) return true;
    if (undefined === checkMe) return true;
    return false;
}

function remarQIsNotNullOrUndefined_LISTID_(checkMe) {
    return !remarQIsNullOrUndefined_LISTID_(checkMe);
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


    }
}

function remarQRegisterTemplates_LISTID_() {
    if (typeof SPClientTemplates === "undefined") return;
    if (!remarQIsRegistered_LISTID_) {

        var overrideCtx_LISTID_ = {};
        overrideCtx_LISTID_.Templates = {};
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