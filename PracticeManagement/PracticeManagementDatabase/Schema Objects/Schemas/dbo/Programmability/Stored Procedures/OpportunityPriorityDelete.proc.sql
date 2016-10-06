﻿CREATE PROCEDURE [dbo].[OpportunityPriorityDelete]
@UpdatedPriorityId INT,
@DeletedPriorityId INT,
@UserLogin         NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON
	-- Start logging session
		EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin

	IF(@UpdatedPriorityId IS NOT NULL)
	BEGIN 
		UPDATE v_Opportunity
		SET PriorityId =@UpdatedPriorityId
		WHERE PriorityId =@DeletedPriorityId
		
		UPDATE [dbo].[OpportunityPriorities]
		SET [IsInserted] = 1    
		WHERE id = @UpdatedPriorityId
	END
	
	UPDATE [dbo].[OpportunityPriorities]
	SET [Description] =NULL  
	   ,[IsInserted] = 0    
	   ,[DisplayName] = NULL
	WHERE id = @DeletedPriorityId

	-- End logging session
		EXEC dbo.SessionLogUnprepare
END

GO
