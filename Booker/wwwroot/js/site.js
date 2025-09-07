// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function handleImageUpload(input) {
    const preview = input.closest("section").querySelector(".image-preview-container");
    preview.innerHTML = ""; // Clear existing previews
    const imageErrorSpan = input.closest("section").querySelector("#imageErrorMsg");

    if (input.files.length > 6) {
        imageErrorSpan.textContent = "Możesz dodać maksymalnie 6 zdjęć.";
        return;
    }
    imageErrorSpan.textContent = "";

    const files = Array.from(input.files);
    const dataTransfer = new DataTransfer();

    const processingPromises = files.map((file, index) => {
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
                        dataTransfer.items.add(compressedFile);

                        
                        const imageElement = document.createElement("img");
                        imageElement.src = URL.createObjectURL(compressedFile);
                        imageElement.alt = `Zdjęcie książki ${index + 1}`;
                        imageElement.classList.add("book-image-preview");
                        if (index === 0) {
                            imageElement.classList.add("main");
                        }

                        resolve(imageElement);

                    }, "image/jpeg", 0.8);
                };
                img.onerror = reject;
                img.src = e.target.result;
            };
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    });

    Promise.all(processingPromises)
        .then(imageElements => {
            imageElements.forEach(img => {
                preview.appendChild(img);
            });
            input.files = dataTransfer.files;
            addLabelToMainImage();
        })
        .catch(error => {
            console.error("Error processing images:", error);
            imageErrorSpan.textContent = "Wystąpił błąd podczas przetwarzania zdjęć.";
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





function toggleHamburgerMenu(check) {
    const hamburger = document.getElementById('hamburger').querySelector('details');
    if (hamburger == null) {
        return;
    }
    if (check.checked) {
        hamburger.setAttribute("open", "");
    } else {
        hamburger.removeAttribute("open");
    }
}

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