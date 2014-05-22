using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExifLib;
using System.IO;

namespace File_Name_Manager
{
    public partial class MainViewModel
    {

        public DateTime GetEXIFDateTime(string imageFile)
        {
            DateTime retValue = new DateTime(1900, 1, 1);
            DateTime datePictureTaken;
            try // just getting the Standard EXIF data
            {
                using (ExifReader r = new ExifReader(imageFile))
                {
                    if (r.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out datePictureTaken))
                    {
                        retValue = datePictureTaken;
                    }
                }
            }
            catch (Exception) // if standard EXIF data cannot be found
            {
                try // getting the getting the RAW Exif data
                {
                    if (UseRawDate)
                    {
                        AppendToProgressLog("No standard EXIF data found in " + imageFile + " Attempting to get RAW EXIF date");
                        retValue = GetRawExifDate(imageFile);                        
                    }
                }
                catch (Exception) // if that still fails
                {
                    if (UseFileDateOnError) // use the file date if user has selected this option
                    {
                        try
                        {
                            AppendToProgressLog("No EXIF data found in " + imageFile + " attempting to get file date.");
                            datePictureTaken = File.GetLastWriteTime(imageFile);
                            retValue = datePictureTaken;
                        }
                        catch (Exception) 
                        {
                            AppendToProgressLog("Unable to get file date. Cannot process this file");
                        } // we're dead in the water, just pass back the default date

                    }
                }
            }

            if (retValue==new DateTime(1900,1,1))
            {
                AppendToProgressLog("Unable to file a useable date. Cannot process this file.");
            }

            return retValue;
        }
    }
     
}
