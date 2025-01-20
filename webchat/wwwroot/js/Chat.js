
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js"></script>

     // إعداد اتصال SignalR
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    connection.start()
        .then(() => console.log("Connected to SignalR"))
        .catch(err => console.error("SignalR connection error:", err));

    // إرسال رسالة عند النقر على زر الإرسال
    document.getElementById("sendButton").addEventListener("click", () => {
        const receiverId = parseInt(document.getElementById("receiverId").value);
        const message = document.getElementById("messageInput").value.trim();

        if (message !== "") {
            const userId = parseInt(document.getElementById("userId").value);
            connection.invoke("SendMessage", userId, receiverId, message)
                .then(() => {
                    // إضافة الرسالة إلى واجهة المستخدم
                    const msgDiv = document.createElement("div");
                    msgDiv.className = "message sent";
                    msgDiv.innerHTML = `<div class="bubble">${message}</div>`;
                    document.getElementById("chatContainer").appendChild(msgDiv);

                    // تفريغ حقل الإدخال
                    document.getElementById("messageInput").value = "";
                })
                .catch(err => console.error(err));
        }
    });

    // استقبال رسالة جديدة عبر SignalR
    connection.on("ReceiveMessage", (senderId, message, senderName) => {
        // إذا كان المستخدم غير متصل بالمرسل، أضف الرسالة إلى قائمة الرسائل غير المقروءة
        const unseenMessagesContainer = document.getElementById("unseenMessagesContainer");

        const msgDiv = document.createElement("div");
        msgDiv.className = "unseen-message p-2 mb-2 bg-light rounded";
        msgDiv.innerHTML = `
            <p><strong>From:</strong> ${senderName}</p>
            <p>${message}</p>
            <small>${new Date().toLocaleString()}</small>
        `;

        msgDiv.setAttribute("data-sender-id", senderId);
        msgDiv.setAttribute("data-message", message);

        // عند النقر على الرسالة، يتم تحديث حالتها إلى "مقروءة"
        msgDiv.addEventListener("click", function () {
            markMessageAsRead(senderId);
            msgDiv.remove();
            openChatWithSender(senderId);
        });

        unseenMessagesContainer.appendChild(msgDiv);

        // إظهار إشعار أن هناك رسالة جديدة
        document.getElementById("notification").style.display = "block";
    });

    // تحميل الرسائل غير المقروءة عند تحميل الصفحة
    document.addEventListener("DOMContentLoaded", function () {
        const unseenMessages = JSON.parse(document.getElementById("unseenMessagesData").value);
        const unseenMessagesContainer = document.getElementById("unseenMessagesContainer");

        // عرض جميع الرسائل غير المقروءة
        if (unseenMessages && unseenMessages.length > 0) {
            unseenMessages.forEach((message) => {
                const messageElement = document.createElement("div");
                messageElement.classList.add("unseen-message", "p-2", "mb-2", "bg-light", "rounded");
                messageElement.innerHTML = `
                    <p><strong>From:</strong> ${message.senderName}</p>
                    <p>${message.content}</p>
                    <small>${new Date(message.timestamp).toLocaleString()}</small>
                `;

                // عند النقر على الرسالة، يتم تحديث حالتها إلى "مقروءة" وفتح الشات مع المرسل
                messageElement.addEventListener("click", function () {
                    markMessageAsRead(message.id);
                    messageElement.remove();
                    openChatWithSender(message.senderId);
                });

                unseenMessagesContainer.appendChild(messageElement);
            });
        } else {
            unseenMessagesContainer.innerHTML = "<p>No unseen messages</p>";
        }
    });

    // وظيفة لتحديث حالة الرسالة إلى "مقروءة" باستخدام طلب AJAX
    function markMessageAsRead(messageId) {
        fetch(`/Home/MarkAsRead/${messageId}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-CSRF-TOKEN": document.querySelector("input[name='__RequestVerificationToken']").value,
            },
        })
            .then((response) => {
                if (response.ok) {
                    console.log("Message marked as read.");
                } else {
                    console.error("Failed to mark message as read.");
                }
            })
            .catch((error) => console.error("Error:", error));
    }

    function openChatWithSender(senderId) {
        // فتح الشات مع المرسل في جزء الشات
        window.location.href = `/Home/Chat/${senderId}`;
    }

    // تنفيذ التحقق عند تحميل الصفحة
    window.onload = function () {
        checkReceiverSelected();
    };

    // التحقق من اختيار المستخدم المستلم
    function checkReceiverSelected() {
        const receiverId = document.getElementById("receiverId").value;
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
 

