CREATE TABLE [dbo].[ReadUnread_LISTID_](
	[ItemId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[ReadOn] [datetime2](7) NULL,
	[Path] [nvarchar](256) NULL,
	[Leaf] [nvarchar](128) NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].ReadUnread_LISTID_ ADD  CONSTRAINT [DF_ReadUnread_LISTID__ReadOn]  DEFAULT (getutcdate()) FOR [ReadOn]
GO
