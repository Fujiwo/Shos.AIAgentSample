## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ 準備

#### 1\. \.NET

- 1\.1 \.NET のバージョンの確認

```console
dotnet --version
```
  9\.0以上であればOK

- 1\.2 (9\.0未満の場合) \.NET9\.0 のダウンロード

![Download \.NET](./Images/download_dotnet.png)

- 1\.3 (9\.0未満の場合) \.NET9\.0 のインストール

![Install \.NET](./Images/install_dotnet.png)

- 1\.4 \.NET のバージョンの再確認

```console
dotnet --version
```

#### 2\. Node.js の Windows 版をインストール

- 2\.1 Node.js のダウンロード

  - [Node\.js — Node\.js®をダウンロードする](https://nodejs.org/ja/download)

![Download Node.js](./images/download_nodejs.png)

- 2\.2 Node.js のインストール

![Node\.js のインストール](./Images/nodejs_installer.png)

- 2\.2 確認

```console
node -v
```

```console
npm -v
```

- 2\.3 npx のインストール

```console
npm install -g npx
```

- 2\.4 確認

```console
npx -v
```

#### 3\. Ollama の Windows 版をインストール

- 3\.1 Ollama の Windows 版をダウンロード

  - [Ollama](https://www.ollama.com)

![Download Ollama](./Images/download_ollama.png)

- 3\.2 Ollama をインストール
![Install Ollama](./Images/install_ollama.png)

- 3\.3 起動して、サインアップ

![Sign up Ollama](./Images/signup_ollama.png)

- 3\.4 動作確認

![Ollama](./Images/ollama.png)


### ■ Azure OpenAI の準備

#### 1\. Visual Studio Subscription の Azure 特典のアクティブ化

- [Visual Studio Subscriptions Portal](https://my.visualstudio.com)

![Visual Studio Subscription 特典](./Images/vs_azure.png)

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
※ 「202510azureopenai」の部分は、別のユニークな文字列とする
※ この文字列はメモ

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
