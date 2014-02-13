using System.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using GribleAdaptor.Helpers;
using GribleAdaptor.Json;

namespace GribleAdaptor
{
    /// <summary>
    /// Class that represents Test Table entity from Grible.
    /// </summary>
    public class TestTable
    {
        private readonly string _tableName;
        private readonly string _productName;
        private readonly string _productPath;

        public TestTable(string name)
        {
            _tableName = name;
            _productName = GribleSettings.ProductName;
            _productPath = GribleSettings.ProductPath;
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

            try
            {
                if (GribleSettings.AppType == AppTypes.POSTGRESQL)
                {
                    var ds = new DataSet();

                    var conn = GetConnection();
                    conn.Open();

                    var sql = ("SELECT k.name, v.value FROM keys as k JOIN values as v "
                        + "ON v.keyid=k.id AND k.tableid =(SELECT id FROM tables WHERE type="
                        + "(SELECT id FROM tabletypes WHERE name='" + subTableType + "') AND parentid="
                        + "(SELECT id FROM tables WHERE name='" + _tableName + "' AND categoryid IN "
                        + "(SELECT id FROM categories WHERE productid=" + "(SELECT id FROM products WHERE name='"
                        + _productName + "') AND type=(SELECT id FROM tabletypes WHERE name='table'))))");

                    var da = new NpgsqlDataAdapter(sql, conn);
                    ds.Reset();
                    da.Fill(ds);
                    var dt = ds.Tables[0];

                    foreach (DataRow row in dt.Rows)
                    {
                        var keyId = (string)row.ItemArray.GetValue(0);
                        var value = (string)row.ItemArray.GetValue(1);
                        result.Add(keyId, value);
                    }
                    conn.Close();
                }
                else
                {
                    string fileName = _tableName + "_" + subTableType.ToUpper() + ".json";
                    string filePath = IOHelper.SearchFile(_productPath + "\\" + "TestTables", fileName);
                    if (filePath == null)
                    {
                        throw new Exception("File '" + fileName + "' not found in directory '" + _productPath + "\\" + "TestTables" + "'.");
                    }
                    TableJson tableJson = IOHelper.ParseTableJson(filePath);
                    KeyJson[] keys = tableJson.Keys;
                    string[][] values = tableJson.Values;
                    for (int j = 0; j < values[0].Length; j++)
                    {
                        result.Add(keys[j].Name, values[0][j]);
                    }
                }
            }
            catch (Exception e)
            {
                GribleSettings.ErHandler.OnAdaptorFail(e);
            }
            return result;
        }

        /// <summary>
        /// Retrieves data from General sheet.
        /// </summary>
        /// <returns>List of Dictionary&lt;ParameterName, ParameterValue&gt;.</returns>
        public List<Dictionary<string, string>> GetGeneralTable()
        {
            return GetValuesFromGrible("table");
        }

        internal List<Dictionary<string, string>> GetDataStorageValues()
        {
            return GetValuesFromGrible("storage");
        }

        internal Dictionary<int, Dictionary<string, string>> GetDataStorageValues(int[] iterationNumbers)
        {
            return GetValuesFromGrible(iterationNumbers);
        }

        private List<Dictionary<string, string>> GetValuesFromGrible(string entityType)
        {
            var result = new List<Dictionary<string, string>>();
            try
            {
                if (GribleSettings.AppType == AppTypes.POSTGRESQL)
                {
                    var conn = GetConnection();
                    conn.Open();

                    var nameColumn = entityType.Equals("table") ? "name" : "classname";

                    var ds = new DataSet();

                    var keys = new Dictionary<int, string>();
                    var sql = "SELECT id, name FROM keys WHERE tableid="
                        + "(SELECT id FROM tables WHERE " + nameColumn + "='" + _tableName + "' AND type="
                        + "(SELECT id FROM tabletypes WHERE name='" + entityType + "') AND categoryid IN "
                        + "(SELECT id FROM categories WHERE productid=(SELECT id FROM products WHERE name='" + _productName
                        + "')))";
                    var da = new NpgsqlDataAdapter(sql, conn);
                    ds.Reset();
                    da.Fill(ds);
                    var dt = ds.Tables[0];
                    foreach (DataRow row in dt.Rows)
                    {
                        var id = (int)row.ItemArray.GetValue(0);
                        var name = (string)row.ItemArray.GetValue(1);
                        keys.Add(id, name);
                    }

                    sql = "SELECT id FROM rows WHERE tableid=" + "(SELECT id FROM tables WHERE " + nameColumn
                        + "='" + _tableName + "' AND type=" + "(SELECT id FROM tabletypes WHERE name='" + entityType
                        + "') AND categoryid IN " + "(SELECT id FROM categories WHERE productid="
                        + "(SELECT id FROM products WHERE name='" + _productName + "'))) ORDER BY \"order\"";
                    da = new NpgsqlDataAdapter(sql, conn);
                    ds.Reset();
                    da.Fill(ds);
                    dt = ds.Tables[0];
                    var rowIds = (from DataRow row in dt.Rows select (int)row.ItemArray.GetValue(0)).ToList();

                    foreach (var rowId in rowIds)
                    {
                        var iterRow = new Dictionary<string, string>();

                        sql = "SELECT keyid, value FROM values WHERE rowid=" + rowId;
                        da = new NpgsqlDataAdapter(sql, conn);
                        ds.Reset();
                        da.Fill(ds);
                        dt = ds.Tables[0];
                        foreach (DataRow row in dt.Rows)
                        {
                            var keyId = (int)row.ItemArray.GetValue(0);
                            var value = (string)row.ItemArray.GetValue(1);
                            iterRow.Add(keys[keyId], value);
                        }
                        result.Add(iterRow);
                    }

                    conn.Close();
                }
                else
                {
                    string filePath = null;

                    if (entityType.Equals("table"))
                    {
                        string fileName = _tableName + ".json";
                        string sectionDir = "TestTables";
                        filePath = IOHelper.SearchFile(_productPath + "\\" + sectionDir, fileName);
                        if (filePath == null)
                        {
                            throw new Exception("File '" + fileName + "' not found in directory '"
                                    + _productPath + "\\" + sectionDir + "'.");
                        }
                    }
                    else
                    {
                        string className = _tableName;
                        string sectionDir = "DataStorages";
                        filePath = IOHelper.SearchFileByClassName(_productPath + "\\" + sectionDir, className);
                        if (filePath == null)
                        {
                            throw new Exception("File with class name '" + className + "' not found in directory '"
                                    + _productPath + "\\" + sectionDir + "'.");
                        }
                    }
                    TableJson tableJson = IOHelper.ParseTableJson(filePath);
                    KeyJson[] keys = tableJson.Keys;
                    string[][] values = tableJson.Values;
                    for (int i = 0; i < values.Length; i++)
                    {
                        var row = new Dictionary<string, string>();
                        for (int j = 0; j < values[0].Length; j++)
                        {
                            row.Add(keys[j].Name, values[i][j]);
                        }
                        result.Add(row);
                    }
                }
            }
            catch (Exception e)
            {
                GribleSettings.ErHandler.OnAdaptorFail(e);
            }
            if (result.Count == 0)
            {
                GribleSettings.ErHandler.OnAdaptorFail(new Exception("Grible error: " + entityType + " '" + _tableName + "' is missing."));
            }
            return result;
        }

        private Dictionary<int, Dictionary<string, string>> GetValuesFromGrible(IEnumerable<int> iterationNumbers)
        {
            var result = new Dictionary<int, Dictionary<string, string>>();
            try
            {
                if (GribleSettings.AppType == AppTypes.POSTGRESQL)
                {
                    var conn = GetConnection();
                    conn.Open();
                    var ds = new DataSet();

                    var keys = new Dictionary<int, string>();
                    string sql = "SELECT id, name FROM keys WHERE tableid="
                            + "(SELECT id FROM tables WHERE classname='" + _tableName + "' AND type="
                            + "(SELECT id FROM tabletypes WHERE name='storage') AND categoryid IN "
                            + "(SELECT id FROM categories WHERE productid=(SELECT id FROM products WHERE name='" + _productName
                            + "')))";
                    var da = new NpgsqlDataAdapter(sql, conn);
                    ds.Reset();
                    da.Fill(ds);
                    var dt = ds.Tables[0];
                    foreach (DataRow row in dt.Rows)
                    {
                        var id = (int)row.ItemArray.GetValue(0);
                        var name = (string)row.ItemArray.GetValue(1);
                        keys.Add(id, name);
                    }

                    var rowNumbersAndIds = new Dictionary<int, int>();
                    sql = "SELECT id, \"order\" FROM rows WHERE tableid="
                            + "(SELECT id FROM tables WHERE classname='" + _tableName
                            + "' AND type=(SELECT id FROM tabletypes WHERE name='storage') AND categoryid IN "
                            + "(SELECT id FROM categories WHERE productid=(SELECT id FROM products WHERE name='" + _productName
                            + "'))) AND \"order\" IN (" + String.Join(",", iterationNumbers) + ") ORDER BY \"order\"";
                    da = new NpgsqlDataAdapter(sql, conn);
                    ds.Reset();
                    da.Fill(ds);
                    dt = ds.Tables[0];
                    foreach (DataRow row in dt.Rows)
                    {
                        var id = (int)row.ItemArray.GetValue(0);
                        var order = (int)row.ItemArray.GetValue(1);
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
                            var keyId = (int)row.ItemArray.GetValue(0);
                            var value = (string)row.ItemArray.GetValue(1);
                            iterRow.Add(keys[keyId], value);
                        }
                        result.Add(rowNumbersAndIds[rowId], iterRow);
                    }

                    conn.Close();
                }
                else
                {
                    string className = _tableName;
                    string sectionDir = "DataStorages";

                    string filePath = IOHelper.SearchFileByClassName(_productPath + "\\" + sectionDir, className);
                    if (filePath == null)
                    {
                        throw new Exception("File with class name '" + className + "' not found in directory '"
                                + _productPath + "\\" + sectionDir + "'.");
                    }
                    TableJson tableJson = IOHelper.ParseTableJson(filePath);
                    KeyJson[] keys = tableJson.Keys;
                    string[][] values = tableJson.Values;
                    int[] iterNumbers = (int[])iterationNumbers;
                    for (int i = 0; i < iterNumbers.Length; i++)
                    {
                        var row = new Dictionary<string, string>();
                        for (int j = 0; j < values[0].Length; j++)
                        {
                            row.Add(keys[j].Name, values[iterNumbers[i] - 1][j]);
                        }
                        result.Add(iterNumbers[i], row);
                    }
                }
            }
            catch (Exception e)
            {
                GribleSettings.ErHandler.OnAdaptorFail(e);
            }
            return result;
        }

        private NpgsqlConnection GetConnection()
        {
            if (Connection == null)
            {
                string dbhost = GribleSettings.Dbhost;
                string dbport = GribleSettings.Dbport;
                string dbName = GribleSettings.Dbname;
                string dblogin = GribleSettings.Dblogin;
                string dbpswd = GribleSettings.Dbpswd;

                string connstring = String.Format("Server={0};Port={1};" +
                                "User Id={2};Password={3};Database={4};",
                                dbhost, dbport, dblogin, dbpswd, dbName);

                Connection = new NpgsqlConnection(connstring);
            }
            return Connection;
        }

    }
}
