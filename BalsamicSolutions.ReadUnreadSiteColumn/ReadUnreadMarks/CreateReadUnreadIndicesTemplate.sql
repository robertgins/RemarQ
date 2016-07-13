--Note that we have used IGNORE_DUP_KEY = ON here so that we can simply do build inserts and let SQL drop the failing constraints
CREATE UNIQUE NONCLUSTERED INDEX [ReadUnread_LISTID_Idx] ON [dbo].[ReadUnread_LISTID_]
(
	[ItemId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = ON, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [ReadUnreadPath_LISTID_Idx] ON [dbo].[ReadUnread_LISTID_]
(
	[UserId] ASC,
	[Path] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE VIEW [dbo].[Hierarchy_LISTID_] WITH SCHEMABINDING
AS
	SELECT   [ItemId], [ReadOn],[Path], [Leaf]
	FROM     dbo.ReadUnread_LISTID_
	WHERE    (UserId = -1)
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
 	INSERT INTO [ReadUnread_LISTID_] ([ItemId],[UserId]) SELECT [Id],@userId FROM @itemIds
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

CREATE PROCEDURE rq_MarkItemsUnread_LISTID_ ( @itemIds dbo.ReadUnreadItemIds READONLY,@userId int)
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM [ReadUnread_LISTID_] WHERE [UserId]=@userId  AND  [ItemId] IN (SELECT Id FROM @itemIds)
END
GO

CREATE PROCEDURE rq_MarkListUnread_LISTID_ (@userId int)
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM [ReadUnread_LISTID_] WHERE [UserId]=@userId
END
GO


CREATE PROCEDURE rq_ResetItemsReadMark_LISTID_ (@itemIds dbo.ReadUnreadItemIds READONLY)
AS
DECLARE @versionId bigint
BEGIN
	BEGIN TRY
    BEGIN TRANSACTION   
	DELETE FROM [ReadUnread_LISTID_] WHERE [userId] != -1 AND [ItemId] IN (SELECT [Id] FROM @itemIds)
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
END
GO


CREATE PROCEDURE rq_AreAllPeerItemsRead_LISTID_ (@userId int,@itemId int, @referencePath nvarchar(256),@folderId int)
AS
BEGIN
	SELECT TOP 1 ItemId FROM [dbo].[Hierarchy_LISTID_] WHERE [dbo].[Hierarchy_LISTID_].[Path] =@referencePath 
			AND [dbo].[Hierarchy_LISTID_].[ItemId] !=@itemId AND [dbo].[Hierarchy_LISTID_].[ItemId] !=@folderId
			AND [dbo].[Hierarchy_LISTID_].[ItemId] NOT IN (SELECT ItemId FROM  [dbo].[ReadUnread_LISTID_] WHERE [dbo].[ReadUnread_LISTID_].[UserId] = @userId)
END
GO

CREATE PROCEDURE rq_AllUnreadItems_LISTID_ (@userId int)
AS
BEGIN
	SELECT ItemId FROM [dbo].[ReadUnread_LISTID_] WHERE [dbo].[ReadUnread_LISTID_].[ItemId] NOT IN (SELECT ItemId FROM  [dbo].[ReadUnread_LISTID_] WHERE [dbo].[ReadUnread_LISTID_].[UserId] = @userId)
END
GO

CREATE PROCEDURE rq_AllReaders_LISTID_ (@itemId int,@userId int)
AS
BEGIN
	SELECT [ReadOn],[UserId],0 FROM [dbo].[ReadUnread_LISTID_] WHERE [dbo].[ReadUnread_LISTID_].[ItemId] = @itemId 
END
GO


CREATE PROCEDURE rq_GetReadMarksFromAListForAUser_LISTID_ (@userId int,@itemId int,@folderId int)
AS
BEGIN
	SELECT [ItemId] FROM [dbo].[ReadUnread_LISTID_] WHERE [UserId]=@userId AND [ItemId] >= @itemId AND [ItemId] <= @folderId
END
GO

CREATE PROCEDURE rq_GetChildPathMap_LISTID_ (@itemId int,@folderId int)
AS
BEGIN
	SELECT [ItemId],[Path],[Leaf] FROM [dbo].[Hierarchy_LISTID_] WHERE [ItemId] >= @itemId AND [ItemId] <= @folderId
END
GO

CREATE PROCEDURE rq_UpdateItemPath_LISTID_ (@itemId int,@userId int, @referencePath nvarchar(256), @referencePath2 nvarchar(256))
AS
BEGIN
	SET NOCOUNT ON
	SET DEADLOCK_PRIORITY HIGH
	BEGIN TRY
    BEGIN TRANSACTION 
		IF EXISTS(SELECT [ItemId] FROM [ReadUnread_LISTID_] WHERE [ItemId]=@itemId AND [UserId]=@userId)
			BEGIN
				UPDATE [ReadUnread_LISTID_] SET [Path] =@referencePath,[Leaf]=@referencePath2 WHERE [ItemId]=@itemId AND [UserId]=@userId
			END
		ELSE
			BEGIN
				INSERT INTO [ReadUnread_LISTID_]([ItemId],[UserId],[Path],[Leaf]) VALUES(@itemId,@userId,@referencePath,@referencePath2)
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
