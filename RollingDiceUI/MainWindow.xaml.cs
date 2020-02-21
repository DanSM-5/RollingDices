using RollingDiceUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace RollingDiceUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cts;
        public MainWindow()
        {
            InitializeComponent();

            // Number of dices to work with
            // The application cannot work with more than 8 dices at the same time
            var dices = (Enumerable.Range(1, 8));

            // Sequences available to roll
            // You can add, change, or delete the numbers
            var sequences = new int[] { 1000000, 2000000, 4000000, 8000000, 16000000, 32000000 };

            // Required files
            // List of the names of the files that are required to execute the application
            var files = new string[] { "0.png", "1.png", "2.png", "3.png", "4.png", "5.png", "6.png" };

            // Name of the directory to search for
            var directoryName = "Images";

            // Get the path of the required files
            string path = (new DirectoryFinder(directoryName, files)).GetDirectoryPath();

            if (!String.IsNullOrEmpty(path))
            {
                DataContext = new RollDice(path, dices, sequences);
            }
            else
            {
                MessageBox.Show($"There are missing files!{Environment.NewLine}Application can't start", "Missing Files", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private async void startBtn_Click(object sender, RoutedEventArgs e)
        {
            if (numOfDiceCbx.SelectedIndex == -1 || sequenceCbx.SelectedIndex == -1)
            {
                return;
            }

            cts = new CancellationTokenSource();
            startBtn.IsEnabled = false;
            timeTbl.Text = "0";

            Progress <ProgressReportModel> progress = new Progress<ProgressReportModel>();
            progress.ProgressChanged += ReportProgress;

            try
            {
                timeTbl.Text = await (DataContext as RollDice).ExecuteRoll(progress, cts.Token);

                if (cts.Token.IsCancellationRequested)
                {
                    cts.Token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException ex)
            {
                timeTbl.Text = $"{ex.Message}";
                startBtn.IsEnabled = true;
                cts.Dispose();
                cts = null;
                cancelBtn.IsEnabled = true;
            }

            startBtn.IsEnabled = true;
        }

        private void ReportProgress(object sender, ProgressReportModel e)
        {
            progressBar.Value = e.PercentageComplete;
           
            if (e.PercentageComplete % 5 == 0)
            {
                percentageTbl.Text = $"{e.PercentageComplete}%";
            }
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
                cancelBtn.IsEnabled = false;
            }
        }
    }
}
