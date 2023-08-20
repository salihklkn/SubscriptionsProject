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
		public object GetAllSubscriptions()
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

			return Ok(new ApiResponse<List<GetAllSubscriptionsModel>>
			{
				Success = true,
				Message = "",
				Data = allsbs
			});
		}

		[HttpPost]
		[Route("api/AddNewSubscriptionCampaign")]
		[JwtAuthentication]
		public object AddNewSubscriptionCampaign([FromBody] AddNewSubscriptionCampaignModel scm)
		{
			try
			{
				if (String.IsNullOrEmpty(scm.Name) == true || scm.Price == 0 || scm.IsActive == null || scm.DayCount == 0)
				{
					return Ok(new ApiResponse<string>
					{
						Success = false,
						Message = "Alanları boş bırakmayınız..",
					});
				}


				Subscription sbs = new Subscription();
				sbs.IsActive = scm.IsActive;
				sbs.Name = scm.Name;
				sbs.Price = scm.Price;
				sbs.SubscriptionDayCount = scm.DayCount;

				db.Subscriptions.Add(sbs);
				db.SaveChanges();

				return Ok(new ApiResponse<string>
				{
					Success = true,
					Message = "Ekleme işlemi başarılı.",
				});
			}
			catch (Exception ex)
			{
				return Ok(new ApiResponse<string>
				{
					Success = false,
					Message = ex.Message,
				});
			}




		}


	}
}
