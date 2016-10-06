--Calculating Adjusted Median for Associate Professor
INSERT INTO STATS
SELECT 'COMP','AdjMedAssocP',
((((SELECT ResultValue FROM STATS WHERE resultName='Median' and resulttype='COMP')
*@k)+@associate)*@dla);

--Calculating Adjusted Median for Full Proffessor
insert into STATS
Select 'COMP','AdjMedFull',
(((select ResultValue from stats where resultName='AdjMedAssocP' and resulttype='COMP')+@Full)*@dlf);