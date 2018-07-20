using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using UmbracoForms = Umbraco.Forms;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Forms.Data.Storage;
using System.Globalization;

namespace Escc.Umbraco.Forms.BackOffice
{
    /// <summary>
    /// Create a new tree in the Umbraco Forms back office that links to an entries viewer for forms to which you have access,
    /// without requiring 'Manage Forms' permission.
    /// </summary>
    [Tree("forms", "FormEntries", "Form entries")]
    [PluginController("EsccUmbracoFormsBackOffice")]
    public class EntriesViewerTreeController : TreeController
    {
        /// <summary>
        /// The method called to render the contents of the tree structure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings">All of the query string parameters passed from jsTree</param>
        /// <returns></returns>
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be pased in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            // check if we're rendering the root node's children
            if (id == Constants.System.Root.ToInvariantString())
            {
                var tree = new TreeNodeCollection();

                // Add a tree node for each form to which the current user has access
                using (FormStorage formStorage = new FormStorage())
                {
                    IEnumerable<UmbracoForms.Core.Form> forms = formStorage.GetForms();
                    foreach (UmbracoForms.Core.Form form in forms)
                    {
                        if (UmbracoForms.Web.FormsSecurity.HasAccessToForm(form.Id))
                        {
                            tree.Add(CreateTreeNode(form.Id.ToString(), id, queryStrings, form.Name, "icon-autofill"));
                        }
                    }
                }

                // Sort the forms alphabetically by name
                tree.Sort(new TreeNodeComparer());

                return tree;
            }
            // this tree doesn't support rendering more than 1 level
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the menu structure for the node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return null;
        }

        private class TreeNodeComparer : IComparer<TreeNode>
        {
            public int Compare(TreeNode x, TreeNode y)
            {
                return StringComparer.Create(CultureInfo.CurrentUICulture, true).Compare(x.Name, y.Name);
            }
        }
    }
}