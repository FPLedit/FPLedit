using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI.Validators
{
    public class ValidatorCollection
    {
        private BaseValidator[] validators;

        public ValidatorCollection(params BaseValidator[] validators)
        {
            this.validators = validators;
        }

        public string Message
            => string.Join(Environment.NewLine, validators.Where(v => !v.Valid).Select(v => v.ErrorMessage));

        public bool IsValid => validators.All(v => v.Valid);
    }
}
