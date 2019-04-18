# Escc.Umbraco.Forms.FieldTypes

## Email, phone and URL

There are three new field types for `Email`, `PhoneNumber` and `URL` which are similar to 'Short answer', but use the correct HTML field types to trigger helpful keypads on touch screens. Each one includes a better validation than the 'Validate as email', 'Validate as a number' and 'Validate as a Url' options which come with Umbraco Forms' 'Short answer' field type. 

`URL` supports entering internal URLs which don't have a . in the hostname, and adds a script which inserts `https://` at the start of the entered value if no protocol is entered, so that values like `www.example.org` validate correctly.

## Formatted text 

`FormattedTextField` provides a way for form designers to add some static HTML in the form. It is implemented as a question which does not need an answer.

This works by setting up TinyMCE as a new setting type, so that a setting on a field can use the rich text editor. The new setting type requires a view at `~\App_Plugins\UmbracoForms\BackOffice\Common\SettingTypes\RichTextEditor.html` and a controller loaded by the `~\App_Plugins\EsccUmbracoFormsFieldTypes\*` plugin.

## Ethnic group

`EthnicGroup` inserts a standardised dropdown list of ethnic groups, and an 'other' box which appears if any of the 'other' options are selected. To make this work you need to add code to inject a condition into the page which hooks into Umbraco Forms' own conditional logic. Add the following code to `~\Views\Partials\Forms\Themes\[your-theme]\Script.cshtml` before the line which reads `var formJsObj = new ...`. 

	// START: Ethnic group field
    //
    // The 'Ethnic group' field type has two fields in the HTML returned to the client, 
    // and the second is only needed if specific values are selected in the first.
    //
    // Inject an extra condition for this page of the form to cause the Umbraco Forms 
    // conditional code to handle that for us, so that we can be sure it behaves consistently.

    foreach (var fieldset in Model.CurrentPage.Fieldsets)
    {
        foreach (var group in fieldset.Containers)
        {
            foreach (var field in group.Fields)
            {
                if (field.FieldTypeName == "Ethnic group")
                {
                    var rules = new List<Umbraco.Forms.Web.Models.ConditionRuleViewModel>();
                    rules.Add(new Umbraco.Forms.Web.Models.ConditionRuleViewModel
                    {
                        Field = new Guid(field.Id),
                        FieldsetId = new Guid(fieldset.Id),
                        Operator = Umbraco.Forms.Core.FieldConditionRuleOperator.Contains,
                        Value = "other"
                    });
                    var condition = new Umbraco.Forms.Web.Models.ConditionViewModel
                    {
                        ActionType = Umbraco.Forms.Core.FieldConditionActionType.Show,
                        LogicType = Umbraco.Forms.Core.FieldConditionLogicType.All,
                        Rules = rules
                    };

                    Model.FieldConditions.Add(new Guid("12345678-" + field.Id.Substring(9)), condition);
                }
            }
        }
    }
    // END: Ethnic group field

## `Gender`

The `Gender` field type inserts a standardised dropdown list using the [gender options recommended in the GOV.UK Design System](https://design-system.service.gov.uk/patterns/gender-or-sex/).