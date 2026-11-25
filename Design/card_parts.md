# 着せ替えカード(パーツ画像)の仕様

カード番号・画像・重ね順の対応表です。データ本体は `Assets/Cards/Data/cards.json`(同じ内容の表が `cards.csv`)。

## 画像のルール
- すべての画像は **2048 × 2048 の同じキャンバス**で、素体(`Assets/UI/character/body_base.png`)と位置が合わせてあります。
- 同じ大きさ・同じ位置の Image を重ねるだけで、正しく着せ替わります(位置調整は不要)。
- 背景は透明です。
- WebGL のメモリ節約のため、インポート時の Max Size は 1024 程度を推奨します。

## 重ね順(order が小さいほど奥)
| order | layer | 内容 |
|---|---|---|
| 10 | Tail | しっぽ |
| 20 | HairBack | 後ろ髪(Himeganimal のヘアのみ) |
| 30 | Body | 素体(カードなし) |
| 40 | Shoes | シューズ |
| 50 | Bottom | ボトムス |
| 60 | Top | トップス |
| 70 | Face | フェイス |
| 80 | HairFront | 前髪・ヘア |
| 90 | Ears | 耳 |
| 100 | Accessory | アクセ |

## カード番号(QR の中身)
番号は「セット番号 + 部位番号(下2桁)」です。既存の CARD:0001〜0030 とは重なりません。

| 部位 | Ganimals | Himeganimal | Set03(仮名) |
|---|---|---|---|
| ヘア | CARD:0101 | CARD:0201(前髪 + 後ろ髪の2枚) | CARD:0301 |
| フェイス | CARD:0102 | CARD:0202 | CARD:0302 |
| 耳 | CARD:0103 | CARD:0203 | なし |
| アクセ | CARD:0104 | CARD:0204 | CARD:0304 |
| トップス | CARD:0105 | CARD:0205 | CARD:0305 |
| ボトムス | CARD:0106 | CARD:0206 | CARD:0306(スカート) |
| シューズ | CARD:0107 | CARD:0207 | CARD:0307(靴下と靴) |
| しっぽ | CARD:0108 | CARD:0208 | なし |

部位番号(下2桁)はセットが変わっても同じです。Set03 には耳としっぽがないので、0303 と 0308 は欠番です。新しいセットを追加するときは、CARD:0401〜0408 のように続けます。

## 実装時の注意
- CARD:0201(Himeganimal のヘア)は **画像が2枚**(前髪 `HairFront`、後ろ髪 `HairBack`)です。1枚のカードで2つの層を同時に切り替えてください。
- 今の `CardDefinition.cs` の `CardSlot`(Ears, Tail, Markings, Top, Bottom, Accessory)には、**Hair・Face・Shoes がありません**。また、Markings に当たるパーツはありません。
- `displayName` は仮の名前です(例: "Ganimals Hair")。正式な名前が決まったら差し替えてください。
- 素体にはカードはありません。
- Set03 は元の画像が 1180×1766 のキャンバスだったため、素体の 2048×2048 キャンバスに合わせて配置し直しました(倍率はそのまま、右に 420px・上に 48px 移動。腕のブレスレットの位置を目印に合わせています)。ずれが気になる場合は、元の絵を描いた人に 2048×2048 で書き出し直してもらうのが確実です。
- Set03 の名前は仮です(元のフォルダ名は GAMEJAM)。

## ファイル
- パーツ画像: `Assets/Cards/Parts/Ganimals/`、`Assets/Cards/Parts/Himeganimal/`、`Assets/Cards/Parts/Set03/`
- データ: `Assets/Cards/Data/cards.json`、`Assets/Cards/Data/cards.csv`
- 印刷用 QR: `Design/qr/ganimals_qr_cards.pdf`(1枚ずつの QR は `Design/qr/qr_XXXX.png`)
- 着せ替えた見本: `Design/preview_sets.png`
