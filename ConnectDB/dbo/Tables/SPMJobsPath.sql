﻿CREATE TABLE [dbo].[SPMJobsPath] (
    [JobNo] VARCHAR (50)   NOT NULL,
    [BOMNo] VARCHAR (50)   NULL,
    [Path]  NVARCHAR (500) NULL,
    CONSTRAINT [PK_SPMJobsPath] PRIMARY KEY CLUSTERED ([JobNo] ASC)
);



