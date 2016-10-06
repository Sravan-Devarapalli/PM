CREATE PROCEDURE [dbo].[PersonListAllSeniorityFilterWithCurrentPay]
(
	@PracticeId    INT, 
	@ShowAll       BIT,	--@ShowAll is reverse of Active
	@PageSize      INT,
	@PageNo        INT,
	@Looked		   NVARCHAR(40),
    @StartDate     DATETIME,
    @EndDate       DATETIME,
	@RecruiterId   INT,
	@MaxSeniorityLevel	INT,
	@SortBy			NVARCHAR(225) = NULL,
	@TimescaleId	INT,
	@Projected		BIT,
	@Inactive		BIT,
	@Terminated		BIT,
	@Alphabet		NVARCHAR(5)
)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @FirstRecord INT
	SET @FirstRecord = @PageSize * @PageNo

	IF @Looked IS NOT NULL
		SET @Looked = '%' + RTRIM(@Looked) + '%'
	ELSE
		SET @Looked = '%'
		
	IF @Alphabet IS NOT NULL
		SET @Alphabet = @Alphabet + '%'
	ELSE
		SET @Alphabet = '%'

    DECLARE @flags bit
    IF (@StartDate IS NULL) OR (@EndDate IS NULL)
		SET @flags = 1
	ELSE
		SET @flags = 0

		-- Listing a specified page
		DECLARE @LastRecord INT, @Now DATETIME
		SELECT @LastRecord = @FirstRecord + @PageSize, @Now=GETDATE()
		
		
		DECLARE @SqlQuery NVARCHAR(4000),
				@OrderBy NVARCHAR(1000)
		IF ISNULL(@SortBy,'') = ''
		BEGIN
		 SET @OrderBy = 'ORDER BY  LastName, FirstName'
		 END
		 ELSE
		  SET @OrderBy = 'ORDER BY ' + @SortBy
		
		
		SELECT @SqlQuery = 'SELECT *
		  FROM (
				SELECT TOP (@LastRecord)
					   p.PersonId,
					   p.FirstName,
					   p.LastName,
					   p.PTODaysPerAnnum,
					   p.HireDate,
					   p.TerminationDate,
					   p.Alias,
					   M.LastLoginDate,
					   p.DefaultPractice,
					   p.PracticeName,
					   p.PersonStatusId,
		               p.PersonStatusName,
					   p.EmployeeNumber,
					   ROW_NUMBER() OVER(' + @OrderBy + ') - 1 AS rownum,
		               p.SeniorityId,
		               p.SeniorityName,
					   p.ManagerId,
					   p.ManagerAlias,
					   p.ManagerFirstName,
					   p.ManagerLastName,
					   p.PracticeOwnedId,
					   p.PracticeOwnedName,
					   p.TelephoneNumber,
					   p.IsStrawMan,
					   TS.PayPersonId,
					   TS.StartDate,
					   TS.EndDate,
					   TS.Amount,
					   TS.Timescale,
					   TS.AmountHourly,
					   TS.TimesPaidPerMonth,
					   TS.Terms,
					   TS.VacationDays,
					   TS.BonusAmount,
					   TS.BonusHoursToCollect,
					   TS.IsYearBonus,
					   TS.DefaultHoursPerDay,
					   TS.TimescaleName
				  FROM dbo.v_Person AS p
				  OUTER APPLY 
						(SELECT TOP 1 
							   pay.PersonId PayPersonId,
							   pay.StartDate,
							   pay.EndDate,
							   pay.Amount,
							   pay.Timescale,
							   pay.AmountHourly,
							   pay.TimesPaidPerMonth,
							   pay.Terms,
							   pay.VacationDays,
							   pay.BonusAmount,
							   pay.BonusHoursToCollect,
							   pay.IsYearBonus,
							   pay.DefaultHoursPerDay,
							   pay.TimescaleName
						  FROM dbo.v_Pay AS pay
						 WHERE p.PersonId = pay.PersonId
						   AND ((@Now >= pay.StartDate
						   AND @Now < pay.EndDateOrig) OR @Now < pay.StartDate)
						   ORDER BY pay.StartDate
						) TS
				 LEFT JOIN dbo.aspnet_Users U ON (U.UserName = P.Alias)
				 LEFT JOIN dbo.aspnet_Membership M ON (U.UserId = M.UserId)
				 WHERE   p.IsStrawman = 0 AND
				 (   (p.PersonStatusId IN (1,5) AND @ShowAll = 0) 
							OR (p.PersonStatusId = 2 AND @Terminated = 1)
							OR (p.PersonStatusId = 3 AND @Projected = 1)
							OR (p.PersonStatusId = 4 AND @Inactive = 1) 
						) 
		            AND ( p.DefaultPractice = @PracticeId OR @PracticeId IS NULL )
					AND ( p.FirstName LIKE @Looked OR p.LastName LIKE @Looked OR p.EmployeeNumber LIKE @Looked )
		            AND (   @RecruiterId IS NULL
		                 OR EXISTS (SELECT 1
		                              FROM dbo.RecruiterCommission AS c
		                             WHERE c.RecruitId = p.PersonId AND c.RecruiterId = @RecruiterId))
		            AND (	(TS.Timescale = @TimescaleId) OR @TimescaleId IS NULL	)
		            AND ( p.LastName LIKE @Alphabet )
					AND ((@MaxSeniorityLevel IS NULL) OR (@MaxSeniorityLevel >= p.SeniorityValue))' + @OrderBy + '
		       ) AS tmp
		 WHERE ( (@FirstRecord !=0 AND tmp.rownum BETWEEN @FirstRecord AND @LastRecord - 1)
				 OR @FirstRecord = 0 ) '
		 

		EXEC sp_executeSql 
				@SqlQuery,		
				N'@FirstRecord	INT, @LastRecord	INT, @Looked	NVARCHAR(40), @Now	DATETIME, 
				@MaxSeniorityLevel	INT, @RecruiterId	INT, @PracticeId	INT, @ShowAll	BIT, @TimescaleId INT,
				@Projected BIT, @Terminated BIT, @Inactive BIT, @Alphabet NVARCHAR(5)',				
				@FirstRecord = @FirstRecord, @LastRecord = @LastRecord,
				@RecruiterId = @RecruiterId, @PracticeId = @PracticeId, @Looked = @Looked,
				@MaxSeniorityLevel = @MaxSeniorityLevel, @ShowAll = @ShowAll, @Now = @Now, @TimescaleId = @TimescaleId,
				@Projected = @Projected, @Terminated = @Terminated, @Inactive = @Inactive, @Alphabet = @Alphabet
				
	END

GO



