USE [SALARY]
GO

/****** Object:  StoredProcedure [dbo].[sp_StatsSummary]    Script Date: 4/18/2015 10:39:26 PM ******/
DROP PROCEDURE [dbo].[sp_StatsSummary]
GO

/****** Object:  StoredProcedure [dbo].[sp_StatsSummary]    Script Date: 4/18/2015 10:39:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jhovani Salgado, sp_StatSummary>
-- Create date: <4/16/15>
-- Description:	<This procedure will calculate median, 1percentile, 3percentile and avg from rpt_tab data>
-- =============================================
CREATE PROCEDURE [dbo].[sp_StatsSummary]
@typeValue varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Check if STATS Table exist
	if not exists(select 1 from sys.tables where name='STATS')
	Create Table STATS
	(
	ResultType varchar(50),
	ResultName varchar(50),
	ResultValue decimal(18,2)
	)
	
	Declare @type varchar(50)
	set @type=@typeValue
	delete from STATS where ResultType=@type 

	--Calculate Median
	DECLARE @c BIGINT = (SELECT COUNT(*) FROM rpt_tab);

	insert into stats
	SELECT @type,'Median' as name,AVG(1.0 * salary) as value
	FROM (
		SELECT salary FROM RPT_TAB
		ORDER BY Salary
		OFFSET (@c-1) / 2 ROWS
		FETCH NEXT 1 + (1 - @c % 2) ROWS ONLY
		) AS rptdata

	--Calculate 1st Percentile
	DECLARE @c1p BIGINT= (SELECT COUNT(*) FROM rpt_tab)/2;

	insert into stats
	SELECT @type,'1st Percentile' as name,AVG(1.0 * salary) as value
	FROM (
		SELECT salary FROM RPT_TAB
		ORDER BY Salary
		OFFSET (@c1p-1) / 2 ROWS
		FETCH NEXT 1 + (1 - @c1p % 2) ROWS ONLY
		) AS rptdata

	--Calculate 3rd Percentile
	DECLARE @c3p BIGINT= (3*(SELECT COUNT(*) FROM rpt_tab)+1)/2;

	insert into stats
	SELECT @type,'3rd Percentile' as name,AVG(1.0 * salary) as value
	FROM (
		SELECT salary FROM RPT_TAB
		ORDER BY Salary
		OFFSET (@c3p-1) / 2 ROWS
		FETCH NEXT 1 + (1 - @c3p % 2) ROWS ONLY
		) AS rptdata

	--Calculate AVG(mean) using formulat E(x)=x*p(x), where p(x)=1/n if no duplicate values exist. else if salary shows more than once then p(x)=# of times salary shows on table/n
	if exists(select 1 from sys.tables where name='#tabletemp')
	drop table #tabletemp

	select salary,count(salary) as countsal
	into #tabletemp
	from RPT_TAB
	group by SALARY

	update RPT_TAB
	set prob=(rpt.SALARY*tmp.countsal)
	from rpt_tab rpt,#tabletemp tmp
	where rpt.SALARY=tmp.salary

	insert into stats
	select @type, 'Mean',sum(prob)/(select count(salary) from RPT_TAB) from RPT_TAB

END

GO


