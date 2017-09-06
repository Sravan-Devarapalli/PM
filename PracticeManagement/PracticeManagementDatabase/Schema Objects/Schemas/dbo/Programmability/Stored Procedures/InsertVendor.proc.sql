CREATE PROCEDURE [dbo].[InsertVendor]
(
	@Id					INT OUT,
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

	INSERT INTO dbo.Vendor(Name, ContactName, Status, Email, TelephoneNumber, VendorTypeId)
					Values(@Name, @ContactName, @Status, @Email, @TelephoneNumber, @VendorTypeId)

	EXEC dbo.SessionLogUnprepare

	SET @Id =SCOPE_IDENTITY()

	DECLARE @Domain NVARCHAR(100)
	SELECT @Domain= SUBSTRING(@Email, CHARINDEX('@',@Email)+1,LEN(@Email))

	IF NOT EXISTS(SELECT 1 FROM Domain Where Name = @Domain)
	BEGIN
		DECLARE @sortOrderLocal INT 
		SELECT @sortOrderLocal= MAX(SortOrder)+1 FROM Domain 

		INSERT INTO Domain(Name,SortOrder)
		VALUES(@Domain, @sortOrderLocal)
	END

	COMMIT TRAN T1;
	END TRY
	BEGIN CATCH
	ROLLBACK TRAN T1;
		DECLARE @ErrorMessage NVARCHAR(MAX)
		SET @ErrorMessage = ERROR_MESSAGE()

		RAISERROR(@ErrorMessage, 16, 1)
	END CATCH

END
