using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Canadean.Common.Wpf.ViewModel;
using System.IO;
using System.Threading;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using FreeImageAPI;

namespace File_Name_Manager
{
    public partial class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Busy.Off();
            setupRelayCommands();

            LoadCombos();

            LoadUserSettings();
        }

        private void LoadCombos()
        {

            //todo: add some code!
        }

        private bool canProcess;
        public bool CanProcess
        {
            get { return canProcess; }
            set
            {
                canProcess = value;
                OnPropertyChanged("CanProcess");
            }
        }

        private void GetSampleFileName()
        {
            if (IncludeFileName)
            {
                SampleFileName = SampleDate + "_" + "My original FileName.jpg";
            }
            else
            {
                SampleFileName = SampleDate + ".jpg";
            }
        }

        private string sampleDate;
        public string SampleDate
        {
            get { return sampleDate; }
            set
            {
                sampleDate = value;
                OnPropertyChanged("SampleDate");
            }
        }

        private string sampleSubFolderDate;
        public string SampleSubFolderDate
        {
            get { return sampleSubFolderDate; }
            set
            {
                sampleSubFolderDate = value;
                OnPropertyChanged("SampleSubFolderDate");
            }
        }


        private string sampleFileName;
        public string SampleFileName
        {
            get { return sampleFileName; }
            set
            {
                sampleFileName = value;
                OnPropertyChanged("SampleFileName");
            }
        }

        private string folderFileFoundMessage;
        public string FolderFileFoundMessage
        {
            get { return folderFileFoundMessage; }
            set
            {
                folderFileFoundMessage = value;
                OnPropertyChanged("FolderFileFoundMessage");
            }
        }

        private string progressLog;
        public string ProgressLog
        {
            get { return progressLog; }
            set
            {
                progressLog = value;
                OnPropertyChanged("ProgressLog");
            }
        }

        private Single progressPercent;
        public Single ProgressPercent
        {
            get { return progressPercent; }
            set
            {
                progressPercent = value;
                OnPropertyChanged("ProgressPercent");
            }
        }

        private string statusText;
        public string StatusText
        {
            get { return statusText; }
            set
            {
                statusText = value;
                OnPropertyChanged("StatusText");
            }
        }

        public int FolderCount { get; set; }
        public int FileCount { get; set; }

        public List<string> FileList = new List<string>();
        public List<string> FolderList = new List<string>();


        private void GetFoldersAndFilesFromPath(string pathName)
        {
            // gets the folder and file lists from a specified path recursivley.
            try
            {
                foreach (string dir in Directory.EnumerateDirectories(pathName,"*.*", SearchOption.AllDirectories))
                {
                    FolderList.Add(dir);
                }

                foreach (string f in Directory.EnumerateFiles(pathName, "*.*", SearchOption.AllDirectories))
                {
                    FileList.Add(f);
                }
            }
            catch (Exception)
            {
                FolderList.Clear();
                FileList.Clear();
                StatusText = "Error accessing folder";
            }
        }

        private bool sourceFolderIsInvalid;
        public bool SourceFolderIsInvalid
        {
            get { return sourceFolderIsInvalid; }
            set {
                sourceFolderIsInvalid = value;
                OnPropertyChanged("SourceFolderIsInvalid");
            }
        }

        private bool targetFolderIsInvalid;
        public bool TargetFolderIsInvalid
        {
            get { return targetFolderIsInvalid; }
            set
            {
                targetFolderIsInvalid = value;
                OnPropertyChanged("TargetFolderIsInvalid");
            }
        }

        // DECLARE Relay Commands
        public RelayCommand SelectSourceFolder { get; set; }
        public RelayCommand SelectTargetFolder { get; set; }
        public RelayCommand ProcessMain { get; set; }
        public RelayCommand SaveSettings { get; set; }

        private void setupRelayCommands()
        {
            SelectTargetFolder = new RelayCommand((_) => selectTargetFolder());
            SelectSourceFolder = new RelayCommand((_) => selectSourceFolder());
            ProcessMain = new RelayCommand((_) => processMain_Async());
            SaveSettings = new RelayCommand((_) => SaveUserSettings());
        }

        private DateTime GetRawExifDate(string RawImagweFileName)
        {
            DateTime result = new DateTime();

            FIBITMAP bm = new FIBITMAP();
            bm = fiLoadImage(RawImagweFileName);

            List<MetaSimple> meta = new List<MetaSimple>();
            meta = fiGetMetaData(bm);

            var md = (from m in meta
                           where m.ModelName=="FIMD_EXIF_MAIN" && m.ModelTag=="DateTime"
                            select m.TagValue).FirstOrDefault();

            result = ParseRAWDate(md);

            return result;
        }

        private static DateTime ParseRAWDate(object md)
        {
            DateTime result = new DateTime(1900, 1, 1);
            
            int colonCount = md.ToString().Count(f => f == ':');

            if (colonCount == 4) // probable colon formatted date
            {
                try
                {
                    var rgx = new Regex(Regex.Escape(":"));
                    var txt = rgx.Replace(md.ToString(), @"/", 2);

                    result = Convert.ToDateTime(txt);
                }
                catch (Exception) {}
            }

            return result;
        }

        private void selectTargetFolder()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            string selectedFolder = SelectFolder(Properties.Settings.Default.LastFolderUsed);

            if (selectedFolder != "")
            {
                TargetFolder = selectedFolder;
            }
        }
        private void selectSourceFolder()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            string selectedFolder = SelectFolder(Properties.Settings.Default.LastFolderUsed);

            if (selectedFolder != "")
            {
                SourceFolder = selectedFolder;
            }
        }

        private async void processMain_Async()
        {
            busy.On();

            SaveUserSettings();

            try
            {
                var result = await Task.Run(() =>
                {
                    processMain();
                    return "OK";
                });
                StatusText = "Completed: " + result;
            }
            catch (Exception e)
            {
                StatusText = "Failed: " + e.Message;
                ModernDialog.ShowMessage(StatusText, "Oops", System.Windows.MessageBoxButton.OK);
            }
            ProgressPercent = 100;
            AppendToProgressLog("Finished.....");
            AppendToProgressLog("");
            AppendToProgressLog("Ready.....");
            busy.Off();
            ModernDialog.ShowMessage("Process Complete", "Completed", System.Windows.MessageBoxButton.OK);
        }


        /// <summary>
        /// This method is the main process method that renames the files
        /// </summary>
        public void processMain()
        {
            // reset the log
            ProgressLog = "Started...";

            Single i = 0;
            Single t = FileList.Count;
            int skippedCount = 0;
            int renamedCount = 0;

            string newFileName;
            foreach (var fileName in FileList)
            {
                i++;
                ProgressPercent = (i - (Single)0.5) / t * 100;
                AppendToProgressLog("");
                AppendToProgressLog("Processing " + fileName);
                DateTime dt = GetEXIFDateTime(fileName);
                if (dt==new DateTime(1900,1,1))
                {
                    AppendToProgressLog("Skipping " + fileName + ". No EXIF date information found");
                    skippedCount++;
                }
                else
                {
                    string dateLabel = dt.ToString(CustomDateFormat);
                    string subFolderDateLabel = dt.ToString(SubFolderFormatInternal);
                    newFileName = RenameFile(fileName, dateLabel, subFolderDateLabel, IncludeFileName);
                    AppendToProgressLog("Renamed: " + fileName + " to ==> " + newFileName);
                    renamedCount++;
                }
            }

            AppendToProgressLog("");
            AppendToProgressLog(string.Format("Renamed {0} file and skipped {1} files.",renamedCount,skippedCount));
            AppendToProgressLog("");
        }

        //Add a text entry to the end ofthe progress log
        public void AppendToProgressLog(string s)
        {
            ProgressLog = ProgressLog + s + "\n";
        }
    }


}
