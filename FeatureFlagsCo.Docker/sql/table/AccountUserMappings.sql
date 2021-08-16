USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[AccountUserMappings]
(
	[Id]        INT IDENTITY (1, 1) NOT NULL,
	[AccountId] INT NOT NULL,
	[UserId]    NVARCHAR (450) NOT NULL,
	[Role]      NVARCHAR (128) NOT NULL,
	[InvitorUserId] NCHAR(450) NULL, 
    CONSTRAINT [PK_AccountUserMappings] PRIMARY KEY CLUSTERED ([Id] ASC)
)
