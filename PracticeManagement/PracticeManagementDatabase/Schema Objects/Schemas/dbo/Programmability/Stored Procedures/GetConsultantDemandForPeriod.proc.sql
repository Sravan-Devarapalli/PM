﻿/* Views in Consultant Demand Pages:
1.Summary View- @StartDate,@EndDate,@IsSummary
2.Details View -@StartDate,@EndDate,@IsDetail
	a.Group by Month-@GroupByMonth,@ViewByTitleSkill
	b.Group by Title/Skill-@GroupByTitleSkill,@ViewByTitleSkill
	c.Group by Title :@GroupByTitle,@ViewBySkill
	d.Group by Skill :@GroupBySkill,@ViewByTitle
3.Graphs View  @StartDate,@EndDate,@IsGraph,
	a.Group By Month: @GroupByMonth
		(i).View By Title  : @ViewByTitle
		(ii).View By Skill : @ViewBySkill
	b.Group By Title :@GroupByTitle,@ViewByTitle
	c.Group By Skill :@GroupBySkill,@ViewBySkill
*/
CREATE PROCEDURE dbo.GetConsultantDemandForPeriod
 (
	@StartDate DATETIME,
	@EndDate DATETIME,
	@Titles VARCHAR(MAX) = NULL,
	@Skills VARCHAR(MAX) = NULL,
	@SalesStages VARCHAR(MAX) = NULL,
	--Report Type
	@IsSummary BIT=0,
	@IsDetail BIT=0,
	@IsGraph BIT=0,
	--Group By
	@GroupByMonth BIT=0,
	@GroupByTitle BIT=0,
	@GroupBySkill BIT=0,
	@GroupByTitleSkill BIT=0,
	--View By
	@ViewByTitle BIT = 0,
	@ViewBySkill BIT = 0,
	@ViewByTitleSkill BIT = 0,
	--Order By
	@OrderByCerteria VARCHAR(MAX) = 'MonthStartDate'
)
AS
BEGIN
	SELECT @StartDate = CONVERT(DATE,@StartDate),@EndDate = CONVERT(DATE,@EndDate)
	DECLARE @MonthStartDate DATETIME = DATEADD(D,-DAY(@StartDate)+1,@StartDate)
	SELECT @OrderByCerteria  = ' ORDER BY ' + @OrderByCerteria 

	DECLARE @Query NVARCHAR(MAX) = ' FROM [dbo].[v_ConsultingDemand] CD ',
			@GroupBy NVARCHAR(MAX) = ' GROUP BY ',
			@Select NVARCHAR(MAX) = 'SELECT ',
			@Where  NVARCHAR(MAX) = ' WHERE CD.ResourceStartDate BETWEEN @StartDate AND @EndDate'+
									CASE WHEN @Titles IS NOT NULL THEN ' AND CD.Title IN (''' + REPLACE(REPLACE(@Titles,'''',''''''),',',''',''') + ''')'  ELSE '' END +
									CASE WHEN @Skills IS NOT NULL THEN ' AND CD.Skill IN (''' + REPLACE(REPLACE(@Skills,'''',''''''),',',''',''') + ''')' ELSE '' END +
									CASE WHEN @SalesStages IS NOT NULL THEN ' AND CD.SalesStage IN (''' + REPLACE(REPLACE(@SalesStages,'''',''''''),',',''',''') + ''')' ELSE '' END ,
			@Columns NVARCHAR(MAX) = ''

	--1.Summary View
	IF @IsSummary=1
	BEGIN
		SELECT @Query = 'FROM 
								(SELECT C.MonthStartDate,D.Title,D.Skill
								FROM 
								(SELECT C.MonthStartDate FROM dbo.Calendar C WHERE C.Date BETWEEN @MonthStartDate AND @EndDate GROUP BY C.MonthStartDate) C
								CROSS JOIN
								(
									SELECT B.Title,B.Skill
									FROM [dbo].[v_ConsultingDemand] B  
									WHERE B.MonthStartDate BETWEEN @MonthStartDate AND @EndDate  AND B.ResourceStartDate BETWEEN @StartDate AND @EndDate
									GROUP BY B.Title,B.Skill
								) D 
								)CD
								LEFT JOIN [dbo].[v_ConsultingDemand] A  ON A.MonthStartDate = CD.MonthStartDate AND A.Title = CD.Title AND A.Skill = CD.Skill AND A.ResourceStartDate BETWEEN @StartDate AND @EndDate'

		SELECT @GroupBy = @GroupBy + 'CD.MonthStartDate,CD.Title,CD.Skill',
				@Select = @Select+ 'CD.MonthStartDate,CD.Title,CD.Skill,ISNULL(SUM(COUNT),0) AS [COUNT]',
				@Where  = ' WHERE 1=1 '+
									CASE WHEN @Titles IS NOT NULL THEN ' AND CD.Title IN (''' + REPLACE(REPLACE(@Titles,'''',''''''),',',''',''') + ''')'  ELSE '' END +
									CASE WHEN @Skills IS NOT NULL THEN ' AND CD.Skill IN (''' + REPLACE(REPLACE(@Skills,'''',''''''),',',''',''') + ''')' ELSE '' END 
	END
	--2.Details View 
	ELSE IF @IsDetail=1 
	BEGIN
		
		SELECT @Columns = 'CD.ClientId,CD.AccountName,CD.ProjectId,CD.ProjectNumber,CD.ProjectName,CD.ProjectDescription,CD.OpportunityId,CD.OpportunityNumber,CD.ResourceStartDate,CD.SalesStage' + 
			CASE 
				WHEN @GroupByMonth = 1 THEN ',CD.MonthStartDate'
				WHEN @GroupByTitleSkill = 1 THEN ',CD.Title,CD.Skill'
				WHEN @GroupByTitle = 1 THEN ',CD.Title'
				WHEN @GroupBySkill = 1 THEN ',CD.Skill'
				ELSE ''
			END + 
			CASE 
				WHEN @GroupByTitleSkill = 0 AND @ViewByTitleSkill = 1 THEN ',CD.Title,CD.Skill'
				WHEN @GroupByTitleSkill = 0 AND @GroupByTitle = 0 AND @ViewByTitle = 1 THEN ',CD.Title'
				WHEN @GroupByTitleSkill = 0 AND @GroupBySkill = 0 AND @ViewBySkill = 1 THEN ',CD.Skill'
				ELSE ''
			END
			SELECT	@Select = @Select + @Columns + ',ISNULL(SUM(COUNT),0) AS [COUNT]',
					@GroupBy = @GroupBy + @Columns

	END
	--2.Graph View 
	ELSE IF @IsGraph=1 
	BEGIN

	SELECT @Columns =
			CASE 
				WHEN @GroupByMonth = 1 THEN 'CD.MonthStartDate'
				WHEN @GroupByTitleSkill = 1 THEN 'CD.Title,CD.Skill'
				WHEN @GroupByTitle = 1 THEN 'CD.Title'
				WHEN @GroupBySkill = 1 THEN 'CD.Skill'
				ELSE ''
			END + 
			CASE 
				WHEN @GroupByTitleSkill = 0 AND @ViewByTitleSkill = 1 THEN ',CD.Title,CD.Skill'
				WHEN @GroupByTitleSkill = 0 AND @GroupByTitle = 0 AND @ViewByTitle = 1 THEN ',CD.Title'
				WHEN @GroupByTitleSkill = 0 AND @GroupBySkill = 0 AND @ViewBySkill = 1 THEN ',CD.Skill'
				ELSE ''
			END

			SELECT	@Select = @Select + @Columns + ',ISNULL(SUM(COUNT),0) AS [COUNT]',
					@GroupBy = @GroupBy + @Columns

	END
	
	SELECT @Query = @Select + @Query + @Where + @GroupBy + @OrderByCerteria

	--Print @Query
	EXEC sp_executeSql  @Query,N'@StartDate DATETIME,@EndDate DATETIME,@MonthStartDate DATETIME',@StartDate=@StartDate,@EndDate=@EndDate,@MonthStartDate = @MonthStartDate

END

