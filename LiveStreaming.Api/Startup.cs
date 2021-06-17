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


//����
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
            #region //ע�� AddSwaggerGen 
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LiveStreaming.Api", Version = "v1" });

                //����swagger��֤����
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "���¿�����������ͷ����Ҫ���Jwt��ȨToken��Bearer Token",
                    Name = "Authorization",//jwtĬ�ϵĲ�������
                    In = ParameterLocation.Header,//jwtĬ�ϴ��authorization��Ϣ��λ��(����ͷ��)
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                //���ȫ�ְ�ȫ����
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
                //��ʾ�Զ����Heard Token
                 c.OperationFilter<AuthTokenHeaderParameter>();

            });
            #endregion

 
            #region ע��jwt
            JWTTokenOptions JWTTokenOptions = new JWTTokenOptions();

            //��ȡappsettings������
            services.Configure<JWTTokenOptions>(this.Configuration.GetSection("JWTToken"));
            //�������Ķ���ʵ���󶨵�ָ�������ý�
            Configuration.Bind("JWTToken", JWTTokenOptions);

            //ע�ᵽIoc����
            services.AddSingleton(JWTTokenOptions);

            //�����֤����
            services.AddAuthentication(option =>
            {
                //Ĭ�������֤ģʽ
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //Ĭ�Ϸ���
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(option =>
            {
                //����Ԫ���ݵ�ַ��Ȩ���Ƿ���ҪHTTP
                option.RequireHttpsMetadata = false;
                option.SaveToken = true;
                //������֤����
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    //��ȡ������Ҫʹ�õ�Microsoft.IdentityModel.Tokens.SecurityKey����ǩ����֤��
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.
                    GetBytes(JWTTokenOptions.Secret)),
                    //��ȡ������һ��System.String������ʾ��ʹ�õ���Ч�����߼����ҵķ����ߡ� 
                    ValidIssuer = JWTTokenOptions.Issuer,
                    //��ȡ������һ���ַ��������ַ�����ʾ�����ڼ�����Ч���ڷ������ƵĹ��ڡ�
                    ValidAudience = JWTTokenOptions.Audience,
                    //�Ƿ���֤������
                    ValidateIssuer = false,
                    //�Ƿ���֤������
                    ValidateAudience = false,
                    ////����ķ�����ʱ��ƫ����
                    ClockSkew = TimeSpan.Zero,
                    ////�Ƿ���֤Token��Ч�ڣ�ʹ�õ�ǰʱ����Token��Claims�е�NotBefore��Expires�Ա�
                    ValidateLifetime = true
                };
                //���jwt���ڣ��ڷ��ص�header�м���Token-Expired�ֶ�Ϊtrue��ǰ���ڻ�ȡ����headerʱ�ж�
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

            app.UseAuthentication();  //�����֤
            app.UseAuthorization();   //�����Ȩ��.netCore 3.x��ʹ�ã�


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
