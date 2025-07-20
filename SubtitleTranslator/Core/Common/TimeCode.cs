using System.Globalization;

namespace SubtitleTranslator.Core.SubtitleFormats
{
    public class TimeCode
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Milliseconds { get; set; }

        public TimeCode()
        {
        }

        public TimeCode(double totalMilliseconds)
        {
            TotalMilliseconds = totalMilliseconds;
        }

        public TimeCode(int hours, int minutes, int seconds, int milliseconds)
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Milliseconds = milliseconds;
        }

        public double TotalMilliseconds
        {
            get
            {
                return Hours * 60 * 60 * 1000 + Minutes * 60 * 1000 + Seconds * 1000 + Milliseconds;
            }
            set
            {
                var ts = TimeSpan.FromMilliseconds(value);
                Hours = ts.Hours;
                Minutes = ts.Minutes;
                Seconds = ts.Seconds;
                Milliseconds = ts.Milliseconds;
            }
        }

        public double TotalSeconds => TotalMilliseconds / 1000.0;

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}", Hours, Minutes, Seconds, Milliseconds);
        }

        public string ToSrtTime()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00},{3:000}", Hours, Minutes, Seconds, Milliseconds);
        }

        public static TimeCode FromString(string timeString)
        {
            var tc = new TimeCode();

            if (string.IsNullOrEmpty(timeString))
                return tc;

            // Handle SRT format (HH:MM:SS,mmm)
            timeString = timeString.Replace(',', '.');

            var parts = timeString.Split(':');
            if (parts.Length == 3)
            {
                if (int.TryParse(parts[0], out int hours))
                    tc.Hours = hours;

                if (int.TryParse(parts[1], out int minutes))
                    tc.Minutes = minutes;

                var secondsParts = parts[2].Split('.');
                if (secondsParts.Length == 2)
                {
                    if (int.TryParse(secondsParts[0], out int seconds))
                        tc.Seconds = seconds;

                    if (int.TryParse(secondsParts[1], out int milliseconds))
                        tc.Milliseconds = milliseconds;
                }
                else if (int.TryParse(parts[2], out int seconds2))
                {
                    tc.Seconds = seconds2;
                }
            }

            return tc;
        }

        public static bool TryParse(string timeString, out TimeCode timeCode)
        {
            timeCode = new TimeCode();

            try
            {
                timeCode = FromString(timeString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}