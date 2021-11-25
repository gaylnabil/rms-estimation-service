using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using FontAwesome.Sharp;
using RMS_Estimation_Service.Models;
using SimajeEmeakomLib.Messages;
using static RMS_Estimation_Service.RmsService;
using MessageBox = System.Windows.Forms.MessageBox;

namespace RMS_Estimation_Service.ServiceSettings
{
    public partial class CategorieSettings : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        public CategorieSettings()
        {
            //this.SuspendLayout();
            InitializeComponent();
            //this.ResumeLayout();
            LblTitle.Text = this.Text;

            dataGridView1.MouseWheel += dataGridView1_MouseWheel;
            txtSearch.GotFocus += txtSearch_GotFocus;
            txtSearch.LostFocus += txtSearch_LostFocus;
        }

        private void txtSearch_GotFocus(object sender, EventArgs e)
        {
            txtSearch.Focus();
            if (txtSearch.Text== "Search here")
            {
                txtSearch.Clear();
            }
        }

        private void txtSearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text.Trim()))
            {
                txtSearch.Text = "Search here";
            }
        }


        int _displayRow =0;
        private void CategoriesSettings_Load(object sender, EventArgs e)
        {
            txtServiceCategory.AutoSize = false;
            ShowData();

            radVScrollBar1.Maximum = dataGridView1.RowCount;
            _displayRow = dataGridView1.DisplayedRowCount(true);
            radVScrollBar1.Value = 0;
            AutoHideScrollBar();
        }


        private void CategoriesSettings_FormClosed(object sender, FormClosedEventArgs e)
        {
            //wSettings.Dispose();
            //wSettings = new WindowSettings();
        }

        private void AutoHideScrollBar()
        {

            try
            {
                radVScrollBar1.LargeChange = dataGridView1.DisplayedRowCount(true);
                radVScrollBar1.Maximum = dataGridView1.RowCount;// - radVScrollBar1.LargeChange + 1;
                radVScrollBar1.SmallChange = 1;

                radVScrollBar1.Visible = _displayRow - 1 < dataGridView1.RowCount;
            }
            catch (Exception ex)
            {
                _ = ex.StackTrace;
            }

        }

        private void dataGridView1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (radVScrollBar1.Visible == false) return;
            if (this.dataGridView1.FirstDisplayedScrollingRowIndex == -1) return;

            var currentIndex = this.dataGridView1.FirstDisplayedScrollingRowIndex;
            var scrollLines = SystemInformation.MouseWheelScrollLines;

            if (e.Delta > 0)
            {
                this.dataGridView1.FirstDisplayedScrollingRowIndex = Math.Max(0, currentIndex - scrollLines);
            }
            else if (e.Delta < 0)
            {
                this.dataGridView1.FirstDisplayedScrollingRowIndex = currentIndex + scrollLines;
            }
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void iconButtonBrowse_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtServiceCategory.Text))
            {
                SimajeMessageBox.Show(@"Please Fill the Zone Text !!", @"Warning",
                    MessageBoxButton.OK,
                    SimajeMessageBox.MessageBoxImage.Warning);
                return;
            }

            StopDbSynchronization();
            try
            {

                var category = new ServiceCategory()
                {
                    ID = string.IsNullOrEmpty(txtIdCategory.Text.Trim()) ? -1 : int.Parse(txtIdCategory.Text.Trim()),
                    Name = txtServiceCategory.Text.Trim(),
                    Description = richDescription.Text.Trim(),
                    IconData = richIconData.Text.Trim(),
                    IconName = txtIconName.Text.Trim(),
                };

                var message = $@"The New Category '' {category.Name} '' has been added successfully !!";
                if (category.ID == -1)
                {
                    AddServiceCategoryAsync(category);

                }
                else
                {
                    UpdateServiceCategoryAsync(category);

                    message = $@"The Category Type '' {category.Name} '' updated successfully !!";
                }

                Reset();

                SimajeMessageBox.Show(message, @"Information", MessageBoxButton.OK,
                    SimajeMessageBox.MessageBoxImage.Information);
                RmsMain.RefreshDataCategories();

            }
            catch (Exception exception)
            {
                SimajeMessageBox.Show(exception.Message, @"Error", MessageBoxButton.OK,
                    SimajeMessageBox.MessageBoxImage.Error);
            }

            // BeginDbSynchronization();
        }

        private void iconButtonDelete_Click(object sender, EventArgs e)
        {
            var message = @"Do you want to delete the Service Category ?";
            if (SimajeMessageBox.Show(message, @"Question", MessageBoxButton.YesNo,
                    SimajeMessageBox.MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            StopDbSynchronization();

            try
            {
                var category = new ServiceCategory()
                {
                    ID = int.Parse(txtIdCategory.Text),
                };

                DeleteServiceCategoryAsync(category);
                message = $@"The Service Category '' {category.Name} '' deleted successfully !!";

                Reset();

                SimajeMessageBox.Show(message, @"Information", MessageBoxButton.OK, SimajeMessageBox.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                SimajeMessageBox.Show(ex.Message, @"Error", MessageBoxButton.OK, SimajeMessageBox.MessageBoxImage.Error);
            }

            RmsMain.RefreshDataCategories();
            // BeginDbSynchronization();

        }



        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar != (char)13 && !char.IsControl(e.KeyChar))
            //{
            //    e.Handled = false;
            //}
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text == @"Search here") return;
            ViewCategories.RowFilter = $"Name LIKE '%{txtSearch.Text.Trim()}%'";
                                         //$"Description LIKE '%{txtSearch.Text.Trim()}%'";
            AutoHideScrollBar();
        }

        private void iconButtonClear_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            txtServiceCategory.Text = dataGridView1.Rows[e.RowIndex].Cells["Name"].Value.ToString();
            richDescription.Text = dataGridView1.Rows[e.RowIndex].Cells["Description"].Value.ToString();
            richIconData.Text = dataGridView1.Rows[e.RowIndex].Cells["IconData"].Value.ToString();
            txtIconName.Text = dataGridView1.Rows[e.RowIndex].Cells["IconName"].Value.ToString();

            txtIdCategory.Text = dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString();

            iconButtonAddServiceType.IconChar = IconChar.Edit;
            iconButtonDelete.Enabled = true;
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            txtServiceCategory.Text = dataGridView1.Rows[e.RowIndex].Cells["Name"].Value.ToString();
            richDescription.Text = dataGridView1.Rows[e.RowIndex].Cells["Description"].Value.ToString();
            richIconData.Text = dataGridView1.Rows[e.RowIndex].Cells["IconData"].Value.ToString();
            txtIconName.Text = dataGridView1.Rows[e.RowIndex].Cells["IconName"].Value.ToString();

            txtIdCategory.Text = dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString();

            iconButtonAddServiceType.IconChar = IconChar.Edit;
            iconButtonDelete.Enabled = true;
        }

        private void Reset()
        {
            txtIdCategory.Clear();
            txtServiceCategory.Clear();
            richDescription.Clear();
            txtServiceCategory.Focus();
            richIconData.Clear();
            txtIconName.Clear();
            iconButtonAddServiceType.IconChar = IconChar.CheckCircle;
            iconButtonDelete.Enabled = false;
            var temp = txtSearch.Text;
            txtSearch.Clear();
            txtSearch.Text = temp;
        }

        private void ShowData()
        {
            ViewCategories.Sort = @"ID ASC";
            dataGridView1.DataSource = ViewCategories;

            dataGridView1.Columns[dataGridView1.Columns.Count - 1].Visible = false;
            dataGridView1.Columns[dataGridView1.Columns.Count - 2].Visible = false;
        }


        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            radVScrollBar1.Value = e.NewValue;
        }

        private void radVScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.NewValue < 0)
            {
                return;
            }
            if (e.NewValue < dataGridView1.DisplayedRowCount(true))
            {
                //radVScrollBar1.
                dataGridView1.FirstDisplayedScrollingRowIndex = e.NewValue;

            }
            //LblScrollValues.Text = $"e.NewValue : {e.NewValue}; LargeChange : {radVScrollBar1.LargeChange}; " +
            //                       $"RowCount = {dataGridView1.RowCount};DisplayedRowCount = {dataGridView1.DisplayedRowCount(true)}; " +
            //                       $"first display rows : {displayRow}";
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            AutoHideScrollBar();
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            AutoHideScrollBar();
        }

        private void iconButtonSearchClear_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            txtSearch.Focus();
        }

        private void iconButtonRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
            var oldValue = txtSearch.Text;
            txtSearch.Text = " ";
            txtSearch.Text = oldValue;
            AutoHideScrollBar();
        }

        internal void RefreshData()
        {
            //StopDbSynchronization();
            this.Invoke((MethodInvoker)delegate
            {
                this.dataGridView1.Refresh();

            });
            //BeginDbSynchronization();
        }
    }
}
