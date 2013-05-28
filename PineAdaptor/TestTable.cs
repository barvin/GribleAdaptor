using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace PineAdaptor
{
    /// <summary>
    /// Class that represents Test Table entity from Pine.
    /// </summary>
    public class TestTable
    {
        private string tableName;
        private string productName;

        public TestTable(string name)
        {
            this.tableName = name;
            this.productName = PineSettings.ProductName;
        }

        private static NpgsqlConnection Connection { get; set; }

        /// <summary>
        /// Retrieves data from Preconditions sheet.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetPreconditionsTable()
        {
            return GetOneRowTable("precondition");
        }

        /// <summary>
        /// Retrieves data from Postconditions sheet.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetPostconditionsTable()
        {
            return GetOneRowTable("postcondition");
        }

        private Dictionary<string, string> GetOneRowTable(string subTableType)
        {
            var result = new Dictionary<string, string>();

            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            try
            {
                NpgsqlConnection conn = GetConnection();
                conn.Open();

                string sql = ("SELECT k.name, v.value FROM keys as k JOIN values as v "
                    + "ON v.keyid=k.id AND k.tableid =(SELECT id FROM tables WHERE type="
                    + "(SELECT id FROM tabletypes WHERE name='" + subTableType + "') AND parentid="
                    + "(SELECT id FROM tables WHERE name='" + tableName + "' AND categoryid IN "
                    + "(SELECT id FROM categories WHERE productid=" + "(SELECT id FROM products WHERE name='"
                    + productName + "') AND type=(SELECT id FROM tabletypes WHERE name='table'))))");

                var da = new NpgsqlDataAdapter(sql, conn);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];

                foreach (DataRow row in dt.Rows)
                {
                    string keyId = (string)row.ItemArray.GetValue(0);
                    string value = (string)row.ItemArray.GetValue(1);
                    result.Add(keyId, value);
                }
                conn.Close();
            }
            catch (Exception e)
            {
                PineSettings.ErHandler.OnAdaptorFail(e);
            }
            return result;
        }

        /// <summary>
        /// Retrieves data from General sheet.
        /// </summary>
        /// <returns>List of Dictionary<ParameterName, ParameterValue>.</returns>
        public List<Dictionary<string, string>> GetGeneralTable()
        {
            return GetValuesFromPine("table");
        }

        internal List<Dictionary<string, string>> GetDataStorageValues()
        {
            return GetValuesFromPine("storage");
        }

        internal Dictionary<int, Dictionary<string, string>> GetDataStorageValues(int[] iterationNumbers)
        {
            return GetValuesFromPine(iterationNumbers);
        }

        private List<Dictionary<string, string>> GetValuesFromPine(string entityType)
        {
            var result = new List<Dictionary<string, string>>();
            try
            {
                NpgsqlConnection conn = GetConnection();
                conn.Open();

                string nameColumn = "";
                if (entityType.Equals("table"))
                {
                    nameColumn = "name";
                }
                else
                {
                    nameColumn = "classname";
                }

                DataSet ds = new DataSet();
                DataTable dt = new DataTable();

                var keys = new Dictionary<int, string>();
                string sql = "SELECT id, name FROM keys WHERE tableid="
                    + "(SELECT id FROM tables WHERE " + nameColumn + "='" + tableName + "' AND type="
                    + "(SELECT id FROM tabletypes WHERE name='" + entityType + "') AND categoryid IN "
                    + "(SELECT id FROM categories WHERE productid=(SELECT id FROM products WHERE name='" + productName
                    + "')))";
                var da = new NpgsqlDataAdapter(sql, conn);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                foreach (DataRow row in dt.Rows)
                {
                    int id = (int)row.ItemArray.GetValue(0);
                    string name = (string)row.ItemArray.GetValue(1);
                    keys.Add(id, name);
                }

                var rowIds = new List<int>();
                sql = "SELECT id FROM rows WHERE tableid=" + "(SELECT id FROM tables WHERE " + nameColumn
                    + "='" + tableName + "' AND type=" + "(SELECT id FROM tabletypes WHERE name='" + entityType
                    + "') AND categoryid IN " + "(SELECT id FROM categories WHERE productid="
                    + "(SELECT id FROM products WHERE name='" + productName + "'))) ORDER BY \"order\"";
                da = new NpgsqlDataAdapter(sql, conn);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                foreach (DataRow row in dt.Rows)
                {
                    int id = (int)row.ItemArray.GetValue(0);
                    rowIds.Add(id);
                }

                for (int i = 0; i < rowIds.Count; i++)
                {
                    var iterRow = new Dictionary<string, string>();

                    sql = "SELECT keyid, value FROM values WHERE rowid=" + rowIds[i];
                    da = new NpgsqlDataAdapter(sql, conn);
                    ds.Reset();
                    da.Fill(ds);
                    dt = ds.Tables[0];
                    foreach (DataRow row in dt.Rows)
                    {
                        int keyId = (int)row.ItemArray.GetValue(0);
                        string value = (string)row.ItemArray.GetValue(1);
                        iterRow.Add(keys[keyId], value);
                    }
                    result.Add(iterRow);
                }

                conn.Close();
            }
            catch (Exception e)
            {
                PineSettings.ErHandler.OnAdaptorFail(e);
            }
            if (result.Count == 0)
            {
                PineSettings.ErHandler.OnAdaptorFail(new Exception("Pine error: " + entityType + " '" + tableName + "' is missing."));
            }
            return result;
        }

        private Dictionary<int, Dictionary<string, string>> GetValuesFromPine(int[] iterationNumbers)
        {
            var result = new Dictionary<int, Dictionary<string, string>>();
            try
            {
                NpgsqlConnection conn = GetConnection();
                conn.Open();
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();

                var keys = new Dictionary<int, string>();
                string sql = "SELECT id, name FROM keys WHERE tableid="
                        + "(SELECT id FROM tables WHERE classname='" + tableName + "' AND type="
                        + "(SELECT id FROM tabletypes WHERE name='storage') AND categoryid IN "
                        + "(SELECT id FROM categories WHERE productid=(SELECT id FROM products WHERE name='" + productName
                        + "')))";
                var da = new NpgsqlDataAdapter(sql, conn);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                foreach (DataRow row in dt.Rows)
                {
                    int id = (int)row.ItemArray.GetValue(0);
                    string name = (string)row.ItemArray.GetValue(1);
                    keys.Add(id, name);
                }

                var rowNumbersAndIds = new Dictionary<int, int>();
                sql = "SELECT id, \"order\" FROM rows WHERE tableid="
                        + "(SELECT id FROM tables WHERE classname='" + tableName
                        + "' AND type=(SELECT id FROM tabletypes WHERE name='storage') AND categoryid IN "
                        + "(SELECT id FROM categories WHERE productid=(SELECT id FROM products WHERE name='" + productName
                        + "'))) AND \"order\" IN (" + String.Join(",", iterationNumbers) + ") ORDER BY \"order\"";
                da = new NpgsqlDataAdapter(sql, conn);
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];
                foreach (DataRow row in dt.Rows)
                {
                    int id = (int)row.ItemArray.GetValue(0);
                    int order = (int)row.ItemArray.GetValue(1);
                    rowNumbersAndIds.Add(id, order);
                }

                foreach (int rowId in rowNumbersAndIds.Keys)
                {
                    var iterRow = new Dictionary<string, string>();
                    sql = "SELECT keyid, value FROM values WHERE rowid=" + rowId;
                    da = new NpgsqlDataAdapter(sql, conn);
                    ds.Reset();
                    da.Fill(ds);
                    dt = ds.Tables[0];
                    foreach (DataRow row in dt.Rows)
                    {
                        int keyId = (int)row.ItemArray.GetValue(0);
                        string value = (string)row.ItemArray.GetValue(1);
                        iterRow.Add(keys[keyId], value);
                    }
                    result.Add(rowNumbersAndIds[rowId], iterRow);
                }

                conn.Close();
            }
            catch (Exception e)
            {
                PineSettings.ErHandler.OnAdaptorFail(e);
            }
            return result;
        }

        private NpgsqlConnection GetConnection()
        {
            if (Connection == null)
            {
                string dbhost = PineSettings.Dbhost;
                string dbport = PineSettings.Dbport;
                string dbName = PineSettings.Dbname;
                string dblogin = PineSettings.Dblogin;
                string dbpswd = PineSettings.Dbpswd;

                string connstring = String.Format("Server={0};Port={1};" +
                                "User Id={2};Password={3};Database={4};",
                                dbhost, dbport, dblogin, dbpswd, dbName);

                Connection = new NpgsqlConnection(connstring);
            }
            return Connection;
        }

    }
}
