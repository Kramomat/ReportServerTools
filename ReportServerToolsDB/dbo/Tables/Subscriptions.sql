CREATE TABLE [dbo].[Subscriptions] (
    [SubscriptionID]    UNIQUEIDENTIFIER NOT NULL,
    [OwnerID]           UNIQUEIDENTIFIER NOT NULL,
    [Report_OID]        UNIQUEIDENTIFIER NOT NULL,
    [Locale]            NVARCHAR (128)   COLLATE Latin1_General_100_CI_AS_KS_WS NOT NULL,
    [InactiveFlags]     INT              NOT NULL,
    [ExtensionSettings] NTEXT            COLLATE Latin1_General_100_CI_AS_KS_WS NULL,
    [ModifiedByID]      UNIQUEIDENTIFIER NOT NULL,
    [ModifiedDate]      DATETIME         NOT NULL,
    [Description]       NVARCHAR (512)   COLLATE Latin1_General_100_CI_AS_KS_WS NULL,
    [LastStatus]        NVARCHAR (260)   COLLATE Latin1_General_100_CI_AS_KS_WS NULL,
    [EventType]         NVARCHAR (260)   COLLATE Latin1_General_100_CI_AS_KS_WS NOT NULL,
    [MatchData]         NTEXT            COLLATE Latin1_General_100_CI_AS_KS_WS NULL,
    [LastRunTime]       DATETIME         NULL,
    [Parameters]        NTEXT            COLLATE Latin1_General_100_CI_AS_KS_WS NULL,
    [DataSettings]      NTEXT            COLLATE Latin1_General_100_CI_AS_KS_WS NULL,
    [DeliveryExtension] NVARCHAR (260)   COLLATE Latin1_General_100_CI_AS_KS_WS NULL,
    [Version]           INT              NOT NULL,
    [ReportZone]        INT              NOT NULL,
    [IsDeleted]         BIT              NOT NULL,
    [DeletionDate]      DATETIME         NULL
);

