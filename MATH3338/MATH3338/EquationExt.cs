using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MTPARSERCOMLib;

namespace MATH3338
{
    public class UHmedian:IMTFunction
    {
        
        private double resultVal;
        private string whereclause="";
        SQLiteData sql = new SQLiteData();

        public double evaluate0()
        {
            throw new Exception("Error No Arguments on Function");
        }

        //Get UH Median for 1 dept
        public double evaluate1(double deptid)
        {
            string deptval = String.Format("H{0}", deptid.ToString("0000"));
            whereclause = "where deptid in('" + deptval + "')";
            resultVal=sql.calcMedian(whereclause, "DB");
            return resultVal;

        }

        //Get UH Median for 1 dept and 1 job
        public double evaluate2(double deptid,double job)
        {
            string deptval = String.Format("H{0}", deptid.ToString("0000"));
            sql.addDic();
            string jobVal = sql.jobDic[Convert.ToInt32(job)];
            string where = String.Format("where deptid in ('{0}') and jobtitle in ('{1}')", deptval, jobVal);
            resultVal = sql.calcMedian(where, "DB");            
            return resultVal;
        }

        public double evaluate3(double x,double y, double z)
        {
            throw new Exception("Too Many Arguments");
        }

        public double evaluate(Array arg)
        {
            throw new Exception("Too Many Arguments");
        }

        public string getDescription()
        {
            return "Compute median of UH Main data given deptid and job title";
        }

        public string getHelpString()
        {
            return "uh.median(deptid) or uh.median(deptid,jobtitleid)";
        }

        public int getNbArgs()
        {
            return -1;
        }

        public string getSymbol()
        {
            return "uh.median";
        }
    }

    public class UhMean:IMTFunction
    {

        private double resultVal;
        private string whereclause = "";        
        SQLiteData sql = new SQLiteData();

        public double evaluate0()
        {
            throw new Exception("Error No Arguments on Function");
        }

        //Get UH Median for 1 dept
        public double evaluate1(double deptid)
        {
            string deptval = String.Format("H{0}", deptid.ToString("0000"));
            whereclause = "where deptid in('" + deptval + "')";
            resultVal = sql.calcMean(whereclause, "DB");
            return resultVal;

        }

        //Get UH Median for 1 dept and 1 job
        public double evaluate2(double deptid, double job)
        {
            string deptval = String.Format("H{0}", deptid.ToString("0000"));
            sql.addDic();
            string jobVal = sql.jobDic[Convert.ToInt32(job)];
            string where = String.Format("where deptid in ('{0}') and jobtitle in ('{1}')", deptval, jobVal);
            resultVal = sql.calcMean(where, "DB");
            return resultVal;
        }

        public double evaluate3(double x, double y, double z)
        {
            throw new Exception("Too Many Arguments");
        }

        public double evaluate(Array arg)
        {
            throw new Exception("Too Many Arguments");
        }

        public string getDescription()
        {
            return "Compute median of UH Main data given deptid and job title";
        }

        public string getHelpString()
        {
            return "uh.mean(deptid) or uh.mean(deptid,jobtitleid)";
        }

        public int getNbArgs()
        {
            return -1;
        }

        public string getSymbol()
        {
            return "uh.mean";
        }
    }

    public class CompMean:IMTFunction
    {

        private double resultVal;
        private string whereclause = "";
        SQLiteData sql = new SQLiteData();

        public double evaluate0()
        {
            throw new Exception("Error No Arguments on Function");
        }

        //Get UH Median for 1 dept
        public double evaluate1(double deptid)
        {
            string deptval = String.Format("H{0}", deptid.ToString("0000"));
            whereclause = "where deptid in('" + deptval + "')";
            resultVal = sql.calcMean(whereclause, "COMP");
            return resultVal;

        }

        public double evaluate2(double deptid, double job)
        {
            throw new Exception("Too Many Arguments");
        }

        public double evaluate3(double x, double y, double z)
        {
            throw new Exception("Too Many Arguments");
        }

        public double evaluate(Array arg)
        {
            throw new Exception("Too Many Arguments");
        }

        public string getDescription()
        {
            return "Compute mean of Compression data given deptid";
        }

        public string getHelpString()
        {
            return "cmp.mean(deptid)";
        }

        public int getNbArgs()
        {
            return -1;
        }

        public string getSymbol()
        {
            return "cmp.mean";
        }
    }

    public class CompMedian:IMTFunction
    {

        private double resultVal;
        private string whereclause = "";
        SQLiteData sql = new SQLiteData();

        public double evaluate0()
        {
            throw new Exception("Error No Arguments on Function");
        }

        //Get UH Median for 1 dept
        public double evaluate1(double deptid)
        {
            string deptval = String.Format("H{0}", deptid.ToString("0000"));
            whereclause = "where deptid in('" + deptval + "')";
            resultVal = sql.calcMedian(whereclause, "COMP");
            return resultVal;

        }

        public double evaluate2(double deptid, double job)
        {
            throw new Exception("Too Many Arguments");
        }

        public double evaluate3(double x, double y, double z)
        {
            throw new Exception("Too Many Arguments");
        }

        public double evaluate(Array arg)
        {
            throw new Exception("Too Many Arguments");
        }

        public string getDescription()
        {
            return "Compute median of Compress data given deptid";
        }

        public string getHelpString()
        {
            return "cmp.median(deptid)";
        }

        public int getNbArgs()
        {
            return -1;
        }

        public string getSymbol()
        {
            return "cmp.median";
        }
    }

    public class CompAdjMedian:IMTFunction
    {

        private double resultVal;
        private string nameType = "";
        SQLiteData sql = new SQLiteData();

        public double evaluate0()
        {
            throw new Exception("Error No Arguments on Function");
        }

        //Get UH Median for 1 dept
        public double evaluate1(double deptid)
        {

            throw new Exception("Error No Arguments on Function");

        }

        public double evaluate2(double deptid, double job)
        {
            throw new Exception("Error No Arguments on Function");
        }

        public double evaluate3(double x, double y, double z)
        {
            throw new Exception("Error No Arguments on Function");
        }

        //deptid,jobtitle,k,d
        public double evaluate(Array arg)
        {
            //Make sure Dictionary has correct values
            sql.addDic();
            nameType = "";
            double deptid =Convert.ToDouble(arg.GetValue(0));
            string job = sql.jobDic[Convert.ToInt32(arg.GetValue(1))];
            string [] deptval =new string[1]{String.Format("H{0}", deptid.ToString("0000"))};
            string k = arg.GetValue(2).ToString();
            string d = arg.GetValue(3).ToString();
            switch (job)
            {
                case "PROFESSOR":
                    nameType = "AdjMedFull";
                    break;
                case "ASSOCIATE PROFESSOR":
                    nameType = "AdjMedAssocP";
                    break;
            }

            sql.calcAdj(deptval, k, d, "COMP");
            resultVal = Convert.ToDouble(sql.getStat(nameType, "COMP"));
            return resultVal;
        }

        public string getDescription()
        {
            return "Compute adjusted median of Compress data given deptid";
        }

        public string getHelpString()
        {
            return "cmp.adjmedian(deptid,jobid,k,d)";
        }

        public int getNbArgs()
        {
            return -1;
        }

        public string getSymbol()
        {
            return "cmp.adjmedian";
        }

    }
}
