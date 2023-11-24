using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Security.Cryptography;

namespace lab1_dotnet_framework
{
    internal class DataBase
    {

        private List<string> columnNamesTest = new List<string> {"id", "x", "v", "v2", "v_v2", "loc_prec", "h", "c1", "c2", "u", "u_v" };
        private List<string> columnNamesMain1 = new List<string> { "id", "x", "v", "v2", "v_v2", "loc_prec", "h", "c1", "c2" };
        private List<string> columnNamesMain2 = new List<string> { "u2", "id", "x", "v", "v2", "v_v2", "loc_prec", "h", "c1", "c2" };
        private string columnNamesTestString = "";
        private string columnNamesMain1String = "";
        private string columnNamesMain2String = "";
        
        string BaseDirectory { get; }

        public DataBase(string directory)
        {
            BaseDirectory = directory;

            for (int i = 0; i < columnNamesTest.Count; i++)
            {
                columnNamesTestString += columnNamesTest[i];

                if (i != columnNamesTest.Count - 1)
                    columnNamesTestString += ", ";
            }

            for (int i = 0; i < columnNamesMain1.Count; i++)
            {
                columnNamesMain1String += columnNamesMain1[i];

                if (i != columnNamesMain1.Count - 1)
                    columnNamesMain1String += ", ";
            }

            for (int i = 0; i < columnNamesMain2.Count; i++)
            {
                columnNamesMain2String += columnNamesMain2[i];

                if (i != columnNamesMain2.Count - 1)
                    columnNamesMain2String += ", ";
            }

        }

        private SqliteConnection GetConnection()
        {
            string connectionString = "Data Source=" + Directory.GetCurrentDirectory() + BaseDirectory;

            SqliteConnection connection = null;

            try
            {
                connection = new SqliteConnection(connectionString);

                connection.Open();
            }
            catch
            {
                throw new Exception("База данных не найдена");
            }

            return connection;
        }

        public List<List<string>> GetAllStartConditions(string table)
        {
            SqliteConnection connection = GetConnection();

            SqliteCommand command = connection.CreateCommand();

            if (table != "main2") 
                command.CommandText =   "select distinct x0, u0 from " + table + ";";
            else
                command.CommandText = "select distinct x0, u0, u0der from " + table + ";";

            SqliteDataReader reader = command.ExecuteReader();

            List<List<string>> allConditions =  new List<List<string>>();

            while (reader.Read()) {
                List<string> currentRow = new List<string>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    currentRow.Add(reader.GetString(i));
                }

                allConditions.Add(currentRow);
            }

            connection.Close();

            return allConditions;
        }
        

        public List<List<string>> GetDataForStartCondition(string table, List<string> startCondition)
        {
            SqliteConnection connection = GetConnection();

            SqliteCommand command = connection.CreateCommand();

            string columnNamesString;

            switch (table)
            {
                case "test": columnNamesString = columnNamesTestString; break;
                case "main1": columnNamesString = columnNamesMain1String; break;
                default: columnNamesString = columnNamesMain2String; break;
            }

            string query = command.CommandText = "select " + columnNamesString + " from " + table + " where x0 = "
                    + startCondition[0].Replace(",", ".") + " and u0 = " + startCondition[1].Replace(",", ".");

            if (table == "main2" || table == "main2der")
                query += " and u0der = " + startCondition[2].Replace(",", ".");

            query += ";";


            SqliteDataReader reader =  command.ExecuteReader();

            List<List<string>> data = new List<List<string>>(); 

            while (reader.Read())
            {
                List<string> current  = new List<string>(); 


                for (int i = 0; i < reader.FieldCount; i++)
                {
                    current.Add(reader.GetString(i));   
                }

                data.Add(current);  
            }

            connection.Close();

            return data;
        }

        public void SaveParameters(List<string> parameters, string table)
        {
            SqliteConnection connection = GetConnection();

            SqliteCommand command = connection.CreateCommand();

            string sourceQuery = " from pars where tablename = " + "'" + table + "'" + " and x0 = " + parameters[1]
                + " and u0 = " + parameters[2] + " and u0der = " + parameters[3] + ";";

            command.CommandText = "delete " + sourceQuery;

            command.ExecuteNonQuery();

            command.CommandText = "insert into pars" + " values(" + "'" + table + "', ";

            for (int i = 0; i < parameters.Count; i++) 
            {
                command.CommandText += parameters[i];
                if (i != parameters.Count - 1)
                {
                    command.CommandText += ", ";
                }
            }

            command.CommandText += ");";

            command.ExecuteNonQuery();

            connection.Close();
        }

        public List<string> GetParameters(string table, List<string> startCondition)
        {
            SqliteConnection connection = GetConnection();
            SqliteCommand command = connection.CreateCommand();

            string sourceQuery = " from pars where tablename = " + "'" + table + "'" + " and x0 = " + startCondition[0]
                + " and u0 = " + startCondition[1] + " and u0der = " + startCondition[2] + ";";

            command.CommandText = "select * " + sourceQuery;

            SqliteDataReader reader = command.ExecuteReader();
            reader.Read();

            List<string> parameters = new List<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                parameters.Add(reader.GetString(i));
            }

            connection.Close();

            return parameters;  
        }
    }
}
    
