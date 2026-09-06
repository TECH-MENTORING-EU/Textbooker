// Admin user table: lockout dialog handling. The listener lives on document.body
// (event delegation) because htmx replaces table rows when the search is used -
// newly swapped rows must not require the handler to be re-attached.
(() => {
    document.body.addEventListener("htmx:afterRequest", (event) => {
        const button = event.detail.elt;
        if (!(button instanceof HTMLElement) || button.id !== "confirmLockout") return;

        if (event.detail.successful) {
            button.closest("dialog")?.close();
            return;
        }

        const errorId = button.dataset.errorTarget;
        const errorElement = errorId ? document.getElementById(errorId) : null;
        if (errorElement) {
            errorElement.textContent =
                event.detail.xhr.responseText || "Nie udało się zablokować użytkownika.";
        }
    });
})();
