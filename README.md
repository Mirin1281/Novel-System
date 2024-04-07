## はじめに
Testと付いている通り安全な動作はまだ保証できませんが、GitHubの練習も兼ねていろいろ上げたいと思います

やっちゃいけないことをやっていたらご指摘ください
<br>
<br>


# Novelエディター
自分用につくったUnityのノベル制作ツールアセットです

元々Fungusという有名なフリーアセット(https://github.com/snozbot/fungus) があるのですが、それを自家製で作ってみました。

使い方もFungusに準じている所が多いです

より軽量で、より扱いやすいものを目指しました
<br>
<br>



# 環境
Unity2022.3.20f1で制作しましたが、2021〜2023で動作することを確認しています。

前提としてUniTask、SubclassSelector、RubyTextMeshPro(とTMPro)の3つの外部アセットをインポートする必要があります

↓各リンク

・https://github.com/Cysharp/UniTask 

・https://github.com/mackysoft/Unity-SerializeReferenceExtensions 

・https://github.com/jp-netsis/RubyTextMeshPro 
<br>
<br>



# インポート
ReleasesからNovel.unitypackageをダウンロードして、プロジェクトフォルダ内にドラッグアンドドロップしてください
<br>
<br>


# サンプルについて
Novel/Sample/SampleScene内にそのまま動くサンプルを載せています

なおフォントは源の角ゴシック、イラストは「らぬきの立ち絵保管庫」様のをお借りしました
<br>
<br>


# 使い方
Novelフォルダは場所を変えたり、名前を変えても大丈夫です。ただし特殊なフォルダには入れないでください
<br>
<br>


## フローチャートについて
`FlowchartExecutor`コンポーネントを使うことで会話フローを呼び出すことができます。
コマンドの操作は、インスペクターからFlowchartEditorWindowを表示して行ってください。
シーン間で共有などをしたい場合は`FlowchartData`で生成されるScriptableObjectを用いることができます

※注意:
`FlowchartExecutor`がアタッチされたオブジェクトはプレハブにしないでください
<br>
<br>


## コマンドについて
このアセットは各自でカスタムコマンドを追加して頂くことを想定しています。CommandBaseを継承して、他のコマンドを参考にして作成してください。
コマンドの色、ウィンドウのリストへの表示内容、詳しい説明、CSVへの入出力がエディタ拡張無しで実装可能です。
詳しく知りたい場合は`CommandBase`のvirtual関数の実装をご覧ください。

※注意:
Novel/Scriptable/Commands内にあるデータは基本的に直接操作しないでください。
また、コマンドのクラス名は必ず`〇〇Command`としてください

※インスペクターにコマンド名と`Enable`しか表示されない場合は、コマンド名のラベル部分をクリックしてください
<br>
<br>

## メッセージボックス等について
ゲーム開始時に`CreateManagerData`によって`MessageBoxManager`を生成します。
必要がなければ削除してもかまいませんし、プレハブを使って後から生成することもできます

既にシーン内に該当するメッセージボックスが存在する場合はそれが使用されます。(オーバーライドのように扱えます)

また、ポートレート(立ち絵)についてもメッセージボックスと同様です。
例えば立ち絵を追加したい場合は、DefaultPortraitプレハブを基にして(Prefab Variant推奨)ポートレートオブジェクトを作成、`PortraitType`の項目を増やしてからPortraitsDataにセットしてください
<br>
<br>


## フラグについて
※ここでいうフラグは`bool`値以外も含むものとします。

本アセットは標準で`FlagManager`と`FlagKeyData`によるフラグ管理をサポートしています。
`Dictionary<string, object>`というやや古典的な手法です。`FlagKeyData`をキーとして値のやり取りをします。
`FlagKeyData`はジェネリックなのでさまざまな型のフラグを作れます。

`SayCommand`などでフラグの値を文中に埋め込むことができます。
JsonNetなどを使えばセーブ機能も比較的簡単に実装できるため、おそらく本家Fungusよりは扱いやすいと思います
<br>
<br>

## CSVの入出力について
エクスポートすると、シーン内の全フローチャートGameObject(またはプロジェクト内の全フローチャートScriptableObject)のコマンドデータが書き込まれます。
インポートすると、出力した形式でデータを取り込むことができます。
Excelでのデータ形式に準じていますので、そのまま扱えます。

出力するデータや入力するデータを設定したい場合は、それぞれのコマンドの、`CommandBase`内の`CSVContent1`や`CSVContent2`プロパティをオーバーライドしてください。ゲッターとセッターは相互変換を推奨します。
また、縦の列のコマンドを自由に増やすこともできます。

すでにあるコマンドをCSVから消す機能は実装していません。とりあえず無効にしたい場合は"\<Null\>"または単に"Null"を入れてください。

※注意:
CSVのファイル名は変更してもかまいませんが、CSV内の1行目と2行目のデータは基本的に変えないでください。
<br>
<br>
<br>
<br>

## Introduction
As the name "Test" implies, I can't guarantee safe operation yet, but I'd like to upload some stuff to GitHub for practice.

If I'm doing something I shouldn't, please let me know.
<br>
<br>


# Novel editor
This is a novel creation UnityAssets I created for myself.

Originally, there is a famous free asset called Fungus(https://github.com/snozbot/fungus) , but I made it by myself.

The usage of this tool is similar to Fungus.
<br>
<br>



# Environment
I used Unity 2022.3.20f1, but I have confirmed that it works with 2021-2023.

You need to import 3 external assets: UniTask, SubclassSelector, RubyTextMeshPro (and TMPro) as prerequisites.

↓ each link

https://github.com/Cysharp/UniTask 

https://github.com/mackysoft/Unity-SerializeReferenceExtensions 

https://github.com/jp-netsis/RubyTextMeshPro 
<br>
<br>



# Import
Download Novel.unitypackage from Releases and drag and drop it into your project folder
<br>
<br>


# About samples
Novel/Sample/SampleScene contains samples that work as they are.

The font is Gen Kaku Gothic and the illustrations are from "Ranuki's Stand-up Picture Storage".
<br>
<br>


# How to use
You can change the location of the Novel folder or rename it. However, please do not put it in a special folder.
<br>
<br>


## About flowchart
The `FlowchartExecutor` component allows you to invoke a conversation flow.
To operate the command, display the FlowchartEditorWindow from the inspector.
ScriptableObjects generated by `FlowchartData` can be used for sharing between scenes.

Note: If you want to use the FlowchartExecuteObject
Do not prefabricate the object to which `FlowchartExecutor` is attached.
<br>
<br>


## About commands
This asset is intended for users to add their own custom commands, which can be created by inheriting from CommandBase and referring to other commands.
The command's color, window list, detailed description, and input/output to/from CSV can be implemented without any editor extensions.
If you want to know more details, see `CommandBase` virtual function implementation.

*Note: The function is not implemented in the `CommandBase`.
Do not directly manipulate the data in Novel/Scriptable/Commands.
Also, be sure to use `~~Command` as the class name of the command.

If you only see the command name and `Enable` in the inspector, click on the label part of the command name.
<br>
<br>

## About message boxes, etc.
`MessageBoxManager` is created at the start of the game by `CreateManagerData`.
You can delete it if you don't need it, or you can create it later using prefabrication.

If the corresponding message box already exists in the scene, it will be used. (This can be handled like an override)

Portraits (standing pictures) can be created in the same way as message boxes.
For example, if you want to add a standing portrait, create a portrait object based on the DefaultPortrait prefab (Prefab Variant is recommended), increase the `PortraitType` field, and set it to PortraitsData.
<br>
<br>


## About flags
Flags here include values other than `bool` values.

This asset supports flag management by `FlagManager` and `FlagKeyData` by default.
It is a somewhat classic `Dictionary<string, object>` method. The `FlagKeyData` is used as a key to exchange values.
FlagKeyData` is generic, so you can create flags of various types.

Flag values can be embedded in sentences with `SayCommand` and so on.
(I think it is probably easier to use than the original Fungus, because it is relatively easy to implement save functions using JsonNet, etc.)
<br>
<br>

## About CSV input/output
When exporting, command data for all flowchart GameObjects in a scene (or all flowchart ScriptableObjects in a project) are written.
When imported, the data can be imported in the format in which it was output.
The data format follows that of Excel, so it can be handled as is.

If you want to set the data to be output or input, override the `CSVContent1` or `CSVContent2` property in the `CommandBase` of the respective command. Mutual conversion is recommended for getters and setters.
You can also freely add more commands in the vertical columns.

We have not implemented a function to erase already existing commands from CSV. If you want to disable it for now, please enter "\<Null\>" or just "Null".

Note: You may change the CSV file name, but please do not change the data in the first and second lines of the CSV.


