# Unity Sprite Sheet Capture Tool

A Unity tool for capturing directional motion frames into sprite sheets.

This tool includes external components and assets.  
License information is listed in the [LICENSE](LICENSE) file.

---

## Features

- Frame capture: Captures animation at specified frame intervals and composes them into a sprite sheet.
- Camera control: Adjusts camera direction to capture directional motion and generate sprite sheets.
- Sprite composition: Automatically merges captured cells into a single texture.
- Pose sampling: Uses PlayableGraph for precise pose evaluation.

---

## Requirements

- Unity: 2021.3 or later

---

## Setup

1. In the Hierarchy, select the `SpriteTool` GameObject.
2. In the Inspector, configure the following fields in `SpriteToolContext`:
   - CharacterPrefab: The character prefab to be captured (required)
   - AnimatorControllers: AnimatorController(s) to apply (optional, multiple allowed)
   - AdditionalClips: Additional AnimationClips to capture (optional, multiple allowed)

**Important:** Do not place the character prefab directly in the scene.  
The tool automatically instantiates the character during capture.  
Placing it manually may result in duplicate captures.

---

## UI Operation

The capture process consists of two steps: preview and export.

- Step 1 — Load & Generate: Captures frames and displays a preview image for confirmation.
- Step 2 — Export: After confirming the preview, exports the sprite sheet to a file.

**Output settings:** The export destination can be configured via the UI.

For sample images of the tool interface and output results, see  
[Issue #1: Sample Images](https://github.com/d-tamura123/SpriteTool/issues/1)

---

## Tips

- Instantiation only: Always use `CharacterPrefab`; avoid placing characters manually in the scene.
- Preview check: Review the preview image before exporting to ensure correct output.

---

## License

This tool includes external components and assets.  
License information is listed in the [LICENSE](LICENSE) file.

---

# Unity スプライトシートキャプチャツール

方向別のモーションをスプライトシートとしてキャプチャする Unity ツールです。

本ツールには外部コンポーネントおよびアセットが含まれています。  
ライセンス情報は [LICENSE](LICENSE) に記載しています。

---

## 機能

- フレームキャプチャ：アニメーションを指定したフレーム間隔でキャプチャし、スプライトシートにまとめます。
- カメラ制御：カメラの方向を制御して、方向別でキャプチャし、スプライトシートにまとめます。
- スプライト合成：セル画像を自動で 1 枚のテクスチャに合成します。
- ポーズ：PlayableGraph を用いて、正確なポーズサンプリングを行います。

---

## 動作環境

- Unity：2021.3 以降

---

## セットアップ

1. ヒエラルキーで`SpriteTool` を選択します。
2. インスペクターで `SpriteToolContext` の以下を設定します：
   - CharacterPrefab：キャプチャ対象のキャラクタープレハブ（必須）
   - AnimatorControllers：使用する AnimatorController（任意・複数可）
   - AdditionalClips：追加でキャプチャする AnimationClip（任意・複数可）

重要：キャラクタープレハブをシーンに直接配置しないでください。  
ツールがキャプチャ時に自動生成するため、シーンに配置すると重複キャプチャの原因になります。

---

## ボタン操作

キャプチャは「プレビュー → エクスポート」の 2 段階です。

- ステップ1 — 読み込み・生成：フレームをキャプチャし、確認用プレビュー画像を表示します。
- ステップ2 — エクスポート：プレビューを確認後、スプライトシートをファイルに書き出します。

出力設定：保存先は UI から設定可能です。

ツール画面や出力結果の参考画像については、以下のIssueをご覧ください。  
[Issue #1: Sample Images](https://github.com/d-tamura123/SpriteTool/issues/1)

---

## ヒント

- インスタンス生成前提：`CharacterPrefab` を必ず使用し、手動配置は避けてください。
- プレビュー確認：エクスポート前に確認用プレビュー画像をチェックしてください。

---

## ライセンス

本ツールには外部コンポーネントおよびアセットが含まれています。  
ライセンス情報は [LICENSE](LICENSE) に記載しています。
