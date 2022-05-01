using Eternity.Forms;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Eternity.Utils.Filters
{
    public class ProfanityFilter
    {
        static string GenerateStars(int count)
        {
            return new string('*', count);
        }
        static string GenerateText(Match match)
        {
            return 
                string.Join(string.Empty, match.Value.Split(
                    new[] { ' ' }).Select(word => word.First())) + GenerateStars(match.Value.Length);
        }

        public static string Replace(string message)
        {
            var regex = new Regex(RandomText());
            return regex.Replace(message, GenerateText);
        }

        public static string RandomText()
        {
            var filters = File.ReadLines("Files\\filters.txt");
            var text = string.Empty;
            foreach (var item in filters)
                text += item + "|";

            return text + text.ToUpper().Remove(text.Length - 1);
        }
    }
}
