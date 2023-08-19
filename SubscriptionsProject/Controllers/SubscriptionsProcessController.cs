using SubscriptionsProject.Common;
using SubscriptionsProject.Models;
using SubscriptionsProject.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SubscriptionsProject.Controllers
{
    public class SubscriptionsProcessController : ApiController
    {
		SubscribeMembers_DBEntities db = new SubscribeMembers_DBEntities();

		[HttpGet]
		[Route("api/GetAllSubscriptions")]
		[JwtAuthentication]
		public List<GetAllSubscriptionsModel> GetAllSubscriptions()
		{
			var getAllsbs = db.Subscriptions.ToList();

			List<GetAllSubscriptionsModel> allsbs = new List<GetAllSubscriptionsModel>();


			foreach (var item in getAllsbs)
			{
				allsbs.Add(new GetAllSubscriptionsModel()
				{
					ID = item.ID,
					isActive = Convert.ToBoolean(item.IsActive),
					Name = item.Name,
					Price = Convert.ToString(item.Price),
					SubscriptionDayCount = Convert.ToInt32(item.SubscriptionDayCount),
				});
			}
			
			return allsbs;
		}
	}
}
