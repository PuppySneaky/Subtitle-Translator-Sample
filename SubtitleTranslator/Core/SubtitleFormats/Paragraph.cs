using SubtitleTranslator.Core.Common;

namespace SubtitleTranslator.Core.SubtitleFormats
{
    public class Paragraph
    {
        public int Number { get; set; }
        public TimeCode StartTime { get; set; }
        public TimeCode EndTime { get; set; }
        public string Text { get; set; }
        public string Id { get; set; }

        public Paragraph()
        {
            StartTime = new TimeCode();
            EndTime = new TimeCode();
            Text = string.Empty;
            Id = string.Empty;
        }

        public Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            StartTime = startTime;
            EndTime = endTime;
            Text = text ?? string.Empty;
            Id = string.Empty;
        }

        public double Duration => EndTime.TotalMilliseconds - StartTime.TotalMilliseconds;

        public override string ToString()
        {
            return StartTime + " --> " + EndTime + " " + Text;
        }

        public string ToSrt()
        {
            return $"{Number}\r\n{StartTime.ToSrtTime()} --> {EndTime.ToSrtTime()}\r\n{Text}\r\n";
        }
    }
}