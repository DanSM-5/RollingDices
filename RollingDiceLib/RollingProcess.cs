using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RollingDiceLib
{
    public class RollingProcess
    {
        private int NumOfDices { get; set; }
        private int Sequence { get; set; }
        private List<Counter> CounterList { get; set; }

        // Current number of operations
        public int Progress { get; set; } = 0;
        // Percentage of the current progress completed
        public int Percentage => Convert.ToInt32(Math.Ceiling((double)Progress * 100 / Total));

        // Total number of operations
        public long Total => NumOfDices * Sequence;
        // One percent of the number of operations. Used to report progress to the progress bar.
        public long Step => Total / 100;
        // Cancel operation if True
        public bool Cancel { get; set; } = false;

        public RollingProcess(int numOfDices, int sequence)
        {
            NumOfDices = numOfDices;
            Sequence = sequence;
        }

        /// <summary>
        /// Roll dices synchronously blocking GUI
        /// </summary>
        public void RollDiceSyncBlocking(Action<int,int> report, Action<int,int> done, CancellationToken ct)
        {
            CounterList = new List<Counter>();
            Counter counter;
            for (int i = 0; i < NumOfDices; i++)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                counter = new Counter();
                CounterList.Add(counter);

                // Run the number of dices synchronously blocking the UI
                RollingTaskSync(counter, i, report, done, ct);
            }

            // Report progress at the end of the process
            report(Progress,Percentage);
        }

        /// <summary>
        /// Roll dices asynchronously in sequence
        /// </summary>
        public async Task RollDiceAsyncNonBlocking(Action<int, int> report, Action<int,int> done, CancellationToken ct)
        {
            CounterList = new List<Counter>();
            Counter counter;
            for (int i = 0; i < NumOfDices; i++)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                counter = new Counter();
                CounterList.Add(counter);

                // Wait for each task to be completed
                await RollingTaskAsync(counter, i, report, done, ct);
            }

            // Report progress at the end of the process
            report(Progress, Percentage);
        }

        /// <summary>
        /// Roll dices asynchronously in parallel tasks
        /// </summary>
        public async Task RollDiceAsyncParallel(Action<int, int> report, Action<int,int> done, CancellationToken ct)
        {
            // List to save the reference of the tasks
            var tasks = new List<Task<int>>();
            CounterList = new List<Counter>();
            Counter counter;
            for (int i = 0; i < NumOfDices; i++)
            {
                counter = new Counter();
                CounterList.Add(counter);

                // call of independent rolling tasks
                // that will be executed in parallel
                tasks.Add(RollingTaskAsync(counter, i, report, done, ct));
            }
            // Wait for all the rolling tasks to finish
            var results = await Task.WhenAll(tasks);

            // Report progress at the end of the process
            report(Progress, Percentage);
        }

        /// <summary>
        /// Executes a rolling task asynchronously
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="index"></param>
        /// <returns>Returns the final result of the dice as an integer</returns>
        private async Task<int> RollingTaskAsync(Counter counter, int diceNumber, Action<int, int> report, Action<int,int> done, CancellationToken ct)
        {
            #region Task
            counter.Result = await Task.Run(() => {
                int result = 1;
                long prev = Step;
                Random random = new Random();
                for (int j = 0; j < Sequence; j++)
                {
                    // Break the loop of the rolling process
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    // Rolling simulation
                    result = random.Next(1, 7);
                    // Saving progress
                    counter.Value++;

                    // Update on screen everytime it has progressed 1%
                    if (prev < j)
                    {
                        Progress = CounterList.Select(c => c.Value).Sum();
                        report(Progress, Percentage);
                        // Increase to update again after 1% of the process
                        prev += Step;
                    }
                }
                // If task is cancel use a value generated by the random object
                if (ct.IsCancellationRequested)
                {
                    return random.Next(1, 7);
                }
                // Otherwise, create a new random to return a different number
                else
                {
                    return (new Random()).Next(1, 7);
                }
            });
            #endregion Task

            // Report final result
            Progress = CounterList.Select(c => c.Value).Sum();
            done(diceNumber, counter.Result);

            // Report progress at the end of the process  
            report(Progress, Percentage);

            return counter.Result;
        }


        /// <summary>
        /// Executes a rolling task synchronously
        /// </summary>
        private int RollingTaskSync(Counter counter, int diceNumber, Action<int, int> report, Action<int,int> done, CancellationToken ct)
        {
            int result = 1;
            long prev = Step;
            Random random = new Random();
            for (int j = 0; j < Sequence; j++)
            {
                // Break the loop of the rolling process
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                // Rolling simulation
                result = random.Next(1, 7);
                // Saving progress
                counter.Value++;

                // Update on screen everytime it has progressed 1%
                if (prev < j)
                {
                    Progress = CounterList.Select(c => c.Value).Sum();
                    report(Progress, Percentage);
                    // Increase to update again after 1% of the process
                    prev += Step;
                }
            }

            // Set result
            counter.Result = result;

            // Report final result
            done(diceNumber, counter.Result);

            // Report progress at the end of the process 
            report(Progress,Percentage);

            return counter.Result;
        }
    }
}
