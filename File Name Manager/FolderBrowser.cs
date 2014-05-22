using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Controls;

namespace File_Name_Manager
{
    public partial class MainViewModel
    {

        public string SelectFolder(string defaultFolder)
        {
            string folderName = "";

            System.Windows.Forms.FolderBrowserDialog openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            openFolderDialog.SelectedPath = defaultFolder;

            // Show open file dialog box 
            System.Windows.Forms.DialogResult result = openFolderDialog.ShowDialog();
            if (result.ToString() == "OK")
            {
                folderName = openFolderDialog.SelectedPath;
            }

            return folderName;
        }

    }
}
