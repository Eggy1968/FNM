using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Name_Manager
{
    public partial class MainViewModel
    {


        // TODO: Add code to create the target folder
        private string RenameFile(string fileName, string dateLabel, string subFolderDateLabel, bool includeFileName)
        {
            string oldFileNamePart;
            string newFileNamePart;
            string fileExtention;
            string filePath;

            string errorMessage = "";

            oldFileNamePart = Path.GetFileNameWithoutExtension(fileName);
            fileExtention = Path.GetExtension(fileName);

            if (RenameOnlyMode)
            {
                filePath = Path.GetDirectoryName(fileName);            
            }
            else
            {
                filePath = (UseSourceImageFolder) ? SourceFolder : TargetFolder;

                if (CreateSubFoldersByDate)
                {
                    filePath = Path.Combine(filePath, subFolderDateLabel);
                }
            }

            // create the folder if required.
            try
            {
                Directory.CreateDirectory(filePath);
            }
            catch (Exception e)
            {
                AppendToProgressLog(e.Message);
            }

            newFileNamePart = dateLabel;

            if (includeFileName)
            {
                newFileNamePart = newFileNamePart + "_" + oldFileNamePart;
            }

            newFileNamePart = newFileNamePart + fileExtention;

            try
            {
                string finalFileName = Path.Combine(filePath, newFileNamePart);
                if (KeepOriginal)
                {
                    File.Copy(fileName, finalFileName);
                }
                else
                {
                    File.Move(fileName, finalFileName);
                }
            }
            catch (Exception e)
            {
                errorMessage = "ERROR ! : " + e.Message + " ";
            }

            string result = "";
            if (RenameOnlyMode)
            {
                result = errorMessage + newFileNamePart;
            }
            else
            {
                result = errorMessage + Path.Combine(filePath, newFileNamePart);
            }

            return result;
        }

    }
}
