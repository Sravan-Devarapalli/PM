CREATE PROCEDURE [Skills].[SavePersonProfiles]
(
	@PersonId         INT,		
	@ProfilesXml	  XML,
	@UserLogin		  NVARCHAR (100)      
)
AS
/*
<Profiles>
<Profile Id='' ProfileName='' ProfileURL='' IsDefault='' > </Profile>
<Profile Id='' ProfileName='' ProfileURL='' IsDefault=''> </Profile>
....
</Profiles>
*/
BEGIN
	-- Start logging session
	EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin
	SET ANSI_NULLS ON
	DECLARE @ModifiedBy INT ,@Now DATETIME
	SELECT @ModifiedBy = PersonId FROM dbo.Person WHERE Alias =  @UserLogin
	SET @Now = dbo.InsertingTime()

	BEGIN TRY
		BEGIN TRANSACTION SavePersonProfiles
	
		--Delete Person Profiles 
		DELETE PProfile
		FROM Skills.PersonProfile PProfile
		LEFT JOIN @ProfilesXml.nodes('/Profiles/Profile') Profiles(P) ON Profiles.P.value('@Id', 'INT') = PProfile.Id
		WHERE PProfile.PersonId = @PersonId
			AND ISNULL(Profiles.P.value('@Id', 'INT'),0) = 0

		-- Start logging session
		EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin

		--update operation
			UPDATE PProfile
			SET PProfile.ProfileUrl = Profiles.P.value('@ProfileURL', 'NVARCHAR(MAX)')
			,PProfile.ProfileName = Profiles.P.value('@ProfileName', 'NVARCHAR(50)')
			,ModifiedBy = @ModifiedBy
			,ModifiedDate = @Now
			,IsDefault = Profiles.P.value('@IsDefault', 'BIT')
			FROM Skills.PersonProfile PProfile
			INNER JOIN @ProfilesXml.nodes('/Profiles/Profile') Profiles(P) ON Profiles.P.value('@Id', 'INT') = PProfile.Id
			WHERE PersonId = @PersonId
			AND ( 
					PProfile.ProfileUrl <> Profiles.P.value('@ProfileURL', 'NVARCHAR(MAX)') 
					OR PProfile.ProfileName <> Profiles.P.value('@ProfileName', 'NVARCHAR(50)')
					OR IsDefault <> Profiles.P.value('@IsDefault', 'BIT')
				)
					
		-- Start logging session
		EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin

		INSERT INTO [Skills].[PersonProfile](PersonId,ProfileName,ProfileUrl,IsDefault,ModifiedBy,CreatedDate,ModifiedDate)
		SELECT  @PersonId,
				Profiles.P.value('@ProfileName', 'NVARCHAR(50)'),
				Profiles.P.value('@ProfileURL', 'NVARCHAR(MAX)'),
				Profiles.P.value('@IsDefault', 'BIT'),
				@ModifiedBy,
				@Now,
				@Now
		FROM  @ProfilesXml.nodes('/Profiles/Profile') Profiles(P)
		WHERE Profiles.P.value('@Id', 'INT')  =  -1
		
		COMMIT TRANSACTION SavePersonProfiles
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION SavePersonProfiles

		DECLARE @Error NVARCHAR(MAX)
		SEt @Error = ERROR_MESSAGE()

		RAISERROR(@Error, 16, 1)
	END CATCH
	
	-- End logging session
	EXEC dbo.SessionLogUnprepare

END
