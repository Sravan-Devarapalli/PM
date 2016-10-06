﻿-- =============================================
-- Author:		Anatoliy Lokshin
-- Create date: 8-29-2008
-- Updated by:	Anatoliy Lokshin
-- Update date:	11-20-2008
-- Description:	Selects all subordinated persons for a specified practice manager.
-- =============================================
CREATE PROCEDURE dbo.PersonListSubordinates
(
	@PracticeManagerId   INT
)
AS
	SET NOCOUNT ON

	DECLARE @PracticeManagerSeniorityValue INT

	--Get the SeniorityId of the Practice Manager and store it in a variable
	SELECT @PracticeManagerSeniorityValue = S.SeniorityValue
	FROM dbo.Person P
	JOIN dbo.Seniority S ON P.SeniorityId = S.SeniorityId
	WHERE PersonId = @PracticeManagerId

	SELECT p.PersonId,
	       p.FirstName,
	       p.LastName,
	       p.PTODaysPerAnnum,
	       p.HireDate,
	       p.TerminationDate,
	       p.Alias,
	       p.DefaultPractice,
	       p.PracticeName,
	       p.PersonStatusId,
	       p.PersonStatusName,
		   p.EmployeeNumber,
		   p.SeniorityId,
		   p.SeniorityName,
	       p.ManagerId,
	       p.ManagerAlias,
	       p.ManagerFirstName,
	       p.ManagerLastName,
	       p.PracticeOwnedId,
	       p.PracticeOwnedName, 
	       p.TelephoneNumber
	  FROM dbo.v_Person AS p
	  LEFT JOIN dbo.Practice AS pr ON p.DefaultPractice = pr.PracticeId
	 WHERE ISNULL(pr.IsCompanyInternal, 0) = 0
					AND p.SeniorityValue > @PracticeManagerSeniorityValue -- All the persons, having seniority level below the Practice Manager's seniority level
					AND @PracticeManagerSeniorityValue <= 65 -- According to 2656, Managers and up should be able to see their subordinates, but not equals.
					AND p.PersonStatusId = 1 -- According to 2891 we should show only active persons
	ORDER BY p.LastName, p.FirstName

