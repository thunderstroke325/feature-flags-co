USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[Workspaces] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (MAX) NULL,
    [CreatorUserId] NVARCHAR (128) NULL,
    [Description]   NVARCHAR (MAX) NULL,
    [Secret]        NVARCHAR (MAX) NULL,
    [MobileSecret]  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Workspaces] PRIMARY KEY CLUSTERED ([Id] ASC)
);

