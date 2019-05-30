if (typeof (jQuery) !== 'undefined') {
    "use strict";
    $(function () {
        // If someone enters a web address that doesn't start with https?, add the protocol so that
        // we can use the HTML 'url' field type to trigger a helpful keyboard
        $('.umbraco-forms-field.url input[type=text],.umbraco-forms-field.url input[type=url]').on('blur', function () {
            var string = $(this).val();
            if (!string.match(/^https?:/) && string.length) {
                string = "https://" + string;
                $(this).val(string)
            }
        });
    });
}