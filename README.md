# ReportServerTools
Tools for helping administer and work with your Report Server

ReportServerTools consists of a SQL Server Database and a command line utility based on .Net Framework 4.8

##Paramters
Following Parameters are supported

/r Reporting Catalog: Make a copy of ReportingCatalog table
The reporting catalog contains all the metadata of the currently saved reports. This should be the first step when using the ReportServerTools, since everything is related to a ItemID in Catalog table

/c Cache Warming
For all reports referenced in CacheWarming table having ActivateCacheWarming set to 1, an http request will be send to Report Server, so reports will be executed and data loaded into memory. 
Currently only works for Power BI Reports on Power BI Report Server, support paginated reports will be added later

/e Execution Log
Copies the execution to ReportServerTools DB, so analysis of Report Usage can be done, even when ExecutionLog has been cleared or reports have been deleted

/s Subscirption Log
Copies the subscription list and subscription log  ReportServerTools DB, so analysis of Subscription execution can be done, even when ExecutionLog has been cleared or subscriptions have been deleted

## Setup
1. Edit app.config file, ReportServerTools require this information:
   - URL your report server can be reached (just base url like http://reportserver01/)
     This is only required for cache warming
   - A username and a password, which can be used to access the report server and the according reports via http, if you like to use cache warming
   - A connectionstring to reach your ReportServerTools database. 
   - The name of your ReportServer DB. ReportServerTools DB and ReportServer DB need to reside on the same SQL Server 
2. Create ReportServerTools database using dacpac file
3. Automate the execution of ReportServerTools
   If you like to do this via SQL Server Agent, you need to create a credential and a CmdExec proxy first, of a user which has sysadmin rights and local rights to executed the ReportServerTools.exe file
   Then create a job with several steps and the parameters you like to use.   

## Backlog
- Support of paginated reports for cache warming
- SSIS implementation insteadt of .exe file (more secure)
- Testing Reporting Services, currently only tested against Power BI Report Server
- Provide code for sample SQL Server Agent job using CmdExec
