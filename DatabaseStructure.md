# 数据库结构

## data.db
Table: User
| 字段名 | 类型 | 说明 |	备注 |
| :---: | :---: | :---: | :---: |
| ID | int | 用户 ID | 主键，自增 |
| Name | varchar(32) | 用户名 | 唯一，非空 |
| Nick | varchar(32) | 昵称 |  |
| Hash | blob | 密码哈希 | 非空 |
| Salt | blob | 密码盐 | 非空 |

``