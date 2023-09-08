using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.IO;

namespace BBBPresentationParser.PresentationUtils
{
    internal class Presentation
    {
        private List<byte[]> _slides = new();

        public byte[][] Slides => _slides.ToArray();

        public void AddSlide(byte[] data) => _slides.Add(data);

        public void AddSlides(IEnumerable<byte[]> dataList) => _slides.AddRange(dataList);

        public string? SavePdf(string path)
        {
            if (_slides.Count == 0)
                return null;

            var im = Image.GetInstance(_slides[0]);

            using var doc = new Document(new Rectangle(im));

            PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));
            doc.Open();

            foreach (byte[] data in _slides)
            {
                Image i = Image.GetInstance(data);
                doc.SetPageSize(new Rectangle(i));

                i.Alignment = Element.ALIGN_CENTER;
                i.ScaleToFit(doc.PageSize.Width, doc.PageSize.Height);

                doc.Add(i);

                doc.NewPage();
            }

            return path;
        }
    }
}
