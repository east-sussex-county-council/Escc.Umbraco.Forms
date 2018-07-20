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
    /// Removes the link to the entries viewer supplied with Umbraco Forms, which is replaced by the version defined in this project
    /// </summary>
    /// <seealso cref="Umbraco.Core.ApplicationEventHandler" />
    public class RemoveOriginalEntriesViewerEventHandler : ApplicationEventHandler
    {
        /// <summary>
        /// Overridable method to execute when Bootup is completed, this allows you to perform any other bootup logic required for the application.
        /// Resolution is frozen so now they can be used to resolve instances.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            TreeControllerBase.TreeNodesRendering += TreeControllerBase_TreeNodesRendering;

            base.ApplicationStarted(umbracoApplication, applicationContext);
        }

        private void TreeControllerBase_TreeNodesRendering(TreeControllerBase sender, TreeNodesRenderingEventArgs e)
        {
            if (sender.TreeAlias == "form")
            {
                // If this is the 'Entries' node under a form, remove it
                if (e.Nodes.Count == 1 && e.Nodes[0].Name == "Entries")
                {
                    e.Nodes.Clear();
                }
                else
                {
                    // Otherwise it's the node for a form. 'Entries' was to be its only child, 
                    // so set HasChildren to false to remove the arrow indicating that the tree can be expanded.
                    foreach (var node in e.Nodes)
                    {
                        node.HasChildren = false;
                    }
                }
            }
        }
    }
}