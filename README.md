# Avatar Parameters Exclusive Group

パラメーター排他制御を簡単にするツール

## インストール

### VCCによる方法

1. https://vpm.narazaka.net/ から「Add to VCC」ボタンを押してリポジトリをVCCにインストールします。
2. VCCでSettings→Packages→Installed Repositoriesの一覧中で「Narazaka VPM Listing」にチェックが付いていることを確認します。
3. アバタープロジェクトの「Manage Project」から「Avatar Parameters Exclusive Group」をインストールします。

## 使い方

アバター内のGameObjectに「Add Component」ボタンなどから「Avatar Parameters Exclusive Group」コンポーネントを付けて設定します。

## 更新履歴

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
