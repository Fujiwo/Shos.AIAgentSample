# リポジトリ全体レビュー (#04)

**レビュー実施日**: 2026年11月9日
**レビュー担当**: Claude Code (Claude Opus 5.5)
**対象**: リポジトリ全体 (master ブランチの作業ツリー。未コミットの変更を含む)

- コンソールエージェント: FCAIAgent, FCAIAgent1〜5
- MCP サーバー: McpServer.Con, McpServer.Sse
- Web チャット: FCAIChat1〜5
- デスクトップ: FCAICad1
- ドキュメントと設定: README.md, Documents/tutorial.*.md, .mcp.json, .vscode/mcp.json

---

## エグゼクティブサマリー

コンソールエージェント (FCAIAgent 系) と MCP サーバーは、教材として読みやすく書かれています。`static class My` で自作コードを区別する規約や、「旧/新」コメントで差分を示す書き方は、学習者にとって分かりやすい工夫です。これらは .NET 10 SDK で警告なしにビルドできることを確認しました。

ただし、**チュートリアルどおりに進めても動かない箇所**と、**Web/デスクトップ版の実行時の不具合**が見つかりました。特に優先度が高いのは次の 5 件です。

| # | 重要度 | 概要 |
|---|---|---|
| H-1 | 高 | `/sse` エンドポイントが MCP SDK 2.4.0 では既定で存在せず、チュートリアル 2.3 / 3.2 と `.mcp.json` の接続先が 404 になる |
| H-2 | 高 | FCAIChat4: SignalR の Hub は呼び出しごとに生成されるため、会話履歴が保持されず、メッセージごとに MCP サーバーのプロセスがリークする |
| H-3 | 高 | FCAIChat5: セッション復元処理が実際には一度も動かない (デッドコード) |
| H-4 | 高 | FCAICad1: LLM のツール呼び出しが UI スレッド外から WinForms コントロールを操作する |
| H-5 | 高 | README の FCAIChat1〜3 の説明が実装と異なる (実際は LLM を使わない) |

---

## 1. 重要度：高

### H-1. MCP (HTTP) サーバーの `/sse` エンドポイントが存在しない

**該当箇所**
- `McpServer.Sse/Program.cs:62-70` (`WithHttpTransport()` + `MapMcp()`)
- `Documents/tutorial.2.3.md:51, 77, 147` (`http://localhost:3001/sse`)
- `Documents/tutorial.3.2.md:266` (`const string endPoint = "http://localhost:3001/sse";`)
- `.mcp.json`, `.vscode/mcp.json` (`"url": "http://localhost:3001/sse"`)

**問題**
ModelContextProtocol.AspNetCore 2.4.0 の `HttpServerTransportOptions.EnableLegacySse` は**既定値が `false`** です。そのため、従来の SSE エンドポイント (`/sse`, `/message`) はマップされません (パッケージ同梱の XML ドキュメントで確認)。`MapMcp()` が公開するのは Streamable HTTP の `/` だけです。

コード側の FCAIAgent5 / FCAIAgent は `http://localhost:3001/` を使っているので動作します。一方、チュートリアル 3.2 のコード片、チュートリアル 2.3 の MCP Inspector と VS Code の手順、`.mcp.json` はいずれも `/sse` を指しており、学習者がこれらに沿って進めると接続に失敗します。

**修正案** (どちらか)
1. 接続先をすべて `http://localhost:3001/` に統一する (推奨。Streamable HTTP が現行の標準)
2. サーバー側で従来の SSE を明示的に有効にする
   ```csharp
   builder.Services.AddMcpServer()
                   .WithHttpTransport(options => options.EnableLegacySse = true)
                   .WithToolsFromAssembly();
   ```

あわせて、プロジェクト名 `McpServer.Sse` とコメント中の「SSE (Server-Sent Events) を使ったリアルタイム通信」という説明も、Streamable HTTP の実態に合わせて補足するとよいでしょう。

---

### H-2. FCAIChat4: Hub のフィールドにエージェントを持っているため、毎回作り直される

**該当箇所**: `FCAIChat4/Hubs/ChatHub.cs:10`

```csharp
readonly MyChatAgent chatAgent = new();
```

**問題**
ASP.NET Core SignalR の Hub は **Transient** で、メソッド呼び出しのたびに新しいインスタンスが作られます。そのため、次の問題が起きます。

1. `@Agent` 宛てのメッセージを送るたびに新しい `MyChatAgent` が作られ、`AgentSession` も新しくなるので、**複数ターンの会話になりません** (README の「ステップ 4」の説明と矛盾)。
2. `Start()` で `npx @playwright/mcp@latest` の MCP サーバー (子プロセス) が毎回起動されますが、Hub は `Dispose` されても `chatAgent.Dispose()` を呼びません。そのため、**メッセージを送るたびに Node.js プロセスがリーク**します。
3. 毎回システムメッセージを LLM に送るので、API 呼び出しの回数が 2 倍になります。

**修正案**
FCAIChat5 と同様に、接続 ID ごとに静的な `ConcurrentDictionary` で保持し、`OnDisconnectedAsync` で破棄します。より望ましいのは、エージェントを管理するサービスを DI に Singleton として登録し、Hub からはそれを参照する形です。

---

### H-3. FCAIChat5: セッション復元が一度も成功しない

**該当箇所**
- `FCAIChat5/Hubs/ChatHub.cs:80-96` (`GetOrCreateChatAgentAsync`)
- `FCAIChat5/AIAgents/ChatAgent.cs:45-52` (`DeserializeSessionAsync`)
- `FCAIChat5/AIAgents/ChatAgent.cs:122` (`RestoreThread`)

**問題**
1. `GetOrCreateChatAgentAsync` は `new MyChatAgent()` の直後に `DeserializeSessionAsync` を呼びます。しかしこの時点では `Start()` が未実行で `Agent` が `null` のため、`DeserializeSessionAsync` は何もせずに `false` を返します。復元に失敗しても例外にならないため、**誰にも気づかれません**。
2. 本来この順序を想定して作られた `RestoreThread()` (`pendingThreadRestore` を使う経路) は呼び出し箇所がコメントアウトされており、使われていません。
3. そもそも保存のキーが `Context.ConnectionId` です。SignalR の接続 ID は再接続 (ページの再読み込みを含む) のたびに変わるため、仮に 1. を直しても同じ会話には戻れません。
4. `OnDisconnectedAsync` は「再接続に備えて」保存済みのセッションを残しますが、3. の理由で二度と参照されません。`InMemoryThreadStore` には接続ごとにエントリが増え続け、**メモリリーク**になります。

**修正案**
- 復元のキーは、ログインユーザー ID (`Context.UserIdentifier`) か、クライアントが生成して保持する会話 ID にする
- 復元は `RestoreThread()` の経路 (`Start()` 内で `Agent` を作った後にデシリアライズ) に一本化し、使われない `DeserializeSessionAsync` 経由の処理を削除する
- 保存したセッションに有効期限を付けるか、明示的に削除する

---

### H-4. FCAICad1: UI スレッド外からコントロールを操作している

**該当箇所**
- `FCAICad1/Toolbox.cs` (すべての `Draw*` と `ClearAll`)
- `FCAICad1/Model.cs:24-35` (`Add` / `Clear` が `Update` イベントを同期的に発火)
- `FCAICad1/MainForm.cs:17-22`, `FCAICad1/View.cs:21-28` (`CreateGraphics()` で直接描画)

**問題**
ツール関数は `FunctionInvokingChatClient` から呼ばれます。Azure SDK などの内部では `ConfigureAwait(false)` が使われるため、ツール関数は**スレッドプールのスレッドで実行される可能性**があります。その場合、次の問題が起きます。

- `view.Draw()` の `CreateGraphics()` や `view.Invalidate()` が UI スレッド以外から呼ばれる。デバッガー接続時は `InvalidOperationException` (クロススレッド操作)、それ以外では未定義動作になる
- UI スレッドの `OnPaint` が `Model` の `List<Figure>` を列挙している最中に、別スレッドから `Add` されると「コレクションが変更されました」例外が発生しうる

**修正案**
`Toolbox` の各メソッド、または `MainForm.AddFigure` / `Clear` の中で、`MainForm.Invoke(...)` (または `BeginInvoke`) を使って UI スレッドに処理を移します。

```csharp
public void AddFigure(Figure figure)
{
    if (InvokeRequired)
        Invoke(() => model.Add(figure));
    else
        model.Add(figure);
}
```

---

### H-5. README の FCAIChat1〜3 の説明が実装と一致しない

**該当箇所**: `README.md` 「Web アプリケーション (FCAIChat シリーズ)」の表、「ファイル・ディレクトリ構成」

**問題**
README では、FCAIChat1 は「Ollama との対話」、FCAIChat2 は「Ollama と Azure OpenAI の切り替え」、FCAIChat3 は「セッション管理による会話履歴」と説明されています。しかし実際の FCAIChat1〜3 は **AI 関連のパッケージもコードも含まない**、SignalR と EF Core によるチャットアプリです。

- FCAIChat1: SignalR チャット
- FCAIChat2: DB (`MessagesDbContext`) への永続化を追加
- FCAIChat3: 部分ビュー (`_MessagePartial`) によるメッセージ表示を追加

ほかにも、実装と食い違う記述があります。

- FCAIChat4 は Playwright MCP、FCAIChat5 は Brave Search MCP を使っている (README の「複数のツールを統合」とは異なる。FCAIChat5 も単一の MCP サーバー)
- 構成図の FCAIChat1 に `Controllers/ Models/ Views/` (MVC) とあるが、実際は Razor Pages (`Pages/`)
- 構成図で FCAICad1 の `Program.cs` を「ツール定義」としているが、実際の定義は `Toolbox.cs`
- FCAICad1 の「提供されるツール」が `GetPaperSize` / `DrawLine` の 2 つだけになっているが、実装は 11 個 (`DrawCircle`, `DrawCurve` など)

**修正案**: 表と構成図を実装に合わせて更新します。

---

## 2. 重要度：中

### M-1. README のクイックスタートどおりに進めるとモデルが見つからない

- `README.md:75, 533` では `ollama pull llama3.2` を案内しているが、コードは全プロジェクトで `ollama.SelectedModel = "minimax-m3:cloud"` を使っている
- `:cloud` モデルを使うには Ollama アカウントでのサインイン (`ollama signin`) が必要で、`llama3.2` を pull しても使われない

**修正案**: クイックスタートに `minimax-m3:cloud` の使い方 (サインイン手順) を書くか、ローカルモデルに切り替える方法 (`SelectedModel` の変更箇所) を明記します。

### M-2. ドキュメントと実コードのモデル名・バージョン表記のずれ

| 項目 | コード | ドキュメント |
|---|---|---|
| Azure OpenAI デプロイ名 | `MAI-Thinking-1` (作業ツリー) | tutorial 1.2 / 1.3 / 3.1 / 3.2 は `gpt-5-mini`。直近のコミットメッセージは `gpt-5.5` |
| ターゲット | 全プロジェクト `net10.0` | README のバッジは `.NET 9.0` / `C# 12.0`。本文は「.NET 8.0 以上」 |

全プロジェクトが `net10.0` を対象としているため、**.NET 8 / 9 SDK ではビルドできません**。「.NET 8.0 以上」という記述は誤解を招くので、「.NET 10 SDK 必須」と明記すべきです。

### M-3. FCAIChat5 の `async void Dispose()`

**該当箇所**: `FCAIChat5/AIAgents/ChatAgent.cs:54`、`FCAIChat4/AIAgents/ChatAgent.cs:36`

`async void` のため、呼び出し側は破棄の完了を待てません。また、`DisposeAsync` で例外が出ると**プロセス全体がクラッシュ**します。FCAICad1 と同じように `IAsyncDisposable` を実装し、`await agent.DisposeAsync()` で破棄してください。

### M-4. 起動失敗時の例外処理 (Web / CAD)

- `ChatAgent.Start()` は `RunAgentAsync` の `try` の外で実行されます。そのため、MCP サーバーの起動失敗 (npx が無い、`BRAVE_API_KEY` が未設定など) や環境変数の未設定による例外はそのまま呼び出し元に伝わります。
  - FCAIChat5: Hub メソッドが例外で終わります。ユーザーのメッセージはすでにブロードキャスト済みですが、エージェントからは何も返りません。
  - FCAICad1: `Program.OnPrompted` が `async void` なので未処理例外になり、`promptButton` も無効のまま戻りません (`FCAICad1/Program.cs:99-104`)。
- FCAICad1 は CAD 操作が主目的なのに、Brave Search の API キーがないと**一切動作しません**。Brave Search は任意 (失敗しても CAD ツールだけで続行する) にすることを推奨します。

### M-5. FCAICad1 の終了時に `RunAsync().Wait()` を使っている

**該当箇所**: `FCAICad1/Program.cs:88`

`Application.Run` が終わった後も STA スレッドには `WindowsFormsSynchronizationContext` が残っています。その状態で `await chatAgent.DisposeAsync()` が非同期に完了すると、続きの処理は UI スレッドにポストされます。しかし UI スレッドは `.Wait()` でブロックされているので、**終了時にデッドロックする恐れ**があります (要実機確認)。`Main` で同期的に `Application.Run` を呼び、破棄は `FormClosing` などで行う形を推奨します。

### M-6. FCAIChat5 の Hub の権限まわり

- `ChatHub` に `[Authorize]` がなく、`SendMessage(string user, ...)` の `user` をクライアントが自由に指定できます (なりすましが可能)。サーバー側では `Context.User.Identity.Name` を使うべきです。
- `Clear()` は**全ユーザー共通のメッセージ履歴を削除**しますが、エージェントの状態は呼び出したユーザーの分しか初期化されません。さらに、`Clients.All` への通知がないため、他のユーザーの画面は古いままです。
- `chat.js:36-38` では `invoke("Clear")` の完了を待たずに `location.reload()` を実行しています。そのため、削除が終わる前に再読み込みされたり、再読み込みによる切断で処理が中断されたりする可能性があります。`invoke(...).then(() => location.reload())` にしてください。

### M-7. FCAIAgent5 / FCAIAgent の MCP 接続まわり

- `GetMcpTools()` は 3 つのサーバーに順に接続します。途中の接続に失敗すると、すでに接続済みのクライアントが破棄されません。また、メインの処理でも MCP クライアントの破棄が `try/finally` で保護されていません。
- `@modelcontextprotocol/server-filesystem` に渡しているパス `"/work"` は、Windows (チュートリアルが前提とする環境) では実在しないことが多く、サーバーの起動に失敗します。
- `MCPServer.Con\MCPServer.Con.csproj` という表記は、実際のファイル名 `McpServer.Con/McpServer.Con.csproj` と大文字・小文字が異なります。Windows では動きますが、README で動作対象としている Linux / macOS では見つかりません。

### M-8. 共有している LocalDB データベース

FCAIChat1〜5 の `appsettings.json` は、すべて同じデータベース名 `aspnet-FCAIChat-ecb85b14-...` を使っています。一方で、各プロジェクトのマイグレーションは同じではありません (FCAIChat1 には `MessagesDbContext` がない)。そのため、ステップを行き来すると `__EFMigrationsHistory` の不整合やテーブルの欠落が起こりえます。プロジェクトごとにデータベース名を変えることを推奨します。

---

## 3. 重要度：低

| # | 該当箇所 | 内容 |
|---|---|---|
| L-1 | FCAIAgent2〜5, FCAIAgent, FCAIChat4/5, FCAICad1 の `GetEndPoint` / `GetKey` | コメントの「例」が逆になっている。エンドポイントの例が 16 進文字列 (キーの形式)、API キーの例が URL になっている。また「ここではハードコードする」と書いてあるが、実際は環境変数から読んでいる |
| L-2 | FCAIAgent3〜5, FCAIChat4/5, FCAICad1 | `Instructions` と同じ内容の `SystemPrompt` を、System ロールの「1 ターン目」として `RunAsync` で送っている。LLM がシステムメッセージに応答するので、API 呼び出しが 1 回余分に発生する (コンソール版ではその応答が「Agent: ...」として表示される)。`instructions` だけで十分 |
| L-3 | FCAIAgent3 と FCAIAgent4/5 の `RunAsync` | 引数名が `thread` と `session` で混在している (FCAIAgent だけ `session`)。Agent Framework の `AgentThread` から `AgentSession` への名称変更に合わせて統一するとよい |
| L-4 | FCAIAgent5:229 / FCAIAgent:194 | `weartherMcpClient` は `weatherMcpClient` のタイプミス |
| L-5 | FCAIChat4/5 `ChatHub` | `responce` は `response` のタイプミス |
| L-6 | FCAIChat4/5 `ChatHub.GetPromptForAgent` | 判定 (`Contains`) は大文字・小文字を区別しないが、削除 (`Replace`) は区別する。`@agent` と書くと、宛先の文字列がプロンプトに残る |
| L-7 | FCAIChat5 `ChatHub.Clear` | `RemoveRange(dbSet)` は全行をメモリに読み込んでから削除する。`ExecuteDeleteAsync()` で十分 |
| L-8 | FCAIChat3〜5 `chat.js:47` | LLM の応答全文を GET のクエリ文字列で送っている。長い応答では URL の長さ制限に達し、フォールバック表示 (書式なし) になる。POST にするか、クライアント側で組み立てる |
| L-9 | FCAIChat3〜5 `Index.cshtml` | 未ログインの場合、`userInput` が空のため送信ボタンを押しても何も起きない (エラー表示もない) |
| L-10 | FCAICad1 `ChatAgent.cs:133` | システムプロンプトが `DrawFreeLine` を案内しているが、実際のツール名は `DrawFreeFormCurve`。`DrawPolylineOrPolygon` / `DrawCurve` / `DrawRoundedRectangle` も説明に含まれていない |
| L-11 | FCAICad1 `Figure.ColorName` | `Color.FromName` は未知の色名を受け取ると透明色を返す。LLM が誤った色名を指定しても透明で描かれるだけで、誤りが伝わらない。`IsKnownColor` で検証して例外 (エラー文字列) を返すとよい |
| L-12 | FCAICad1 `Animator.Animate` | 停止した `Timer` を `Dispose` していない (プロンプトのたびに 1 個ずつリークする) |
| L-13 | FCAICad1 `MainForm.AppendResponse` | 名前は「追加」だが、実際は `Text` を上書きしている |
| L-14 | McpServer.Sse `Properties/launchSettings.json` | `applicationUrl` が `5158` / `7219` で、`launchBrowser: true` になっている。しかしコードは `Run("http://localhost:3001")` で 3001 を固定しているので、F5 実行時に開くブラウザーは接続できない |
| L-15 | McpServer.Con `GetTimeInTimezone` | 引数 `timezone` に `[Description]` がなく、どの形式 (IANA か Windows ID か) を渡すべきかが LLM に伝わらない。また `catch {}` ですべての例外を握りつぶしている。`TimeZoneNotFoundException` / `InvalidTimeZoneException` に限定するとよい |
| L-16 | McpServer.Sse `GetWeatherForecast` | 完全一致でしか判定しないので、「東京」や「Tokyo」では既定の応答になる (教材として意図的であれば問題なし) |
| L-17 | `.github/copilot-instructions.md` | 見出しだけで内容がない |
| L-18 | FCAIAgent5 | 「旧: ここから」のコメントアウトされたコードが大量に残っている。差分を示す意図は理解できるが、最終形 (FCAIAgent) との差がこのコメントと引数名だけなので、FCAIAgent の存在意義を README で説明するとよい |

---

## 4. 設計面の所見

### 4.1 コードの複製と同期

ステップごとに完全なコピーを持つ構成は、教材として合理的です。ただし、モデル名、パッケージのバージョン、`GetChatClient` の実装などを変えるたびに、**最大 13 プロジェクトとチュートリアル 5 ファイル**を手作業で同期する必要があります。実際に、デプロイ名 (M-2)、エンドポイント (H-1)、コメントの誤り (L-1) がプロジェクト間やドキュメント間でずれています。次の対策を推奨します。

- パッケージのバージョンは `Directory.Packages.props` (Central Package Management) で一元管理する
- モデル名やエンドポイントは `appsettings.json` / 環境変数 / User Secrets に移し、コードから定数をなくす (教材上の分かりやすさとのトレードオフになるので、少なくとも FCAIChat / FCAICad では推奨)
- ドキュメント中のコード片と実コードの差分を検出する簡単なスクリプトを用意する

### 4.2 Web 版のエージェントのライフサイクル

FCAIChat4/5 では、エージェント (と MCP の子プロセス) のライフサイクルを Hub の中で管理しています。H-2 / H-3 / M-3 / M-6 はいずれもこの点に起因します。`IHostedService` または Singleton の `AgentManager` を用意し、次のように役割を分けると、構造が明確になります。

- MCP クライアントはアプリ全体で 1 つにして共有する (プロセスの起動コストを削減)
- `AgentSession` はユーザー単位で管理する
- アプリ終了時に `IAsyncDisposable` で確実に破棄する

### 4.3 FCAICad1 のツール設計

`AIFunctionFactory.Create` で静的メソッドをそのままツール化する設計は、シンプルで教材として優れています。今後の改善点としては、次の 2 つが考えられます。

- ツールの戻り値を `void` から結果の文字列 (例: 「描画しました: Circle at (100,100) r=50」) に変えると、LLM が自分の操作結果を確認できる
- 用紙サイズ (3000×2000) とビューの表示サイズの間のスケーリングを明示する

---

## 5. ビルド検証結果

.NET SDK 10.0.114 (Linux, WSL2) で `dotnet build` を実行しました。

| プロジェクト | 結果 |
|---|---|
| FCAIAgent, FCAIAgent1〜5 | ✅ 成功 (警告 0) |
| McpServer.Con, McpServer.Sse | ✅ 成功 (警告 0) |
| FCAIChat4, FCAIChat5 | ✅ 成功 (警告 0) |
| FCAICad1 | ― (Windows 専用のため未実施) |

※ 実行時の動作確認 (LLM / MCP サーバーへの接続) は行っていません。H-4 と M-5 はコードを読んだうえでの指摘で、実機での再現確認が必要です。

---

## 6. 推奨対応順序

1. **H-1** チュートリアルと `.mcp.json` の接続先を修正する (学習者が確実につまずくため)
2. **H-5 / M-1 / M-2** README とチュートリアルの記述を実装に合わせる
3. **H-2 / H-3 / M-3** Web 版のエージェントのライフサイクルを見直す
4. **H-4 / M-4 / M-5** FCAICad1 のスレッドと例外処理を修正する
5. 低重要度の項目は、上記の修正のついでに順次対応する
