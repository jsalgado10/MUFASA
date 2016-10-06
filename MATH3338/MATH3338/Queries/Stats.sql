--DROP TABLES STATS AND VAR
DROP TABLE IF EXISTS VARS;
DROP TABLE IF EXISTS RPTAVG;
DROP TABLE IF EXISTS TABLETEMP;

--RECREATE TABLES BEFORE CALCULATING STATS
CREATE TABLE IF NOT EXISTS STATS
(
ResultType varchar(50),
ResultName varchar(50),
ResultValue decimal(18,2)
);

CREATE TEMP TABLE IF NOT EXISTS VARS
(
NAME TEXT PRIMARY KEY,
VAL INTEGER
);

--ADD COUNT FOR CALCULATIONS
INSERT INTO VARS(NAME,VAL)
VALUES("1Q",(SELECT COUNT(*) FROM RPT_TAB)/2),
("2Q",(SELECT COUNT(*) FROM RPT_TAB)),
("3Q",3*(SELECT COUNT(*) FROM RPT_TAB)/2); 

--media
INSERT INTO STATS
SELECT @type,"Median" as name,AVG(1.0*SALARY) as value
FROM (SELECT SALARY
      FROM RPT_TAB
      ORDER BY SALARY
      LIMIT 2 - (SELECT VAL FROM VARS WHERE NAME="2Q") % 2
      OFFSET ((SELECT VAL FROM VARS WHERE NAME="2Q") - 1) / 2) AS rptdata;
--1p values
INSERT INTO STATS
SELECT @type,"1st Quartile" as name,AVG(1.0*SALARY) as value
FROM (SELECT SALARY
      FROM RPT_TAB
      ORDER BY SALARY
      LIMIT 2 - (SELECT VAL FROM VARS WHERE NAME="1Q") % 2
      OFFSET ((SELECT VAL FROM VARS WHERE NAME="1Q") - 1) / 2) AS rptdata;

--3p
INSERT INTO STATS
SELECT @type,"3rd Quartile" as name,AVG(1.0*SALARY) as value
FROM (SELECT SALARY
      FROM RPT_TAB
      ORDER BY SALARY
      LIMIT 2 - (SELECT VAL FROM VARS WHERE NAME="3Q") % 2
      OFFSET ((SELECT VAL FROM VARS WHERE NAME="3Q") - 1) / 2) AS rptdata;




create temp table IF NOT EXISTS TABLETEMP AS
select salary,count(salary) as countsal
from RPT_TAB
group by SALARY;

--Table use to store the new prob


create table if not exists RPTAVG as 
select rpt.salary,(rpt.salary*tmp.countsal) AS PROB
from rpt_tab rpt,tabletemp tmp
where tmp.salary=rpt.salary;

--update rpt_tab prob column
update RPT_TAB
set prob=(SELECT ravg.prob from RPTAVG ravg where rpt_tab.Salary=ravg.salary);

--UPDATE STAST TABLE AND ADD MEAN
insert into STATS
select @type, "Mean",sum(prob)/(select count(salary) from RPT_TAB) from RPT_TAB