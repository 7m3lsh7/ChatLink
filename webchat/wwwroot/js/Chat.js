const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.start()
    .then(() => console.log("Connected to SignalR"))
    .catch(err => console.error("SignalR connection error:", err));

function checkReceiverSelected() {
    const receiverId = parseInt(document.getElementById("receiverId").value);
    const sendButton = document.getElementById("sendButton");
    const messageInput = document.getElementById("messageInput");

    if (receiverId) {
        sendButton.disabled = false;
        messageInput.disabled = false;
    } else {
        sendButton.disabled = true;
        messageInput.disabled = true;
    }
}

document.getElementById("sendButton").addEventListener("click", () => {
    const receiverId = parseInt(document.getElementById("receiverId").value);
    const message = document.getElementById("messageInput").value.trim();

    if (message !== "") {
        const userId = parseInt(document.getElementById("userId").value);
        connection.invoke("SendMessage", userId, receiverId, message)
            .then(() => {
                const msgDiv = document.createElement("div");
                msgDiv.className = "message sent";
                msgDiv.innerHTML = `<div class="bubble">${message}</div>`;
                document.getElementById("chatContainer").appendChild(msgDiv);
                document.getElementById("messageInput").value = "";
            })
            .catch(err => console.error(err));
    }
});

connection.on("ReceiveMessage", (senderId, message, senderName) => {
    const unseenMessagesContainer = document.getElementById("unseenMessagesContainer");

    const msgDiv = document.createElement("div");
    msgDiv.className = "message-item unseen";
    msgDiv.innerHTML = `
        <div>
            <img src="" alt="User">
            <div class="message-content">
                <strong>${senderName}</strong>
                <p>${message}</p>
            </div>
        </div>
    `;

    msgDiv.addEventListener("click", function () {
        markMessageAsRead(senderId);
        msgDiv.remove();
        openChatWithSender(senderId);
    });

    unseenMessagesContainer.appendChild(msgDiv);
    document.getElementById("notification").style.display = "block";
});

function markMessageAsRead(messageId) {
    fetch(`/Home/MarkAsRead/${messageId}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
    })
        .then(response => {
            if (response.ok) {
                console.log("Message marked as read.");
            }
        })
        .catch(err => console.error(err));
}

function openChatWithSender(senderId) {
    window.location.href = `/Home/Chat/?receiverId=${senderId}`;
}

window.onload = function () {
    checkReceiverSelected();
};
