﻿











CREATE view  [dbo].[UnionLogs]
as
SELECT [Date]
      ,[Logger] as loggerFrmName
      ,[Thread] as ctrlNameThread
      ,[Level] as EventLevel
      ,[Message] as MessageValue
      ,[Exception] as ExceptionUser
 from [SPM_Database].[dbo].[Log]

