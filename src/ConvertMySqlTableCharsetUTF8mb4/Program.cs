using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using System.Data;
using System.IO;

namespace ConvertMySqlTableCharsetUTF8mb4
{
    internal class Program
    {
        static DateTime datestart = DateTime.MinValue;
        static DateTime dateend = DateTime.MinValue;

        static void Main(string[] args)
        {
            try
            {
                run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            dateend = DateTime.Now;

            TimeSpan timeUsed = dateend - datestart;

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine($"Begin Time : {datestart.ToString("yyyy-MM-dd HH:mm:ss")}");
            Console.WriteLine($"End Time   : {dateend.ToString("yyyy-MM-dd HH:mm:ss")}");
            Console.WriteLine($"Total Time : {timeUsed.Days} d {timeUsed.Hours} h {timeUsed.Minutes} m {timeUsed.Seconds} s {timeUsed.Milliseconds} ms");

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("all completed");
            Console.WriteLine("press any key to exit...");

            Console.ReadKey();
        }

        static void run()
        {
            Console.WriteLine("Enter MySQL Connection String: (server=127.0.0.1;user=<username>;pwd=<pwd>;)");
            Console.WriteLine();

            string constr = Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine();

            datestart = DateTime.Now;

            Console.WriteLine("Task begin...");

            Console.WriteLine();
            Console.WriteLine();

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

                    foreach (DataRow dr in dtDatabase.Rows)
                    {
                        string database = dr["SCHEMA_NAME"] + "";

                        switch (database)
                        {
                            case "information_schema":
                            case "mysql":
                            case "performance_schema":
                            case "sys":
                                continue;
                        }

                        string db_charset = dr["DEFAULT_CHARACTER_SET_NAME"] + "";
                        string db_collation = dr["DEFAULT_COLLATION_NAME"] + "";

                        Console.WriteLine();
                        Console.WriteLine();

                        Console.WriteLine($"Handling database: {database}");
                        Console.WriteLine();

                        if (db_charset == "utf8mb4" && db_collation == "utf8mb4_general_ci")
                        {
                            Console.WriteLine("Database Conversion is not needed");
                        }
                        else
                        {
                            cmd.CommandText = $"ALTER DATABASE `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
                            cmd.ExecuteNonQuery();
                            Console.WriteLine("Database Charset converted.");
                        }

                        Console.WriteLine("...obtaining table list...");

                        cmd.CommandText = $"use `{database}`";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "show table status;";

                        DataTable dtTables = new DataTable();

                        MySqlDataAdapter da2 = new MySqlDataAdapter(cmd);
                        da2.Fill(dtTables);

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
                                    Console.WriteLine($"{tablename}: converted success!");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"{tablename}: Failed! {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"{tablename}: skipped...");
                            }
                        }
                    }

                    conn.Close();
                }
            }
        }
    }
}
