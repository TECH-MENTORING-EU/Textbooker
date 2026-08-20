(() => {
    function setFilter(name, value, enable) {
        const element = document.querySelector(`[name="${name}"]`);
        if (!element) return;

        element.value = enable ? value : "";
        element.form.requestSubmit();
    }

    // Drop empty-valued parameters so the URL htmx pushes contains only the
    // filters the user actually set; clearing a filter then yields a clean /Browse.
    document.body.addEventListener("htmx:configRequest", (event) => {
        const params = event.detail.parameters;
        for (const name of Array.from(params.keys())) {
            if (!params.get(name)) params.delete(name);
        }
    });

    // The clear button requests /Browse without parameters; blanking the controls
    // here only keeps the visible form in sync with the unfiltered results.
    document.getElementById("clearFilters")?.addEventListener("click", () => {
        const form = document.querySelector(".filter > form");
        form?.querySelectorAll("input, select").forEach(control => {
            control.value = "";
        });
    });

    htmx.onLoad(content => {
        content.querySelectorAll("button.subject, button.grade, button.level").forEach(button => {
            button.addEventListener("click", function () {
                const name = this.classList.contains("subject")
                    ? "subject"
                    : this.classList.contains("grade") ? "grade" : "level";
                const value = name === "grade"
                    ? this.textContent.trim().slice(6, 7)
                    : this.textContent.trim();

                this.classList.toggle("outline");
                setFilter(name, value, !this.classList.contains("outline"));
            });
        });
    });

    document.getElementById("mobileFilterSubmit")?.addEventListener("click", () => {
        document.getElementById("filterDetails")?.removeAttribute("open");
    });
})();
