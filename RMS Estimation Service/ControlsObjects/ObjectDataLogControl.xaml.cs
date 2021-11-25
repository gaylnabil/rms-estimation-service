namespace RMS_Estimation_Service.ControlsObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using static RMS_Estimation_Service.RmsService;

    /// <summary>
    /// Interaction logic for ObjectDataLogControl.xaml.
    /// </summary>
    public partial class ObjectDataLogControl : UserControl
    {
        /// <summary>
        /// Gets or sets the Result.
        /// </summary>
        public TimeSpan Result { get; set; }
        public int NumberSlides { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDataLogControl"/> class.
        /// </summary>
        public ObjectDataLogControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDataLogControl"/> class.
        /// </summary>
        /// <param name="category">The category<see cref="string"/>.</param>
        /// <param name="serviceType">The serviceType<see cref="string"/>.</param>
        /// <param name="estimation">The estimation<see cref="TimeSpan"/>.</param>
        /// <param name="numberSlides">The numberSlides<see cref="int"/>.</param>
        public ObjectDataLogControl(string category, string serviceType, TimeSpan estimation, int numberSlides, object tag) : this()
        {
            this.Tag = tag;
            this.NumberSlides = numberSlides;
            TxtBlockCategoryService.Text = category;
            TxtBlockTypeService.Text = serviceType;
            TxtEstimation.Text = estimation.ToString("hh':'mm");
            TxtNbrSlide.Text = numberSlides.ToString();

            var value = TimeSpanToDouble(estimation, 'm') * this.NumberSlides;

            Result = DoubleToTimeSpan(value, 'm');

            TxtResult.Text = $"{Result.TotalHours:00}:{Result.Minutes:00}";
            RmsMain.Duration += this.Result;

            var totalEstimation = int.Parse(RmsMain.TxtTotalEstimation.Text) + this.NumberSlides;
            RmsMain.TxtTotalEstimation.Text = totalEstimation.ToString();

            RmsMain.TxtDuration.Text = RmsMain.GetDuration();
        }

        /// <summary>
        /// The BtnCloseLog_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void BtnCloseLog_Click(object sender, RoutedEventArgs e)
        {
            DeleteLogValues();
            RmsMain.StackPanelContentLogs.Children.Remove(this);
        }

        public void DeleteLogValues()
        {
            RmsMain.Duration -= this.Result;

            var totalEstimation = int.Parse(RmsMain.TxtTotalEstimation.Text) - this.NumberSlides;
            RmsMain.TxtTotalEstimation.Text = totalEstimation.ToString();

            RmsMain.TxtDuration.Text = RmsMain.GetDuration();
        }
    }
}
