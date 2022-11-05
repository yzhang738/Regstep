using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using RSToolKit.Domain.Data;

//COMPLETE
namespace RSToolKit.Domain.Entities
{
    [Table("CSS")]
    public class CSS : IRSData
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SortingId { get; set; }

        [Key]
        public Guid UId { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateModified { get; set; }

        public Guid ModificationToken { get; set; }

        public Guid ModifiedBy { get; set; }

        private string _rawCss;

        public bool StandAlone { get; set; }
        public Guid? ComponentUId { get; set; }
        public Guid? StylesheetUId { get; set; }
        [NotMapped]
        public Stylesheet Stylesheet { get; set; }
        [MaxLength(250)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Class { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RawCss
        {
            get
            {
                return _rawCss;
            }
            set
            {
                _rawCss = value;
                InlineCss = _rawCss;
            }
        }
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string RawSelectors
        {
            get
            {
                string returnString = "";
                foreach (string selector in Selectors)
                {
                    returnString += @"!" + selector;
                }
                return returnString;
            }
            set
            {
                Regex rgxSelectors = new Regex(@"!([^!]*)");
                MatchCollection matches = rgxSelectors.Matches(value);
                foreach (Match match in matches)
                {
                    Selectors.Add(match.Groups[1].Value);
                }
            }
        }

        [NotMapped]
        protected List<string> Selectors { get; set; }

        [NotMapped]
        protected Dictionary<string, string> CssTags { get; set; }

        /// <summary>
        /// Gets and sets the string of the css for inline styling.
        /// </summary>
        [NotMapped]
        public string InlineCss
        {
            get
            {
                string css = "";
                foreach (KeyValuePair<string, string> kvp in CssTags)
                {
                    css += kvp.Key.ToLower() + ": " + kvp.Value + "; ";
                }
                return css;
            }
            set
            {
                CssTags.Clear();
                var rgxParseCss = new Regex(@" ?([^:]+): ?([^;]+);");
                var match = rgxParseCss.Match(value);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        CssTags.Add(match.Groups[1].Value, match.Groups[2].Value);
                        match = match.NextMatch();
                    }
                }
            }
        }

        [NotMapped]
        public string InlineTag
        {
            get
            {
                return " style=\"" + InlineCss + "\" ";
            }
        }

        [NotMapped]
        public string this[string tag]
        {
            get
            {
                tag = tag.ToLower();
                if (!CssTags.ContainsKey(tag))
                {
                    return "";
                }
                else
                {
                    return CssTags[tag];
                }
            }
            set
            {
                tag = tag.ToLower();
                CssTags[tag] = value;
            }
        }

        public CSS() : base()
        {
            UId = Guid.NewGuid();
            StylesheetUId = null;
            ComponentUId = null;
            Selectors = new List<string>();
            CssTags = new Dictionary<string, string>();
            Class = "";
            RawCss = "";
            RawSelectors = "";
        }

        #region Public Methods

        /// <summary>
        /// Adds the desired selector from the list of selectors.
        /// </summary>
        /// <param name="selector">The selector that is to be added.</param>
        /// <returns>Returns a RSResult with the outcome of the operation.</returns>
        public Result AddSelector(string selector)
        {
            if (Selectors.Contains(selector)) return new Result() { Success = false, Message = "Selector already exists." };
            else Selectors.Add(selector);
            return new Result() { Success = true };
        }

        /// <summary>
        /// Removes the desired selector from the list of selectors.
        /// </summary>
        /// <param name="selector">The selector that is to be removed.</param>
        /// <returns>Returns a RSResult with the outcome of the operation.</returns>
        public Result RemoveSelector(string selector)
        {
            if (!CssTags.ContainsKey(selector.ToLower())) return new Result() { Success = false, Message = "Selector does not exists." };
            else
            {
                CssTags.Remove(selector.ToLower());
            }
            return new Result() { Success = true };
        }

        /// <summary>
        /// Retrieves the tag and value for the desired tag to be used in a style.
        /// </summary>
        /// <param name="tag">The tag of the css you are requesting.</param>
        /// <returns>The string representing the tag and value (tag: value; ). A space is included after the semicolon.</returns>
        public string GetTagAndValue(string tag)
        {
            if (!CssTags.ContainsKey(tag)) return "";
            else return tag + ": " + CssTags[tag] + "; ";
        }

        /// <summary>
        /// Prepares the list of selectors for storing it in the database. 
        /// </summary>
        /// <returns>Returns a string that represents all the selectors.</returns>
        protected void _PrepareSelectors()
        {
            string returnString = "";
            foreach (string selector in Selectors)
            {
                returnString += @"!" + selector;
            }
            RawSelectors = returnString;
        }

        /// <summary>
        /// Returns a string representing the Css class as a css style.
        /// It will format it with spaces in the front and end of the string.
        /// For standalone css (inline css) it will give the style tag for use in HTML 5 elements.
        /// For stylesheet css it will give the selectors and the css styles inside braces.
        /// </summary>
        /// <returns>The string that represents the Css.</returns>
        public override string ToString()
        {
            if (StandAlone)
            {
                return " style=\"" + InlineCss + "\" ";
            }
            else
            {
                return _SelectorsString() + " { " + InlineCss + " } ";
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets a list of the selectors seperated by a comma and white space with no leading or trailing spaces.
        /// </summary>
        /// <returns>The string that represents the selectors.</returns>
        protected string _SelectorsString()
        {
            string retString = "";
            foreach (string selector in Selectors)
            {
                retString += selector + ", ";
            }
            retString = retString.Substring(0, retString.Length - 2);
            return retString;
        }


        #endregion

        #region Static Methods

        public static IDictionary<string, string> ParseCss(string str)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            var rgxParseCss = new Regex(@" ?([^:]+): ?([^;]+);");
            var match = rgxParseCss.Match(str);
            if (match.Success)
            {
                while (match.Success)
                {
                    dic.Add(match.Groups[1].Value, match.Groups[2].Value);
                    match = match.NextMatch();
                }
            }
            return dic;
        }

        public static string ParseCss(IDictionary<string, string> dic)
        {
            string css = "";
            foreach (KeyValuePair<string, string> kvp in dic)
            {
                css += kvp.Key.ToLower() + ": " + kvp.Value + "; ";
            }
            return css;
        }

        #endregion

    }
}
