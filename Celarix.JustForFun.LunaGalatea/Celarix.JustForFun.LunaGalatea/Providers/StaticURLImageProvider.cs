using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public class StaticURLImageProvider : IProvider<KeyValuePair<string, string>>
    {
        private readonly KeyValuePair<string, string>[] urls;
        private readonly Random random;
        private readonly HttpClient client;

        public StaticURLImageProvider()
        {
            urls = new[]
            {
                new KeyValuePair<string, string>("2nd and Broadway",
                    "https://trimarc.org/images/milestone/CCTV_05_2nd_&_Broadway.jpg"),
                new KeyValuePair<string, string>("2nd and Main",
                    "https://trimarc.org/images/milestone/CCTV_05_2nd_&_Main.jpg"),
                new KeyValuePair<string, string>("2nd and Market",
                    "https://trimarc.org/images/milestone/CCTV_05_2nd_&_Market.jpg"),
                new KeyValuePair<string, string>("9th and Market",
                    "https://trimarc.org/images/milestone/CCTV_05_9th_&_Market.jpg"),
                new KeyValuePair<string, string>("Brook and Broadway",
                    "https://trimarc.org/images/milestone/CCTV_05_Brook_&_Broadway.jpg"),
                new KeyValuePair<string, string>("I-264 at Bank Street",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0007.jpg"),
                new KeyValuePair<string, string>("I-264 at Bardstown Road",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0156.jpg"),
                new KeyValuePair<string, string>("I-264 at Bells Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0041.jpg"),
                new KeyValuePair<string, string>("I-264 at Breckenridge Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0179.jpg"),
                new KeyValuePair<string, string>("I-264 at Brownsboro Road",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0221.jpg"),
                new KeyValuePair<string, string>("I-264 at Cane Run Road",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0052.jpg"),
                new KeyValuePair<string, string>("I-264 at Crums Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0060.jpg"),
                new KeyValuePair<string, string>("I-264 at Dixie Highway",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0075.jpg"),
                new KeyValuePair<string, string>("I-264 at Freedom Way",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0116.jpg"),
                new KeyValuePair<string, string>("I-264 at I-71",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0051.jpg"),
                new KeyValuePair<string, string>("I-264 at I-71",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0051.jpg"),
                new KeyValuePair<string, string>("I-264 at Newburg Road",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0145.jpg"),
                new KeyValuePair<string, string>("I-264 at Poplar Level Road",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0133.jpg"),
                new KeyValuePair<string, string>("I-264 at River Park Drive",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0016.jpg"),
                new KeyValuePair<string, string>("I-264 at Shelbyville Road",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0199.jpg"),
                new KeyValuePair<string, string>("I-264 at Taylor Boulevard",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0093.jpg"),
                new KeyValuePair<string, string>("I-264 at Taylorsville Road",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0170.jpg"),
                new KeyValuePair<string, string>("I-264 at Virginia Avenue and Dumesnil Street",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0027.jpg"),
                new KeyValuePair<string, string>("I-264 at Westport Road",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0208.jpg"),
                new KeyValuePair<string, string>("I-264 between Dixie Highway and Crums Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_264_0065.jpg"),
                new KeyValuePair<string, string>("I-265 South of the Lewis and Clark Bridge",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0384.jpg"),
                new KeyValuePair<string, string>("I-265 at Bardstown Road",
                    "https://trimarc.org/images/milestone/CCTV_05_265_0173.jpg"),
                new KeyValuePair<string, string>("I-265 at I-64",
                    "https://trimarc.org/images/milestone/CCTV_05_265_0256.jpg"),
                new KeyValuePair<string, string>("I-265 at I-71",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0349.jpg"),
                new KeyValuePair<string, string>("I-265 at LaGrange Road",
                    "https://trimarc.org/images/milestone/CCTV_05_265_0305.jpg"),
                new KeyValuePair<string, string>("I-265 at Old Henry Road",
                    "https://trimarc.org/images/milestone/CCTV_05_265_0287.jpg"),
                new KeyValuePair<string, string>("I-265 at Shelbyville Road",
                    "https://trimarc.org/images/milestone/CCTV_05_265_0268.jpg"),
                new KeyValuePair<string, string>("I-265 at Smyrna Parkway",
                    "https://trimarc.org/images/milestone/CCTV_05_265_0135.jpg"),
                new KeyValuePair<string, string>("I-265 at Taylorsville Road",
                    "https://trimarc.org/images/milestone/CCTV_05_265_0231.jpg"),
                new KeyValuePair<string, string>("I-265 at US-42 entrance ramp",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0366.jpg"),
                new KeyValuePair<string, string>("I-265 at Westport Road",
                    "https://trimarc.org/images/milestone/CCTV_05_265_0325.jpg"),
                new KeyValuePair<string, string>("I-265 at mile marker 38",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0380.jpg"),
                new KeyValuePair<string, string>("I-265 at the tunnel",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0369.jpg"),
                new KeyValuePair<string, string>("I-265 near Harrods Creek",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0375.jpg"),
                new KeyValuePair<string, string>("I-265 near I-71",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0356.jpg"),
                new KeyValuePair<string, string>("I-265 near US-42",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0361.jpg"),
                new KeyValuePair<string, string>("I-265 past the tunnel",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0373.jpg"),
                new KeyValuePair<string, string>("I-64 East of Cochran Hill Tunnel",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0088.jpg"),
                new KeyValuePair<string, string>("I-64 Eastbound at 22nd Street 3.1",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0030.jpg"),
                new KeyValuePair<string, string>("I-64 West of 22nd Street",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0022.jpg"),
                new KeyValuePair<string, string>("I-64 West of Oxmoor Farm Overpass",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0131.jpg"),
                new KeyValuePair<string, string>("I-64 at 9th Street",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0040.jpg"),
                new KeyValuePair<string, string>("I-64 at Blankenbaker Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0171.jpg"),
                new KeyValuePair<string, string>("I-64 at Breckenridge Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0114.jpg"),
                new KeyValuePair<string, string>("I-64 at Clark Station",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0238.jpg"),
                new KeyValuePair<string, string>("I-64 at Echo Trail",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0214.jpg"),
                new KeyValuePair<string, string>("I-64 at English Station Road",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0195.jpg"),
                new KeyValuePair<string, string>("I-64 at Frazier Museum",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0041.jpg"),
                new KeyValuePair<string, string>("I-64 at Grinstead Drive",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0079.jpg"),
                new KeyValuePair<string, string>("I-64 at Hurstbourne Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0150.jpg"),
                new KeyValuePair<string, string>("I-64 at I-254 (East intersection)",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0121.jpg"),
                new KeyValuePair<string, string>("I-64 at I-264",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0012.jpg"),
                new KeyValuePair<string, string>("I-64 at I-265",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0189.jpg"),
                new KeyValuePair<string, string>("I-64 at Mellwood Avenue",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0068.jpg"),
                new KeyValuePair<string, string>("I-64 at Oxmoor Farm Road",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0138.jpg"),
                new KeyValuePair<string, string>("I-64 at mile marker 5.3",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0053.jpg"),
                new KeyValuePair<string, string>("I-64 at mile marker 5.5",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0055.jpg"),
                new KeyValuePair<string, string>("I-64 at mile marker 5.8",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0058.jpg"),
                new KeyValuePair<string, string>("I-64 at mile marker 6.3",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0061_WB.jpg"),
                new KeyValuePair<string, string>("I-64 at the Jefferson County Line",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0242.jpg"),
                new KeyValuePair<string, string>("I-64 near I-264 (West Louisville)",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0008.jpg"),
                new KeyValuePair<string, string>("I-64 near I-65 (alternate)",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0054-2.jpg"),
                new KeyValuePair<string, string>("I-64 near I-65",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0054-1.jpg"),
                new KeyValuePair<string, string>("I-64 near Slugger Field",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0050.jpg"),
                new KeyValuePair<string, string>("I-64 over East Muhammad Ali Boulevard",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1356.jpg"),
                new KeyValuePair<string, string>("I-65 South at the Kennedy Bridge",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1365.jpg"),
                new KeyValuePair<string, string>("I-65 adjacent to Arthur Street",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1337.jpg"),
                new KeyValuePair<string, string>("I-65 adjacent to the Fairgrounds",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1317.jpg"),
                new KeyValuePair<string, string>("I-65 at Eastern Parkway",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1330.jpg"),
                new KeyValuePair<string, string>("I-65 at Fern Valley Road",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1283.jpg"),
                new KeyValuePair<string, string>("I-65 at Grade Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1297.jpg"),
                new KeyValuePair<string, string>("I-65 at I-264",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1307.jpg"),
                new KeyValuePair<string, string>("I-65 at I-265",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1252.jpg"),
                new KeyValuePair<string, string>("I-65 at Outer Loop",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1267.jpg"),
                new KeyValuePair<string, string>("I-65 at Saint Catherine",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1344.jpg"),
                new KeyValuePair<string, string>("I-65 near Broadway",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1353.jpg"),
                new KeyValuePair<string, string>("I-65 near I-64 and I-71",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1362.jpg"),
                new KeyValuePair<string, string>("I-65 next to Crittenden Drive",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1327.jpg"),
                new KeyValuePair<string, string>("I-65 on the Kennedy Bridge at mile marker 136.8",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1368.jpg"),
                new KeyValuePair<string, string>("I-65 on the Kennedy Bridge at mile marker 137",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1370.jpg"),
                new KeyValuePair<string, string>("I-65 on the Lincoln Bridge at mile marker 136.7",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1367.jpg"),
                new KeyValuePair<string, string>("I-65 on the Lincoln Bridge at mile marker 136.9",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1369.jpg"),
                new KeyValuePair<string, string>("I-65 over Phillips Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_65_1313.jpg"),
                new KeyValuePair<string, string>("I-71 North of I-264",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0055.jpg"),
                new KeyValuePair<string, string>("I-71 North of I-264",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0055.jpg"),
                new KeyValuePair<string, string>("I-71 North of I-265",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0116.jpg"),
                new KeyValuePair<string, string>("I-71 at Blankenbaker Lane",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0037.jpg"),
                new KeyValuePair<string, string>("I-71 at Frankfort Avenue",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0001.jpg"),
                new KeyValuePair<string, string>("I-71 at I-264",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0048.jpg"),
                new KeyValuePair<string, string>("I-71 at I-264",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0048.jpg"),
                new KeyValuePair<string, string>("I-71 at I-265",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0089.jpg"),
                new KeyValuePair<string, string>("I-71 at Indian Hills Avenue",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0030.jpg"),
                new KeyValuePair<string, string>("I-71 at Zorn Avenue",
                    "https://trimarc.org/images/milestone/CCTV_05_71_0018.jpg"),
                new KeyValuePair<string, string>("KY-22 West at I-265",
                    "https://trimarc.org/images/milestone/CCTV_05_ADMS111_KY22-WB.jpg"),
                new KeyValuePair<string, string>("KY-841 at New Cut Road",
                    "https://trimarc.org/images/milestone/CCTV_05_841_0060.jpg"),
                new KeyValuePair<string, string>("Westbound I-64 at mile marker 6.1",
                    "https://trimarc.org/images/milestone/CCTV_05_64_0061_EB.jpg"),
                new KeyValuePair<string, string>("NOAA Weather Radar",
                    "https://radar.weather.gov/ridge/standard/KLVX_0.gif")
            };

            random = new Random();
            client = new HttpClient();
        }

        public KeyValuePair<string, string> GetDisplayObject()
        {
            var randomImageKvp = urls[random.Next(0, urls.Length)];
            var savePath = GetImageSavePath(randomImageKvp.Value);

            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            try
            {
                // https://stackoverflow.com/a/66655958
                var request = new HttpRequestMessage(HttpMethod.Get, randomImageKvp.Value);
                var response = client.Send(request);
                var inputStream = response.Content.ReadAsStream();
                using Stream outputStream = File.OpenWrite(savePath);
                inputStream.CopyTo(outputStream);
                outputStream.Close();

                return new KeyValuePair<string, string>(randomImageKvp.Key, savePath);
            }
            catch (Exception ex)
            {
                return new KeyValuePair<string, string>($"{randomImageKvp.Key}: Failed to load {randomImageKvp.Value}: {ex.Message}", null);
            }
        }

        private static string GetImageSavePath(string url)
        {
            var baseFolderPath = Environment.MachineName.ToLowerInvariant() switch
            {
                "pavilion-core" => @"G:\Documents\Files\Pictures\Miscellaneous\TRIMARC",
                "akridge-pc" => @"C:\Users\ChrisAckridge\Pictures\TRIMARC",
                "bluebell01" => @"C:\Users\celarix\Pictures\TRIMARC",
                "chris-hp15" => @"C:\Users\cakri\Pictures\TRIMARC",
				_ => throw new ArgumentOutOfRangeException()
            };

            var imageFolderPath = Path.GetFileNameWithoutExtension(url);
            var imageFileName = Path.GetFileNameWithoutExtension(url)
                + $"_{DateTimeOffset.Now:yyyyMMdd_HHmmss}{Path.GetExtension(url)}";

            return Path.Combine(baseFolderPath, imageFolderPath, imageFileName);
        }
    }
}
