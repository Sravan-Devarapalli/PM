CREATE TABLE [dbo].[MarginExceptionResponse]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [RequestId] INT NOT NULL, 
    [Status] INT NOT NULL, 
    [ResponseDate] DATETIME NOT NULL
)

