using SubtitleTranslator.Core.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleTranslator.Core.SubtitleFormats
{
    public class SubRipFormat : ISubtitleFormat
    {
        private static readonly Regex TimeCodeRegex = new Regex(
            @"^(\d+):(\d+):(\d+)[:,](\d+)\s*-->\s*(\d+):(\d+):(\d+)[:,](\d+)",
            RegexOptions.Compiled);

        public string Name => "SubRip";
        public string Extension => ".srt";

        public bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0].StartsWith("WEBVTT", StringComparison.OrdinalIgnoreCase))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > 0;
        }

        public void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();

            var expecting = ExpectingLine.Number;
            var paragraph = new Paragraph();

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();

                if (string.IsNullOrEmpty(line))
                {
                    if (expecting == ExpectingLine.Text && !string.IsNullOrEmpty(paragraph.Text))
                    {
                        subtitle.Paragraphs.Add(paragraph);
                        paragraph = new Paragraph();
                        expecting = ExpectingLine.Number;
                    }
                    continue;
                }

                switch (expecting)
                {
                    case ExpectingLine.Number:
                        if (int.TryParse(line, out int number))
                        {
                            paragraph.Number = number;
                            expecting = ExpectingLine.TimeCodes;
                        }
                        break;

                    case ExpectingLine.TimeCodes:
                        var match = TimeCodeRegex.Match(line);
                        if (match.Success)
                        {
                            paragraph.StartTime = new TimeCode(
                                int.Parse(match.Groups[1].Value),
                                int.Parse(match.Groups[2].Value),
                                int.Parse(match.Groups[3].Value),
                                int.Parse(match.Groups[4].Value));

                            paragraph.EndTime = new TimeCode(
                                int.Parse(match.Groups[5].Value),
                                int.Parse(match.Groups[6].Value),
                                int.Parse(match.Groups[7].Value),
                                int.Parse(match.Groups[8].Value));

                            expecting = ExpectingLine.Text;
                        }
                        break;

                    case ExpectingLine.Text:
                        if (string.IsNullOrEmpty(paragraph.Text))
                            paragraph.Text = line;
                        else
                            paragraph.Text += Environment.NewLine + line;
                        break;
                }
            }

            // Add the last paragraph if it has content
            if (!string.IsNullOrEmpty(paragraph.Text))
            {
                subtitle.Paragraphs.Add(paragraph);
            }
        }

        public string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();

            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(p.Number.ToString());
                sb.AppendLine($"{p.StartTime.ToSrtTime()} --> {p.EndTime.ToSrtTime()}");
                sb.AppendLine(p.Text);
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private enum ExpectingLine
        {
            Number,
            TimeCodes,
            Text
        }
    }
}