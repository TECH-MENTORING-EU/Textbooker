(() => {
    function setFilter(name, value, enable) {
        const element = document.querySelector(`[name="${name}"]`);
        if (!element) return;

        element.value = enable ? value : "";
        element.form.requestSubmit();
    }

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
