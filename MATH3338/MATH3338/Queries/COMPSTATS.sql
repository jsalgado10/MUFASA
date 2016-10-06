insert into COmPSTATS
select @dept,@job,printf('%.2f',(select resultvalue from stats where resultname=@ActMed)) ,ROUND((select resultvalue from stats where resultname=@AdjMed),2),0.00;

update comPSTATS
set ratio=ROUND((ActualMed/AdjMed),2);