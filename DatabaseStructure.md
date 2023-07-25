# 数据库结构

## users.db
| 字段名 | 类型 | 说明 |	备注 |
| :---: | :---: | :---: | :---: |
| id | int | 用户id | 主键，自增 |
| username | varchar(32) | 用户名 | 唯一 |
| nickname | varchar(32) | 昵称 |  |
| passwordHash | varchar(128) | 密码哈希 |  |
| passwordSalt | varchar(32) | 密码盐 |  |