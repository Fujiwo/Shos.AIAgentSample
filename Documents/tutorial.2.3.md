## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ 【参考】MCP サーバーの動作確認

#### Visual Studio Code の場合

1. Visual Studio Code への GitHub Copilot の設定

- Visual Studio Code への GitHub Copilot の設定方法: [Set up GitHub Copilot in VS Code](https://code.visualstudio.com/docs/copilot/setup)

2. Agent モードの有効化

Visual Studio Code を起動し、設定を開く

\"chat.agent.enabled\" で検索し、チェックが入っていなければチェック
![alt text](./Images/vscode_agentmode.png)

3. メニュー -「表示」-「コマンド パレット」

1. 「MCP サーバーの追加」コマンドを実行
![Visual Studio Code MCP の設定](./Images/vscode_mcp_1.png)

1. 「コマンド (stdio) MCP プロトコルを実装するローカル コマンドを実行する」を選択
![Visual Studio Code MCP の設定](./Images/vscode_mcp_2.png)

1. 「コマンドの入力」では、「実行するコマンド」に \"dotnet  run --project [MCPServer\.Con\.csprojのフルパス]\" と入力して Enter
- [MCPServer\.Con\.csprojのフルパス] の部分は、実際のもので置き換えてください<br>
  (例. C:\\\\Source\\\\Shos.AIAgentSample\\\\MCPServer.Con\\\\MCPServer.Con.csproj)

![Visual Studio Code MCP の設定](./Images/vscode_mcp_3.png)

7. 「サーバー ID の入力」で \"McpServer.Con\" と入力して Enter
![Visual Studio Code MCP の設定](./Images/vscode_mcp_4.png)

1. 「MCP サーバーをインストールする場所を選択する」では「ワークスペース このワークスペースで利用可能で、ローカルで実行されます」を選択
![Visual Studio Code MCP の設定](./Images/vscode_mcp_5.png)

1. 再度「MCP サーバーの追加」コマンドを実行
![Visual Studio Code MCP の設定](./Images/vscode_mcp_1.png)

1. 「HTTP (HTTP またはサーバー送信イベント) MCP プロトコルを実装するリモート HTTP サーバーに接続する」を選択
![Visual Studio Code MCP の設定](./Images/vscode_mcp_6.png)

1. 「サーバー URL の入力」では \"http://localhost:3001/sse\" と入力して Enter
![Visual Studio Code MCP の設定](./Images/vscode_mcp_7.png)

1. 「サーバー ID の入力」で \"McpServer.Sse\" と入力して Enter
![Visual Studio Code MCP の設定](./Images/vscode_mcp_8.png)

1. 「MCP サーバーをインストールする場所を選択する」では「ワークスペース このワークスペースで利用可能で、ローカルで実行されます」を選択
![Visual Studio Code MCP の設定](./Images/vscode_mcp_5.png)

1. 次のファイルが作成される

[ソリューション フォルダー]\\\.vscode\\mcp\.json
```json
{
  "servers": {
    "McpServer.Con": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "[MCPServer.Con.csprojのフルパス]"
      ]
    },
    "McpServer.Sse": {
      "type": "http",
      "url": "http://localhost:3001/sse"
    }
  },
  "inputs": []
}
```
※ [MCPServer\.Con\.csprojのフルパス] の部分は、実際のもので置き換わります

15. ツールの確認

- 予め McpServer\.Sse を実行しておく
  - Visual Studio のメニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」
```console
cd McpServer.Sse
dotnet run
```

![GitHub Copilot | Visual Studio Code](./Images/vscode_github_copilot_1.png)

mcp\.json ファイルを開き、正常に起動していないものがあれば、起動

[ソリューション フォルダー]\\\.vscode\\mcp\.json
![Visual Studio Code mcp.json](./Images/vscode_mcp_json.png)

16. 「チャットを開く」

![GitHub Copilot | Visual Studio Code](./Images/vscode_github_copilot_2.png)

![GitHub Copilot | Visual Studio Code](./Images/vscode_github_copilot_3.png)

17.  実行結果

![GitHub Copilot | Visual Studio Code](./Images/vscode_github_copilot_4.png)

![GitHub Copilot | Visual Studio Code](./Images/vscode_github_copilot_5.png)

![GitHub Copilot | Visual Studio Code](./Images/vscode_github_copilot_6.png)

![GitHub Copilot | Visual Studio Code](./Images/vscode_github_copilot_7.png)


#### [参考] Visual Studio の場合

1. GitHub アカウントと GitHub へのサインインが必要

![alt text](./Images/vs_account.png)

2. メニュー -「ツール」-「オプション」でオプション ダイアログを表示

3. 「GitHub」-「Copilot」

![Visual Studio MCP の設定](./Images/vs_mcp_1.png)

4. ソリューション フォルダーに \"\.mcp\.json\" ファイルを作成

[ソリューション フォルダー]\\\.mcp\.json
```json
{
  "servers": {
    "McpServer.Con": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "[MCPServer.Con.csprojのフルパス]"
      ]
    },
    "McpServer.Sse": {
      "type": "http",
      "url": "http://localhost:3001/sse"
    },
    "github": {
      "type": "http",
      "url": "https://api.githubcopilot.com/mcp/"
    },
    "microsoft.docs.mcp": {
      "type": "http",
      "url": "https://learn.microsoft.com/api/mcp"
    }
  },
  "inputs": []
}

```
- [MCPServer\.Con\.csprojのフルパス] の部分は、実際のもので置き換えてください<br>
  (例. C:\\\\Source\\\\Shos.AIAgentSample\\\\MCPServer.Con\\\\MCPServer.Con.csproj)

5. 「GitHub Copilot」-「チャット ウィンドウを開く」
![GitHub Copilot | Visual Studio](./Images/vs_github_copilot_1.png)

![GitHub Copilot | Visual Studio](./Images/vs_github_copilot_2.png)

6. ツールの確認

- 予め McpServer\.Sse を実行しておく
  - Visual Studio のメニューの「ツール」-「コマンド ライン」-「開発者コマンド プロンプト」
```console
cd McpServer.Sse
dotnet run
```

![GitHub Copilot | Visual Studio](./Images/vs_github_copilot_3.png)

\.mcp\.json ファイルを開き、正常に起動していないものがあれば、起動

[ソリューション フォルダー]\\\.mcp\.json
![.mcp.json](./Images/vs_mcp_json.png)
