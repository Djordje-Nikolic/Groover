using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Groover.BL.Models.Exceptions;

namespace Groover.BL
{
	public static class ErrorProcessor
	{
		public static void Process(IEnumerable<IdentityError> identityErrors, ILogger logger)
		{
			string devErrors = "";
			string clientErrors = "";
			foreach (var error in identityErrors)
			{
				logger.LogWarning($"Registration error: {error.Description}");
				devErrors += error.Description;
				clientErrors += error.Code;
				if (identityErrors.Last() != error)
				{
					devErrors += " ";
					clientErrors += " ";
				}
			}

			logger.LogWarning("Registration failed.");
			throw new BadRequestException("Registration errors: " + devErrors, clientErrors);
		}

		public static void Process(IEnumerable<string> invalidFields, ILogger logger)
		{
			string errors = "";
			foreach (var invalidField in invalidFields)
			{
				logger.LogWarning($"Invalid field: {invalidField}");
				errors += invalidField;
				if (invalidFields.Last() != invalidField)
				{
					errors += " ";
				}
			}

			logger.LogWarning("Validation failed.");
			throw new BadRequestException("Invalid fields: " + errors, errors, "failed_validation");
		}

		public static void Process(List<ValidationResult> validationResults, ILogger logger)
		{
			string errors = "";
			foreach (var validationRes in validationResults)
			{
				foreach (var member in validationRes.MemberNames)
				{
					logger.LogWarning($"Invalid field: {member}");
					errors += member;
					if (validationRes.MemberNames.Last() != member)
					{
						errors += " ";
					}
				}
			}

			logger.LogWarning("Validation failed.");
			throw new BadRequestException("Invalid fields: " + errors, errors, "failed_validation");
		}
	}
}
