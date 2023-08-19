using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SubscriptionsProject.Models
{
	public class UserPanelModel
	{
		public int CampaignCount { get; set; }

		public int UserCount { get; set; }

		public int ProcessCount { get; set; }
		public int CurrentDate { get; set; }
		public User UserItem { get; set; }
	}

	public class UserInformationModel
	{
		public User UserItem { get; set; }

		public List<Subscription> Subscription { get; set; }
	}

	public class AdminPanelModel
	{
		public int CampaignCount { get; set; }
		public int UserCount { get; set; }
		public int ProcessCount { get; set; }
		public int NotPaidCount { get; set; }
		public AdminUser AdminItem { get; set; }
	}

	public class AdminPanelUsersModel
	{
		public List<User> Users { get; set; }
		public List<Subscription> Subscriptions { get; set; }
	}
	

}