/*
dotnet add package Microsoft.Agents.AI --prerelease
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Agents.AI.OpenAI --prerelease
dotnet add package ModelContextProtocol --prerelease
dotnet add package OllamaSharp
 */

#define AZURE_OPENAI
#if AZURE_OPENAI
// Azure OpenAI のクライアント用
using Azure;
using Azure.AI.OpenAI;
#else // AZURE_OPENAI
// Ollama 用
using OllamaSharp;
#endif // AZURE_OPENAI

// Microsoft Agent Framework 用
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
// MCP クライアントとツールを利用するための名前空間
using ModelContextProtocol.Client;
// Debug.WriteLine を使うための名前空間
using System.Diagnostics;

namespace FCAICad;

// AI エージェントの基底クラス: チャット クライアントの生成、ツール (MCP を含む) の取得、会話のセッション管理を行う
// 初回の GetResponseAsync で Start が呼ばれ、エージェントとセッションが作成される
public abstract class ChatAgent : IAsyncDisposable
{
    // 終了時に破棄する MCP クライアントと、初回の呼び出しかどうか
    IEnumerable<McpClient> mcpClients           = [];
    bool                   isFirst              = true;

    // AIAgent を公開してスレッドのシリアライズに使用
    public AIAgent?           Agent        { get; private set; }
    // 複数ターンに対応するために AgentSession (会話の状態・履歴などを管理) を作成
    public AgentSession?      Session      { get; set; }
    // エージェント名と指示
    public abstract string    Name         { get; }
    protected abstract string Instructions { get; }
    // エージェントのシステムロールに与える文脈的な指示
    protected abstract string SystemPrompt { get; }

    public async ValueTask DisposeAsync()
    {
        // 終了処理 MCP クライアントを破棄
        foreach (var mcpClient in mcpClients)
            await mcpClient.DisposeAsync();
    }

    // ユーザーのプロンプトをエージェントに送り、応答のテキストを返す (初回はエージェントを準備してから送る)
    public async Task<string> GetResponseAsync(string userPrompt)
    {
        await Start();
        // ユーザープロンプトをエージェントスレッドに送信し、応答を取得
        return await RunAgentAsync(ToUserMessage(userPrompt)) ?? string.Empty;

        static ChatMessage ToUserMessage(string userPrompt) => new(ChatRole.User, userPrompt);
    }

    // ファクトリ関数 (派生クラスで、使用する LLM のチャット クライアントを生成する)
    protected abstract IChatClient GetChatClient();
    
    // エージェントに渡すツールと、それを提供する MCP クライアントを取得する (既定ではツール無し)
    protected virtual Task<(IEnumerable<McpClient>, IEnumerable<AITool>)> GetToolsAsync()
        => Task.FromResult<(IEnumerable<McpClient>, IEnumerable<AITool>)>(([], []));

    // 初回だけ、ツールを取得してエージェントとセッションを作成し、システム メッセージを送る
    async Task Start()
    {
        if (!isFirst)
            return;

        var (mcpClients, tools) = await GetToolsAsync();
        try {
            Agent   = CreateAgent(tools);
            Session = await Agent.CreateSessionAsync();

            // システムメッセージを最初に送信
            await SendSystemMessageAsync();
        } catch {
            // 開始に失敗したら MCP クライアントを破棄し、次回の呼び出しで再試行する
            foreach (var mcpClient in mcpClients)
                await mcpClient.DisposeAsync();
            Agent   = null;
            Session = null;
            throw;
        }

        this.mcpClients = mcpClients;
        isFirst         = false;

        // チャット クライアント・指示・ツールから ChatClientAgent を作成
        AIAgent CreateAgent(IEnumerable<AITool> tools)
                => new ChatClientAgent(
                    chatClient  : GetChatClient(),
                    instructions: Instructions,
                    name        : Name,
                    description : null,
                    tools       : tools.Any() ? tools.ToList() : null
                );

        // システムメッセージを作成して送信
        async Task SendSystemMessageAsync() => await RunAgentAsync(new(ChatRole.System, SystemPrompt));
    }

    // エージェントに ChatMessage を投げて応答を取得
    async Task<string> RunAgentAsync(ChatMessage chatMessage)
    {
        if (Agent is null)
            return string.Empty;

        try {
            var response = await Agent.RunAsync(chatMessage, Session);
            return response?.Text ?? string.Empty;
        } catch (Exception ex) {
            // エラーを握りつぶさず、内容を応答欄に表示する (詳細はデバッグ出力へ)
            Debug.WriteLine($"Error running agent: {ex}");
            return $"(エラー: {ex.Message})";
        }
    }
}

// CAD を操作する AI エージェント: Toolbox の CAD ツール (と、使えれば Brave Search の MCP ツール) で図を描く
public class MyChatAgent : ChatAgent
{
    // エージェント名と指示
    public override string    Name         => "イラストレーター";
    protected override string Instructions => "ベクターイラストレーターとして、CAD の図形描画ツールを使って美しく分かりやすいイラストを描いてください。";
    // エージェントのシステムロールに与える文脈的な指示
    protected override string SystemPrompt => @$"
あなたは一流のベクターイラストレーターです。線・円・多角形・曲線などの図形を組み合わせて、
フラットデザインの温かく親しみやすいイラスト (絵本やアイコンのような画風) を描くことを得意としています。
目的: 一目で何を描いたか分かり、配色と構図が美しいイラストを描くこと。

## キャンバス
- 作図領域は幅{Toolbox.GetPaperSize().Width}・高さ{Toolbox.GetPaperSize().Height}。左上が原点で、右が X+、下が Y+。
- 用紙の地の色は白です。空や地面などの背景は、用紙を覆う塗りつぶしの矩形から描き始めます。
- 後から描いた図形ほど上 (手前) に重なります。必ず奥から手前の順に描いてください。
- 文字を描くツールと半透明はありません。形と色だけで表現してください。

## ツールの性質
- 塗りつぶし (isFilled: true) は、輪郭線と同じ色で描かれます。
  輪郭線の色を塗りと変えたいときは、同じ図形を 2 回描きます (1 回目: 明るい色で塗りつぶす → 2 回目: 同じ座標を isFilled: false にして濃い色でなぞる)。
- 線は輪郭の中心に引かれるので、図形は線幅の半分だけ外側に広がります。輪郭線の無い塗りだけの図形は、線幅 3 で描くと指定どおりの大きさになります。
- 塗りつぶした有機的な形 (葉・雲・体など) は、閉じた `DrawCurve` (isClosed: true, isFilled: true) で描きます。点は 5〜12 個で、外周に沿って順に並べます。
- 開いた `DrawCurve` (isClosed: false) は、口・眉・茎・波などの滑らかな線に使います。
- `DrawFreeFormCurve` は塗りつぶせません。髪・毛並み・模様などの線にだけ使い、点の間隔は 10 以上空けてください。
- 直線的な形 (建物・屋根・山など) は `DrawPolylineOrPolygon` で描きます。
- 線幅は 3〜30 の範囲で指定します。目安: 外側の輪郭線 6〜10、内側の線・細部 3〜5、太い枝や道など 12〜30。
- 消しゴムと取り消しのツールはありません。
- 結果が `Error` で始まる場合は、原因を直して描き直してください。

## 構図
- 主役を 1 つ決め、画面の 40〜60% ほどの大きさで、中央かやや中央寄りに置きます。
- 用紙の端から 5〜10% は余白にし、主役や大事な部品がはみ出さないようにします (背景の塗りは用紙全体に広げてかまいません)。
- 地平線や地面は、高さの約 2/3 の位置に置きます。
- 主役に加えて、背景と小物を 2〜4 個描き、場面として成り立たせます (特に指示がなければ、少し多めに描き込む)。
- 生き物の左右の目・耳や、建物の窓など、対になる部品は座標を計算して左右対称にします。
- 地面に立つ物は地面に接するように置き、浮かないようにします。

## 分かりやすさ
- その物らしさを決める特徴を必ず描き、少し大げさにします (例: 猫は三角の耳とひげ、うさぎは長い耳、車はタイヤと窓、家は三角屋根と窓とドア)。
- 主役は背景と明るさがはっきり違う色にし、隣り合う部品に同じ色を使いません。
- 細部は小さすぎると見えません。目や小物の半径は 15 以上、細い線も線幅 4 以上にします。
- キャラクターは頭を大きめ (体と同じくらい) にし、目を大きくすると親しみやすくなります。

## 立体感と画風の統一
- 光は左上から当たるものとし、すべての物で影とハイライトの向きをそろえます。
- 影: 本体と同じ形を右下に少しずらし、本体より濃い同系色で先に描いてから、その上に本体を描きます。
- 接地影: 地面に立つ物の足元に、地面より少し濃い色の平たい楕円を、本体より先に描きます。
- ハイライト: 本体の左上に、一段明るい色 (または White) の小さな円や曲線を描きます。目の黒目にも White の小さな円を入れます。
- 輪郭線を付けるなら主役と小物のすべてに付け、色は本体と同系色の濃い色にします (例: Gold → DarkGoldenrod、LightPink → PaleVioletRed)。背景 (空・地面・遠くの山) には輪郭線を付けません。

## 部品の描き方の例
- 空と地面: 用紙全体を淡い空色の矩形で塗り、地平線から下を草色や土色の矩形で塗ります。
- 雲: White の円を 3〜5 個、中央を大きくして横に少しずつ重ねます。
- 太陽: Gold の円の周りに、Orange の短い線を 8 本、等間隔の角度で放射状に描きます。
- 木: Sienna の幹 (下が少し広い多角形) → 濃い緑の円を 3〜4 個重ねて葉の塊 → 左上に明るい緑の円を重ねてハイライト。
- 家: 壁は矩形、屋根は壁より少し幅の広い三角形、窓とドアは角丸矩形。窓には十字の窓枠を線で描きます。
- 山: 三角形を 2〜3 個、奥ほど淡い色にして重ね、頂上に White の雪 (ぎざぎざの多角形) をのせます。
- 花: 中心の円の周りに花びらの円を 5 個、等間隔の角度で並べ、茎は開いた曲線、葉は閉じた曲線で描きます。
- 顔: 頭 → 耳 → 白目の楕円 → 黒目の円 → 黒目のハイライト → 頬 (LightPink の楕円) → 鼻 → 口 (開いた曲線) の順に描きます。
- 水: 青系の矩形の上に、明るい色の短い波線を数本描きます。

## 座標の決め方
- 主役の中心と大きさを先に決め、部品の座標はそこからの比率で計算します (例: 頭の半径を R とすると、目は中心から左右に 0.35R・上に 0.1R の位置で、半径 0.15R)。
- 円周上に並べる部品 (花びら・太陽の光など) は、角度から座標を計算して等間隔にします。

## 配色
- 主に使う色は 3〜5 色に絞り、同じ系統の色で明暗を付けます (例: Gold → Orange → DarkOrange)。
- 広い面積 (空・地面・背景) には淡い色、主役には鮮やかな色、輪郭線には濃い色を使います。純粋な Black は避けます。
- 配色の例:
  - 空: AliceBlue, LightSkyBlue, SkyBlue / 夕焼け: PeachPuff, LightSalmon, Coral / 夜: MidnightBlue, DarkSlateBlue
  - 草木: PaleGreen, MediumSeaGreen, SeaGreen, DarkGreen, OliveDrab
  - 地面・木: Wheat, BurlyWood, Peru, Sienna, SaddleBrown
  - 水: PaleTurquoise, MediumTurquoise, SteelBlue
  - 肌: PeachPuff, NavajoWhite, Bisque / 頬: LightPink, HotPink
  - アクセント: Tomato, Gold, DeepSkyBlue, MediumOrchid
- 使える色: {GetAvailableColorNameList()}

## 描画の手順
1. 描く前に、構図 (主役・背景・小物の位置と大きさ)、配色 (使う色 3〜5 色)、光の向きを決めます。
2. 次の順に描きます: 背景 (空・地面) → 遠景 → 接地影 → 主役の影 → 主役の大きな形 → 主役の部品 (目・模様など) → 輪郭線 → ハイライト → 手前の小物 (前景の草など)。
3. 図形の数の目安は全体で 30〜80 個です。主役が 10 個以下の図形でできているなら、描き込みが足りません。
4. 描き終えたら、ツールの結果に返る座標を見て、部品が欠けていないか、重ね順が間違っていないか、ずれ・はみ出しがないかを見直し、必要なら描き足します。
5. 最後に、何をどう描いたかを日本語で 1〜2 文で説明します。

## 修正の依頼
- 「〜を追加して」と言われたら、今の絵を消さずに描き足します。
- 「〜を消して」「〜を変えて」と言われたら、その部分を周りの背景と同じ色の図形で覆ってから描き直します。
- 全体を描き直すように言われたときだけ、ClearAll してから描きます。

## ルール
- 特に指示がなければ、描かれたものは消さないでください (ClearAll しない)。
- ユーザーから指示があるまでは何も描画しないでください。指示があったら直ちに描画してください。
- 指示が曖昧なときは、質問せずに妥当な解釈で描いてください。
";
    // システム プロンプトに埋め込む、使用できる色名のカンマ区切りの一覧
    static string GetAvailableColorNameList()
    {
        var availableColorNames = Toolbox.GetAvailableColors();
        return string.Join(", ", availableColorNames);
    }

#if AZURE_OPENAI
    //Azure OpenAI を使う場合のクライアント生成
    protected override IChatClient GetChatClient()
    {
        // 使用するモデルを指定
        const string deploymentName = "MAI-Thinking-1";
        var azureOpenAIEndPoint = GetEndPoint();
        var openAIApiKey = GetKey();
        var credential = new AzureKeyCredential(openAIApiKey);

        var azureOpenAIClient = new AzureOpenAIClient(new Uri(azureOpenAIEndPoint), credential);
        // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
        IChatClient chatClient = azureOpenAIClient.GetChatClient(deploymentName)
                                                  .AsIChatClient()
                                                  .AsBuilder()
                                                  .UseFunctionInvocation() // ツール呼び出しを使う
                                                  .Build();
        return chatClient;

        static string GetEndPoint()
        {
            const string AzureOpenAIEndpointEnvironmentVariable = "AZURE_OPENAI_ENDPOINT";
            var azureOpenAIEndPoint = Environment.GetEnvironmentVariable(AzureOpenAIEndpointEnvironmentVariable);
            if (string.IsNullOrEmpty(azureOpenAIEndPoint))
                throw new InvalidOperationException($"Please set the {AzureOpenAIEndpointEnvironmentVariable} environment variable.");
            return azureOpenAIEndPoint;

            // 上記のように、セキュリティ上 Azure OpenAI のエンドポイントは環境変数から取得するのが望ましい。ハードコードする場合は、上記の代わりに以下のように書く
            // 例: https://your-resource-name.openai.azure.com/
            //return @"[Azure OpenAI のエンドポイント]";
        }

        static string GetKey()
        {
            const string AzureOpenAIApiKeyEnvironmentVariable = "AZURE_OPENAI_API_KEY";
            var openAIApiKey = Environment.GetEnvironmentVariable(AzureOpenAIApiKeyEnvironmentVariable);
            if (string.IsNullOrEmpty(openAIApiKey))
                throw new InvalidOperationException($"Please set the {AzureOpenAIApiKeyEnvironmentVariable} environment variable.");
            return openAIApiKey;

            // 上記のように、セキュリティ上 Azure OpenAI の APIキーは環境変数から取得するのが望ましい。ハードコードする場合は、上記の代わりに以下のように書く
            // 例: 1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef
            //return @"[Azure OpenAI の APIキー]";
        }
    }
#else // AZURE_OPENAI
    // Ollama を使う場合のクライアント生成(ローカルの Ollama サーバーに接続)
    protected override IChatClient GetChatClient()
    {
        var uri = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri);
        // 使用するモデルを指定
        // クラウドベースのモデルを使用(実行速度の向上のため)
        // ローカル LLM を使用する場合は "gemma3:latest" などに変更してください
        ollama.SelectedModel = "minimax-m3:cloud";

        // IChatClient インターフェイスに変換して、ツール呼び出しを有効にしてビルド
        IChatClient chatClient = ollama;
        chatClient = chatClient.AsBuilder()
                               .UseFunctionInvocation() // ツール呼び出しを使う
                               .Build();
        return chatClient;
    }
#endif // AZURE_OPENAI

    // CAD 操作用のツールと、MCP サーバーのツールを取得
    // - Brave Search の MCP サーバーは任意 (接続できない場合は CAD ツールだけで動作する)
    // - 戻り値は (IEnumerable<McpClient>, IEnumerable<AITool>) で、終了時にすべての McpClient について DisposeAsync() を呼ぶ必要がある
    protected override async Task<(IEnumerable<McpClient>, IEnumerable<AITool>)> GetToolsAsync()
    {
        McpClient? client = null;
        try {
            client = await McpClient.CreateAsync(GetBraveSearchToolClientTransport());
            IList<McpClientTool> mcpTools = await client.ListToolsAsync();
            foreach (var tool in mcpTools)
                Debug.WriteLine($"{tool.Name} ({tool.Description})");

            return ([client], [.. GetCadTools(), .. mcpTools.Cast<AITool>()]);
        } catch (Exception ex) {
            // BRAVE_API_KEY が未設定、npx が見つからない、などの場合
            Debug.WriteLine($"Brave Search MCP server is not available: {ex.Message}");
            if (client is not null)
                await client.DisposeAsync();
            return ([], GetCadTools());
        }

        // BraveSearch ツールを使うためのクライアント生成
        static IClientTransport GetBraveSearchToolClientTransport()
        {
            return new StdioClientTransport(new() {
                        Name                 = "braveSearch",
                        Command              = "npx",
                        Arguments            = ["-y", "@modelcontextprotocol/server-brave-search"],
                        EnvironmentVariables = new Dictionary<string, string?> {
                            ["BRAVE_API_KEY"] = GetKey()
                        }
                   });

            static string GetKey()
            {
                const string braveApiKeyEnvironmentVariable = "BRAVE_API_KEY";
                var braveApiKey = Environment.GetEnvironmentVariable(braveApiKeyEnvironmentVariable);
                if (string.IsNullOrEmpty(braveApiKey))
                    throw new InvalidOperationException($"Please set the {braveApiKeyEnvironmentVariable} environment variable.");
                return braveApiKey;

                // 上記のように、セキュリティ上 Brave の APIキーは環境変数から取得するのが望ましい。ハードコードする場合は、上記の代わりに以下のように書く
                //return @"[Brave の APIキー]";
            }
        }

        // CAD 操作用のツール群を取得
        static IEnumerable<AITool> GetCadTools()
            => [AIFunctionFactory.Create(Toolbox.GetPaperSize         ),
                AIFunctionFactory.Create(Toolbox.GetAvailableColors   ),
                AIFunctionFactory.Create(Toolbox.ClearAll             ),
                AIFunctionFactory.Create(Toolbox.DrawLine             ),
                AIFunctionFactory.Create(Toolbox.DrawRoundedRectangle ),
                AIFunctionFactory.Create(Toolbox.DrawRectangle        ),
                AIFunctionFactory.Create(Toolbox.DrawCircle           ),
                AIFunctionFactory.Create(Toolbox.DrawEllipse          ),
                AIFunctionFactory.Create(Toolbox.DrawPolylineOrPolygon),
                AIFunctionFactory.Create(Toolbox.DrawCurve            ),
                AIFunctionFactory.Create(Toolbox.DrawFreeFormCurve    )];
    }
}
