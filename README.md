# LogQuery
A tool that can parse logs and query them using SQL.

LogQuery can parse, structure and insert logs into SQL tables. It create and structure the Database according to Regular-Expression groups. Each Regular-Expression will create a table and each group in that regular expression match will create a column in that table. The matched log liness will be structured and inserted to their corresponding SQL table.
