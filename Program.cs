// See https://aka.ms/new-console-template for more information

// PM> Install-Package IronOcr
using System;
using System.Drawing;
using System.IO;
using Tesseract;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using DynamicTesseract;
using System.Reflection.PortableExecutable;

namespace OCRConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("C:\Users\utente\Desktop\scanner\PDF-1.pdf");
            string filePath = Console.ReadLine();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            string extractedText;
            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".pdf")
            {
                extractedText = ProcessPdf(filePath);
            }
            else if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp" || extension == ".tiff")
            {
                extractedText = ProcessImage(filePath);
            }
            else
            {
                Console.WriteLine("Unsupported file format.");
                return;
            }

            Console.WriteLine("Extracted Text:");
            Console.WriteLine(extractedText);
        }

        private static string ProcessImage(string imagePath)
        {
            using var engine = new TesseractEngine(@"C:\Users\utente\Downloads\tesseract-ocr-tesseract-e3f272b\tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            return page.GetText();
        }

        private static string ProcessPdf(string pdfPath)
        {
            using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
            string extractedText = "";

            for (int i = 0; i < document.PageCount; i++)
            {
                using var page = document.Pages[i];
                using var image = ConvertPdfPageToImage(page);

                if (image != null)
                {
                    extractedText += ProcessImageFromMemory(image);
                }
            }

            return extractedText;
        }

        private static Image ConvertPdfPageToImage(PdfPage page)
        {
            int dpi = 300; // Set DPI for better accuracy
            var width = (int)(page.Width.Point * dpi / 72);
            var height = (int)(page.Height.Point * dpi / 72);

            using var xImage = new XImageRenderer(page);
            var bitmap = new Bitmap(width, height);

            using (var gfx = Graphics.FromImage(bitmap))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(xImage, 0, 0, width, height);
            }

            return bitmap;
        }

        private static string ProcessImageFromMemory(Image image)
        {
            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromMemory(ms.ToArray());
            using var page = engine.Process(img);
            return page.GetText();
        }
    }
}

