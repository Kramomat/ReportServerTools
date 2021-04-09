using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Net.Http;
using ReportServerTools.Properties;
using System.Net;
using System.Data;

namespace ReportServerTools
{
    public class ToolLibrary
    {
        /// <summary>
        /// Reads to be warmed reports from configuration table
        /// Opens http connections to Report Server and executes warming
        /// </summary>
        public async Task ExecuteCacheWarming()
        {
            HttpClientHandler handler = new HttpClientHandler();

            handler.Credentials = new NetworkCredential(Settings.Default.Benutzername, Settings.Default.Passwort);

            HttpClient client = new HttpClient(handler);

            SqlConnection sqlcon = new SqlConnection(Settings.Default.SQLConnectionString);
            SqlConnection sqlconLog = new SqlConnection(Settings.Default.SQLConnectionString);
            // Select Reports to be warmed
            SqlCommand sqlCommand = new SqlCommand("SELECT ItemID FROM CacheWarming WHERE [ActivateCacheWarming] = 1", sqlcon);
            // Insert Log Entry
            SqlCommand sqlCommandLogInsert = new SqlCommand(@"INSERT INTO CacheWarmingLog (ItemId, StartTime, Status)
                                                                OUTPUT INSERTED.CacheWarmingLogID
                                                                VALUES (@ItemId, getdate(), 0)", sqlconLog);
            sqlCommandLogInsert.Parameters.Add("@ItemId", SqlDbType.UniqueIdentifier);
            // Update Log Entry
            SqlCommand sqlCommandLogUpdate = new SqlCommand(@"UPDATE CacheWarmingLog
                                                              SET EndTime = getdate(), Status=@Status, Details=@Details, Duration=datediff(s,StartTime,getdate())
                                                              WHERE CacheWarmingLogID = @CacheWarmingLogID", sqlconLog);
            sqlCommandLogUpdate.Parameters.Add("@Status", SqlDbType.Int);
            sqlCommandLogUpdate.Parameters.Add("@Details", SqlDbType.VarChar, 4000);
            sqlCommandLogUpdate.Parameters["@Details"].IsNullable = true;
            sqlCommandLogUpdate.Parameters.Add("@CacheWarmingLogID", SqlDbType.Int);

            int logid;
            int noOfReports = 0;
            HttpResponseMessage response;
            Guid itemId;
            bool isError;

            sqlcon.Open();
            sqlconLog.Open();
            SqlDataReader sqlReaderReportList = sqlCommand.ExecuteReader();
            while (sqlReaderReportList.Read())
            {
                itemId = sqlReaderReportList.GetGuid(0);
                isError = false;

                //INSERT new Log Entry
                sqlCommandLogInsert.Parameters["@ItemId"].Value = itemId;
                logid = Int32.Parse(sqlCommandLogInsert.ExecuteScalar().ToString());

                //Execute Cache Warming Event Level 1
                try
                {
                    response = await client.GetAsync(String.Concat(Settings.Default.ReportServerURL, "powerbi/?id=", itemId.ToString()));
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    //UPDATE Log Set to Error
                    sqlCommandLogUpdate.Parameters["@Status"].Value = 9;
                    sqlCommandLogUpdate.Parameters["@Details"].Value = e.Message;
                    sqlCommandLogUpdate.Parameters["@CacheWarmingLogID"].Value = logid;
                    sqlCommandLogUpdate.ExecuteNonQuery();
                    isError = true;

                    Console.WriteLine("Exception Occured");
                    Console.WriteLine(e.Message);
                }


                if (!isError)
                {
                    //Execute Cache Warming Event Level 2
                    try
                    {

                        response = await client.GetAsync(String.Concat(Settings.Default.ReportServerURL, "powerbi/api/explore/reports/", itemId.ToString(), "/modelsAndExploration?preferReadOnlySession=true"));
                        response.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException e)
                    {
                        //UPDATE Log Set to Error
                        sqlCommandLogUpdate.Parameters["@Status"].Value = 19;
                        sqlCommandLogUpdate.Parameters["@Details"].Value = e.Message;
                        sqlCommandLogUpdate.Parameters["@CacheWarmingLogID"].Value = logid;
                        sqlCommandLogUpdate.ExecuteNonQuery();
                        isError = true;
                    }

                    // UPDATE Log Successful
                    sqlCommandLogUpdate.Parameters["@Status"].Value = 1;
                    sqlCommandLogUpdate.Parameters["@Details"].Value = DBNull.Value;
                    sqlCommandLogUpdate.Parameters["@CacheWarmingLogID"].Value = logid;
                    sqlCommandLogUpdate.ExecuteNonQuery();

                    noOfReports++;
                }
            }
            sqlReaderReportList.Close();
            sqlconLog.Close();
            sqlcon.Close();

            Console.WriteLine(String.Concat("Cache Warming successful of ",noOfReports," Reports."));

        }


        /// <summary>
        /// Copies and historizes the reporting Catalog
        /// </summary>
        public void CopyCatalog()
        {
            SqlConnection sqlcon = new SqlConnection(Settings.Default.SQLConnectionString);
            SqlCommand sqlCommand = new SqlCommand(String.Concat(@"MERGE ReportServerTools.dbo.CacheWarming AS tgt
USING 
  (SELECT [ItemID]
      ,[Path]
      ,[Name]
      ,[ParentID]
	  ,[ModifiedDate]
  FROM [", Settings.Default.ReportServerDBName, @"].[dbo].[Catalog]
  WHERE [Type] IN (2,13)) AS src
 ([ItemID],[Path],[Name],[ParentID],[ModifiedDate])
ON (tgt.ItemID = src.ItemID)
WHEN MATCHED AND tgt.[ModifiedDate] <> src.[ModifiedDate] THEN 
     UPDATE SET [Path] = src.[Path], [name]=src.[Name], ParentID = src.ParentID, [ModifiedDate]=src.[ModifiedDate]
WHEN NOT MATCHED BY TARGET THEN
     INSERT ([ItemID],[Path],[Name],[ParentID],[ModifiedDate])
	 VALUES (src.[ItemID],src.[Path],src.[Name],src.[ParentID],src.[ModifiedDate])
WHEN NOT MATCHED BY SOURCE THEN
     DELETE;"), sqlcon);


            SqlCommand sqlCommand2 = new SqlCommand(String.Concat(@"MERGE ReportServerTools.dbo.CatalogHistory AS tgt
USING 
  (SELECT [ItemID]
      ,[Path]
      ,[Name]
      ,[ParentID]
      ,[Type]
      ,[Content]
      ,[Intermediate]
      ,[SnapshotDataID]
      ,[LinkSourceID]
      ,[Property]
      ,[Description]
      ,[Hidden]
      ,[CreatedByID]
      ,[CreationDate]
      ,[ModifiedByID]
      ,[ModifiedDate]
      ,[MimeType]
      ,[SnapshotLimit]
      ,[Parameter]
      ,[PolicyID]
      ,[PolicyRoot]
      ,[ExecutionFlag]
      ,[ExecutionTime]
      ,[SubType]
      ,[ComponentID]
      ,[ContentSize]
  FROM [", Settings.Default.ReportServerDBName, @"].[dbo].[Catalog]
  WHERE [Type] IN (2,13)) AS src
 ([ItemID],[Path],[Name],[ParentID],[Type],[Content],[Intermediate],[SnapshotDataID],[LinkSourceID],[Property],[Description],[Hidden],[CreatedByID],[CreationDate],[ModifiedByID],[ModifiedDate],[MimeType],[SnapshotLimit],[Parameter],[PolicyID],[PolicyRoot],[ExecutionFlag],[ExecutionTime],[SubType],[ComponentID],[ContentSize])
ON (tgt.ItemID = src.ItemID)
WHEN MATCHED AND tgt.[ModifiedDate] <> src.[ModifiedDate] THEN 
     UPDATE SET [Path] = src.[Path], [name]=src.[Name], ParentID = src.ParentID, [Type] = src.[Type], [Content] = src.[Content], [Intermediate] = src.[Intermediate],  [SnapshotDataID] = src.[SnapshotDataID], [LinkSourceID] = src.[LinkSourceID], [Property] = src.[Property], [Description] = src.[Description], [Hidden] = src.[Hidden], [CreatedByID] = src.[CreatedByID], [CreationDate] = src.[CreationDate], [ModifiedByID] = src.[ModifiedByID], [ModifiedDate] = src.[ModifiedDate], [MimeType] = src.[MimeType], [SnapshotLimit] = src.[SnapshotLimit], [Parameter] = src.[Parameter], [PolicyID] = src.[PolicyID], [PolicyRoot] = src.[PolicyRoot], [ExecutionFlag] = src.[ExecutionFlag], [ExecutionTime] = src.[ExecutionTime], [SubType] = src.[SubType], [ComponentID] = src.[ComponentID], [ContentSize] = src.[ContentSize]
WHEN NOT MATCHED BY TARGET THEN
     INSERT ([ItemID],[Path],[Name],[ParentID],[Type],[Content],[Intermediate],[SnapshotDataID],[LinkSourceID],[Property],[Description],[Hidden],[CreatedByID],[CreationDate],[ModifiedByID],[ModifiedDate],[MimeType],[SnapshotLimit],[Parameter],[PolicyID],[PolicyRoot],[ExecutionFlag],[ExecutionTime],[SubType],[ComponentID],[ContentSize],[IsDeleted])
	 VALUES (src.[ItemID],src.[Path],src.[Name],src.[ParentID],src.[Type],src.[Content],src.[Intermediate],src.[SnapshotDataID],src.[LinkSourceID],src.[Property],src.[Description],src.[Hidden],src.[CreatedByID],src.[CreationDate],src.[ModifiedByID],src.[ModifiedDate],src.[MimeType],src.[SnapshotLimit],src.[Parameter],src.[PolicyID],src.[PolicyRoot],src.[ExecutionFlag],src.[ExecutionTime],src.[SubType],src.[ComponentID],src.[ContentSize],0)
WHEN NOT MATCHED BY SOURCE AND tgt.IsDeleted = 0 THEN
     UPDATE SET isDeleted = 1, DeletionDate = getdate();"), sqlcon);
            sqlcon.Open();
            sqlCommand.ExecuteNonQuery();
            Console.WriteLine("Cache Warming Catalog Updated");

            sqlCommand2.ExecuteNonQuery();
            Console.WriteLine("Reporting Catalog Updated");
            sqlcon.Close();

        }

        public void CopyExecutionLog()
        {
            SqlConnection sqlcon = new SqlConnection(Settings.Default.SQLConnectionString);
            SqlCommand sqlCommand = new SqlCommand(String.Concat(@"MERGE ReportServerTools.dbo.ExecutionLogStorage as tgt
    USING
	(SELECT [LogEntryId]
		  ,[InstanceName]
		  ,[ReportID]
		  ,[UserName]
		  ,[ExecutionId]
		  ,[RequestType]
		  ,[Format]
		  ,[Parameters]
		  ,[ReportAction]
		  ,[TimeStart]
		  ,[TimeEnd]
		  ,[TimeDataRetrieval]
		  ,[TimeProcessing]
		  ,[TimeRendering]
		  ,[Source]
		  ,[Status]
		  ,[ByteCount]
		  ,[RowCount]
		  ,[AdditionalInfo]
	  FROM [", Settings.Default.ReportServerDBName, @"].[dbo].[ExecutionLogStorage]) As src
	  ([LogEntryId],[InstanceName],[ReportID],[UserName],[ExecutionId],[RequestType],[Format],[Parameters],[ReportAction],[TimeStart],[TimeEnd],[TimeDataRetrieval],[TimeProcessing],[TimeRendering],[Source],[Status],[ByteCount],[RowCount],[AdditionalInfo])
ON (tgt.[LogEntryId] = src.[LogEntryId])
WHEN NOT MATCHED BY TARGET THEN
	INSERT ([LogEntryId],[InstanceName],[ReportID],[UserName],[ExecutionId],[RequestType],[Format],[Parameters],[ReportAction],[TimeStart],[TimeEnd],[TimeDataRetrieval],[TimeProcessing],[TimeRendering],[Source],[Status],[ByteCount],[RowCount],[AdditionalInfo])
	VALUES (src.[LogEntryId],src.[InstanceName],src.[ReportID],src.[UserName],src.[ExecutionId],src.[RequestType],src.[Format],src.[Parameters],src.[ReportAction],src.[TimeStart],src.[TimeEnd],src.[TimeDataRetrieval],src.[TimeProcessing],src.[TimeRendering],src.[Source],src.[Status],src.[ByteCount],[RowCount],src.[AdditionalInfo]);"), sqlcon);
            sqlcon.Open();
            sqlCommand.ExecuteNonQuery();
            sqlcon.Close();

            Console.WriteLine("Executionlog Copied");
        }

        /// <summary>
        /// Copies the Subscription History from ReportServerDB to ReportServerTools DB
        /// Also merges eventually changed entries
        /// </summary>
        public void CopySubscriptionLog()
        {
            SqlConnection sqlcon = new SqlConnection(Settings.Default.SQLConnectionString);
            
            //This command merges the Subscription History
            SqlCommand sqlCommand = new SqlCommand(String.Concat(@"MERGE ReportServerTools.dbo.[SubscriptionHistory] AS tgt
USING
(SELECT[SubscriptionHistoryID]
      ,[SubscriptionID]
      ,[Type]
      ,[StartTime]
      ,[EndTime]
      ,[Status]
      ,[Message]
      ,[Details]
 FROM [", Settings.Default.ReportServerDBName, @"].[dbo].[SubscriptionHistory]) As src
([SubscriptionHistoryID], [SubscriptionID], [Type], [StartTime], [EndTime], [Status], [Message], [Details])
 ON(src.[SubscriptionHistoryID] = tgt.[SubscriptionHistoryID])
 WHEN NOT MATCHED BY TARGET THEN

    INSERT([SubscriptionHistoryID],[SubscriptionID],[Type],[StartTime],[EndTime],[Status],[Message],[Details])
	VALUES(src.[SubscriptionHistoryID], src.[SubscriptionID], src.[Type], src.[StartTime], src.[EndTime], src.[Status], src.[Message], src.[Details]);"), sqlcon);

            // Since SubscriptionHistory only contains status and metadata, also Subscriptions itself need to be copied
            // After deletion a subscription, entries would be lost. They will be saved in ReportServerTools DB
            SqlCommand sqlCommand2 = new SqlCommand(String.Concat(@"MERGE ReportServerTools.dbo.[Subscriptions] AS tgt
USING
(SELECT[SubscriptionID]
      ,[OwnerID]
      ,[Report_OID]
      ,[Locale]
      ,[InactiveFlags]
      ,[ExtensionSettings]
      ,[ModifiedByID]
      ,[ModifiedDate]
      ,[Description]
      ,[LastStatus]
      ,[EventType]
      ,[MatchData]
      ,[LastRunTime]
      ,[Parameters]
      ,[DataSettings]
      ,[DeliveryExtension]
      ,[Version]
      ,[ReportZone]
  FROM [", Settings.Default.ReportServerDBName, @"].[dbo].[Subscriptions]) As src
 ([SubscriptionID], [OwnerID], [Report_OID], [Locale], [InactiveFlags], [ExtensionSettings], [ModifiedByID], [ModifiedDate], [Description], [LastStatus], [EventType], [MatchData], [LastRunTime], [Parameters], [DataSettings], [DeliveryExtension], [Version], [ReportZone])
  ON src.[SubscriptionID] = tgt.[SubscriptionID]
   WHEN NOT MATCHED BY TARGET THEN
    INSERT([SubscriptionID],[OwnerID],[Report_OID],[Locale],[InactiveFlags],[ExtensionSettings],[ModifiedByID],[ModifiedDate],[Description],[LastStatus],[EventType],[MatchData],[LastRunTime],[Parameters],[DataSettings],[DeliveryExtension],[Version],[ReportZone],IsDeleted)
	VALUES(src.[SubscriptionID], src.[OwnerID], src.[Report_OID], src.[Locale], src.[InactiveFlags], src.[ExtensionSettings], src.[ModifiedByID], src.[ModifiedDate], src.[Description], src.[LastStatus], src.[EventType], src.[MatchData], src.[LastRunTime], src.[Parameters], src.[DataSettings], src.[DeliveryExtension], src.[Version], src.[ReportZone], 0)
WHEN MATCHED AND tgt.[ModifiedDate] <> src.[ModifiedDate] THEN
     UPDATE SET[SubscriptionID] = src.[SubscriptionID], [OwnerID]= src.[OwnerID],[Report_OID]= src.[Report_OID],[Locale]= src.[Locale],[InactiveFlags]= src.[InactiveFlags],[ExtensionSettings]= src.[ExtensionSettings],[ModifiedByID]= src.[ModifiedByID],[ModifiedDate]= src.[ModifiedDate],[Description]= src.[Description],[LastStatus]= src.[LastStatus],[EventType]= src.[EventType],[MatchData]= src.[MatchData],[LastRunTime]= src.[LastRunTime],[Parameters]= src.[Parameters],[DataSettings]= src.[DataSettings],[DeliveryExtension]= src.[DeliveryExtension],[Version]= src.[Version],[ReportZone]= src.[ReportZone]
WHEN NOT MATCHED BY SOURCE AND tgt.IsDeleted = 0 THEN
     UPDATE SET isDeleted = 1, DeletionDate = getdate();", sqlcon));


            sqlcon.Open();
            sqlCommand.ExecuteNonQuery();
            sqlCommand2.ExecuteNonQuery();
            sqlcon.Close();

            Console.WriteLine("Subscriptionlog Copied");

        }
    }
}
