using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;

namespace Escc.Umbraco.Forms.FieldTypes
{
    /// <summary>
    /// An Umbraco Forms field type that validates a URL
    /// </summary>
    /// <seealso cref="Umbraco.Forms.Core.FieldType" />
    public class Url : FieldType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Url"/> class.
        /// </summary>
        public Url()
        {
            Id = new Guid("55edde34-c49e-4347-aef6-fd236052e2b3");
            Name = "URL";
            Description = "A URL. Triggers a keypad with : and // on touch screens.";
            DataType = FieldDataType.String;
            FieldTypeViewName = "FieldType.Url.cshtml";
            Icon = "icon-mouse-cursor";
            HideLabel = false;
            SupportsPrevalues = false;
            SupportsRegex = false;
            SortOrder = 25;
        }

        /// <summary>
        /// Require a JavaScript file that adds a protocol if it's missed
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public override IEnumerable<string> RequiredJavascriptFiles(Field field)
        {
            var list = new List<string>(base.RequiredJavascriptFiles(field));
            list.Add("~/Scripts/Escc.Umbraco.Forms.FieldTypes/FieldType.Url.js");
            return list;
        }

        [Setting("Default value")]
        public string DefaultValue { get; set; }

        [Setting("Placeholder")]
        public string Placeholder { get; set; }

        [Setting("Allow internal network URLs", description = "Allows URLs without a . in the domain name", view ="checkbox")]
        public string AllowInternal { get; set; }

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
                if (!ValidateUrl(value))
                {
                    // This message is the same as the jQuery Validate one, as both may appear
                    // and it could be confusing to have different messages.
                    errorMessages.Add("Please enter a valid URL."); 
                    break;
                }
            }

            return errorMessages;
        }

        /// <summary>
        /// Validates a URL based on a regex and the selected settings. Does not check that the address exists.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns></returns>
        public bool ValidateUrl(string url)
        {
            // this is not a required field validator
            if (String.IsNullOrEmpty(url)) return true;

            // Do the regex validation
            var urlRegex = "^https?://[a-zA-Z0-9-.]+.[a-zA-Z]{2,}";
            if (AllowInternal?.ToUpperInvariant() == "TRUE")
            {
                urlRegex = "^https?://[a-zA-Z0-9-.]+";
            }

            if (!Regex.IsMatch(url, urlRegex))
            {
                return false;
            }


            return true;
        }
    }
}