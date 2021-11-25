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
using Microsoft.VisualBasic;
using static RMS_Estimation_Service.RmsService;

namespace RMS_Estimation_Service.ControlsObjects
{
    /// <summary>
    /// Interaction logic for ObjectServiceControl.xaml
    /// </summary>
    public partial class ObjectServiceControl : UserControl
    {
        public string ServiceTitleHeader { get; set; }
        public ObjectServiceControl()
        {
            InitializeComponent();
        }

        private void BtnValidation_Click(object sender, RoutedEventArgs e)
        {
            //if (IsServiceTypeExist(this.Tag))
            //{
            //    MessageBox.Show($"{TxtBlockTitleServiceType.Text} already exist !!!");
            //    return;
            //}

            var dataLog = GetServiceType(this.Tag);

            if (dataLog != null)
            {
                dataLog.DeleteLogValues();
                RmsMain.StackPanelContentLogs.Children.Remove(dataLog);
            }

            var value = TxtNumberSlide.Value ?? 1.0;

            dataLog = new ObjectDataLogControl(this.ServiceTitleHeader,
                TxtBlockTitleServiceType.Text,
                TimeSpan.Parse(TxtEstimation.Text),
                (int)value,
                this.Tag
            );
            dataLog.Opacity = 0;

            RmsMain.StackPanelContentLogs.Children.Add(dataLog);
            DoAnimation(dataLog, 0, 1.0,  TimeSpan.FromSeconds(1), UserControl.OpacityProperty);
            RmsMain.ScrollViewerLog.ScrollToEnd();
        }

        private bool IsServiceTypeExist(object tag)
        {
            return RmsMain.StackPanelContentLogs.Children.OfType<ObjectDataLogControl>().Any(obj => obj.Tag == tag);
        }

        private ObjectDataLogControl GetServiceType(object tag)
        {
            return RmsMain.StackPanelContentLogs.Children.OfType<ObjectDataLogControl>().SingleOrDefault(obj => obj.Tag == tag);
        }

        //private void TxtNumberSlide_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (!Information.IsNumeric(TxtNumberSlide.Text))
        //    {
        //        TxtNumberSlide.Text = "1";
        //    }
        //}
    }
}
