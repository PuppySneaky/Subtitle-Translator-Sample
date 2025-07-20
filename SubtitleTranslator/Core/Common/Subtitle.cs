using SubtitleTranslator.Core.SubtitleFormats;
using System.Text;

namespace SubtitleTranslator.Core.Common
{
    public class Subtitle
    {
        public List<Paragraph> Paragraphs { get; set; }
        public string FileName { get; set; }
        public string OriginalFormat { get; set; }

        public Subtitle()
        {
            Paragraphs = new List<Paragraph>();
            FileName = string.Empty;
            OriginalFormat = string.Empty;
        }

        public static Subtitle Parse(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            var lines = File.ReadAllLines(fileName, Encoding.UTF8).ToList();
            return Parse(lines, Path.GetExtension(fileName));
        }

        public static Subtitle Parse(List<string> lines, string fileExtension)
        {
            var subtitle = new Subtitle();
            var ext = fileExtension.ToLowerInvariant();

            if (ext == ".srt")
            {
                var format = new SubRipFormat();
                format.LoadSubtitle(subtitle, lines, string.Empty);
                subtitle.OriginalFormat = "SubRip";
            }

            return subtitle;
        }

        public static Subtitle ParseFromText(string content, string fileExtension)
        {
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
            return Parse(lines, fileExtension);
        }

        public string ToSrt()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Paragraphs.Count; i++)
            {
                var p = Paragraphs[i];
                p.Number = i + 1;
                sb.AppendLine(p.Number.ToString());
                sb.AppendLine($"{p.StartTime.ToSrtTime()} --> {p.EndTime.ToSrtTime()}");
                sb.AppendLine(p.Text);
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        public void Renumber()
        {
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                Paragraphs[i].Number = i + 1;
            }
        }

        public int Count => Paragraphs.Count;

        public void Clear()
        {
            Paragraphs.Clear();
        }

        public void Add(Paragraph paragraph)
        {
            Paragraphs.Add(paragraph);
        }
    }
}