using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SubscriptionsProject.Models;
using SubscriptionsProject.Models.ApiModels;

namespace SubscriptionsProject.Common
{
	public class Helpers
	{
		SubscribeMembers_DBEntities db = new SubscribeMembers_DBEntities();

		#region UsersProcess

		public string hasSessionAuth(bool isAdmin = false)
		{
			if (isAdmin == true)
			{
				object isSessionAdmin = HttpContext.Current.Session[Consts.SessionControlAdminValue];

				return isSessionAdmin == null ? String.Empty : Convert.ToString(isSessionAdmin);
			}
			object isSession = HttpContext.Current.Session[Consts.SessionControlValue];

			return isSession == null ? String.Empty : Convert.ToString(isSession);
		}
		public User GetUserWithUserName(string username)
		{
			var getUser = db.Users.Where(x => x.Username == username).FirstOrDefault();
			return getUser;
		}

		public AdminUser GetAdminWithUserName(string username)
		{
			var getUser = db.AdminUsers.Where(x => x.UserName == username).FirstOrDefault();
			return getUser;
		}

		public AdminUser GetLoginAdmin(string username, string password)
		{
			var getUser = db.AdminUsers.Where(x => x.UserName == username && x.Password == password).FirstOrDefault();
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

		public string AddNewUser(User usr)
		{
			User newUser = new User();
			newUser.Name = usr.Name;
			newUser.Surname = usr.Surname;
			newUser.PhoneNumber = usr.PhoneNumber;
			newUser.IsActive = true;
			newUser.Username = usr.Username;
			newUser.Password = usr.Password;
			newUser.RegistrationDate = DateTime.Now;

			var subsCurrents = db.Subscriptions.Where(x => x.ID == usr.ID).First();
			newUser.ConditionalDate = DateTime.Now.AddDays(Convert.ToInt32(subsCurrents.SubscriptionDayCount));
			newUser.CurrentSubscriptionID = usr.ID;
			db.Users.Add(newUser);
			db.SaveChanges();

			decimal decimalValue = Convert.ToDecimal(subsCurrents.Price);
			double fiftyPercent = Convert.ToDouble(Convert.ToDouble(decimalValue) * 0.5);

			SubscriptionTransaction tran = new SubscriptionTransaction();
			tran.IsPaid = true;
			tran.UserID = db.Users.OrderByDescending(x => x.RegistrationDate).First().ID;
			tran.SubscriptionID = usr.ID;
			tran.TransactionDate = DateTime.Now;
			db.SubscriptionTransactions.Add(tran);
			db.SaveChanges();

			UserRegisterDepozit depozit = new UserRegisterDepozit();
			depozit.DepozitPrice = Convert.ToDecimal(fiftyPercent);
			depozit.UserID = db.Users.OrderByDescending(x => x.RegistrationDate).First().ID;
			db.UserRegisterDepozits.Add(depozit);
			db.SaveChanges();

			return fiftyPercent.ToString();
		}


		public async Task<string> AddNewUserApi(AddUserModel user)
		{
			var subsCurrents = await db.Subscriptions.Where(x => x.ID == user.CurrentSubscriptionID).FirstOrDefaultAsync();

			User usr = new User();
			usr.IsActive = true;
			usr.Name = user.Name;
			usr.Username = user.UserName;
			usr.Password = user.Password;
			usr.Surname = user.Surname;
			usr.PhoneNumber = user.PhoneNumber;
			usr.RegistrationDate = DateTime.Now;

			usr.ConditionalDate = DateTime.Now.AddDays(Convert.ToInt32(subsCurrents.SubscriptionDayCount));
			usr.CurrentSubscriptionID = user.CurrentSubscriptionID;

			db.Users.Add(usr);
			await db.SaveChangesAsync();

			decimal decimalValue = Convert.ToDecimal(subsCurrents.Price);
			double fiftyPercent = Convert.ToDouble(Convert.ToDouble(decimalValue) * 0.5);

			SubscriptionTransaction tran = new SubscriptionTransaction();
			tran.IsPaid = true;
			tran.UserID = db.Users.OrderByDescending(x => x.RegistrationDate).First().ID;
			tran.SubscriptionID = user.CurrentSubscriptionID;
			tran.TransactionDate = DateTime.Now;
			db.SubscriptionTransactions.Add(tran);
			await db.SaveChangesAsync();

			UserRegisterDepozit depozit = new UserRegisterDepozit();
			depozit.DepozitPrice = Convert.ToDecimal(fiftyPercent);
			depozit.UserID = db.Users.OrderByDescending(x => x.RegistrationDate).First().ID;
			db.UserRegisterDepozits.Add(depozit);
			await db.SaveChangesAsync();


			return fiftyPercent.ToString();
		}


		#endregion


	}
}