# Shos.StaffManager

[English version of README / 英語版はこちら](README.md)

.NET 10.0 と C# で作られた、コンソール ベースの社員管理システムです。日本語対応の対話型コマンドライン インターフェイスで社員と部署を管理でき、Microsoft Agent Framework で構築されたアプリ内 AI チャット アシスタントと、外部 AI アシスタントと連携するためのスタンドアロンな MCP (Model Context Protocol) サーバーを備えています。

## 目的と用途

Shos.StaffManager は、小規模で現実的な業務ツールをモダンな C# で記述し、それを AI アシスタントに開放する方法を示すためのサンプル アプリケーションです。

想定している用途:

- **小規模な社員名簿の管理**: 部署と社員 (社員番号・氏名・ふりがな・部署) の一覧を 1 つの JSON ファイルで保持し、日本語の対話型コンソール UI から編集する
- **モダンな .NET の学習**: レコード、ジェネリック、LINQ、`System.Text.Json` によるシリアル化、コマンド パターン、コンソール UI の構築を、チュートリアルより大きく、かつ通読できる規模のコードベースで学ぶ
- **AI 連携の実験**: 組み込みの AI チャット (Microsoft Agent Framework + Ollama または Azure OpenAI) から名簿を操作し、同じ操作を同梱の MCP サーバー経由で Claude Desktop や VS Code などの外部 AI アシスタントに公開する

本番の人事システムとしての利用は想定していません。データベースも認証もなく、JSON ファイルに対する同時実行制御は「最後に書き込んだ側が勝つ」だけです。

## プログラミング学習者にとっての価値

このサンプル アプリケーションは、モダンな C# / .NET 開発を学ぶ開発者にとって優れた教材になります。コードベースには数多くの重要なプログラミング概念とパターンが含まれています。

### 🎯 **基本的なプログラミング概念**
- **オブジェクト指向プログラミング**: クラス、継承、カプセル化、ポリモーフィズム
- **モダンな C# の機能**: レコード、null 許容参照型、パターン マッチング、コレクション式
- **SOLID 原則**: 単一責任、依存関係逆転、インターフェイス分離
- **ジェネリック プログラミング**: ジェネリック クラス、メソッド、型制約

### 🏗️ **ソフトウェア アーキテクチャとデザイン パターン**
- **Model-View-Controller (MVC)**: データ・表示・ビジネス ロジックのきれいな分離
- **Command パターン**: 再利用可能なコマンド オブジェクトとして実装されたメニュー操作
- **Factory パターン**: コマンドの生成と管理
- **Strategy パターン**: さまざまな検証ルールと処理方式
- **Template Method パターン**: 振る舞いをカスタマイズできる基底クラス

### 📊 **データ管理とシリアル化**
- **JSON シリアル化**: `System.Text.Json` によるカスタム シリアル化
- **データ転送オブジェクト (DTO)**: 永続化用の独立したシリアル化可能モデル
- **ファイル入出力**: UTF-8 エンコーディングとエラー処理
- **データ検証**: 独自ルールとエラー メッセージによる入力検証

### 🔧 **応用的な C# のテクニック**
- **拡張メソッド**: 文字列操作と型解析のユーティリティ
- **リフレクション**: 動的な型解析とメソッド呼び出し
- **LINQ**: 関数型のクエリ操作とデータ変換
- **タプルの分解**: 名前付きタプルによるモダンな戻り値パターン
- **リソース管理**: コンソール色管理での `IDisposable` パターン

### 🌐 **国際化とローカライズ**
- **Unicode 対応**: 日本語文字 (全角/半角) の適切な扱い
- **文字幅の計算**: 全角と半角が混在するテキストの表示調整
- **テキスト正規化**: 入力を一貫して扱うための Unicode 正規化

### 🎨 **ユーザー インターフェイス設計**
- **コンソール UI パターン**: メニュー システム、ダイアログ、表形式のデータ表示
- **入力検証**: 分かりやすいエラー メッセージによるその場での検証
- **状態管理**: 複数ステップのウィザードと繰り返し可能な操作
- **ユーザー体験**: 確認ダイアログとナビゲーションのパターン

### 🧪 **実践されているベスト プラクティス**
- **エラー処理**: 具体的な例外型を用いた try-catch
- **コードのドキュメント化**: 充実した XML ドキュメント コメント
- **命名規則**: 明確で説明的なメソッド名・変数名
- **コードの構成**: 論理的な名前空間構造とファイル構成
- **型安全性**: null 許容参照型と null チェック

### 🎓 **スキル別の学習ポイント**

**初級:**
- C# の基本文法とオブジェクト指向の概念
- ファイル入出力とデータ永続化
- コンソール アプリケーションの開発
- エラー処理の基礎

**中級:**
- デザイン パターンの実装
- ジェネリック プログラミング
- LINQ と関数型プログラミングの考え方
- 独自のシリアル化戦略

**上級:**
- リフレクションとメタプログラミング
- 高度な型システムの機能
- アーキテクチャ パターン
- パフォーマンス最適化の手法
- **AI 連携**: Model Context Protocol (MCP) サーバーの開発
- **ツール設計**: AI から利用できるツールと API の作成

### 📚 **学習用の練習課題**

次のような拡張に取り組むと理解が深まります:
1. **機能追加**: 社員・部署の編集/削除操作を実装する
2. **検証の拡張**: より複雑な業務ルールと検証ロジックを追加する
3. **データベース連携**: JSON 保存を Entity Framework Core に置き換える
4. **テスト**: ビジネス ロジックと検証の単体テストを書く
5. **ローカライズ**: 複数言語対応を追加する
6. **パフォーマンス**: キャッシュや非同期処理を実装する
7. **UI 改善**: 色分け、進捗表示、メニュー ショートカットを追加する
8. **MCP の拡張**: レポートや分析用の MCP ツールを追加する
9. **AI 連携**: MCP サーバーを使ってさまざまな AI アシスタントを試す
10. **プロトコル開発**: 特定業務向けの独自 MCP ツールを作成する

このコードベースは、単純なチュートリアルと複雑な業務アプリケーションの間を埋める、現実的でよく構造化された例であり、さまざまなレベルの開発者にとって理想的な学習素材です。

## 主な機能

### 中核となる機能
- **社員管理**: 社員の追加、一覧表示、検索、所属部署の変更
- **部署管理**: 部署の追加、一覧表示、検索、名称変更
- **検索機能**: 氏名・社員番号・部署による社員検索、名称・部署コードによる部署検索
- **データの永続化**: UTF-8 エンコーディングによる JSON ファイル保存
- **日本語対応**: 表示幅を正しく計算した日本語文字の完全サポート
- **MCP 連携**: AI アシスタント連携のための Model Context Protocol サーバー

### ユーザー インターフェイス
- 対話型のコンソール メニュー
- [Shos.Console](https://www.nuget.org/packages/Shos.Console/) ライブラリによる表形式のデータ表示
- エラー メッセージ付きの入力検証
- データ入力操作での確認ダイアログ
- まとめて処理するための繰り返し実行コマンド

### AI チャット (アプリ内)
- **組み込みの AI アシスタント**: **(c) AIチャット** メニュー コマンドで、[Microsoft Agent Framework](https://www.nuget.org/packages/Microsoft.Agents.AI/) 上に構築された AI エージェントとのチャットをコンソール アプリ内で直接開始できます
- **差し替え可能なチャット バックエンド**: 既定ではローカルの [Ollama](https://ollama.com/) サーバー (`http://localhost:11434`、モデル `minimax-m3:cloud`) を使用し、`AZURE_OPENAI` 条件付きコンパイル シンボルを有効にすると Azure OpenAI を使用します
- **ツール呼び出し**: エージェントは社員/部署の管理ツール (`Toolbox`) に加え、`npx @modelcontextprotocol/server-brave-search` で起動する外部の Brave Search MCP ツールも呼び出せます

### AI アシスタント連携 (MCP サーバー)
- **MCP サーバー**: 独立した `Shos.StaffManager.MCPServer` プロジェクトが、Model Context Protocol を通じて社員管理機能を外部 AI アシスタント (Claude Desktop、VS Code など) に公開します
- **ツール ベースのアクセス**: AI アシスタントが社員と部署を直接管理できます
- **リアルタイムな操作**: 追加・削除・検索・名称変更・所属変更を MCP ツールとして利用できます

## 技術アーキテクチャ

### デザイン パターン
- **Model-View-Controller (MVC)**: 関心事のきれいな分離
- **Command パターン**: メニュー操作をコマンドとして実装
- **Repository パターン**: JSON ベースのデータ永続化層

### クラス図

ソリューション全体の最新の Mermaid クラス図は [Documents/ClassDiagrams.md](Documents/ClassDiagrams.md) に別途まとめてあります:

- **名前空間図**: 3 つのプロジェクト (`Shos.StaffManager`、`Shos.StaffManager.Models`、`Shos.StaffManager.MCPServer`) にまたがる全名前空間の依存関係
- **クラス図**: ソリューション内のすべてのクラス/レコード (継承、コンポジション、集約、依存関係) を名前空間ごとにグループ化

### プロジェクト構成
```
Shos.StaffManager/
├── Shos.StaffManager/                  # メインのコンソール アプリケーション
│   ├── Program.cs                        # エントリ ポイント (Load → メニュー ループ → Save)
│   ├── Application/
│   │   ├── View.cs                       # データ表示の整形 (Views 名前空間)
│   │   └── Controllers.cs                # メニュー コマンドと CommandManager (Controllers 名前空間)
│   ├── AI/
│   │   ├── AIAgent.cs                    # ChatAgent / MyChatAgent (Microsoft Agent Framework)
│   │   └── ToolBox.cs                    # アプリ内 AI チャット用の関数呼び出しツール
│   ├── Common/
│   │   ├── Helpers.cs                    # UserInterface、TypeParser、拡張メソッド
│   │   └── ControllerBase.cs             # Window、Command<TModel>、Menu<TModel> などの基底型
│   └── Data/
│       └── FC.StaffManager.json          # サンプル データ ファイル
├── Shos.StaffManager.Models/            # 共有データ モデル ライブラリ
│   └── Models.cs                         # Department、Staff、DepartmentList、StaffList、Company
└── Shos.StaffManager.MCPServer/         # 外部 AI アシスタント連携用の MCP サーバー
    └── Program.cs                        # ホストの設定と StaffManagerTools (MCP ツール定義)
```

### データ モデル

#### Staff レコード
- **Number**: 社員番号 (1〜9999)
- **Name**: 氏名 (1〜30 文字)
- **Ruby**: ふりがな (カタカナ)
- **Department**: 所属部署のオブジェクト

#### Department レコード
- **Code**: 部署コード (100〜999)
- **Name**: 部署名 (1〜30 文字)

### データ検証
- 社員番号は 1〜9999 の範囲で一意であること
- 部署コードは 100〜999 の範囲で一意であること
- 名前の項目は 1〜30 文字の長さ制限があること
- 社員登録時に部署の参照が検証されること

## Shos.StaffManager.MCPServer

### MCP (Model Context Protocol) とは

**Model Context Protocol (MCP)** は、AI アシスタントと外部のデータ ソースやツールを安全に接続するためのオープン標準です。MCP により、Claude や ChatGPT などの AI アシスタントは、標準化されたプロトコルを通じて外部システムにアクセス・操作でき、学習データを超えた能力を得られます。

MCP の主な利点:
- **安全な統合**: 外部ツールとデータへの制御されたアクセス
- **リアルタイムなデータ**: 最新の情報へのアクセス
- **ツールの実行**: AI アシスタントが外部システムに対して操作を実行できる
- **標準化されたプロトコル**: AI プラットフォームをまたいだ一貫したインターフェイス

### MCP サーバー コンポーネント

`Shos.StaffManager.MCPServer` プロジェクトは、社員管理システムの機能を AI アシスタントが使えるツールとして公開する MCP サーバーを提供します。これにより、AI アシスタントはコンソールを手動操作することなく、社員管理システムを直接操作できます。

#### 利用できる MCP ツール

MCP サーバーは、AI アシスタント向けに次のツールを提供します。

**部署管理:**
- `GetAllDepartments()` - すべての部署を取得
- `SearchDepartments(keyword)` - キーワードで部署を検索
- `AddNewDepartment(newDepartment)` - 新しい部署を追加
- `RemoveDepartmentByCode(departmentCode)` - 部署コードで部署を削除
- `ChangeDepartmentNameByCode(departmentCode, newName)` - 部署コードで部署名を変更

**社員管理:**
- `GetAllStaffs()` - すべての社員を取得
- `SearchStaffs(keyword)` - キーワードで社員を検索
- `AddNewStaff(newStaff)` - 新しい社員を追加
- `RemoveStaffbyNumber(number)` - 社員番号で社員を削除
- `ChangeStaffsDepartmentByNumber(number, newDepartmentCode)` - 社員番号で所属部署を変更

#### MCP サーバーのセットアップ

1. **MCP サーバーのビルド**:
   ```bash
   dotnet build Shos.StaffManager.MCPServer
   ```

2. **MCP Inspector での実行** (動作確認用):
   ```bash
   npx @modelcontextprotocol/inspector dotnet run --project ./Shos.StaffManager.MCPServer/Shos.StaffManager.MCPServer.csproj
   ```

3. **Claude Desktop での設定**:

   `claude_desktop_config.json` ファイルを次のように編集します:
   ```json
   {
     "mcpServers": {
       "StaffManagerTools": {
         "command": "[path-to-executable]\\Shos.StaffManager.MCPServer.exe"
       }
     }
   }
   ```

4. **Visual Studio Code での設定**:

   VS Code の MCP 設定に次を追加します:
   ```json
   {
     "servers": {
       "MCPServer.Console": {
         "type": "stdio",
         "command": "dotnet",
         "args": [
           "run",
           "--project",
           "C:\\[project-folder]\\Shos.StaffManager.MCPServer.csproj"
         ]
       }
     }
   }
   ```

#### AI 連携の活用例

MCP サーバーを動かしておくと、AI アシスタントは次のことができます:
- 最新データを使って社員や部署に関する**質問に答える**
- 社員情報を照会・分析して**レポートを作成する**
- 複数の社員をまとめて追加するなどの**一括操作を行う**
- 重複や不整合を確認して**データの整合性を検証する**
- 社員管理業務に対して**対話的な支援を提供する**

この連携により、コンソール ベースの社員管理システムは、AI アシスタントが活用できるバックエンド サービスへと拡張されます。

## 動作環境

### コンソール アプリケーション
- **.NET 10.0 SDK** 以降 (Windows / macOS / Linux。Visual Studio 2026 以降は任意)
- **Shos.Console 1.1.5**、**Shos.UndoRedoList 1.0.5** (`Shos.StaffManager.Models` 経由)
- **Microsoft.Agents.AI 1.14.0** / **Microsoft.Agents.AI.OpenAI 1.14.0** - アプリ内 AI チャット用の Microsoft Agent Framework
- **OllamaSharp 5.4.27** - ローカル LLM チャット クライアント (既定のバックエンド)
- **Azure.AI.OpenAI 2.1.0** - 任意の Azure OpenAI チャット クライアント (`AZURE_OPENAI` 条件付きコンパイル シンボルで有効化)
- **ModelContextProtocol 1.4.1** - アプリ内 AI チャットが外部の Brave Search ツールを呼ぶための MCP クライアント

(上記のパッケージはすべて NuGet により自動的に復元されます)

**(c) AIチャット** コマンドを使う場合のみ必要:

- `http://localhost:11434` で動作する [Ollama](https://ollama.com/) サーバーと `minimax-m3:cloud` モデル。**または** `AZURE_OPENAI` 条件付きコンパイル シンボル付きでビルドした場合は `AZURE_OPENAI_ENDPOINT` / `AZURE_OPENAI_API_KEY` 環境変数
- 外部の Brave Search MCP ツール用に [Node.js](https://nodejs.org/) (`npx` のため) と `BRAVE_API_KEY` 環境変数

### MCP サーバー
- **.NET 10.0 SDK** 以降
- **ModelContextProtocol 1.4.1** (NuGet により自動的に復元)
- **Microsoft.Extensions.Hosting 10.0.10** (NuGet により自動的に復元)

## インストールとビルド方法

### コンソール アプリケーション

1. **リポジトリをクローンする**:
   ```bash
   git clone https://github.com/Fujiwo/Shos.StaffManager.git
   cd Shos.StaffManager
   ```

2. **アプリケーションをビルドする**:

   .NET CLI の場合:
   ```bash
   dotnet build
   ```

   Visual Studio の場合:
   1. `Shos.StaffManager.sln` を開く
   2. `Shos.StaffManager` をスタートアップ プロジェクトに設定する
   3. ソリューションをビルドする (**ビルド** > **ソリューションのビルド**、または `Ctrl+Shift+B`)

3. **コンソール アプリケーションを実行する**:
   ```bash
   dotnet run --project Shos.StaffManager
   ```
   Visual Studio では `F5` (デバッグ実行) または `Ctrl+F5` (デバッグなしで実行) を押します。

### MCP サーバーのセットアップ

1. **MCP サーバーをビルドする**:
   ```bash
   dotnet build Shos.StaffManager.MCPServer
   ```

2. **MCP Inspector で動作確認する**:
   ```bash
   npx @modelcontextprotocol/inspector dotnet run --project Shos.StaffManager.MCPServer
   ```

3. **AI アシスタントを設定する** (詳しい設定は上記の MCP サーバーの節を参照)

## 使い方

### メイン メニューの項目
アプリケーションは次の日本語メニューを表示します:

- **(s) 社員一覧** - すべての社員を一覧表示
- **(f) 社員検索** - 社員を検索
- **(a) 社員追加** - 社員を新規追加
- **(d) 部署一覧** - すべての部署を一覧表示
- **(e) 部署追加** - 部署を新規追加
- **(c) AIチャット** - アプリ内 AI アシスタントとチャット
- **(x) 終了** - アプリケーションを終了

### 操作例

#### 部署の追加
1. メイン メニューで **(e) 部署追加** を選ぶ
2. 部署コード (100〜999) を入力する
3. 部署名 (1〜30 文字) を入力する
4. 追加を確認する

#### 社員の追加
1. メイン メニューで **(a) 社員追加** を選ぶ
2. 社員番号 (1〜9999) を入力する
3. 氏名 (1〜30 文字) を入力する
4. ふりがなを入力する
5. 既存の部署コードを入力する
6. 追加を確認する

#### 社員の検索
1. メイン メニューで **(f) 社員検索** を選ぶ
2. 検索語 (氏名・社員番号・部署) を入力する
3. 絞り込まれた結果を確認する
4. 検索を繰り返すか、メイン メニューに戻る

## データの保存

### ファイルの場所
コンソール アプリケーションと MCP サーバーは、いずれも同じハードコードされたパス `C:\work\FC.StaffManager.json` を読み書きします (`Shos.StaffManager/Program.cs` と `Shos.StaffManager.MCPServer/Program.cs` の `dataFilePath` 定数)。別の場所を使う場合はこの定数を変更してください。

コンソール アプリケーションは起動時に読み込み、終了時に保存します。一方 MCP サーバーはデータを変更するツールを呼ぶたびに保存するため、同じファイルに対して両方を同時に動かすと「最後に書き込んだ側が勝つ」動作になります。

### ファイル形式
データは `FC.StaffManager.json` に UTF-8 エンコーディングの整形済み JSON として保存されます:

```json
{
  "SerializableDepartmentList": [
    {
      "Code": 114,
      "Name": "ウェブ開発部"
    }
  ],
  "SerializableStaffList": [
    {
      "Number": 2562,
      "Name": "森 誠", 
      "Ruby": "モリ マコト",
      "DepartmentCode": 114
    }
  ]
}
```

### データの準備
ファイルが存在しない場合、アプリケーションは空の会社データで起動し、次の保存時にファイルを作成します。サンプル データは `Shos.StaffManager/Data/FC.StaffManager.json` に含まれています。部署と社員が入った状態で始めたい場合は、これを `C:\work\FC.StaffManager.json` にコピーしてください。

## 主要な技術

### 中核となる技術
- **.NET 10.0**: 最新の言語機能を備えたモダンな C#
- **System.Text.Json**: 高性能な JSON シリアル化
- **Shos.Console**: コンソールでの表形式表示の強化
- **Shos.UndoRedoList**: `DepartmentList` / `StaffList` が内部で使う、元に戻す/やり直し可能なコレクション
- **レコード**: 検証を伴う不変のデータ構造
- **LINQ**: 関数型のクエリ操作
- **UTF-8**: 日本語テキストの完全な Unicode 対応

### AI チャット連携 (アプリ内)
- **Microsoft Agent Framework** (`Microsoft.Agents.AI`): 「AIチャット」メニュー コマンドのエージェント/セッション抽象
- **OllamaSharp**: 既定のローカル LLM チャット クライアント (`http://localhost:11434`)
- **Azure.AI.OpenAI**: 任意の Azure OpenAI チャット クライアント (`AZURE_OPENAI` 条件付きコンパイル シンボルで有効化)
- **ModelContextProtocol (クライアント)**: アプリ内エージェントが自身の `Toolbox` 関数に加えて外部の Brave Search MCP ツールを呼べるようにする

### MCP 連携 (サーバー)
- **ModelContextProtocol**: AI アシスタントのツール連携のための標準プロトコル
- **Microsoft.Extensions.Hosting**: MCP サーバー用の .NET ホスティング モデル
- **標準入出力トランスポート**: AI アシスタントとの標準入出力による通信

## 開発上の特徴

### エラー処理
- 網羅的な入力検証
- 適切なエラーからの復帰
- 日本語による分かりやすいエラー メッセージ
- フォールバック付きの型安全な解析

### 拡張性
- Command パターンにより新しい操作を簡単に追加可能
- ジェネリックな型解析により複数のデータ型に対応
- モジュール化されたアーキテクチャによる機能追加のしやすさ
- 関心事の分離による単体テストのしやすさ

### パフォーマンス
- メモリ上でのデータ操作による高速アクセス
- 効率的な JSON のシリアル化/逆シリアル化
- 必要になったタイミングでのデータ読み込み
- 日本語表示のための最適化された文字幅計算

## コントリビュート

このプロジェクトは標準的な .NET 開発のプラクティスに従います:
- 既存のコード スタイルとパターンに従うこと
- 新機能には単体テストを追加すること
- 日本語文字のサポートを維持すること
- API を変更した場合はドキュメントを更新すること

## ライセンス

このプロジェクトは MIT License の下で公開されています。MIT License は、著作権表示とライセンス条項を含めることを条件に、商用利用を含む自由な利用・改変・再配布を許可する寛容なライセンスです。詳細は [LICENSE.txt](LICENSE.txt) ファイルを参照してください。
