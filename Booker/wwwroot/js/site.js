// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const processedImagesByInput = new WeakMap();
const uploadStateByInput = new WeakMap();

function getUploadState(input) {
    let state = uploadStateByInput.get(input);
    if (!state) {
        state = {
            committedFiles: processedImagesByInput.get(input) ?? [],
            queue: Promise.resolve(),
            pending: 0
        };
        uploadStateByInput.set(input, state);
    }

    return state;
}

function assignFiles(input, files) {
    const dataTransfer = new DataTransfer();
    files.forEach(file => dataTransfer.items.add(file));
    input.files = dataTransfer.files;
}

function setImageUploadBusy(input, isBusy) {
    input.disabled = isBusy;
    input.setAttribute("aria-busy", isBusy ? "true" : "false");

    const form = input.closest("form");
    if (!form) {
        return;
    }

    if (window.htmx?.trigger) {
        htmx.trigger(form, "image-upload-state", { busy: isBusy });
        return;
    }

    toggleFormSubmitState(form, isBusy);
}

function toggleFormSubmitState(form, isBusy) {
    const controls = Array.from(form.elements).filter(element => {
        if (!(element instanceof HTMLButtonElement || element instanceof HTMLInputElement)) {
            return false;
        }

        return element.type === "submit";
    });

    controls.forEach(element => {
        element.disabled = isBusy;
        element.setAttribute("aria-disabled", isBusy ? "true" : "false");
    });

    const statusElement = form.querySelector("#imageProcessingMsg");
    if (statusElement) {
        statusElement.textContent = isBusy ? "Trwa przetwarzanie zdjęć. Poczekaj chwilę." : "";
    }
}

function renderImagePreview(preview, files) {
    preview.innerHTML = "";
    files.forEach((file, index) => {
        const imageElement = document.createElement("img");
        imageElement.src = URL.createObjectURL(file);
        imageElement.alt = `Zdjęcie książki ${index + 1}`;
        imageElement.classList.add("book-image-preview");
        if (index === 0) {
            imageElement.classList.add("main");
        }
        preview.appendChild(imageElement);
    });
    addLabelToMainImage();
}

function processSelectedImages(selectedFiles) {
    return Promise.all(selectedFiles.map(file => {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = function (e) {
                const img = new Image();
                img.onload = function () {
                    const MAX_WIDTH = 800;
                    const MAX_HEIGHT = 600;
                    let width = img.width;
                    let height = img.height;

                    if (width > height && width > MAX_WIDTH) {
                        height *= MAX_WIDTH / width;
                        width = MAX_WIDTH;
                    } else if (height > MAX_HEIGHT) {
                        width *= MAX_HEIGHT / height;
                        height = MAX_HEIGHT;
                    }

                    const canvas = document.createElement("canvas");
                    canvas.width = width;
                    canvas.height = height;
                    const ctx = canvas.getContext("2d");
                    ctx.drawImage(img, 0, 0, width, height);

                    canvas.toBlob(blob => {
                        if (!blob) return reject(new Error("Compression failed"));

                        const compressedFile = new File([blob], file.name.replace(/\.[^/.]+$/, ".jpg"), {
                            type: "image/jpeg",
                            lastModified: Date.now()
                        });
                        resolve(compressedFile);

                    }, "image/jpeg", 0.8);
                };
                img.onerror = reject;
                img.src = e.target.result;
            };
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    }));
}

function handleImageUpload(input) {
    const preview = input.closest("section").querySelector(".image-preview-container");
    const imageErrorSpan = input.closest("section").querySelector("#imageErrorMsg");
    const selectedFiles = Array.from(input.files);
    const state = getUploadState(input);

    if (selectedFiles.length === 0) {
        return;
    }

    assignFiles(input, state.committedFiles);

    state.pending += 1;
    setImageUploadBusy(input, true);

    const allowedTypes = [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "image/heic",
        "image/heif",
        "image/avif"
    ];

    state.queue = state.queue
        .then(async () => {
            const existingFiles = state.committedFiles;

            const maxImages = 6;
            const remainingSlots = Math.max(0, maxImages - existingFiles.length);
            const acceptedFiles = selectedFiles.slice(0, remainingSlots);
            const rejectedCount = selectedFiles.length - acceptedFiles.length;

            if (acceptedFiles.length === 0) {
                imageErrorSpan.textContent = "Masz już maksymalnie 6 zdjęć. Usuń jedno, aby dodać kolejne.";
                assignFiles(input, existingFiles);
                return;
            }

            for (const file of acceptedFiles) {
                if (!allowedTypes.includes(file.type)) {
                    imageErrorSpan.textContent = (`Plik ${file.name} nie jest obsługiwanym formatem pliku.`);
                    assignFiles(input, existingFiles);
                    return;
                }
            }

            imageErrorSpan.textContent = rejectedCount > 0
                ? `Możesz dodać maksymalnie ${maxImages} zdjęć. Dodano ${acceptedFiles.length}, pominięto ${rejectedCount}.`
                : "";

            const processedFiles = await processSelectedImages(acceptedFiles);
            const allFiles = [...existingFiles, ...processedFiles];

            state.committedFiles = allFiles;
            processedImagesByInput.set(input, allFiles);
            assignFiles(input, allFiles);
            renderImagePreview(preview, allFiles);
        })
        .catch(error => {
            console.error("Error processing images:", error);
            imageErrorSpan.textContent = "Wystąpił błąd podczas przetwarzania zdjęć.";
            assignFiles(input, state.committedFiles);
        })
        .finally(() => {
            state.pending -= 1;
            if (state.pending === 0) {
                setImageUploadBusy(input, false);
            }
        });
}
function addLabelToMainImage() {
    const previewContainer = document.querySelector(".image-preview-container");
    const mainImage = previewContainer.querySelector(".book-image-preview.main");

    if (!mainImage) {
        return;
    }

    const label = document.createElement("span");
    label.textContent = "Główne zdjęcie";
    label.classList.add("image-label--dynamic");

    previewContainer.appendChild(label);

    const top = mainImage.offsetTop + mainImage.offsetHeight - 30;
    const left = mainImage.offsetLeft + 10;

    label.style.top = `${top}px`;
    label.style.left = `${left}px`;
}

function updateCharCount() {
    const count = this.value.length;
    const max = this.getAttribute('maxlength');
    const charCountElement = this.nextElementSibling?.querySelector(".char-count");
    if (charCountElement) {
        charCountElement.textContent = `${count} / ${max}`;
    }
}

function showSummary(event) {
    event.preventDefault(); // Prevent form submission

    if (v.isValid(event.target)) {
        document.getElementById('summaryTitle').textContent = document.getElementById('Input_Title').value;
        document.getElementById('summarySubject').textContent = document.getElementById('Input_Subject').value;
        document.getElementById('summaryGrade').textContent = document.getElementById('Input_Grade').value;
        document.getElementById('summaryLevel').textContent = document.getElementById('Input_Level').value;
        document.getElementById('summaryDescription').textContent = document.getElementById('Input_Description').value || "Brak opisu";
        document.getElementById('summaryState').textContent = document.getElementById('Input_State').value;
        document.getElementById('summaryPrice').textContent = document.getElementById('Input_Price').value + " PLN";

        const firstPreviewImg = document.querySelector('.image-preview-container img');
        if (firstPreviewImg) {
            document.getElementById('summaryImage').src = firstPreviewImg.src;
            document.getElementById('summaryImage').style.display = 'block';
        } else {
            document.getElementById('summaryImage').src = '';
            document.getElementById('summaryImage').style.display = 'none';
        }

        event.target.dataset.inSummary = true;
        const dialog = document.querySelector("main dialog");
        if (dialog) dialog.showModal();
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const hamburgerTrigger = document.getElementById("hamburger-toggle");
    const hamburgerMenu = document.getElementById("hamburger");
    const nav = hamburgerTrigger?.closest("nav");

    if (!hamburgerTrigger || !hamburgerMenu || !nav) {
        return;
    }

    const setOpen = open => {
        nav.classList.toggle("hamburger-open", open);
        hamburgerTrigger.setAttribute("aria-expanded", open ? "true" : "false");

        const accountDetails = hamburgerMenu.querySelector("details.dropdown");
        if (accountDetails) {
            accountDetails.toggleAttribute("open", open);
        }
    };

    hamburgerTrigger.addEventListener("click", () => {
        setOpen(!nav.classList.contains("hamburger-open"));
    });

    // Wired into the shared document-level Escape handler above; Escape is
    // its only trigger, so no separate keydown listener is needed here.
    closeHamburgerMenu = () => {
        if (!nav.classList.contains("hamburger-open")) {
            return;
        }
        setOpen(false);
        hamburgerTrigger.focus();
    };

    hamburgerMenu.querySelectorAll("a, button").forEach(item => {
        item.addEventListener("click", () => setOpen(false));
    });
});

let v = new aspnetValidation.ValidationService();
v.bootstrap({ watch: true });

document.querySelector(".input-validation-error")?.scrollIntoView({ behavior: "smooth" });

document.querySelectorAll(".input-validation-error").forEach(element => {
    element.ariaInvalid = true;
    element.classList.remove("input-validation-error");
});

document.querySelectorAll("button").forEach(button => {
    button.addEventListener("htmx:beforeRequest", function () { this.ariaBusy = true; })
    button.addEventListener("htmx:afterRequest", function () { this.ariaBusy = false; })
});

function getHtmxIndicator(source) {
    const selector = source?.closest("[hx-indicator]")?.getAttribute("hx-indicator");
    return selector ? document.querySelector(selector) : null;
}

document.body.addEventListener("htmx:beforeRequest", event => {
    const indicator = getHtmxIndicator(event.detail.elt);
    if (!indicator) return;
    indicator.setAttribute("aria-busy", "true");
    indicator.textContent = "Ładowanie wyników…";
});

document.body.addEventListener("htmx:afterRequest", event => {
    const indicator = getHtmxIndicator(event.detail.elt);
    if (!indicator) return;
    indicator.setAttribute("aria-busy", "false");
    indicator.textContent = event.detail.successful
        ? "Wyniki zostały zaktualizowane."
        : "Nie udało się zaktualizować wyników. Spróbuj ponownie.";
    window.setTimeout(() => {
        if (indicator.getAttribute("aria-busy") === "false") indicator.textContent = "";
    }, 1000);
});

// ===== Accessibility helpers (WCAG 2.2 AA) =====

// Track the element that opened a dialog so we can return focus to it.
const dialogFocusStack = [];

// Open a dialog identified by data-target on the trigger element.
// Usage: onclick="openDialog(this)"
function openDialog(trigger) {
    const triggerIsDialog = trigger?.tagName === "DIALOG";
    const id = triggerIsDialog ? null : trigger?.getAttribute("data-target");
    const dialog = triggerIsDialog ? trigger : (id ? document.getElementById(id) : null);
    if (!dialog || typeof dialog.showModal !== "function") return;

    dialog.showModal();
    if (!triggerIsDialog && typeof trigger.focus === "function") {
        dialogFocusStack.push(trigger);
    }
    // Move focus to first focusable element inside the dialog.
    focusFirstFocusable(dialog);
}

// Close a dialog from a descendant (e.g. close button inside the dialog).
// Usage: onclick="closeDialog(this)"
function closeDialog(caller) {
    const dialog = caller.closest("dialog");
    if (dialog) {
        dialog.close();
    }
}

// Return focus to the trigger when a dialog closes.
document.addEventListener("close", function (event) {
    if (event.target.tagName !== "DIALOG") return;
    const trigger = dialogFocusStack.pop();
    if (trigger && typeof trigger.focus === "function") {
        trigger.focus();
    }
}, true);

// Set by the hamburger initializer; lets the shared Escape handler close the
// mobile menu without knowing the nav internals. No-op while it is unset or
// the menu is closed.
let closeHamburgerMenu = () => {};

// One delegated Escape handler for every dismissible overlay. It stays at
// document level on purpose: focus can leave an open menu (e.g. tabbing past
// its last item), so an element-scoped listener would stop firing while the
// menu is still open. Native <dialog> handles Escape itself; this is a safety
// net for dialogs and the only Escape handling dropdowns have.
document.addEventListener("keydown", function (event) {
    if (event.key !== "Escape") return;
    document.querySelectorAll("dialog[open]").forEach(d => {
        if (typeof d.close === "function") d.close();
    });
    closeOpenDropdowns({ returnFocus: true });
    closeHamburgerMenu();
});

function focusFirstFocusable(container) {
    const selector = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';
    const candidates = container.querySelectorAll(selector);
    for (const el of candidates) {
        if (el.offsetParent !== null || el === document.activeElement) {
            try { el.focus(); } catch (e) { /* ignore */ }
            return;
        }
    }
}

// Initialize the account-menu dropdown: keep aria-expanded in sync, close it
// on item activation and outside click; Escape comes from the shared
// document-level handler above.
function initDropdownBehavior() {
    document.querySelectorAll("details.dropdown").forEach(details => {
        const summary = details.querySelector("summary");
        if (!summary) return;
        const update = () => summary.setAttribute("aria-expanded", details.hasAttribute("open") ? "true" : "false");
        update();
        details.addEventListener("toggle", update);
        // Close the menu when a menu item is activated.
        details.querySelectorAll("a, button").forEach(item => {
            item.addEventListener("click", () => details.removeAttribute("open"));
        });
    });

    document.addEventListener("click", function (event) {
        // Synthetic clicks dispatched on document/window have a non-Element
        // target without closest(); bail out instead of throwing.
        if (!(event.target instanceof Element)) return;
        // The hamburger trigger toggles the account details itself; treating
        // its click as "outside" would immediately undo the open state.
        if (event.target.closest("#hamburger-toggle")) return;
        // Clicks inside the dropdown (summary toggle, menu items) have their
        // own close handling.
        if (event.target.closest("details.dropdown")) return;

        closeOpenDropdowns();
    });
}

// Close every open dropdown. Returns focus to the summary only when asked
// (Escape): the keyboard flow starts from the summary, while outside clicks
// should leave focus where the user clicked.
function closeOpenDropdowns({ returnFocus = false } = {}) {
    document.querySelectorAll("details.dropdown[open]").forEach(details => {
        const summary = details.querySelector("summary");
        if (returnFocus && summary && details.contains(document.activeElement)) {
            summary.focus();
        }
        details.removeAttribute("open");
    });
}

document.addEventListener("DOMContentLoaded", initDropdownBehavior);
