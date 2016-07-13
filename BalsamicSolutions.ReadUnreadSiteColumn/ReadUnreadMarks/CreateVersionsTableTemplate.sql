CREATE TABLE [dbo].[_TABLENAME_](
	[Version] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [date] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[_TABLENAME_] ADD  CONSTRAINT [DF___TABLENAME_CreatedOn]  DEFAULT (getutcdate()) FOR [CreatedOn]
GO
