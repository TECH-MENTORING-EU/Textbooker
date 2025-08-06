// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function handleImageUpload(input) {
    const preview = input.closest("section").querySelector(".image-preview-container");
    const imageErrorSpan = input.closest("section").querySelector("small span");
    const files = Array.from(input.files).slice(0, 6);
    const dataTransfer = new DataTransfer();

    preview.innerHTML = "";

    const processingPromises = files.map((file, index) => {
        return new Promise((resolve, reject) => {
            console.log(index);
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
                        if (!blob) return reject("Compression failed");

                        const compressedFile = new File([blob], "canvasblob.jpg", {
                            type: "image/jpeg",
                            lastModified: Date.now()
                        });

                        dataTransfer.items.add(compressedFile);

                        const newReader = new FileReader();
                        newReader.onloadend = () => {
                            console.log("Imagecreation");
                            const imageElement = document.createElement("img");
                            imageElement.src = newReader.result;
                            imageElement.alt = `Podgląd zdjęcia książki numer ${index + 1}.`;
                            imageElement.id = `Image${index + 1}`;
                            imageElement.classList.add("book-image-preview", "active");
                            preview.appendChild(imageElement);
                            imageErrorSpan.style.display = "none";
                            resolve();
                        };
                        newReader.readAsDataURL(compressedFile);
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
        .then(() => {
            input.files = dataTransfer.files;
            applyStyling(files.length, preview);
        })
        .catch(err => {
            console.error("Image upload failed:", err);
            imageErrorSpan.textContent = "Błąd przetwarzania obrazu.";
            imageErrorSpan.style.display = "block";
        });
}

function applyStyling(imageAmount, preview) {
    const images = [];
    for (let i = 1; i <= imageAmount; i++) {
        const img = document.getElementById(`Image${i}`);
        if (img) images.push(img);
    }

    if (images.length < 2) return;

    const mainWrapper = document.createElement('div');
    mainWrapper.style.display = 'flex';
    mainWrapper.style.flexDirection = 'column';
    mainWrapper.style.gap = '10px';
    mainWrapper.style.maxWidth = '800px';
    mainWrapper.style.marginRight = '20px';


    const topWrapper = document.createElement('div');
    topWrapper.style.display = 'flex';
    topWrapper.style.gap = '10px';
    topWrapper.style.height = '300px';


    const mainImageWrapper = document.createElement('div');
    mainImageWrapper.style.width = 'calc(2 * ((100% - 20px) / 3) + 10px)';
    mainImageWrapper.style.position = 'relative';
    mainImageWrapper.style.display = 'flex';

    const mainImage = images[0];
    mainImage.style.width = '100%';
    mainImage.style.height = '100%';
    mainImage.style.objectFit = 'cover';
    mainImage.style.borderRadius = '6px';

    const label = document.createElement('span');
    label.textContent = 'Główny obrazek';
    label.style.position = 'absolute';
    label.style.bottom = '5%';
    label.style.left = '5%';
    label.style.backgroundColor = 'rgba(15,15,15,0.8)';
    label.style.color = 'white';
    label.style.fontSize = '12px';
    label.style.padding = '4px 6px';
    label.style.borderRadius = '4px';

    mainImageWrapper.appendChild(label);
    mainImageWrapper.appendChild(mainImage);


    const sideWrapper = document.createElement('div');
    sideWrapper.style.width = 'calc((100% - 20px) / 3)';
    sideWrapper.style.display = 'flex';
    sideWrapper.style.flexDirection = 'column';
    sideWrapper.style.gap = '10px';

    for (let i = 1; i < Math.min(3, images.length); i++) {
        const img = images[i];
        img.style.width = '100%';
        img.style.height = images.length == 2 ? '100%' : '50%';
        img.style.objectFit = 'cover';
        img.style.borderRadius = '6px';
        sideWrapper.appendChild(img);
    }

    topWrapper.appendChild(mainImageWrapper);
    topWrapper.appendChild(sideWrapper);
    mainWrapper.appendChild(topWrapper);


    if (images.length > 3) {
        const bottomWrapper = document.createElement('div');
        bottomWrapper.style.display = 'flex';
        bottomWrapper.style.gap = '10px';
        bottomWrapper.style.height = '150px';

        const remaining = images.slice(3, 6);
        for (const img of remaining) {
            img.style.width = 'calc((100% - 20px) / 3)';
            img.style.height = '100%';
            img.style.objectFit = 'cover';
            img.style.borderRadius = '6px';
            bottomWrapper.appendChild(img);
        }

        mainWrapper.appendChild(bottomWrapper);
    }

    preview.appendChild(mainWrapper);
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