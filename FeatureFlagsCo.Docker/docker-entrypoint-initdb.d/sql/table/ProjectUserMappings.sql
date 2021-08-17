USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[ProjectUserMappings]
(
	[Id]        INT IDENTITY (1, 1) NOT NULL,
	[ProjectId] INT NOT NULL,
	[UserId]    NVARCHAR (450),
	[Role]      NVARCHAR (128) NOT NULL,
	CONSTRAINT [PK_ProjectUserMappings] PRIMARY KEY CLUSTERED ([Id] ASC)
)
