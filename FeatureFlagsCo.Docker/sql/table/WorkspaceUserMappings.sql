USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[WorkspaceUserMappings] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [UserId]      NVARCHAR (128) NULL,
    [WorkspaceId] INT            NULL,
    [Role]        NVARCHAR (128) NULL,
    CONSTRAINT [PK_WorkspaceUserMappings] PRIMARY KEY CLUSTERED ([Id] ASC)
);

