 CREATE TABLE [dbo].[_TABLENAME_](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[WebId] [uniqueidentifier] NOT NULL,
	[ListId] [uniqueidentifier] NOT NULL,
	[QueueCommand] [int] NOT NULL,
	[Batch] [nvarchar](50) NOT NULL,
	[PercentComplete] [int]  NULL,
	[LockName] [nvarchar](50) NULL,
	[LastUpdate] [datetime] NULL
 CONSTRAINT [PK__TABLENAME_] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 
GO

CREATE NONCLUSTERED INDEX [Command__TABLENAME__Idx] ON [dbo].[_TABLENAME_]
(
	[SiteId] ASC,
	[Batch] ASC,
	[QueueCommand] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
