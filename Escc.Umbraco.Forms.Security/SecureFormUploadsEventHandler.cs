using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using umbraco;
using Umbraco.Core;

namespace Escc.Umbraco.Forms.Security
{
    /// <summary>
    /// Event handler to set up a route for downloading and viewing Umbraco Forms uploads in the back office,
    /// after storing those files in a secure location which is not the default.
    /// </summary>
    /// <seealso cref="Umbraco.Core.ApplicationEventHandler" />
    public class SecureFormUploadsEventHandler : ApplicationEventHandler
    {
        /// <summary>
        /// Overridable method to execute when Bootup is completed, this allows you to perform any other bootup logic required for the application.
        /// Resolution is frozen so now they can be used to resolve instances.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Note: Place the action after the filename so that the URL does not end with the file extension. 
            // If it does, some file extensions are not given an UmbracoContext and therefore cannot authorise accesss for a back-office User.
            // https://our.umbraco.com/projects/developer-tools/301-url-tracker/version-2/45546-Value-cannot-be-null-Parameter-name-umbracoContext#comment-163828
            RouteTable.Routes.MapRoute(
            name: "form-uploads",
            url: GlobalSettings.UmbracoMvcArea + "/backoffice/SecureFormUploads/{formId}/{fileId}/{filename}/{action}",
            defaults: new
            {
                controller = "SecureFormUploads"
            });
        }
    }
}