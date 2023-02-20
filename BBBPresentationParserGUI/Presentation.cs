using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BBBPresentationParser
{
    internal class Presentation
    {
        private List<string> _slides;
        private DateTime _createTime;
        
        public Presentation()
        {
            _slides = new List<string>();
            _createTime = DateTime.Now;
        }

        public string[] Slides => _slides.ToArray();

        public void AddSlide(string path) => _slides.Add(path);

        public void AddSlides(string[] paths) => _slides.AddRange(paths);

        public void Save(string path)
        {
            string name = Path.GetFileName(path);

            string? doc = SavePdf(DateTime.Now.Ticks.ToString());
            List<byte> data = new List<byte>();
            data.AddRange(GetCreateTimeData());
            List<byte> nameData = Encoding.Unicode.GetBytes(name).ToList();
            while(nameData.Count < 512)
                nameData.Add(0);

            data.AddRange(nameData);

            var ver = Assembly.GetExecutingAssembly()?.GetName()?.Version;
            if(ver != null)
                data.AddRange(new byte[] { (byte)ver.Major, (byte)ver.Minor, (byte)ver.Build });
            else
                data.AddRange(new byte[] { 0, 0, 0 });

            if(doc != null)
                data.AddRange(File.ReadAllBytes(doc));

            File.WriteAllBytes(path, data.ToArray());
        }

        // https://awebinar.bsu.edu.ru/bigbluebutton/presentation/458094f2fbba4eb1be4085186bd460241919222f-1647243418872/458094f2fbba4eb1be4085186bd460241919222f-1647243418872/f51a2bf320654786b0bc6d68c70388f2197b79b4-1647243691459/svg/37
        // https://nwebinar.bsu.edu.ru/bigbluebutton/presentation/11e85a659c09a862d9cb0997fa4c5b8e36841d01-1647240583558/11e85a659c09a862d9cb0997fa4c5b8e36841d01-1647240583558/f51a2bf320654786b0bc6d68c70388f2197b79b4-1647242094893/svg/35

        public byte[] GetCreateTimeData()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)_createTime.Day);
            data.Add((byte)_createTime.Month);
            data.AddRange(BitConverter.GetBytes(_createTime.Year));
            data.Add((byte)_createTime.Hour);
            data.Add((byte)_createTime.Minute);

            return data.ToArray();
        }

        public void SetCreateTimeByData(byte[] data)
        {
            int day, month, year, hour, minute;
            day = data[0];
            month = data[1];
            year = BitConverter.ToInt32(data.Skip(2).Take(4).ToArray());
            hour = data[6];
            minute = data[7];

            _createTime = new DateTime(year, month, day, hour, minute, 0);
        }

        public string? SavePdf(string path)
        {
            // return @"C:\Users\artem\Desktop\Резюме.pdf";
            if (_slides == null)
                return null;

            var im = Image.GetInstance(_slides[0]);

            var doc = new Document(new Rectangle(im));
            string filename = $"Презентация {DateTime.Now:dd.MM.yy HH mm}.pdf";
            filename = path;

            PdfWriter.GetInstance(doc, new FileStream(filename, FileMode.Create));
            doc.Open();

            foreach (string img in _slides)
            {
                Image i = Image.GetInstance(img);
                doc.SetPageSize(new Rectangle(i));
                i.Alignment = Element.ALIGN_CENTER;
                i.ScaleToFit(doc.PageSize.Width, doc.PageSize.Height);
                doc.Add(i);
                doc.NewPage();
                File.Delete(img);
            }
            doc.Close();
            return filename;
        }
    }

    internal class PresentationFile
    {
        private string _path = string.Empty;

        public PresentationFile(string path)
        {
            _path = path;
        }

        public Presentation? Read()
        {
            Presentation presentation = new Presentation();

            byte[] data = File.ReadAllBytes(_path);
            presentation.SetCreateTimeByData(data.Take(8).ToArray());

            string name = Path.GetFileName(_path);
            string? doc = presentation.SavePdf(DateTime.Now.Ticks.ToString());

            using (FileStream file = new FileStream(_path, FileMode.CreateNew))
            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.Write(presentation.GetCreateTimeData());
                List<byte> nameData = Encoding.Unicode.GetBytes(name).ToList();
                while (nameData.Count < 512)
                    nameData.Add(0);

                writer.Write(nameData);

                var ver = Assembly.GetExecutingAssembly()?.GetName()?.Version;

                if (ver != null)
                    writer.Write(new byte[] { (byte)ver.Major, (byte)ver.Minor, (byte)ver.Build });
                else
                    writer.Write(new byte[] { 0, 0, 0 });

                if (doc != null)
                    writer.Write(File.ReadAllBytes(doc));
            }
        }

        public void Write(Presentation presentation)
        {
            string name = Path.GetFileName(_path);
            string? doc = presentation.SavePdf(DateTime.Now.Ticks.ToString());
            
            using (FileStream file = new FileStream(_path, FileMode.CreateNew))
            using(StreamWriter writer = new StreamWriter(file))
            {
                writer.Write(presentation.GetCreateTimeData());
                List<byte> nameData = Encoding.Unicode.GetBytes(name).ToList();
                while (nameData.Count < 512)
                    nameData.Add(0);

                writer.Write(nameData);

                var ver = Assembly.GetExecutingAssembly()?.GetName()?.Version;

                if (ver != null)
                    writer.Write(new byte[] { (byte)ver.Major, (byte)ver.Minor, (byte)ver.Build });
                else
                    writer.Write(new byte[] { 0, 0, 0 });

                if (doc != null)
                    writer.Write(File.ReadAllBytes(doc));
            }
        }
    }
}
