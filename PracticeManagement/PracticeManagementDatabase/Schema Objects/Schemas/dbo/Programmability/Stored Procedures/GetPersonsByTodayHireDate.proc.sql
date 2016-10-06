﻿CREATE PROCEDURE [dbo].[GetPersonsByTodayHireDate]	
AS
BEGIN
	DECLARE @Today DATETIME
	SELECT @Today = CONVERT(DATETIME,CONVERT(DATE,[dbo].[GettingPMTime](GETUTCDATE())))
	--unlock out
	UPDATE m
	SET m.IsLockedOut = 0,
	    m.Password = ''
	FROM   dbo.aspnet_Users AS u
	INNER JOIN dbo.aspnet_Applications AS a ON u.ApplicationId = a.ApplicationId
	INNER JOIN  dbo.aspnet_Membership AS m ON u.UserId = m.UserId
	INNER JOIN Person as P ON u.LoweredUserName = LOWER(P.Alias) 
	WHERE P.PersonStatusId IN (1,5) AND HireDate = (@Today) AND P.IsWelcomeEmailSent = 0 AND a.LoweredApplicationName =LOWER('PracticeManagement')
	
	DECLARE @Personids NVARCHAR(500) = 'From SPROC GetPersonsByTodayHireDate Return PersonIds :'
	
	SELECT @Personids  = ISNULL(@Personids ,'From SPROC GetPersonsByTodayHireDate Return PersonIds :') + CONVERT(NVARCHAR,P.PersonId) + ',' 
	FROM  Person as P 
	WHERE P.PersonStatusId IN (1,5) AND HireDate = (@Today) AND P.IsWelcomeEmailSent = 0
	
	INSERT INTO [dbo].[SchedularLog]
       ([LastRun]
       ,[Status]
       ,[Comments]
       ,[NextRun]
       )
	SELECT @Today,'Success' as [Status],@Personids as [Comments],@Today+1 as [NextRun]


	SELECT P.PersonId,
	       p.FirstName,
		   p.Alias
	FROM  Person as P 
	WHERE P.PersonStatusId IN (1,5) AND HireDate = (@Today) AND P.IsWelcomeEmailSent = 0

END

