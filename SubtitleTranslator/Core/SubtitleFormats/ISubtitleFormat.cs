using SubtitleTranslator.Core.Common;

namespace SubtitleTranslator.Core.SubtitleFormats
{
    public interface ISubtitleFormat
    {
        string Name { get; }
        string Extension { get; }
        bool IsMine(List<string> lines, string fileName);
        void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);
        string ToText(Subtitle subtitle, string title);
    }
}