/*若表不存在则创建*/
if not exists (select * from sysobjects where name='Base_UnitTest' and xtype='U')
   create table dbo.Base_UnitTest (
   Id                   varchar(50)          not null,
   UserId               varchar(50)          null,
   UserName             varchar(50)          null,
   Age                  int                  null,
   constraint PK_Base_UnitTest primary key (Id)
)


if exists (select 1 from  sys.extended_properties
           where major_id = object_id('dbo.Base_UnitTest') and minor_id = 0)
begin 
   execute sp_dropextendedproperty 'MS_Description',  
   'user', 'dbo', 'table', 'Base_UnitTest' 
 
end 


execute sp_addextendedproperty 'MS_Description',  
   '单元测试表', 
   'user', 'dbo', 'table', 'Base_UnitTest'


if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.Base_UnitTest')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Id')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'Base_UnitTest', 'column', 'Id'

end


execute sp_addextendedproperty 'MS_Description', 
   '自然主键',
   'user', 'dbo', 'table', 'Base_UnitTest', 'column', 'Id'


if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.Base_UnitTest')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'UserId')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'Base_UnitTest', 'column', 'UserId'

end


execute sp_addextendedproperty 'MS_Description', 
   '用户Id',
   'user', 'dbo', 'table', 'Base_UnitTest', 'column', 'UserId'


if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.Base_UnitTest')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'UserName')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'Base_UnitTest', 'column', 'UserName'

end


execute sp_addextendedproperty 'MS_Description', 
   '用户名',
   'user', 'dbo', 'table', 'Base_UnitTest', 'column', 'UserName'


if exists(select 1 from sys.extended_properties p where
      p.major_id = object_id('dbo.Base_UnitTest')
  and p.minor_id = (select c.column_id from sys.columns c where c.object_id = p.major_id and c.name = 'Age')
)
begin
   execute sp_dropextendedproperty 'MS_Description', 
   'user', 'dbo', 'table', 'Base_UnitTest', 'column', 'Age'

end


execute sp_addextendedproperty 'MS_Description', 
   '年龄',
   'user', 'dbo', 'table', 'Base_UnitTest', 'column', 'Age'

