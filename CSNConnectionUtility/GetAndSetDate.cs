using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSNConnectionUtility
{
    public class GetAndSetDate
    {
        /// <summary>
        /// Get date in YYYYMMDD formate
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetDateInStringFormate(string date)
        {
            string[] userDobCollection = null;
            string userDob = "";
            if (date != "")
            {
                userDobCollection = date.Split('/');
                for(int i = userDobCollection.Length - 1; i > -1; i--)
                {
                    userDob = userDob + userDobCollection[i];
                }
            }
            return userDob;
        }
        /// <summary>
        /// Get date in DD/MM/YYYY formate
        /// Pass the input parameter in YYYYMMDD formate.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetDateInDateFormate(string dateInYYYYMMDD)
        {
            char[] userDobCollection = null;
            string userDob = "";
            if (dateInYYYYMMDD != "" && dateInYYYYMMDD.Length >= 8)
            {
                userDobCollection = dateInYYYYMMDD.ToCharArray();
                userDob = userDobCollection[6].ToString() + userDobCollection[7].ToString();
                userDob = userDob + "/" + userDobCollection[4].ToString() + userDobCollection[5].ToString();
                userDob = userDob + "/" + userDobCollection[0].ToString() + userDobCollection[1].ToString() + userDobCollection[2].ToString() + userDobCollection[3].ToString();
            }
            return userDob;
        }
    }
}
