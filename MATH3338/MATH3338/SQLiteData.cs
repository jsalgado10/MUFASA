using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Data;
using System.Windows.Forms;

namespace MATH3338
{
    class SQLiteData
    {
        #region Variables and Instantiations
        private SQLiteConnection uhconn;
        private SQLiteCommand uhcmd;
        private SQLiteDataAdapter uhadapter;
        private SQLiteDataReader uhsqlrdr;
        private OleDbConnection excelconn;
        private OleDbCommand excelcmd;
        private OleDbDataAdapter exceladp;
        private string sqlconn;
        private string excelconnstring;
        private string excelqry;
        private string sqlqry;        
        private string sqltable;
        private BackgroundWorker dataBW;
        private Log debug;
        private ini config = new ini(Directory.GetCurrentDirectory().ToString() + "\\Math.ini");
        string datafile ="";
        public Dictionary<int, string> jobDic = new Dictionary<int, string>();

        #endregion 

        #region Constructors

        public SQLiteData()
        {
            datafile = Directory.GetCurrentDirectory().ToString() + "\\" + config.Read("SQL", "DBFILE");
            sqlconn = String.Format("Data Source={0};Version=3", datafile);
        }

        public SQLiteData(object sender)
        {
            dataBW = sender as BackgroundWorker;
            debug = new Log(dataBW, "Math3338.log");
            datafile = Directory.GetCurrentDirectory().ToString() + "\\" + config.Read("SQL", "DBFILE");
            sqlconn = String.Format("Data Source={0};Version=3", datafile);
        }

        #endregion

        #region Properties

        public string currentSpace
        {
            set
            {
                datafile = value;
                sqlconn = String.Format("Data Source={0};Version=3", datafile);
            }

            get
            {
                return datafile;
            }
        }

        #endregion Properties

        #region Methods

        public void importData(string FilePath, string dataSource)
        {
            //Initialize Data Base File
            //initDataBase();
            DataTable dt = new DataTable();
            DataTable excelData = new DataTable();
            sqltable = config.Read("SQL", dataSource + "TABLE");
            string deleteqry = String.Format("delete from {0}", sqltable);
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                //Open connection to SQLite DB File
                uhconn.Open();

                //Delete Data from Table
                uhcmd = new SQLiteCommand(deleteqry, uhconn);
                uhcmd.ExecuteNonQuery();

                string insertqry = config.Read("QUERIES", dataSource + "INSERT");

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
                        if (row["TABLE_NAME"].ToString().Contains("$"))
                        {
                            excelSheets[index] = row["TABLE_NAME"].ToString();
                            index++;
                        }
                    }
                    DataSet output = new DataSet();
                    DataTable outputTab = new DataTable();
                    excelqry = String.Format(config.Read("QUERIES", dataSource + "EXCEL"), excelSheets[0].ToString().Replace("'", ""));
                    excelcmd = new OleDbCommand(excelqry, excelconn);                 
                    exceladp = new OleDbDataAdapter(excelcmd);
                    exceladp.Fill(excelData);                    
                    
                    using (var trs = uhconn.BeginTransaction())
                    {
                        //Write Data Table to SQLite Table
                        using (uhcmd = uhconn.CreateCommand())
                        {
                            uhcmd.CommandText = insertqry;
                            foreach (DataRow row in excelData.Rows)
                            {
                                switch (dataSource)
                                {
                                    case "DB":
                                        uhcmd.Parameters.AddWithValue("job", row[0].ToString().ToUpper());
                                        uhcmd.Parameters.AddWithValue("salary", Convert.ToDecimal(row[1]));
                                        uhcmd.Parameters.AddWithValue("depit", row[2].ToString().ToUpper());
                                        break;
                                    case "COMP":
                                        uhcmd.Parameters.AddWithValue("job", row[0].ToString().ToUpper());
                                        uhcmd.Parameters.AddWithValue("salary", Convert.ToDecimal(row[1]));
                                        uhcmd.Parameters.AddWithValue("year", row[2].ToString());
                                        uhcmd.Parameters.AddWithValue("deptid", row[3].ToString().ToUpper());
                                        break;
                                    case "UHEQTY":
                                        uhcmd.Parameters.AddWithValue("code", row[0].ToString());
                                        uhcmd.Parameters.AddWithValue("job", row[1].ToString().ToUpper());
                                        uhcmd.Parameters.AddWithValue("deptid", row[2].ToString().ToUpper());
                                        uhcmd.Parameters.AddWithValue("weight", row[3].ToString());
                                        uhcmd.Parameters.AddWithValue("decription", row[4].ToString().ToUpper());
                                        break;
                                    case "T1EQTY":
                                        uhcmd.Parameters.AddWithValue("salary", Convert.ToDecimal(row[0]));
                                        uhcmd.Parameters.AddWithValue("job", row[1].ToString().ToUpper());
                                        uhcmd.Parameters.AddWithValue("code", row[2].ToString());
                                        uhcmd.Parameters.AddWithValue("description", row[3].ToString().ToUpper());
                                        break;
                                }
                                uhcmd.ExecuteNonQuery();
                            }
                        }
                        trs.Commit();
                    }

                }
                if (dataSource == "DB")
                {
                    
                    addDic();
                }
                dataBW.ReportProgress(0, "Filter"+ dataSource);
            }
        }

        public DataTable GetDataTableFromDB(string query)
        {

            DataTable dt = new DataTable();


            uhconn = new SQLiteConnection(sqlconn);
            uhcmd = new SQLiteCommand(query,uhconn);
            using (uhadapter = new SQLiteDataAdapter(uhcmd))
            {
                uhadapter.Fill(dt);
            }

            
            return dt;
        }

        public void initDataBase()
        {
            try
            {
                using (uhconn = new SQLiteConnection(sqlconn))
                {
                    //Open connection to SQLite DB File
                    uhconn.Open();
                    using (var transaction = uhconn.BeginTransaction())
                    {
                        //Initial DB load
                        string initialQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\Init.sql");
                        uhcmd = new SQLiteCommand(initialQry, uhconn);
                        uhcmd.ExecuteNonQuery();
                        transaction.Commit();
                    }

                }
            }
            catch(Exception initError)
            {
                MessageBox.Show(initError.Message);
            }
        }

        public void createNewConnection(string filePath)
        {
            sqlconn = String.Format("Data Source={0};Version=3", filePath);
        }
        
        public List<string> getFilter(string fType, string prefix)
        {
            List<string> filters = new List<string>();
            string table = config.Read("SQL", prefix + "TABLE");
            if (prefix == "UHEQTY")
            {
                if (fType == "JobTitle")
                {
                    filters.Add("NEW ASSISTANT");
                }
                table = "UHDATA";
            }
            else if (prefix == "T1EQTY")
            {
                table = "UHEQTY_TAB";
            }
            //This method will read distinct values from
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                sqlqry = "select distinct " + fType + " from " + table + " order by " + fType;
                uhcmd = new SQLiteCommand(sqlqry, uhconn);
                if (uhconn.State == ConnectionState.Closed)
                {
                    uhconn.Open();
                }
                using (uhsqlrdr = uhcmd.ExecuteReader())
                    if (uhsqlrdr.HasRows)
                    {
                        while (uhsqlrdr.Read())
                        {
                            if (!uhsqlrdr.IsDBNull(0))
                            {
                                string item = uhsqlrdr.GetString(uhsqlrdr.GetOrdinal(fType)).ToUpper();
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
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();

                uhcmd = new SQLiteCommand(qry, uhconn);
                uhadapter = new SQLiteDataAdapter(uhcmd);

                gridData.Locale = System.Globalization.CultureInfo.InvariantCulture;

                uhadapter.Fill(gridData);
            }
            return gridData;
        }

        public string getStat(string stat, string type)
        {
            string statistic = "";
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();
                string qry = String.Format("SELECT printf('%.2f',ResultValue) FROM Stats WHERE ResultName ='{0}' and ResultType='{1}'", stat, type);

                uhcmd = new SQLiteCommand(qry, uhconn);
                var returnValue = uhcmd.ExecuteScalar();
                if (returnValue != null)
                {
                    statistic = returnValue.ToString();
                }
            }

            return statistic;
        }

        public string getRowCount(string table)
        {
            string rowCount = "";
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();
                string tableqry = String.Format("Select count(*) from {0}", table);
                uhcmd = new SQLiteCommand(tableqry, uhconn);
                var returnValue = uhcmd.ExecuteScalar();
                rowCount = returnValue.ToString();
            }

            return rowCount;
        }

        public void addData(string jt, string s, string id)
        {
            sqltable = config.Read("SQL", "DBTABLE");
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();

                string query = "INSERT INTO UHDATA(JobTitle,Salary,DeptId) Values('" + jt + "','" + s + "','" + id + "')";

                using (uhcmd = new SQLiteCommand(query, uhconn))
                {
                    uhcmd.ExecuteNonQuery();
                }
            }

        }

        public void Fillreport(string whereclause, string table)
        {           
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                uhconn.Open();
                using (var trs = uhconn.BeginTransaction())
                {
                    string drop = "drop table if exists rpt_tab";
                    uhcmd = new SQLiteCommand(drop, uhconn);
                    uhcmd.ExecuteNonQuery();
                    string rptqry = config.Read("QUERIES", "SQLFILTER");
                    rptqry = String.Format(rptqry, config.Read("SQL", "REPORTTABLE"), config.Read("SQL", table + "TABLE"), whereclause);
                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                    uhcmd.ExecuteNonQuery();
                    string purge = "delete from stats where ResultType in('" + table + "')";
                    uhcmd = new SQLiteCommand(purge, uhconn);
                    uhcmd.ExecuteNonQuery();
                    string statQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\Stats.sql");
                    uhcmd = new SQLiteCommand(statQry, uhconn);
                    uhcmd.Parameters.AddWithValue("@type", table);                 
                    uhcmd.ExecuteNonQuery();
                    trs.Commit();
                }
                dataBW.ReportProgress(0, "GridView"+table);
            }
        }

        //This method can be deleted since it was included into compratio
        public void CompStats(string k, string d)
        {            
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                //Open SQL Connection
                uhconn.Open();
                string yearQry="select strftime('%Y',date('now'))-c.'Year Hired' from COMP_TAB c, rpt_tab rpt where rpt.deptid=c.deptid";
                uhcmd = new SQLiteCommand(yearQry, uhconn);
                double n =Convert.ToDouble(uhcmd.ExecuteScalar());
                double newk = Math.Pow(Convert.ToDouble(k),n);
                string compQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\Compression.sql");
                uhcmd = new SQLiteCommand(compQry, uhconn);
                uhcmd.Parameters.AddWithValue("@k",newk);
                uhcmd.Parameters.AddWithValue("@dla",Math.Pow(Convert.ToDouble(d),Convert.ToDouble(config.Read("CONSTANT","LA"))));
                uhcmd.Parameters.AddWithValue("@dlf",Math.Pow(Convert.ToDouble(d), Convert.ToDouble(config.Read("CONSTANT", "LF"))));
                uhcmd.Parameters.AddWithValue("@associate",Convert.ToDouble(config.Read("CONSTANT", "ASSOCIATE")));
                uhcmd.Parameters.AddWithValue("@Full", Convert.ToDecimal(config.Read("CONSTANT", "FULL")));
                uhcmd.ExecuteNonQuery();
                
            }

        }

        public void compRatio(string [] depts,string k,string d,string table)
        {
            string[] jobTitles = new string[] { "PROFESSOR", "ASSOCIATE PROFESSOR" };
            string newwhere = "";
            string nameType = "";
            string drop = "";
            string rptqry = "";
            string purge = "";
            string whereclause = "";
            string adjmed = "";
            using (uhconn=new SQLiteConnection(sqlconn))
            {
                uhconn.Open();
                using (var sqlitetrs = uhconn.BeginTransaction())
                {
                    purge = "delete from COMPSTATS";
                    uhcmd = new SQLiteCommand(purge, uhconn);
                    uhcmd.ExecuteNonQuery();
                    foreach (string dept in depts)
                    {
                        //Calculate Stats for COMP Based on Dept. Median is most important since it will be used for Compression
                        whereclause = "WHERE DEPTID IN('" + dept + "')";
                        drop = "drop table if exists rpt_tab";
                        uhcmd = new SQLiteCommand(drop, uhconn);
                        uhcmd.ExecuteNonQuery();
                        rptqry = config.Read("QUERIES", "SQLFILTER");
                        rptqry = String.Format(rptqry, config.Read("SQL", "REPORTTABLE"), config.Read("SQL", table + "TABLE"), whereclause);
                        uhcmd = new SQLiteCommand(rptqry, uhconn);
                        uhcmd.ExecuteNonQuery();
                        purge = "delete from stats where ResultType in('" + table + "')";
                        uhcmd = new SQLiteCommand(purge, uhconn);
                        uhcmd.ExecuteNonQuery();
                        string statQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\Stats.sql");
                        uhcmd = new SQLiteCommand(statQry, uhconn);
                        uhcmd.Parameters.AddWithValue("@type", table);
                        uhcmd.ExecuteNonQuery();

                        //Calculate Actual Medina from UH Data
                        foreach (string job in jobTitles)
                        {
                            switch (job)
                            {
                                case "PROFESSOR":
                                    nameType = "ActualMedianFP";
                                    break;
                                case "ASSOCIATE PROFESSOR":
                                    nameType = "ActualMedianAP";
                                    break;
                            }
                            //Fill TEMPRPT with filter data
                            if (uhconn.State == ConnectionState.Closed)
                            {
                                uhconn.Open();
                            }
                            newwhere = whereclause + "and JobTitle in('" + job + "')";
                            drop = "drop table if exists TEMPRPT";
                            uhcmd = new SQLiteCommand(drop, uhconn);
                            uhcmd.ExecuteNonQuery();
                            rptqry = config.Read("QUERIES", "SQLFILTER");
                            rptqry = String.Format(rptqry, "TEMPRPT", config.Read("SQL", "DBTABLE"), newwhere);
                            uhcmd = new SQLiteCommand(rptqry, uhconn);
                            uhcmd.ExecuteNonQuery();
                            //Calculate Median for Prof and Associate Prof
                            string medianQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\MedianCalc.sql");
                            uhcmd = new SQLiteCommand(medianQry, uhconn);
                            uhcmd.Parameters.AddWithValue("@type", "COMP");
                            uhcmd.Parameters.AddWithValue("@name", nameType);
                            uhcmd.ExecuteNonQuery();

                        }

                        //Calculate Adjusted Medians
                        if (uhconn.State == ConnectionState.Closed)
                        {
                            uhconn.Open();
                        }
                        string yearQry = "select strftime('%Y',date('now'))-c.'Year Hired' from COMP_TAB c, rpt_tab rpt where rpt.deptid=c.deptid";
                        uhcmd = new SQLiteCommand(yearQry, uhconn);
                        double n = Convert.ToDouble(uhcmd.ExecuteScalar());
                        double newk = Math.Pow(Convert.ToDouble(k), n);
                        string compQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\Compression.sql");
                        uhcmd = new SQLiteCommand(compQry, uhconn);
                        uhcmd.Parameters.AddWithValue("@k", newk);
                        uhcmd.Parameters.AddWithValue("@dla", Math.Pow(Convert.ToDouble(d), Convert.ToDouble(config.Read("CONSTANT", "LA"))));
                        uhcmd.Parameters.AddWithValue("@dlf", Math.Pow(Convert.ToDouble(d), Convert.ToDouble(config.Read("CONSTANT", "LF"))));
                        uhcmd.Parameters.AddWithValue("@associate", Convert.ToDouble(config.Read("CONSTANT", "ASSOCIATE")));
                        uhcmd.Parameters.AddWithValue("@Full", Convert.ToDecimal(config.Read("CONSTANT", "FULL")));
                        uhcmd.ExecuteNonQuery();

                        //Insert into COMPSTATS
                        foreach (string job in jobTitles)
                        {
                            switch (job)
                            {
                                case "PROFESSOR":
                                    nameType = "ActualMedianFP";
                                    adjmed = "AdjMedFull";
                                    break;
                                case "ASSOCIATE PROFESSOR":
                                    nameType = "ActualMedianAP";
                                    adjmed = "AdjMedAssocP";
                                    break;
                            }

                            rptqry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\COMPSTATS.sql");
                            uhcmd = new SQLiteCommand(rptqry, uhconn);
                            uhcmd.Parameters.AddWithValue("@dept", dept);
                            uhcmd.Parameters.AddWithValue("@job", job);
                            uhcmd.Parameters.AddWithValue("@ActMed", nameType);
                            uhcmd.Parameters.AddWithValue("AdjMed", adjmed);
                            uhcmd.ExecuteNonQuery();

                        }
                    }
                    sqlitetrs.Commit();
                }
            
            }
            dataBW.ReportProgress(0, "GridView" + table);
        }        

        public void Eqty(string []jobs,string [] depts, string table,bool allcalc)
        {
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                if (allcalc)
                {
                    jobs = new string[4] { "PROFESSOR", "ASSOCIATE PROFESSOR", "ASSISTANT PROFESSOR", "NEW ASSISTANT" };
                }
                string whereclause = "";
                string source = "";
                string rpttab = "TEMPRPT";
                string drop = "";
                string rptqry = "";
                string meanQry = "";
                string purge = "";
                string eqtyQry = "";
                string statTab="";

                if (uhconn.State == ConnectionState.Closed)
                {
                    uhconn.Open();
                }
                //Case for type of EQTY
                switch (table)
                {
                    #region UHEQTY
                    case "UHEQTY":                        
                        statTab="IEQTYSTATS";
                        using (var trs=uhconn.BeginTransaction())
                        {
                            purge = "delete from IEQTYSTATS";
                            uhcmd = new SQLiteCommand(purge, uhconn);
                            uhcmd.ExecuteNonQuery();
                            foreach (string dept in depts)
                            {
                                source = "UHDATA";
                                purge = "delete from stats where ResultType in('" + table + "')";
                                uhcmd = new SQLiteCommand(purge, uhconn);
                                uhcmd.ExecuteNonQuery();
                                foreach (string job in jobs)
                                {

                                    if (job == "NEW ASSISTANT")
                                    {
                                        source = config.Read("SQL", "COMPTABLE");
                                    }
                                    else
                                    {
                                        source = "UHDATA";
                                    }
                                    whereclause = "WHERE Deptid in('" + dept + "') and jobtitle in('" + job + "')";

                                    if (uhconn.State == ConnectionState.Closed)
                                    {
                                        uhconn.Open();
                                    }

                                    //Drop TEMPRPT Table
                                    drop = "drop table if exists " + rpttab;
                                    uhcmd = new SQLiteCommand(drop, uhconn);
                                    uhcmd.ExecuteNonQuery();

                                    //Insert Filter Data into TEMPRPT
                                    rptqry = config.Read("QUERIES", "SQLFILTER");
                                    rptqry = String.Format(rptqry, rpttab, source, whereclause);
                                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                                    uhcmd.ExecuteNonQuery();

                                    //Calculate Mean
                                    meanQry = String.Format(File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\MeanCalc.sql"), rpttab);
                                    uhcmd = new SQLiteCommand(meanQry, uhconn);
                                    uhcmd.Parameters.AddWithValue("@type", table);
                                    uhcmd.Parameters.AddWithValue("@name", job);
                                    uhcmd.ExecuteNonQuery();
                                }
                                if (allcalc)
                                {
                                    //add data to IEQTYSTATS
                                    rptqry = String.Format(File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\IEQTY.sql"), statTab, table);
                                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                                    uhcmd.Parameters.AddWithValue("@dept", dept);
                                    uhcmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    rptqry = String.Format("INSERT INTO IEQTYSTATS select '{0}','{1}/{2}',printf('%.2f',(select resultvalue from stats where resulttype='{3}' and resultname='{1}')),printf('%.2f',(select resultvalue from stats where resulttype='{3}' and resultname='{2}')),0.00;", dept, jobs[0], jobs[1], table);
                                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                                    uhcmd.ExecuteNonQuery();

                                    rptqry = "UPDATE IEQTYSTATS SET RESULT=ROUND(VALUE1/VALUE2,2);";
                                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                                    uhcmd.ExecuteNonQuery();
                                }
                            }
                            trs.Commit();
                        }
                        break;
                    #endregion

                    #region EEQTY
                    case "T1EQTY":              
                        statTab="EEQTYSTATS";
                        //Drop TEMPRPT Table
                        drop = "drop table if exists " + rpttab;
                        uhcmd = new SQLiteCommand(drop, uhconn);
                        uhcmd.ExecuteNonQuery();
                        using (var trs = uhconn.BeginTransaction())
                        {
                            purge = "delete from EEQTYSTATS";
                            uhcmd = new SQLiteCommand(purge, uhconn);
                            uhcmd.ExecuteNonQuery();
                            foreach(string dept in depts)
                            {
                                purge = "delete from stats where ResultType in('" + table + "')";
                                uhcmd = new SQLiteCommand(purge, uhconn);
                                uhcmd.ExecuteNonQuery();
                                if (allcalc)
                                {
                                    //if using t1 ratio only
                                    foreach (string job in jobs)
                                    {
                                        //Drop TEMPRPT Table
                                        drop = "drop table if exists " + rpttab;
                                        uhcmd = new SQLiteCommand(drop, uhconn);
                                        uhcmd.ExecuteNonQuery();

                                        //Insert sum(salary*weight) into stats
                                        whereclause = "WHERE UH.Deptid in('" + dept + "') and UH.jobtitle in('" + job + "')";
                                        eqtyQry = String.Format(File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\EQTY.sql"), whereclause);
                                        uhcmd = new SQLiteCommand(eqtyQry, uhconn);
                                        uhcmd.Parameters.AddWithValue("@type", table);
                                        uhcmd.Parameters.AddWithValue("@name", job);
                                        uhcmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    //Drop TEMPRPT Table
                                    drop = "drop table if exists " + rpttab;
                                    uhcmd = new SQLiteCommand(drop, uhconn);
                                    uhcmd.ExecuteNonQuery();

                                    //Insert sum(salary*weight) into stats
                                    whereclause = "WHERE UH.Deptid in('" + dept + "') and UH.jobtitle in('" + jobs[0].ToUpper() + "')";
                                    eqtyQry = String.Format(File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\EQTY.sql"), whereclause);
                                    uhcmd = new SQLiteCommand(eqtyQry, uhconn);
                                    uhcmd.Parameters.AddWithValue("@type", table);
                                    uhcmd.Parameters.AddWithValue("@name", jobs[0].ToUpper());
                                    uhcmd.ExecuteNonQuery();

                                    //insert uh avg into stats
                                    source = "UHDATA";
                                    whereclause = "WHERE Deptid in('" + dept + "') and jobtitle in('" + jobs[1].ToUpper() + "')";
                                    string uhqry = config.Read("QUERIES", "SQLFILTER");
                                    uhqry = String.Format(uhqry, rpttab, source, whereclause);
                                    uhcmd = new SQLiteCommand(uhqry, uhconn);
                                    uhcmd.Parameters.AddWithValue("@type", table);
                                    uhcmd.Parameters.AddWithValue("@name", jobs[1].ToUpper());
                                    uhcmd.ExecuteNonQuery();
                                    //Calculate Mean
                                    string uhMeandQry = String.Format(File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\MeanCalc.sql"), rpttab);
                                    uhcmd = new SQLiteCommand(uhMeandQry, uhconn);
                                    uhcmd.Parameters.AddWithValue("@type", table);
                                    uhcmd.Parameters.AddWithValue("@name", jobs[1].ToUpper());
                                    uhcmd.ExecuteNonQuery();
                                }
                                                        

                                //Save to EEQTYstats table
                                if (allcalc)
                                {
                                    //add data to IEQTYSTATS
                                    rptqry = String.Format(File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\IEQTY.sql"),statTab,table);
                                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                                    uhcmd.Parameters.AddWithValue("@dept", dept);
                                    uhcmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    rptqry = String.Format("INSERT INTO {0}  select '{1}','{2}/{3}',printf('%.2f',(select resultvalue from stats where resulttype='{4}' and resultname='{2}')),printf('%.2f',(select resultvalue from stats where resulttype='{4}' and resultname='{3}')),0.00;",statTab, dept, jobs[0], jobs[1],table);
                                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                                    uhcmd.ExecuteNonQuery();

                                    rptqry = String.Format("UPDATE {0} SET RESULT=ROUND(VALUE1/VALUE2,2);",statTab);
                                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                                    uhcmd.ExecuteNonQuery();
                                }
                            }
                            trs.Commit();
                        }
                        break;
                    #endregion
                }
                //Update gridview
                dataBW.ReportProgress(0, "GridView" + table);   
            }
        }

        public void addDic()
        {
            //Add all Job Titles to Dictionary with a number as index starting with 1
            List<string> uhjobs = new List<string>();
            uhjobs=getFilter("JobTitle", "DB");
            int count = 1;
            jobDic.Clear();
            foreach(string job in uhjobs)
            {
                jobDic.Add(count, job);
                count++;
            }
        }

        public double calcMedian(string where,string table)
        {
            double result = 0;
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                //Fill TEMPRPT with filter data
                if (uhconn.State == ConnectionState.Closed)
                {
                    uhconn.Open();
                }
                using (var trs = uhconn.BeginTransaction())
                {
                    string drop = "drop table if exists TEMPRPT";
                    uhcmd = new SQLiteCommand(drop, uhconn);
                    uhcmd.ExecuteNonQuery();
                    string purge = "delete from stats where ResultType in('" + table + "')";
                    uhcmd = new SQLiteCommand(purge, uhconn);
                    uhcmd.ExecuteNonQuery();
                    string rptqry = config.Read("QUERIES", "SQLFILTER");
                    rptqry = String.Format(rptqry, "TEMPRPT", config.Read("SQL", table + "TABLE"), where);
                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                    uhcmd.ExecuteNonQuery();
                    //Calculate Median for Prof and Associate Prof
                    string medianQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\MedianCalc.sql");
                    uhcmd = new SQLiteCommand(medianQry, uhconn);
                    uhcmd.Parameters.AddWithValue("@type", table);
                    uhcmd.Parameters.AddWithValue("@name", "Median");
                    uhcmd.ExecuteNonQuery();
                    trs.Commit();
                }
                result = Convert.ToDouble(getStat("Median", table));
            }
            return result;
        }

        public double calcMean(string where, string table)
        {
            double result = 0;
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                //Fill TEMPRPT with filter data
                if (uhconn.State == ConnectionState.Closed)
                {
                    uhconn.Open();
                }
                using (var trs = uhconn.BeginTransaction())
                {
                    string drop = "drop table if exists TEMPRPT";
                    uhcmd = new SQLiteCommand(drop, uhconn);
                    uhcmd.ExecuteNonQuery();
                    string purge = "delete from stats where ResultType in('" + table + "')";
                    uhcmd = new SQLiteCommand(purge, uhconn);
                    uhcmd.ExecuteNonQuery();
                    string rptqry = config.Read("QUERIES", "SQLFILTER");
                    rptqry = String.Format(rptqry, "TEMPRPT", config.Read("SQL", table + "TABLE"), where);
                    uhcmd = new SQLiteCommand(rptqry, uhconn);
                    uhcmd.ExecuteNonQuery();
                    //Calculate Median for Prof and Associate Prof
                    string medianQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\MeanCalc.sql");
                    uhcmd = new SQLiteCommand(medianQry, uhconn);
                    uhcmd.Parameters.AddWithValue("@type", table);
                    uhcmd.Parameters.AddWithValue("@name", "Mean");
                    uhcmd.ExecuteNonQuery();
                    trs.Commit();
                    
                }
                result = Convert.ToDouble(getStat("Mean", table));
                
            }
            return result;
        }

        public void uhStats(string [] depts,string [] jobs,string table)
        {
            string whereclause = "";
            string rptqry = "";
            string drop = "";
            string purge = "";
            string statQry = "";
            string whereclauseExt = "";
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                uhconn.Open();
                using (var trs = uhconn.BeginTransaction())
                {
                    purge = "Delete from UHSTATS";
                    uhcmd = new SQLiteCommand(purge, uhconn);
                    uhcmd.ExecuteNonQuery();
                    foreach (string dept in depts)
                    {
                        whereclause = "WHERE DEPTID IN('" + dept + "')";
                        foreach (string job in jobs)
                        {
                            //Add Filter Data to RPT_TAB for calculations
                            drop = "drop table if exists rpt_tab";
                            uhcmd = new SQLiteCommand(drop, uhconn);
                            uhcmd.ExecuteNonQuery();
                            whereclauseExt = whereclause + " AND JOBTITLE IN('" + job + "')";
                            rptqry = config.Read("QUERIES", "SQLFILTER");
                            rptqry = String.Format(rptqry, config.Read("SQL", "REPORTTABLE"), config.Read("SQL", table + "TABLE"), whereclauseExt);
                            uhcmd = new SQLiteCommand(rptqry, uhconn);
                            uhcmd.ExecuteNonQuery();

                            //Delete DB Data before 
                            purge = "delete from stats where ResultType in('" + table + "')";
                            uhcmd = new SQLiteCommand(purge, uhconn);
                            uhcmd.ExecuteNonQuery();
                            statQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\Stats.sql");
                            uhcmd = new SQLiteCommand(statQry, uhconn);
                            uhcmd.Parameters.AddWithValue("@type", table);
                            uhcmd.ExecuteNonQuery();

                            //Insert into UHSTATS
                            statQry = "INSERT INTO UHSTATS SELECT @dept,@job,printf('%.2f',(select resultvalue from STATS where resultName='Median' and resulttype='DB')),"+
                                "printf('%.2f',(select resultvalue from STATS where resultName='Mean' and resulttype='DB')),printf('%.2f',(select resultvalue from STATS where resultName='1st Quartile' and resulttype='DB'))," +
                                "printf('%.2f',(select resultvalue from STATS where resultName='3rd Quartile'and resulttype='DB'))" ;
                            uhcmd = new SQLiteCommand(statQry, uhconn);
                            uhcmd.Parameters.AddWithValue("@dept", dept);
                            uhcmd.Parameters.AddWithValue("@job", job);
                            uhcmd.ExecuteNonQuery();
                        }
                    }
                    trs.Commit();
                }
                
            }
            dataBW.ReportProgress(0, "GridView" + table);
        }

        public void calcAdj(string[] depts, string k, string d, string table)
        {
            string[] jobTitles = new string[] { "PROFESSOR", "ASSOCIATE PROFESSOR" };
            string newwhere = "";
            string nameType = "";
            string drop = "";
            string rptqry = "";
            string purge = "";
            string whereclause = "";
            string adjmed = "";
            using (uhconn = new SQLiteConnection(sqlconn))
            {
                uhconn.Open();
                using (var sqlitetrs = uhconn.BeginTransaction())
                {
                    purge = "delete from COMPSTATS";
                    uhcmd = new SQLiteCommand(purge, uhconn);
                    uhcmd.ExecuteNonQuery();
                    foreach (string dept in depts)
                    {
                        //Calculate Stats for COMP Based on Dept. Median is most important since it will be used for Compression
                        whereclause = "WHERE DEPTID IN('" + dept + "')";
                        drop = "drop table if exists rpt_tab";
                        uhcmd = new SQLiteCommand(drop, uhconn);
                        uhcmd.ExecuteNonQuery();
                        rptqry = config.Read("QUERIES", "SQLFILTER");
                        rptqry = String.Format(rptqry, config.Read("SQL", "REPORTTABLE"), config.Read("SQL", table + "TABLE"), whereclause);
                        uhcmd = new SQLiteCommand(rptqry, uhconn);
                        uhcmd.ExecuteNonQuery();
                        purge = "delete from stats where ResultType in('" + table + "')";
                        uhcmd = new SQLiteCommand(purge, uhconn);
                        uhcmd.ExecuteNonQuery();
                        string statQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\Stats.sql");
                        uhcmd = new SQLiteCommand(statQry, uhconn);
                        uhcmd.Parameters.AddWithValue("@type", table);
                        uhcmd.ExecuteNonQuery();

                        //Calculate Actual Medina from UH Data
                        foreach (string job in jobTitles)
                        {
                            switch (job)
                            {
                                case "PROFESSOR":
                                    nameType = "ActualMedianFP";
                                    break;
                                case "ASSOCIATE PROFESSOR":
                                    nameType = "ActualMedianAP";
                                    break;
                            }
                            //Fill TEMPRPT with filter data
                            if (uhconn.State == ConnectionState.Closed)
                            {
                                uhconn.Open();
                            }
                            newwhere = whereclause + "and JobTitle in('" + job + "')";
                            drop = "drop table if exists TEMPRPT";
                            uhcmd = new SQLiteCommand(drop, uhconn);
                            uhcmd.ExecuteNonQuery();
                            rptqry = config.Read("QUERIES", "SQLFILTER");
                            rptqry = String.Format(rptqry, "TEMPRPT", config.Read("SQL", "DBTABLE"), newwhere);
                            uhcmd = new SQLiteCommand(rptqry, uhconn);
                            uhcmd.ExecuteNonQuery();
                            //Calculate Median for Prof and Associate Prof
                            string medianQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\MedianCalc.sql");
                            uhcmd = new SQLiteCommand(medianQry, uhconn);
                            uhcmd.Parameters.AddWithValue("@type", "COMP");
                            uhcmd.Parameters.AddWithValue("@name", nameType);
                            uhcmd.ExecuteNonQuery();

                        }

                        //Calculate Adjusted Medians
                        if (uhconn.State == ConnectionState.Closed)
                        {
                            uhconn.Open();
                        }
                        string yearQry = "select strftime('%Y',date('now'))-c.'Year Hired' from COMP_TAB c, rpt_tab rpt where rpt.deptid=c.deptid";
                        uhcmd = new SQLiteCommand(yearQry, uhconn);
                        double n = Convert.ToDouble(uhcmd.ExecuteScalar());
                        double newk = Math.Pow(Convert.ToDouble(k), n);
                        string compQry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\Compression.sql");
                        uhcmd = new SQLiteCommand(compQry, uhconn);
                        uhcmd.Parameters.AddWithValue("@k", newk);
                        uhcmd.Parameters.AddWithValue("@dla", Math.Pow(Convert.ToDouble(d), Convert.ToDouble(config.Read("CONSTANT", "LA"))));
                        uhcmd.Parameters.AddWithValue("@dlf", Math.Pow(Convert.ToDouble(d), Convert.ToDouble(config.Read("CONSTANT", "LF"))));
                        uhcmd.Parameters.AddWithValue("@associate", Convert.ToDouble(config.Read("CONSTANT", "ASSOCIATE")));
                        uhcmd.Parameters.AddWithValue("@Full", Convert.ToDecimal(config.Read("CONSTANT", "FULL")));
                        uhcmd.ExecuteNonQuery();

                        //Insert into COMPSTATS
                        foreach (string job in jobTitles)
                        {
                            switch (job)
                            {
                                case "PROFESSOR":
                                    nameType = "ActualMedianFP";
                                    adjmed = "AdjMedFull";
                                    break;
                                case "ASSOCIATE PROFESSOR":
                                    nameType = "ActualMedianAP";
                                    adjmed = "AdjMedAssocP";
                                    break;
                            }

                            rptqry = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + "\\Queries\\COMPSTATS.sql");
                            uhcmd = new SQLiteCommand(rptqry, uhconn);
                            uhcmd.Parameters.AddWithValue("@dept", dept);
                            uhcmd.Parameters.AddWithValue("@job", job);
                            uhcmd.Parameters.AddWithValue("@ActMed", nameType);
                            uhcmd.Parameters.AddWithValue("AdjMed", adjmed);
                            uhcmd.ExecuteNonQuery();

                        }
                    }
                    sqlitetrs.Commit();
                }

            }
        }        

        #endregion
    }

}
