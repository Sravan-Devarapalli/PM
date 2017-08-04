﻿
CREATE PROCEDURE dbo.MilestonePersonEntryListByMilestonePersonId
(
	@MilestonePersonId   INT
)
AS
	SET NOCOUNT ON

	SELECT mp.MilestonePersonId,
		   mp.SeniorityId,
           mp.PersonId,
	       mp.StartDate,
	       mp.EndDate,
	       mp.PersonRoleId,
	       mp.Amount,
	       mp.HoursPerDay,
	       mp.RoleName,
		   mp.VacationDays,	
	       mp.ExpectedHoursWithVacationDays,
		   mp.Location,
		   mp.LastName,
		   mp.FirstName,
		   mp.EntryId,
		   mp.MilestonePersonDiscount,
		   mp.LockDiscount,
		   mp.IsNewToBudget
	  FROM dbo.v_MilestonePerson AS mp
	 WHERE mp.MilestonePersonId = @MilestonePersonId

	 SELECT  personid,Date,ActualHours 
	 FROM dbo.[GetPersonTimeoffValuesByMilestoneId](NULL,NULL,@MilestonePersonId,NULL)

