﻿--The source is 
-- =============================================
-- Author:		Anatoliy Lokshin
-- Create date: 5-26-2008
-- Update by:	Anatoliy Lokshin
-- Update date:	7-3-2008
-- Description:	Retrives a current or nearest future payment for the specified person.
-- =============================================
CREATE PROCEDURE [dbo].[PayGetCurrentByPerson]
(
	@PersonId   INT
)
AS
	SET NOCOUNT ON

	DECLARE @Now DATETIME
	SET @Now = GETDATE()

	SELECT p.PersonId,
	       p.StartDate,
	       p.EndDate,
	       p.Amount,
	       p.Timescale,
	       p.AmountHourly,
	       p.VacationDays,
	       p.BonusAmount,
	       p.BonusHoursToCollect,
	       p.IsYearBonus,
	       p.TimescaleName,
		   p.PracticeId,
		   pr.Name PracticeName,
		   Tt.TitleId,
		   Tt.Title,
	       p.SLTApproval,
		   p.SLTPTOApproval,
		   p.DivisionId,
		   d.DivisionName,
		   p.VendorId,
		   p.VendorName
	  FROM dbo.v_Pay AS p
	  LEFT JOIN dbo.Title Tt ON Tt.TitleId = P.TitleId
	  LEFT JOIN dbo.Practice pr on pr.PracticeId = p.PracticeId
	  LEFT JOIN dbo.PersonDivision d ON d.DivisionId=p.DivisionId
	 WHERE p.PersonId = @PersonId
	   AND @Now >= p.StartDate
	   AND @Now < p.EndDateOrig
	UNION ALL
	SELECT TOP 1
	       p.PersonId,
	       p.StartDate,
	       p.EndDate,
	       p.Amount,
	       p.Timescale,
	       p.AmountHourly,
	       p.VacationDays,
	       p.BonusAmount,
	       p.BonusHoursToCollect,
	       p.IsYearBonus,
	       p.TimescaleName,
		   p.PracticeId,
		   pr.Name PracticeName,
		   Tt.TitleId,
		   Tt.Title,
   	       p.SLTApproval,
		   p.SLTPTOApproval,
		   p.DivisionId,
		   d.DivisionName,
		   p.VendorId,
		   p.VendorName
	  FROM dbo.v_Pay AS p
	  LEFT JOIN dbo.Title Tt ON Tt.TitleId = P.TitleId
	  LEFT JOIN dbo.Practice pr on pr.PracticeId = p.PracticeId
	  LEFT JOIN dbo.PersonDivision d ON d.DivisionId=p.DivisionId
	 WHERE p.PersonId = @PersonId
	   AND NOT EXISTS(SELECT 1
	                    FROM dbo.v_Pay AS p
	                   WHERE p.PersonId = @PersonId
	                     AND @Now >= p.StartDate
	                     AND @Now < p.EndDateOrig)
	   AND @Now < p.StartDate
	ORDER BY p.StartDate

