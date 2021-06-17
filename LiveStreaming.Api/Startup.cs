using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


//引用
using LiveStreaming.Common;
using LiveStreaming.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LiveStreaming.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            #region //注入 AddSwaggerGen 
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LiveStreaming.Api", Version = "v1" });

                //启用swagger验证功能
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                //添加全局安全条件
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"}
                        },new string[] { }
                    }
                });
                //显示自定义的Heard Token
                 c.OperationFilter<AuthTokenHeaderParameter>();

            });
            #endregion

 
            #region 注册jwt
            JWTTokenOptions JWTTokenOptions = new JWTTokenOptions();

            //获取appsettings的内容
            services.Configure<JWTTokenOptions>(this.Configuration.GetSection("JWTToken"));
            //将给定的对象实例绑定到指定的配置节
            Configuration.Bind("JWTToken", JWTTokenOptions);

            //注册到Ioc容器
            services.AddSingleton(JWTTokenOptions);

            //添加验证服务
            services.AddAuthentication(option =>
            {
                //默认身份验证模式
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //默认方案
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(option =>
            {
                //设置元数据地址或权限是否需要HTTP
                option.RequireHttpsMetadata = false;
                option.SaveToken = true;
                //令牌验证参数
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    //获取或设置要使用的Microsoft.IdentityModel.Tokens.SecurityKey用于签名验证。
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.
                    GetBytes(JWTTokenOptions.Secret)),
                    //获取或设置一个System.String，它表示将使用的有效发行者检查代币的发行者。 
                    ValidIssuer = JWTTokenOptions.Issuer,
                    //获取或设置一个字符串，该字符串表示将用于检查的有效受众反对令牌的观众。
                    ValidAudience = JWTTokenOptions.Audience,
                    //是否验证发起人
                    ValidateIssuer = false,
                    //是否验证订阅者
                    ValidateAudience = false,
                    ////允许的服务器时间偏移量
                    ClockSkew = TimeSpan.Zero,
                    ////是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                    ValidateLifetime = true
                };
                //如果jwt过期，在返回的header中加入Token-Expired字段为true，前端在获取返回header时判断
                option.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LiveStreaming.Api v1"));
            }

            app.UseRouting();

            //app.UseAuthorization();

            app.UseAuthentication();  //添加认证
            app.UseAuthorization();   //添加授权（.netCore 3.x中使用）


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
