using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain
{
    public static class StringExtensions
    {
        public readonly static string[] doubleUpperCase = new string[] { "mc", "mac", "o'", "ver" };
        public readonly static string[] noUpperCase = new string[] { "and", "to", "from", "the", "a", "is", "an" };
        public readonly static string[] allUpperCase = new string[] { "ceo", "vp" };

        public static string ToTitleCase(this string thisString)
        {
            var words = thisString.ToLower().Split(' ');
            var formattedWords = new List<string>();
            foreach (var word in words)
            {
                if (String.IsNullOrWhiteSpace(word))
                    continue;
                var newWord = word;
                if (noUpperCase.Contains(newWord))
                {
                    formattedWords.Add(newWord);
                    continue;
                }
                if (allUpperCase.Contains(newWord))
                {
                    formattedWords.Add(newWord.ToUpper());
                    continue;
                }
                foreach (var dc in doubleUpperCase)
                {
                    if (newWord.StartsWith(dc))
                    {
                        var ind = word.IndexOf(dc);
                        if (newWord.Length > dc.Length)
                            newWord = newWord.Substring(0, ind + dc.Length) + char.ToUpper(newWord[ind + dc.Length]) + newWord.Substring(ind + dc.Length + 1);
                    }
                }
                newWord = char.ToUpper(newWord[0]) + newWord.Substring(1);
                formattedWords.Add(newWord);
            }
            return String.Join(" ", formattedWords);
        }
    }
}
