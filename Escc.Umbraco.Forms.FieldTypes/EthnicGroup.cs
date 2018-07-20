using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;

namespace Escc.Umbraco.Forms.FieldTypes
{
    /// <summary>
    /// An Umbraco Forms field type that displays a dropdown list of ethnic groups
    /// </summary>
    /// <seealso cref="Umbraco.Forms.Core.FieldType" />
    public class EthnicGroup : FieldType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EthnicGroup"/> class.
        /// </summary>
        public EthnicGroup()
        {
            Id = new Guid("a5f62415-bd82-4acf-b2f8-787442327474");
            Name = "Ethnic group";
            Description = "A dropdown list of ethnic groups, with an 'other' text field";
            DataType = FieldDataType.String;
            FieldTypeViewName = "FieldType.EthnicGroup.cshtml";
            Icon = "icon-fingerprint";
            SupportsPrevalues = false;
            SupportsRegex = false;
            SortOrder = 100;
        }
    }
}