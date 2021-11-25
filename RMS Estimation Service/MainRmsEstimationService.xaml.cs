using System.Data.SqlClient;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using RMS_Estimation_Service.Models;
using SimajeEmeakomLib.Messages;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using UserControl = System.Windows.Controls.UserControl;

namespace RMS_Estimation_Service
{
    using ControlsObjects;
    using ServiceSettings;
    using SimajeEmeakomLib.Controls;
    using SimajeEmeakomLib.Events;
    using System;
    using System.Data;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;
    using TableDependency.SqlClient.Base.EventArgs;
    using static RmsService;

    /// <summary>
    /// Defines the <see cref="MainRmsEstimationService" />.
    /// </summary>
    public partial class MainRmsEstimationService : Window
    {
        /// <summary>
        /// Defines the DURATION_DEFAUT_VALLUE.
        /// </summary>
        public readonly TimeSpan DURATION_DEFAUT_VALLUE = TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the Duration.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the CurrentBtn.
        /// </summary>
        public SimajeButtonFeature CurrentBtn { get; set; }

        /// <summary>
        /// Defines the _timeCategory, _timeService.
        /// </summary>
        private DispatcherTimer _timeCategory, _timeService;// _timeAsyncData;

        /// <summary>
        /// Defines the categories, services.
        /// </summary>
        private DataTable categories, services;

        /// <summary>
        /// Defines the index, indexService.
        /// </summary>
        private int index, indexService;

        private bool _isFistTime = false;

        private SqlTableDependency<ServiceCategory> SyncCategory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainRmsEstimationService"/> class.
        /// </summary>
        public MainRmsEstimationService()
        {
            try
            {
                Opacity = 0;
                InitializeComponent();
                categories = GetServiceCategories();
                index = 0;

                _timeCategory = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                _timeCategory.Tick += TimeCategory_OnTick;

                _timeService = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
                _timeService.Tick += TimeService_OnTick;

                RmsMain = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// The RmsEstimationService1_Loaded.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void RmsEstimationService1_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Duration = DURATION_DEFAUT_VALLUE;
                wSettings = new WindowSettings();
                SpCategories.Children.Clear();
                StackPanelContentLogs.Children.Clear();
                ContainerBottom.Children.Clear();

                DoAnimation(this, 0, 1.0, TimeSpan.FromMilliseconds(1000),Window.OpacityProperty ,Storyboard_OnCompleted);


            }
            catch (Exception ex)
            {
                SimajeMessageBox.Show(ex.Message, @"Error :  RmsEstimationService1_Loaded", MessageBoxButton.OK, SimajeMessageBox.MessageBoxImage.Error);
            }
        }


        private void RmsEstimationService1_Closed(object sender, EventArgs e)
        {
            StopDbSynchronization();
        }

        /// <summary>
        /// The TimeCategory_OnTick.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="EventArgs"/>.</param>
        private void TimeCategory_OnTick(object sender, EventArgs e)
        {
            if (index >= categories.Rows.Count)
            {
                _timeCategory.Stop();
                var first = SpCategories.Children[0] as SimajeButtonFeature;
                if (first != null) first.IsChecked = true;
                index = 0;
                return;
            }

            var row = categories.Rows[index];
            var btn = new SimajeButtonFeature
            {
                Opacity = 0,
                Name = $"Feature_{row["ID"]}",
                Tag = int.Parse(row["ID"].ToString()),
                IsToggleButton = true,
                Margin = new Thickness(8, 0, 8, 0),
                TitleHeader = row["Name"].ToString(),
                Description = row["Description"].ToString(),
                IconName = !string.IsNullOrEmpty(row["IconName"].ToString()) ? row["IconName"].ToString() : string.Empty,
                IconData = !string.IsNullOrEmpty(row["IconData"].ToString()) ? Geometry.Parse(row["IconData"].ToString()) : null,

            };


            btn.IsCheckedValueChanged += SimajeButtonFeature_OnIsCheckedValueChanged;
            SpCategories.Children.Add(btn);
            DoAnimation(btn, 0, 1.0, TimeSpan.FromMilliseconds(1500), SimajeButtonFeature.OpacityProperty);


            index++;
        }

        /// <summary>
        /// The TimeService_OnTick.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="EventArgs"/>.</param>
        private void TimeService_OnTick(object sender, EventArgs e)
        {

            if (indexService >= services.Rows.Count)
            {
                _timeService.Stop();
                return;
            }

            var serviceRow = services.Rows[indexService];
            var obj = new ObjectServiceControl
            {
                Opacity = 0,
                Tag = $"{CurrentBtn.Tag}.{serviceRow["ID"]}",
                ServiceTitleHeader = CurrentBtn.TitleHeader,
                TxtBlockTitleServiceType = { Text = serviceRow["Service_Type"].ToString() },
                TxtBlockDescription = { Text = serviceRow["Description"].ToString() },
                TxtEstimation = { Text = serviceRow["Estimation"].ToString() }
            };

            ContainerBottom.Children.Add(obj);
            DoAnimation(obj, 0, 1.0, TimeSpan.FromMilliseconds(1000), UserControl.OpacityProperty);
            indexService++;
        }

        /// <summary>
        /// The SimajeButtonFeature_OnIsCheckedValueChanged.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="CheckedChangedEventArgs"/>.</param>
        private void SimajeButtonFeature_OnIsCheckedValueChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.IsChecked) return;
            this.CurrentBtn = sender as SimajeButtonFeature;
            if (this.CurrentBtn == null) return;

            foreach (var child in SpCategories.Children.OfType<SimajeButtonFeature>().ToList())
            {
                if (!child.Name.Equals(this.CurrentBtn.Name))
                    child.IsChecked = false;
            }

            RefreshDataServices();
        }

        /// <summary>
        /// The RefreshDataServices.
        /// </summary>
        public void RefreshDataCategories()
        {
            _timeCategory.Stop();
            SpCategories.Children.Clear();
            ContainerBottom.Children.Clear();
            _timeCategory.Start();
        }

        public void RefreshDataServices()
        {
            _timeService.Stop();
            indexService = 0;
            ContainerBottom.Children.Clear();

            services = GetServicesTypesByIdServiceCategory((int)CurrentBtn.Tag);

            _timeService.Start();
        }

        /// <summary>
        /// The GridTitleBar_MouseDown.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Input.MouseButtonEventArgs"/>.</param>
        private void GridTitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// The BtnClose_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }


        /// <summary>
        /// The BtnClearAll_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            Duration = DURATION_DEFAUT_VALLUE;
            StackPanelContentLogs.Children.Clear();
            TxtTotalEstimation.Text = "0";
            TxtDuration.Text = GetDuration();
        }

        /// <summary>
        /// The BtnSettings_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            wSettings.FormClosed += WSettings_FormClosed;
            wSettings.Show();
        }

        /// <summary>
        /// The WSettings_FormClosed.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Forms.FormClosedEventArgs"/>.</param>
        private void WSettings_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Activate();
        }

        /// <summary>
        /// The StoryboardOnCompleted.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="EventArgs"/>.</param>
        private void Storyboard_OnCompleted(object sender, EventArgs e)
        {
            _timeCategory.Start();

            if (!_isFistTime)
            {
                _isFistTime = true;
                AnimationQuote();
            }

            // BeginDbSynchronization();

            //_timeAsyncData = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            //_timeAsyncData.Tick += TimeAsyncData_OnTickAsync;

            //_timeAsyncData.Start();
        }

        //private int test = 0;

        //private void TimeAsyncData_OnTickAsync(object sender, EventArgs e)
        //{
        //    //AsynchronousDataUpdate();

        //    TextBlockTest.Text = $"Good Test :{test++}";
        //}

        /// <summary>
        /// The Storyboard Quotes On Completed.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="EventArgs"/>.</param>
        private void StoryboardQuotes_OnCompleted(object sender, EventArgs e)
        {
            AnimationQuote();

            //StopDbSynchronization();
            //BeginDbSynchronization();
        }

        /// <summary>
        /// The GetDuration.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetDuration()
        {
            return $"{Duration.TotalHours:00}:{Duration.Minutes:00}";
        }

        private void AnimationQuote()
        {
            TxtBlockQuotes.Text = GetQuote();

            var from = CanvasQuoteContainer.ActualWidth + 10;

            var to = -(TxtBlockQuotes.ActualWidth  * 2);

            DoAnimation(TxtBlockQuotes, from, to, TimeSpan.FromMinutes(2), Canvas.LeftProperty, StoryboardQuotes_OnCompleted);
        }

    }
}
