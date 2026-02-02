using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrService.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException() : base("One or more validation failures have occurred.")
        {
            Errors = new List<ErrorDescriptor>();
        }
        public List<ErrorDescriptor> Errors { get; }
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            foreach (var failure in failures)
            {
                Errors.Add(new ErrorDescriptor(failure.PropertyName, failure.ErrorMessage));
            }
        }
    }
}
public record ErrorDescriptor(string Name, string Reason);