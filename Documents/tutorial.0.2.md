## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ 準備 - Azure OpenAI
![準備 - Azure OpenAI](./Images/tutorial_banner_02.png)

この手順では、チュートリアルのための準備として、Azure OpenAI による LLM を作成します。

#### 1\. Microsoft Azure アカウントの準備

##### 1\.1 Visual Studio Subscription の Azure 特典のアクティブ化

※ Visual Studio Subscription で Professional 以上を契約している方限定

Visual Studio Subscriptions Portal へ行く
- [Visual Studio Subscriptions Portal](https://my.visualstudio.com)

特典の「Azure」がアクティブ化されてなければ、アクティブ化
![Visual Studio Subscription 特典](./Images/vs_azure.png)

##### 1\.2 Microsoft Azure アカウントを作成

Microsoft Azure アカウントがまだない場合、下記を参考に作成

- [Azure アカウントの作成 \- \.NET \| Microsoft Learn](https://learn.microsoft.com/ja-jp/dotnet/azure/create-azure-account)

※ 注. 課金されることがある


#### 2\. Azure OpenAI で LLM を作成

- 2\.1 [Microsoft Azure Portal](https://portal.azure.com) へ行き、「リソースの作成」

![リソースの作成 | Azure Portal](./Images/azure_portal.png)

- 2\.2 「Azure OpenAI」で検索し、「Azure OpenAI」をクリック

![Azure OpenAI | リソースの作成](./Images/azure_create_resource.png)

- 2\.3 「Azure OpenAI」を作成

![Azure OpenAI の作成](./Images/azure_openai1.png)

リソース グループは新規作成する

![リソース グループの新規作成 | Azure OpenAI の作成](./Images/azure_openai2.png)

下記のように入力し、「次へ」を3回クリック
※ 「名前」の部分は、ユニークな文字列とする

![Azure OpenAI の作成](./Images/azure_openai3.png)

「作成」

![Azure OpenAI の作成](./Images/azure_openai4.png)

デプロイが終わったら、「リソースへ移動」

![Azure OpenAI の作成](./Images/azure_openai5.png)

「Azure AI Foundry ポータルへ移動」

![Azure AI Foundry ポータルへ移動](./Images/azure_openai6.png)

「APIキー」と「Azure OpenAI エンドポイント」をコピーしてメモ

![Azure AI Foundry ポータルへ移動](./Images/azure_openai7.png)

「チャット」を選び、「デプロイの作成」

![チャット](./Images/azure_openai8.png)

「gpt-5-mini」を選び、「確認」

![デプロイの作成](./Images/azure_openai9.png)

「デプロイ」

![デプロイ](./Images/azure_openai10.png)
