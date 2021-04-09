# ReportServerTools
Tools for helping administer and work with your Report Server

ReportServerTools consists of a SQL Server Database and a command line utility based on .Net Framework 4.8
Following Parameters are supported:

/r Reporting Catalog: Make a copy of ReportingCatalog table
The reporting catalog contains all the metadata of the currently saved reports. This should be the first step when using the ReportServerTools, since everything is related to a ItemID in Catalog table

/c Cache Warming
For all reports referenced in CacheWarming table having ActivateCacheWarming set to 1, an http request will be send to Report Server, so reports will be executed and data loaded into memory. 
Currently only works for Power BI Reports on Power BI Report Server, support paginated reports will be added later

/e Execution Log
Copies the execution to ReportServerTools DB, so analysis of Report Usage can be done, even when ExecutionLog has been cleared or reports have been deleted

/s Subscirption Log
Copies the subscription list and subscription log  ReportServerTools DB, so analysis of Subscription execution can be done, even when ExecutionLog has been cleared or subscriptions have been deleted

