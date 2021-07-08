using Groover.BL.Models.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Groover.API.Utils
{
    public class ErrorHandlingMiddleware
    {
		private readonly RequestDelegate next;
		private readonly ILogger<ErrorHandlingMiddleware> logger;
		public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
		{
			this.next = next;
			this.logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await next(context);
			}
			catch (NotFoundException nfEx)
			{
				this.logger.LogWarning(nfEx.ToString());
				await HandleExceptionAsync(context, nfEx);
			}
			catch (BadRequestException brEx)
			{
				this.logger.LogWarning(brEx.ToString());
				await HandleExceptionAsync(context, brEx);
			}
			catch (UnauthorizedException unEx)
            {
				this.logger.LogWarning(unEx.ToString());
				await HandleExceptionAsync(context, unEx);
            }
			catch (Exception ex)
			{
				this.logger.LogError("Error has occured. " + ex.ToString());
				await HandleExceptionAsync(context, ex);
			}
		}

		private static Task HandleExceptionAsync(HttpContext context, Exception ex)
		{
			HttpStatusCode code = HttpStatusCode.InternalServerError; // 500 if unexpected
			string clientMessage = null;
			string grooverCode = null;
			string errorVal = null;

			if (ex is NotFoundException)
			{
				var notFoundException = (NotFoundException)ex;
				clientMessage = notFoundException.ClientMessage;
				grooverCode = notFoundException.ErrorCode;
				errorVal = notFoundException.ErrorValue;
				code = HttpStatusCode.NotFound;
			}
			else if (ex is BadRequestException)
			{
				var badRequestException = (BadRequestException)ex;
				clientMessage = badRequestException.ClientMessage;
				grooverCode = badRequestException.ErrorCode;
				errorVal = badRequestException.ErrorValue;
				code = HttpStatusCode.BadRequest;
			}
			else if (ex is UnauthorizedException)
            {
				var unauthorizedException = (UnauthorizedException)ex;
				clientMessage = unauthorizedException.ClientMessage;
				grooverCode = unauthorizedException.ErrorCode;
				errorVal = unauthorizedException.ErrorValue;
				code = HttpStatusCode.Unauthorized;
            }
			else if (ex is GrooverException)
			{
				var grooverException = (GrooverException)ex;
				clientMessage = grooverException.ClientMessage;
				grooverCode = grooverException.ErrorCode;
				errorVal = grooverException.ErrorValue;
				//Code ostaje InternalServerError
			}
			else if (ex is Exception)
            {
				clientMessage = "Unknown error has happened.";
            }

			var result = JsonConvert.SerializeObject(new { error = clientMessage, errorCode = grooverCode, errorValue = errorVal });
			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)code;
			return context.Response.WriteAsync(result);
		}
	}

}
