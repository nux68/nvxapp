using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Interfaces
{
    public interface IValidator<in T> where T : class
    {

        IEnumerable<ValidationResult> Validate(T entity);

        IEnumerable<ValidationResult> CanBeEliminated(T entity);

    }
}
