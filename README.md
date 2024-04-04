## はじめに
初めてGitHubのコレを書くため、色々不手際があるかと思います
やっちゃいけないことをやっていたらご指摘ください

# Novelエディター
自分用につくったノベル制作ツールです
元々Fungusという有名なフリーアセットがあるのですが、それを自家製で作ってみました。
使い方はFungusに準じている所が多いです

# 環境
Unity2022.3.20f1で制作しましたが、2021〜2023で動作することを確認しています。
前提としてUniTask、SubclassSelector、RubyTextMeshPro(とTMPro)の3つの外部アセットをインポートする必要があります

↓各リンク

・https://github.com/Cysharp/UniTask 

・https://github.com/mackysoft/Unity-SerializeReferenceExtensions 

・https://github.com/jp-netsis/RubyTextMeshPro 


# インポート
ReleasesからNovel.unitypackageをダウンロードして、プロジェクトフォルダ内にドラッグアンドドロップしてください

# サンプルについて
Novel/Sample/SampleScene内にそのまま動くサンプルを載せています

# 使い方
Novelフォルダは場所を変えたり、名前を変えても大丈夫です。ただし特殊なフォルダには入れないでください

## フローチャートについて
`FlowchartExecutor`コンポーネントを使うことで会話フローを呼び出すことができます。
コマンドの操作は、インスペクターからFlowchartEditorWindowを表示して行ってください。
シーン間で共有などをしたい場合は`FlowchartData`で生成されるScriptableObjectを用いることができます

※注意:
`FlowchartExecutor`がアタッチされたオブジェクトはプレハブにしないでください

## コマンドについて
このアセットは各自でカスタムコマンドを追加して頂くことを想定しています。CommandBaseを継承して、他のコマンドを参考にして作成してください。
コマンドの色、ウィンドウのリストへの表示内容、詳しい説明、CSVへの入出力がエディタ拡張無しで実装可能です。
詳しく知りたい場合は`CommandBase`のvirtual関数の実装をご覧ください。

※注意:
Novel/Scriptable/Commands内にあるデータは基本的に直接操作しないでください。
また、コマンドのクラス名は必ず`〇〇Command`としてください

※こんな時は:
インスペクターにコマンド名と`Enable`しか表示されない
→ コマンド名のラベル部分をクリックしてください

## フラグについて
※ここでいうフラグは`bool`値以外も含むものとします。
本アセットは標準で`FlagManager`と`FlagKeyData`によるフラグ管理をサポートしています。
`Dictionary<string, object>`というやや古典的な手法です。`FlagKeyData`をキーとして値のやり取りをします。
`FlagKeyData`はジェネリックなのでさまざまな型のフラグを作れます。
そして、`SayCommand`などでフラグの値を文中に埋め込むことができます。

## CSVの入出力について
エクスポートすると、シーン内の全フローチャートGameObject(またはプロジェクト内の全フローチャートScriptableObject)のコマンドデータが書き込まれます。
インポートすると、出力した形式でデータを取り込むことができます。
Excelでのデータ形式に準じていますので、そのまま扱えます。

出力するデータや入力するデータを設定したい場合は、それぞれのコマンドの、`CommandBase`内の`CSVContent1`や`CSVContent2`プロパティをオーバーライドしてください。ゲッターとセッターは相互変換を推奨します。
また、縦の列のコマンドを自由に増やすこともできます。

すでにあるコマンドをCSVから消す機能は実装していません。とりあえず無効にしたい場合は"\<Null\>"または単に"Null"を入れてください。

※注意:
CSVのファイル名は変更してもかまいませんが、CSV内の1行目と2行目のデータは基本的に変えないでください。
