using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeImageAPI;
using FreeImageAPI.Metadata;

namespace File_Name_Manager
{
    public partial class MainViewModel
    {
        public class MetaSimple
        {
            public string ModelName { get; set; }
            public string ModelTag { get; set; }
            public object TagValue { get; set; }
        }

        public FIBITMAP fiLoadImage (string fileName)
        {
            FIBITMAP bm = new FIBITMAP();

            try 
        	{	        
                // Load the image
			    bm = FreeImage.LoadEx(fileName);
	        }
	        catch (Exception)
	        {
		        throw new Exception("Failed to load image.");
	        }

            return bm;
        }

        public List<MetaSimple> fiGetMetaData(FIBITMAP bm)
        {
            List<MetaSimple> results = new List<MetaSimple>();

            // Create a wrapper for all metadata the image contains
			ImageMetadata iMetadata = new ImageMetadata(bm);

            // Get each metadata model
            foreach (MetadataModel metadataModel in iMetadata)
            {
                // Get each metadata tag and create a subnode for it
                foreach (MetadataTag metadataTag in metadataModel)
                {
                    MetaSimple tagValue = new MetaSimple();

                    tagValue.ModelName = metadataModel.ToString();
                    tagValue.ModelTag = metadataTag.Key;
                    tagValue.TagValue = metadataTag;

                    results.Add(tagValue);
                }

            }

            return results;

        }


    }
}
