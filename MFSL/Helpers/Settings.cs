
// Helpers/Settings.cs This file was automatically added when you installed the Settings Plugin. If you are not using a PCL then comment this file back in to use it.
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;

namespace MFSL.Helpers
{
	/// <summary>
	/// This is the Settings static class that can be used in your Core solution or in any
	/// of your client applications. All settings are laid out the same exact way with getters
	/// and setters. 
	/// </summary>
	public static class Settings
	{
		private static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}

        public static string Username
        {
            get
            {
                return AppSettings.GetValueOrDefault("Username", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("Username", value);
            }
        }
        public static string Password
        {
            get
            {
                return AppSettings.GetValueOrDefault("Password", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("Password", value);
            }
        }

        public static string AccessToken
        {
            get
            {
                return AppSettings.GetValueOrDefault("AccessToken", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("AccessToken", value);
            }
        }

        public static DateTime AccessTokenExpirationDate
        {
            get
            {
                return AppSettings.GetValueOrDefault
                    ("AccessTokenExpirationDate", DateTime.UtcNow);
            }
            set
            {
                AppSettings.AddOrUpdateValue("AccessTokenExpirationDate", value);
            }
        }

        public static int TokenFlag
        {
            get
            {
                return AppSettings.GetValueOrDefault("TokenFlag", 0);
            }
            set
            {
                AppSettings.AddOrUpdateValue("TokenFlag", value);
            }
        }

        public static string RoleForThisUser
        {
            get
            {
                return AppSettings.GetValueOrDefault("RoleForThisUser", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("RoleForThisUser", value);
            }
        }

        public static string UserFirstName
        {
            get
            {
                return AppSettings.GetValueOrDefault("UserFirstName", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("UserFirstName", value);
            }
        }
        public static string UserMidName
        {
            get
            {
                return AppSettings.GetValueOrDefault("UserMidName", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("UserMidName", value);
            }
        }
        public static string UserLastName
        {
            get
            {
                return AppSettings.GetValueOrDefault("UserLastName", "");
            }
            set
            {
                AppSettings.AddOrUpdateValue("UserLastName", value);
            }
        }

        public static int VNPFNo
        {
            get
            {
                return AppSettings.GetValueOrDefault("VNPFNo", 0);
            }
            set
            {
                AppSettings.AddOrUpdateValue("VNPFNo", value);
            }
        }

        public static int LoanNo
        {
            get
            {
                return AppSettings.GetValueOrDefault("LoanNo", 0);
            }
            set
            {
                AppSettings.AddOrUpdateValue("LoanNo", value);
            }
        }

        public static DateTime DateRegistered
        {
            get
            {
                return AppSettings.GetValueOrDefault("DateRegistered", DateTime.Now);
            }
            set
            {
                AppSettings.AddOrUpdateValue("DateRegistered", value);
            }
        }

    }
}