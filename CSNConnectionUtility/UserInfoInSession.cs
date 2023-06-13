using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSNConnectionUtility
{
    public class UserInfoInSession
    {
        public ActiveUserInformation GetUserInfoInSession(string userName)
        {
            DataTable dtUserProfile = null;
            DataBaseUtility dataBaseUtility = new DataBaseUtility();
            ActiveUserInformation activeUserInformation = null;
            Queue paramName = new Queue();
            paramName.Enqueue(new SqlParameter("@UserName", userName.Trim()));
            //paramName.Enqueue(new SqlParameter("@UserRole", userRole.Trim()));
            //paramName.Enqueue(new SqlParameter("@UserPassword", password.Trim()));
            dtUserProfile = dataBaseUtility.GetDataSet("sp_GetUserProfile", "GetUserProfile", paramName);
            if (dtUserProfile != null)
            {
                if (dtUserProfile.Rows.Count > 0)
                {
                    activeUserInformation = new ActiveUserInformation();
                    activeUserInformation.UserEmailAddress = dtUserProfile.Rows[0]["EmailAddress"].ToString();
                    activeUserInformation.UserName = dtUserProfile.Rows[0]["UserName"].ToString();
                    activeUserInformation.UserRole = dtUserProfile.Rows[0]["RoleID"].ToString();
                    activeUserInformation.UserPassword = dtUserProfile.Rows[0]["Password"].ToString();
                    activeUserInformation.UserCollegeCode = dtUserProfile.Rows[0]["CollegeCode"].ToString();
                }
            }
            return activeUserInformation;
        }

        public bool DeActivateUser(string userName, string userRole)
        {
            DataBaseUtility dataBaseUtility = new DataBaseUtility();
            Queue paramName = new Queue();
            paramName.Enqueue(new SqlParameter("@UserName", userName.Trim()));
            paramName.Enqueue(new SqlParameter("@UserRole", userRole.Trim()));
            if (dataBaseUtility.UserSession("sp_DeActivateUser", paramName) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    
}
