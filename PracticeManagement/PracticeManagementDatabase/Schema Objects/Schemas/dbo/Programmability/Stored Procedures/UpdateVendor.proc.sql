CREATE PROCEDURE [dbo].[UpdateVendor]
(
	@Id			INT,
	@Name				NVARCHAR (50),			
	@ContactName		NVARCHAR (50),		
	@Status				INT,						
	@Email			    NVARCHAR (50),		
	@TelephoneNumber	NVARCHAR(20),				
	@VendorTypeId		INT,
	@UserLogin          NVARCHAR(255)
)
AS
BEGIN

	SET NOCOUNT ON;
	BEGIN TRY
	BEGIN TRAN  T1;

	EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin

	UPDATE dbo.Vendor
	SET Name=@Name,
		ContactName=@ContactName,
		Status=@Status,
		Email=@Email,
		TelephoneNumber=@TelephoneNumber,
		VendorTypeId=@VendorTypeId
	WHERE Id=@Id

	DECLARE @Domain NVARCHAR(100)
	SELECT @Domain= SUBSTRING(@Email, CHARINDEX('@',@Email)+1,LEN(@Email))

	IF NOT EXISTS(SELECT 1 FROM Domain Where Name = @Domain)
	BEGIN
		DECLARE @sortOrderLocal INT 
		SELECT @sortOrderLocal= MAX(SortOrder)+1 FROM Domain 

		INSERT INTO Domain(Name,SortOrder)
		VALUES(@Domain, @sortOrderLocal)
	END

	EXEC dbo.SessionLogUnprepare

	COMMIT TRAN T1;
	END TRY
	BEGIN CATCH
	ROLLBACK TRAN T1;
		DECLARE @ErrorMessage NVARCHAR(MAX)
		SET @ErrorMessage = ERROR_MESSAGE()

		RAISERROR(@ErrorMessage, 16, 1)
	END CATCH

END
