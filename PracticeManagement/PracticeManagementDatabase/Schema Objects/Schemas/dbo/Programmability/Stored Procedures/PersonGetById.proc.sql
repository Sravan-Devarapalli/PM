﻿-- =============================================
-- Author:		Anatoliy Lokshin
-- Create date: 8-04-2008
-- Updated by:	Srinivas.M
-- Update date: 05-21-2012
-- Description:	Retrives the Person by its ID.
-- =============================================
CREATE PROCEDURE [dbo].[PersonGetById]
(
	@PersonId	INT
)
AS
BEGIN
	SET NOCOUNT ON

	SELECT p.PersonId,
	       p.FirstName,
	       p.LastName,
		   p.PreferredFirstName,
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
		   p.PracticesOwned,
		   p.TelephoneNumber,
		   p.IsDefaultManager,
		   p.IsWelcomeEmailSent,
		   p.IsStrawman AS IsStrawman,
		   p.IsOffshore,
		   p.PaychexID,
		   p.DivisionId,
		   p.TerminationReasonId,
		   p.RecruiterId AS RecruiterId,
		   p.TitleId,
           p.Title,
		   p.JobSeekerStatusId,
		   p.SourceId,
		   p.TargetedCompanyId,
		   p.EmployeeReferralId,
		   p.EmployeeReferralFirstName,
		   p.EmployeeReferralLastName,
		   p.CohortAssignmentId,
		   p.CohortAssignmentName,
		   p.LocationId,
		   P.LocationCode,
		   p.LocationName,
		   p.PracticeLeadershipId,
		   p.IsMBO,
		   p.IsInvestmentResource,
		   p.TargetUtilization,
		   p.RighttoPresentStartDate,
		   p.RighttoPresentEndDate
	  FROM dbo.v_Person AS p
	 WHERE p.PersonId = @PersonId
END

