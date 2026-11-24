mergeInto(LibraryManager.library, {
  // Assets/WebGLTemplates/QRTest/index.html で定義された window.__qrScanner を起動する。
  // カメラ起動とQR解析ループはテンプレート側のJSで完結しており、
  // ここではそれを呼び出すだけにしている。
  StartQRScanner: function () {
    if (window.__qrScanner) {
      window.__qrScanner.start();
    } else {
      console.error("QRScanner: window.__qrScanner が見つかりません。WebGL Templateが QRTest になっているか確認してください。");
    }
  }
});
