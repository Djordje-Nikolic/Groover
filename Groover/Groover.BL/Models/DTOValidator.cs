using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Models
{
	public class DTOValidator<TDTO> where TDTO : class
	{
		public bool IsValid { get; set; }
		public List<ValidationResult> ValidationResults { get; set; }

		public void Validate(TDTO DTOObject)
		{
			var context = new ValidationContext(DTOObject, null, null);
			ValidationResults = new List<ValidationResult>();
			IsValid = Validator.TryValidateObject(DTOObject, context, ValidationResults);
		}
	}
}
