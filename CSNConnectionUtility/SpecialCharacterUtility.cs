using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSNConnectionUtility
{
    public class SpecialCharacterUtility
    {
        public static bool validateSpecialCharecter(string stringToCheckSpecialCharecter, string charecterToValidate)
        {
            var regx = new Regex(charecterToValidate);
            if (regx.IsMatch(stringToCheckSpecialCharecter))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
