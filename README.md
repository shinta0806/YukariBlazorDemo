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

## 検索

上部メニューから「検索」をクリックして、検索ページを表示します。

<img src="Server/Documents/Images/SearchForm_AnyWord.png" width="256" align="right">
「なんでも検索」タブが表示されており、ここにキーワードを入力して「検索」ボタンをクリックすることで、動画を検索できます。
> このデモでは実際の動画は検索できず、サンプルデータとして登録されているダミーの動画を検索することになります。

<img src="Server/Documents/Images/SearchResult_Hana.png" width="256" align="right">
例えば「花」でなんでも検索すると、曲名・タイアップ名・歌手名など、何らかの名前に「花」が含まれる動画が検索できます。

検索結果ページはタブが 2 段並んでいますが、上段のタブでキーワードの対象を切り替えることができます。
「曲名」タブをクリックすると、曲名に「花」を含む動画だけを検索することができます。タイアップ名・歌手名も同様です。

下段のタブで検索結果をソートできます。
デフォルトでは「新着順」（動画の更新日が新しい順）に並んでいますが、曲名順・歌手名順・サイズ順（動画のファイルサイズが大きい順）にソートすることができます。

<img src="Server/Documents/Images/SearchForm_Detail.png" width="256" align="right">
検索ページの「詳細検索」タブでは、複合検索ができます。

<img src="Server/Documents/Images/SearchResult_Detail.png" width="256" align="right">
例えば、「タイアップ名」に「花」、「歌手名」に「海」を指定して検索することで、「タイアップ名」に「花」を含み、かつ、「歌手名」に「海」を含む動画だけを絞り込んで検索できます。






