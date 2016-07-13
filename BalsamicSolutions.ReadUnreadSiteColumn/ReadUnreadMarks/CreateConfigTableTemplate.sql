CREATE TABLE [dbo].[_TABLENAME_](
	[ListId] [uniqueidentifier] NOT NULL,
	[WebId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[FieldId] [uniqueidentifier] NOT NULL,
	[PublicName] [nvarchar](50) NOT NULL,
	[CultureName] [nvarchar](50) NOT NULL,
	[ColumnRenderMode] [int] NOT NULL,
	[UnreadImageUrl] [nvarchar](512) NOT NULL,
	[ReadImageUrl] [nvarchar](512) NOT NULL,
	[UnreadhHtmlColor] [nvarchar](50) NOT NULL,
	[ThreadUnReadHtmlColor][nvarchar](50) NOT NULL,
	[LayoutsUrl] [nvarchar](512) NOT NULL,
	[ShowEditingTools] [int] NOT NULL,
	[VersionUpdateFlags] [int] NOT NULL,
	[RefreshInterval] [int] NOT NULL,
	[ConfigOne] [int] NOT NULL,
	[ConfigTwo] [int] NOT NULL,
	[ConfigThree] [int] NOT NULL
 CONSTRAINT [PK__TABLENAME_] PRIMARY KEY CLUSTERED 
(
	[ListId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE PROCEDURE UpdateListConfiguration(@Listid uniqueidentifier,@WebId uniqueidentifier,@SiteId uniqueidentifier,@FieldId uniqueidentifier, @PublicName nvarchar(50),
										 @CultureName nvarchar(50),@ColumnRenderMode int,@UnreadImageUrl nvarchar(512),@ReadImageUrl nvarchar(512),
										 @UnreadhHtmlColor nvarchar(50),@ThreadUnReadHtmlColor nvarchar(50),@ShowEditingTools int,@LayoutsUrl nvarchar(512),
										 @VersionUpdateFlags int,@RefreshInterval int)
AS
BEGIN
	BEGIN TRY
	BEGIN TRANSACTION  
	UPDATE [_TABLENAME_] SET [WebId]=@WebId,[SiteId]=@SiteId,
							 [FieldId]=@FieldId,[PublicName]=@PublicName,
							 [CultureName]=@CultureName,[ColumnRenderMode]=@ColumnRenderMode,
							 [UnreadImageUrl]=@UnreadImageUrl,[ReadImageUrl]=@ReadImageUrl,
							 [UnreadhHtmlColor]=@UnreadhHtmlColor,[ThreadUnReadHtmlColor]=@ThreadUnReadHtmlColor,
							 [ShowEditingTools]=@ShowEditingTools,[LayoutsUrl]=@LayoutsUrl,
							 [VersionUpdateFlags]=@VersionUpdateFlags,
							 [RefreshInterval]=@RefreshInterval
						WHERE [ListId] = @ListId
	COMMIT TRANSACTION  
	END TRY
	BEGIN CATCH
    IF (XACT_STATE()) = -1
    BEGIN
        ROLLBACK TRANSACTION;
    END
    IF (XACT_STATE()) = 1
    BEGIN
        COMMIT TRANSACTION   
    END
	END CATCH
END
GO

CREATE PROCEDURE CreateListConfiguration(@Listid uniqueidentifier,@WebId uniqueidentifier,@SiteId uniqueidentifier,@FieldId uniqueidentifier, @PublicName nvarchar(50),
										 @CultureName nvarchar(50),@ColumnRenderMode int,@UnreadImageUrl nvarchar(512),@ReadImageUrl nvarchar(512),
										 @UnreadhHtmlColor nvarchar(50),@ThreadUnReadHtmlColor nvarchar(50),@ShowEditingTools int,@LayoutsUrl nvarchar(512),
										 @VersionUpdateFlags int,@RefreshInterval int)
AS
BEGIN
	BEGIN TRY
	BEGIN TRANSACTION  
	DELETE FROM [_TABLENAME_] WHERE [ListId]=@ListId

	INSERT INTO [_TABLENAME_]   ([ListId],[WebId],[SiteId],[FieldId],[PublicName],[CultureName],[ColumnRenderMode],[UnreadImageUrl],[ReadImageUrl],[UnreadhHtmlColor],[ThreadUnReadHtmlColor],[ShowEditingTools],[LayoutsUrl],[VersionUpdateFlags],[RefreshInterval],[ConfigOne],[ConfigTwo],[ConfigThree])
								Values(@ListId,@WebId,@SiteId,@FieldId,@PublicName,@CultureName,@ColumnRenderMode,@UnreadImageUrl,@ReadImageUrl,@UnreadhHtmlColor,@ThreadUnReadHtmlColor,@ShowEditingTools,@LayoutsUrl,@VersionUpdateFlags,0,0,0,0)
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

CREATE PROCEDURE GetListConfiguration (@Listid uniqueidentifier)
AS
BEGIN
	SELECT * FROM [_TABLENAME_] WHERE [ListId]=@ListId
END
GO

CREATE PROCEDURE GetListConfigurations  
AS
BEGIN
	SELECT * FROM [_TABLENAME_]  
END
GO