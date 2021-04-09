CREATE TABLE [dbo].[CacheWarming] (
    [ItemID]               UNIQUEIDENTIFIER NOT NULL,
    [Path]                 NVARCHAR (425)   COLLATE Latin1_General_100_CI_AS_KS_WS NOT NULL,
    [Name]                 NVARCHAR (425)   COLLATE Latin1_General_100_CI_AS_KS_WS NOT NULL,
    [ParentID]             UNIQUEIDENTIFIER NULL,
    [ModifiedDate]         DATETIME         NOT NULL,
    [ActivateCacheWarming] BIT              CONSTRAINT [DF_CacheWarming_ActivateCacheWarming] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_CacheWarming] PRIMARY KEY CLUSTERED ([ItemID] ASC)
);

