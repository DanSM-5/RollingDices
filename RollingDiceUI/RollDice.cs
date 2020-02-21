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
        private Process processType;
        private string time;

        // Properties
        public ObservableCollection<int> AvailableDices { get; } 
        public ObservableCollection<int> ValidSequences { get; }
        public List<Image> Images { get; }
        public List<ImageSource> Sources { get; }
        public List<Visibility> Visibles { get; }
        public IProgress<ProgressReportModel> Progress { get; set; }

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
        public RollDice(string path, IEnumerable<int> dices, IEnumerable<int> sequences)
        {
            AvailableDices = new ObservableCollection<int>(dices);
            ValidSequences = new ObservableCollection<int>(sequences);

            Images = new List<Image>();
            Sources = new List<ImageSource> { null, null, null, null, null, null, null, null};
            Visibles = new List<Visibility>();
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

        /// <summary>
        /// Enebles/disebles images on the UI
        /// </summary>
        /// <param name="number"></param>
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

        /// <summary>
        /// Set values to default
        /// </summary>
        private void SetInitialValues()
        {
            for(int i = 0; i < Sources.Count; i++)
            {
                Sources[i] = Images[0].Source;
            }
            OnPropertyChanged(nameof(Sources));
        }

        /// <summary>
        /// Starts de execution of the rolling tasks
        /// </summary>
        /// <param name="progress">Object to report and update the UI</param>
        /// <param name="ct">Token to cancel the process of rolling</param>
        /// <returns>The time required to finish the process</returns>
        public async Task<string> ExecuteRoll(IProgress<ProgressReportModel> progress, CancellationToken ct)
        {
            Progress = progress;
            RollingProcess rollingProcess = new RollingProcess(NumOfDice, Sequence);
            SetInitialValues();
            Progress.Report(new ProgressReportModel { PercentageComplete = 0, Total = 0});
            Stopwatch stopwatch = new Stopwatch();

            switch (ProcessType)
            {
                case Process.Sync:
                    stopwatch.Start();
                    rollingProcess.RollDiceSyncBlocking(Report,AssignPicture, ct);
                    stopwatch.Stop();
                    break;
                case Process.Async:
                    stopwatch.Start();
                    await rollingProcess.RollDiceAsyncNonBlocking(Report, AssignPicture, ct);
                    stopwatch.Stop();
                    break;
                case Process.AsyncParallel:
                    stopwatch.Start();
                    await rollingProcess.RollDiceAsyncParallel(Report, AssignPicture, ct);
                    stopwatch.Stop();
                    break;
                default:
                    return "0";
            }

            return $"{stopwatch.Elapsed.ToString("ss\\.fff")} s";
        }

        /// <summary>
        /// Assign the picture based in the result of the rolling process
        /// </summary>
        /// <param name="index">Index of the dice in the collection of sources and visibles</param>
        /// <param name="value">Value of the result</param>
        public void AssignPicture(int index, int value)
        {
            Visibles[index] = Visibility.Hidden;
            Sources[index] = Images[value].Source;
            Visibles[index] = Visibility.Visible;
            OnPropertyChanged(nameof(Visibles));
            OnPropertyChanged(nameof(Sources));
        }

        /// <summary>
        /// Reports the current progress of the rolling process
        /// </summary>
        /// <param name="counted">Number of </param>
        /// <param name="percentage"></param>
        public void Report(int counted, int percentage)
        {
            Progress.Report(new ProgressReportModel 
            { 
                PercentageComplete = percentage,
                Total = counted
            });
        }

        /// <summary>
        /// Reports when a property has changed value
        /// </summary>
        /// <param name="property">Name of the property to report</param>
        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        // Events
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
