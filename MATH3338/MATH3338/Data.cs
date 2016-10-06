using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;

namespace MATH3338
{
    internal class Data
    {
        #region Variables and Instantiations

        private SqlConnection uhconn;
        private SqlCommand uhcmd;
        private SqlBulkCopy uhbulkcopy;
        private SqlDataAdapter uhadapter;
        private SqlDataReader uhsqlrdr;
        private OleDbConnection excelconn;
        private OleDbDataReader excelrdr;
        private OleDbCommand excelcmd;
        private string sqlconn;
        private string excelconnstring;
        private string excelqry;
        private string sqlqry;
        private string sqlserver;
        private string sqltable;
        private BackgroundWorker dataBW;
        private Log debug;
        private ini config = new ini(Directory.GetCurrentDirectory().ToString() + "\\Math.ini");

        #endregion Variables and Instantiations

        #region Constructors

        public Data()
        {
        }

        public Data(object sender)
        {
            dataBW = sender as BackgroundWorker;
            debug = new Log(dataBW, "Math3338.log");
        }

        #endregion Constructors

        #region Properties

        public string server
        {
            set
            {
                sqlserver = value;
            }

            get
            {
                return sqlserver;
            }
        }

        #endregion Properties

        #region Methods

        public void importData(string FilePath, string dataSource)
        {
            DataTable dt = new DataTable();
            sqlconn = "data source=" + config.Read("SQL", "SERVER") + ";integrated security=true;Initial catalog=" + config.Read("SQL", "DBNAME");
            sqltable = config.Read("SQL", dataSource + "TABLE");
            debug.debugWrite(sqlconn);
            string deleteqry = String.Format("delete from {0}",sqltable);
            using (uhconn = new SqlConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();

                //Table Verification
                uhcmd = new SqlCommand("sp_setupTab", uhconn);
                uhcmd.ExecuteNonQuery();

                //Delete Data from Table
                uhcmd = new SqlCommand(deleteqry, uhconn);
                uhcmd.ExecuteNonQuery();

                excelconnstring = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FilePath + ";Extended Properties=\"Excel 12.0 Xml;\"";

                using (excelconn = new OleDbConnection(excelconnstring))
                {
                    excelconn.Open();

                    //Getting All Sheets from excel file and building query using first sheet
                    dt = excelconn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    String[] excelSheets = new String[dt.Rows.Count];
                    int index = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[index] = row["TABLE_NAME"].ToString();
                        index++;
                    }

                    excelqry = String.Format(config.Read("QUERIES", dataSource + "EXCEL"), excelSheets[0].ToString().Replace("'", ""));
                    excelcmd = new OleDbCommand(excelqry, excelconn);
                    excelrdr = excelcmd.ExecuteReader();

                    //Bulk Copy Data to SqlTable
                    uhbulkcopy = new SqlBulkCopy(uhconn);
                    uhbulkcopy.DestinationTableName = sqltable;
                    uhbulkcopy.WriteToServer(excelrdr);
                }
            }

            //add Filters
            dataBW.ReportProgress(0, "Filter");
        }

        public List<string> getFilter(string fType,string prefix)
        {
            List<string> filters = new List<string>();
            string table = config.Read("SQL", prefix + "TABLE");
            filters.Add("All");
            //This method will read distinct values from
            using (uhconn = new SqlConnection(sqlconn))
            {
                sqlqry = "select distinct " + fType + " from " + table + " order by " + fType;
                uhcmd = new SqlCommand(sqlqry, uhconn);
                uhconn.Open();
                using (uhsqlrdr = uhcmd.ExecuteReader())
                    if (uhsqlrdr.HasRows)
                    {
                        while (uhsqlrdr.Read())
                        {
                            if (!uhsqlrdr.IsDBNull(0))
                            {
                                string item = uhsqlrdr.GetString(uhsqlrdr.GetOrdinal(fType));
                                filters.Add(item);
                            }
                        }
                    }
            }
            return filters;
        }

        public DataTable getGridData(string qry)
        {
            DataTable gridData = new DataTable();
            sqlconn = "data source=" + config.Read("SQL", "SERVER") + ";integrated security=true;Initial catalog=" + config.Read("SQL", "DBNAME");
            using (uhconn = new SqlConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();

                uhcmd = new SqlCommand(qry, uhconn);
                uhadapter = new SqlDataAdapter(uhcmd);

                gridData.Locale = System.Globalization.CultureInfo.InvariantCulture;

                uhadapter.Fill(gridData);
            }
            return gridData;
        }

        public string getStat(string stat,string type)
        {
            string statistic = "";

            sqlconn = "data source=" + config.Read("SQL", "SERVER") + ";integrated security=true;Initial catalog=" + config.Read("SQL", "DBNAME");
            using (uhconn = new SqlConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();
                string qry = String.Format("SELECT ResultValue FROM Stats WHERE ResultName ='{0}' and ResultType='{1}'",stat,type);

                uhcmd = new SqlCommand(qry, uhconn);
                var returnValue = uhcmd.ExecuteScalar();
                if (returnValue != null)
                {
                    statistic = returnValue.ToString();
                }
            }

            return statistic;
        }

        public void Fillreport(string whereclause,string table)
        {
            sqlconn = "data source=" + config.Read("SQL", "SERVER") + ";integrated security=true;Initial catalog=" + config.Read("SQL", "DBNAME");
            using (uhconn = new SqlConnection(sqlconn))
            {
                uhconn.Open();
                string drop = "if exists (select 1 from sys.tables where name='RPT_TAB') drop table rpt_tab";
                uhcmd = new SqlCommand(drop, uhconn);
                uhcmd.ExecuteNonQuery();
                string rptqry = config.Read("QUERIES", "SQLFILTER");
                rptqry = String.Format(rptqry, config.Read("SQL", "REPORTTABLE"), config.Read("SQL", table+"TABLE"), whereclause);
                uhcmd = new SqlCommand(rptqry, uhconn);
                uhcmd.ExecuteNonQuery();
                uhcmd = new SqlCommand("sp_StatsSummary", uhconn);
                uhcmd.CommandType = CommandType.StoredProcedure;
                uhcmd.Parameters.Add("@typeValue", SqlDbType.NVarChar);
                uhcmd.Parameters["@typeValue"].Value = table;
                
                uhcmd.ExecuteNonQuery();

                dataBW.ReportProgress(0, "GridView");
            }
        }

        public string getRowCount(string table)
        {
            string rowCount = "";

            sqlconn = "data source=" + config.Read("SQL", "SERVER") + ";integrated security=true;Initial catalog=" + config.Read("SQL", "DBNAME");
            using (uhconn = new SqlConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();
                string tableqry = String.Format("Select count(*) from {0}", table);
                uhcmd = new SqlCommand(tableqry, uhconn);
                var returnValue = uhcmd.ExecuteScalar();
                rowCount = returnValue.ToString();
            }

            return rowCount;
        }

        public string statsString()
        {
            string stats = "";

            //stats = getStat("Mean") + ", " + getStat("Median") + ", " + getStat("1st percentile") + ", " + getStat("3rd percentile");

            return stats;
        }

        public void addData(string jt, string s, string id)
        {
            sqlconn = "data source=" + config.Read("SQL", "SERVER") + ";integrated security=true;Initial catalog=" + config.Read("SQL", "DBNAME");
            sqltable = config.Read("SQL", "DBTABLE");
            using (uhconn = new SqlConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();

                string query = "INSERT INTO UHDATA(JobTitle,Salary,DeptId) Values('"+ jt + "','" + s + "','" + id + "')";

                using (uhcmd = new SqlCommand(query, uhconn))
                {
                    uhcmd.ExecuteNonQuery();
                }
            }

        }

        public void CompStats(string k,string d,string l,string APConstant,string FConstant)
        {

            sqlconn = "data source=" + config.Read("SQL", "SERVER") + ";integrated security=true;Initial catalog=" + config.Read("SQL", "DBNAME");
            using (uhconn = new SqlConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();
                uhcmd = new SqlCommand("InternalComp", uhconn);
                uhcmd.CommandType = CommandType.StoredProcedure;
                uhcmd.Parameters.Add("@kValue", SqlDbType.Decimal);
                uhcmd.Parameters["@kValue"].Value = Convert.ToDecimal(k);

                uhcmd.Parameters.Add("@dValue", SqlDbType.Decimal);
                uhcmd.Parameters["@dValue"].Value =Convert.ToDecimal(d);

                uhcmd.Parameters.Add("@ACValue", SqlDbType.Decimal);
                uhcmd.Parameters["@ACValue"].Value = Convert.ToDecimal(APConstant);

                uhcmd.Parameters.Add("@FCValue", SqlDbType.Decimal);
                uhcmd.Parameters["@FCValue"].Value = Convert.ToDecimal(APConstant);

                uhcmd.Parameters.Add("@lValue", SqlDbType.Decimal);
                uhcmd.Parameters["@lValue"].Value = Convert.ToDecimal(l);

                uhcmd.ExecuteNonQuery();
            }

        }
        #endregion Methods
    }
}