# Escc.Umbraco.Forms.FieldTypes

## Email, phone and URL

There are three new field types for `Email`, `PhoneNumber` and `URL` which are similar to 'Short answer', but use the correct HTML field types to trigger helpful keypads on touch screens. Each one includes a better validation than the 'Validate as email', 'Validate as a number' and 'Validate as a Url' options which come with Umbraco Forms' 'Short answer' field type. 

`URL` supports entering internal URLs which don't have a . in the hostname, and adds a script which inserts `https://` at the start of the entered value if no protocol is entered, so that values like `www.example.org` validate correctly.

## Formatted text 

`FormattedTextField` provides a way for form designers to add some static HTML in the form. It is implemented as a question which does not need an answer.

This works by setting up TinyMCE as a new setting type, so that a setting on a field can use the rich text editor. The new setting type requires a view at `~\App_Plugins\UmbracoForms\BackOffice\Common\SettingTypes\RichTextEditor.html` and a controller loaded by the `~\App_Plugins\EsccUmbracoFormsFieldTypes\*` plugin.

## Ethnic group

`EthnicGroup` inserts a standardised dropdown list of ethnic groups, and an 'other' box which appears if any of the 'other' options are selected. To make this work you need to add code to injects JSON into the page which hooks into Umbraco Forms' own conditional logic. Add the following code to `~\Views\Partials\Forms\Themes\[your-theme]\Script.cshtml` before the line which reads `var jsCommand = ...`. 

	// START: Ethnic group field
    //
    // The 'Ethnic group' field type has two fields in the HTML returned to the client, 
    // and the second is only needed if specific values are selected in the first.
    //
    // Inject extra JSON into the conditions setup for this page of the form to cause the Umbraco Forms 
    // conditional code to handle that for us, so that we can be sure it behaves consistently.
    var ethnicConditions = new List<string>();
    foreach (var fieldset in Model.CurrentPage.Fieldsets)
    {
        foreach (var group in fieldset.Containers)
        {
            foreach (var field in group.Fields)
            {
                if (field.FieldTypeName == "Ethnic group")
                {
                    ethnicConditions.Add("'" + field.Id + @"-other': {
    'id': '00000000-0000-0000-0000-000000000000',
    'actionType': 'Show',
    'logicType': 'Any',
    'rules': [
      {
        'id': '00000000-0000-0000-0000-000000000000',
        'fieldsetId': '" + fieldset.Id + @"',
        'field': '" + field.Id + @"',
        'operator': 'Contains',
        'value': 'other'
      },{
        'id': '00000000-0000-0000-0000-000000000000',
        'fieldsetId': '" + fieldset.Id + @"',
        'field': '" + field.Id + @"',
        'operator': 'Contains',
        'value': 'Other'
      }
    ]
  	}");
                }
            }
        }
    }

    if (ethnicConditions.Count > 0) {
        var fieldConditionsJson = fieldConditions.ToHtmlString();
        var ethnicConditionsJson = String.Join(",", ethnicConditions.ToArray<string>());
        if (fieldConditionsJson == "{}")
        {
            fieldConditions = new HtmlString("{" + ethnicConditionsJson + "}");
        }
        else
        {
            fieldConditions = new HtmlString("{" + ethnicConditionsJson + "," + fieldConditionsJson.Substring(1));
        }
    }
    // END: Ethnic group field

## `Gender`

The `Gender` field type inserts a standardised dropdown list using the [gender options recommended in the GOV.UK Design System](https://design-system.service.gov.uk/patterns/gender-or-sex/).