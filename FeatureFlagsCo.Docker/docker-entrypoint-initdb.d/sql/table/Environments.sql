USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[Environments]
(
	[Id]            INT IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (MAX) NOT NULL,
    [ProjectId]     INT NOT NULL,
    [Description]   NVARCHAR (MAX) NULL,
    [Secret]        NVARCHAR (MAX) NULL,
    [MobileSecret]  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Environments] PRIMARY KEY CLUSTERED ([Id] ASC)
)
