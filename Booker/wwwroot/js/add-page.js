(() => {
    document.querySelectorAll("textarea").forEach(textarea => {
        const updateCount = () => {
            const counter = textarea.nextElementSibling?.querySelector(".char-count");
            if (counter) counter.textContent = `${textarea.value.length} / ${textarea.maxLength || 200}`;
        };
        textarea.addEventListener("input", updateCount);
        updateCount();
    });

    let isSubmitting = false;
    const confirmButton = document.getElementById("confirmAddBtn");
    const addForm = document.getElementById("add-form");
    document.getElementById("summaryModal")?.addEventListener("close", () => {
        if (addForm) delete addForm.dataset.inSummary;
    });

    confirmButton?.addEventListener("click", event => {
        if (isSubmitting) {
            event.preventDefault();
            return;
        }
        isSubmitting = true;
        setTimeout(() => {
            confirmButton.disabled = true;
            confirmButton.textContent = "Dodawanie...";
        }, 0);
    });
})();
