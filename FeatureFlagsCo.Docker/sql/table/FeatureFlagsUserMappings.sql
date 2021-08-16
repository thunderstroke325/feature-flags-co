USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[FeatureFlagsUserMappings]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [WorkspaceId] INT NULL, 
    [FeatureFlagId] INT NULL, 
    [LastUpdatedTime] DATETIME2 NULL, 
    [RedisFeatureFlagUserKey] NVARCHAR(MAX) NULL, 
    [ResultValue] BIT NULL
)
