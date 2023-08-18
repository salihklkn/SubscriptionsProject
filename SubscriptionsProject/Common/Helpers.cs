using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SubscriptionsProject.Models;
namespace SubscriptionsProject.Common
{
	public class Helpers
	{
		SubscribeMembers_DBEntities db = new SubscribeMembers_DBEntities();

		#region UsersProcess

		public string hasSessionAuth()
		{
			object isSession = HttpContext.Current.Session[Consts.SessionControlValue];
			
			return isSession == null ? String.Empty : Convert.ToString(isSession);
		}
		public User GetUserWithUserName(string username)
		{
			var getUser = db.Users.Where(x => x.Username == username).FirstOrDefault();
			return getUser;
		}

		public User GetLoginUser(string username, string password)
		{
			var getUser = db.Users.Where(x => x.Username == username && x.Password == password).FirstOrDefault();
			return getUser;
		}

		public void UpdateUser(User usr)
		{
			var getUser = db.Users.Where(x => x.Username == usr.Username).FirstOrDefault();
			getUser.Name = usr.Name;
			getUser.Surname = usr.Surname;
			getUser.Password = usr.Password;
			getUser.PhoneNumber = usr.PhoneNumber;
			db.SaveChanges();
		}


		#endregion


	}
}