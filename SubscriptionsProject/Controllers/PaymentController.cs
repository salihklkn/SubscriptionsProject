using SubscriptionsProject.Common;
using SubscriptionsProject.Models;
using SubscriptionsProject.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SubscriptionsProject.Controllers
{
	public class PaymentController : ApiController
	{
		SubscribeMembers_DBEntities db = new SubscribeMembers_DBEntities();

		[HttpGet]
		[Route("api/GetUnPaidData")]
		[JwtAuthentication]
		public List<UnPaidDataResponse> GetUnPaidData()
		{
			var getAllUnpaid = db.SubscriptionTransactions.Where(x => x.IsPaid == false).ToList();

			List<UnPaidDataResponse> unpaidlist = new List<UnPaidDataResponse>();

			foreach (var item in getAllUnpaid)
			{
				unpaidlist.Add(new UnPaidDataResponse() { ID = item.ID, SubscriptionID = Convert.ToInt32(item.SubscriptionID), UserID = Convert.ToInt32(item.UserID) });
			}

			return unpaidlist;
		}

		[HttpPost]
		[Route("api/PaymentUser")]
		[JwtAuthentication]
		public string PaymentUser(PaymentUserResponse payment)
		{
			if (payment.ID == 0 && payment.SubscriptionID == 0 && payment.UserID == 0)
			{
				return "ID (Transaction ID, UserID, SubscriptionID gereklidir)";
			}

			var getTranInfo = db.SubscriptionTransactions.Where(x => x.ID == payment.ID && x.UserID == payment.UserID && x.SubscriptionID == payment.SubscriptionID).FirstOrDefault();
			if (getTranInfo != null)
			{
				getTranInfo.IsPaid = true;
				getTranInfo.TransactionDate = DateTime.Now;
				db.SaveChanges();
			}

			var getUserConditionalDate = db.Users.Where(x => x.ID == payment.UserID).First();
			getUserConditionalDate.ConditionalDate = DateTime.Now.AddDays(Convert.ToInt32(getUserConditionalDate.Subscription.SubscriptionDayCount));
			db.SaveChanges();

			int getUserTran = db.SubscriptionTransactions.Where(x => x.UserID == payment.UserID && x.IsPaid == false).Count();
			if (getUserTran == 0)
			{
				var getUserInfo = db.Users.Where(x => x.ID == payment.UserID).First();
				getUserInfo.IsActive = true;
				db.SaveChanges();
			}

			return "success";
		}

		[HttpPost]
		[Route("api/PaymentTransactionTransferProtocol")]
		[JwtAuthentication]
		public async Task<string> PaymentTransactionTransferProtocol()
		{
			var getNeedPaymentUsers = await db.Users.Where(x => x.ConditionalDate < DateTime.Now).ToListAsync();

			int processCount = getNeedPaymentUsers.Count();
			foreach (var item in getNeedPaymentUsers)
			{
				SubscriptionTransaction sbsTran = new SubscriptionTransaction();
				sbsTran.IsPaid = false;
				sbsTran.SubscriptionID = item.CurrentSubscriptionID;
				sbsTran.UserID = item.ID;

				db.SubscriptionTransactions.Add(sbsTran);
				await db.SaveChangesAsync();

				var getUser = db.Users.Where(x => x.ID == item.ID).First();
				getUser.IsActive = false;
				db.SaveChanges();
			}

			return processCount.ToString() + " adet kullanıcıya ait ödenmesi gereken fatura sisteme eklendi.";
		}


	}
}
