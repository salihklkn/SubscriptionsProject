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
	public class AuthController : ApiController
	{
		SubscribeMembers_DBEntities db = new SubscribeMembers_DBEntities();

		[HttpPost]
		[Route("api/AdminLogin")]
		[AllowAnonymous]
		public object AdminLogin(string username, string password)
		{
			var getAdmin = db.AdminUsers.Where(x => x.UserName == username && x.Password == password).FirstOrDefault();
			if (getAdmin == null)
			{
				return Ok(new ApiResponse<string>
				{
					Success = false,
					Message = "Kullanıcı adı veya Şifre hatalı.",
				});
			}


			return Ok(new ApiResponse<string>
			{
				Success = true,
				Message = "Başarılı.",
				Data = JwtManager.GenerateToken(username),
			});


			throw new HttpResponseException(HttpStatusCode.Unauthorized);
		}
	}
}
