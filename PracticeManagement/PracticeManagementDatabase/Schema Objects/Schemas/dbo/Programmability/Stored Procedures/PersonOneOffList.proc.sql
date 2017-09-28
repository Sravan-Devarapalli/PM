﻿-- =============================================
-- Author:		
-- Create date:
-- Last Updated by:	Shwetha
-- Last Update date: 10-07-2012
-- Read All persons firstname and last name  except having inactive status and must have compensation for today or in future.
-- =============================================
CREATE PROCEDURE dbo.PersonOneOffList
(
	@DateToday   DATETIME
)
AS
BEGIN

	SET NOCOUNT ON;

	 DECLARE @FutureDate DATETIME    
    SET @FutureDate = dbo.GetFutureDate()

	SELECT p.PersonId,
	       p.FirstName,
	       p.LastName,
		   p.TitleId,
		   T.Title
	  FROM dbo.Person AS p
      LEFT  JOIN dbo.Practice AS pr ON p.DefaultPractice = pr.PracticeId	 
	  LEFT JOIN dbo.Title AS T ON t.TitleId = P.TitleId
	  WHERE p.PersonStatusId in (1,3,5,6) AND ISNULL(pr.IsCompanyInternal, 0) = 0 -- adding Right to Present
		   AND EXISTS (SELECT 1 FROM dbo.Pay y
						WHERE p.PersonId = y.Person 
						AND  (@DateToday <= y.StartDate
						 OR @DateToday BETWEEN y.StartDate AND (ISNULL(y.EndDate, @FutureDate) - 1)
						 )
					  )
		  AND p.IsStrawman = 0
     ORDER BY p.LastName, p.FirstName

END

