# 修正計画書 (#04): リポジトリ全体レビューへの対応

**作成日**: 2026年11月9日
**作成者**: Claude Code (Claude Opus 5.5)
**元レビュー**: [Reviews/CodeReview.04.Repository.md](../Reviews/CodeReview.04.Repository.md)

---

## 1. 目的と方針

レビュー #04 の指摘 (高 5 / 中 8 / 低 18) を、**学習者への影響が大きい順**に解消する。

- **教材としての分かりやすさを優先する**: 修正によってステップ間の差分が読みにくくならないようにする。各ステップの「旧/新」コメントによる差分表示の慣例は維持する。
- **コードとドキュメントを同時に直す**: `Documents/tutorial.*.md` にはコードがそのまま掲載されている。コードを修正したら、同じフェーズの中で対応するチュートリアルのコード片も更新する。
- **共通部分は全プロジェクトに同じ変更を入れる**: CLAUDE.md に記載した慣例に従う。
- **フェーズごとにコミットを分ける**: レビューと巻き戻しをしやすくするため。

---

## 2. 着手前に決める事項

以下はリポジトリ管理者の判断が必要なので、着手前に決めておく。

| # | 決める事項 | 選択肢 | 推奨 |
|---|---|---|---|
| D-1 | MCP (HTTP) サーバーの接続先 (H-1) | A: すべて `http://localhost:3001/` (Streamable HTTP) に統一する / B: サーバー側で `EnableLegacySse = true` にして `/sse` を残す | **A**。現行標準に合わせる。MCP Inspector と VS Code はどちらも Streamable HTTP に対応している |
| D-2 | `McpServer.Sse` の名前 | 維持 / `McpServer.Http` に改名 | **維持**。改名すると、ソリューション、チュートリアル、スクリーンショットの差し替え範囲が大きい。名前はそのままにして、説明文で Streamable HTTP であることを補足する |
| D-3 | Azure OpenAI のデプロイ名 (M-2) | `MAI-Thinking-1` / `gpt-5.5` / `gpt-5-mini` | 管理者が決める。決めた名前でコードとドキュメントを統一する |
| D-4 | Ollama の既定モデル (M-1) | `minimax-m3:cloud` を維持して手順を追記する / ローカルモデル (例: `gemma3`) に戻す | **維持して手順を追記**。直近のコミットの意図 (実行速度) を尊重する |
| D-5 | 対応 .NET バージョンの表記 (M-2) | 「.NET 10 必須」と明記する / マルチターゲットにして .NET 8 に対応する | **「.NET 10 必須」と明記** |
| D-6 | FCAIChat1〜3 の位置付け (H-5) | README を実装に合わせる / 実装を README に合わせて LLM 対応を追加する | **README を実装に合わせる** |

### 決定結果 (2026年11月9日)

| # | 決定 |
|---|---|
| D-1 | A: すべて `http://localhost:3001/` (Streamable HTTP) に統一する |
| D-2 | `McpServer.Sse` の名前を維持し、説明文で補足する |
| D-3 | デプロイ名は **`MAI-Thinking-1`** にそろえる |
| D-4 | `minimax-m3:cloud` を維持し、手順を追記する |
| D-5 | 「.NET 10 必須」と明記する |
| D-6 | README を実装に合わせる |

---

## 3. フェーズ別の作業計画

### フェーズ 1: 学習者がつまずく箇所の修正 (ドキュメント・設定中心)

**対応する指摘**: H-1, H-5, M-1, M-2, M-7 (パスの大文字・小文字), L-14, L-17

| タスク | 対象ファイル | 内容 |
|---|---|---|
| 1-1 | `Documents/tutorial.2.3.md`, `Documents/tutorial.3.2.md`, `.mcp.json`, `.vscode/mcp.json` | D-1 に従い、`http://localhost:3001/sse` を `http://localhost:3001/` に変更する。MCP Inspector の手順 (トランスポート種別の選択) も見直す |
| 1-2 | `McpServer.Sse/Program.cs`, `Documents/tutorial.2.2.md` | コメントの「SSE を使ったリアルタイム通信」を「Streamable HTTP」に修正し、従来の SSE が既定で無効なことを注記する |
| 1-3 | `McpServer.Sse/Properties/launchSettings.json` | `applicationUrl` を `http://localhost:3001` にし、`launchBrowser` を `false` にする (L-14) |
| 1-4 | `README.md` | FCAIChat1〜5 の表を実装に合わせて書き直す。<br>1: SignalR / 2: DB 永続化 / 3: 部分ビュー / 4: Playwright MCP / 5: Brave Search MCP + セッション保存 |
| 1-5 | `README.md` | 構成図を修正する。FCAIChat1 は `Pages/` (Razor Pages)、FCAICad1 のツール定義は `Toolbox.cs`。FCAICad1 のツール一覧は実装の 11 個に更新する |
| 1-6 | `README.md` | バッジを `.NET 10.0` にし、前提条件とトラブルシューティングの「.NET 8.0 以上」を「.NET 10 SDK 必須」に修正する (D-5) |
| 1-7 | `README.md` | クイックスタートの `ollama pull llama3.2` を、D-4 で決めた手順 (`ollama signin` など) に置き換える。ローカルモデルに切り替える場合の変更箇所 (`SelectedModel`) も追記する |
| 1-8 | 全プロジェクト、`Documents/tutorial.1.2/1.3/3.1/3.2.md` | D-3 で決めたデプロイ名にそろえる |
| 1-9 | `FCAIAgent4/5`, `FCAIAgent`, `Documents/tutorial.3.1/3.2.md` | `MCPServer.Con\MCPServer.Con.csproj` を実際のファイル名 `McpServer.Con\McpServer.Con.csproj` に修正する |
| 1-10 | `.github/copilot-instructions.md` | CLAUDE.md の要点 (段階的構成、同期の慣例) を記載する。不要であれば削除する |

**完了条件**
- `grep -rn "3001/sse" .` の結果が 0 件 (`Reviews/` を除く)
- README の表と構成図の各項目が、実在するファイルとパッケージに対応している

---

### フェーズ 2: Web 版 (FCAIChat4/5) のエージェントのライフサイクルの修正

**対応する指摘**: H-2, H-3, M-3, M-4 (Web), M-6, L-5, L-6, L-7

**設計方針** (レビュー 4.2 節に基づく)

```
ChatHub (Transient)
   │ 参照
   ▼
IChatAgentManager (Singleton)
   ├─ GetOrCreateAsync(userKey) → MyChatAgent
   ├─ RemoveAsync(userKey)
   └─ IAsyncDisposable: 終了時にすべてのエージェントと MCP クライアントを破棄
```

| タスク | 対象 | 内容 |
|---|---|---|
| 2-1 | FCAIChat4/5 `AIAgents/ChatAgent.cs` | `IDisposable` + `async void Dispose()` をやめて `IAsyncDisposable` にする (M-3) |
| 2-2 | FCAIChat4/5 `AIAgents/` (新規) `ChatAgentManager.cs` | `ConcurrentDictionary<string, Lazy<Task<MyChatAgent>>>` でエージェントを管理する Singleton を作り、`Program.cs` で DI に登録する |
| 2-3 | FCAIChat4 `Hubs/ChatHub.cs` | フィールドの `new MyChatAgent()` をやめ、Manager から取得する (H-2) |
| 2-4 | FCAIChat5 `Hubs/ChatHub.cs`, `ChatAgent.cs` | キーを `Context.ConnectionId` から `Context.UserIdentifier` (ログインユーザー) に変更する。復元は `RestoreThread()` の経路 (`Start()` 内でエージェント生成後にデシリアライズ) に一本化し、使われない `DeserializeSessionAsync` を削除する (H-3) |
| 2-5 | FCAIChat5 `Services/InMemoryThreadStore.cs` | キーをユーザー単位にしたので、エントリが増え続けることはなくなる。「Clear」ボタンでの削除は維持する |
| 2-6 | FCAIChat4/5 `ChatAgent.Start()` / `ChatHub.SendMessage` | 起動失敗 (MCP サーバー、環境変数) を `try/catch` で捕まえ、エラー内容をエージェントの発言として返す (M-4)。`isFirst` は成功したときだけ `false` にする |
| 2-7 | FCAIChat5 `ChatHub` | `[Authorize]` を付ける。送信者名は引数の `user` ではなく `Context.User.Identity.Name` を使う (M-6) |
| 2-8 | FCAIChat5 `ChatHub.Clear`, `wwwroot/js/chat.js` | 削除を `ExecuteDeleteAsync()` に変え (L-7)、`Clients.All` に再描画を通知する。`chat.js` は `invoke("Clear").then(() => location.reload())` にする (M-6) |
| 2-9 | FCAIChat4/5 `ChatHub` | `responce` を `response` に直す (L-5)。宛先の除去を `Regex.Replace(..., RegexOptions.IgnoreCase)` にして、判定と大文字・小文字の扱いをそろえる (L-6) |
| 2-10 | `Documents/` | Web 版のコードはチュートリアルに掲載されていないことを確認する (掲載されていれば同期する) |

**完了条件**
- `@Agent` 宛てに 2 回続けて発言すると、2 回目が 1 回目の文脈を踏まえて応答する (FCAIChat4/5)
- 10 回送信した後も、Node.js の MCP プロセスがユーザーあたり 1 つだけ (`tasklist | findstr node` で確認)
- FCAIChat5 でページを再読み込みしても会話が継続する
- `BRAVE_API_KEY` が未設定のとき、例外で落ちずにエラーメッセージが表示される

---

### フェーズ 3: FCAICad1 のスレッドと例外処理の修正

**対応する指摘**: H-4, M-4 (CAD), M-5, L-10, L-11, L-12, L-13

| タスク | 対象 | 内容 |
|---|---|---|
| 3-1 | `MainForm.cs` | `AddFigure` / `Clear` / `PaperSize` で `InvokeRequired` を判定し、UI スレッドで実行する (H-4) |
| 3-2 | `Program.cs` | `RunAsync().Wait()` をやめる。`Main` で `Application.Run(mainForm)` を呼び、破棄は `FormClosing` で行う (M-5) |
| 3-3 | `Program.OnPrompted` | 処理全体を `try/catch/finally` で囲み、`finally` で必ず `OnPromptedEnd()` を呼ぶ (M-4) |
| 3-4 | `ChatAgent.GetToolsAsync` | Brave Search への接続を任意にする。失敗したら `Debug.WriteLine` に記録し、CAD ツールだけで続行する (M-4) |
| 3-5 | `ChatAgent.cs` のシステムプロンプト | `DrawFreeLine` を `DrawFreeFormCurve` に直し、`DrawRoundedRectangle` / `DrawPolylineOrPolygon` / `DrawCurve` を追記する (L-10)。ツール名はハードコードせず、`GetCadTools()` から自動生成することも検討する |
| 3-6 | `Toolbox.cs` / `Model.cs` | 色名を `Color.FromName(...).IsKnownColor` で検証し、不正な色名には例外 (エラー文字列) を返す (L-11) |
| 3-7 | `Animator.cs` | 停止時に `timer.Dispose()` する (L-12) |
| 3-8 | `MainForm.cs` | `AppendResponse` を `SetResponse` に改名する。または実際に追記する動作に変える (L-13) |
| 3-9 | (任意) `Toolbox.cs` | 描画ツールが結果の文字列を返すようにする (レビュー 4.3 節) |

**完了条件** (Windows 実機)
- デバッガー接続時に「円を 3 つ描いて」と指示しても、クロススレッド例外が発生しない
- `BRAVE_API_KEY` が未設定でも描画できる
- ウィンドウを閉じるとプロセスが速やかに終了する (デッドロックしない)

---

### フェーズ 4: コンソール版と MCP サーバーの品質改善

**対応する指摘**: M-7, L-1, L-2, L-3, L-4, L-15, L-16, L-18

| タスク | 対象 | 内容 |
|---|---|---|
| 4-1 | FCAIAgent2〜5, FCAIAgent, FCAIChat4/5, FCAICad1, `Documents/tutorial.1.2〜3.2.md` | `GetEndPoint` / `GetKey` のコメント例を入れ替え、「ここではハードコードする」を「ハードコードする場合は以下のように書く」に修正する (L-1) |
| 4-2 | 同上 | システムメッセージを 1 ターン目に送る処理を見直す (L-2)。<br>**案**: `instructions` に統一して送信処理を削除する。ただし FCAIAgent3 のチュートリアル (1.3) の説明に関わるので、削除するか、応答を表示しないようにするだけにするかは、チュートリアルの記述と合わせて決める |
| 4-3 | FCAIAgent3〜5 | `RunAsync` の引数名 `thread` を `session` に統一する (L-3) |
| 4-4 | FCAIAgent5, FCAIAgent | `weartherMcpClient` を `weatherMcpClient` に直す (L-4) |
| 4-5 | FCAIAgent5, FCAIAgent | `GetMcpTools()` で途中の接続が失敗したら、接続済みのクライアントを破棄する。メイン処理は `try/finally` で破棄する (M-7) |
| 4-6 | FCAIAgent5, FCAIAgent, `Documents/tutorial.3.2.md` | filesystem MCP のパス `"/work"` を、`Environment.CurrentDirectory` など実在するパスにする。または「環境に合わせて変更」と明記する (M-7) |
| 4-7 | `McpServer.Con/Program.cs`, `Documents/tutorial.2.1.md` | `timezone` 引数に `[Description("IANA または Windows のタイムゾーン ID (例: Asia/Tokyo)")]` を付ける。`catch` を `TimeZoneNotFoundException` / `InvalidTimeZoneException` に限定する (L-15) |
| 4-8 | `README.md` | FCAIAgent (最終形) と FCAIAgent5 の違い (「旧/新」コメントの有無) を説明する (L-18) |
| 4-9 | (対応しない) | L-16 (天気ツールの完全一致判定) は教材として意図的なものとみなし、対応しない |

**完了条件**
- 対象となるコンソールプロジェクトをすべてビルドして、警告 0
- チュートリアルのコード片と、対応するプロジェクトの `Program.cs` が一致している (下記 5 章の確認手順)

---

### フェーズ 5: 構成の改善 (任意・中長期)

**対応する指摘**: M-8, L-8, L-9、レビュー 4.1 節

| タスク | 内容 |
|---|---|
| 5-1 | FCAIChat1〜5 の `appsettings.json` で、データベース名をプロジェクトごとに分ける (`aspnet-FCAIChat1` など) (M-8) |
| 5-2 | `Directory.Packages.props` を導入し、パッケージバージョンを一元管理する (Central Package Management) |
| 5-3 | `chat.js` の部分ビュー取得を POST にする。またはクライアント側で組み立てる (L-8) |
| 5-4 | 未ログインのとき、送信欄を無効にしてログインを促す表示を出す (L-9) |
| 5-5 | チュートリアルのコード片とプロジェクトのコードの差分を検出するスクリプトを `tools/` に追加する |
| 5-6 | FCAIChat / FCAICad のモデル名とエンドポイントを `appsettings.json` / User Secrets に移す (コンソール版は教材の分かりやすさを優先して定数のままにする) |

---

## 4. 作業順序とコミット単位

```
D-1〜D-6 の決定
   │
   ▼
フェーズ 1 (ドキュメント・設定)  ──► commit: "チュートリアルと設定の MCP 接続先・記述を実装に合わせて修正"
   │
   ├──► フェーズ 2 (FCAIChat4/5) ──► commit: "FCAIChat4/5 のエージェントのライフサイクルを修正"
   │
   ├──► フェーズ 3 (FCAICad1)    ──► commit: "FCAICad1 の UI スレッド処理と例外処理を修正"
   │
   └──► フェーズ 4 (コンソール/MCP) ──► commit: "コンソール版と MCP サーバーのコメント・後始末を修正"
            │
            ▼
       フェーズ 5 (任意)
```

フェーズ 2〜4 は互いに独立しているので、順序を入れ替えても並行して進めてもよい。ただし、フェーズ 4 のタスク 4-1 と 4-2 は FCAIChat4/5 と FCAICad1 のファイルにも手を入れるので、フェーズ 2 / 3 と同時に進める場合は競合に注意する。

---

## 5. 検証方法

### 5.1 ビルド

```bash
# Linux / WSL (FCAICad1 を除く)
for p in FCAIAgent FCAIAgent1 FCAIAgent2 FCAIAgent3 FCAIAgent4 FCAIAgent5 \
         McpServer.Con McpServer.Sse FCAIChat1 FCAIChat2 FCAIChat3 FCAIChat4 FCAIChat5; do
  dotnet build $p -warnaserror || echo "FAILED: $p"
done

# Windows (全体)
dotnet build Shos.AIAgentSample.sln -warnaserror
```

### 5.2 実行確認 (手動)

| 対象 | 確認内容 |
|---|---|
| McpServer.Sse + MCP Inspector | `http://localhost:3001/` に Streamable HTTP で接続し、`GetWeatherForecast` を実行できる |
| FCAIAgent4 / 5 | 「今何時?」「東京都の天気は?」に、ツールを呼び出して答える |
| FCAIChat4 / 5 | フェーズ 2 の完了条件 |
| FCAICad1 | フェーズ 3 の完了条件 (Windows) |

### 5.3 ドキュメントとコードの整合性

```bash
grep -rn "3001/sse\|MCPServer.Con\|llama3.2\|\.NET-9.0" --include=*.md --include=*.json --include=*.cs . | grep -v Reviews/
grep -rn "deploymentName = " --include=*.cs --include=*.md . | grep -v Reviews/   # 1 種類にそろっていること
```

---

## 6. リスクと留意事項

| リスク | 対策 |
|---|---|
| チュートリアルのスクリーンショット (`Documents/Images/`) がコード修正後の画面と合わなくなる | フェーズ 1 で、影響する画像を洗い出す。MCP Inspector の接続画面など、差し替えが必要な画像を一覧にする |
| セミナー資料 PDF (`Documents/*.pdf`) の記述と食い違う | PDF は今回の修正対象外とし、README に「PDF は執筆時点の内容」と注記する |
| Microsoft Agent Framework / MCP SDK の API が今後も変わる | フェーズ 5-2 (CPM) と 5-5 (差分検出スクリプト) で追従のコストを下げる |
| フェーズ 2 の `[Authorize]` 追加で、未ログインでは試せなくなる | README の FCAIChat の実行手順に、ユーザー登録とログインの手順を明記する |
| FCAICad1 の修正は Linux では検証できない | フェーズ 3 は Windows 環境で検証してからコミットする |

---

## 7. 実施結果 (2026年11月9日)

フェーズ 1〜4 を実施した (未コミット)。フェーズ 5 は未着手。

### 計画から変更した点

| タスク | 変更内容と理由 |
|---|---|
| 2-2 〜 2-5 | `ChatAgentManager` はユーザーごとに接続 ID の集合を持ち、**最後の接続が切れた時点で**会話を保存してエージェントを破棄する方式にした。破棄しないと MCP の子プロセスがアプリ終了まで残り、保存した会話を復元する場面もなくなるため。再接続時は、前のエージェントの保存完了を待ってから復元する |
| 2-3 | FCAIChat4 は `[Authorize]` を付けず、キーは `Context.UserIdentifier ?? Context.ConnectionId` とした (ステップ 4 の範囲を広げないため) |
| 2-7 | FCAIChat5 の `SendMessage` の引数から `user` を削除し、`chat.js` も合わせて変更した |
| 2-8 | `Clear` 後の再表示は、`chat.js` での即時 `location.reload()` をやめ、サーバーからの `Cleared` 通知で全クライアントを再表示する方式にした |
| 3-9 (任意) | 実施した。描画ツールは結果の文字列を返し、不正な色名の場合は `Error: ...` を返す (3-6 と一体で実施) |
| 4-2 (L-2) | **変更しない**。チュートリアル 1.3 では、最初のシステムメッセージへの応答を「エージェントから先に話しかける」演出として使っている (RPG の例) ため |
| 4-6 (M-7) | **`"/work"` を維持し、コメントのみ追記**。チュートリアル 3.2 は事前に `\work` フォルダーを作る手順になっており、Windows では `/work` が `C:\work` を指す。レビューの指摘はこの点の見落とし |
| 追加 | チュートリアル 3.2 のコードが古い API (`agent.GetNewThread()`、`ChatClientAgentOptions`) のままでコンパイルできなかったため、FCAIAgent5 の `Program.cs` と同期した。あわせて 1.3 / 3.1 も同期し、7 本のチュートリアルのコード片が対応する `Program.cs` と完全に一致することを機械的に確認した |
| 追加 | `CLAUDE.md` の FCAIChat の説明 (初回作成時の誤り) を実装に合わせて修正した |

### 検証結果

- ビルド (.NET SDK 10.0.114 / Linux): FCAIAgent, FCAIAgent3〜5, McpServer.Con, FCAIChat4/5, FCAICad1 (`-p:EnableWindowsTargeting=true`) がすべて成功。警告はネットワーク未接続時の NU1900 のみ
- 5.3 の整合性チェック: `Reviews/` と `Plans/` を除き、古い記述の残りは 0 件
- **未実施**: 5.2 の実行確認 (LLM / MCP サーバー / ブラウザー / Windows 実機) はすべて未実施。特にフェーズ 2 の完了条件とフェーズ 3 (Windows) は実機での確認が必要

## 8. 指摘と計画の対応表

| 指摘 | フェーズ / タスク | 指摘 | フェーズ / タスク |
|---|---|---|---|
| H-1 | 1-1, 1-2 | L-1 | 4-1 |
| H-2 | 2-2, 2-3 | L-2 | 4-2 |
| H-3 | 2-4, 2-5 | L-3 | 4-3 |
| H-4 | 3-1 | L-4 | 4-4 |
| H-5 | 1-4, 1-5 | L-5 | 2-9 |
| M-1 | 1-7 | L-6 | 2-9 |
| M-2 | 1-6, 1-8 | L-7 | 2-8 |
| M-3 | 2-1 | L-8 | 5-3 |
| M-4 | 2-6, 3-3, 3-4 | L-9 | 5-4 |
| M-5 | 3-2 | L-10 | 3-5 |
| M-6 | 2-7, 2-8 | L-11 | 3-6 |
| M-7 | 1-9, 4-5, 4-6 | L-12 | 3-7 |
| M-8 | 5-1 | L-13 | 3-8 |
| | | L-14 | 1-3 |
| | | L-15 | 4-7 |
| | | L-16 | 対応しない (4-9) |
| | | L-17 | 1-10 |
| | | L-18 | 4-8 |
