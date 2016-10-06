INSERT INTO {0}
select @dept,'ASSOCIATE/ASSISTANT',printf('%.2f',(select resultvalue from stats where resulttype='{1}' and resultname='ASSOCIATE PROFESSOR')),printf('%.2f',(select resultvalue from stats where resulttype='{1}' and resultname='ASSISTANT PROFESSOR')),0.00;

INSERT INTO {0}
select @dept,'FULL/ASSOCIATE',printf('%.2f',(select resultvalue from stats where resulttype='{1}' and resultname='PROFESSOR')),printf('%.2f',(select resultvalue from stats where resulttype='{1}' and resultname='ASSOCIATE PROFESSOR')),0.00;

INSERT INTO {0}
select @dept,'ASSOCIATE/NEW ASSISTANT',printf('%.2f',(select resultvalue from stats where resulttype='{1}' and resultname='ASSOCIATE PROFESSOR')),printf('%.2f',(select resultvalue from stats where resulttype='{1}' and resultname='NEW ASSISTANT')),0.00;

INSERT INTO {0}
select @dept,'FULL/NEW ASSISTANT',printf('%.2f',(select resultvalue from stats where resulttype='{1}' and resultname='PROFESSOR')),printf('%.2f',(select resultvalue from stats where resulttype='{1}' and resultname='NEW ASSISTANT')),0.00;

UPDATE {0}
SET RESULT=ROUND(VALUE1/VALUE2,2);