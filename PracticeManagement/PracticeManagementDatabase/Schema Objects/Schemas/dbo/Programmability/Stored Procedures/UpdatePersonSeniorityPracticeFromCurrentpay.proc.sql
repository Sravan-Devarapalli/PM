﻿CREATE PROCEDURE dbo.UpdatePersonSeniorityPracticeFromCurrentPay
AS
BEGIN
	DECLARE @Today DATETIME,@FutureDate DATETIME
	SELECT @Today = CONVERT(DATETIME,CONVERT(DATE,[dbo].[GettingPMTime](GETDATE()))),@FutureDate = dbo.GetFutureDate()

	UPDATE P
	SET P.SeniorityId = Pa.SeniorityId,
		P.DefaultPractice = Pa.PracticeId
	FROM dbo.Person P
	JOIN dbo.Pay Pa
	ON P.PersonId = Pa.Person AND 
		pa.StartDate <= @Today AND ISNULL(EndDate,@FutureDate) > @Today
		AND (P.SeniorityId <> Pa.SeniorityId OR P.DefaultPractice <> Pa.PracticeId)

	UPDATE dbo.Pay
	SET IsActivePay = CASE WHEN StartDate <= @Today AND  EndDate > @Today
							   THEN 1 ELSE 0 END
	WHERE IsActivePay <> (CASE WHEN StartDate <= @Today AND  EndDate > @Today
							   THEN 1 ELSE 0 END)
END

