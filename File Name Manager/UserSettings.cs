using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Canadean.Common.Wpf.ViewModel;

namespace File_Name_Manager
{
    public partial class MainViewModel : ViewModelBase
    {
        private void SaveUserSettings()
        {
            // Save settings for next time...
            Properties.Settings.Default.LastCustomDateFormat = CustomDateFormat;
            Properties.Settings.Default.LastFolderUsed = SourceFolder;
            Properties.Settings.Default.LastIncludeFileName = IncludeFileName;
            Properties.Settings.Default.UseFileDateOnError = useFileDateOnError;
            Properties.Settings.Default.RenameOnlyMode = RenameOnlyMode;
            Properties.Settings.Default.UseSourceImageFolder = UseSourceImageFolder;
            Properties.Settings.Default.LastTargetFolder = TargetFolder;
            Properties.Settings.Default.CreateSubFoldersByDate = CreateSubFoldersByDate;
            Properties.Settings.Default.SubFolderFormat = SubFolderFormat;
            Properties.Settings.Default.KeepOriginal = KeepOriginal;
            Properties.Settings.Default.UseRawDate = UseRawDate;

            Properties.Settings.Default.Save();
        }

        private void LoadUserSettings()
        {
            SourceFolder = Properties.Settings.Default.LastFolderUsed;
            CustomDateFormat = Properties.Settings.Default.LastCustomDateFormat;
            IncludeFileName = Properties.Settings.Default.LastIncludeFileName;
            UseFileDateOnError = Properties.Settings.Default.UseFileDateOnError;
            RenameOnlyMode = Properties.Settings.Default.RenameOnlyMode;
            UseSourceImageFolder = Properties.Settings.Default.UseSourceImageFolder;
            TargetFolder = Properties.Settings.Default.LastTargetFolder;
            CreateSubFoldersByDate = Properties.Settings.Default.CreateSubFoldersByDate;
            SubFolderFormat = Properties.Settings.Default.SubFolderFormat;
            KeepOriginal = Properties.Settings.Default.KeepOriginal;
            UseRawDate = Properties.Settings.Default.UseRawDate;
        }

        private bool renameOnlyMode;
        public bool RenameOnlyMode
        {
            get { return renameOnlyMode; }
            set
            {
                renameOnlyMode = value;
                OnPropertyChanged("RenameOnlyMode");
            }
        }


        private bool includeFileName;
        public bool IncludeFileName
        {
            get { return includeFileName; }
            set
            {
                includeFileName = value;
                OnPropertyChanged("IncludeFileName");
                GetSampleFileName();
            }
        }

        private string customDateFormat;
        public string CustomDateFormat
        {
            get { return customDateFormat; }
            set
            {
                customDateFormat = value;
                OnPropertyChanged("CustomDateFormat");
                SampleDate = DateTime.Now.ToString(value);
                GetSampleFileName();
            }
        }

        private string sourceFolder;
        public string SourceFolder
        {
            get { return sourceFolder; }
            set
            {
                sourceFolder = value;
                Busy.On();
                OnPropertyChanged("SourceFolder");
                SourceFolderIsInvalid = !Directory.Exists(value);

                if (!SourceFolderIsInvalid)
                {
                    GetFoldersAndFilesFromPath(value);
                    FolderCount = FolderList.Count;
                    FileCount = FileList.Count;
                    FolderFileFoundMessage = string.Format("Found {0} file(s) in {1} folder(s)", FileCount, FolderCount);
                    CanProcess = (FileCount > 0);
                }
                else
                {
                    FolderFileFoundMessage = "No valid folder selected";
                }
                Busy.Off();
            }
        }



        /// <summary>
        /// user option, if not EXIF data exists, use physical filedate.
        /// Currently this will also apply to files with EXIF info that cannot be normally accessed such as in *.RAW/*.CR2/*.NEF files
        /// TODO: Add support for these file types. Need to investage how Windows O/S uses Codecs to load this info.
        /// </summary>
        private bool useFileDateOnError;
        public bool UseFileDateOnError
        {
            get { return useFileDateOnError; }
            set
            {
                useFileDateOnError = value;
                OnPropertyChanged("UseFileDateOnError");
            }
        }

        /// <summary>
        /// If standard EXIF date is not found use the RAW date
        /// WARNING, the option can be slow!!
        /// </summary>
        private bool useRawDate;
        public bool UseRawDate
        {
            get { return useRawDate; }
            set
            {
                useRawDate = value;
                OnPropertyChanged("UseRawDate");
            }
        }

        /// <summary>
        /// Used in conjunction with the Move & Rename option
        /// This will effectively copy the original and rename it, leaving the original intact.
        /// </summary>
        private bool keepOriginal;
        public bool KeepOriginal
        {
            get { return keepOriginal; }
            set
            {
                keepOriginal = value;
                OnPropertyChanged("KeepOriginal");
            }
        }

        private bool useSourceImageFolder;
        public bool UseSourceImageFolder
        {
            get { return useSourceImageFolder; }
            set
            {
                useSourceImageFolder = value;
                OnPropertyChanged("UseSourceImageFolder");
            }
        }

        private string targetFolder;
        public string TargetFolder
        {
            get { return targetFolder; }
            set
            {
                targetFolder = value;
                OnPropertyChanged("TargetFolder");
            }
        }

        private bool createSubFoldersByDate;
        public bool CreateSubFoldersByDate
        {
            get { return createSubFoldersByDate; }
            set
            {
                createSubFoldersByDate = value;
                OnPropertyChanged("CreateSubFoldersByDate");
            }
        }

        private string subFolderFormat;
        public string SubFolderFormat
        {
            get { return subFolderFormat; }
            set
            {
                subFolderFormat = value;
                OnPropertyChanged("SubFolderFormat");
                SubFolderFormatInternal = value.Replace(@"/", @"\\");
            }
        }

        private string subFolderFormatInternal;
        public string SubFolderFormatInternal
        {
            get { return subFolderFormatInternal; }
            set
            {
                subFolderFormatInternal = value;
                OnPropertyChanged("SubFolderFormatInternal");
                SampleSubFolderDate = GetFormattedDate(value);
            }
        }


        private string GetFormattedDate(string dateFormat)
        {
            string result="";

            try
            {
                result = DateTime.Now.ToString(dateFormat);
            }
            catch (Exception)
            {

                result = "";
            }
            return result;

        }

        

    }
}
