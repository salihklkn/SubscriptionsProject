using SubscriptionsProject.Common;
using SubscriptionsProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
			panel.CampaignCount = db.Subscriptions.Where(x=> x.IsActive == true).Count();
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
		public async Task<JsonResult> UpdateUserSubscription(string subsId)
		{
			try
			{
				int subscriptionId = Convert.ToInt32(subsId);
				Helpers helper = new Helpers();
				string sessionValue = helper.hasSessionAuth();
				var getUser = helper.GetUserWithUserName(sessionValue);

				if (getUser.CurrentSubscriptionID == subscriptionId)
				{
					return Json(new { success = false, message = "Bu abonelik kampanyasına zaten üyesiniz." });
				}

				var getSubsInfo = await db.Subscriptions.Where(x => x.ID == subscriptionId).FirstAsync();

				SubscriptionTransaction sbsTrancastion = new SubscriptionTransaction();
				sbsTrancastion.SubscriptionID = subscriptionId;
				sbsTrancastion.UserID = getUser.ID;
				sbsTrancastion.TransactionDate = DateTime.Now;
				sbsTrancastion.IsPaid = true;
				db.SubscriptionTransactions.Add(sbsTrancastion);
				await db.SaveChangesAsync();


				var updateUserCurrents = await db.Users.Where(x => x.ID == getUser.ID).FirstAsync();
				updateUserCurrents.ConditionalDate = DateTime.Now.AddDays(Convert.ToInt32(getSubsInfo.SubscriptionDayCount));
				updateUserCurrents.CurrentSubscriptionID = subscriptionId;
				await db.SaveChangesAsync();

				return Json(new { success = true, message = "Abonelik değiştirme işleminiz başarıyla gerçekleşti" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Bir hata oluştu" });
			}


		}



		public ActionResult UserAllProcess()
		{
			Helpers helper = new Helpers();
			string sessionValue = helper.hasSessionAuth();

			if (String.IsNullOrEmpty(sessionValue) == true)
			{
				return RedirectToAction("Index");
			}

			var getUser = helper.GetUserWithUserName(sessionValue);

			var getAllHistory = db.SubscriptionTransactions.Where(x => x.UserID == getUser.ID).OrderByDescending(x=> x.TransactionDate).ToList();


			return View(getAllHistory);
		}


		[HttpPost]
		public JsonResult UpdateUserBillingInformation(string tranId)
		{
			try
			{
				int transactionId = Convert.ToInt32(tranId);
				Helpers helper = new Helpers();
				string sessionValue = helper.hasSessionAuth();
				var getUser = helper.GetUserWithUserName(sessionValue);

				var getTranInfo = db.SubscriptionTransactions.Where(x => x.ID == transactionId).First();
				getTranInfo.IsPaid = true;
				getTranInfo.TransactionDate = DateTime.Now;
				db.SaveChanges();

				var getUserConditionalDate = db.Users.Where(x => x.ID == getUser.ID).First();
				getUserConditionalDate.ConditionalDate = DateTime.Now.AddDays(Convert.ToInt32(getUserConditionalDate.Subscription.SubscriptionDayCount));
				db.SaveChanges();

				///Ödenmemiş başka faturası yok ise kullancıyı aktif et
				int getUserTran = db.SubscriptionTransactions.Where(x => x.UserID == getUser.ID && x.IsPaid == false).Count();
				if (getUserTran == 0)
				{
					var getUserInfo = db.Users.Where(x => x.ID == getUser.ID).First();
					getUserInfo.IsActive = true;
					db.SaveChanges();
				}

				return Json(new { success = true, message = "Ödeme işlemi başarılı" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Bir hata oluştu" });
			}

		}

		[HttpPost]
		public JsonResult PaySubscription(string tranId)
		{
			try
			{
				int transactionId = Convert.ToInt32(tranId);
				Helpers helper = new Helpers();
				string sessionValue = helper.hasSessionAuth();
				var getUser = helper.GetUserWithUserName(sessionValue);

				var getTranInfo = db.SubscriptionTransactions.Where(x => x.ID == transactionId).First();
				getTranInfo.IsPaid = true;
				db.SaveChanges();


				int getUserTran = db.SubscriptionTransactions.Where(x => x.UserID == getUser.ID && x.IsPaid == false).Count();
				if (getUserTran == 0)
				{
					var getUserInfo = db.Users.Where(x => x.ID == getUser.ID).First();
					getUserInfo.IsActive = true;
					db.SaveChanges();
				}


				return Json(new { success = true, message = "Ödeme işlemi başarılı" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "Bir hata oluştu" });
			}
		}

		[HttpPost]
		public ActionResult Logout()
		{
			Session.Abandon();
			return RedirectToAction("Index","Home");
		}



	}
}