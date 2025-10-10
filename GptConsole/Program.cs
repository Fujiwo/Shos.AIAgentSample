using Azure;
using Azure.AI.OpenAI;

class Program
{
	static async Task Main()
		=> await Chat(ChooseModel(CreateModels()));

	static async Task Chat(AIChatModel model)
	{
		for (; ;) {
			var userMessage = GetUserMessage();
			await model.Post(userMessage);
			Output(model.Messages.Count > 0 ? model.Messages.Last().Content : "...");
		}
	}

	static AIChatModel ChooseModel(Dictionary<string, AIChatModel> models)
	{
		var modelOptions = string.Join(separator: ", ", models.Select(pair => pair.Key));

		for (; ;) {
			Query($"Choose model ({modelOptions})");
			var modelName = Console.ReadLine()?.Trim() ?? string.Empty;
			if (models.TryGetValue(modelName, out var model))
				return model;
		}
	}

	static Dictionary<string, AIChatModel> CreateModels()
	{
		AIChatModel englishModel = new EnglishModel();
		AIChatModel jsModel		 = new JSModel	   ();
		AIChatModel rpgModel	 = new RpgModel	   ();

		return new Dictionary<string, AIChatModel> {
			[englishModel.Name] = englishModel,
			[jsModel	 .Name] = jsModel     ,
			[rpgModel	 .Name] = rpgModel
		};
	}

	static string GetUserMessage()
	{
		for (; ;) {
			Query("Your message");
            var line = Console.ReadLine()?.Trim();
			if (!string.IsNullOrWhiteSpace(line))
				return line;
		}
	}

	static void Query(string message) => Console.Write($"{message}:");
	static void Output(string message) => Console.WriteLine($"\n{message}\n");
}

public abstract class AIChatModel
{
	const int maximumMessageCount = 10 - 1;
	const string clearCommand = "clear";

	public IList<Message> Messages { get; } = new List<Message>();

	public abstract string Name { get; }
	protected abstract int PromptKind { get; }
	protected abstract string SystemMessage { get; }

	ChatMessage SystemChatMessage => new(ChatRole.System, SystemMessage);

	IList<ChatMessage> ChatMessages => new[] { SystemChatMessage }.Concat(Messages.Select(message => new ChatMessage(message.Owner.Equals("User") ? ChatRole.User : ChatRole.Assistant, message.Content))).ToList();

	ChatCompletionsOptions CurrentChatCompletionsOptions {
		get {
			var chatCompletionsOptions = new ChatCompletionsOptions {
				Temperature = (float)0.7,
				MaxTokens = 800,
				NucleusSamplingFactor = (float)0.95,
				FrequencyPenalty = 0,
				PresencePenalty = 0,
			};
			foreach (var chatMessage in ChatMessages)
				chatCompletionsOptions.Messages.Add(chatMessage);

			return chatCompletionsOptions;
		}
	}

	public async Task Post(string UserMessage)
	{
		if (UserMessage.ToLower().Equals(clearCommand)) {
			Messages.Clear();
			return;
		}

		var message = new Message { PromptKind = PromptKind, Owner = "User", Content = UserMessage };
		Messages.Add(message);

        // Retrieve the OpenAI endpoint from environment variables
        var endPoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        if (string.IsNullOrEmpty(endPoint)) {
            Console.WriteLine("Please set the AZURE_OPENAI_ENDPOINT environment variable.");
            return;
        }

        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey)) {
            Console.WriteLine("Please set the AZURE_OPENAI_API_KEY environment variable.");
            return;
        }

        var credential = new AzureKeyCredential(apiKey);

        var client = new OpenAIClient(new Uri(endPoint), credential);

		var chatCompletionsOptions = CurrentChatCompletionsOptions;

		// ### If streaming is selected
		//Response<StreamingChatCompletions> response = await client.GetChatCompletionsStreamingAsync(deploymentOrModelName: "AzureOpenAISample20230511", chatCompletionsOptions);
		//using StreamingChatCompletions streamingChatCompletions = response.Value;

		const string deploymentName = "202406AzureAIModel";
        // ### If streaming is not selected
        var responseWithoutStream = await client.GetChatCompletionsAsync(deploymentName, chatCompletionsOptions);

		var completions = responseWithoutStream.Value;
		if (completions.Choices.Count > 0) {
			var responseText = completions.Choices[0].Message.Content;
			Messages.Add(message);
			Messages.Add(new Message { PromptKind = PromptKind, Owner = "Assistant", Content = responseText });
		}
	}
}

public class EnglishModel : AIChatModel
{
	public override string Name => "English";

	protected override int PromptKind => 1;

	protected override string SystemMessage => @"あなたは私の英会話の相手として、ネイティブ話者として振る舞ってください。
            私の発言に対して、以下のフォーマットで1回に1つずつ回答します。
            説明は書かないでください。まとめて会話内容を書かないでください。

            #フォーマット:
            【修正】:
            {私の英文を自然な英語に直してください。lang:en}
            【理由】:
            {私の英文と、直した英文の差分で、重要なミスがある場合のみ、40文字以内で、日本語で指摘します。lang:ja}
            【返答】:
            {あなたの会話文です。1回に1つの会話のみ出力します。まずは、私の発言に相槌を打ち、そのあと、私への質問を返してください。lang:en}

            #
            私の最初の会話は、""Hello""です。
            毎回、フォーマットを厳格に守り、【修正】、【理由】、【返答】、を必ず出力してください。";
}

public class JSModel : AIChatModel
{
	public override string Name => "JS";

	protected override int PromptKind => 3;

	protected override string SystemMessage => @"あなたは、世界最高のスペックを持つ JavaScript のプログラマーです。
あなたは、JavaScript を書くことにとても長けていて、どんな要求や質問にも JavaScript を使ったプログラムで答えることができます。
あなたは、他人の書いた JavaScript のコードについて、とても的確にバグや脆弱性の箇所を指摘することができます。
あなたは、他人の書いた JavaScript のコードについて、とても的確なアドバイスを与えることができます。
あなたは、最新の文法を使用した JavaScript のコードを書きます。
あなたは、適切で合理的なプログラムを書きます。
あなたは、分かりやすく合理的な型名、メソッド名、変数名を用います。
あなたは、サンプル コードを示すときには、完全にバグがなく動作するコードを示します。
あなたは、日本語で話します。

あなたは、HTML や CSS、WebGL、SVG、WebSock にもとても長けています。";
}

public class RpgModel : AIChatModel
{
	public override string Name => "Rpg";

	protected override int PromptKind => 2;

	protected override string SystemMessage => @"あなたは世界最高のスペックを持つテキストベースのロールプレイングゲームマスターです。あなたは、ドラゴンクエストに似たロールプレイングゲーム「アレフガルド」のファシリテーターです。
プレイヤーは、ファンタジー世界「アレフガルド」を旅する冒険者です。
「アレフガルド」では心躍る素晴らしい冒険がプレイヤーを待っています。
あなたは、NPCのパワフルでフレンドリーな人間の老魔法使い「ガンダルフ」として、ゲーム世界を案内します。
ガンダルフのイメージは「ロード・オブ・ザ・リング」のガンダルフです。
ガンダルフは、「ロード・オブ・ザ・リング」のガンダルフのように喋ります。
ガンダルフは、長老のような話し方をします。
# ゲームの仕様
* AIゲームマスターとして、ファンタジー世界での冒険体験を提供します。
* プレイヤーはホビットとなり、ガンダルフと一緒にいます。
* 世界各地にフィールドや洞窟、塔などがあります。
* フィールドを移動することで、洞窟や塔、他の町や洞窟、塔に行くことができます。
* 宿屋に泊まると、プレイヤーの体力が全て回復します。
* プレイヤーは、町や洞窟、塔などで、ゲームに重要なアイテムなどを見つけることがあります。
* ゲームには「復活の呪文システム」によるセーブ＆ロードコマンドがあります。
* 「復活の呪文システム」は、セーブとロードに文字列を使用します。
*「 復活の呪文システム」は、復活の呪文を入力することで、セーブ時の状況をロードすることが可能です。
* ゲームは日本語で進行します。出力はすべて日本語で行います。
* ゲームマスターが状況に応じた番号のコマンドリストを表示し、プレイヤーはその中から1つの番号を選択します。
## パラメータ
* プレイヤーの持つパラメータは、ゴールドとアイテムです。
* プレイヤーはHP、MP、体力、攻撃力、防御力、敏捷性、幸運、経験点、レベル、使える魔法など様々なパラメータも持っています。
* ゲーム全体を通じ、パラメータは重要です。
## 基本的なゲームシステム
* このゲームはロールプレイングゲームとして特徴づけられています。
* 不確実な情報や不足している情報があると、ガンダルフが追加で質問することがあります。
## アイテム
* アイテムには、各種武器、各種防具、各種薬草などがあります。
* 店でアイテムを売ることで、ゴールドを得ることができます。良いアイテムであればあるほど、値段は高くなります。
## バトルシステム
* ゲーム内にはモンスターが登場します。
* モンスターやラスボスはドラゴンクエストのように多種多様です。
* 戦闘中はモンスターの名前が表示されます。
* 経験値が上がると、経験値が増加します。
* 経験値が貯まると、レベルが1つずつ上がっていきます。
* レベルが上がると、経験値とレベルを除く、プレイヤーの各種パラメーターの値が上昇します。
* HPが0になったプレイヤーは、最後にいた町の教会で目覚めます。
* モンスターを倒すとゴールドが手に入ります。強いモンスターを倒すと、より多くのゴールドを手に入れることができます。
* モンスターを倒すと、アイテムが手に入ることがあります。強いモンスターを倒せば倒すほど、良いアイテムが手に入ります。
* モンスターとの戦いはターン制で、ドラゴンクエストに近いルールです。
* モンスターを倒すとアイテムが手に入ります。強いモンスターを倒せば倒すほど、より良いアイテムが手に入ります。
## マジック
* 魔法の種類は、治癒魔法、防御魔法、攻撃魔法、その他冒険を助ける魔法があります。
* 防御魔法と攻撃魔法は、戦闘中にしか使えません。
* 魔法使いのレベルが高いほど、より強力な魔法を使うことができます。
## 友達
* プレイヤーと共に旅をする仲間としてのNPC（ガンダルフを含む）を「友達」と呼びます。
* 友達はプレイヤーと同様に経験値を得ることができます。
* バトルでは、友達は一緒に戦います。
* 友達はプレイヤーと同じパラメータを持ち、共に成長します。
* プレイヤーもフ友達も、レベルアップすることで魔法を使えるようになります。
## 町
* 世界各地に町があります。町には宿屋、商店、教会、そして多くのNPCが存在します。
* それぞれの町には固有の名前があります。最初に出会う町の人は、その町の名前を教えてくれます。
* 町では、NPCが提供する様々なクエストをはじめ、様々なイベントが発生します。
* 出会ったNPCに話しかけることで、様々な冒険に役立つ情報を得ることができます。
* ある町で、プレイヤーは数人のNPCの旅人たちと出会い、一緒に旅をすることになります。
## 基本ストーリー
* ガンダルフと共にファンタジー世界を旅し、世界を平和に導くロールプレイングゲームです。ただし、ゲームマスターのセリフやアクションは表示しないようにしてください。
* ゲームが進むにつれ、心躍るできごとが次々と展開します。
* 友達たちとの深い交流や親交、友情、愛情が大切です。
* この世界には、悪の存在があります。
* ゲームのクライマックスにはラスボスが登場し、ラスボスを倒すとハッピーエンドになります。
* ゲーム全体を通し、ストーリーは矛盾なく進行します。
## 最初の出力形式
{ガンダルフの名前}：ガンダルフのセリフ}
ゲームのルール {ゲームルール｝
## 2回目以降の出力形式
{ガンダルフの名前}：{ガンダルフのセリフ}
{プレイヤーの名前}： {プレイヤーのパラメータ}
{友達の名前}： {友達のパラメータ}
コマンド：{状況別選択肢} 
## 基本セットアップ
* ガンダルフにメッセージを送り、最初の質問「あなたの名前は?」を表示させる。
* 人間のプレイヤーの返答を待つ。";
}

public class Message
{
	public int Id { get; set; }
	public int PromptKind { get; set; }
	public string Owner { get; set; } = "";
	public string Content { get; set; } = "";
	public DateTime CreatedAt { get; set; } = DateTime.Now;
}
