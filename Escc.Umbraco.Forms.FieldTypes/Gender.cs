using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;

namespace Escc.Umbraco.Forms.FieldTypes
{
    /// <summary>
    /// An Umbraco Forms field type that asks for gender in the GOV.UK-recommended way
    /// </summary>
    /// <seealso cref="Umbraco.Forms.Core.FieldType" />
    /// <remarks>
    /// https://www.gov.uk/service-manual/design/gender-or-sex
    /// </remarks>
    public class Gender : FieldType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gender"/> class.
        /// </summary>
        public Gender()
        {
            Id = new Guid("f384b6df-1344-4416-a4af-562c97a23844");
            Name = "Gender";
            Description = "Only ask if you'll use it. Call it 'Gender', not 'Sex' (unless it's medical).";
            DataType = FieldDataType.String;
            FieldTypeViewName = "FieldType.Gender.cshtml";
            Icon = "icon-male-and-female";
            HideLabel = false;
            SupportsPrevalues = false;
            SupportsRegex = false;
            SortOrder = 100;
        }
    }
}