using SubscriptionsProject.Common;
using SubscriptionsProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace SubscriptionsProject.Controllers
{
	public class HomeController : Controller
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

			var getUser = helper.GetLoginUser(username, password);
			if (getUser == null)
			{
				return RedirectToAction("Index");
			}

			Session[Consts.SessionControlValue] = getUser.Username;
			return RedirectToAction("UserPanel");

		}
		public ActionResult UserPanel()
		{
			Helpers helper = new Helpers();
			string sessionValue = helper.hasSessionAuth();

			if (String.IsNullOrEmpty(sessionValue) == true)
			{
				return RedirectToAction("Index");
			}

			UserPanelModel panel = new UserPanelModel();

			var getUser = helper.GetUserWithUserName(sessionValue);
			panel.UserItem = getUser;
			panel.CampaignCount = db.Subscriptions.Count();
			panel.UserCount = db.Users.Count();
			panel.ProcessCount = db.SubscriptionTransactions.Where(x => x.UserID == getUser.ID).Count();

			if (getUser.ConditionalDate != null)
			{
				DateTime startDate = DateTime.Now;
				DateTime endDate = Convert.ToDateTime(getUser.ConditionalDate);
				TimeSpan duration = endDate - startDate;
				panel.CurrentDate = duration.Days;
			}



			return View(panel);
		}

		public ActionResult UserInformation()
		{
			Helpers helper = new Helpers();
			string sessionValue = helper.hasSessionAuth();

			if (String.IsNullOrEmpty(sessionValue) == true)
			{
				return RedirectToAction("Index");
			}
			var getUser = helper.GetUserWithUserName(sessionValue);

			UserInformationModel information = new UserInformationModel();
			information.Subscription = db.Subscriptions.ToList();
			information.UserItem = getUser;
			return View(information);
		}

		[HttpPost]
		public JsonResult UpdateUserInformation(User usr)
		{
			if (String.IsNullOrEmpty(usr.Name) == true || String.IsNullOrEmpty(usr.Surname) == true || String.IsNullOrEmpty(usr.Password) == true)
			{
				return Json(new { success = false, message = "Gerekli alanları boş bırakmayınız" });
			}
			else
			{
				Helpers helper = new Helpers();
				helper.UpdateUser(usr);

				return Json(new { success = true, message = "Bilgileriniz güncellendi." });
			}
		}

		[HttpPost]
		public JsonResult UpdateUserSubscription(string subsId)
		{
			try
			{
				Helpers helper = new Helpers();
				string sessionValue = helper.hasSessionAuth();
				var getUser = helper.GetUserWithUserName(sessionValue);

				if (getUser.CurrentSubscriptionID == Convert.ToInt32(subsId))
				{
					return Json(new { success = false, message = "Bu abonelik kampanyasına zaten üyesiniz." });
				}

				var getSubsInfo = db.Subscriptions.Where(x => x.ID == Convert.ToInt32(subsId)).First();

				SubscriptionTransaction sbsTrancastion = new SubscriptionTransaction();
				sbsTrancastion.SubscriptionID = Convert.ToInt32(subsId);
				sbsTrancastion.UserID = getUser.ID;
				sbsTrancastion.TransactionDate = DateTime.Now;
				db.SubscriptionTransactions.Add(sbsTrancastion);
				db.SaveChanges();




				var updateUserCurrents = db.Users.Where(x => x.ID == getUser.ID).First();
				updateUserCurrents.ConditionalDate = DateTime.Now.AddDays(Convert.ToInt32(getSubsInfo.SubscriptionDayCount));
				updateUserCurrents.CurrentSubscriptionID = Convert.ToInt32(subsId);
				db.SaveChanges();

				return Json(new { success = false, message = "Bu abonelik kampanyasına zaten üyesiniz." });
			}
			catch (Exception)
			{
				return Json(new { success = false, message = "Bir hata oluştu" });
			}


		}


	}
}