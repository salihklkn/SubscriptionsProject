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
}