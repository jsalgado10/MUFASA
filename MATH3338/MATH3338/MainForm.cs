using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Web;
using System.Data.SQLite;
using MTPARSERCOMLib;
using System.Data;
using System.Web.UI;
using ClosedXML.Excel;

namespace MATH3338
{
    public partial class MainForm : Form
    {
        private SQLiteData sqlData;
        private ini config = new ini(Directory.GetCurrentDirectory().ToString() + "\\Math.ini");
        MTParser parser;
        MTExcepData parserError;

        private enum appState { Idle, Busy };

        private enum appFct { import, calculate, importCompression, calculateCompression, importUHEquity, calculateUHEquity, importT1Equity, calculateT1Equity};

        private string file = "";

        private string[] iEJobs = new string[2];

        private string[] eEJobs = new string[2];

        private string[] deptArray;

        private string[] jobArray;

        string opt = "";
        bool calcAll = false;
        bool extCalcAll = false;

        public MainForm()
        {
            InitializeComponent();
            sqlData = new SQLiteData(statWorker);         
            fctInit();
            filterOpt.SelectedIndex = 0;
            sqlData.initDataBase();
        }

        public void fctInit()
        {
            parser = new MTParser();
            parserError = new MTExcepData();
            parser.defineFunc(new UHmedian());
            parser.defineFunc(new UhMean());
            parser.defineFunc(new CompMean());
            parser.defineFunc(new CompMedian());
            parser.defineFunc(new CompAdjMedian());
            getFuncHelp();
        }

        public void getFuncHelp()
        {
            sMTFunction w;
            string helpstr = "";
            List<string> eqHelpitems = new List<string>();
            int funcNo=parser.getNbDefinedFuncs();
            for (int i = 0; i < funcNo; i++)
            {
                w=parser.getFunc(i);
                helpstr = w.helpString;
                eqHelpitems.Add(helpstr);
            }
            eqHelp.DataSource = eqHelpitems;
            eqHelp.Refresh();
            
        }



        private void fillFilter(string table)
        {
            //This Method will update Combo Boxes for each tab
            switch (table)
            {
                case "DB":
                    jobTitleListBox.DataSource = sqlData.getFilter("JobTitle", table);
                    jobTitleListBox.Refresh();
                    deptIDListBox.DataSource = sqlData.getFilter("DeptId", table);
                    deptIDListBox.Refresh();
                    dataImportReminder.Visible = false;
                    dataCalculateReminder.Visible = true;
                    uhDataGrid.DataSource = "";
                    uhDataGrid.Refresh();
                    jobKey.DataSource = new BindingSource(sqlData.jobDic, null);
                    jobKey.DisplayMember = "Value";
                    jobKey.ValueMember = "Key";
                    jobKey.Refresh();

                    break;
                case "COMP":
                    cDeptListBox.DataSource = sqlData.getFilter("DeptId", table);
                    cDeptListBox.Refresh();
                    compImportReminder.Visible = false;
                    compCalcReminder.Visible = true;
                    break;
                case "UHEQTY":
                    eDeptListBox.DataSource = sqlData.getFilter("DeptID", table);
                    eDeptListBox.Refresh();
                    iEJobN.DataSource = sqlData.getFilter("JobTitle", table);
                    iEJobN.Refresh();
                    iEJobD.DataSource = sqlData.getFilter("JobTitle", table);
                    iEJobD.Refresh();
                    eqtyImportReminder.Visible = false;
                    eqtyCalcReminder.Visible = true;
                    break;
                case "T1EQTY":
                    eENumerator.DataSource = sqlData.getFilter("JobTitle", table);
                    eENumerator.Refresh();
                    eEDenominator.DataSource = sqlData.getFilter("JobTitle", table);
                    eEDenominator.Refresh();
                    eEqtyCalculateReminder.Visible = true;
                    eEqtyImportremind.Visible = false;
                    break;
                default:
                    break;
            }
            resetAllFilters(table);
            controlStatus(true,table);

        }

        private void updateGrid(string table)
        {
            switch (table)
            {
                case "DB":
                    dataCalculateReminder.Visible = false;
                    if (opt == "SUMMARY")
                    {
                        uhDataGrid.DataSource = sqlData.getGridData(config.Read("GRID", table + "GRID"));
                        uhDataGrid.Refresh();


                        //Disable Column Sort
                        uhDataGrid.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                        uhDataGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        uhDataGrid.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                        uhDataGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        uhDataGrid.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                        uhDataGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        uhDataGrid.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
                        uhDataGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        uhDataGrid.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
                        uhDataGrid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        uhDataGrid.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;
                        uhDataGrid.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        //Set Row Number
                        foreach (DataGridViewRow row in uhDataGrid.Rows)
                        {
                            row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
                        }

                    }
                    else
                    {
                        uhDataGrid.DataSource = sqlData.getGridData(config.Read("GRID", table + "GRID2"));
                        uhDataGrid.Refresh();
                        //Disable Column Sort
                        uhDataGrid.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                        uhDataGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        uhDataGrid.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                        uhDataGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                        uhDataGrid.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                        uhDataGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        //Set Row Number
                        foreach (DataGridViewRow row in uhDataGrid.Rows)
                        {
                            row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
                        }

                        gridCount.Text = String.Format("Total Records: {0}", sqlData.getRowCount("rpt_tab"));
                        medValue.Text = sqlData.getStat("Median", table);
                        p1Value.Text = sqlData.getStat("1st Quartile", table);
                        p3Value.Text = sqlData.getStat("3rd Quartile", table);
                        meanValue.Text = sqlData.getStat("Mean", table);
                    }
                    break;
                case "COMP":
                    compCalcReminder.Visible = false;
                    config.Read("GRID", table + "GRID");
                    compDataGrid.DataSource = sqlData.getGridData(config.Read("GRID", table + "GRID"));
                    compDataGrid.Refresh();                 
                    //Disabling Sorting and align columns
                    //Disable Column Sort
                    compDataGrid.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                    compDataGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    compDataGrid.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                    compDataGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    compDataGrid.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                    compDataGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    compDataGrid.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
                    compDataGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    compDataGrid.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
                    compDataGrid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                    foreach (DataGridViewRow row in compDataGrid.Rows)
                    {
                        row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
                    }
                    if (cDeptListBox.CheckedItems.Count == 1)
                    {
                        adjAsscoMed.Text = sqlData.getStat("AdjMedAssocP", table);
                        adjFullMed.Text = sqlData.getStat("AdjMedFull", table);
                        actualMedAssoc.Text = sqlData.getStat("ActualMedianAP", table);
                        actualMedFull.Text = sqlData.getStat("ActualMedianFP", table);
                        adjAvgAsso.Text = (Convert.ToDouble(actualMedAssoc.Text) / Convert.ToDouble(adjAsscoMed.Text)).ToString("0.00");
                        adjAvgFull.Text = (Convert.ToDouble(actualMedFull.Text) / Convert.ToDouble(adjFullMed.Text)).ToString("0.00");
                    }
                    break;

                case "UHEQTY":
                    eqtyDataGrid.DataSource = sqlData.getGridData(config.Read("GRID", table + "GRID"));
                    eqtyDataGrid.Refresh();
                    eqtyDataGrid.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                    eqtyDataGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    eqtyDataGrid.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                    eqtyDataGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    eqtyDataGrid.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                    eqtyDataGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    eqtyDataGrid.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
                    eqtyDataGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    eqtyDataGrid.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
                    eqtyDataGrid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    eqtyCalcReminder.Visible = false;
                    break;

                case "T1EQTY":
                    eEqtyGrid.DataSource = sqlData.getGridData(config.Read("GRID", table + "GRID"));
                    eEqtyGrid.Refresh();
                    eEqtyCalculateReminder.Visible = false;
                    break;
            }            
        }


        #region BackGround Worker Functions

        //Start process
        private void statWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            switch ((appFct)e.Argument)
            {
                case appFct.import:
                    sqlData.importData(file,"DB");
                    break;

                case appFct.calculate:
                    if (opt == "SUMMARY")
                    {
                        sqlData.uhStats(deptArray, jobArray, "DB");
                    }
                    else if(opt=="SEARCH")
                    {
                        sqlData.Fillreport(getFilterString("DB"), "DB");
                    }
                    break;

                case appFct.importCompression:
                    sqlData.importData(file,"COMP");
                    break;

                case appFct.calculateCompression:
                    sqlData.compRatio(deptArray,kBox.Text, dBox.Text,"COMP");                    
                    break;

                case appFct.importUHEquity:
                    sqlData.importData(file,"UHEQTY");
                    break;

                case appFct.calculateUHEquity:
                    sqlData.Eqty(iEJobs, deptArray, "UHEQTY",calcAll);
                    break;

                case appFct.importT1Equity:
                    sqlData.importData(file, "T1EQTY");
                    break;

                case appFct.calculateT1Equity:
                    sqlData.Eqty(eEJobs, deptArray, "T1EQTY",extCalcAll);
                    break;
            }

            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        //Process Complete
        private void statWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                //Error Occurred
                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message, "An Error was Encounter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cancel by User
                else if (e.Cancelled)
                {
                    MessageBox.Show("Process Was Canceled by User", "Cancel by User", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Process Completed
                else
                {
                    MessageBox.Show("Process Completed Successfully", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
            }
        }

        //Progress Changed
        private void statWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.UserState as string)
            {
                case "FilterDB":
                    fillFilter("DB");
                    break;
                case "FilterCOMP":
                    fillFilter("COMP");
                    break;
                case "FilterUHEQTY":
                    fillFilter("UHEQTY");
                    break;
                case "FilterT1EQTY":
                    fillFilter("T1EQTY");
                    break;
                case "GridViewDB":
                    updateGrid("DB");
                    break;
                case "GridViewCOMP":
                    updateGrid("COMP");
                    break;
                case "GridViewUHEQTY":
                    updateGrid("UHEQTY");
                    break;
                case "GridViewT1EQTY":
                    updateGrid("T1EQTY");
                    break;
                default:
                    break;
            }
        }

        #endregion BackGround Worker Functions

        private string fileDialogOpen(string action)
        {
            string fileNameString = "";


            if (action == "open")
            {

                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

                dialog.Title = "Select a data file";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileNameString = dialog.FileName;
                }
            }
            else if (action == "save")
            {
                SaveFileDialog dialog = new SaveFileDialog();

                dialog.Filter = "Excel Files|*.xlsx;*.xlsm";

                dialog.Title = "Select a save name";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileNameString = dialog.FileName;
                }
                else if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    return "cancel";
                }
            }
            else if(action=="openWs")
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "WorkSpace Files|*.mfsw";
                dialog.Title = "Open Saved Workspace";
                if(dialog.ShowDialog()==DialogResult.OK)
                {
                    fileNameString = dialog.FileName;
                }
                else
                {
                    return "cancel";
                }
            }
            else if(action=="saveWS")
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "WorkSpace|*.mfsw";
                dialog.Title = "Salect Save Destination";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileNameString = dialog.FileName;
                }
                else if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    return "cancel";
                }
            }

            return fileNameString;
        }

        private string getFilterString(string source)
        {
            string filterString = "";
            string jobTitleString = "";
            string deptIDString = "";
            switch (source)
            {
                case "DB":
                    if (jobTitleListBox.CheckedItems.Contains("All") || (jobTitleListBox.CheckedItems.Count == 0))
                    {
                        jobTitleString = "";
                    }
                    else
                    {
                        foreach (string item in jobTitleListBox.CheckedItems)
                        {
                            jobTitleString = jobTitleString + "'" + item + "'" + ",";
                        }

                        jobTitleString = "JobTitle In(" + jobTitleString.Substring(0, (jobTitleString.Length - 1)) + ") AND ";

                    }

                    if (deptIDListBox.CheckedItems.Contains("All") || (deptIDListBox.CheckedItems.Count == 0))
                    {
                        deptIDString = "";

                        if (!(jobTitleListBox.CheckedItems.Contains("All")) & !((jobTitleListBox.CheckedItems.Count == 0)))
                        {
                            jobTitleString = jobTitleString.Substring(0, (jobTitleString.Length - 4));
                        }

                    }
                    else
                    {
                        foreach (string item in deptIDListBox.CheckedItems)
                        {
                            deptIDString = deptIDString + "'" + item + "'" + ",";
                        }
                        if (String.IsNullOrEmpty(jobTitleString))
                        {
                            deptIDString = "DeptId In(" + deptIDString.Substring(0, (deptIDString.Length - 1)) + ")";
                        }
                        else
                        {
                            deptIDString = "DeptId In(" + deptIDString.Substring(0, (deptIDString.Length - 1)) + ")";
                        }

                    }
                    if ((deptIDListBox.CheckedItems.Contains("All") || (deptIDListBox.CheckedItems.Count == 0)) & (jobTitleListBox.CheckedItems.Contains("All") || (jobTitleListBox.CheckedItems.Count == 0)))
                    {
                        filterString = "";
                    }
                    else
                    {
                        filterString = " WHERE " + jobTitleString + deptIDString;
                    }
                    break;

                case "COMP":
                    if (cDeptListBox.CheckedItems.Count>0)
                    {
                        foreach(string citem in cDeptListBox.CheckedItems)
                        {
                            deptIDString = deptIDString + "'" + citem +"'" + ",";
                        }
                        deptIDString="DeptId in("+deptIDString.Substring(0,(deptIDString.Length-1))+")";
                    }
                    filterString = "WHERE " + deptIDString;
                    break;
                case "UHEQTY":
                    if(eDeptListBox.CheckedItems.Count>0)
                    {
                        foreach(string edept in eDeptListBox.CheckedItems)
                        {
                            deptIDString = deptIDString + "'" + edept + "',";
                        }
                        deptIDString = "(" + deptIDString.Substring(0, (deptIDString.Length - 1)) + ")";
                    }
                    filterString = deptIDString;
                    break;
            }
            return filterString;
        }

        private void deptIDListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            allBoxesFunctionality(deptIDListBox,e);
        }

        private void jobTitleListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            allBoxesFunctionality(jobTitleListBox, e);
        }

        private void genderListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            allBoxesFunctionality(genderListBox, e);
        }

        private void cDeptListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            oneBoxFunctionality(cDeptListBox, e);
        }

        private void eDeptListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            oneBoxFunctionality(eDeptListBox, e);
        }

        private void allBoxesFunctionality(CheckedListBox sender, ItemCheckEventArgs i)
        {
            //if the CheckBox checked was the first box(All) then clear all the other entries
            if (sender.Items[i.Index].ToString() == "All")
            {
                foreach (int item in sender.CheckedIndices)
                {
                    if (item == 0)
                    {
                        return;
                    }
                    else
                    {
                        sender.SetItemCheckState(item, CheckState.Unchecked);
                    }
                }
            }
        }

        private void oneBoxFunctionality(CheckedListBox sender, ItemCheckEventArgs i)
        {
            //if a box is checked and another box is checked then clear other box (one box at a time)
            if (sender.CheckedItems.Count > 0)
            {
                foreach (int item in sender.CheckedIndices)
                {
                    if (item == i.Index)
                    {
                        return;
                    }
                    else
                    {
                        sender.SetItemCheckState(item, CheckState.Unchecked);
                    }
                }
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help helpfile = new Help();
            helpfile.ShowDialog();
        }

        private void resetflags(CheckedListBox sender)
        {
            foreach (int item in sender.CheckedIndices)
            {
                sender.SetItemCheckState(item, CheckState.Unchecked);
            }
            sender.SetItemCheckState(0, CheckState.Checked);
        }

        private void resetAllFilters(string tab)
        {
            switch (tab)
            {
                case "DB":
                    resetflags(jobTitleListBox);
                    resetflags(deptIDListBox);
                    resetflags(genderListBox);
                    break;

                case "COMP":
                    resetflags(cDeptListBox);
                    resetflags(cGenderListBox);
                    break;
                case "UHEQTY":
                    resetflags(eDeptListBox);
                    break;
            }
        }

        private void rstFilters_Click(object sender, EventArgs e)
        {
            resetAllFilters("DB");
        }

        private void controlStatus(bool status,string table)
        {
            switch (table)
            {
                case "DB":
                    jobTitleListBox.Enabled = status;
                    deptIDListBox.Enabled = status;
                    genderListBox.Enabled = status;
                    rstFilters.Enabled = status;
                    statCal.Enabled = status;
                    statBoxUH.Enabled = status;
                    uhDataGrid.Enabled = status;

                    exportToolStripMenuItem.Enabled = status;
                    openToolStripMenuItem.Enabled = status;
                    saveWorkSpaceToolStripMenuItem.Enabled = status;
                    addEmployeeDataToolStripMenuItem.Enabled = status;                    
                    break;
                case "COMP":
                    compDataGrid.Enabled = status;
                    actualMedian.Enabled = status;
                    AdjustedMedian.Enabled = status;
                    AdjMedianRatio.Enabled = status;
                    compCalc.Enabled = status;
                    compVars.Enabled = status;
                    cDeptListBox.Enabled = status;
                    cGenderListBox.Enabled = status;

                    exportToolStripMenuItem.Enabled = status;
                    openToolStripMenuItem.Enabled = status;
                    saveWorkSpaceToolStripMenuItem.Enabled = status;
                    addEmployeeDataToolStripMenuItem.Enabled = status;
                    break;
                case "UHEQTY":
                    eqtyDataGrid.Enabled = status;
                    eDeptListBox.Enabled = status;
                    extEqtyGroup.Enabled = status;
                    internalEquityGroup.Enabled = status;

                    exportToolStripMenuItem.Enabled = status;
                    openToolStripMenuItem.Enabled = status;
                    saveWorkSpaceToolStripMenuItem.Enabled = status;
                    addEmployeeDataToolStripMenuItem.Enabled = status;
                    break;
                case "T1EQTY":
                    eqtyDataGrid.Enabled = status;
                    eDeptListBox.Enabled = status;
                    extEqtyGroup.Enabled = status;
                    internalEquityGroup.Enabled = status;

                    exportToolStripMenuItem.Enabled = status;
                    openToolStripMenuItem.Enabled = status;
                    saveWorkSpaceToolStripMenuItem.Enabled = status;
                    addEmployeeDataToolStripMenuItem.Enabled = status;
                    break;
            }
        
            
            
        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void addEmployeeDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddEmployee addData = new AddEmployee();
            addData.ShowDialog();
            fillFilter("DB");
        }

        private void dataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //start import of data
            file = fileDialogOpen("open");
            if (!String.IsNullOrEmpty(file))
            {
                statWorker.RunWorkerAsync(appFct.import);
            }
        }

        private void compressionDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(sqlData.getRowCount("UHDATA")) == 0)
            {
                MessageBox.Show("Import Professor Data First", "An Error was Encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //start import of data
                file = fileDialogOpen("open");
                if (!String.IsNullOrEmpty(file))
                {
                    statWorker.RunWorkerAsync(appFct.importCompression);
                }
            }

        }

        private void clearStats()
        {
            medValue.Text = "";
            p1Value.Text = "";
            p3Value.Text = "";
            meanValue.Text = "";
        }

        private void statCal_Click(object sender, EventArgs e)
        {
            if (jobTitleListBox.CheckedItems.Count == 0 || deptIDListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("Missing Values", "An Error was Encounter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //Fill in array for all dept check
                deptArray = new string[deptIDListBox.CheckedItems.Count];
                int i = 0;
                foreach (string cdept in deptIDListBox.CheckedItems)
                {
                    deptArray[i] = cdept;
                    i++;
                }

                //Fill in array for all job check
                jobArray = new string[jobTitleListBox.CheckedItems.Count];
                int j = 0;
                foreach (string cjob in jobTitleListBox.CheckedItems)
                {
                    jobArray[j] = cjob;
                    j++;
                }
                opt = filterOpt.Text.ToUpper();

                statWorker.RunWorkerAsync(appFct.calculate);
            }
        }

        private void cGenderListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            allBoxesFunctionality(this.cGenderListBox, e);
        }

        private void uHDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(sqlData.getRowCount("UHDATA")) == 0)
            {
                MessageBox.Show("Import Professor Data First", "An Error was Encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //start import of data
                file = fileDialogOpen("open");
                if (!String.IsNullOrEmpty(file))
                {
                    statWorker.RunWorkerAsync(appFct.importUHEquity);
                }
            }
        }

        private void tier1DataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //start import of data
            file = fileDialogOpen("open");
            if (!String.IsNullOrEmpty(file))
            {
                statWorker.RunWorkerAsync(appFct.importT1Equity);
            }
        }

        private void compCalc_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(kBox.Text) || String.IsNullOrEmpty(dBox.Text)||cDeptListBox.CheckedItems.Count==0)
            {
                MessageBox.Show("Missing Values", "An Error was Encounter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //Fill in array for all dept check
                deptArray = new string[cDeptListBox.CheckedItems.Count];
                int i = 0;
                foreach(string cdept in cDeptListBox.CheckedItems)
                {
                    deptArray[i] = cdept;
                    i++;
                }
                statWorker.RunWorkerAsync(appFct.calculateCompression);
            }
        }

        private void eECalc_Click(object sender, EventArgs e)
        {
            if(eDeptListBox.CheckedItems.Count==0)
            {
                MessageBox.Show("Missing Values", "An Error was Encounter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //Fill in array for all dept check
                deptArray = new string[eDeptListBox.CheckedItems.Count];
                int i = 0;
                foreach (string cdept in eDeptListBox.CheckedItems)
                {
                    deptArray[i] = cdept;
                    i++;
                }
                eEJobs= new string[2] { eENumerator.Text, eEDenominator.Text};
                statWorker.RunWorkerAsync(appFct.calculateT1Equity);   
            }
        }

        private void iECalc_Click(object sender, EventArgs e)
        {
            if (eDeptListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("Missing Values", "An Error was Encounter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if(Convert.ToInt32(sqlData.getRowCount("COMP_TAB"))==0)
            {
                MessageBox.Show("Compression Data Missing. Please reimport.", "An Error was Encounter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //Fill in array for all dept check
                deptArray = new string[eDeptListBox.CheckedItems.Count];
                int i = 0;
                foreach (string cdept in eDeptListBox.CheckedItems)
                {
                    deptArray[i] = cdept;
                    i++;
                }
                iEJobs = new string[2] {iEJobN.Text,iEJobD.Text };
                statWorker.RunWorkerAsync(appFct.calculateUHEquity);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                textBox3.Text = parser.evaluate(config.Read("FORMULAS", eqListBox.SelectedItem.ToString())).ToString("0.00");
            }
            catch (Exception eqError) 
            {
                MessageBox.Show(eqError.Message);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            eqListBox.DataSource=config.GetEntryNames("FORMULAS");
            eqListBox.Refresh();
            eqVal.Text = config.Read("FORMULAS", eqListBox.SelectedValue.ToString());

        }

        private void eqListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            eqVal.Text = config.Read("FORMULAS", eqListBox.SelectedValue.ToString());   
        }

        private void addEq_Click(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(newEqName.Text))
            {
                config.Write("FORMULAS", newEqName.Text, "");
                eqListBox.DataSource = config.GetEntryNames("FORMULAS");    
                eqListBox.Refresh();
                eqListBox.SelectedItem = newEqName.Text;
                newEqName.Text = "";
            }
        }

        private void editEq_Click(object sender, EventArgs e)
        {
            eqVal.Enabled = true;
        }

        private void saveEq_Click(object sender, EventArgs e)
        {
            config.Write("FORMULAS", eqListBox.SelectedValue.ToString(), eqVal.Text);
            eqVal.Enabled = false;
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string savePath = fileDialogOpen("save");

            if (savePath != "cancel")
            {

                XLWorkbook wb = new XLWorkbook();

                System.Data.DataTable dt = sqlData.GetDataTableFromDB("select * from UHSTATS");
                wb.Worksheets.Add(dt, "UH Stats");

                dt = sqlData.GetDataTableFromDB("select * from COMPSTATS");
                wb.Worksheets.Add(dt, "Compression Stats");

                dt = sqlData.GetDataTableFromDB("select * from IEQTYSTATS");
                wb.Worksheets.Add(dt, "Internal Equity Stats");

                dt = sqlData.GetDataTableFromDB("select * from EEQTYSTATS");
                wb.Worksheets.Add(dt, "External Equity Stats");

                wb.SaveAs(savePath);

                MessageBox.Show("Data Exported Successfully");
            }

        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options op = new Options();
            op.ShowDialog();
        }

        private void keyValueFormat(object sender, ListControlConvertEventArgs e)
        {

            KeyValuePair<int,string> item = (KeyValuePair<int,string>)e.ListItem;
            e.Value = string.Format("{0} - {1}", item.Key, item.Value);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newFile=fileDialogOpen("openWs");
            if (newFile != "cancel")
            {
                sqlData.currentSpace = newFile;
                sqlData.addDic();
                fillFilter("DB");
                fillFilter("COMP");
                fillFilter("UHEQTY");
                fillFilter("T1EQTY");

                opt = "SUMMARY";
                updateGrid("DB");
                updateGrid("COMP");
                updateGrid("UHEQTY");
                updateGrid("T1EQTY");
                MessageBox.Show("Workspace Successfully Opened");
            }
        }

        private void checkCalcAll_CheckedChanged(object sender, EventArgs e)
        {
            if(checkCalcAll.Checked)
            {
                calcAll = true;
                iEJobN.Enabled = false;
                iEJobD.Enabled = false;
            }
            else
            {
                calcAll = false;
                iEJobN.Enabled = true;
                iEJobD.Enabled = true;
            }
        }

        private void extEqtyAll_CheckedChanged(object sender, EventArgs e)
        {
            if (extEqtyAll.Checked)
            {
                extCalcAll = true;
                eENumerator.Enabled = false;
                eEDenominator.Enabled = false;
            }
            else
            {
                extCalcAll = false;
                eENumerator.Enabled = true;
                eEDenominator.Enabled = true;
            }
        }

        private void checkStatAll(CheckedListBox sender,bool checkStat)
        {
            for(int i=0;i<sender.Items.Count;i++)
            {
                sender.SetItemChecked(i, checkStat);
            }
        }

        private void eqtyUncheckAll_Click(object sender, EventArgs e)
        {
            checkStatAll(eDeptListBox, false);
        }

        private void eqtyCheckAll_Click(object sender, EventArgs e)
        {
            checkStatAll(eDeptListBox, true);
        }

        private void saveWorkSpaceToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

    }
}