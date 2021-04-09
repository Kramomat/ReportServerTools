CREATE TABLE [dbo].[CacheWarmingLog] (
    [CacheWarmingLogID] INT              IDENTITY (1, 1) NOT NULL,
    [ItemID]            UNIQUEIDENTIFIER NOT NULL,
    [StartTime]         DATETIME         NOT NULL,
    [EndTime]           DATETIME         NULL,
    [Status]            TINYINT          NOT NULL,
    [Duration]          SMALLINT         NULL,
    [Details]           VARCHAR (4000)   NULL,
    CONSTRAINT [PK_CacheWarmingLog] PRIMARY KEY CLUSTERED ([CacheWarmingLogID] ASC)
);

