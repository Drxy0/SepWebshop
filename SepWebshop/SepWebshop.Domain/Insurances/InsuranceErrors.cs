using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SepWebshop.Domain.Insurances;

public static class InsuranceErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Insurances.NotFound",
        $"The insurance with the Id = '{id}' was not found");
}

