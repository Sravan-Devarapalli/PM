﻿CREATE TABLE [dbo].[Timescale] (
    [TimescaleId]  INT          NOT NULL,
    [Name]         VARCHAR (50) NOT NULL,
    [DefaultTerms] INT          NULL,
	IsContractType    BIT       NOT NULL,
	TimescaleCode	VARCHAR(10) NULL
);


