USE [SALARY]
GO

/****** Object:  StoredProcedure [dbo].[sp_setupTab]    Script Date: 4/18/2015 10:39:48 PM ******/
IF Exists (select * from sys.objects where object_id=OBJECT_ID('[dbo].[sp_SetupTab]')
DROP PROCEDURE [dbo].[sp_setupTab]
GO

/****** Object:  StoredProcedure [dbo].[sp_setupTab]    Script Date: 4/18/2015 10:39:48 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Salgado Jhovani>
-- Create date: <04/11/2015>
-- Description:	<Check if UHDATA Table exist if not create, else delete all data from it>
-- =============================================
CREATE PROCEDURE [dbo].[sp_setupTab]
AS
BEGIN
	SET NOCOUNT ON;

	if not exists (select 1 from sys.tables where name='UHDATA')
	CREATE TABLE [dbo].[UHDATA](
	[JobTitle] [varchar](50) NULL,
	[Salary] [decimal](18, 2) NULL,
	[DeptId] [varchar](50) NULL,
	[Gender] [varchar](50) NULL,
	[Ethnicity] [varchar](50) NULL,
	[Prob] [decimal](18, 2) NULL
	)

	if not exists (select 1 from sys.tables where name='COMP_TAB')
	CREATE TABLE [dbo].[COMP_TAB](
	[JobTitle] [varchar](50) NULL,
	[Salary] [decimal](18, 2) NULL,
	[Year Hired] [varchar](50) NULL,
	[DeptId] [varchar](50) NULL,
	[Prob] [decimal](18, 2) NULL
	)

	if not exists (select 1 from sys.tables where name='EQTY_TAB')
	CREATE TABLE [dbo].[EQTY_TAB](
	[JobTitle] [varchar](50) NULL,
	[Salary] [decimal](18, 2) NULL,
	[DeptId] [varchar](50) NULL,
	[Gender] [varchar](50) NULL,
	[Ethnicity] [varchar](50) NULL,
	[Prob] [decimal](18, 2) NULL
	)

	if exists (select 1 from sys.tables where name='RPT_TAB')
	drop table rpt_tab

END


GO


