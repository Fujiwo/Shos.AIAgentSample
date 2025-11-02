"use strict";

document.addEventListener('DOMContentLoaded', run);

function run() {
    var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

    //Disable the send and clear button until connection is established.
    document.getElementById("sendButton" ).disabled = true;
    document.getElementById("clearButton").disabled = true;

    connection.on("ReceiveMessage", function (user, message, createdAt) {
        console.log("Receiving message:", user, message);
        // サーバーで部分ビューをレンダリングして取得
        addMessagePartial(user, message, createdAt);
    });

    connection.start().then(function () {
        document.getElementById("sendButton" ).disabled = false;
        document.getElementById("clearButton").disabled = false;
    }).catch(function (err) {
        return console.error(err.toString());
    });

    document.getElementById("sendButton").addEventListener("click", function (event) {
        var user    = document.getElementById("userInput"   ).value;
        var message = document.getElementById("messageInput").value;
        console.log("Sending message:", user, message);
        if (user.trim() !== "" && message.trim() !== "")
            connection.invoke("SendMessage", user, message)
                      .catch(err => console.error(err.toString()));
        event.preventDefault();
    });

    document.getElementById("clearButton").addEventListener("click", function (event) {
        connection.invoke("Clear")
                  .catch(err => console.error(err.toString()));
        location.reload();
        event.preventDefault();
    });

    toLocalTimeStringAll();
}

async function addMessagePartial(user, message, createdAt) {
    try {
        const url = `/Messages/RenderPartial?Message.UserName=${encodeURIComponent(user)}&Message.Content=${encodeURIComponent(message)}&Message.CreatedAt=${encodeURIComponent(createdAt)}`;
        const res = await fetch(url, { method: 'GET', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (!res.ok) {
            console.error('Failed to fetch partial:', res.status, res.statusText);
            // フォールバック: クライアント側組み立て
            return addMessageRow(user, message, createdAt);
        }
        const html = await res.text();
        const container = document.querySelector('.talk1') || document.getElementById('messagesList');
        if (!container) return;

        const temp = document.createElement('div');
        temp.innerHTML = html;
        // 返ってくるのはルート要素が複数の可能性があるので、子ノードを順に追加
        while (temp.firstChild) {
            container.appendChild(temp.firstChild);
        }

        // 取得した断片に含まれる UTC 時刻をローカル表示へ
        toLocalTimeStringAll();
    } catch (e) {
        console.error('Error fetching partial:', e);
        // フォールバック: クライアント側組み立て
        addMessageRow(user, message, createdAt);
    }
}

function addMessageRow(user, message, createdAt) {
    var div = document.createElement("div");
    document.getElementById("messagesList").appendChild(div);
    setToDiv(div, user, message, createdAt);
}

function setToDiv(div, user, message, createdAt) {
    div.textContent = `${user} says ${message} - `;

    var span = document.createElement("span");
    span.innerHTML = `${toLocalTimeString(createdAt)}`;
    span.className = "utc-time";
    div.appendChild(span);
}

function toLocalTimeStringAll() {
    document.querySelectorAll('.utc-time').forEach(element => {
        element.classList.remove('hidden');
        element.textContent = toLocalTimeString(element.textContent)
    });
}

function toLocalTimeString(utcDateString) {
    const date = new Date(utcDateString);
    return date.toLocaleString();
}
