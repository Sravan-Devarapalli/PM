﻿-- =============================================
-- Author:		Anatoliy Lokshin
-- Create date: 8-06-2008
-- Updated by:	Anatoliy Lokshin
-- Update date: 9-09-2008
-- Description:	Retrives the Person by the Alias (email)
-- =============================================
CREATE PROCEDURE dbo.PersonGetByAlias
(
	@Alias   NVARCHAR(256)
)
AS
	SET NOCOUNT ON

	SELECT p.PersonId,
	       p.FirstName,
	       p.LastName,
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
	       p.TelephoneNumber,
		   p.Title,
		   p.TitleId
	  FROM dbo.v_Person AS p
	 WHERE p.Alias = @Alias

