// Admin user table: lockout and delete dialog handling. The listener lives on
// document.body (event delegation) because htmx replaces table rows when the
// search is used - newly swapped rows must not require the handler to be
// re-attached.
(() => {
    document.body.addEventListener("htmx:afterRequest", (event) => {
        const button = event.detail.elt;
        if (!(button instanceof HTMLElement) || !button.dataset.errorTarget) return;

        if (event.detail.successful) {
            button.closest("dialog")?.close();
            return;
        }

        const errorElement = document.getElementById(button.dataset.errorTarget);
        if (errorElement) {
            errorElement.textContent =
                event.detail.xhr.responseText || "Nie udało się wykonać operacji.";
        }
    });
})();
