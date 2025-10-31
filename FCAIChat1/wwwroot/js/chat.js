"use strict";

document.addEventListener('DOMContentLoaded', run);

function run() {
    var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

    //Disable the send button until connection is established.
    document.getElementById("sendButton").disabled = true;

    connection.on("ReceiveMessage", function (user, message, createdAt) {
        console.log("Receiving message:", user, message);
        addMessageRow(user, message, createdAt);
    });

    connection.start().then(function () {
        document.getElementById("sendButton").disabled = false;
    }).catch(function (err) {
        return console.error(err.toString());
    });

    document.getElementById("sendButton").addEventListener("click", function (event) {
        var user = document.getElementById("userInput").value;
        var message = document.getElementById("messageInput").value;
        console.log("Sending message:", user, message);
        if (user.trim() !== "" && message.trim() !== "")
            connection.invoke("SendMessage", user, message)
                .catch(err => console.error(err.toString()));
        event.preventDefault();
    });
}

function addMessageRow(user, message, createdAt) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    setToLI(li, user, message, createdAt);
}

function setToLI(li, user, message, createdAt) {
    li.textContent = `${user} says ${message} - `;

    var span = document.createElement("span");
    span.innerHTML = `${toLocalTimeString(createdAt)}`;
    span.className = "utc-time";
    li.appendChild(span);
}

function toLocalTimeString(utcDateString) {
    const date = new Date(utcDateString);
    return date.toLocaleString();
}