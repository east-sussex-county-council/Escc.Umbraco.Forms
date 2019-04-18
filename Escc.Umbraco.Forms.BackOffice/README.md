# Escc.Umbraco.Forms.BackOffice

This implements a new entries viewer based on the original, with fixes for several issues. It also allows new lines in the help text for a field. 

## Separating the form design and form processing roles

By default in Umbraco Forms, to grant a user access to view and download their form data, you must also grant them 'Manage Forms' permission, which lets them modify and even delete their form. This opens up potential problems due to the lack of versioning, and due to privacy requirements that must be met. Form design is a role for specialists that are trained to understand such issues, and needs to be separate from processing the data that is submitted to an individual form.

This is achieved by the `~\App_Plugins\EsccUmbracoFormsBackOffice` folder, which adds an extra branch in the tree for the Forms section for viewing entries without offering the ability to design a form. A client-side controller (`controller.js`) and view (`edit.html`) copied from the out-of-the-box files ensures that the built-in functionality for viewing forms is all available. These files may need to be manually replaced each time Umbraco Forms is updated, and the changes are documented in `controller.js` and `edit.html`.

`RemoveOriginalEntriesViewerEventHandler` removes the link to the original entries viewer from the tree to avoid confusion,and `YourForms.html` along with a change to `Dashboard.config` changes the initial Umbraco Forms dashboard to point at the new entries viewer as well. Unfortunately the dashboard does not display the count of form submissions for users without Manage Forms permissions, as it calls an API in closed server-side code.

This issue is logged with Umbraco as [Issue #3](https://github.com/umbraco/Umbraco.Forms.Issues/issues/3).


## A URL to view a single form entry

Unfortunately Umbraco Forms doesn't support a native URL to view a single form entry. This is an important part of implementing a more secure email workflow that sends a link to the secure entries viewer rather than sending submitted form data in an email. 

To add that support, `~\App_Plugins\EsccUmbracoFormsBackOffice\BackOffice\FormEntries\controller.js` has added code that looks for an `entry` parameter on the querystring and opens the view for that entry. This will only work with low form volumes as the entry to view needs to be on the first page of results when they load. 

This issue is logged with Umbraco as [Issue #7](https://github.com/umbraco/Umbraco.Forms.Issues/issues/7).

## Allow paragraphs in question help text 

By default pressing Enter in the help text field will close the 'Edit question' overlay, but `~\App_Plugins\EsccUmbracoFormsBackOffice\BackOffice\allow-new-lines-in-question-help-text.js` fixes this by removing the attribute that triggers it. You can now press Enter to get a new line. 

To complete the fix you will need to update the way help text is rendered in `~\Views\Partials\Forms\Themes\...\Form.cshtml` to replace new lines with paragraph tags.

Change this:

	<span class="help-block">@f.ToolTip</span>

to this:

 	var tooltip = f.ToolTip.StripHtml();
    tooltip = tooltip.Replace("\n\n", "</p><p class=\"help-block\">");
    <p class="help-block">@Html.Raw(tooltip)</p>