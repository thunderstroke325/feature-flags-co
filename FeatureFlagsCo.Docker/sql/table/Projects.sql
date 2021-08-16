USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[Projects]
(
	[Id]        INT IDENTITY (1, 1) NOT NULL,
	[AccountId] INT NOT NULL,
	[Name]      NVARCHAR (MAX) NULL,
	CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED ([Id] ASC)
)
