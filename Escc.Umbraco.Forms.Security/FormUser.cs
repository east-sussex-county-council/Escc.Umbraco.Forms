using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Escc.Umbraco.Forms.Security
{
    /// <summary>
    /// A user who has back-office access to a form
    /// </summary>
    public class FormUser
    {
        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int UserId { get; set; }
    }
}