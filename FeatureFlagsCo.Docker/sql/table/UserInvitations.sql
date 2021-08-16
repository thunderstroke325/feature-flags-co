USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[UserInvitations]
(
	[Id]        INT IDENTITY (1, 1) NOT NULL,
	[UserId]    NVARCHAR (450) NOT NULL,
	[AccountId] INT NOT NULL,
	[InitialPassword]   NVARCHAR (20) NULL,
	CONSTRAINT [PK_UserInvitations] PRIMARY KEY CLUSTERED ([Id] ASC)
)
