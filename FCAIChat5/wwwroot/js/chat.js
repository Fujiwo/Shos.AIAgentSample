// チャット画面のスクリプト
// - ES モジュールとして読み込む (<script type="module">)。モジュールは strict モードで、HTML の解析後に実行される
// - ES2022 のトップレベル await で SignalR の接続を待つ

const sendButton   = document.getElementById("sendButton");
const userInput    = document.getElementById("userInput");
const messageInput = document.getElementById("messageInput");
// 新: クリア ボタン
const clearButton  = document.getElementById("clearButton");

const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

// 接続が確立するまで送信ボタンを無効にする
sendButton.disabled = true;
// 新: クリア ボタンも無効にする
clearButton.disabled = true;

connection.on("ReceiveMessage", (user, message, createdAt) => {
    console.log("Receiving message:", user, message);
    // サーバーで部分ビューをレンダリングして取得
    addMessagePartial(user, message, createdAt);
});

// 新: 履歴がクリアされたら再表示
connection.on("Cleared", () => location.reload());

sendButton.addEventListener("click", async event => {
    event.preventDefault();
    const user    = userInput.value;
    const message = messageInput.value;
    console.log("Sending message:", user, message);
    if (user.trim() === "" || message.trim() === "")
        return;
    try {
        // 旧:
        //await connection.invoke("SendMessage", user, message);
        // 新: 送信者名はサーバー側で認証情報から取得するので、送らない
        await connection.invoke("SendMessage", message);
    } catch (error) {
        console.error(error.toString());
    }
});

// 新: ここから クリア ボタンで会話とメッセージ履歴をクリア
clearButton.addEventListener("click", async event => {
    event.preventDefault();
    try {
        // サーバー側でクリアが完了すると "Cleared" が全クライアントに通知され、再表示される
        await connection.invoke("Clear");
    } catch (error) {
        console.error(error.toString());
    }
});
// 新: ここまで

// サーバーで描画された UTC 時刻をローカル時刻で表示
toLocalTimeStringAll();

// 接続を開始 (ES2022 のトップレベル await)
try {
    await connection.start();
    sendButton.disabled = false;
    // 新: クリア ボタンも有効にする
    clearButton.disabled = false;
} catch (error) {
    console.error(error.toString());
}

async function addMessagePartial(user, message, createdAt) {
    try {
        const query = new URLSearchParams({
            "Message.UserName" : user,
            "Message.Content"  : message,
            "Message.CreatedAt": createdAt
        });
        const response = await fetch(`/Messages/RenderPartial?${query}`, { headers: { "X-Requested-With": "XMLHttpRequest" } });
        if (!response.ok)
            throw new Error(`Failed to fetch partial: ${response.status} ${response.statusText}`);

        const container = document.querySelector(".talk1") ?? document.getElementById("messagesList");
        if (!container)
            return;

        // 返ってくるのはルート要素が複数の可能性があるので、template 経由ですべての子ノードを追加
        const template = document.createElement("template");
        template.innerHTML = await response.text();
        container.append(template.content);

        // 取得した断片に含まれる UTC 時刻をローカル表示へ
        toLocalTimeStringAll();
    } catch (error) {
        console.error("Error fetching partial:", error);
        // フォールバック: クライアント側で組み立てる
        addMessageRow(user, message, createdAt);
    }
}

// メッセージの行をクライアント側で組み立てて追加
function addMessageRow(user, message, createdAt) {
    const row = document.createElement("div");
    row.textContent = `${user} says ${message} - `;

    const time = document.createElement("span");
    time.className   = "utc-time";
    time.textContent = toLocalTimeString(createdAt);
    row.append(time);

    document.getElementById("messagesList").append(row);
}

// まだ変換していない (hidden クラスが付いている) UTC 時刻だけをローカル時刻に変換して表示
function toLocalTimeStringAll() {
    for (const element of document.querySelectorAll(".utc-time.hidden")) {
        element.textContent = toLocalTimeString(element.textContent);
        element.classList.remove("hidden");
    }
}

function toLocalTimeString(utcDateString) {
    return new Date(utcDateString).toLocaleString();
}
