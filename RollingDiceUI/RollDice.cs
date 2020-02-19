using RollingDiceLib;
using RollingDiceUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RollingDiceUI
{
    public class RollDice : INotifyPropertyChanged
    {
        // Fields
        private int numOfDice;
        private int sequence;
        private readonly string path;
        private Process processType;
        private RollingProcess RollingProcess;
        private string time;
        private string progress;

        // Properties
        public ObservableCollection<int> AvailableDices { get; } 
        public ObservableCollection<int> ValidSequences { get; }
        public List<Image> Images { get; }
        public List<ImageSource> Sources { get; }
        public List<Visibility> Visibles { get; }
        //public ProgressReportModel ReportModel { get; set; }
        public IProgress<ProgressReportModel> Progress { get; set; }


        public string Percentage
        {
            get { return progress; }
            set 
            { 
                progress = value;
                OnPropertyChanged(nameof(Percentage));
            }
        }


        public string Time
        {
            get { return time; }
            set 
            { 
                time = value;
                OnPropertyChanged(nameof(Time));
            }
        }


        public int NumOfDice
        {
            get { return numOfDice; }
            set 
            { 
                numOfDice = value;
                OnPropertyChanged(nameof(NumOfDice));
                SetImages(numOfDice);
                SetInitialValues();
            }
        }


        public int Sequence
        {
            get { return sequence; }
            set 
            { 
                sequence = value;
                OnPropertyChanged(nameof(Sequence));
            }
        }


        public Process ProcessType
        {
            get { return processType; }
            set 
            { 
                processType = value;
                OnPropertyChanged(nameof(ProcessType));
            }
        }


        // Constructor
        public RollDice(string path)
        {
            AvailableDices = new ObservableCollection<int>(Enumerable.Range(1,8));
            ValidSequences = new ObservableCollection<int> { 1000000, 2000000, 4000000, 8000000, 16000000, 32000000 };

            Images = new List<Image>();
            Sources = new List<ImageSource> { null, null, null, null, null, null, null, null};
            Visibles = new List<Visibility>();
            this.path = path;
            ProcessType = Process.NoSelected;

            foreach (var file in Directory.GetFiles(path))
            {
                var source = new BitmapImage(new Uri(file));
                Image image = new Image();
                image.Source = source;
                Images.Add(image);
            }

            for (int i = 0; i < AvailableDices.Count; i++)
            {
                Sources[i] = Images[0].Source;
                Visibles.Add(Visibility.Hidden);
            }
        }

        // Methods

        private void SetImages(int number)
        {
            for (int i = 0; i < AvailableDices.Count; i++)
            {
                if (i < number)
                {
                    Visibles[i] = Visibility.Visible;
                    OnPropertyChanged(nameof(Visibles));
                }
                else
                {
                    Visibles[i] = Visibility.Hidden;
                    OnPropertyChanged(nameof(Visibles));
                }
            }
        }

        private void SetInitialValues()
        {
            for(int i = 0; i < Sources.Count; i++)
            {
                Sources[i] = Images[0].Source;
            }
            OnPropertyChanged(nameof(Sources));
        }

        public async Task<string> ExecuteRoll(IProgress<ProgressReportModel> progress, CancellationToken ct)
        {
            Progress = progress;
            RollingProcess = new RollingProcess(NumOfDice, Sequence);
            SetInitialValues();
            Progress.Report(new ProgressReportModel { PercentageComplete = 0, Total = 0});
            Stopwatch stopwatch = new Stopwatch();

            switch (ProcessType)
            {
                case Process.Sync:
                    stopwatch.Start();
                    RollingProcess.RollDiceSyncBlocking(Report,AssignPicture, ct);
                    stopwatch.Stop();
                    break;
                case Process.Async:
                    stopwatch.Start();
                    await RollingProcess.RollDiceAsyncNonBlocking(Report, AssignPicture, ct);
                    stopwatch.Stop();
                    break;
                case Process.AsyncParallel:
                    stopwatch.Start();
                    await RollingProcess.RollDiceAsyncParallel(Report, AssignPicture, ct);
                    stopwatch.Stop();
                    break;
                default:
                    return "0";
            }

            return $"{stopwatch.Elapsed.ToString("ss\\.fff")} s";
        }

        public void AssignPicture(int index, int value)
        {
            Visibles[index] = Visibility.Hidden;
            Sources[index] = Images[value].Source;
            Visibles[index] = Visibility.Visible;
            OnPropertyChanged(nameof(Visibles));
            OnPropertyChanged(nameof(Sources));
        }

        public void Report(int counted, int percentage)
        {
            Progress.Report(new ProgressReportModel 
            { 
                PercentageComplete = percentage,
                Total = counted
            });
            Percentage = $"{percentage}%";
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        // Events
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
