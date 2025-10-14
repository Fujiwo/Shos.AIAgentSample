🧑‍💻User:
>MCP サーバーを用いる場合、LLM には、どのようにして、ツールの一覧を渡すのですか?
とても具体的な、LLM への入力の例を教えてください。

🤖Agent:
>MCPサーバーを用いる場合、LLMにはツールの一覧がJSON Schema形式で渡されます。以下に具体的な例を示します：

```json
{
  "messages": [
    {
      "role": "user",
      "content": "ファイルシステムの内容を確認してください"
    }
  ],
  "tools": [
    {
      "name": "filesystem:read_file",
      "description": "Read the complete contents of a file from the file system. Handles various text encodings and provides detailed error messages if the file cannot be read.",
      "input_schema": {
        "type": "object",
        "properties": {
          "path": {
            "type": "string",
            "description": "Path to the file to read"
          }
        },
        "required": ["path"]
      }
    },
    {
      "name": "filesystem:list_directory",
      "description": "Get a detailed listing of all files and directories in a specified path.",
      "input_schema": {
        "type": "object",
        "properties": {
          "path": {
            "type": "string",
            "description": "Path to the directory to list"
          }
        },
        "required": ["path"]
      }
    },
    {
      "name": "web_search",
      "description": "Search the web",
      "input_schema": {
        "type": "object",
        "properties": {
          "query": {
            "type": "string",
            "description": "Search query"
          }
        },
        "required": ["query"]
      }
    }
  ]
}
```

>MCPサーバーは以下のような流れで動作します：
>
>1. ツール発見フェーズ: MCPクライアントがサーバーに tools/list リクエストを送信
>2. スキーマ取得: 各ツールのJSON Schemaを取得
>3. LLMへの提供: 上記のような形式でLLMにツール情報を渡す
>4. ツール実行: LLMがツールを呼び出す際は、MCPサーバーが実際の処理を実行
>
>実際のLLMとの対話では、私はこれらのツールを以下のような形式で呼び出します：


リクエスト
>Read file

```json
{
  `path`: `/home/user/document.txt`
}
```

レスポンス
```'
Error: Access denied - path outside allowed directories: C:\home\user\document.txt not in C:\work
```

🧑‍💻User:
>>MCPサーバーを用いる場合、LLMにはツールの一覧がJSON Schema形式で渡されます。
>
>どのようなプロンプトとともに渡すのでしょうか。とても具体的に教えてください。
>ただし、どのような JSON を渡すかは、具体的にわかりましたので、必要ありません。

🤖Agent:
>MCPサーバーを用いる場合、LLMへのプロンプトは通常以下のような構造で渡されます：
```
----- LLM に入力したPrompt ここから -----
You are Claude, an AI assistant created by Anthropic.

In this environment you have access to a set of tools you can use to answer the user's question.
You can invoke functions by writing a "$FUNCTION_NAME" block like the following as part of your reply to the user:
String and scalar parameters should be specified as is, while lists and objects should use JSON format.
Here are the functions available in JSONSchema format:
<functions>
[ここに先ほどのJSONSchemaが挿入される]
</functions>
The current date is Monday, July 07, 2025.
[その他のシステム指示やコンテキスト情報]
```

🧑‍💻User:
>それを渡すと、MCP サーバー内のツールの呼び出しについて、LLMからはどのような指示が返ってくるのでしょうか。とても具体的に教えてください。

🤖Agent:
>LLMからは、以下のような具体的な指示が返ってきます：
>
>----- LLM から出力されたテキスト ここから -----
```
ファイルシステムの内容を確認いたします。まず、利用可能なディレクトリを確認してから、その内容を調べてみましょう。
```
```
List allowed directories
```
>リクエスト
```
{}
```
>レスポンス
```
Allowed directories:
C:\work
```

>C:\work利用可能なディレクトリが確認できました。次に、このディレクトリの内容を確認します。
```
List directory
```
>リクエスト
```
{
  `path`: `C:\\work`
}
```
>レスポンス
```
[FILE] text1.txt
[FILE] text2.txt
```
>ディレクトリ内に2つのファイルが見つかりました。text1.txtとtext2.txtです。これらのファイルの内容を確認しますか？
>----- LLM から出力されたテキスト ここまで -----

>重要なポイント：
>
>1. **XML形式のツール呼び出し**: 
>関数名
>
>リクエスト
```json
{
  "パラメータ名": "値"
}
```
>レスポンス
Tool "関数名" not found.` の形式
```

>2. 自然言語での説明: ツール呼び出しの前後に、何をしようとしているかの説明
>
>3.結果の解釈: `関数名` 内の結果を受けて、次のアクションや解釈を提示

>リクエスト
```json
{
  "パラメータ名": "値"
}
```

>レスポンス
```
Tool "関数名" not found.
```

>4. パラメータの適切な指定: JSON Schemaで定義された通りのパラメータ名と型で値を指定
>
>この出力を受けて、MCPクライアントが実際にMCPサーバーのツールを実行し、結果をLLMに返すという流れになります。