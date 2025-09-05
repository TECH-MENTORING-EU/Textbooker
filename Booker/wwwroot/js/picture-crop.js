class CustomCropper {
    constructor(canvasId, overlayId) {
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext('2d');
        this.overlay = document.getElementById(overlayId);
        this.image = null;
        this.scale = 1;
        this.minScale = 0.1;
        this.maxScale = 5;
        this.offsetX = 0;
        this.offsetY = 0;
        this.isDragging = false;
        this.lastX = 0;
        this.lastY = 0;
        this.lastPinchDistance = null;
        this.cropRadius = 100;
        this.cssWidth = 0;
        this.cssHeight = 0;
        this.dpr = window.devicePixelRatio || 1;
        this.onChange = null;
        this.rafPending = false;
        this.maskId = `cropMask-${Math.random().toString(36).slice(2)}`;
        this.overlaySvg = null;
        this.maskCircle = null;
        this.cropCircle = null;
        this._onResize = () => {
            if (this.canvas.offsetParent !== null) {
                this.resizeCanvas();
                if (this.image) {
                    this.updateMinScale();
                    this.clampOffsets();
                    this.requestDraw();
                    this.notifyChange();
                }
            }
        };
        this._onMouseDown = (e) => this.startDrag(e);
        this._onMouseMove = (e) => this.drag(e);
        this._onMouseUp = () => this.endDrag();
        this._onMouseLeave = () => this.endDrag();
        this._onWheel = (e) => this.zoom(e);
        this._onTouchStart = (e) => {
            e.preventDefault();
            if (e.touches.length === 1) {
                const touch = e.touches[0];
                this.startDrag({
                    clientX: touch.clientX,
                    clientY: touch.clientY
                });
            }
        };
        this._onTouchMove = (e) => {
            e.preventDefault();
            if (e.touches.length === 1) {
                const touch = e.touches[0];
                this.drag({
                    clientX: touch.clientX,
                    clientY: touch.clientY
                });
            } else if (e.touches.length === 2) {
                this.handlePinchZoom(e);
            }
        };
        this._onTouchEnd = (e) => {
            e.preventDefault();
            this.lastPinchDistance = null;
            this.endDrag();
        };
        this.setupEventListeners();
        this.resizeCanvas();
        this.canvas.style.cursor = 'grab';
        this.ctx.imageSmoothingEnabled = true;
        this.ctx.imageSmoothingQuality = 'high';
        window.addEventListener('resize', this._onResize);
        if (window.ResizeObserver) {
            this.resizeObserver = new ResizeObserver(() => this._onResize());
            if (this.canvas.parentElement) {
                this.resizeObserver.observe(this.canvas.parentElement);
            }
        }
    }
    destroy() {
        this.canvas.removeEventListener('mousedown', this._onMouseDown);
        this.canvas.removeEventListener('mousemove', this._onMouseMove);
        this.canvas.removeEventListener('mouseup', this._onMouseUp);
        this.canvas.removeEventListener('mouseleave', this._onMouseLeave);
        this.canvas.removeEventListener('wheel', this._onWheel);
        this.canvas.removeEventListener('touchstart', this._onTouchStart);
        this.canvas.removeEventListener('touchmove', this._onTouchMove);
        this.canvas.removeEventListener('touchend', this._onTouchEnd);
        window.removeEventListener('resize', this._onResize);
        if (this.resizeObserver) {
            this.resizeObserver.disconnect();
            this.resizeObserver = null;
        }
        if (this.overlay) {
            this.overlay.innerHTML = '';
        }
        this.image = null;
        this.overlaySvg = null;
        this.maskCircle = null;
        this.cropCircle = null;
    }
    notifyChange() {
        if (typeof this.onChange === 'function') this.onChange();
    }
    resizeCanvas() {
        const container = this.canvas.parentElement || this.canvas;
        const containerWidth = container.clientWidth || 300;
        this.cssWidth = containerWidth;
        this.cssHeight = containerWidth;
        this.cropRadius = containerWidth / 4;
        this.dpr = window.devicePixelRatio || 1;
        this.canvas.style.width = this.cssWidth + 'px';
        this.canvas.style.height = this.cssHeight + 'px';
        this.canvas.width = Math.round(this.cssWidth * this.dpr);
        this.canvas.height = Math.round(this.cssHeight * this.dpr);
        this.ctx.setTransform(this.dpr, 0, 0, this.dpr, 0, 0);
        this.ensureOverlay();
        this.updateOverlayGeometry();
        this.requestDraw();
    }
    ensureOverlay() {
        if (!this.overlay) return;
        if (!this.overlaySvg) {
            this.overlay.innerHTML = '';
            const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
            svg.style.width = '100%';
            svg.style.height = '100%';
            svg.style.position = 'absolute';
            svg.style.top = '0';
            svg.style.left = '0';
            svg.style.pointerEvents = 'none';
            const defs = document.createElementNS('http://www.w3.org/2000/svg', 'defs');
            const mask = document.createElementNS('http://www.w3.org/2000/svg', 'mask');
            mask.setAttribute('id', this.maskId);
            const maskRect = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
            maskRect.setAttribute('width', '100%');
            maskRect.setAttribute('height', '100%');
            maskRect.setAttribute('fill', 'white');
            const maskCircle = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
            maskCircle.setAttribute('fill', 'black');
            mask.appendChild(maskRect);
            mask.appendChild(maskCircle);
            defs.appendChild(mask);
            const overlayRect = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
            overlayRect.setAttribute('width', '100%');
            overlayRect.setAttribute('height', '100%');
            overlayRect.setAttribute('fill', 'rgba(0, 0, 0, 0.5)');
            overlayRect.setAttribute('mask', `url(#${this.maskId})`);
            const cropCircle = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
            cropCircle.setAttribute('fill', 'none');
            cropCircle.setAttribute('stroke', 'white');
            cropCircle.setAttribute('stroke-width', '2');
            cropCircle.setAttribute('stroke-dasharray', '5,5');
            svg.appendChild(defs);
            svg.appendChild(overlayRect);
            svg.appendChild(cropCircle);
            this.overlay.appendChild(svg);
            this.overlaySvg = svg;
            this.maskCircle = maskCircle;
            this.cropCircle = cropCircle;
        }
    }
    updateOverlayGeometry() {
        if (!this.overlaySvg) return;
        const canvasWidth = this.cssWidth;
        const canvasHeight = this.cssHeight;
        const centerX = canvasWidth / 2;
        const centerY = canvasHeight / 2;
        this.overlaySvg.setAttribute('viewBox', `0 0 ${canvasWidth} ${canvasHeight}`);
        if (this.maskCircle) {
            this.maskCircle.setAttribute('cx', centerX);
            this.maskCircle.setAttribute('cy', centerY);
            this.maskCircle.setAttribute('r', this.cropRadius);
        }
        if (this.cropCircle) {
            this.cropCircle.setAttribute('cx', centerX);
            this.cropCircle.setAttribute('cy', centerY);
            this.cropCircle.setAttribute('r', this.cropRadius);
        }
    }
    setupEventListeners() {
        this.canvas.addEventListener('mousedown', this._onMouseDown);
        this.canvas.addEventListener('mousemove', this._onMouseMove);
        this.canvas.addEventListener('mouseup', this._onMouseUp);
        this.canvas.addEventListener('mouseleave', this._onMouseLeave);
        this.canvas.addEventListener('wheel', this._onWheel, {
            passive: false
        });
        this.canvas.addEventListener('touchstart', this._onTouchStart, {
            passive: false
        });
        this.canvas.addEventListener('touchmove', this._onTouchMove, {
            passive: false
        });
        this.canvas.addEventListener('touchend', this._onTouchEnd, {
            passive: false
        });
    }
    loadImage(src) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => {
                this.image = img;
                this.resetTransform();
                this.requestDraw();
                this.notifyChange();
                resolve();
            };
            img.onerror = reject;
            img.src = src;
        });
    }
    updateMinScale() {
        if (!this.image) return;
        const minCoverScale = Math.max((2 * this.cropRadius) / this.image.width, (2 * this.cropRadius) / this.image.height);
        this.minScale = Math.max(0.1, minCoverScale);
        if (this.scale < this.minScale) {
            this.scale = this.minScale;
        }
    }
    resetTransform() {
        if (!this.image) return;
        const canvasWidth = this.cssWidth;
        const canvasHeight = this.cssHeight;
        const imageWidth = this.image.width;
        const imageHeight = this.image.height;
        const scaleX = canvasWidth / imageWidth;
        const scaleY = canvasHeight / imageHeight;
        this.scale = Math.max(scaleX, scaleY);
        this.updateMinScale();
        this.scale = Math.max(this.scale, this.minScale);
        this.offsetX = 0;
        this.offsetY = 0;
        this.clampOffsets();
        this.requestDraw();
        this.notifyChange();
    }
    clampOffsets() {
        if (!this.image) return;
        const canvasWidth = this.cssWidth;
        const canvasHeight = this.cssHeight;
        const scaledWidth = this.image.width * this.scale;
        const scaledHeight = this.image.height * this.scale;
        const centerX = canvasWidth / 2;
        const centerY = canvasHeight / 2;
        const r = this.cropRadius;
        const halfW = (canvasWidth - scaledWidth) / 2;
        const halfH = (canvasHeight - scaledHeight) / 2;
        const minOffsetX = (centerX + r - scaledWidth) - halfW;
        const maxOffsetX = (centerX - r) - halfW;
        const minOffsetY = (centerY + r - scaledHeight) - halfH;
        const maxOffsetY = (centerY - r) - halfH;
        this.offsetX = Math.min(Math.max(this.offsetX, minOffsetX), maxOffsetX);
        this.offsetY = Math.min(Math.max(this.offsetY, minOffsetY), maxOffsetY);
    }
    getMousePos(e) {
        const rect = this.canvas.getBoundingClientRect();
        return {
            x: e.clientX - rect.left,
            y: e.clientY - rect.top
        };
    }
    startDrag(e) {
        this.isDragging = true;
        const pos = this.getMousePos(e);
        this.lastX = pos.x;
        this.lastY = pos.y;
        this.canvas.style.cursor = 'grabbing';
    }
    drag(e) {
        if (!this.isDragging || !this.image) return;
        const pos = this.getMousePos(e);
        const deltaX = pos.x - this.lastX;
        const deltaY = pos.y - this.lastY;
        this.offsetX += deltaX;
        this.offsetY += deltaY;
        this.lastX = pos.x;
        this.lastY = pos.y;
        this.clampOffsets();
        this.requestDraw();
        this.notifyChange();
    }
    endDrag() {
        if (!this.isDragging) return;
        this.isDragging = false;
        this.canvas.style.cursor = 'grab';
        this.notifyChange();
    }
    zoom(e) {
        e.preventDefault();
        if (!this.image) return;
        const zoomFactor = e.deltaY > 0 ? 0.9 : 1.1;
        this.setZoom(zoomFactor);
    }
    setZoom(factor) {
        if (!this.image) return;
        const newScale = Math.max(this.minScale, Math.min(this.maxScale, this.scale * factor));
        if (newScale !== this.scale) {
            this.scale = newScale;
            this.clampOffsets();
            this.requestDraw();
            this.notifyChange();
        }
    }
    requestDraw() {
        if (this.rafPending) return;
        this.rafPending = true;
        requestAnimationFrame(() => {
            this.rafPending = false;
            this.draw();
        });
    }
    draw() {
        if (!this.image) {
            this.ctx.clearRect(0, 0, this.cssWidth, this.cssHeight);
            return;
        }
        const canvasWidth = this.cssWidth;
        const canvasHeight = this.cssHeight;
        this.ctx.clearRect(0, 0, canvasWidth, canvasHeight);
        const scaledWidth = this.image.width * this.scale;
        const scaledHeight = this.image.height * this.scale;
        const x = (canvasWidth - scaledWidth) / 2 + this.offsetX;
        const y = (canvasHeight - scaledHeight) / 2 + this.offsetY;
        this.ctx.drawImage(this.image, x, y, scaledWidth, scaledHeight);
    }
    getCroppedImage(outputSize = 400) {
        if (!this.image) return null;
        const canvasWidth = this.cssWidth;
        const canvasHeight = this.cssHeight;
        const centerX = canvasWidth / 2;
        const centerY = canvasHeight / 2;
        const tempCanvas = document.createElement('canvas');
        const tempCtx = tempCanvas.getContext('2d');
        tempCanvas.width = outputSize;
        tempCanvas.height = outputSize;
        tempCtx.imageSmoothingEnabled = true;
        tempCtx.imageSmoothingQuality = 'high';
        tempCtx.fillStyle = '#ffffff';
        tempCtx.fillRect(0, 0, outputSize, outputSize);
        const scaledImageWidth = this.image.width * this.scale;
        const scaledImageHeight = this.image.height * this.scale;
        const imageXOnCanvas = (canvasWidth - scaledImageWidth) / 2 + this.offsetX;
        const imageYOnCanvas = (canvasHeight - scaledImageHeight) / 2 + this.offsetY;
        const sourceX = (centerX - this.cropRadius - imageXOnCanvas) / this.scale;
        const sourceY = (centerY - this.cropRadius - imageYOnCanvas) / this.scale;
        const sourceW = (this.cropRadius * 2) / this.scale;
        const sourceH = (this.cropRadius * 2) / this.scale;
        tempCtx.save();
        tempCtx.beginPath();
        tempCtx.arc(outputSize / 2, outputSize / 2, outputSize / 2, 0, Math.PI * 2);
        tempCtx.clip();
        tempCtx.drawImage(this.image, sourceX, sourceY, sourceW, sourceH, 0, 0, outputSize, outputSize);
        tempCtx.restore();
        return tempCanvas;
    }
    getCroppedBlob(outputSize = 400, quality = 0.9) {
        return new Promise((resolve) => {
            const croppedCanvas = this.getCroppedImage(outputSize);
            if (!croppedCanvas) {
                resolve(null);
                return;
            }
            const q = Math.max(0, Math.min(1, quality ?? 0.9));
            croppedCanvas.toBlob(resolve, 'image/jpeg', q);
        });
    }
    handlePinchZoom(e) {
        const touch1 = e.touches[0];
        const touch2 = e.touches[1];
        const distance = Math.hypot(touch2.clientX - touch1.clientX, touch2.clientY - touch1.clientY);
        if (!this.lastPinchDistance) {
            this.lastPinchDistance = distance;
            return;
        }
        const scale = distance / this.lastPinchDistance;
        this.setZoom(scale);
        this.lastPinchDistance = distance;
    }
}
let customCropper = null;
let currentPreviewUrl = null;
let lastUpdateToken = 0;
let controlsInitialized = false;
const editPhotoModal = document.getElementById('editPhotoModal');
const uploadStep = document.getElementById('uploadStep');
const cropperStep = document.getElementById('cropperStep');
const imageInput = document.getElementById('imageInput');
const currentProfilePictureDisplay = document.getElementById('currentProfilePictureDisplay');
const profilePictureForm = document.getElementById('profilePictureForm');
const submitButton = document.getElementById('submitButton');
const cropPreview = document.getElementById('cropPreview');

function debounce(fn, delay = 150) {
    let t;
    return (...args) => {
        clearTimeout(t);
        t = setTimeout(() => fn(...args), delay);
    };
}

function nextFrame() {
    return new Promise(resolve => requestAnimationFrame(resolve));
}

function showStep(stepName) {
    if (uploadStep) uploadStep.style.display = (stepName === 'upload') ? 'block' : 'none';
    if (cropperStep) cropperStep.style.display = (stepName === 'cropper') ? 'block' : 'none';
    if (submitButton) submitButton.disabled = (stepName === 'upload');
}
async function updateCroppedBlob() {
    if (!customCropper || !customCropper.image) return;
    const token = ++lastUpdateToken;
    try {
        if (cropPreview) {
            const blob = await customCropper.getCroppedBlob(400, 0.9);
            if (token !== lastUpdateToken) return;
            if (blob) {
                if (currentPreviewUrl) {
                    URL.revokeObjectURL(currentPreviewUrl);
                    currentPreviewUrl = null;
                }
                currentPreviewUrl = URL.createObjectURL(blob);
                cropPreview.src = currentPreviewUrl;
            }
        }
        if (submitButton) submitButton.disabled = false;
    } catch (error) {
        console.error('Error getting cropped image:', error);
    }
}
const updateCroppedBlobDebounced = debounce(updateCroppedBlob, 150);

function resetState() {
    if (imageInput) imageInput.value = '';
    if (currentPreviewUrl) {
        URL.revokeObjectURL(currentPreviewUrl);
        currentPreviewUrl = null;
    }
    if (cropPreview) cropPreview.src = '';
    if (customCropper) {
        customCropper.destroy();
        customCropper = null;
    }
    showStep('upload');
}

function displayMessage(message, type) {
    const container = document.getElementById('messages');
    if (!container) return;
    const bgColor = type === 'danger' ? 'var(--pico-color-red-100)' : 'var(--pico-color-green-100)';
    const textColor = type === 'danger' ? 'var(--pico-color-red-900)' : 'var(--pico-color-green-900)';
    container.innerHTML = `<div role="alert" style="background-color: ${bgColor}; color: ${textColor}; padding: 1rem; border-radius: var(--pico-border-radius); margin: 1rem 0;">${message}</div>`;
}
window.openModal = function () {
    editPhotoModal.showModal();
    resetState();
};
window.closeModal = function () {
    editPhotoModal.close();
    if (customCropper) {
        customCropper.destroy();
        customCropper = null;
    }
    if (currentPreviewUrl) {
        URL.revokeObjectURL(currentPreviewUrl);
        currentPreviewUrl = null;
    }
    if (imageInput) imageInput.value = '';
    showStep('upload');
};

function setupCropperControls() {
    if (controlsInitialized) return;
    controlsInitialized = true;
    const zoomInButton = document.getElementById('zoomInButton');
    const zoomOutButton = document.getElementById('zoomOutButton');
    const resetButton = document.getElementById('resetButton');
    if (zoomInButton) {
        zoomInButton.addEventListener('click', () => {
            if (customCropper) {
                customCropper.setZoom(1.1);
                updateCroppedBlobDebounced();
            }
        });
    }
    if (zoomOutButton) {
        zoomOutButton.addEventListener('click', () => {
            if (customCropper) {
                customCropper.setZoom(0.9);
                updateCroppedBlobDebounced();
            }
        });
    }
    if (resetButton) {
        resetButton.addEventListener('click', () => {
            if (customCropper) {
                customCropper.resetTransform();
                updateCroppedBlobDebounced();
            }
        });
    }
}
async function refreshCurrentProfilePicture(freshBlob) {
    if (!currentProfilePictureDisplay) return;
    const base = currentProfilePictureDisplay.dataset.baseSrc || currentProfilePictureDisplay.src;
    let tempUrl = null;
    if (freshBlob) {
        tempUrl = URL.createObjectURL(freshBlob);
        currentProfilePictureDisplay.src = tempUrl;
    }
    try {
        const baseUrl = new URL(base, window.location.href);
        const sameOrigin = baseUrl.origin === window.location.origin;
        if (!sameOrigin) {
            const busted = addCacheBust(base);
            await preloadImage(busted);
            cleanupObjectUrl(currentProfilePictureDisplay.dataset.objectUrl);
            if (tempUrl) cleanupObjectUrl(tempUrl);
            currentProfilePictureDisplay.src = busted;
            delete currentProfilePictureDisplay.dataset.objectUrl;
        } else {
            const resp = await fetch(base, {
                cache: 'reload',
                credentials: 'include'
            });
            if (!resp.ok) throw new Error('HTTP ' + resp.status);
            const serverBlob = await resp.blob();
            const serverUrl = URL.createObjectURL(serverBlob);
            cleanupObjectUrl(currentProfilePictureDisplay.dataset.objectUrl);
            if (tempUrl) cleanupObjectUrl(tempUrl);
            currentProfilePictureDisplay.src = serverUrl;
            currentProfilePictureDisplay.dataset.objectUrl = serverUrl;
        }
    } catch (err) {
        console.warn('Nie udało się odświeżyć obrazu z serwera, zostaję przy lokalnym blobie:', err);
        if (tempUrl) {
            cleanupObjectUrl(currentProfilePictureDisplay.dataset.objectUrl);
            currentProfilePictureDisplay.dataset.objectUrl = tempUrl;
        }
    }

    function cleanupObjectUrl(url) {
        if (url && typeof url === 'string' && url.startsWith('blob:')) {
            try {
                URL.revokeObjectURL(url);
            } catch { }
        }
    }

    function addCacheBust(url) {
        try {
            const u = new URL(url, window.location.href);
            u.searchParams.set('v', Date.now());
            return u.toString();
        } catch {
            const sep = url.includes('?') ? '&' : '?';
            return url + sep + 'v=' + Date.now();
        }
    }

    function preloadImage(src) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve();
            img.onerror = reject;
            img.src = src;
        });
    }
}
if (imageInput) {
    imageInput.addEventListener('change', async function () {
        const file = this.files[0];
        if (!file || !file.type.startsWith('image/')) {
            displayMessage('Proszę wybrać plik obrazu.', 'danger');
            imageInput.value = '';
            resetState();
            return;
        }
        try {
            const reader = new FileReader();
            reader.onload = async (e) => {
                showStep('cropper');
                await nextFrame();
                try {
                    if (customCropper) {
                        customCropper.destroy();
                        customCropper = null;
                    }
                    customCropper = new CustomCropper('cropperCanvas', 'cropperOverlay');
                    customCropper.onChange = updateCroppedBlobDebounced;
                    customCropper.resizeCanvas();
                    await customCropper.loadImage(e.target.result);
                    setupCropperControls();
                    updateCroppedBlobDebounced();
                } catch (error) {
                    console.error('Error initializing cropper:', error);
                    displayMessage('Błąd podczas inicjalizacji croppera. Spróbuj ponownie.', 'danger');
                    resetState();
                }
            };
            reader.onerror = (error) => {
                console.error('FileReader error:', error);
                displayMessage('Błąd podczas czytania pliku. Spróbuj ponownie.', 'danger');
                resetState();
            };
            reader.readAsDataURL(file);
        } catch (error) {
            console.error('Error loading image:', error);
            displayMessage('Błąd podczas ładowania obrazu. Spróbuj ponownie.', 'danger');
            resetState();
        }
    });
}
if (profilePictureForm) {
    profilePictureForm.addEventListener('submit', async function (event) {
        event.preventDefault();
        if (!customCropper) {
            displayMessage('Proszę wybrać i dostosować zdjęcie przed przesłaniem.', 'danger');
            return;
        }
        try {
            const freshBlob = await customCropper.getCroppedBlob(400, 0.9);
            if (!freshBlob) {
                displayMessage('Proszę wybrać i dostosować zdjęcie przed przesłaniem.', 'danger');
                return;
            }
            const formData = new FormData();
            formData.append('Input.Image', freshBlob, 'profile_picture.jpeg');
            const antiForgery = document.querySelector('input[name="__RequestVerificationToken"]');
            if (antiForgery && antiForgery.value) {
                formData.append('__RequestVerificationToken', antiForgery.value);
            }
            submitButton.disabled = true;
            submitButton.textContent = 'Zapisywanie...';
            const response = await fetch(this.action, {
                method: 'POST',
                body: formData
            });
            if (!response.ok) throw new Error('Network response was not ok');
            await response.text();
            closeModal();
            await refreshCurrentProfilePicture(freshBlob);
            displayMessage('Zdjęcie profilowe zostało zaktualizowane🥳 Wyglądasz ślicznie😉', 'success');
        } catch (error) {
            console.error('Error:', error);
            displayMessage('Wystąpił błąd podczas zapisywania zdjęcia. Spróbuj ponownie.', 'danger');
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = submitButton.dataset.defaultText || 'Prześlij i zapisz zdjęcie 💾';
        }
    });
}
document.addEventListener('DOMContentLoaded', () => {
    if (currentProfilePictureDisplay) {
        currentProfilePictureDisplay.dataset.baseSrc = currentProfilePictureDisplay.src;
    }
    if (submitButton) {
        submitButton.dataset.defaultText = submitButton.textContent;
    }
    if (new URLSearchParams(window.location.search).get('openModal') === 'true') {
        openModal();
    }
});