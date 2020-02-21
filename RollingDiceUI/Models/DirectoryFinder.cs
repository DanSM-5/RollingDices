using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RollingDiceUI.Models
{
    public class DirectoryFinder
    {
        private string DirectoryPath { get; set; }
        public bool IsFound { get; set; }

        public DirectoryFinder()
        {
            DirectoryPath = "";
            IsFound = false;
        }

        public DirectoryFinder(string directoryName) : this(directoryName, null) { }

        public DirectoryFinder(string directoryName, params string[] fileNames)
        {
            FindPath(directoryName, fileNames);
        }

        /// <summary>
        /// Get the path of the last directory provided
        /// </summary>
        /// <returns>The path of the directory if it was found, otherwise it returns an empty string.</returns>
        public string GetDirectoryPath()
        {
            return GetDirectoryPath(null, null);
        }

        /// <summary>
        /// Get the path of the specified directory
        /// </summary>
        /// <param name="directoryName">>Name of the directory to look for</param>
        /// <returns>The path of the directory if it was found, otherwise it returns an empty string.</returns>
        public string GetDirectoryPath(string directoryName)
        {          
            return GetDirectoryPath(directoryName, null);
        }

        /// <summary>
        /// Get the path of the specified directory if all the required files exist
        /// </summary>
        /// <param name="directoryName">Name of the directory to look for</param>
        /// <param name="fileNames">Name of the files to identify</param>
        /// <returns>The path of the directory if it was found and all the required fieles are found, otherwise it returns an empty string.</returns>
        public string GetDirectoryPath(string directoryName, params string[] fileNames)
        {
            if (!String.IsNullOrEmpty(directoryName))
            {
                FindPath(directoryName, fileNames); 
            }

            if (IsFound)
            {
                return DirectoryPath;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Searches for the path of a directory and confirms the existance of the specified files
        /// </summary>
        /// <param name="directoryName">Name of the directory to look for</param>
        /// <param name="fileNames">Name of the files to identify</param>
        private void FindPath(string directoryName, params string[] fileNames)
        {
            if (!String.IsNullOrEmpty(directoryName))
            {
                try
                {
                    IsFound = true;
                    DirectoryPath = Environment.CurrentDirectory;

                    while (!(Directory.Exists($"{DirectoryPath}\\{directoryName}")))
                    {
                        if (DirectoryPath.Equals(Directory.GetDirectoryRoot(DirectoryPath)))
                        {
                            IsFound = false;
                            DirectoryPath = "";
                            break;
                        }
                        DirectoryPath = Path.GetFullPath(Path.Combine(DirectoryPath, @"..\"));
                    }

                    if (IsFound)
                    {
                        DirectoryPath = $"{DirectoryPath}\\{directoryName}\\";

                        if (fileNames != null)
                        {
                            foreach (var name in fileNames)
                            {
                                if (!File.Exists($"{DirectoryPath}{name}"))
                                {
                                    DirectoryPath = "";
                                    IsFound = false;
                                }
                            }
                        }
                    } 
                }
                catch (Exception)
                {
                    DirectoryPath = "";
                    IsFound = false;
                }
            }
            else
    	    {
                DirectoryPath = "";
                IsFound = false;
            }
        }
    }
}
