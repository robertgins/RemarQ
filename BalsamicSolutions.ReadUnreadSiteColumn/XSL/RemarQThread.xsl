<xsl:stylesheet xmlns:x="http://www.w3.org/2001/XMLSchema" xmlns:d="http://schemas.microsoft.com/sharepoint/dsp" version="1.0" exclude-result-prefixes="xsl msxsl ddwrt" xmlns:ddwrt="http://schemas.microsoft.com/WebParts/v2/DataView/runtime" xmlns:asp="http://schemas.microsoft.com/ASPNET/20" xmlns:__designer="http://schemas.microsoft.com/WebParts/v2/DataView/designer" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:SharePoint="Microsoft.SharePoint.WebControls" xmlns:ddwrt2="urn:frontpage:internal" ddwrt:oob="true">
  <xsl:import href="/_layouts/15/xsl/main.xsl"/>
  <xsl:output method="html" indent="no"/>
  <xsl:param name="CAML_Expand"/>
  <xsl:param name="CAML_ShowOriginalEmailBody"/>
  <xsl:template name="FieldRef_Thread_PersonViewMinimal_Computed_body" match="FieldRef[@Name='PersonViewMinimal']" mode="Computed_body" ddwrt:dvt_mode="body">
    <xsl:param name="thisNode" select="."/>
    <table cellpadding="0" cellspacing="0">
      <tr>
        <td style="padding-left: 5px;">
          <table cellpadding="0" cellspacing="0">
            <tr>
              <td width="1px">
                <a onclick="GoToLink(this);return false;" href="{$ServerRelativeUrl}/_layouts/15/userdisp.aspx?ID={$thisNode/@MyEditor.id}">
                  <img>
                    <xsl:choose>
                      <xsl:when test="not($thisNode/@MyEditor.picture) or $thisNode/@MyEditor.picture=''">
                        <xsl:attribute name="width">62</xsl:attribute>
                        <xsl:attribute name="height">62</xsl:attribute>
                        <xsl:attribute name="border">0</xsl:attribute>
                        <xsl:attribute name="src">"/_layouts/15/images/person.gif?rev=23"</xsl:attribute>
                        <xsl:attribute name="alt">
                          <xsl:value-of select="$Rows/@resource.wss.userinfo_schema_pictureplaceholderalt"/>
                          <xsl:text ddwrt:whitespace-preserve="yes" xml:space="preserve"> </xsl:text>
                          <xsl:value-of select="$thisNode/@Editor.title"/>
                          <xsl:text ddwrt:whitespace-preserve="yes" xml:space="preserve"> </xsl:text>
                        </xsl:attribute>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:attribute name="style">max-width:62px; max-height:62px; border:none;</xsl:attribute>
                        <xsl:attribute name="src">
                          <xsl:value-of select="$thisNode/@MyEditor.picture"/>
                        </xsl:attribute>
                        <xsl:attribute name="alt">
                          <xsl:value-of select="$Rows/@resource.wss.userinfo_schema_picturealt"/>
                          <xsl:text ddwrt:whitespace-preserve="yes" xml:space="preserve"> </xsl:text>
                          <xsl:value-of select="$thisNode/@Editor.title"/>
                          <xsl:text ddwrt:whitespace-preserve="yes" xml:space="preserve"> </xsl:text>
                        </xsl:attribute>
                      </xsl:otherwise>
                    </xsl:choose>
                  </img>
                </a>
              </td>
              <td height="1px" width="80px" valign="bottom">
                <img src="/_layouts/15/images/blank.gif?rev=23" width="80px" height="1px" alt="" />
              </td>
            </tr>
          </table>
        </td>
      </tr>
      <tr>
        <td style="padding-left: 5px;" nowrap="nowrap">
          <xsl:value-of select="$thisNode/@Editor.span" disable-output-escaping="yes"/>
        </td>
      </tr>
    </table>
  </xsl:template>
  <xsl:template name="FieldRef_Thread_BodyAndMore_Computed_Thread" match="FieldRef[@Name='BodyAndMore']" mode="Computed_body" ddwrt:dvt_mode="body">
    <xsl:param name="thisNode" select="."/>
    <xsl:param name="Position" select="1"/>
    <xsl:if test="$Position = 1">
      <input type="hidden" name="CAML_Expand" value="{$CAML_Expand}"/>
      <input type="hidden" name="CAML_ShowOriginalEmailBody" value="{$CAML_ShowOriginalEmailBody}"/>
    </xsl:if>
    <xsl:variable name="isRootPost">
      <xsl:call-template name="IsRootPost">
        <xsl:with-param name="thisNode" select="$thisNode"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:if test="$isRootPost='TRUE'">
      <div style="padding-bottom: 4px;">
        <b>
          <xsl:choose>
            <xsl:when test="$thisNode/@FSObjType='1'">
              <xsl:value-of select="$thisNode/@Title"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$thisNode/@DiscussionTitleLookup"/>
            </xsl:otherwise>
          </xsl:choose>
        </b>
      </div>
    </xsl:if>
    <xsl:variable name="BodyPositioningClass">
      <xsl:if test="$isRootPost='TRUE'">ms-disc-root-body</xsl:if>
    </xsl:variable>
    <div class="{$BodyPositioningClass}">
      <xsl:choose>
        <xsl:when test="$thisNode/@Body=''">
          <xsl:value-of select="$thisNode/../@resource.wss.NoText"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="WasExpanded">
            <xsl:call-template name="BodyWasExpanded">
              <xsl:with-param name="thisNode" select ="$thisNode"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="$WasExpanded='TRUE'">
              <!-- fullbody -->
              <xsl:call-template name="FullBody">
                <xsl:with-param name="thisNode" select="$thisNode"/>
              </xsl:call-template>
              <xsl:variable name="CorrectBody">
                <xsl:call-template name="CorrectBodyToShow">
                  <xsl:with-param name="thisNode" select="$thisNode"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:choose>
                <xsl:when test="contains($CorrectBody, 'TrimmedBody')">
                  <div>
                    <table border="0" cellspacing="0" cellpadding="0" class="ms-disc" dir="{$XmlDefinition/List/@direction}">
                      <tr valign="top">
                        <td>
                          <xsl:call-template name="LessLink">
                            <xsl:with-param name="thisNode" select ="$thisNode"/>
                          </xsl:call-template>
                        </td>
                        <td> | </td>
                        <td>
                          <xsl:call-template name="ToggleQuotedText">
                            <xsl:with-param name="thisNode" select ="$thisNode"/>
                          </xsl:call-template>
                        </td>
                      </tr>
                    </table>
                  </div>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="LessLink">
                    <xsl:with-param name="thisNode" select ="$thisNode"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="TextWasExpanded">
                <xsl:call-template name="QuotedTextWasExpanded">
                  <xsl:with-param name="thisNode" select ="$thisNode"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:choose>
                <xsl:when test="$TextWasExpanded='TRUE'">
                  <xsl:call-template name="FullBody">
                    <xsl:with-param name="thisNode" select="$thisNode"/>
                  </xsl:call-template>
                  <xsl:call-template name="ToggleQuotedText">
                    <xsl:with-param name="thisNode" select ="$thisNode"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="LimitedBody">
                    <xsl:with-param name="thisNode" select ="$thisNode"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>
  <xsl:template name="FieldRef_Thread_Threading_Computed_Thread" match="FieldRef[@Name='Threading']" mode="Computed_body" ddwrt:dvt_mode="body">
    <xsl:param name="thisNode" select="."/>
    <xsl:param name="Position" select="1"/>
    <table class="ms-disc-nopad" border="0" cellpadding="0" cellspacing="0" width="100%">
      <tr>
        <td width="1px" rowspan="2">
          <xsl:call-template name="Indentation">
            <xsl:with-param name="thisNode" select="$thisNode"/>
          </xsl:call-template>
        </td>
        <td class="ms-disc-bordered-noleft">
          <xsl:call-template name="FieldRef_Thread_BodyAndMore_Computed_Thread">
            <xsl:with-param name="thisNode" select="$thisNode"/>
            <xsl:with-param name="Position" select="$Position"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <td height="1px" valign="bottom">
          <img src="/_layouts/15/images/blank.gif?rev=23" alt="" width="450px" height="1px" />
        </td>
      </tr>
    </table>
  </xsl:template>
  <xsl:template name="LessLink">
    <xsl:param name="thisNode" select="."/>
    <div>
      <br />
      <a id="LessLink{$thisNode/@ID}" href="javascript:" onclick="javascript:return CollapseBody('{$thisNode/@GUID}','{$thisNode/@GUID}',this);">
        <img id="lessIcon" border="0" align="absmiddle" alt="{$thisNode/../@resource.wss.LessText}" src="/_layouts/15/images/dlmin.gif?rev=23" />
        <xsl:value-of select="$thisNode/../@resource.wss.LessText"/>
      </a>
    </div>
  </xsl:template>
  <xsl:template name="FullBody">
    <xsl:param name="thisNode" select="."/>
    <xsl:variable name="MessageBodyText">
      <xsl:call-template name="MessageBody">
        <xsl:with-param name="thisNode" select="$thisNode"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@RichText='TRUE'">
        <xsl:value-of select="$MessageBodyText" disable-output-escaping ="yes"/>
      </xsl:when>
      <xsl:otherwise>
        <!--todo: deal with autonewline stuffs.-->
        <xsl:value-of select="$MessageBodyText" disable-output-escaping ="yes"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="ToggleQuotedText">
    <xsl:param name="thisNode" select="."/>
    <xsl:variable name="CorrectBody">
      <xsl:call-template name="CorrectBodyToShow">
        <xsl:with-param name="thisNode" select="$thisNode"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$CorrectBody='TrimmedBody'">
        <div>
          <br />
          <a id="ToggleQuotedText{$thisNode/@ID}" href="javascript:" onclick="javascript:return ShowQuotedText('{$thisNode/@GUID}','{$thisNode/@GUID}',this);">
            <span style="height:13px;width:13px;position:relative;display:inline-block;overflow:hidden;" class="s4-clust">
              <img src="/_layouts/15/images/fgimg.png?rev=23" alt="More" style="left:-0px !important;top:-158px !important;position:absolute;" id="showQuotedIcon" border="0" align="absmiddle" />
            </span>
            <xsl:value-of select="$thisNode/../@resource.wss.ShowQuotedMessages"/>
          </a>
        </div>
      </xsl:when>
      <xsl:when test="$CorrectBody='UnTrimmedBody'">
        <div>
          <br />
          <a id="ToggleQuotedText{$thisNode/@ID}" href="javascript:" onclick="javascript:return HideQuotedText('{$thisNode/@GUID}','{$thisNode/@GUID}',this);">
            <img id="hideQuotedIcon" border="0" align="absmiddle" alt="More" src="/_layouts/15/images/dlmin.gif?rev=23" />
            <xsl:value-of select="$thisNode/../@resource.wss.HideQuotedMessages"/>
          </a>
        </div>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="MessageBody">
    <xsl:param name="thisNode" select="."/>
    <xsl:variable name="CorrectBody">
      <xsl:call-template name="CorrectBodyToShow">
        <xsl:with-param name="thisNode" select="$thisNode"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$CorrectBody='TrimmedBody'">
        <xsl:value-of select="$thisNode/@TrimmedBody"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$thisNode/@Body"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="LimitedBody">
    <xsl:param name="thisNode" select="."/>
    <xsl:variable name="MessageBodyText">
      <xsl:call-template name="MessageBody">
        <xsl:with-param name="thisNode" select="$thisNode"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@RichText='TRUE'">
        <xsl:value-of select="$MessageBodyText" disable-output-escaping ="yes"/>
        <xsl:call-template name="ToggleQuotedText">
          <xsl:with-param name="thisNode" select ="$thisNode"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="ddwrt:Limit($MessageBodyText, 1500, '...')" disable-output-escaping="yes"/>
        <xsl:call-template name="MoreLink">
          <xsl:with-param name="thisNode" select="$thisNode"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="MoreLink">
    <xsl:param name="thisNode" select="."/>
    <div>
      <br />
      <a id="MoreLink{$thisNode/@ID}" href="javascript:" onclick="javascript:return ExpandBody('{$thisNode/@GUID}'','{$thisNode/@GUID}',this);">
        <span style="height:13px;width:13px;position:relative;display:inline-block;overflow:hidden;" class="s4-clust">
          <img src="/_layouts/15/images/fgimg.png?rev=23" alt="{$thisNode/../@resource.wss.MoreText}" style="left:-0px !important;top:-158px !important;position:absolute;" id="moreIcon" border="0" align="absmiddle" />
        </span>
        <xsl:value-of select="$thisNode/../@resource.wss.ShowQuotedMessages"/>
        <xsl:value-of select="$thisNode/../@resource.wss.MoreText"/>
      </a>
    </div>
  </xsl:template>
  <xsl:template name="BodyWasExpanded">
    <xsl:param name="thisNode" select="."/>
    <xsl:choose>
      <xsl:when test="$CAML_Expand and contains($CAML_Expand, $thisNode/@GUID)">TRUE</xsl:when>
      <xsl:otherwise>FALSE</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="QuotedTextWasExpanded">
    <xsl:param name="thisNode" select="."/>
    <xsl:choose>
      <xsl:when test="$CAML_ShowOriginalEmailBody and contains($CAML_ShowOriginalEmailBody, $thisNode/@GUID)">TRUE</xsl:when>
      <xsl:otherwise>FALSE</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="CorrectBodyToShow">
    <xsl:param name="thisNode" select="."/>
    <xsl:variable name="WasExpanded">
      <xsl:call-template name="QuotedTextWasExpanded">
        <xsl:with-param name="thisNode" select ="$thisNode"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$WasExpanded='TRUE'">UnTrimmedBody</xsl:when >
      <xsl:otherwise>
        <!-- If we have an unedited item with a TrimmedBody, show that. Otherwise, show Body.-->
        <xsl:choose>
          <xsl:when test="$thisNode/@Created. =$thisNode/@Modified.">
            <xsl:variable name="isRootPost">
              <xsl:call-template name="IsRootPost">
                <xsl:with-param name="thisNode" select="$thisNode"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:choose>
              <xsl:when test="$isRootPost='TRUE'">BODY</xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="$thisNode/@TrimmedBody=''">BODY</xsl:when>
                  <xsl:otherwise>TrimmedBody</xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>Body</xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="IsRootPost">
    <xsl:param name="thisNode" select="."/>
    <xsl:variable name="V_ShortestThreadIndexId">
      <xsl:choose>
        <xsl:when test="@FSObjType = '1'">
          <xsl:choose>
            <xsl:when test="$thisNode/@ShortestThreadIndexId='0'">
              <xsl:value-of select="$thisNode/@ID"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$thisNode/@ShortestThreadIndexId"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$thisNode/@ShortestThreadIndexIdLookup"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$V_ShortestThreadIndexId=''">
        <xsl:variable name="indentLevel">
          <xsl:call-template name="IndentLevel">
            <xsl:with-param name="thisNode" select="$thisNode"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="$indentLevel=0">TRUE</xsl:when>
          <xsl:otherwise>FALSE</xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="$V_ShortestThreadIndexId=$thisNode/@ID">TRUE</xsl:when>
          <xsl:otherwise>FALSE</xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="IndentLevel">
    <xsl:param name="thisNode" select="."/>
    <xsl:value-of select="floor(string-length($thisNode/@ThreadIndex) div 10) - 4"/>
  </xsl:template>
  <xsl:template name="Indentation">
    <xsl:param name="thisNode" select="."/>
    <img src="/_layouts/15/images/blank.gif?rev=23" width="{string-length($thisNode/@ThreadIndex) - 46}px" height="1px" alt="" />
  </xsl:template>
  <xsl:template name="IndentStatusBar">
    <xsl:param name="thisNode" select="."/>
    <table class="ms-disc-nopad" border="0" cellpadding="0" cellspacing="0" width="100%">
      <tr>
        <td width="1px">
          <xsl:call-template name="Indentation">
            <xsl:with-param name="thisNode" select="$thisNode"/>
          </xsl:call-template>
        </td>
        <td width="100%">
          <xsl:call-template name="StatusBar">
            <xsl:with-param name="thisNode" select="$thisNode"/>
          </xsl:call-template>
        </td>
      </tr>
    </table>
  </xsl:template>
  <xsl:template match="FieldRef[@Name='ReplyNoGif']" mode="Computed_body" ddwrt:dvt_mode="body">
    <xsl:param name="thisNode" select="."/>
    <xsl:call-template name="ReplyNoGif">
      <xsl:with-param name="thisNode" select ="$thisNode"/>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="FieldRef_Thread_StatusBar_Computed_body" match="FieldRef[@Name='StatusBar']" mode="Computed_body" ddwrt:dvt_mode="body">
    <xsl:param name="thisNode" select="."/>
    <xsl:call-template name="StatusBar">
      <xsl:with-param name="thisNode" select="$thisNode"/>
      <xsl:with-param name="indent" select="0"/>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="StatusBar">
    <xsl:param name="thisNode" select="."/>
    <xsl:param name="indent" select="1"/>
    <xsl:variable name="indentLevel">
      <xsl:call-template name="IndentLevel">
        <xsl:with-param name="thisNode" select="$thisNode"/>
      </xsl:call-template>
    </xsl:variable>
    <table class="ms-disc-bar" border="0" cellpadding="0" cellspacing="0" width="100%" margin-left="{$indentLevel}0px">
      <tr>
        <td width="100%" nowrap="TRUE">
          <div>
            <a name="{$thisNode/@GUID}"></a>
            <xsl:choose>
              <xsl:when test="$thisNode/@Created. = $thisNode/@Modified.">
                <xsl:choose>
                  <xsl:when test="$thisNode/@EmailSender=''">
                    <xsl:variable name="isRootPost">
                      <xsl:call-template name="IsRootPost">
                        <xsl:with-param name="thisNode" select="$thisNode"/>
                      </xsl:call-template>
                    </xsl:variable>
                    <xsl:choose>
                      <xsl:when test="$isRootPost='TRUE'">
                        <xsl:choose>
                          <xsl:when test="$indent">
                            <xsl:value-of select="$thisNode/../@resource.wss.Discussion_Started"/>
                            <xsl:value-of select="$thisNode/@Modified"/>
                            <xsl:text ddwrt:whitespace-preserve="yes" xml:space="preserve"> </xsl:text>
                            <xsl:value-of select="$ByText"/>
                            <xsl:value-of select="$thisNode/@Editor.span" disable-output-escaping="yes"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="$thisNode/../@resource.wss.Discussion_Started"/>
                            <xsl:value-of select="$thisNode/@Modified"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:choose>
                          <xsl:when test="$indent">
                            <xsl:value-of select="$thisNode/../@resource.wss.Discussion_Posted"/>
                            <xsl:value-of select="$thisNode/@Modified"/>
                            <xsl:text ddwrt:whitespace-preserve="yes" xml:space="preserve"> </xsl:text>
                            <xsl:value-of select="$ByText"/>
                            <xsl:value-of select="$thisNode/@Editor.span" disable-output-escaping="yes"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="$thisNode/../@resource.wss.Discussion_Posted"/>
                            <xsl:value-of select="$thisNode/@Modified"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="$indent">
                        <xsl:value-of select="$thisNode/../@resource.wss.Discussion_Emailed"/>
                        <xsl:value-of select="$thisNode/@Modified"/>
                        <xsl:text ddwrt:whitespace-preserve="yes" xml:space="preserve"> </xsl:text>
                        <xsl:value-of select="$ByText"/>
                        <xsl:value-of select="$thisNode/@Editor.span" disable-output-escaping="yes"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$thisNode/../@resource.wss.Discussion_Emailed"/>
                        <xsl:value-of select="$thisNode/@Modified"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="$indent">
                    <xsl:value-of select="$thisNode/../@resource.wss.Discussion_Edited"/>
                    <xsl:value-of select="$thisNode/@Modified"/>
                    <xsl:text ddwrt:whitespace-preserve="yes" xml:space="preserve"> </xsl:text>
                    <xsl:value-of select="$ByText"/>
                    <xsl:value-of select="$thisNode/@Editor.span" disable-output-escaping="yes"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$thisNode/../@resource.wss.Discussion_Edited"/>
                    <xsl:value-of select="$thisNode/@Modified"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </div >
        </td >
        <xsl:if test="$thisNode/@Attachments='1'">
          <td width="1" nowrap="TRUE">
            <div>
              <nobr>
                <xsl:apply-templates select="." mode="Attachments_body">
                  <xsl:with-param name="thisNode" select="$thisNode"/>
                </xsl:apply-templates>
              </nobr>
            </div>
          </td>
        </xsl:if>
        <td style="border-style:none" nowrap="TRUE">
          <div>
            <!--RemarQ is this where the remarq toggle link is inserted-->
            <nobr>
              <xsl:if test="$Userid!='-1'">
                <a id="remarQLink{$thisNode/@ID}" remarQItemId ="{$thisNode/@ID}" remarQListId="{$List}" class="ms-comm-cmdSpaceListItem" style="cursor: pointer;" onclick="javascript:remarQToggleReadUnreadMark('{$thisNode/@ID}','{$List}','{$Userid}');return false;">
                  <nobr>
                    <img id="remarQMenuImg{$thisNode/@ID}" src="/_layouts/15/images/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadLogo16.png" style="cursor: pointer;border:0px,none;height:16px;width:16px;" alt="" />
                  </nobr>
                </a>
              </xsl:if>
              <a id="DisplayLink{$thisNode/@ID}" href="{$FORM_DISPLAY}&amp;ID={$thisNode/@ID}" onclick="EditLink2(this,{$ViewCounter});return false;" target="_self">
                <nobr>
                  <xsl:value-of select="$thisNode/../@resource.wss.ViewItemLink"/>
                </nobr>
              </a>
            </nobr>
          </div>
        </td>
        <xsl:variable name="hasRight">
          <xsl:call-template name="IfHasRight">
            <xsl:with-param name="thisNode" select ="$thisNode"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="$hasRight">
          <td class="ms-separator">
            <img src='/_layouts/15/images/blank.gif' alt='' />
          </td>
          <td style="border-style:none" nowrap="nowrap">
            <div>
              <xsl:call-template name="ReplyNoGif">
                <xsl:with-param name="thisNode" select ="$thisNode"/>
              </xsl:call-template>
            </div>
          </td>
        </xsl:if>
      </tr>
    </table>
  </xsl:template>
  <xsl:template name="ReplyNoGif">
    <xsl:param name="thisNode" select="."/>
    <a id="ReplyLink{$thisNode/@ID}" href="{$ENCODED_FORM_NEW}&amp;ContentTypeId=0x0107&amp;DiscussionParentID={$thisNode/@ID}" onclick="EditLink2(this,{$ViewCounter});return false;" target="_self">
      <img id="replyButton" border="0" align="middle" alt="{$thisNode/../@resource.wss.ReplyLinkText}" src="/_layouts/15/images/reply.gif?rev=23" />
      <xsl:text disable-output-escaping="yes" ddwrt:nbsp-preserve="yes">&amp;nbsp;</xsl:text>
      <nobr>
        <b>
          <xsl:value-of select="$thisNode/../@resource.wss.ReplyLinkText"/>
        </b>
      </nobr>
    </a>
  </xsl:template>
  <xsl:template mode="Item" match="Row[../../@BaseViewID='1']">
    <xsl:param name="Fields" select="."/>
    <xsl:param name="Collapse" select="."/>
    <xsl:param name="Position" select="1" />
    <xsl:variable name="thisNode" select="."/>
    <xsl:for-each select="$Fields">
      <tr>
        <xsl:if test="$Collapse">
          <xsl:attribute name="style">display:none</xsl:attribute>
        </xsl:if>
        <td class="ms-disc-padabove">
          <xsl:call-template name="IndentStatusBar">
            <xsl:with-param name="thisNode" select="$thisNode"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <xsl:if test="$Collapse">
          <xsl:attribute name="style">display:none</xsl:attribute>
        </xsl:if>
        <td>
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when test="position()=1">ms-disc-bordered</xsl:when>
              <xsl:otherwise>ms-disc-bordered-noleft</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:if test="@Name='Threading'">
            <xsl:attribute name="width">100%</xsl:attribute>
          </xsl:if>
          <xsl:apply-templates mode="PrintFieldWithDisplayFormLink" select=".">
            <xsl:with-param name="thisNode" select="$thisNode"/>
            <xsl:with-param name="Position" select="$Position"/>
          </xsl:apply-templates>
        </td>
      </tr>
      <tr>
        <td>
          <img src="/_layouts/15/images/blank.gif?rev=23" width="1px" height="15px" alt=""/>
        </td>
      </tr>
    </xsl:for-each>
  </xsl:template>
  <xsl:template mode="Item" match="Row[../../@BaseViewID='2']">
    <xsl:param name="Fields" select="."/>
    <xsl:param name="Collapse" select="."/>
    <xsl:param name="Position" select="1" />
    <xsl:variable name="thisNode" select="."/>
    <tr>
      <xsl:if test="$Collapse">
        <xsl:attribute name="style">display:none</xsl:attribute>
      </xsl:if>
      <td colspan="{count($Fields)}">
        <xsl:attribute name="class">
          <xsl:choose>
            <xsl:when test="position()=1">ms-disc-padabove</xsl:when>
            <xsl:otherwise>ms-disc-nopad</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:for-each select="$XmlDefinition/ViewFields/FieldRef[@Name='StatusBar']">
          <xsl:apply-templates mode="PrintFieldWithDisplayFormLink" select=".">
            <xsl:with-param name="thisNode" select="$thisNode"/>
            <xsl:with-param name="Position" select="$Position"/>
          </xsl:apply-templates>
        </xsl:for-each>
      </td>
    </tr>
    <tr>
      <xsl:if test="$Collapse">
        <xsl:attribute name="style">display:none</xsl:attribute>
      </xsl:if>
      <xsl:for-each select="$Fields">
        <td>
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when test="position()=1">ms-disc-bordered</xsl:when>
              <xsl:otherwise>ms-disc-bordered-noleft</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:if test="@Name='BodyAndMore'">
            <xsl:attribute name="width">100%</xsl:attribute>
          </xsl:if>
          <xsl:apply-templates mode="PrintFieldWithDisplayFormLink" select=".">
            <xsl:with-param name="thisNode" select="$thisNode"/>
            <xsl:with-param name="Position" select="$Position"/>
          </xsl:apply-templates>
        </td>
      </xsl:for-each>
    </tr>
    <tr>
      <xsl:if test="$Collapse">
        <xsl:attribute name="style">display:none</xsl:attribute>
      </xsl:if>
      <td>
        <img src="/_layouts/15/images/blank.gif?rev=23" width="1px" height="15px" alt=""/>
      </td>
    </tr>
  </xsl:template>
  <xsl:template name="FieldRef_Thread_UserBody" match="FieldRef" mode="User_body" ddwrt:dvt_mode="body">
    <xsl:param name="thisNode" select="."/>
    <xsl:choose>
      <xsl:when test="$XmlDefinition/@BaseViewID='1' or $XmlDefinition/@BaseViewID='2'">
        <xsl:value-of disable-output-escaping="yes" select="$thisNode/@*[name()=concat(current()/@Name, '.span')]" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of disable-output-escaping="yes" select="$thisNode/@*[name()=current()/@Name]" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="View_Thread_Summary_RootTemplate" match="View[List/@TemplateType='108' and @BaseViewID='0']" mode="RootTemplate" ddwrt:dvt_mode="root">
    <xsl:apply-templates select="." mode="full" />
  </xsl:template>
  <xsl:template name="View_Thread_Default_RootTemplate" match="View[List/@TemplateType='108']" mode="RootTemplate" ddwrt:dvt_mode="root">
    <xsl:choose>
      <xsl:when test="$ClientRender='1'">
        <xsl:call-template name="CTXGeneration"/>
        <div id="script{$ContainerId}"></div>
        <div id="scriptPaging{$ContainerId}"></div>
      </xsl:when>
      <xsl:otherwise>
        <table width="100%" cellspacing="0" cellpadding="0" border="0">
          <xsl:call-template name="CTXGeneration"/>
          <tr>
            <td>
              <xsl:if test="not($NoAJAX)">
                <iframe src="javascript:false;" id="FilterIframe{$ViewCounter}" name="FilterIframe{$ViewCounter}" style="display:none" height="0" width="0" FilterLink="{$FilterLink}"></iframe>
              </xsl:if>
              <table id="{$List}-{$View}" Summary="{List/@title}" xmlns:o="urn:schemas-microsoft-com:office:office" o:WebQuerySourceHref="{$HttpPath}&amp;XMLDATA=1&amp;RowLimit=0&amp;View={$View}"
                     width="100%" border="0" cellspacing="0" cellpadding="1" dir="{List/@Direction}" >
                <xsl:if test="@BaseViewID='3'">
                  <xsl:attribute name="onmouseover">
                    EnsureSelectionHandler(event,this,<xsl:value-of select="$ViewCounter"/>)
                  </xsl:attribute>
                </xsl:if>
                <xsl:attribute name="class">
                  <xsl:choose>
                    <!-- Any time we display 0 items in a view = class="ms-viewEmpty" -->
                    <xsl:when test="$dvt_RowCount = 0">ms-viewEmpty</xsl:when>
                    <xsl:otherwise>
                      <xsl:choose>
                        <!-- Threaded/Flat view with non-zero number of items = class="ms-disc" -->
                        <xsl:when test="(@BaseViewID='1' or @BaseViewID='2')">ms-disc</xsl:when>
                        <!-- Subject view with non-zero number of items = class="ms-listviewtable" -->
                        <xsl:otherwise>ms-listviewtable</xsl:otherwise>
                      </xsl:choose>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:attribute>
                <xsl:apply-templates select="." mode="full" />
              </table>
            </td>
          </tr>
          <xsl:if test="$dvt_RowCount = 0">
            <tr>
              <td>
                <table width="100%" border="0" dir="{List/@Direction}">
                  <xsl:call-template name="EmptyTemplate" />
                </table>
              </td>
            </tr>
          </xsl:if>
        </table>
        <xsl:call-template name="pagingButtons" />
        <xsl:if test="Toolbar[@Type='Freeform'] or ($MasterVersion &gt;=4 and Toolbar[@Type='Standard'])">
          <xsl:call-template name="Freeform">
            <xsl:with-param name="AddNewText">
              <xsl:choose>
                <xsl:when test="List/@TemplateType='108'">
                  <xsl:value-of select="$Rows/@resource.wss.Add_New_Discussion"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$Rows/@resource.wss.addnewitem"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
            <xsl:with-param name="ID">
              <xsl:choose>
                <xsl:when test="List/@TemplateType='108'">idHomePageNewDiscussion</xsl:when>
                <xsl:otherwise>idHomePageNewItem</xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:if>
        <!--RemarQ we are at the end of the table that contains the view so get a link to our server generated javascript -->
        <xsl:if test="$Userid!='-1'">
          <script type="text/javascript" src="{$HttpVDir}/_layouts/15/BalsamicSolutions.ReadUnreadSiteColumn/Jslink.aspx?remarQId={$List}-{$View}-{$Userid}">
          </script>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="EmptyTemplate">
    <tr>
      <td class="ms-vb" colspan="99">
        <xsl:value-of select="$NoAnnouncements"/>
        <xsl:if test="$ListRight_AddListItems = '1'">
          <xsl:if test="not (BaseViewID='1' or @BaseViewID='2')">
            <xsl:text ddwrt:whitespace-preserve="yes" xml:space="preserve"> </xsl:text>
            <xsl:value-of select="$NoAnnouncementsHowTo"/>
          </xsl:if>
        </xsl:if>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>
