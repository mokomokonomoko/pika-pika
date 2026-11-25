(() => {
  const video = document.getElementById("qr-video");
  const frame = document.getElementById("qr-frame");
  const portrait = document.getElementById("qr-portrait");
  const buffer = document.getElementById("qr-canvas");
  const context = buffer.getContext("2d", { willReadFrequently: true });
  let rect = { x: 0.1, y: 0.24, width: 0.8, height: 0.8 };
  let mode = 0;
  let session = 0;
  let stream;
  let timer;
  let lastText = "";
  let lastSentAt = 0;

  function notify(method, value) {
    window.unityInstance?.SendMessage("QRManager", method, value);
  }

  function positionFrame() {
    const width = window.innerWidth;
    const height = window.innerHeight;
    const scale = Math.min(width, height * 0.64 / Math.max(rect.width, rect.height));
    frame.style.left = ((rect.x + rect.width / 2) * width - rect.width * scale / 2) + "px";
    frame.style.top = (rect.y * height) + "px";
    frame.style.width = (rect.width * scale) + "px";
    frame.style.height = (rect.height * scale) + "px";
  }

  function stopTracks(oldStream) {
    oldStream?.getTracks().forEach(track => track.stop());
  }

  function frameReady() {
    return video.readyState >= 2 && video.videoWidth > 0 && video.videoHeight > 0;
  }

  function drawFrame(size) {
    const crop = Math.min(video.videoWidth, video.videoHeight);
    buffer.width = size;
    buffer.height = size;
    context.drawImage(video, (video.videoWidth - crop) / 2, (video.videoHeight - crop) / 2,
      crop, crop, 0, 0, size, size);
  }

  function tick(activeSession) {
    if (activeSession !== session || (mode !== 1 && mode !== 3)) return;
    if (frameReady()) {
      const size = Math.min(512, video.videoWidth, video.videoHeight);
      drawFrame(size);
      const pixels = context.getImageData(0, 0, size, size);
      const result = jsQR(pixels.data, size, size);
      const now = performance.now();
      if (result?.data && (result.data !== lastText || now - lastSentAt >= 2500) && window.unityInstance) {
        lastText = result.data;
        lastSentAt = now;
        notify("OnQRCodeScanned", result.data);
      }
    }
    if (activeSession === session) timer = window.setTimeout(() => tick(activeSession), 115);
  }

  async function setMode(nextMode) {
    const activeSession = ++session;
    window.clearTimeout(timer);
    stopTracks(stream);
    stream = undefined;
    video.srcObject = null;
    frame.hidden = true;
    mode = nextMode;
    portrait.hidden = mode !== 2 || !portrait.hasAttribute("src");
    lastText = "";
    lastSentAt = 0;

    if (!mode) return;
    document.body.classList.toggle("scanner-demo", mode === 3);
    if (!navigator.mediaDevices?.getUserMedia) {
      notify("OnQRCameraStatus", "Open this game over HTTPS to use your camera.");
      return;
    }

    try {
      const opened = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: { ideal: "environment" }, width: { ideal: 640 }, height: { ideal: 640 } },
        audio: false
      });
      if (activeSession !== session) {
        stopTracks(opened);
        return;
      }

      stream = opened;
      video.srcObject = opened;
      await video.play();
      if (activeSession !== session) return;

      frame.hidden = false;
      positionFrame();
      notify("OnQRCameraStatus", "ready");
      tick(activeSession);
    } catch (error) {
      if (activeSession !== session) return;
      stopTracks(stream);
      stream = undefined;
      video.srcObject = null;
      const message = error.name === "NotAllowedError"
        ? "Camera access was declined. Check your browser permissions and try again."
        : "Camera unavailable. Close other camera apps and try again.";
      notify("OnQRCameraStatus", message);
      if (mode === 3) unityShowBanner(message, "error");
    }
  }

  function capturePhoto() {
    const activeSession = session;
    window.setTimeout(() => {
      if (activeSession !== session || mode !== 2) return;
      if (!frameReady()) {
        notify("OnQRCameraPhoto", "");
        return;
      }
      drawFrame(320);
      const jpeg = buffer.toDataURL("image/jpeg", 0.68).split(",")[1];
      notify("OnQRCameraPhoto", jpeg || "");
    }, 0);
  }

  window.addEventListener("resize", positionFrame);
  window.addEventListener("pagehide", () => {
    ++session;
    window.clearTimeout(timer);
    stopTracks(stream);
    stream = undefined;
    video.srcObject = null;
    frame.hidden = true;
    mode = 0;
  });

  window.__qrScanner = {
    start: () => setMode(3),
    setMode,
    capturePhoto,
    setPortrait(png) {
      if (png) portrait.src = "data:image/png;base64," + png;
      else portrait.removeAttribute("src");
    },
    setRect(x, y, width, height) {
      rect = { x, y, width, height };
      positionFrame();
    }
  };
})();
