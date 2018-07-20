using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using umbraco;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Escc.Umbraco.Forms.Security
{
    /// <summary>
    /// Wires up a 'Permissions' menu item for Umbraco Forms which displays who has permission to a form
    /// </summary>
    /// <seealso cref="Umbraco.Core.ApplicationEventHandler" />
    public class FormPermissionsEventHandler : ApplicationEventHandler
    {
        /// <summary>
        /// Overridable method to execute when Bootup is completed, this allows you to perform any other bootup logic required for the application.
        /// Resolution is frozen so now they can be used to resolve instances.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Add a 'Permissions' menu item to each form under the 'Forms' node
            TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;

            // Add a route for the menu item configured below
            RouteTable.Routes.MapRoute(
              name: "form-permissions",
              url: GlobalSettings.UmbracoMvcArea + "/backoffice/FormPermissions/{formId}",
              defaults: new
              {
                  controller = "FormPermissions",
                  action = "PermissionsForForm"
              });

            base.ApplicationStarted(umbracoApplication, applicationContext);
        }

        /// <summary>
        /// Add a 'Permissions' menu item to each form under the 'Forms' node
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MenuRenderingEventArgs"/> instance containing the event data.</param>
        void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            if (sender.TreeAlias == "form" && e.NodeId != "-1")
            {
                var m = new MenuItem("form-permissions", "Permissions");
                m.Icon = umbraco.BusinessLogic.Actions.ActionRights.Instance.Icon;
                m.LaunchDialogUrl(VirtualPathUtility.ToAbsolute("~/") + GlobalSettings.UmbracoMvcArea + "/backoffice/FormPermissions/" + e.NodeId, "Form permissions");
                e.Menu.Items.Add(m);
            }
        }
    }
}