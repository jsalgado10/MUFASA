/*Creating Initial Tables in SQLite db File*/

drop table if exists RPT_TAB;

Create table IF NOT EXISTS UHDATA 
(JobTitle varchar(50) NULL,
Salary real NULL,
DeptId varchar(50) NULL,
Gender varchar(50) NULL,
Ethnicity varchar(50) NULL,
Prob real NULL);

Create table IF NOT EXISTS COMP_TAB
(JobTitle varchar(50) NULL,
Salary real NULL,
"Year Hired" varchar(50) NULL,
DeptId varchar(50) NULL,
Prob real NULL);

Create table IF NOT EXISTS UHEQTY_TAB
(SCode varchar(50) NULL,
JobTitle varchar(50) NULL,
DeptId varchar(50) NULL,
Weight REAL NULL,
DeptDesc varchar(50) NULL);

Create table IF NOT EXISTS T1EQTY_TAB
(Salary REAL NULL,
JobTitle varchar(50) NULL,
SCode varchar(50) NULL,
DeptDesc varchar(50) NULL);

CREATE TABLE IF NOT EXISTS STATS
(
ResultType varchar(50),
ResultName varchar(50),
ResultValue decimal(18,2)
);

CREATE TABLE IF NOT EXISTS COMPSTATS
(
DeptID varchar(50),
JobTitle varchar(50),
ActualMed REAL null,
AdjMed REAL null,
Ratio REAL Null
);

CREATE TABLE IF NOT EXISTS UHSTATS
(
DeptID varchar(50),
JobTitle varchar(50),
Median REAL null,
Mean REAL null,
Q1 REAL Null,
Q3 Real Null
);

CREATE TABLE IF NOT EXISTS IEQTYSTATS
(
DeptID varchar(50),
Ratio Formula varchar(50),
Value1 REAL null,
Value2 REAL null,
Result REAL Null
);

CREATE TABLE IF NOT EXISTS EEQTYSTATS
(
DeptID varchar(50),
Ratio Formula varchar(50),
Value1 REAL null,
Value2 REAL null,
Result REAL Null
);