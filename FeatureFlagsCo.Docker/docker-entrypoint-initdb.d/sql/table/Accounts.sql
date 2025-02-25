﻿USE FeatureFlagsCo;
GO

CREATE TABLE [dbo].[Accounts]
(
	[Id] INT            IDENTITY (1, 1) NOT NULL,
	[OrganizationName]          NVARCHAR (MAX) NULL,
    [CreatedAt]                 DATETIME2(7) NULL,
    [UpdatedAt]                 DATETIME2(7) NULL,
	CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED ([Id] ASC)
)
