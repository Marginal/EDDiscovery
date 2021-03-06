﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using EDDiscovery.DB;
using System.Diagnostics;
using EDDiscovery2;
using EDDiscovery2.DB;
using System.Globalization;





namespace EDDiscovery
{
    public partial class TravelHistoryControl : UserControl
    {
        string datapath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Frontier_Development_s\\Products"; // \\FORC-FDEV-D-1001\\Logs\\";
        internal List<SystemPosition> visitedSystems;

        public NetLogClass netlog = new NetLogClass();
        List<SystemDist> sysDist = null;
        private SystemPosition currentSysPos = null;
        private EDSCClass edsc;


        private static RichTextBox static_richTextBox;
        public TravelHistoryControl()
        {
            InitializeComponent();
            static_richTextBox = richTextBox_History;
            edsc = new EDSCClass();
        }

        private void button_RefreshHistory_Click(object sender, EventArgs e)
        {
            visitedSystems = null;
            SQLiteDBClass db = new SQLiteDBClass();

            edsc.EDSCGetNewSystems(db);
            db.GetAllSystems();
            RefreshHistory();

            EliteDangerous.CheckED();
        }


     

        static public void LogText(string text)
        {
            LogText(text, Color.Black);
        }

        static public void LogText( string text, Color color)
        {
            try
            {
                
                static_richTextBox.SelectionStart = static_richTextBox.TextLength;
                static_richTextBox.SelectionLength = 0;

                static_richTextBox.SelectionColor = color;
                static_richTextBox.AppendText(text);
                static_richTextBox.SelectionColor = static_richTextBox.ForeColor;




                static_richTextBox.SelectionStart = static_richTextBox.Text.Length;
                static_richTextBox.SelectionLength = 0;
                static_richTextBox.ScrollToCaret();
                static_richTextBox.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception SystemClass: " + ex.Message);
                System.Diagnostics.Trace.WriteLine("Trace: " + ex.StackTrace);
            }
        }


        private void setRowNumber(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Value = (row.Index + 1).ToString();
            }
        }

        public void RefreshHistory()
        {
            Stopwatch sw1 = new Stopwatch();
            //richTextBox_History.Clear();


            sw1.Start();


            TimeSpan maxDataAge;

            switch (comboBoxHistoryWindow.SelectedIndex)
            {
                case 0:
                    maxDataAge = new TimeSpan(6, 0, 0); // 6 hours
                    break;
                case 1:
                    maxDataAge = new TimeSpan(12, 0, 0); // 12 hours
                    break;
                case 2:
                    maxDataAge = new TimeSpan(24, 0, 0); // 24 hours
                    break;
                case 3:
                    maxDataAge = new TimeSpan(3 * 24, 0, 0); // 3 days
                    break;
                case 4:
                    maxDataAge = new TimeSpan(7 * 24, 0, 0); // 1 week
                    break;
                case 5:
                    maxDataAge = new TimeSpan(14 * 24, 0, 0); // 2 weeks
                    break;
                case 6:
                    maxDataAge = new TimeSpan(30, 0, 0, 0); // 30 days (month)
                    break;
                case 7:
                    maxDataAge = new TimeSpan(100000, 24, 0, 0); // all
                    break;
                default:
                    maxDataAge = new TimeSpan(7 * 24, 0, 0); // 1 week (default)
                    break;
            }

            DateTime oldestData = DateTime.Now.Subtract(maxDataAge);

            if (visitedSystems==null || visitedSystems.Count == 0)
                visitedSystems = netlog.ParseFiles(richTextBox_History);


            if (visitedSystems == null)
                return;

            //var result = visitedSystems.OrderByDescending(a => a.time).ToList<SystemPosition>();

            var resultsystems = from systems in visitedSystems where systems.time > oldestData orderby systems.time descending select systems;
            var result = resultsystems.ToList<SystemPosition>();

            //DataTable dt = new DataTable();
            //dataGridView1.Columns.Clear();
            //dt.Columns.Add("Time");
            //dt.Columns.Add("System");
            //dt.Columns.Add("Distance");


            dataGridView1.Rows.Clear();

            //dataGridView1.DataSource = dt;

            System.Diagnostics.Trace.WriteLine("SW1: " + (sw1.ElapsedMilliseconds / 1000.0).ToString("0.000"));


            for (int ii = 0; ii < result.Count; ii++) //foreach (var item in result)
            {
      
                SystemPosition item = result[ii];
                SystemPosition item2;

                if (ii < result.Count - 1)
                    item2 = result[ii + 1];
                else
                    item2 = null;

                AddHistoryRow(oldestData, item, item2);
            }

            System.Diagnostics.Trace.WriteLine("SW2: " + (sw1.ElapsedMilliseconds / 1000.0).ToString("0.000"));

            //setRowNumber(dataGridView1);

            if (dataGridView1.Rows.Count > 0)
            {
                lastRowIndex = 0;
                ShowSystemInformation((SystemPosition)(dataGridView1.Rows[0].Cells[1].Tag));
            }
            System.Diagnostics.Trace.WriteLine("SW3: " + (sw1.ElapsedMilliseconds / 1000.0).ToString("0.000"));
            sw1.Stop();

        }

        private void AddHistoryRow(DateTime oldestData, SystemPosition item, SystemPosition item2)
        {
            SystemClass sys1 = null, sys2;
            double dist;

            sys1 = SystemData.GetSystem(item.Name);
            if (sys1 == null)
            {
                sys1 = new SystemClass(item.Name);
                if (SQLiteDBClass.globalSystemNotes.ContainsKey(sys1.SearchName))
                {
                    sys1.Note = SQLiteDBClass.globalSystemNotes[sys1.SearchName].Note;
                }
            }
            if (item2 != null)
            {
                sys2 = SystemData.GetSystem(item2.Name);
                if (sys2 == null)
                    sys2 = new SystemClass(item2.Name);

            }
            else
                sys2 = null;

            item.curSystem = sys1;
            item.prevSystem = sys2;


            string diststr = "";
            dist = 0;
            if (sys2 != null)
            {

                if (sys1.HasCoordinate && sys2.HasCoordinate)
                    dist = SystemData.Distance(sys1, sys2);
                else
                {

                    dist = DistanceClass.Distance(sys1, sys2);
                }

                if (dist > 0)
                    diststr = dist.ToString("0.00");
            }

            item.strDistance = diststr;

            //richTextBox_History.AppendText(item.time + " " + item.Name + Environment.NewLine);

            if (item.time.Subtract(oldestData).TotalDays > 0)
            {
                object[] rowobj = new object[] { item.time, item.Name, diststr, item.curSystem.Note };
                int rownr;

                if (oldestData.Year == 1990)
                {
                    dataGridView1.Rows.Insert(0, rowobj);
                    rownr = 0;
                }
                else
                {
                    dataGridView1.Rows.Add(rowobj);
                    rownr = dataGridView1.Rows.Count - 1;
                }

                var cell = dataGridView1.Rows[rownr].Cells[1];

                cell.Tag = item;

                if (!sys1.HasCoordinate)  // Mark all systems without coordinates
                    cell.Style.ForeColor = Color.Blue;
            }
        }



        private void ShowSystemInformation(SystemPosition syspos)
        {
            if (syspos == null || syspos.Name==null)
                return;

            currentSysPos = syspos;
            textBoxSystem.Text = syspos.curSystem.name;
            textBoxDistance.Text = syspos.strDistance;
          

            if (syspos.curSystem.HasCoordinate)
            {
                textBoxX.Text = syspos.curSystem.x.ToString("#.#####");
                textBoxY.Text = syspos.curSystem.y.ToString("#.#####");
                textBoxZ.Text = syspos.curSystem.z.ToString("#.#####");
            }
            else
            {
                textBoxX.Text = "?";
                textBoxY.Text = "?";
                textBoxZ.Text = "?";

            }

            int count = GetVisitsCount(syspos.curSystem.name);
            textBoxVisits.Text = count.ToString();

            richTextBoxNote.Text = syspos.curSystem.Note;

            bool distedit = false;

            if (syspos.prevSystem != null)
            {
                textBoxPrevSystem.Text = syspos.prevSystem.name;

                if (syspos.curSystem.status == SystemStatusEnum.Unknown || syspos.prevSystem.status == SystemStatusEnum.Unknown)
                    distedit = true;

            }

            textBoxDistance.Enabled = distedit;
            buttonUpdate.Enabled = distedit;
            



            ShowClosestSystems(syspos.Name);
        }

        private void ShowClosestSystems(string name)
        {
            sysDist = new List<SystemDist>();
            SystemClass LastSystem = null;
            float dx, dy, dz;
            double dist;

            try
            {
                if (name == null)
                {

                    var result = visitedSystems.OrderByDescending(a => a.time).ToList<SystemPosition>();


                    for (int ii = 0; ii < result.Count; ii++) //foreach (var item in result)
                    {
                        SystemPosition item = result[ii];

                        LastSystem = SystemData.GetSystem(item.Name);
                        name = item.Name;
                        if (LastSystem != null)
                            break;
                    }

                }
                else
                {
                    LastSystem = SystemData.GetSystem(name);
                }

                if (name !=null)
                    label3.Text = "Closest systems from " + name.ToString();

                listView1.Items.Clear();

                if (LastSystem == null)
                    return;

                foreach (SystemClass pos in SystemData.SystemList)
                {

                    dx = (float)(pos.x - LastSystem.x);
                    dy = (float)(pos.y - LastSystem.y);
                    dz = (float)(pos.z - LastSystem.z);
                    dist = dx * dx + dy * dy + dz * dz;

                    //distance = (float)((system.x - arcsystem.x) * (system.x - arcsystem.x) + (system.y - arcsystem.y) * (system.y - arcsystem.y) + (system.z - arcsystem.z) * (system.z - arcsystem.z));

                    if (dist > 0)
                    {
                        SystemDist sdist = new SystemDist();
                        sdist.name = pos.name;
                        sdist.dist = Math.Sqrt(dist);
                        sysDist.Add(sdist);
                    }
                }


                var list = (from t in sysDist orderby t.dist select t).Take(50);



                foreach (SystemDist sdist in list)
                {
                    ListViewItem item = new ListViewItem(sdist.name);
                    item.SubItems.Add(sdist.dist.ToString("0.00"));
                    listView1.Items.Add(item);
                }



            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception : " + ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

               
            }

        }


        private void TravelHistoryControl_Load(object sender, EventArgs e)
        {
            //if (!this.DesignMode)
            //    RefreshHistory();
            comboBoxHistoryWindow.SelectedIndex = 4;

            // this improves dataGridView's scrolling performance
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null,
                dataGridView1,
                new object[] { true }
            );
        }

        private void comboBoxHistoryWindow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (visitedSystems != null)
                RefreshHistory();
        }


        private void dgv_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewSorter.DataGridSort(dataGridView1, e.ColumnIndex);
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                //string  SysName = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                lastRowIndex = e.RowIndex;

                ShowSystemInformation((SystemPosition)(dataGridView1.Rows[e.RowIndex].Cells[1].Tag));
            }

        }

    

        private void button2_Click(object sender, EventArgs e)
        {
            EDSCClass edsc = new EDSCClass();

            //string json = edsc.SubmitDistances("Finwen", "19 Geminorum", "HIP 30687", (float)19.26);


            FormMap map2 = new FormMap();
            map2.visitedSystems = visitedSystems;
            map2.Show();

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }
        private int lastRowIndex;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                //string SysName = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                lastRowIndex = e.RowIndex;
                ShowSystemInformation((SystemPosition)(dataGridView1.Rows[e.RowIndex].Cells[1].Tag));
            }
 
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            double dist;

            NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
            string decimalSeparator = numberFormatInfo.NumberDecimalSeparator;

            if (decimalSeparator.Equals(","))  // Allow regions with , as decimal separator to  also use . as decimal separator
                textBoxDistance.Text = textBoxDistance.Text.Replace(".", ",");


            if (!Double.TryParse(textBoxDistance.Text.Trim(), out dist))
                MessageBox.Show("Distance in wrong format!");
            else
            {
                DistanceClass distance = new DistanceClass();

                distance.Dist = dist;
                distance.CreateTime = DateTime.UtcNow;
                distance.CommanderCreate = textBoxCmdrName.Text.Trim();
                distance.NameA = textBoxSystem.Text;
                distance.NameB = textBoxPrevSystem.Text;
                distance.Status = DistancsEnum.EDDiscovery;

                distance.Store();

                SQLiteDBClass.globalDistances.Add(distance);

                dataGridView1.Rows[lastRowIndex].Cells[2].Value = textBoxDistance.Text.Trim();

            }
        }

        private void textBoxCmdrName_Leave(object sender, EventArgs e)
        {
            if (!EDDiscoveryForm.CommanderName.Equals(textBoxCmdrName.Text))
            {
                SQLiteDBClass db = new SQLiteDBClass();

                db.PutSettingString("CommanderName", textBoxCmdrName.Text);
                //EDDiscoveryForm.CommanderName = textBoxCmdrName.Text;
            }
        }

        private void richTextBoxNote_Leave(object sender, EventArgs e)
        {

            StoreSystemNote();
        }

        private void StoreSystemNote()
        {
            string txt;

            try
            {


                //SystemPosition sp = (SystemPosition)dataGridView1.Rows[lastRowIndex].Cells[1].Tag;
                txt = richTextBoxNote.Text;

                if (currentSysPos != null && !txt.Equals(currentSysPos.curSystem.Note))
                {
                    SystemNoteClass sn;
                    List<SystemClass> systems = new List<SystemClass>();

                    if (SQLiteDBClass.globalSystemNotes.ContainsKey(currentSysPos.curSystem.SearchName))
                    {
                        sn = SQLiteDBClass.globalSystemNotes[currentSysPos.curSystem.SearchName];
                        sn.Note = txt;
                        sn.Time = DateTime.Now;

                        sn.Update();
                    }
                    else
                    {
                        sn = new SystemNoteClass();

                        sn.Name = currentSysPos.curSystem.name;
                        sn.Note = txt;
                        sn.Time = DateTime.Now;
                        sn.Add();
                    }


                    currentSysPos.curSystem.Note = txt;
                    dataGridView1.Rows[lastRowIndex].Cells[3].Value = txt;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception : " + ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                LogText("Exception : " + ex.Message, Color.Red);
                LogText(ex.StackTrace, Color.Red);
            }
        }

        private void buttonSync_Click(object sender, EventArgs e)
        {
            if (textBoxCmdrName.Text.Equals(""))
            {
                MessageBox.Show("Please enter commander name before sending distances!");
                return;
            }


            var dists = from p in SQLiteDBClass.globalDistances where p.Status == DistancsEnum.EDDiscovery  orderby p.CreateTime  select p;

            EDSCClass edsc = new EDSCClass();

            foreach (var dist in dists)
            {
                string json;

                if (dist.Dist > 0)
                {
                    LogText("Add distance: " + dist.NameA + " => " + dist.NameB + " :" + dist.Dist.ToString("0.00") + "ly" + Environment.NewLine);
                    json = edsc.SubmitDistances(textBoxCmdrName.Text, dist.NameA, dist.NameB, dist.Dist);
                }
                else
                {
                    dist.Delete();
                    return;
                }
                if (json != null)
                {
                    string str="";
                    if (edsc.ShowDistanceResponce(json, out str))
                    {
                        LogText(str);
                        dist.Status = DistancsEnum.EDDiscoverySubmitted;
                        dist.Update();
                    }
                    else
                    {
                        LogText(str);
                    }
                }
            }

        }


        internal void NewPosition(object source)
        {
            try
            {
                string name = netlog.visitedSystems.Last().Name;
                Invoke((MethodInvoker)delegate
                {

                    LogText("Arrived to system: ");
                    SystemClass sys1 = SystemData.GetSystem(name);
                    if (sys1 == null || sys1.HasCoordinate == false)
                        LogText(name , Color.Blue);
                    else
                        LogText(name );


                    int count = GetVisitsCount(name);

                    LogText("  : Vist nr " + count.ToString()  + Environment.NewLine);
                    System.Diagnostics.Trace.WriteLine("Arrived to system: " + name + " " + count.ToString() + ":th visit.");

                    var result = visitedSystems.OrderByDescending(a => a.time).ToList<SystemPosition>();

                    SystemPosition item = result[0];
                    SystemPosition item2;

                    if (result.Count > 1)
                        item2 = result[1];
                    else
                        item2 = null;


                    AddHistoryRow(new DateTime(1990, 1, 1), item, item2);
                    lastRowIndex += 1;
                    StoreSystemNote();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception NewPosition: " + ex.Message);
                System.Diagnostics.Trace.WriteLine("Trace: " + ex.StackTrace);
            }
        }

        private int GetVisitsCount(string name)
        {
            int count = (from row in visitedSystems
                         where row.Name == name
                         select row).Count();
            return count;
        }

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);

        }




    }



    public class SystemDist
    {
        public string name;
        public double dist;
    }




}