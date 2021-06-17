﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace LiveStreaming.Common
{
    /// <summary>
    /// 授权令牌头部参数类
    /// </summary>
    public class AuthTokenHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters = operation.Parameters ?? new List<OpenApiParameter>();
            var isAuthor = operation != null && context != null;
            if (isAuthor)
            {
                //in query header 
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "Authorization",
                    Description = "身份验证",
                    Required = false,
                    In = ParameterLocation.Header
                });
            }
            if (!context.ApiDescription.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
               !context.ApiDescription.HttpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
    }
}
