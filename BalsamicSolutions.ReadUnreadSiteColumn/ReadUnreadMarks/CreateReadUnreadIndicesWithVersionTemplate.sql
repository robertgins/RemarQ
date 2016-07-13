
--Note that we have used IGNORE_DUP_KEY = ON here so that we can simply do build inserts and let SQL drop the failing constraints
CREATE UNIQUE NONCLUSTERED INDEX [ReadUnread_LISTID_Idx] ON [dbo].[ReadUnread_LISTID_]
(
	[ItemId] ASC,
	[Version] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = ON, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [ReadUnreadPath_LISTID_Idx] ON [dbo].[ReadUnread_LISTID_]
(
	[UserId] ASC,
	[Path] ASC,
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE VIEW [dbo].[Hierarchy_LISTID_] WITH SCHEMABINDING
AS
	SELECT   [ItemId], [ReadOn],[Version],[Path], [Leaf]
	FROM     dbo.ReadUnread_LISTID_
	WHERE    ([UserId] =-1 AND [Version]=0)
GO

CREATE UNIQUE CLUSTERED INDEX [Hierarchy_LISTID_Idx] ON [dbo].[Hierarchy_LISTID_]
(
	[ItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [HierarchyPath_LISTID_Idx] ON [dbo].[Hierarchy_LISTID_]
(
	[Path] ASC,
	[Leaf] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


CREATE PROCEDURE rq_MarkItemsRead_LISTID_ (@itemIds dbo.ReadUnreadItemIds READONLY,@userId int)
AS
BEGIN
	SET NOCOUNT ON
	SET DEADLOCK_PRIORITY HIGH
	BEGIN TRY
    BEGIN TRANSACTION  
 	INSERT INTO [ReadUnread_LISTID_] ([ItemId],[UserId],[Version]) SELECT [Id],@userId,0 FROM @itemIds
	COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		IF (XACT_STATE()) = -1
		BEGIN
			ROLLBACK TRANSACTION
		END;
		IF (XACT_STATE()) = 1
		BEGIN
			COMMIT TRANSACTION   
		END
	END CATCH 
	SET DEADLOCK_PRIORITY NORMAL
END
GO


CREATE PROCEDURE rq_MarkItemsUnread_LISTID_ ( @itemIds dbo.ReadUnreadItemIds READONLY,@userId int)
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM [ReadUnread_LISTID_] WHERE [UserId]=@userId AND [Version]=0 AND  [ItemId] IN (SELECT Id FROM @itemIds)
END
GO

CREATE PROCEDURE rq_MarkListUnread_LISTID_ (@userId int)
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM [ReadUnread_LISTID_] WHERE [UserId]=@userId AND [Version]=0  
END
GO

CREATE PROCEDURE rq_ResetItemsReadMark_LISTID_ (@itemIds dbo.ReadUnreadItemIds READONLY)
AS
DECLARE @versionId bigint
BEGIN
	BEGIN TRY
    BEGIN TRANSACTION 
	INSERT INTO [dbo].[RemarQ.Versions] DEFAULT VALUES SELECT  @versionId= SCOPE_IDENTITY()
	UPDATE [ReadUnread_LISTID_] SET [Version]= @versionId WHERE [Version]=0  AND [userId] != -1 AND [ItemId] IN (SELECT [Id] FROM @itemIds)
	--Create new version Records using -2 as the user id  
	INSERT INTO [ReadUnread_LISTID_] ([ItemId],[UserId],[Version]) SELECT [Id],-2, @versionId FROM @itemIds
	COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		IF (XACT_STATE()) = -1
		BEGIN
			ROLLBACK TRANSACTION
		END;
		IF (XACT_STATE()) = 1
		BEGIN
			COMMIT TRANSACTION   
		END
	END CATCH
END
GO


CREATE PROCEDURE rq_AreAllPeerItemsRead_LISTID_ (@userId int,@itemId int, @referencePath nvarchar(256),@folderId int)
AS
BEGIN
	SELECT TOP 1 ItemId FROM [dbo].[Hierarchy_LISTID_] WHERE [dbo].[Hierarchy_LISTID_].[Path] =@referencePath 
			AND [dbo].[Hierarchy_LISTID_].[ItemId] !=@itemId AND [dbo].[Hierarchy_LISTID_].[ItemId] !=@folderId
			AND [dbo].[Hierarchy_LISTID_].[ItemId] NOT IN (SELECT ItemId FROM  [dbo].[ReadUnread_LISTID_] WHERE [dbo].[ReadUnread_LISTID_].[UserId] = @userId AND [dbo].[ReadUnread_LISTID_].Version=0)
END
GO

CREATE PROCEDURE rq_AllUnreadItems_LISTID_ (@userId int)
AS
BEGIN
	SELECT ItemId FROM [dbo].[ReadUnread_LISTID_] WHERE [dbo].[ReadUnread_LISTID_].[ItemId] NOT IN (SELECT ItemId FROM  [dbo].[ReadUnread_LISTID_] WHERE [dbo].[ReadUnread_LISTID_].[UserId] = @userId  AND [dbo].[ReadUnread_LISTID_].Version=0)
END
GO

CREATE PROCEDURE rq_AllReaders_LISTID_ (@itemId int)
AS
BEGIN
	SELECT [ReadOn],[UserId],[Version] FROM [dbo].[ReadUnread_LISTID_] WHERE [dbo].[ReadUnread_LISTID_].[ItemId] = @itemId 
END
GO

CREATE PROCEDURE rq_GetReadMarksFromAListForAUser_LISTID_ (@userId int,@itemId int,@folderId int)
AS
BEGIN
	SELECT [ItemId] FROM [dbo].[ReadUnread_LISTID_] WHERE [UserId]=@userId AND [ItemId] >= @itemId AND [ItemId] <= @folderId AND [Version]=0
END
GO


CREATE PROCEDURE rq_GetChildPathMap_LISTID_ (@itemId int,@folderId int)
AS
BEGIN
	SELECT [ItemId],[Path],[Leaf] FROM [dbo].[Hierarchy_LISTID_] WHERE [ItemId] >= @itemId AND [ItemId] <= @folderId AND [Version]=0
END
GO

CREATE PROCEDURE rq_UpdateItemPath_LISTID_ (@itemId int,@userId int, @referencePath nvarchar(256), @referencePath2 nvarchar(256))
AS
BEGIN
	SET NOCOUNT ON
	SET DEADLOCK_PRIORITY HIGH
	BEGIN TRY
    BEGIN TRANSACTION
		IF EXISTS(SELECT [ItemId] FROM [ReadUnread_LISTID_] WHERE [ItemId]=@itemId AND [UserId]=@userId AND [Version]=0)
			BEGIN
				UPDATE [ReadUnread_LISTID_] SET [Path] =@referencePath,[Leaf]=@referencePath2 WHERE [ItemId]=@itemId AND [UserId]=@userId AND [Version]=0
			END
		ELSE
			BEGIN
				INSERT INTO [ReadUnread_LISTID_]([ItemId],[UserId],[Path],[Leaf],[Version]) VALUES(@itemId,@userId,@referencePath,@referencePath2,0)
			END
	COMMIT TRANSACTION
	END TRY
	BEGIN CATCH
		IF (XACT_STATE()) = -1
		BEGIN
			ROLLBACK TRANSACTION
		END
		IF (XACT_STATE()) = 1
		BEGIN
			COMMIT TRANSACTION
		END
	END CATCH
	SET DEADLOCK_PRIORITY NORMAL 
END
GO