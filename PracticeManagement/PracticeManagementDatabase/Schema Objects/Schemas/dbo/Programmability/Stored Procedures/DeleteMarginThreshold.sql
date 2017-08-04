CREATE PROCEDURE [dbo].[DeleteMarginThreshold]
(
	@Id  INT
)
AS
BEGIN

	SET NOCOUNT ON;
		
		DELETE FROM MarginThreshold WHERE Id=@Id

END
