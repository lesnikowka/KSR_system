using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections;
using System.Windows.Forms.VisualStyles;
using System.Numerics;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Globalization;
using System.IO;

namespace lab1_dotnet_framework
{
    enum TaskType
    {
        Test,
        Main1,
        Main2
    }

    public partial class MainForm : Form
    {
        
                private DataBase db = null;
                private DataTable table = new DataTable();
                private DataTable table2 = new DataTable();

                private List<string> columnNames = new List<string> { "id", "xi", "vi", "v2i", "|vi-v2i|", "|olp|", "hi", "Делений", "Удвоений", "u", "|u-v|" };
                private List<string> columnNamesForDerivative = new List<string> { "id", "xi", "v'i", "v'2i", "|v'i-v'2i|", "|olp|", "hi", "Делений", "Удвоений" };

                private Dictionary<Tuple<double, double, double>, List<Series>> SeriesForStartConditions = new Dictionary<Tuple<double, double, double>, List<Series>>();

                private TaskType selectedTask = TaskType.Main2;
                private string currentTableDB = "main2";

               // private string bdFolder = "/database/lab1.sqlite3";
               // private string scriptFolder = "\\script"; 

                private string bdFolder = "/../../../database/lab1.sqlite3";
                private string scriptFolder = "\\..\\..\\..\\script";



        public MainForm()
        {
            InitializeComponent();

            chart1.ChartAreas[0].BackColor = Color.Transparent;
            chart2.ChartAreas[0].BackColor = Color.Transparent;
            chart3.ChartAreas[0].BackColor = Color.Transparent;

            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "0.00001";
            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "0.00001";
            chart3.ChartAreas[0].AxisX.LabelStyle.Format = "0.00001";

            chart1.ChartAreas[0].AxisX.Title = "X";
            chart1.ChartAreas[0].AxisY.Title = "U";

            chart1.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 14);
            chart1.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 14);


            chart3.ChartAreas[0].AxisX.Title = "X";
            chart3.ChartAreas[0].AxisY.Title = "U'";

            chart2.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 14);
            chart2.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 14);

            chart2.ChartAreas[0].AxisX.Title = "U";
            chart2.ChartAreas[0].AxisY.Title = "U'";

            chart3.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 14);
            chart3.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 14);

            chart1.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

            chart2.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chart2.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chart2.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

            chart3.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chart3.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chart3.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart3.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                db = new DataBase(bdFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }


            for (int i = 0; i < columnNames.Count - 2; i++)
            {
                table.Columns.Add(columnNames[i], typeof(string));
            }

            for (int i = 0; i < columnNamesForDerivative.Count; i++)
            {
                table2.Columns.Add(columnNamesForDerivative[i], typeof(string));
            }

            dataGridView1.DataSource = table;
            dataGridView2.DataSource = table2;

            dataGridView1.Columns["|olp|"].Width = 200; 
            dataGridView1.Columns["|vi-v2i|"].Width = 200; 
            dataGridView1.Columns["id"].Width = 50; 
            dataGridView1.Columns["Делений"].Width = 95; 
            dataGridView1.Columns["Удвоений"].Width = 95;

            dataGridView2.Columns["|olp|"].Width = 200;
            dataGridView2.Columns["|v'i-v'2i|"].Width = 200;
            dataGridView2.Columns["id"].Width = 50;
            dataGridView2.Columns["Делений"].Width = 95;
            dataGridView2.Columns["Удвоений"].Width = 95;

            /*textBox8.Enabled = false;
            textBox9.Enabled = false;
            textBox10.Enabled = false;*/
        }

        private string replacePI(string s)
        {
            return s.Replace("pi", "3,14159265359");
        }

        private int catchParams(ref double X0, ref double U0, ref double U0der, ref double startStep, ref double localPrecision, ref double boundPrecision, ref double integrationBound, ref int maxStepNumbers, ref bool withControl, ref double a, ref double b, ref double c)
        {
            string x0Text = replacePI(pointsToCommas(textBox1.Text));
            string u0Text = replacePI(pointsToCommas(textBox2.Text));
            string startStepText = replacePI(pointsToCommas(textBox3.Text));
            string localPrecisionText = replacePI(pointsToCommas(textBox4.Text));
            string boundPrecisionText = replacePI(pointsToCommas(textBox5.Text));
            string maxStepNumbersText = pointsToCommas(textBox6.Text);
            string integrationBoundText = replacePI(pointsToCommas(textBox7.Text));
            string aText = replacePI(pointsToCommas(textBox8.Text));
            string bText = replacePI(pointsToCommas(textBox9.Text));
            string cText = replacePI(pointsToCommas(textBox10.Text));
            string u0derText = replacePI(pointsToCommas(textBox11.Text));

            if (x0Text.Length == 0 || u0Text.Length == 0 ||
                startStepText.Length == 0 || localPrecisionText.Length == 0 ||
                boundPrecisionText.Length == 0 || maxStepNumbersText.Length == 0 ||
                integrationBoundText.Length == 0 || aText.Length == 0 || bText.Length == 0 || 
                cText.Length == 0 || u0derText.Length == 0
                )
            {
                MessageBox.Show("Вы ввели не все параметры", "Ошибка");
                return -1;
            }

            try
            {
                X0 = Convert.ToDouble(x0Text);
                U0 = Convert.ToDouble(u0Text);
                startStep = Convert.ToDouble(startStepText);
                localPrecision = Convert.ToDouble(localPrecisionText);
                boundPrecision = Convert.ToDouble(boundPrecisionText);
                maxStepNumbers = Convert.ToInt32(maxStepNumbersText);
                integrationBound = Convert.ToDouble(integrationBoundText);
                a = Convert.ToDouble(aText);
                b = Convert.ToDouble(bText);
                c = Convert.ToDouble(cText);
                U0der = Convert.ToDouble(u0derText);
            }
            catch
            {
                MessageBox.Show("Неверный формат введенных значений", "Ошибка");
                return -1;
            }

            if (X0 + startStep > integrationBound)
            {
                MessageBox.Show("Некорректная граница интегрирования", "Ошибка");
                return -1;
            }

            withControl = checkBox1.Checked;

            return 0;
        }
        
        private void executeMethod()
        {
            ProcessStartInfo deleteInfoStartProcess = new ProcessStartInfo();

            Process deleteValuesProcess = new Process();

            deleteInfoStartProcess.WorkingDirectory = Directory.GetCurrentDirectory() + scriptFolder;
            deleteInfoStartProcess.FileName = "clear_tables.py";
            deleteInfoStartProcess.WindowStyle = ProcessWindowStyle.Hidden;

            deleteValuesProcess.StartInfo = deleteInfoStartProcess;

            deleteValuesProcess.Start();

            deleteValuesProcess.WaitForExit();

            double X0 = 0, U0 = 0, U0der = 0, startStep = 0, localPrecision = 0, boundPrecision = 0, integrationBound = 0, a = 0, b = 0, c = 0;
            int maxStepNumbers = 0;
            bool withControl = true;

            string tableName = getTableString();

            int valid = catchParams(ref X0, ref U0, ref U0der, ref startStep, ref localPrecision, ref boundPrecision, ref integrationBound, ref maxStepNumbers, ref withControl, ref a, ref b, ref c);

            if (valid != 0) return;
            if (tableName != "main2") U0der = 0;
            
            string args = "";

            args += toStringPoint(X0) + " ";
            args += toStringPoint(U0) + " ";
            args += toStringPoint(startStep) + " ";
            args += toStringPoint(maxStepNumbers) + " ";
            args += toStringPoint(localPrecision) + " ";
            args += toStringPoint(boundPrecision) + " ";
            args += (withControl ? 1 : 0).ToString() +  " ";
            args += toStringPoint(a) + " ";
            args += toStringPoint(b) + " ";
            args += toStringPoint(c) + " ";
            args += tableName + " ";
            args += toStringPoint(integrationBound) + " ";
            args += toStringPoint(U0der) + " ";

            ProcessStartInfo infoStartProcess = new ProcessStartInfo();

            Process methodProcess = new Process();

            infoStartProcess.WorkingDirectory = Directory.GetCurrentDirectory() + scriptFolder;
            infoStartProcess.FileName = "RK.py";
            infoStartProcess.Arguments = args;
            infoStartProcess.WindowStyle = ProcessWindowStyle.Hidden;

            methodProcess.StartInfo = infoStartProcess;

            methodProcess.Start();

            methodProcess.WaitForExit();

            ShowDataForStartCondition(doubleConditionsToList(X0, U0, U0der));

            drawGraphs(doubleConditionsToList(X0, U0, U0der));

            int cntrl = checkBox1.Checked ? 1 : 0;

            db.SaveParameters(new List<string> { toStringPoint(X0), toStringPoint(U0), toStringPoint(U0der), toStringPoint(integrationBound),
                toStringPoint(startStep), toStringPoint(localPrecision), toStringPoint(boundPrecision),
                maxStepNumbers.ToString(), toStringPoint(a), toStringPoint(b), toStringPoint(c), cntrl.ToString()
            }, tableName);


            richTextBox1.Text = getInfo(table, cntrl == 1);

            if (selectedTask == TaskType.Main2)
            {
                richTextBox1.Text += "\nДля производной:\n" + getInfo(table2, cntrl == 1, true);
            }
        }

        private string getTableString()
        {
            string tableName;
            if (selectedTask == TaskType.Test) tableName = "test";
            else if (selectedTask == TaskType.Main1) tableName = "main1";
            else tableName = "main2";

            return tableName;
        }

        private List<string> doubleConditionsToList(double X0, double U0, double U0der)
        {
            return new List<string> { toStringPoint(X0), toStringPoint(U0), toStringPoint(U0der) };
        }

        private string toStringPoint(double val)
        {
            return val.ToString().Replace(",", ".");
        }

        private void drawGraphs(List<string> startCondition)
        {
            var x0u0Tuple = createTuple(startCondition);

            deleteOldSeries(startCondition);

            List<Series> newSeriesList = new List<Series>();

            Series newNumericSeries = new Series();
            newNumericSeries.Name = "Численное решение при X0 = " + startCondition[0] + " U0 = " + startCondition[1];
            if (selectedTask == TaskType.Main2)
                newNumericSeries.Name += " U'0 = " + startCondition[2];
            newNumericSeries.ChartType = SeriesChartType.Line;
            newNumericSeries.BorderWidth = 2;
            this.chart1.Series.Add(newNumericSeries);
            newSeriesList.Add(newNumericSeries);

            Series newTrueSeries = new Series();

            if (selectedTask == TaskType.Test)
            {
                newTrueSeries.Name = "Истинное решение при X0 = " + startCondition[0] + " U0 = " + startCondition[1];
                newTrueSeries.ChartType = SeriesChartType.Line;
                newTrueSeries.BorderWidth = 2;
                this.chart1.Series.Add(newTrueSeries);
                newSeriesList.Add(newTrueSeries);
            }

            if (selectedTask == TaskType.Test)
            {
                DrawNumericSolution(newNumericSeries, null, null, startCondition, newTrueSeries);
            }
            else if (selectedTask == TaskType.Main1)
            {
                DrawNumericSolution(newNumericSeries, null, null, startCondition, null);
            }
            else
            {
                Series newDerivativeSeries = new Series();
                newDerivativeSeries.Name = "Производная при X0 = " + startCondition[0] + " U0 = " + startCondition[1] + " U'0 = " + startCondition[2];
                newDerivativeSeries.ChartType = SeriesChartType.Line;
                newDerivativeSeries.BorderWidth = 2;
                this.chart3.Series.Add(newDerivativeSeries);
                newSeriesList.Add(newDerivativeSeries);


                Series newPhaseSeries = new Series();
                newPhaseSeries.Name = "Фазовая траектория при X0 = " + startCondition[0] + " U0 = " + startCondition[1] + " U'0 = " + startCondition[2];
                newPhaseSeries.ChartType = SeriesChartType.Line;
                newPhaseSeries.BorderWidth = 2;
                this.chart2.Series.Add(newPhaseSeries);
                newSeriesList.Add(newPhaseSeries);

                DrawNumericSolution(newNumericSeries, newDerivativeSeries, newPhaseSeries, startCondition, null);
            }

            SeriesForStartConditions.Add(x0u0Tuple, newSeriesList);

        }

        private Tuple<double, double, double> createTuple(List<string> startCondition)
        {
            double X0 = stringToDouble(startCondition[0]);
            double U0 = stringToDouble(startCondition[1]);
            double U0der = stringToDouble(startCondition[2]);

            Tuple<double, double, double> x0u0Tuple = new Tuple<double, double, double>(X0, U0, U0der);

            return x0u0Tuple;
        }

        private void deleteOldSeries(List<string> startCondition)
        {
            var x0u0Tuple = createTuple(startCondition);

            if (SeriesForStartConditions.ContainsKey(x0u0Tuple))
            {

                List<Series> oldSeries = SeriesForStartConditions[x0u0Tuple];

                for (int i = 0; i < oldSeries.Count; i++)
                {
                    this.chart1.Series.Remove(oldSeries[i]);
                    this.chart2.Series.Remove(oldSeries[i]);
                    this.chart3.Series.Remove(oldSeries[i]);
                }

                SeriesForStartConditions.Remove(x0u0Tuple);

            }
        }

        private string pointsToCommas(string s)
        {
            return s.Replace('.', ',');
        }

        private double stringToDouble(string s)
        {
            return Convert.ToDouble(s.Replace(".", ","));
        }

        private double constant(double x0, double v0)
        {
            return v0 / Math.Exp(2 * x0);
        }

        private double TrueSoluitonFunction(double x0, double v0, double x)
        {
            return Math.Exp(2 * x) * constant(x0, v0);
        }

        private void DrawTrueSolution(Series series, double x0, double u0, List<double> X)
        {

            for (int i = 0; i < X.Count; i++)
            {
                series.Points.AddXY(X[i], TrueSoluitonFunction(x0, u0, X[i]));
            }
        }

        private void DrawNumericSolution(Series mainSeries, Series derSeries, Series phaseSeries, List<string> startCondition, Series testSeries)
        {
            string tableName = getTableString();

            List<List<string>> data = db.GetDataForStartCondition(tableName, startCondition);

            if (selectedTask == TaskType.Test || selectedTask == TaskType.Main1)
            {
                List<double> X = new List<double>();

                for (int i = 0; i < data.Count; i++)
                {
                    mainSeries.Points.AddXY(stringToDouble(data[i][1]), stringToDouble(data[i][2]));
                    X.Add(stringToDouble(data[i][1]));
                }

                if (selectedTask == TaskType.Test)
                {
                    DrawTrueSolution(testSeries, stringToDouble(startCondition[0]), stringToDouble(startCondition[1]), X);
                }
            }
            else
            {
                for (int i = 0; i < data.Count; i++)
                {
                    mainSeries.Points.AddXY(stringToDouble(data[i][2]), stringToDouble(data[i][3]));
                }

                for (int i = 0; i < data.Count; i++)
                {
                    derSeries.Points.AddXY(stringToDouble(data[i][2]), stringToDouble(data[i][0]));
                }

                for (int i = 0; i < data.Count; i++)
                {
                    phaseSeries.Points.AddXY(stringToDouble(data[i][3]), stringToDouble(data[i][0]));
                }
            }
        }

        private void ShowDataForStartCondition(List<string> startCondition)
        {
            table.Rows.Clear();
            table2.Rows.Clear();

            addDataToPrimaryTable(startCondition);

            if (selectedTask == TaskType.Main2)
                addDataToDerivativeTable(startCondition);
        }

        private void addDataToPrimaryTable(List<string> startCondition)
        {
            string tableName = getTableString();

            List<List<string>> dataForStartCondition = db.GetDataForStartCondition(tableName, startCondition);

            int columnNamesSize = tableName == "test" ? columnNames.Count : columnNames.Count - 2;

            for (int i = 0; i < dataForStartCondition.Count; i++)
            {
                DataRow row = table.NewRow();

                for (int j = 0; j < columnNamesSize; j++)
                {
                    int j_index = tableName == "main2" ? j + 1 : j;
                    row[columnNames[j]] = dataForStartCondition[i][j_index];
                }

                table.Rows.Add(row);
            }
        }

        private void addDataToDerivativeTable(List<string> startCondition)
        {
            string tableName = "main2der";
            List<List<string>> dataForStartConditionDer = db.GetDataForStartCondition(tableName, startCondition);

            for (int i = 0; i < dataForStartConditionDer.Count; i++)
            {
                DataRow row = table2.NewRow();

                for (int j = 0; j < columnNamesForDerivative.Count; j++)
                {
                    row[columnNamesForDerivative[j]] = dataForStartConditionDer[i][j + 1];
                }

                table2.Rows.Add(row);
            }
        }
        
        private List<string> stringConditionToList(string startConditionString) 
        {
            List<string> startCondition = new List<string>();
            int commaIndex = startConditionString.IndexOf(',');
            startCondition.Add(startConditionString.Substring(0, commaIndex));
            string cropped = startConditionString.Substring(commaIndex + 2);
            int secondCommaIndex = cropped.IndexOf(",");

            if (secondCommaIndex != -1) 
            {
                startCondition.Add(startConditionString.Substring(commaIndex + 2, secondCommaIndex));
                startCondition.Add(startConditionString.Substring(commaIndex + secondCommaIndex + 4));
            }
            else
            {
                startCondition.Add(startConditionString.Substring(commaIndex + 2));
                startCondition.Add("0");
            }

            return startCondition;
        }

        private string getInfo(DataTable curTable, bool cntrl, bool onlyOlp = false)
        {
            if (curTable.Rows.Count == 0)
            {
                return "";
            }

            int n = curTable.Rows.Count, C1sum = 0, C2sum = 0, maxHiXi = 0, minHiXi = 0, maxuiviXi = 0;

            string s = table.Rows[0][6].ToString();

            double maxHi = Convert.ToDouble(curTable.Rows[0][6].ToString(), CultureInfo.InvariantCulture);
            double minHi = Convert.ToDouble(curTable.Rows[0][6].ToString(), CultureInfo.InvariantCulture);
            double maxOlp = 0;
            double maxuivi = 0;
            double xn = Convert.ToDouble(curTable.Rows[curTable.Rows.Count - 1][1].ToString(), CultureInfo.InvariantCulture);
            double bxn = Convert.ToDouble(pointsToCommas(textBox7.Text)) - xn;

            string resultInfo = "";

            for (int i = 0; i < curTable.Rows.Count; i++)
            {


                C1sum += Convert.ToInt32(curTable.Rows[i][7]);
                C2sum += Convert.ToInt32(curTable.Rows[i][8]);

                double hitmp = Convert.ToDouble(curTable.Rows[i][6].ToString(), CultureInfo.InvariantCulture);

                if (hitmp > maxHi)
                {
                    maxHi = hitmp;
                    maxHiXi = i + 1;
                }
                if (hitmp < minHi)
                {
                    minHi = hitmp;
                    minHiXi = i + 1;
                }
                
                double olptmp = Convert.ToDouble(curTable.Rows[i][5].ToString(), CultureInfo.InvariantCulture);

                maxOlp = Math.Abs(olptmp) > maxOlp ? Math.Abs(olptmp) : maxOlp;

                if (selectedTask == TaskType.Test)
                {
                    double uivitmp = Convert.ToDouble(curTable.Rows[i][10].ToString(), CultureInfo.InvariantCulture);

                    if (Math.Abs(uivitmp) > maxuivi)
                    {
                        maxuivi = Math.Abs(uivitmp);
                        maxuiviXi = i + 1;
                    }
                }
            }

            if(!onlyOlp) resultInfo += "n = " + n.ToString() + "\n";
            if (!onlyOlp) resultInfo += "b - xn = " + bxn.ToString() + "\n";
            if(cntrl) resultInfo += "Макс. ОЛП = " + maxOlp.ToString() + "\n";
            if (!onlyOlp) if (cntrl) resultInfo += "Удвоений: " + C2sum.ToString() + "\n";
            if (!onlyOlp) if (cntrl) resultInfo += "Делений: " + C1sum.ToString() + "\n";
            if (!onlyOlp) if (cntrl) resultInfo += "Минимальный шаг: " + minHi.ToString() + " при x = " + minHiXi.ToString() + "\n";
            if (!onlyOlp) if (cntrl) resultInfo += "Максимальный шаг: " + maxHi.ToString() + " при x = " + maxHiXi.ToString() + "\n";
            
            if (selectedTask == TaskType.Test)
            {
                resultInfo += "Максимальный ui - vi: " + maxuivi.ToString() + " при x = " + maxuiviXi.ToString() + "\n";
            }

            return resultInfo;
        }

        private void showParameters(List<string> startCondition) 
        {
            string tableName = getTableString();

            List<string> parameters = db.GetParameters(tableName, startCondition);

            textBox1.Text = parameters[1];
            textBox2.Text = parameters[2];
            textBox11.Text = parameters[3];
            textBox7.Text = parameters[4];
            textBox3.Text = parameters[5];
            textBox4.Text = parameters[6];
            textBox5.Text = parameters[7];
            textBox6.Text = parameters[8];
            textBox8.Text = parameters[9];
            textBox9.Text = parameters[10];
            textBox10.Text = parameters[11];

            int cntrl = Convert.ToInt32(parameters[12]);

            if (cntrl == 1)
                checkBox1.Checked = true;

            else
                checkBox1.Checked = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.chart1.Series.Clear();
            this.chart2.Series.Clear();
            this.chart3.Series.Clear();
            table.Clear();
            table2.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            executeMethod();
        }

        private void dsaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new Help()).Show();
        }
    }
}
