using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentOfPictures
{
    internal static class ImageLoader
    {
        private const string BadImageName = "badImage.png";

        public static Image LoadImage(string filePath)
        {
            try
            {
                string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
                var fileStream = new MemoryStream(File.ReadAllBytes(filePath));
                var image = Image.FromStream(fileStream);
                fileStream.Position = 0;

                if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                {
                    var metadataDirectories = ImageMetadataReader.ReadMetadata(fileStream);
                    var orientationTag = metadataDirectories.SelectMany(d => d.Tags).FirstOrDefault(t => t.Name.Contains("Orientation"));

                    if (orientationTag != null)
                    {
                        string tagDescription = orientationTag.Description;
                        if (tagDescription.Contains("Rotate 180")) { image.RotateFlip(RotateFlipType.Rotate180FlipNone); }
                        else if (tagDescription.Contains("Rotate 90 CW")) { image.RotateFlip(RotateFlipType.Rotate90FlipNone); }
                        else if (tagDescription.Contains("Rotate 90 CCW")) { image.RotateFlip(RotateFlipType.Rotate270FlipNone); }
                    }
                }
                return image;
            }
            catch
            {
                return Image.FromStream(new MemoryStream(File.ReadAllBytes(BadImageName)));
            }
        }
    }
}
