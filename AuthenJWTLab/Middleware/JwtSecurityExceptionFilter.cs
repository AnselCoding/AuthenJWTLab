﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AuthenJWTLab.Middleware
{
    public class JwtSecurityExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            // 捕捉異常
            var exception = context.Exception;

            // 在實際應用中，這裡可以進行日誌記錄或通知操作
            string exceptionType = exception.GetType().Name;
            string errorMessage = exception.Message;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.WriteLine($"Invalid token, {exceptionType}: {errorMessage}");
            Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");

            // 返回自定義的錯誤響應
            var result = new ObjectResult(new { ErrorMessage = "Invalid token." })
            {
                StatusCode = 400, // 400 Bad Request
            };

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}