
using IronPdf;
using IronPdf.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Hosting;
using PuppeteerSharp.Media;
using PuppeteerSharp;
using System.IO;

namespace GlobalUtility
{
    public static class PDF_Builder
    {
        public static readonly string ImageURL = Path.Combine("www.root", "icons", "server", "ServerLogo.jpg");

        public static readonly string TemplatePath = Path.Combine("www.root", "Invoice", "Invoice.Html");

        // Sample data to inject
        private static Dictionary<string, string> values;

        private static string htmlContent;
        private static void LoadTheRequestDataToTheCollection(string requesterName, string requestType, string nationalNumber)
        {

            try
            {
                var base64Image = Convert.ToBase64String(File.ReadAllBytes(ImageURL));

                var imgTag = $"data:image/png;base64,{base64Image}";


                values = new Dictionary<string, string>
                {
                    ["{fullname}"] = requesterName,
                    ["{nationalId}"] = nationalNumber,
                    ["{requestType}"] = requestType,
                    ["{requestDate}"] = DateTime.Now.ToString("yyyy/MM/dd"),
                    ["{imageurl}"] = imgTag
                };
            }
            catch (Exception ex)
            {
            }
        }

        private static bool LoadDataToHtml()
        {
            if (File.Exists(TemplatePath))
            {
                htmlContent = File.ReadAllText(TemplatePath);
                foreach (var kvp in values)
                {
                    htmlContent = htmlContent.Replace(kvp.Key, kvp.Value);
                }

                return true;
            }

            return false;
        }


        private static ChromePdfRenderer RenderOptions()
        {
            return new ChromePdfRenderer
            {
                RenderingOptions = {
                  PaperSize = PdfPaperSize.A4,
                  MarginTop = 40,
                  MarginBottom = 40,
                  MarginLeft = 25,
                  MarginRight = 25
                }
            };

        }


        public static PdfDocument? GenerateInMemory(string requesterName, string requestType, string nationalNumber)
        {
            LoadTheRequestDataToTheCollection(requesterName, requestType, nationalNumber);

            if (!LoadDataToHtml())
            {
                return null;
            }

            var renderer = RenderOptions();

            return renderer.RenderHtmlAsPdf(htmlContent);
        }

        /// <summary>
        /// Generates a PDF as byte array in memory using PuppeteerSharp.
        /// </summary>
        public static async Task<byte[]?> GeneratePdfAsByteArrayAsync(string requesterName, string requestType, string nationalId)
        {
            LoadTheRequestDataToTheCollection(requesterName, requestType, nationalId);

            if (!LoadDataToHtml())
                return null;

         

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe" 
            });

            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(htmlContent);
            await page.EvaluateExpressionHandleAsync("document.fonts.ready");

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                
                Width = "210mm",
                Height = "297mm",
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0",
                    Bottom = "0",
                    Left = "0",
                    Right = "0"
                }
            });


            return pdfBytes;
        }
    }
}
