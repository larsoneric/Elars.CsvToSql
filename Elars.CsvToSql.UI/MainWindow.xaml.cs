using Elars.CsvToSql.Core;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Elars.CsvToSql.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeConverterProperties();
        }

        private void InitializeConverterProperties()
        {
            var converter = new Converter();

            txtTableName.Text = converter.TableName;
            txtBatchSize.Text = converter.BatchSize.ToString();
            chkBatch.IsChecked = converter.BatchSize > 1;
            chkAllowSpaces.IsChecked = converter.AllowSpaces;
            chkCreateTable.IsChecked = converter.CreateTable;
            chkReseed.IsChecked = converter.Reseed;
            chkNoCount.IsChecked = converter.NoCount;
            radClustered.IsChecked = converter.ClusteredIndex;
            radNonclustered.IsChecked = !converter.ClusteredIndex;
            chkIdentityInsert.IsChecked = converter.IdentityInsert;
            chkTruncate.IsChecked = converter.TruncateTable;

            chkBatch_Click(null, null);
            chkIdentityInsert_Click(null, null);
            chkCreateIndex_Click(null, null);
            txtSql_TextChanged(null, null);
        }

        private async void btnFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (!Clipboard.ContainsText())
            {
                txtStatus.Text = "Clipboard does not contain text";
                return;
            }

            await ProcessText(Clipboard.GetText());
        }

        private async Task ProcessText(string text)
        {
            pbStatus.Visibility = Visibility.Visible;

            try
            {
                var converter = NewConverter();
                txtSql.Text = await converter.ProcessString(text);
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

        private Converter NewConverter()
        {
            return new Converter
            {
                TableName = txtTableName.Text,
                CreateTable = chkCreateTable.IsChecked.Value,
                Reseed = chkReseed.IsChecked.Value,
                NoCount = chkNoCount.IsChecked.Value,
                AllowSpaces = chkAllowSpaces.IsChecked.Value,
                ClusteredIndex = radClustered.IsChecked.Value,
                IdentityInsert = chkIdentityInsert.IsChecked.Value,
                BatchSize = chkBatch.IsChecked.Value ? int.Parse(txtBatchSize.Text) : 1,
                IndexColumn = chkCreateIndex.IsChecked.Value ? int.Parse(txtIndexColumn.Text) : (int?)null,
                TruncateTable = chkTruncate.IsChecked.Value
            };
        }

        private void btnToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtSql.Text);
            txtStatus.Text = "Done";
        }

        private void chkBatch_Click(object sender, RoutedEventArgs e)
        {
            txtBatchSize.IsEnabled = chkBatch.IsChecked.Value;
        }

        private void chkIdentityInsert_Click(object sender, RoutedEventArgs e)
        {
            chkReseed.IsEnabled = chkIdentityInsert.IsChecked.Value;
        }

        private void chkCreateIndex_Click(object sender, RoutedEventArgs e)
        {
            txtIndexColumn.IsEnabled = chkCreateIndex.IsChecked.Value;
            radClustered.IsEnabled = chkCreateIndex.IsChecked.Value;
            radNonclustered.IsEnabled = chkCreateIndex.IsChecked.Value;
        }

        private void txtSql_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            btnSaveFile.IsEnabled = !string.IsNullOrEmpty(txtSql.Text);
            btnToClipboard.IsEnabled = !string.IsNullOrEmpty(txtSql.Text);
        }

        private void chkTruncate_Checked(object sender, RoutedEventArgs e)
        {
            var response = MessageBox.Show("Are you sure you want to truncate the table?", "Truncate", MessageBoxButton.YesNo);
            if (response == MessageBoxResult.No)
                chkTruncate.IsChecked = false;
        }

        private async void btnOpenFile_Click(object sender, RoutedEventArgs e)
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

        private void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "SQL file (*.sql)|*.sql"
            };

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, txtSql.Text);

            txtStatus.Text = "Done";
        }
    }
}
