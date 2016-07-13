IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_MarkItemsRead_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_MarkItemsRead_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_MarkItemsUnread_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_MarkItemsUnread_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_ResetItemsReadMark_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_ResetItemsReadMark_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_AreAllPeerItemsRead_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_AreAllPeerItemsRead_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_AllUnreadItems_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_AllUnreadItems_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_AllReaders_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_AllReaders_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_GetReadMarksFromAListForAUser_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_GetReadMarksFromAListForAUser_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_GetChildPathMap_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_GetChildPathMap_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_UpdateItemPath_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_UpdateItemPath_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='rq_MarkListUnread_LISTID_') BEGIN
	DROP PROCEDURE [dbo].[rq_MarkListUnread_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sysobjects WHERE name='Hierarchy_LISTID_') BEGIN
	DROP VIEW [dbo].[Hierarchy_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sys.indexes WHERE name='ReadUnread_LISTID_Idx') BEGIN
	DROP INDEX  [ReadUnread_LISTID_Idx] ON [dbo].[ReadUnread_LISTID_]
END
GO
IF  EXISTS (SELECT 1 FROM sys.indexes WHERE name='ReadUnreadPath_LISTID_Idx') BEGIN
	DROP INDEX  [ReadUnreadPath_LISTID_Idx] ON [dbo].[ReadUnread_LISTID_]
END
GO
