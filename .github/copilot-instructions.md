# copilot-instructions.md

## プロジェクト概要

このリポジトリは **FCAIChat3** を中心とした ASP.NET Core プロジェクトで構成されています。FCAIChat3 は Razor Pages + SignalR + Entity Framework Core を使用したリアルタイムチャットアプリケーションです。

### 技術スタック
- **ターゲットフレームワーク**: `net9.0`
- **UI**: Razor Pages
- **リアルタイム通信**: SignalR
- **データアクセス**: Entity Framework Core (SQL Server)
- **認証**: ASP.NET Core Identity

## 開発方針

### 日時・タイムゾーンの取り扱い

#### サーバ側の基本方針
- サーバ内部では **UTC を基準** として日時を扱うこと
- 日時の取得には `DateTime.UtcNow` を使用する
- データベースに保存する日時は UTC とする

#### EF Core から読み出した DateTime の取り扱い
- EF Core から読み出した `DateTime` の `Kind` プロパティは `DateTimeKind.Unspecified` になる場合がある
- UTC として扱う場合は `DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)` を使用して明示的に指定する
- **注意**: `ToUniversalTime()` はローカル時刻を UTC に変換するメソッドであり、既に UTC の値に対して使用すると誤った結果になる可能性がある

#### クライアント側への日時の送信
- JavaScript 側でローカル時刻表示するため、サーバからは **ISO 8601 形式** (`ToString("o")`) で UTC を送信する
- ISO 8601 形式では末尾に `Z`（UTC を示す）またはタイムゾーンオフセットが付く
- 例: `2025-11-05T08:15:36.1234567Z`

```csharp
// 正しい例
var createdAt = DateTime.UtcNow;
createdAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
var isoString = createdAt.ToString("o"); // "2025-11-05T08:15:36.1234567Z"
```

### Razor 部分ビュー (Partial View) の扱い

#### 部分ビューの定義
- `_MessagePartial.cshtml` のような部分ビューには **`@page` ディレクティブを付けない**
- 部分ビューは独立したページではなく、他のページから埋め込まれるコンポーネントである

#### 部分ビューの呼び出し方法
推奨される呼び出し方法:
```cshtml
<!-- タグヘルパー構文（推奨） -->
<partial name="_MessagePartial" model="message" />

<!-- または HTML ヘルパー構文 -->
@await Html.PartialAsync("_MessagePartial", message)
```

### SignalR / chat.js の連携

#### Hub の設定
- Hub エンドポイント: `/chatHub`
- Hub クラス: `ChatHub` (namespace: `FCAIChat.Hubs`)
- `Program.cs` での設定: `app.MapHub<ChatHub>("/chatHub");`

#### メッセージフロー
1. クライアントが `SendMessage(user, message)` を呼び出す
2. Hub がメッセージを受信し、データベースに保存
3. Hub が全クライアントに `ReceiveMessage(user, message, createdAt)` をブロードキャスト
   - `createdAt` は `"o"` 形式の UTC 文字列（例: `"2025-11-05T08:15:36.1234567Z"`）

```csharp
// Hub 側の実装例
public async Task SendMessage(string user, string message)
{
    var createdAt = DateTime.UtcNow;
    createdAt = DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
    
    // DB 保存
    dbContext.Messages.Add(new () { UserName = user, Content = message, CreatedAt = createdAt });
    await dbContext.SaveChangesAsync();
    
    // ブロードキャスト
    await Clients.All.SendAsync("ReceiveMessage", user, message, createdAt.ToString("o"));
}
```

#### クライアント側での部分ビュー取得パターン
- `/Messages/RenderPartial` エンドポイントを使用して、サーバ側でレンダリングした HTML 断片を取得
- `fetch` API で取得した HTML を DOM に挿入する
- これにより、サーバ側のロジック（認証状態による表示切替など）を活用できる

```javascript
// chat.js の実装例
async function addMessagePartial(user, message, createdAt) {
    const url = `/Messages/RenderPartial?Message.UserName=${encodeURIComponent(user)}&Message.Content=${encodeURIComponent(message)}&Message.CreatedAt=${encodeURIComponent(createdAt)}`;
    const res = await fetch(url, { method: 'GET', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
    const html = await res.text();
    // DOM に挿入
    container.appendChild(html);
}
```

### エスケープ・サニタイズ

#### 基本方針
- Razor ビューでの出力は **`Html.Encode`** を使用してエスケープすることを基本とする
- Razor の `@` 構文は自動的にエスケープされる

#### Html.Raw の使用
- `Html.Raw` は **信頼できる内容のみ** に使用する
- ユーザー入力を直接 `Html.Raw` に渡してはならない
- 例外的に、既にエスケープ済みの HTML を出力する場合のみ使用する

```cshtml
<!-- 安全な例: エスケープしてから改行を <br> に変換 -->
<span>@Html.Raw(Html.Encode(Model.Content).Replace("&#xA;", "<br>"))</span>

<!-- 危険な例: ユーザー入力を直接 Raw で出力（XSS 脆弱性） -->
<span>@Html.Raw(Model.Content)</span> <!-- 絶対に避ける -->
```

## 開発フロー・品質ゲート

### コード変更の基本原則
- **小さく・可検証** な変更を心がける
- 一つの変更は一つの目的に集中する
- 変更後は必ず動作確認を行う

### コミットメッセージのフォーマット

コミットメッセージは以下の形式を推奨:

```
<type>: <短い説明>

<詳細な説明（必要に応じて）>
```

**type の例**:
- `fix:` - バグ修正
- `feat:` - 新機能追加
- `docs:` - ドキュメント変更
- `refactor:` - リファクタリング（機能変更なし）
- `style:` - コードスタイル修正（動作変更なし）
- `test:` - テスト追加・修正
- `chore:` - ビルド設定やツール変更

例:
```
feat: SignalR でメッセージ送信機能を追加

- ChatHub に SendMessage メソッドを実装
- chat.js にクライアント側の送信処理を追加
```

### 変更後の最低限確認項目

コード変更を行った場合、以下の項目を必ず確認する:

1. **ビルドが通る**
   ```bash
   dotnet build FCAIChat3/FCAIChat3.csproj
   ```

2. **主要ページの表示**
   - アプリケーションを起動して、変更に関連するページが正常に表示されることを確認
   ```bash
   dotnet run --project FCAIChat3/FCAIChat3.csproj
   ```

3. **SignalR の送受信の簡易動作確認**
   - チャットメッセージの送信が正常に動作するか
   - メッセージが他のクライアントに届くか
   - 日時がローカル時刻で正しく表示されるか

4. **データベースマイグレーション（モデル変更時）**
   ```bash
   dotnet ef migrations add <MigrationName> --project FCAIChat3
   dotnet ef database update --project FCAIChat3
   ```

## セキュリティ注意事項

### 秘密情報の管理
- **appsettings.json に秘密情報（パスワード、API キー、接続文字列など）を直書きしない**
- 開発環境では User Secrets を使用: `dotnet user-secrets set "KeyName" "Value" --project FCAIChat3`
- 本番環境では環境変数または Azure Key Vault などのシークレット管理サービスを使用

### 入力のバリデーション
- ユーザー入力は必ずサーバ側でバリデーションを行う
- クライアント側のバリデーションは UX 向上のため補助的に使用

### SQL インジェクション対策
- Entity Framework Core のパラメータ化されたクエリを使用する
- 生の SQL を使う場合は必ずパラメータ化する

### XSS (クロスサイトスクリプティング) 対策
- 前述の「エスケープ・サニタイズ」の方針を厳守する
- ユーザー入力を DOM に挿入する際は必ずエスケープする

## まとめ

このドキュメントは FCAIChat3 プロジェクトの開発における重要な方針とベストプラクティスをまとめたものです。新しい機能を追加する際や既存コードを修正する際は、ここに記載された方針に従ってください。
