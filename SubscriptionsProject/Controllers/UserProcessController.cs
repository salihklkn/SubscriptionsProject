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
using System.Threading.Tasks;
using System.Web.Http;

namespace SubscriptionsProject.Controllers
{
	public class UserProcessController : ApiController
	{
		SubscribeMembers_DBEntities db = new SubscribeMembers_DBEntities();

		[HttpPost]
		[Route("api/AddUser")]
		[JwtAuthentication]
		public async Task<object> AddUser([FromBody] AddUserModel user)
		{
			try
			{
				if (String.IsNullOrEmpty(user.UserName) == true || String.IsNullOrEmpty(user.Password) == true || String.IsNullOrEmpty(user.Name) == true || String.IsNullOrEmpty(user.Surname) == true || String.IsNullOrEmpty(user.PhoneNumber) == true)
				{
					return Ok(new ApiResponse<string>
					{
						Success = false,
						Message = "isim, soyad, kullanıcı adı, şifre , telefon numarası boş bırakılamaz.",
					});
				}
				if (user.CurrentSubscriptionID == 0)
				{
					return Ok(new ApiResponse<string>
					{
						Success = false,
						Message = "Kullanıcı eklemek için başlayacağı bir abonelik girilmelidir abonelik kampanyalarını listeleyin ve bir ID atayın.  /api/GetAllSubscriptions.",
					});
				}

				if (db.Users.Where(x => x.Username == user.UserName).Count() > 0)
				{
					return Ok(new ApiResponse<string>
					{
						Success = false,
						Message = "Böyle bir kullanıcı adı ile kayıt zaten var.",
					});
				}

				var subsCurrents = db.Subscriptions.Where(x => x.ID == user.CurrentSubscriptionID).FirstOrDefault();
				if (subsCurrents == null)
				{
					return Ok(new ApiResponse<string>
					{
						Success = false,
						Message = "Abonelik Kampanyası bulunamadı.  /api/GetAllSubscriptions",
					});
				}

				Helpers helper = new Helpers();
				string percentVal = await helper.AddNewUserApi(user);

				return Ok(new ApiResponse<string>
				{
					Success = true,
					Message = "Kullanıcı ekleme işlemi başarılı. Lütfen üyemizden "+ percentVal.ToString() +" ₺ depozito tutarı isteyiniz.",
				});
			}
			catch (Exception ex)
			{
				return Ok(new ApiResponse<string>
				{
					Success = false,
					Message = "Bir hata oluştu " + ex.Message.ToString() ,
				});
			}

		}


		[HttpGet]
		[Route("api/GetAllUser")]
		[JwtAuthentication]
		public object GetAllUser()
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

			return Ok(new ApiResponse<List<GetAllUserModel>>
			{
				Success = true,
				Message = "",
				Data = allUsers,
			});

		}

		///içinde girilen değerlere göre geçen tüm kullanıcılar bulunur ve geçmiş işlemleri de gözükür
		[HttpGet]
		[Route("api/GetUser")]
		[JwtAuthentication]
		public object GetUser(GetUserModel usr)
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

			return Ok(new ApiResponse<List<GetUserModel>>
			{
				Success = true,
				Message = "",
				Data = users,
			});
		}


		[HttpPost]
		[Route("api/FreezeUser")]
		[JwtAuthentication]
		public object FreezeUser([FromBody] FreezeUserModel user)
		{
			try
			{
				if (user.ID == 0)
				{
					return Ok(new ApiResponse<string>
					{
						Success = false,
						Message = "ID değeri boş bırakılamaz",
					});
				}

				var getUser = db.Users.Where(x => x.ID == user.ID).FirstOrDefault();

				if (getUser == null)
				{
					return Ok(new ApiResponse<string>
					{
						Success = false,
						Message = "Belirtilen ID değerine uygun bir kayıt bulunamadı",
					});
				}

				if (getUser.IsActive == false)
				{
					return Ok(new ApiResponse<string>
					{
						Success = false,
						Message = "Bu kullanıcı zaten donduruldu",
					});
				}

				var hasUnpaidTrans = db.SubscriptionTransactions.Where(x => x.UserID == getUser.ID && x.IsPaid == false).Count();

				if (hasUnpaidTrans > 0)
				{
					return Ok(new ApiResponse<string>
					{
						Success = false,
						Message = "BU üyemiz için Ödenmemiş faturalar mevcut.",
					});
				}

				getUser.IsActive = false;
				db.SaveChanges();

				var getDepositAmount = db.UserRegisterDepozits.Where(x => x.UserID == getUser.ID).FirstOrDefault();

				if (getDepositAmount != null)
				{
					decimal price = Convert.ToDecimal(getDepositAmount.DepozitPrice);
					return Ok(new ApiResponse<string>
					{
						Success = true,
						Message = "Abonelik Dondurma işlemi tamamlandı. Lütfen kullanıcının ödemiş olduğu " + price.ToString() + "₺ depozito tutarını iade ediniz",
					});
				}

				return Ok(new ApiResponse<string>
				{
					Success = true,
					Message = "Abonelik Dondurma işlemi tamamlandı",
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
