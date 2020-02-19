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
            DataContext = new RollDice(Environment.CurrentDirectory.Replace(@"bin\Debug", @"Images\"));
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
            }

            startBtn.IsEnabled = true;
        }

        private void ReportProgress(object sender, ProgressReportModel e)
        {
            if (e.PercentageComplete <= 100)
            {
                progressBar.Value = e.PercentageComplete;
            }
            
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
            }
        }
    }
}
