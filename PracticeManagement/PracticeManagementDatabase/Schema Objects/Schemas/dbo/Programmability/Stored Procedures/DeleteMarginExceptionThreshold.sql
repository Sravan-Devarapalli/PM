CREATE PROCEDURE [dbo].[DeleteMarginExceptionThreshold]
(
	@Id  INT
)
AS
BEGIN

	SET NOCOUNT ON;
		
		DELETE FROM MarginException WHERE Id=@Id

END
