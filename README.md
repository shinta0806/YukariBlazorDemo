# ゆかり Blazor WebAssembly デモとは？

カラオケ動画をブラウザ上でリクエストをするためツール「[ゆかり](https://github.com/bee7813993/KaraokeRequestorWeb)」が Blazor WebAssembly 化したらこんな感じになるのではないか、なったらいいな、という妄想デモです。

# 動かし方

「ASP.NET と Web 開発」ワークロードをインストールしてある Visual Studio 2019 でソリューションを開き、F5 キーでデバッグ実行すると、ブラウザでデモが動きます。

デバッグ実行時、ブレーク機能を使いたい場合は、ブラウザを Chrome にする必要があります。

Visual Studio 16.8.3 現在、Blazor WebAssembly アプリを何度かデバッグ実行していると、ブラウザが立ち上がらない現象が発生することがあります。その場合は Visual Studio をいったん閉じた後、タスクマネージャーでゾンビになっている Visual Studio を終了してから、再度 Visual Studio を起動すると治ります。

リリースして IIS 下で実行したい場合は YukariBlazorDemo.Server プロジェクトを publish フォルダーに発行します。その後、SampleDataImage フォルダーを publish フォルダー直下にコピーします。

#### 参考リンク
- [Visual Studio で Blazor](https://shinta0806be.ldblog.jp/archives/10326652.html)
- [Blazor WebAssembly アプリ（ASP.NET Core hosted）をフォルダーに発行する](https://shinta0806be.ldblog.jp/archives/10329623.html)

# 使い方

## 予約ゼロ

<img src="Server/Documents/Images/RequestList_Init.png" width="256" align="right">
起動直後は予約一覧ページが表示されますが、まだ曲を予約していないので、内容がありません。



