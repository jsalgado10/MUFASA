DROP TABLE IF EXISTS VARS;
DROP TABLE IF EXISTS RPTAVG;
DROP TABLE IF EXISTS TABLETEMP;

CREATE TEMP TABLE IF NOT EXISTS VARS
(
NAME TEXT PRIMARY KEY,
VAL INTEGER
);

INSERT INTO VARS(NAME,VAL)
VALUES("1P",(SELECT COUNT(*) FROM TEMPRPT)/2),
("2P",(SELECT COUNT(*) FROM TEMPRPT)),
("3P",3*(SELECT COUNT(*) FROM TEMPRPT)/2); 

create temp table IF NOT EXISTS TABLETEMP AS
select salary,count(salary) as countsal
from TEMPRPT
group by SALARY;

--Table use to store the new prob
create table if not exists RPTAVG as 
select rpt.salary,(rpt.salary*tmp.countsal) AS PROB
from TEMPRPT rpt,tabletemp tmp
where tmp.salary=rpt.salary;

--update TEMPRPT prob column
update TEMPRPT
set prob=(SELECT ravg.prob from RPTAVG ravg where TEMPRPT.Salary=ravg.salary);

--UPDATE STAST TABLE AND ADD MEAN
insert into STATS
select @type, @name,sum(prob)/(select count(salary) from TEMPRPT) from TEMPRPT