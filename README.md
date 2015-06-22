# LogQuery
A tool that can parse logs and query them using SQL.

(work is in progress)
LogQuery can parse, structure and insert logs into SQL tables. It create and structure the Database according to Regular-Expression groups. Each Regular-Expression will create a table and each group in that regular expression match will create a column in that table. The matched log lines will be structured and inserted to their corresponding SQL table.

The Regular-Expressions and their groups are configured using XML files. Each XML file contains a colletion of Regular-Expressions. Each collection has a name that will be used as a namespace for tables that its Regular-Expressions corresponds to.

LogQuery currently only works with SQL Server/LocalDBs. once the database is structured you can connect to it using any IDE (VS, SSMS, etc...).
