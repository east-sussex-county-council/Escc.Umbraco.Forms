// By default, when editing an Umbraco Forms field, pressing Enter in the "help text" box closes
// the overlay, meaning the only way to enter multiple paragraphs is to paste them in.
//
// This removes the attribute that makes that happen, as soon as the field receives focus. It
// attaches to the root node of the document so that we don't need to watch the DOM to attach to 
// the element when it is added. It uses the event capturing phase because the focus event doesn't bubble.
if (typeof (jQuery) !== 'undefined') {
    'use strict';
    $(function () {
        if (document.addEventListener) {
            document.documentElement.addEventListener('focus', function (e) {
                if (e.target.getAttribute('ng-model') === 'model.field.tooltip') {
                    e.target.removeAttribute("overlay-submit-on-enter");
                }
            }, true);
        }
    });
}