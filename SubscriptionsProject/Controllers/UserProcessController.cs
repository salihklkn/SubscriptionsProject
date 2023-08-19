using Newtonsoft.Json;
using SubscriptionsProject.Common;
using SubscriptionsProject.Models;
using SubscriptionsProject.Models.ApiModels;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SubscriptionsProject.Controllers
{
	public class UserProcessController : ApiController
	{
		SubscribeMembers_DBEntities db = new SubscribeMembers_DBEntities();

		[HttpPost]
		[Route("api/AddUser")]
		[JwtAuthentication]
		public string AddUser([FromBody] AddUserModel user)
		{
			try
			{
				if (String.IsNullOrEmpty(user.UserName) == true || String.IsNullOrEmpty(user.Password) == true || String.IsNullOrEmpty(user.Name) == true || String.IsNullOrEmpty(user.Surname) == true || String.IsNullOrEmpty(user.PhoneNumber) == true)
				{
					return "isim, soyad, kullanıcı adı, şifre , telefon numarası boş bırakılamaz";
				}
				if (user.CurrentSubscriptionID == 0)
				{
					return "Kullanıcı eklemek için başlayacağı bir abonelik girilmelidir abonelik kampanyalarını listeleyin ve bir ID atayın.  /api/GetAllSubscriptions";
				}

				if (db.Users.Where(x => x.Username == user.UserName).Count() > 0)
				{
					return "Böyle bir kullanıcı adı ile kayıt zaten var";
				}

				User usr = new User();
				usr.IsActive = true;
				usr.Name = user.Name;
				usr.Username = user.UserName;
				usr.Password = user.Password;
				usr.Surname = user.Surname;
				usr.PhoneNumber = user.PhoneNumber;
				usr.RegistrationDate = DateTime.Now;
				var subsCurrents = db.Subscriptions.Where(x => x.ID == user.CurrentSubscriptionID).FirstOrDefault();
				if (subsCurrents == null)
				{
					return "Abonelik Kampanyası bulunamadı.  /api/GetAllSubscriptions";
				}
				usr.ConditionalDate = DateTime.Now.AddDays(Convert.ToInt32(subsCurrents.SubscriptionDayCount));
				usr.CurrentSubscriptionID = user.CurrentSubscriptionID;

				db.Users.Add(usr);
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

				return "success";
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

		}


		[HttpGet]
		[Route("api/GetAllUser")]
		[JwtAuthentication]
		public List<GetAllUserModel> GetAllUser()
		{
			var getAllUser = db.Users.ToList();

			List<GetAllUserModel> allUsers = new List<GetAllUserModel>();

			foreach (User item in getAllUser)
			{
				allUsers.Add(new GetAllUserModel()
				{
					ConditionalDate = Convert.ToDateTime(item.ConditionalDate),
					CurrentSubscriptionID = Convert.ToInt32(item.CurrentSubscriptionID),
					ID = item.ID,
					IsActive = Convert.ToBoolean(item.IsActive),
					Name = item.Name,
					Password = item.Password,
					PhoneNumber = item.PhoneNumber,
					RegistrationDate = Convert.ToDateTime(item.RegistrationDate),
					Surname = item.Surname,
					UserName = item.Username,
				});
			}
			return allUsers;
		}

		///içinde girilen değerlere göre geçen tüm kullanıcılar bulunur ve geçmiş işlemleri de gözükür
		[HttpGet]
		[Route("api/GetUser")]
		[JwtAuthentication]		
		public List<GetUserModel> GetUser(GetUserModel usr)
		{
			var getAllUser = db.Users.Where(x => x.ID == usr.ID || x.Name == usr.Name || x.Surname == usr.Surname || x.PhoneNumber == usr.PhoneNumber || x.Username == usr.UserName).ToList();

			List<GetUserModel> users = new List<GetUserModel>();

			foreach (User item in getAllUser)
			{
				GetUserModel newUser = new GetUserModel();
				newUser.Transactions = new List<TransactionResponseModel>();

				newUser.ConditionalDate = Convert.ToDateTime(item.ConditionalDate);
				newUser.CurrentSubscriptionID = Convert.ToInt32(item.CurrentSubscriptionID);
				newUser.ID = item.ID;
				newUser.IsActive = Convert.ToBoolean(item.IsActive);
				newUser.Name = item.Name;
				newUser.Password = item.Password;
				newUser.PhoneNumber = item.PhoneNumber;
				newUser.RegistrationDate = Convert.ToDateTime(item.RegistrationDate);
				newUser.Surname = item.Surname;
				newUser.UserName = item.Username;

				foreach (var tranitem in item.SubscriptionTransactions)
				{
					
					newUser.Transactions.Add(new TransactionResponseModel()
					{
						isPaid = Convert.ToBoolean(tranitem.IsPaid),
						SubscriptionID = Convert.ToInt32(tranitem.SubscriptionID),
						TranDate = Convert.ToDateTime(tranitem.TransactionDate),
						UserID = Convert.ToInt32(tranitem.UserID)
					});
				}
				users.Add(newUser);
			}
			return users;
		}
	}
}
