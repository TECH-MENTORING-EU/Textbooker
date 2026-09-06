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
    const descriptionInput = document.getElementById("Input_Description");
    const sensitiveDescriptionConfirmation = document.getElementById("sensitiveDescriptionConfirmation");
    const confirmSensitiveDescriptionCheckbox = document.getElementById("confirmSensitiveDescriptionCheckbox");
    const sensitiveDescriptionPattern = /[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}|(?:\+?48[\s\-]?)?(?:\d[\s\-]?){9}/;

    document.getElementById("summaryModal")?.addEventListener("close", () => {
        if (addForm) delete addForm.dataset.inSummary;
    });

    confirmButton?.addEventListener("click", event => {
        if (descriptionInput && sensitiveDescriptionPattern.test(descriptionInput.value) && !confirmSensitiveDescriptionCheckbox?.checked) {
            event.preventDefault();
            sensitiveDescriptionConfirmation?.removeAttribute("hidden");
            document.getElementById("summaryModal")?.close();
            confirmSensitiveDescriptionCheckbox?.focus();
            return;
        }

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
