# 数据库结构或数据信息

## data.db
Table: User [结构]
| 字段名 | 类型 | 说明 |	备注 |
| :---: | :---: | :---: | :---: |
| ID | int | 用户 ID | 主键，自增 |
| Name | varchar(32) | 用户名 | 唯一，非空 |
| Nick | varchar(32) | 昵称 |  |
| Hash | blob | 密码哈希 | 非空 |
| Salt | blob | 密码盐 | 非空 |

Table: AppInfo [信息]
| 键 | 实际应用类型 | 说明 | 备注 |
| :---: | :---: | :---: | :---: |
| Version | Version | 版本号 | 用于比对应用程序版本 |