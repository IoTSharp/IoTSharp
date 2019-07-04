using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Releases
{
    public class LinkHeaderParser
    {
/*
          <https://api.github.com/search/code?q=addClass+user%3Amozilla&page=2>; rel="next",
          <https://api.github.com/search/code?q=addClass+user%3Amozilla&page=34>; rel="last"
*/

        public string GetNextPageFromHeader(string headerText)
        {
            if (string.IsNullOrWhiteSpace(headerText))
            {
                return string.Empty;
            }

            var links = headerText.Split(',');

            foreach (var link in links.Where(link => link.Contains("rel=\"next\"")))
            {
                return link.Split(';')[0].Replace("<","").Replace(">","").Trim();
            }

            return string.Empty;
        }
    }
}
