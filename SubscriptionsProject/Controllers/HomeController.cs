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
			var getUser = db.Users.Where(x => x.Username == username && x.Password == password).FirstOrDefault();
			if (getUser == null)
			{
				return RedirectToAction("Index");
			}

			Session["KullaniciAdi"] = getUser.Username;
			return RedirectToAction("UserPanel");

		}
		public ActionResult UserPanel()
		{
			return View();
		}

	}
}