﻿-- =============================================
-- Author:		Anatoliy Lokshin
-- Create date: 8-04-2008
-- Updated by:	Anatoliy Lokshin
-- Update date: 11-06-2008
-- Description:	Retrives the filtered list of persons.
-- =============================================
CREATE PROCEDURE dbo.PersonListAll
(
	@PracticeId    INT, 
	@ShowAll       BIT,
	@PageSize      INT,
	@PageNo        INT,
	@Looked		   NVARCHAR(40),
    @StartDate     DATETIME,
    @EndDate       DATETIME,
	@RecruiterId   INT
)
AS
	SET NOCOUNT ON;

	DECLARE @FirstRecord INT,
			@FutureDate DATETIME

	SELECT @FirstRecord = @PageSize * @PageNo ,
		   @FutureDate = dbo.GetFutureDate()

	IF @Looked IS NOT NULL
		SET @Looked = '%' + RTRIM(@Looked) + '%'
	ELSE
		SET @Looked = '%'

    DECLARE @Flags bit
    IF (@StartDate IS NULL) OR (@EndDate IS NULL)
		SET @Flags = 1
	ELSE
		SET @Flags = 0

	IF @FirstRecord IS NULL
	BEGIN
		-- Listing all records
		SELECT p.PersonId,
			   p.FirstName,
			   p.LastName,
			   p.PTODaysPerAnnum,
			   p.HireDate,
			   p.TerminationDate,
			   p.Alias,
			   p.DefaultPractice,
			   p.PracticeName,
			   p.PersonStatusId,
		       p.PersonStatusName,
			   p.EmployeeNumber,
		       p.SeniorityId,
		       p.SeniorityName
		  FROM dbo.v_Person AS p
		 WHERE (   ( (p.PersonStatusId = 1 AND @ShowAll = 0) AND @PracticeId IS NULL )
		        OR ((p.PersonStatusId = 1 AND @ShowAll = 0) AND @PracticeId = p.DefaultPractice)
		        OR ( @ShowAll = 1 AND @PracticeId IS NULL )
		        OR ( @ShowAll = 1 AND @PracticeId = p.DefaultPractice ) )
		   AND ( p.FirstName LIKE @Looked OR p.LastName LIKE @Looked OR p.EmployeeNumber LIKE @Looked )
           AND ( @Flags = 1 OR ( @StartDate <= ISNULL(p.TerminationDate,@FutureDate) AND p.HireDate <= @EndDate))
		   AND (   @RecruiterId IS NULL
		        OR EXISTS (SELECT 1
		                     FROM dbo.RecruiterCommission AS c
		                    WHERE c.RecruitId = p.PersonId AND c.RecruiterId = @RecruiterId))
		ORDER BY p.LastName, p.FirstName
	END
	ELSE
	BEGIN
		-- Listing a specified page
		DECLARE @LastRecord INT
		SET @LastRecord = @FirstRecord + @PageSize

		SELECT tmp.PersonId,
			   tmp.FirstName,
			   tmp.LastName,
			   tmp.PTODaysPerAnnum,
			   tmp.HireDate,
			   tmp.TerminationDate,
			   tmp.Alias,
			   tmp.DefaultPractice,
			   tmp.PracticeName,
			   tmp.PersonStatusId,
		       tmp.PersonStatusName,
			   tmp.EmployeeNumber,
		       tmp.SeniorityId,
		       tmp.SeniorityName
		  FROM (
				SELECT TOP (@LastRecord)
					   p.PersonId,
					   p.FirstName,
					   p.LastName,
					   p.PTODaysPerAnnum,
					   p.HireDate,
					   p.TerminationDate,
					   p.Alias,
					   p.DefaultPractice,
					   p.PracticeName,
					   p.PersonStatusId,
		               p.PersonStatusName,
					   p.EmployeeNumber,
					   ROW_NUMBER() OVER(ORDER BY p.LastName, p.FirstName) - 1 AS rownum,
		               p.SeniorityId,
		               p.SeniorityName
				  FROM dbo.v_Person AS p
				 WHERE (   ( p.PersonStatusId = 1 AND @ShowAll = 0 AND @PracticeId IS NULL )
		                OR ( p.PersonStatusId = 1 AND @ShowAll = 0 AND @PracticeId = p.DefaultPractice)
		                OR ( @ShowAll = 1 AND @PracticeId IS NULL )
		                OR ( @ShowAll = 1 AND @PracticeId = p.DefaultPractice ) ) 
					AND ( p.FirstName LIKE @Looked OR p.LastName LIKE @Looked OR p.EmployeeNumber LIKE @Looked )
		            AND (   @RecruiterId IS NULL
		                 OR EXISTS (SELECT 1
		                              FROM dbo.RecruiterCommission AS c
		                             WHERE c.RecruitId = p.PersonId AND c.RecruiterId = @RecruiterId))
				ORDER BY p.LastName, p.FirstName
		       ) AS tmp
		 WHERE tmp.rownum BETWEEN @FirstRecord AND @LastRecord - 1
		ORDER BY tmp.LastName, tmp.FirstName
	END

