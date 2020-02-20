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
        private string ImagesPath { get; set; }
        public bool IsFound { get; set; }
        public DirectoryFinder()
        {
            try
            {
                ImagesPath = Environment.CurrentDirectory;

                while (!(Directory.Exists($"{ImagesPath}\\Images")))
                {
                    if (ImagesPath.Equals(Directory.GetDirectoryRoot(ImagesPath)))
                    {
                        IsFound = false;
                        break;
                    }
                    ImagesPath = Path.GetFullPath(Path.Combine(ImagesPath, @"..\"));
                }

                if (File.Exists($"{ImagesPath}\\Images\\0.png"))
                {
                    IsFound = true;
                    ImagesPath = $"{ImagesPath}Images\\";
                }
                else
                {
                    IsFound = false;
                    ImagesPath = "";
                }
            }
            catch (Exception)
            {
                ImagesPath = "";
                IsFound = false;
            }
        }

        public string GetImagesPath()
        {
            if (IsFound)
            {
                return ImagesPath;
            }
            else
            {
                return "";
            }
        }
    }
}
