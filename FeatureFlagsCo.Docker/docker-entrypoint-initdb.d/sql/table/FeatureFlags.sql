USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[FeatureFlags] (
    [Id]                             INT            IDENTITY (1, 1) NOT NULL,
    [Name]                           NVARCHAR (512) NOT NULL,
    [WorkspaceId]                    INT            NOT NULL,
    [CreatorUserId]                  NVARCHAR (128) NULL,
    [Status]                         NVARCHAR (128) NULL,
    [DefaultRuleValue]               BIT            NULL,
    [PercentageRolloutForTrue]       FLOAT (53)     NULL,
    [PercentageRolloutForFalse]      FLOAT (53)     NULL,
    [PercentageRolloutBasedProperty] NVARCHAR (256) NULL,
    [KeyName]                        NVARCHAR (512) NULL,
    [ValueWhenDisabled] BIT NULL, 
    [LastUpdatedTime] DATETIME2 NULL, 
    CONSTRAINT [PK_FeatureFlags] PRIMARY KEY CLUSTERED ([Id] ASC)
);

