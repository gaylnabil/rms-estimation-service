using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.VisualBasic.ApplicationServices;
using RMS_Estimation_Service.AssemblyInformation;
using SimajeEmeakomLib.Messages;
using static RMS_Estimation_Service.RmsService;


namespace RMS_Estimation_Service.StartUp
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        private DispatcherTimer _timeLoad, _timeTableLoad, _timeAwait;
        private MainRmsEstimationService main;
        private int _countLines, _countTables, _countLinesTable, _countTotal=0;
        private int _valuelines, _lineStep = 5, _tableLines, _tableIndex = 0;
        private double _percent = 0;


        public SplashScreen()
        {
            Opacity = 0;
            InitializeComponent();

            try
            {

                _timeLoad = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(1),
                };

                _timeLoad.Tick += TimeLoad_Tick;

                /*****************************************/

                _timeTableLoad = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(1),
                };

                _timeTableLoad.Tick += timeTableLoad_Tick;

                /*****************************************/

                _timeAwait = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(20),
                };

                _timeAwait.Tick += timeAwait_Tick;

                ///////////////////////////////////////////

                _countTables = TableNames.Count;

                _countLines = GetAllLinesCount();

                ProgressBarSplash.Maximum = _countLines;
                ProgressBarSplash.Value = _valuelines;


                _tableLines = 0;
                _countLinesTable = GetTableLinesCount(TableNames[_tableIndex]);
            }
            catch (Exception ex)
            {
                SimajeMessageBox.Show(ex.Message, "Error",MessageBoxButton.OK, SimajeMessageBox.MessageBoxImage.Error);
            }



        }



        private void SimajeSplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            var info = new SimajeAssemblyInfo();
            txtCompany.Text = info.Company;
            txtCopyright.Text = info.Copyright;
            txtVersion.Text = $"v{info.AssemblyVersion}";

            DoAnimation(this, 0, 1, TimeSpan.FromMilliseconds(300),Window.OpacityProperty, StoryboardOnCompleted);
        }



        private void StoryboardOnCompleted(object sender, EventArgs e)
        {
            _timeTableLoad.Start();
            _timeLoad.Start();
        }

        private void timeTableLoad_Tick(object sender, EventArgs e)
        {
            if (_tableLines == _countLinesTable)
            {
                _countTotal += _countLinesTable;
                _tableIndex++;

                if (_tableIndex == _countTables)
                {
                    _timeTableLoad.Stop();
                }
                else
                {
                    _tableLines = 0;
                    _countLinesTable = GetTableLinesCount(TableNames[_tableIndex]);

                    if (_countLinesTable == 0)
                    {
                        InitializeTable(TableNames[_tableIndex]);
                    }
                }
            }
            else
            {
                FillTableByNumberLine(TableNames[_tableIndex], _lineStep, _tableLines);
                _tableLines = DsService.Tables[TableNames[_tableIndex]].Rows.Count;
                _valuelines = _countTotal + _tableLines;

                //TxtSubTitle.Text = $"TableNames : {TableNames[_tableIndex]}; Valuelines : {_valuelines}" +
                //                   $"; tableLines : {_tableLines}; Max : {ProgressBarSplash.Maximum}; Value : {ProgressBarSplash.Value}";
            }

        }

        private void TimeLoad_Tick(object sender, EventArgs e)
        {
            if (_valuelines >= _countLines)
            {
                _valuelines = _countLines;
                _timeLoad.Stop();

                _timeAwait.Start();
            }

            ProgressBarSplash.Value = _valuelines;
            _percent = ProgressBarSplash.Value * 100 / ProgressBarSplash.Maximum;
            txtPercent.Text = $"Loading data schema...{_percent:0}%";
        }

        private int _tick = 0;
        private void timeAwait_Tick(object sender, EventArgs e)
        {
            switch (_tick)
            {
                case 0:
                    ViewCategories = DsService.Tables[TableNames[0]].AsDataView();
                    break;

                case 1:
                    ViewServices = DsService.Tables[TableNames[1]].AsDataView();
                    break;

                case 2:
                    ViewRelationship = DsService.Tables[TableNames[2]].AsDataView();
                    break;
                default:
                    break;
            }

           if (_tick >= 5)
           {
               _timeAwait.Stop();
               ProgressBarSplash.Value = _valuelines;
               this.Hide();
               main = new MainRmsEstimationService();
               main.Show();
           }
            _tick++;
        }

        private void GridContainer_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
