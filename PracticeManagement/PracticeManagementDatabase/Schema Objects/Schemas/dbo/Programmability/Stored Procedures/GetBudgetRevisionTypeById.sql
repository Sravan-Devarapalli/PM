CREATE PROCEDURE [dbo].[GetBudgetRevisionTypeById]
(
	@Id INT 
)
AS
	DECLARE @result NVARCHAR(max)=''
	IF EXISTS(select 1 from BudgetResetType where id = @Id)
	BEGIN
		SELECT @result= Name
		FROM BudgetResetType where id = @Id
	END
	SELECT 	@result

