# Escc.Umbraco.Forms.Workflows

## Implementing a more secure email workflow

The email workflows that come with Umbraco Forms are designed to send the form data as part of the email. However, the data may be sensitive and email is not a secure medium, so it is better to send a link to the form in the back office where the data is protected by TLS and authentication.

The 'Send email with template (Razor)' workflow allows you to customise the email that is sent, but the body of the email only has access to the fields submitted with the form, not to metadata about the form itself. To work around this:

* Create a custom email template in `~\Views\Partials\Forms\Emails` which includes the following code:

        @{ 
    	var siteDomain = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        var formEntryId = Model.Fields.FirstOrDefault(field => field.Alias.ToUpperInvariant() == "FORMENTRYID");
        var formEntry = (formEntryId != null ? "edit/" + String.Join("?entry=", formEntryId.FieldValue) : String.Empty);
        <p><a href="@siteDomain/umbraco/#/forms/FormEntries/@formEntry">View the new form entry</a></p>
        }

* Add a field called 'Form entry id' to a form using the 'Hidden' answer type, ensuring it has the alias `formEntryId`.

* Add a workflow using the 'Save form entry id' workflow (implemented by `SaveIdAsFieldWorkflow`).

* Add a workflow using the 'Send email with template (Razor)' workflow **after** the 'Save form entry id' workflow and pick the custom template that you created. 
 
  
* When a form is submitted and the workflows run, the form id and form entry id are both added to the `formEntryId` field as separate values, and used by your template to build a link to view the entry. 

Unfortunately Umbraco Forms doesn't support a native URL to view a single form entry. To add that support install [Escc.Umbraco.Forms.BackOffice](../Escc.Umbraco.Forms.Backoffice/) which adds the modified entries viewer linked to in the code above.


## Implementing a retention schedule

Every form that collects personal data must have a retention schedule, and ideally this should be automated to ensure that it is not forgotten. For retention schedules that are simply a set time after the form is submitted, this is implemented by `RetentionAfterSetDateWorkflow`. 

* Add a field called 'Delete after' to a form using the 'Hidden' answer type, ensuring it has the alias `deleteAfter`.
* Add a workflow using the 'Retention schedule: after a set date' workflow, and set the time period to keep records for.
* When a form is submitted and the workflow runs, a date is added to the `deleteAfter` field.
* Run a scheduled task regularly to call `https://hostname/umbraco/api/UmbracoFormsRetentionApi/ApplySetDateRetentionSchedule` using the HTTP `DELETE` method. It will look for the `deleteAfter` field on all form records, and delete any where it finds a date that has passed. 