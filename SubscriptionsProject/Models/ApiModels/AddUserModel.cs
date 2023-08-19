using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SubscriptionsProject.Models.ApiModels
{
	public class AddUserModel
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
		public string PhoneNumber { get; set; }
		public int CurrentSubscriptionID { get; set; }

	}

	public class GetUserModel
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime ConditionalDate { get; set; }
		public int CurrentSubscriptionID { get; set; }

		public DateTime RegistrationDate { get; set; }
		public bool IsActive { get; set; }
		public int ID { get; set; }
		public List<TransactionResponseModel> Transactions { get; set; }

	}

	public class GetAllUserModel
	{
		public int ID { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime RegistrationDate { get; set; }
		public bool IsActive { get; set; }
		public int CurrentSubscriptionID { get; set; }
		public DateTime ConditionalDate { get; set; }

	}

	public class TransactionResponseModel
	{
		public int UserID { get; set; }
		public int SubscriptionID { get; set; }
		public DateTime TranDate { get; set; }
		public bool isPaid { get; set; }
	}

	public class GetAllSubscriptionsModel
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Price { get; set; }
		public int SubscriptionDayCount { get; set; }
		public bool isActive { get; set; }
	}


}