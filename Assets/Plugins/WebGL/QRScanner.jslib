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
  },

  // x, y, width, height はすべて 0〜1 の割合。詳しい規約はindex.html内のコメント参照。
  SetQRCameraRect: function (x, y, width, height) {
    if (window.__qrScanner) {
      window.__qrScanner.setRect(x, y, width, height);
    } else {
      console.error("QRScanner: window.__qrScanner が見つかりません。WebGL Templateが QRTest になっているか確認してください。");
    }
  }
});
