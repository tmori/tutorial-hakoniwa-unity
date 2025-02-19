# tutorial-hakoniwa-unity

このリポジトリでは、[箱庭Unityフレームワーク](https://github.com/toppers/hakoniwa-sim-csharp)のチュートリアルを行います。

題材はLiDARセンサ(下図)で、箱庭アセットとして組み込む方法を解説します。

![スクリーンショット 2024-12-05 16 16 59](https://github.com/user-attachments/assets/c7b98f9e-a53a-4cd4-90d5-2f43d3d89980)

# サポート環境

- Windows 10/11
- MacOS(Apple Silicon)

# 環境構築

## Unityのインストール

[Unity Hub](https://unity3d.com/jp/get-unity/download)からUnity Hubをダウンロードし、Unity Hubを使ってUnityをインストールします。

## 箱庭コア機能のインストール

以下のサイトを参照して、箱庭コア機能をインストールします。

[箱庭コア機能](https://github.com/toppers/hakoniwa-core-cpp-client)

Windowsの場合は、RAMDISKの設定などが必要です。

以下のサイトが参考になります。

https://github.com/toppers/hakoniwa-px4sim/blob/main/docs/manual/windows_preinstall.md

## 本リポジトリのクローン

以下のコマンドで、本リポジトリをクローンします。

```bash
git clone https://github.com/tmori/tutorial-hakoniwa-unity.git
```

## Unityエディタの起動

Unity Hubを起動し、本リポジトリをクローンしたディレクトリを指定してプロジェクトを開きます。

Unityエディタで、`Assets/Scenes/SampleScene`を開きます。

成功するとこうなります。

![image](images/UnityEditor.png)

# シミュレーション実行方法

## 箱庭コア機能の起動

端末を開き、hakonwia-core-cpp-clientのディレクトリに移動します。

以下のコマンドで、箱庭コア機能を起動します。

```bash
cd examples/unity_lidar
```

```bash
python lidar.py custom.json
```

成功すると、以下のGUIが表示されます。

![image](images/graph.png)

## Unityエディタでの実行

Unityエディタの再生ボタンを押すと、シミュレーションが開始され、LiDARセンサのデータが表示されます。

![image](images/simulation.png)

なお、Unityアプリケーションは以下で公開していますので、そちらをご利用いただくことも可能です。

https://github.com/tmori/tutorial-hakoniwa-unity/releases/tag/v1.0.0

# 操作方法

Unity画面上で、以下の操作が可能です。

- 視点の変更
  - マウスのドラッグで視点を変更
  - マウスのスクロールでズームイン/アウト
- 障害物の移動
  - タブキーを押すと、障害物が選択されます。選択されると黄色くなります。
  - 矢印キーで障害物を移動できます。
- 障害物のサイズ変更
  - Eキーを押すと、障害物が拡大します。
  - Qキーを押すと、障害物が縮小します。

# 箱庭Unityフレームワークの利用方法

## パッケージのインストールについて
本リポジトリのmanifest.jsonには、箱庭Unityフレームワークを利用するためのリポジトリが記載されています(hakoniwa-lab.xxxのものです)が含まれているため、特別なインストール作業は不要です。

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.endel.nativewebsocket": "https://github.com/endel/NativeWebSocket.git#upm",
    "com.hakoniwa-lab.hakoniwa-pdu": "https://github.com/toppers/hakoniwa-pdu-csharp.git#main",
    "com.hakoniwa-lab.hakoniwa-sim": "https://github.com/toppers/hakoniwa-sim-csharp.git",
    :
  }
}
```

また、パッケージマネージャーをひらけば、箱庭Unityフレームワークがインストールされていることを確認できます。

![image](images/HakoniwaPackages.png)

## 利用方法

箱庭Unityフレームワークの利用方法については、以下のリンクを参照してください。

[箱庭Unityフレームワーク](https://github.com/toppers/hakoniwa-sim-csharp)

本リポジトリのプロジェクトでは、以下のファイルが用意されています。

- 箱庭Unityフレームワークを利用しているスクリプト
  - Assets/Scripts/LiDAR2D.cs
  - Assets/Scripts/GUI/SimTime.cs
- 箱庭Unityフレームワークを利用していないスクリプト
  - Assets/Scripts/GUI/SimStart.cs
  - Assets/Scripts/GUI/CameraView.cs
  - Assets/Scripts/GUI/ObjectMove.cs

詳細は、各スクリプトにおいて、`HakoAsset`を呼び出しているコード周辺を参照してください。

# 参考リンク

- [箱庭でLiDARスペックをビジュアライズしてみた](https://qiita.com/kanetugu2018/items/56e9ba90e2d32dad76cb)


