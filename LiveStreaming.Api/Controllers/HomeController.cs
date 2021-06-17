using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveStreaming.Model;
using LiveStreaming.Common;


namespace LiveStreaming.Api.Controllers
{   

    [ApiController]//api控制器
    [Route("api/[controller]/[action]")]//路由
    public class HomeController : Controller
    {
        private readonly JWTTokenOptions _tokenOptions;
        public HomeController(JWTTokenOptions tokenOptions)
        {
            _tokenOptions = tokenOptions;
        }

        /// <summary>
        /// 生成jwt
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public TokenResult GenerateJwt()
        {
            var token = new JwtTokenHelper().AuthorizeToken(123456, _tokenOptions);
            return token;
        }
    }
}
