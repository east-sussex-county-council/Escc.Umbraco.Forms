using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Escc.Umbraco.Forms.Security
{
    /// <summary>
    /// Security settings for an Umbraco Form
    /// </summary>
    public class FormSecurity
    {
        /// <summary>
        /// Gets or sets the name of the form.
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// The users who have back-office access to view the form entries
        /// </summary>
        public IList<FormUser> Users = new List<FormUser>();
    }
}