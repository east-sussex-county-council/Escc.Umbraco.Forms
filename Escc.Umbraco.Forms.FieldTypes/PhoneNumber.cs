using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;

namespace Escc.Umbraco.Forms.FieldTypes
{
    /// <summary>
    /// An Umbraco Forms field type that validates a phone number
    /// </summary>
    /// <seealso cref="Umbraco.Forms.Core.FieldType" />
    public class PhoneNumber : FieldType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumber"/> class.
        /// </summary>
        public PhoneNumber()
        {
            Id = new Guid("b89819aa-13e9-4705-a6a9-98539bc423f9");
            Name = "Phone number";
            Description = "A phone number. Triggers a phone keypad on mobiles.";
            DataType = FieldDataType.String;
            FieldTypeViewName = "FieldType.PhoneNumber.cshtml";
            Icon = "icon-iphone";
            HideLabel = false;
            SupportsPrevalues = false;
            SupportsRegex = false;
            SortOrder = 25;
        }

        /// <summary>
        /// Validates the field.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="field">The field.</param>
        /// <param name="postedValues">The posted values.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override IEnumerable<string> ValidateField(Form form, Field field, IEnumerable<object> postedValues, HttpContextBase context)
        {
            var errorMessages = new List<string>(base.ValidateField(form, field, postedValues, context));

            var regex = new Regex("^\\+?[0-9 ]{11,15}$");
            foreach (string value in postedValues)
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (!regex.IsMatch(value))
                    {
                        errorMessages.Add("Please enter a valid phone number");
                        break;
                    }
                }
            }

            return errorMessages;
        }
    }
}