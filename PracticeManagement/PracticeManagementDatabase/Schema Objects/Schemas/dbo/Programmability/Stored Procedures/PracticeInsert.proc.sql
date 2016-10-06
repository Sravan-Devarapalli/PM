﻿-- =============================================
-- Author:		Nikita Goncharenko
-- Create date: 2010-02-26
-- Description:	Practices routines
-- =============================================
CREATE PROCEDURE dbo.PracticeInsert 
	@Name VARCHAR(100),
	@Abbreviation NVARCHAR(100) = NULL,
	@PracticeManagerId INT,
	@IsActive BIT = 1,
	@IsCompanyInternal BIT = 0 ,
	@UserLogin NVARCHAR(MAX),
	@DivisionIds NVARCHAR(MAX) =NULL,
	@ProjectDivisionIds NVARCHAR(MAX) =NULL
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRY
		BEGIN TRAN PracticeInsert_Tran;

		SELECT @Name = REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(@Name)),' ','<>'),'><',''),'<>',' '),
			   @Abbreviation = REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(@Abbreviation)),' ','<>'),'><',''),'<>',' ')

		DECLARE @IsNotesRequired BIT = 1

		SELECT @IsNotesRequired =(SELECT  s.Value 
								FROM dbo.Settings AS s
								WHERE SettingsKey='NotesRequiredForTimeEntry' AND TypeId=4)

		IF(@IsActive = 0)
		BEGIN
		SET @IsNotesRequired = 1
		END
		DECLARE @Error NVARCHAR(200)
		IF EXISTS(SELECT 1 FROM dbo.Practice WHERE [Name] = @Name)
		BEGIN
			SET @Error = 'Practice area name already exists. Please enter a different practice area name.'
			RAISERROR(@Error,16,1)
		END
		IF EXISTS(SELECT 1 FROM dbo.Practice WHERE ISNULL(Abbreviation,0) = @Abbreviation)
		BEGIN
			SET @Error = 'Abbreviation with this name already exists for a practice area. Please enter different abbreviation name.'
			RAISERROR(@Error,16,1)
		END

		DECLARE @PracticeId INT

		EXEC SessionLogPrepare @UserLogin = @UserLogin

		INSERT INTO dbo.Practice (
			[Name],
			[Abbreviation],
			PracticeManagerId,
			IsActive,
			IsCompanyInternal,
			IsNotesRequired
		) VALUES ( 
			@Name,
			@Abbreviation,
			@PracticeManagerId,
			@IsActive,
			@IsCompanyInternal,
			@IsNotesRequired)

		SELECT @PracticeId = SCOPE_IDENTITY()

		INSERT INTO dbo.PracticeManagerHistory
						(PracticeId,		
						PracticeManagerId,	
						StartDate,			
						EndDate)
				VALUES
					(
					@PracticeId,
					@PracticeManagerId,
					dbo.InsertingTime(),
					NULL)
		EXEC dbo.PracticeStatusHistoryUpdate
			@PracticeId = @PracticeId,
			@IsActive = @IsActive

		EXEC dbo.UpdateDivisionPracticeAreaMapping
			@PracticeId=@PracticeId,
			@DivisionIds=@DivisionIds,
			@ProjectDivisionIds=@ProjectDivisionIds


	COMMIT TRAN PracticeInsert_Tran;
	END TRY
	BEGIN CATCH
	ROLLBACK TRAN PracticeInsert_Tran;
		DECLARE	 @ERROR_STATE	tinyint
				,@ERROR_SEVERITY		tinyint
				,@ERROR_MESSAGE		    nvarchar(2000)
				,@InitialTranCount		tinyint

		SET	 @ERROR_MESSAGE		= ERROR_MESSAGE()
		SET  @ERROR_SEVERITY	= ERROR_SEVERITY()
		SET  @ERROR_STATE		= ERROR_STATE()
		RAISERROR ('%s', @ERROR_SEVERITY, @ERROR_STATE, @ERROR_MESSAGE)
	END CATCH
	EXEC dbo.SessionLogUnprepare
END

