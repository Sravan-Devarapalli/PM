﻿CREATE PROCEDURE dbo.MilestoneGetById
(
	@MilestoneId   INT
)
AS
BEGIN
	SET NOCOUNT ON

	SELECT m.ClientId,
	       m.ProjectId,
	       m.MilestoneId,
	       m.Description,
	       m.Amount,
	       m.StartDate,
		   m.ProjectStatusId,
		   m.ProjectStatusName,
	       m.ProjectedDeliveryDate,
	       m.IsHourlyAmount,
	       m.ProjectName,
	       m.ProjectStartDate,
	       m.ProjectEndDate,
		   m.ProjectNumber,
	       m.Discount,
	       m.ClientName,
	       m.ExpectedHours,
	       m.PersonCount,
	       m.ProjectedDuration,
	       m.ConsultantsCanAdjust,
	       m.ClientIsChargeable,
	       m.ProjectIsChargeable,
	       m.MilestoneIsChargeable,		  
		   m.IsMarginColorInfoEnabled,
		   m.MilestoneType,
		   m.PremiumDiscount,
		   m.DiscountType,
		   m.IsAmountAtMilestone,
		   m.IsDiscountAtMilestone
	  FROM dbo.v_Milestone AS m
	 WHERE m.MilestoneId = @MilestoneId
END

