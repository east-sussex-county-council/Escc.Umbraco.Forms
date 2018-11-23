using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Forms.Core;
using fieldTypes = Umbraco.Forms.Core.Providers.FieldTypes;

namespace Escc.Umbraco.Forms.FieldTypes
{
    /// <summary>
    /// A date picker field for Umbraco Forms with validation that prevents an invalid SQL date from crashing the form
    /// </summary>
    /// <seealso cref="Umbraco.Forms.Core.FieldType" />
    public class DatePicker : fieldTypes.DatePicker
    {
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

            foreach (string value in postedValues)
            {
                ValidateDate(value, errorMessages);
            }

            return errorMessages;
        }

        /// <summary>
        /// Validates a date string and, if invalid, adds errors to a collection of error messages.
        /// </summary>
        /// <param name="value">The date value.</param>
        /// <param name="errorMessages">The error messages.</param>
        public void ValidateDate(string value, IList<string> errorMessages)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                DateTime parsedDate;
                if (!DateTime.TryParse(value, out parsedDate))
                {
                    errorMessages.Add($"Please enter a valid date");
                }

                if (parsedDate < System.Data.SqlTypes.SqlDateTime.MinValue.Value)
                {
                    errorMessages.Add($"You cannot enter a date earlier than {System.Data.SqlTypes.SqlDateTime.MinValue.Value.ToLongDateString()}");
                }
            }
        }
    }
}