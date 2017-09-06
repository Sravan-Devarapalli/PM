CREATE PROCEDURE [dbo].[VendorValidations]
(
	@Name				NVARCHAR (50),			
	@ContactName		NVARCHAR (50),		
	@Email			    NVARCHAR (50),
	@Id					INT=NULL
)
AS
BEGIN	
	BEGIN TRY
		DECLARE @ErrorMessage NVARCHAR(2048), @emailDomain NVARCHAR(100)
			
			IF @Name IS NOT NULL AND EXISTS(SELECT 1
											FROM dbo.[Vendor] AS v
											WHERE v.Name = @Name AND  (@Id IS NULL OR v.Id <> @Id))
			BEGIN
			-- Vendor Name uniqueness violation
				SELECT @ErrorMessage = [dbo].[GetErrorMessage](70022)
				RAISERROR (@ErrorMessage, 16, 1)
			END	
			ELSE IF @ContactName IS NOT NULL AND EXISTS(SELECT 1
											FROM dbo.[Vendor] AS v
											WHERE v.ContactName = @ContactName AND  (@Id IS NULL OR v.Id <> @Id))
			BEGIN
			-- Vendor Name uniqueness violation
				SELECT @ErrorMessage = [dbo].[GetErrorMessage](70023)
				RAISERROR (@ErrorMessage, 16, 1)
			END	
			ELSE IF @Email IS NOT NULL AND EXISTS(SELECT 1
											FROM dbo.[Vendor] AS v
											WHERE v.Email = @Email AND  (@Id IS NULL OR v.Id <> @Id))
			BEGIN
			-- Vendor Name uniqueness violation
				SELECT @ErrorMessage = [dbo].[GetErrorMessage](70024)
				RAISERROR (@ErrorMessage, 16, 1)
			END	
			
			IF @Email IS NOT NULL 
			BEGIN
				SET @emailDomain = SUBSTRING(@Email, CHARINDEX('@',@Email)+1,LEN(@Email))

				DECLARE @ExistingDomains TABLE(domain NVARCHAR(100))

				INSERT INTO @ExistingDomains(domain)
				SELECT SUBSTRING(v.Email, CHARINDEX('@',v.Email)+1,LEN(v.Email))
				FROM Vendor v
				WHERE @Id IS NULL OR v.Id <> @Id

				IF EXISTS(SELECT 1 FROM @ExistingDomains WHERE domain = @emailDomain)
				BEGIN
					SELECT @ErrorMessage = [dbo].[GetErrorMessage](70025)
					RAISERROR (@ErrorMessage, 16, 1)
				END
			END
			
	END TRY
	BEGIN CATCH 
		SELECT @ErrorMessage = ERROR_MESSAGE()
		RAISERROR (@ErrorMessage, 16, 1)
	END CATCH
END
