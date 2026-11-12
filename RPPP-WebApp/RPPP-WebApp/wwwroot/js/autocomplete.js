function initAutocomplete(context) {

    var root = context ? $(context) : $(document);

    root.find("[data-autocomplete]").each(function () {

        var element = $(this);

        if (element.data("autocomplete-initialized")) {
            return;
        }

        element.data("autocomplete-initialized", true);

        var action = element.data("autocomplete");
        var placeholderName =
            element.data("autocomplete-placeholder-name") ?? action;

        var dest = $(
            "[data-autocomplete-placeholder='" + placeholderName + "']"
        );

        var phoneTarget = element.data("phone-target");

        element.on("change", function () {
            var text = element.val();
            if (!text || text !== dest.data("selected-label")) {
                dest.val("");
                dest.removeData("selected-label");

                if (phoneTarget) {
                    $("#" + phoneTarget).text("");
                }
            }
        });

        element.autocomplete({
            source: window.applicationBaseUrl + "autocomplete/" + action,
            autoFocus: true,
            minLength: 1,

            select: function (event, ui) {

                element.val(ui.item.label);
                dest.val(ui.item.id);
                dest.data("selected-label", ui.item.label);

                if (phoneTarget && ui.item.phone) {
                    $("#" + phoneTarget).text(ui.item.phone);
                }

                return false;
            }
        });
    });
}

$(function () {
    initAutocomplete();
});
