CREATE TABLE [dbo].[SubscriptionHistory] (
    [SubscriptionHistoryID] BIGINT           NOT NULL,
    [SubscriptionID]        UNIQUEIDENTIFIER NOT NULL,
    [Type]                  TINYINT          NULL,
    [StartTime]             DATETIME         NULL,
    [EndTime]               DATETIME         NULL,
    [Status]                TINYINT          NULL,
    [Message]               NVARCHAR (1500)  COLLATE Latin1_General_100_CI_AS_KS_WS NULL,
    [Details]               NVARCHAR (4000)  COLLATE Latin1_General_100_CI_AS_KS_WS NULL,
    CONSTRAINT [PK_SubscriptionHistory] PRIMARY KEY CLUSTERED ([SubscriptionHistoryID] ASC)
);

