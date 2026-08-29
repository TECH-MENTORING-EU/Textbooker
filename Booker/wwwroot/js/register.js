(() => {
    const passwordInput = document.querySelector("[data-password-input]");
    const requirements = document.getElementById("passwordRequirements");

    function setRequirement(id, met) {
        const element = document.getElementById(id);
        if (!element) return;
        element.style.color = met ? "var(--pico-ins-color)" : "var(--pico-color)";
        element.innerHTML = `${met ? "&#9989;" : "&#10060;"} ${element.innerHTML.replace(/^.*? /, "")}`;
    }

    function checkPasswordRequirements(password) {
        const checks = [
            ["req-length", password.length >= 8],
            ["req-special", /[^\w\s]/.test(password)],
            ["req-lower", /[a-z]/.test(password)],
            ["req-upper", /[A-Z]/.test(password)],
            ["req-digit", /\d/.test(password)]
        ];
        checks.forEach(([id, met]) => setRequirement(id, met));
        return checks.every(([, met]) => met);
    }

    passwordInput?.addEventListener("focus", () => {
        if (requirements) requirements.style.display = "block";
    });
    passwordInput?.addEventListener("input", event => checkPasswordRequirements(event.currentTarget.value));
    passwordInput?.addEventListener("blur", event => {
        if (requirements && checkPasswordRequirements(event.currentTarget.value)) requirements.style.display = "none";
    });

    const acceptTerms = document.getElementById("acceptTermsCheckbox");
    const submitButton = document.getElementById("registerSubmit");
    const updateSubmitState = () => {
        if (acceptTerms && submitButton) submitButton.disabled = !acceptTerms.checked;
    };
    updateSubmitState();
    acceptTerms?.addEventListener("change", updateSubmitState);

    document.querySelectorAll('#registerForm input[type="checkbox"][required]').forEach(checkbox => {
        checkbox.addEventListener("invalid", () => checkbox.setCustomValidity("Zaznacz to pole, aby kontynuować."));
        checkbox.addEventListener("change", () => checkbox.setCustomValidity(""));
    });
})();
