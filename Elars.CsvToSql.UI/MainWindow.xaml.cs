using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Elars.CsvToSql.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Options _options = new Options();

        public MainWindow()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = _options;
        }

        private async Task ProcessText(string text)
        {
            pbStatus.Visibility = Visibility.Visible;

            try
            {
                var converter = _options.ToConverter();
                string sql = "";

                // try to get the progress bar to appear
                await Task.Run(async () =>
                {
                    sql = await converter.ProcessString(text);
                });

                txtSql.Text = sql;
                txtStatus.Text = converter.NumberOfRecords.ToString() + " record" + (converter.NumberOfRecords != 1 ? "s" : "");
            }
            catch (Exception ex)
            {
                txtStatus.Text = ex.Message;
            }
            finally
            {
                pbStatus.Visibility = Visibility.Collapsed;
            }
        }

        private void HelpCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = "https://github.com/larsoneric/Elars.CsvToSql";
            p.Start();
        }

        private void HelpCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private async void OpenCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (!openFileDialog.ShowDialog() == true)
                return;

            var text = File.ReadAllText(openFileDialog.FileName);
            await ProcessText(text);
        }

        private void OpenCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "SQL file (*.sql)|*.sql"
            };

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, txtSql.Text);

            txtStatus.Text = "Done";
        }

        private void SaveCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(txtSql.Text);
        }

        private void CopyCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Clipboard.SetText(txtSql.Text);
            txtStatus.Text = "Done";
        }

        private void CopyCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(txtSql.Text);
        }

        private async void PasteCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            await ProcessText(Clipboard.GetText());
        }

        private void PasteCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText();
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            var propertyItem = (PropertyItem)e.OriginalSource;
            var displayName = propertyItem.DisplayName;

            if (displayName == "Truncate Table" && (bool)e.NewValue == true)
            {
                var response = System.Windows.MessageBox.Show("Are you sure you want to truncate the table?", "Truncate", MessageBoxButton.YesNo);
                if (response == MessageBoxResult.No)
                {
                    propertyItem.Value = false;
                }
            }

            var refreshColumns = new[] { "Use Batches", "Create Table", "Create Index", "Identity Insert", "Reseed" };
            if (refreshColumns.Contains(displayName))
            {
                propertyGrid.SelectedObject = null;
                propertyGrid.SelectedObject = _options;
            }
        }

        private void propertyGrid_PreparePropertyItem(object sender, PropertyItemEventArgs e)
        {
            var propertyItem = (PropertyItem)e.Item;
            var displayName = propertyItem.DisplayName;

            if (displayName == "Batch Size")
            {
                propertyItem.Visibility = _options.ShowBatchSize() ? Visibility.Visible : Visibility.Collapsed;
            }

            if (displayName == "Identity Insert")
            {
                propertyItem.Visibility = _options.ShowIdentityInsert() ? Visibility.Visible : Visibility.Collapsed;
            }

            if (displayName == "Reseed")
            {
                propertyItem.Visibility = _options.ShowReseed() ? Visibility.Visible : Visibility.Collapsed;
            }

            if (displayName == "Index Type")
            {
                propertyItem.Visibility = _options.ShowIndexType() ? Visibility.Visible : Visibility.Collapsed;
            }

            if (displayName == "Truncate Table")
            {
                propertyItem.Visibility = _options.ShowTruncateTable() ? Visibility.Visible : Visibility.Collapsed;
            }

            if (displayName == "Index Column")
            {
                propertyItem.Visibility = _options.ShowIndexColumn() ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
