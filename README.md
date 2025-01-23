# Avatar Parameters Exclusive Group

パラメーター排他制御を簡単にするツール

## インストール

### VCC用インストーラーunitypackageによる方法（VRChatプロジェクトおすすめ）

https://github.com/Narazaka/AvatarParametersExclusiveGroup/releases/latest から `net.narazaka.vrchat.avatar-parameters-exclusive-group-installer.zip` をダウンロードして解凍し、対象のプロジェクトにインポートする。

### VCCによる方法

0. https://modular-avatar.nadena.dev/ja から「ダウンロード（VCC経由）」ボタンを押してリポジトリをVCCにインストールします。
1. [https://vpm.narazaka.net/](https://vpm.narazaka.net/?q=net.narazaka.vrchat.avatar-parameters-exclusive-group) から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. アバタープロジェクトの「Manage Project」から「Avatar Parameters Exclusive Group」をインストールします。

## 使い方

アバター内のGameObjectに「Add Component」ボタンなどから「Avatar Parameters Exclusive Group」コンポーネントを付けて設定します。

## 更新履歴

- 0.5.0
  - 日本語化
  - UIに簡易的な説明を追加
- 0.4.2
  - インストーラーを修正
- 0.4.1
  - 更新履歴等のURLをマニフェストに追加
- 0.4.0
  - Greater / Less のラベルが`>=`等になっていたのを意味的に正しい`>`に修正
  - IntパラメーターについてGreater / Lessを設定したときに正しく戻るように修正
- 0.3.0
  - NDMF Parameter Provider対応
  - VCCのバグで非互換の依存関係をインストールできてしまう問題があるために緊急的にリリースとした物です。安定性が低い可能性があります。
- 0.3.0-rc.1
  - ビルドの問題を修正
- 0.3.0-rc.0
  - NDMF Parameter Provider対応
- 0.2.2
  - VCCでのUnity 2022プロジェクトへのインストールでUnityバージョン警告がでないように
  - 依存関係の更新
- 0.2.1
  - 依存関係の更新
- 0.2.0
  - AvatarParameterDriver 2.x系に対応
- 0.1.0
  - リリース

## License

[Zlib License](LICENSE.txt)
