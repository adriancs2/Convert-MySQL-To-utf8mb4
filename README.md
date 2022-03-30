# Convert-MySQL-To-utf8mb4

A C# console program that will convert default character set of all database and tables to utf8mb4 and collation of utf8mb4_general_ci.

<b>utf8mb4</b> is the default character set started in MySQL 8. It has the best support for all the language characters of the world, including emoji characters etc. Thus, using utf8mb4 has the best compatible if your application uses variety of unicode characters. This program will be useful when you need to perform a batch conversion throughout all the databases and tables from old projects at once.

This program uses MySqlConnector (MIT) as connector to MySQL.

- https://github.com/mysql-net/MySqlConnector
- https://www.nuget.org/packages/MySqlConnector

The idea is basically get the list of databases, then loop through the database and get the tables. Perform a loop on all tables and check it's default character set, if it is not utf8mb4, then convert it one by one.

First, obtain the list of database:
```
SELECT * FROM information_schema.SCHEMATA;
```
In C#,
```
using (MySqlConnection conn = new MySqlConnection(constr))
{
    using (MySqlCommand cmd = new MySqlCommand())
    {
        conn.Open();
        cmd.Connection = conn;

        DataTable dtDatabase = new DataTable();

        cmd.CommandText = "SELECT * FROM information_schema.SCHEMATA;";
        MySqlDataAdapter da1 = new MySqlDataAdapter(cmd);
        da1.Fill(dtDatabase);

        conn.Close();
    }
}
```
This will return a table that has the following column details:

- SCHEMA_NAME
- DEFAULT_CHARACTER_SET_NAME
- DEFAULT_COLLATION_NAME

Here, character set can be checked. If it is not set to <b>utf8mb4</b>, then it will be modified.

But first, the following databases are needed to be ignored, as they are MySQL readonly specific info:

- information_schema
- mysql
- performance_schema
- sys

The SQL statement for conversion of database character set:
```
ALTER DATABASE `{database name}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
```
In C#,
```
foreach (DataRow dr in dtDatabase.Rows)
{
    string database = dr["SCHEMA_NAME"] + "";

    // ignore
    switch (database)
    {
        case "information_schema":
        case "myql":
        case "performance_schema":
        case "sys":
            continue;
    }

    string db_charset = dr["DEFAULT_CHARACTER_SET_NAME"] + "";
    string db_collation = dr["DEFAULT_COLLATION_NAME"] + "";

    if (db_charset == "utf8mb4" && db_collation == "utf8mb4_general_ci")
    {
        // do nothing
    }
    else
    {
        cmd.CommandText = $"ALTER DATABASE `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
        cmd.ExecuteNonQuery();
    }
}
```
Next, is to obtain the list of tables with the following SQL statement:
```
show table status;
```
In C#,
```
cmd.CommandText = $"use `{database}`";
cmd.ExecuteNonQuery();

DataTable dtTables = new DataTable();

cmd.CommandText = "show table status;";
MySqlDataAdapter da2 = new MySqlDataAdapter(cmd);
da2.Fill(dtTables);
```
Then, loop through each table to obtain the following column value:

- <b>Name</b> = the table's name
- <b>Collation</b> = the collation character set
- 
The SQL statement for conversion of table's character set:
```
ALTER TABLE `{tablename}` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
```
In C#,
```
foreach (DataRow dr2 in dtTables.Rows)
{
    string tablename = dr2["Name"] + "";
    string tableCollation = dr2["Collation"] + "";

    if (tableCollation != "utf8mb4_general_ci")
    {
        try
        {
            cmd.CommandText = $"ALTER TABLE `{tablename}` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            // log the error
        }
    }
}
```
