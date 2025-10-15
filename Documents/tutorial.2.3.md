## 『AIエージェント開発ハンズオンセミナー』(開発者向け) チュートリアル

### ■ MCP サーバーの動作確認

#### Visual Studio の場合

1. メニュー -「ツール」-「オプション」でオプション ダイアログを表示
2. 「GitHub」-「Copilot」

![Visual Studio MCP の設定](./Images/vs_mcp_1.png)

3. ソリューション フォルダーに \"\.mcp\.json\" ファイルを作成

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
      "url": "http://localhost:3001/sse"
    },
    "github": {
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

4. 「GitHub Copilot」-「チャット ウィンドウを開く」
![GitHub Copilot | Visual Studio](./Images/vs_github_copilot_1.png)

![GitHub Copilot | Visual Studio](./Images/vs_github_copilot_2.png)


#### Visual Studio Code の場合

1. メニュー -「表示」-「コマンド パレット」

2. 「MCP サーバーの追加」コマンドを実行
![Visual Studio Code MCP の設定](./Images/vscode_mcp_1.png)

3. 「コマンド (stdio) MCP プロトコルを実装するローカル コマンドを実行する」を選択
![Visual Studio Code MCP の設定](./Images/vscode_mcp_2.png)

4. 「コマンドの入力」では、「実行するコマンド」に \"dotnet  run --project [MCPServer\.Con\.csprojのフルパス]\" と入力して Enter
- [MCPServer\.Con\.csprojのフルパス] の部分は、実際のもので置き換えてください<br>
  (例. C:\\\\Source\\\\Shos.AIAgentSample\\\\MCPServer.Con\\\\MCPServer.Con.csproj)

![Visual Studio Code MCP の設定](./Images/vscode_mcp_3.png)

5. 「サーバー ID の入力」で \"McpServer.Con\" と入力して Enter
![Visual Studio Code MCP の設定](./Images/vscode_mcp_4.png)

6. 「MCP サーバーをインストールする場所を選択する」では「ワークスペース このワークスペースで利用可能で、ローカルで実行されます」を選択
![Visual Studio Code MCP の設定](./Images/vscode_mcp_5.png)

7. 再度「MCP サーバーの追加」コマンドを実行
![Visual Studio Code MCP の設定](./Images/vscode_mcp_1.png)

8. 「HTTP (HTTP またはサーバー送信イベント) MCP プロトコルを実装するリモート HTTP サーバーに接続する」を選択
![Visual Studio Code MCP の設定](./Images/vscode_mcp_6.png)

9. 「サーバー URL の入力」では \"http://localhost:3001/sse\" と入力して Enter
![Visual Studio Code MCP の設定](./Images/vscode_mcp_7.png)

10. 「サーバー ID の入力」で \"McpServer.Sse\" と入力して Enter
![Visual Studio Code MCP の設定](./Images/vscode_mcp_8.png)

11. 「MCP サーバーをインストールする場所を選択する」では「ワークスペース このワークスペースで利用可能で、ローカルで実行されます」を選択
![Visual Studio Code MCP の設定](./Images/vscode_mcp_5.png)

12. 次のファイルが作成される

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
      "url": "http://localhost:3001/sse",
      "type": "http"
    }
  },
  "inputs": []
}
```
※ [MCPServer\.Con\.csprojのフルパス] の部分は、実際のもので置き換わります

13. 「チャットを開く」
![GitHub Copilot | Visual Studio Code](./Images/vscode_github_copilot_1.png)

![GitHub Copilot | Visual Studio Code](./Images/vscode_github_copilot_2.png)

