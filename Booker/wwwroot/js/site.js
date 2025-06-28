// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function handleImageUpload(input) {
    const preview = input.closest("section").querySelector("img")
    const file = input.files[0];
    const imageErrorSpan = input.closest("section").querySelector("small span");

    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const img = new Image();
            img.onload = function () {
                const MAX_WIDTH = 800;
                const MAX_HEIGHT = 600;
                let width = img.width;
                let height = img.height;

                if (width > height) {
                    if (width > MAX_WIDTH) {
                        height *= MAX_WIDTH / width;
                        width = MAX_WIDTH;
                    }
                } else {
                    if (height > MAX_HEIGHT) {
                        width *= MAX_HEIGHT / height;
                        height = MAX_HEIGHT;
                    }
                }

                const canvas = document.createElement('canvas');
                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0, width, height);

                canvas.toBlob((blob) => {
                    const compressedFile = new File([blob], "canvasblob.jpg", {
                        type: 'image/jpeg',
                        lastModified: Date.now()
                    });

                    const dataTransfer = new DataTransfer();
                    dataTransfer.items.add(compressedFile);
                    input.files = dataTransfer.files;

                    const newReader = new FileReader();
                    newReader.onloadend = () => {
                        preview.src = newReader.result;
                        preview.classList.add('active');
                        imageErrorSpan.style.display = 'none';
                    };
                    newReader.readAsDataURL(compressedFile);

                }, 'image/jpeg', 0.8);
            };
            img.src = e.target.result;
        };
        reader.readAsDataURL(file);
    } else {
        preview.src = "#";
        preview.classList.remove('active');
    }
}

function showSummary(event) {
    event.preventDefault(); // Prevent form submission for validation
    if (v.isValid(event.target)) {
        document.getElementById('summaryTitle').textContent = document.getElementById('Input_Title').value;
        document.getElementById('summarySubject').textContent = document.getElementById('Input_Subject').value;
        document.getElementById('summaryGrade').textContent = document.getElementById('Input_Grade').value;
        document.getElementById('summaryLevel').textContent = document.getElementById('Input_Level').value;
        document.getElementById('summaryDescription').textContent = document.getElementById('Input_Description').value || "Brak opisu";
        document.getElementById('summaryState').textContent = document.getElementById('Input_State').value;
        document.getElementById('summaryPrice').textContent = document.getElementById('Input_Price').value + " PLN";

        const bookImagePreviewSrc = document.getElementById('Input_Image').closest('section').querySelector('img').src;
        if (document.getElementById('Input_Image').closest('section').querySelector('img').classList.contains('active') && bookImagePreviewSrc && bookImagePreviewSrc !== window.location.href + '#') {
            document.getElementById('summaryImage').src = bookImagePreviewSrc;
            document.getElementById('summaryImage').style.display = 'block';
        } else {
            document.getElementById('summaryImage').style.display = 'none';
            document.getElementById('summaryImage').src = "";
        }

        event.target.dataset.inSummary = true;
        document.querySelector("main dialog").showModal();
    }
}

let v = new aspnetValidation.ValidationService();
v.bootstrap({ watch: true });

document.querySelectorAll("button").forEach(button => {
    button.addEventListener("htmx:beforeRequest", function () { this.ariaBusy = true; })
    button.addEventListener("htmx:afterRequest", function () { this.ariaBusy = false; })
});