# 景観まちづくり支援ツール 2

## 社内向けの説明

このセクションの説明は社内向けなので、リリース前に削除してください。

- このリポジトリはリリース時に公開されます。
- 開発用 Unity プロジェクトは[こちら](https://github.com/Synesthesias/Landscape-Design-Tool-2-Dev)です。
- 開発環境の構築などの説明は上記リポジトリの README に記載しています。

# ドキュメント作成

## マニュアルの追加

1. [manual](../manual)ディレクトリにマークダウンでページを追加する。
2. 画像を追加したい場合、`Documentation~/resources/ページ名`ディレクトリを作成しファイルを追加する。
3. [toc.yml](../manual/toc.yml)を編集しサイドバーに項目を追加する。

## ローカルでの確認

1. [DocFX をインストール](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html#2-use-docfx-as-a-command-line-tool)する。
2. ビルドしてサーバーを起動する。

```
$ docfx Documentation~/docfx.json --serve
```

3. ブラウザで`http://localhost:8080/`を開く。

## デプロイ

`main`ブランチが更新されたタイミングで Web ページが更新される。
