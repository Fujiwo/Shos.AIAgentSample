// チャット画面のスクリプト
// - ES モジュールとして読み込む (<script type="module">)。モジュールは strict モードで、HTML の解析後に実行される
// - ES2022 のトップレベル await で SignalR の接続を待つ

// 画面の要素
const sendButton   = document.getElementById("sendButton");
const userInput    = document.getElementById("userInput");
const messageInput = document.getElementById("messageInput");

// SignalR の Hub (/chatHub) への接続を作成
const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

// 接続が確立するまで送信ボタンを無効にする
sendButton.disabled = true;

// サーバー (ChatHub) から "ReceiveMessage" が呼ばれたら、受け取ったメッセージを画面に追加
connection.on("ReceiveMessage", (user, message, createdAt) => {
    console.log("Receiving message:", user, message);
    addMessageRow(user, message, createdAt);
});

// 送信ボタンがクリックされたら、Hub の SendMessage を呼び出してメッセージを送信
sendButton.addEventListener("click", async event => {
    event.preventDefault();
    const user    = userInput.value;
    const message = messageInput.value;
    console.log("Sending message:", user, message);
    // 送信者名かメッセージが空なら送信しない
    if (user.trim() === "" || message.trim() === "")
        return;
    try {
        await connection.invoke("SendMessage", user, message);
    } catch (error) {
        console.error(error.toString());
    }
});

// 接続を開始 (ES2022 のトップレベル await)
try {
    await connection.start();
    sendButton.disabled = false;
} catch (error) {
    console.error(error.toString());
}

// メッセージの行をクライアント側で組み立てて追加
function addMessageRow(user, message, createdAt) {
    const row = document.createElement("li");
    row.textContent = `${user} says ${message} - `;

    const time = document.createElement("span");
    time.className   = "utc-time";
    time.textContent = toLocalTimeString(createdAt);
    row.append(time);

    document.getElementById("messagesList").append(row);
}

// UTC の日時の文字列を、ブラウザーのローカル時刻の文字列に変換
function toLocalTimeString(utcDateString) {
    return new Date(utcDateString).toLocaleString();
}
