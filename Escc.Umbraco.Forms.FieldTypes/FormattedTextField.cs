using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;

namespace Escc.Umbraco.Forms.FieldTypes
{
    /// <summary>
    /// An Umbraco Forms field type that displays static HTML on the form
    /// </summary>
    /// <seealso cref="Umbraco.Forms.Core.FieldType" />
    public class FormattedTextField : FieldType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedTextField"/> class.
        /// </summary>
        public FormattedTextField()
        {
            Id = new Guid("8cbb19d4-43ea-4a61-bd19-579853279d60");
            Name = "Formatted text";
            DataType = FieldDataType.LongString;
            FieldTypeViewName = "FieldType.FormattedText.cshtml";
            Icon = "icon-font";
            HideLabel = true;
            SupportsPrevalues = false;
            SupportsRegex = false;
            SortOrder = 100;
        }

        [Setting("Formatted text", view = "RichTextEditor")]
        public string FormattedText { get; set; }
    }
}