using SubscriptionsProject.Common;
using SubscriptionsProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SubscriptionsProject.Controllers
{
	public class AdminController : Controller
	{
		SubscribeMembers_DBEntities db = new SubscribeMembers_DBEntities();
		public ActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public ActionResult Login(string username, string password)
		{
			Helpers helper = new Helpers();

			var getUser = helper.GetLoginAdmin(username, password);
			if (getUser == null)
			{
				return RedirectToAction("Index");
			}

			Session[Consts.SessionControlAdminValue] = getUser.UserName;
			return RedirectToAction("AdminPanel");

		}

		public ActionResult AdminPanel()
		{
			Helpers helper = new Helpers();
			string sessionValue = helper.hasSessionAuth(true);

			if (String.IsNullOrEmpty(sessionValue) == true)
			{
				return RedirectToAction("Index");
			}

			AdminPanelModel panel = new AdminPanelModel();

			var getUser = helper.GetAdminWithUserName(sessionValue);
			panel.AdminItem = getUser;
			panel.CampaignCount = db.Subscriptions.Where(x => x.IsActive == true).Count();
			panel.UserCount = db.Users.Count();
			panel.ProcessCount = db.SubscriptionTransactions.Count();
			panel.NotPaidCount = db.SubscriptionTransactions.Where(x => x.IsPaid == false).Count();

			return View(panel);
		}


		public ActionResult Users()
		{
			Helpers helper = new Helpers();
			string sessionValue = helper.hasSessionAuth(true);

			if (String.IsNullOrEmpty(sessionValue) == true)
			{
				return RedirectToAction("Index");
			}
			AdminPanelUsersModel userPanel = new AdminPanelUsersModel();
			var getAllUsers = db.Users.OrderByDescending(x => x.RegistrationDate).ToList();
			userPanel.Users = getAllUsers;
			userPanel.Subscriptions = db.Subscriptions.ToList();


			return View(userPanel);
		}

		[HttpPost]
		public JsonResult AddNewUser(User usr)
		{
			if (String.IsNullOrEmpty(usr.Name) == true || String.IsNullOrEmpty(usr.Surname) == true || String.IsNullOrEmpty(usr.Password) == true || String.IsNullOrEmpty(usr.PhoneNumber) == true || String.IsNullOrEmpty(usr.Username) == true)
			{
				return Json(new { success = false, message = "Gerekli alanları boş bırakmayınız" });
			}
			else
			{
				if (db.Users.Where(x => x.Username == usr.Username).Count() > 0)
				{
					return Json(new { success = false, message = "Böyle bir kullanıcı adı ile kayıt zaten var" });
				}

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

				return Json(new { success = true, message = "Kullanıcı Eklendi. Lütfen  " + fiftyPercent + "₺ depozito tutarını almayı unutmayınız..." });
			}
		}




		public ActionResult PaymentProcess()
		{
			Helpers helper = new Helpers();
			string sessionValue = helper.hasSessionAuth(true);

			if (String.IsNullOrEmpty(sessionValue) == true)
			{
				return RedirectToAction("Index");
			}

			var transactions = db.SubscriptionTransactions.OrderByDescending(x => x.TransactionDate).ToList();
			return View(transactions);
		}


		[HttpPost]
		public JsonResult FreezeMember(int memberId)
		{
			var getUser = db.Users.Where(x => x.ID == memberId).First();

			if (db.SubscriptionTransactions.Where(x=> x.UserID == memberId && x.IsPaid == false).Count() > 0)
			{
				return Json(new { success = false, message = "Henüz bu kullanıcının ödenmemiş faturaları mevcut, üye dondurulamaz, önce ödeme işlemlerini yapınız" });
			}

			getUser.IsActive = false;
			db.SaveChanges();


			var getDepozit = db.UserRegisterDepozits.Where(x => x.UserID == memberId).FirstOrDefault();

			if (getDepozit == null)
			{
				return Json(new { success = true, message = "İşlem başarıyla yapıldı" });
			}
			else
			{
				return Json(new { success = true, message = "İşlem başarıyla yapıldı lütfen son olarak kişiye  " + getDepozit.DepozitPrice + " ₺ depozitosunu ödeyiniz." });
			}

		}

	}
}