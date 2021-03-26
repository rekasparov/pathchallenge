$(() => {

    "use strict";

    init();

    var connection = new signalR
        .HubConnectionBuilder()
        .withAutomaticReconnect(5000)
        .withUrl("/chatHub")
        .build();

    connection.on("LoginRequestSuccessfull", (username) => { loginRequestSuccessfull(username) });

    connection.on("GetChatRoomListRequestSuccessfull", (jsonChatRoomList) => { getChatRoomListRequestSuccessfull(jsonChatRoomList) });

    connection.on("HasAnErrorOccurred", (errorCode) => { hasAnErrorOccurred(errorCode); });

    connection.on("JoinChatRoomRequestSuccessfull", (username, chatRoomName, jsonChatRoomHistory) => { joinChatRoomRequestSuccessfull(username, chatRoomName, jsonChatRoomHistory); });

    connection.on("HasNewMessageOnChatRoom", (username, chatRoomName, messageDate, message) => { hasNewMessageOnChatRoom(username, chatRoomName, messageDate, message) });

    connection.on("HasDisconnection", (username, chatRoomName, messageDate, message) => { hasDisconnection(username, chatRoomName, messageDate, message) });

    connection.on("HasNewConnection", (username, chatRoomName, messageDate, message) => { hasNewConnection(username, chatRoomName, messageDate, message) });

    connection
        .start()
        .then(() => {
            changeConnectionStatus();
        })
        .catch((error) => {
            console.log(error)
        });

    function hasDisconnection(username, chatRoomName, messageDate, message) {
        appendNewMessageToChatRoomHistory(username, chatRoomName, messageDate, message);
    }

    function hasNewConnection(username, chatRoomName, messageDate, message) {
        appendNewMessageToChatRoomHistory(username, chatRoomName, messageDate, message);
    }

    function hasNewMessageOnChatRoom(username, chatRoomName, messageDate, message) {
        appendNewMessageToChatRoomHistory(username, chatRoomName, messageDate, message);
    }

    function appendNewMessageToChatRoomHistory(username, chatRoomName, messageDate, message) {
        var chatRoomName = $("#chat-room-" + chatRoomName);
        if (chatRoomName) {
            var textareaChatRoomMessageHistory = $("#textarea-chat-room-message-history");
            if (textareaChatRoomMessageHistory) {
                var chatRoomMessages = username;
                chatRoomMessages += " said (";
                chatRoomMessages += messageDate;
                chatRoomMessages += "): ";
                chatRoomMessages += message;
                chatRoomMessages += "\r\n";
                $(textareaChatRoomMessageHistory).val($(textareaChatRoomMessageHistory).val() + chatRoomMessages);
            }
        }
    }

    function loginRequestSuccessfull(username) {
        setCookie(username);
        getChatRoomListRequest();
        showChatRoomPlatform();
    }

    function showChatRoomPlatform() {
        var loginContainer = $("#login-container");
        var chatRoomContainer = $("#chat-room-container");
        if (loginContainer) {
            if (!$(loginContainer).hasClass("d-none")) $(loginContainer).addClass("d-none");
        }
        if (chatRoomContainer) {
            if ($(chatRoomContainer).hasClass("d-none")) $(chatRoomContainer).removeClass("d-none");
        }
    }

    function lockChatRoomArea() {
        var textareaChatRoomMessageHistory = $("#textarea-chat-room-message-history");
        var txtMessages = $("#txt-messages");
        var btnSendMessage = $("#btn-send-message");
        if (textareaChatRoomMessageHistory) {
            if (!$(textareaChatRoomMessageHistory).hasClass("disabled")) $(textareaChatRoomMessageHistory).addClass("disabled")
            $(textareaChatRoomMessageHistory).prop("disabled", "disabled");
            $(textareaChatRoomMessageHistory).val("");
        }
        if (txtMessages) {
            if (!$(txtMessages).hasClass("disabled")) $(txtMessages).addClass("disabled")
            $(txtMessages).prop("disabled", "disabled");
            $(txtMessages).val();
        }
        if (btnSendMessage) {
            if (!$(btnSendMessage).hasClass("disabled")) $(btnSendMessage).addClass("disabled")
            $(btnSendMessage).prop("disabled", "disabled");
        }
    }

    function unlockChatRoomArea() {
        var textareaChatRoomMessageHistory = $("#textarea-chat-room-message-history");
        var txtMessages = $("#txt-messages");
        var btnSendMessage = $("#btn-send-message");
        if (textareaChatRoomMessageHistory) {
            if ($(textareaChatRoomMessageHistory).hasClass("disabled")) $(textareaChatRoomMessageHistory).removeClass("disabled")
            $(textareaChatRoomMessageHistory).removeAttr("disabled");
        }
        if (txtMessages) {
            if ($(txtMessages).hasClass("disabled")) $(txtMessages).removeClass("disabled")
            $(txtMessages).removeAttr("disabled");
        }
        if (btnSendMessage) {
            if ($(btnSendMessage).hasClass("disabled")) $(btnSendMessage).removeClass("disabled")
            $(btnSendMessage).removeAttr("disabled");
        }
    }

    function getChatRoomListRequestSuccessfull(jsonChatRoomList) {
        prepareChatRoomList(jsonChatRoomList);
    }

    function hasAnErrorOccurred(errorCode) {
        var errorMessage;
        switch (errorCode) {
            case "ERROR100":
                errorMessage = "Nickname is not available";
                break;
            case "ERROR101":
                errorMessage = "User has not signed in yet";
                break;
            case "ERROR102":
                errorMessage = "Chat room not found";
                break;
            case "ERROR103":
                errorMessage = "User is not a room member";
                break;
            case "UNKNOWN":
                errorMessage = "An unknown error ocurred";
                break;
            default:
        }
        setLoginError(errorMessage);
    }

    function joinChatRoomRequestSuccessfull(username, chatRoomName, jsonChatRoomHistory) {
        var textareaChatRoomMessageHistory = $("#textarea-chat-room-message-history");
        if (textareaChatRoomMessageHistory) {
            $(textareaChatRoomMessageHistory).val("");
            var chatRoomHistory = JSON.parse(jsonChatRoomHistory);
            var chatRoomMessages = "";
            for (var history in chatRoomHistory) {
                chatRoomMessages += chatRoomHistory[history].Username;
                chatRoomMessages += " said (";
                chatRoomMessages += chatRoomHistory[history].MessageDate;
                chatRoomMessages += "): ";
                chatRoomMessages += chatRoomHistory[history].Message;
                chatRoomMessages += "\r\n";
            }
            $(textareaChatRoomMessageHistory).val(chatRoomMessages);
        }
        var charRoomHistoryHeader = $("#chat-room-history-header");
        if (charRoomHistoryHeader) $(charRoomHistoryHeader).html((username + " joined to " + chatRoomName + " chat room"));
        unlockChatRoomArea();
    }

    function setLoginError(errorMessage) {
        var loginAlert = $(".login-alert");
        if (loginAlert) {
            if ($(loginAlert).hasClass("d-none")) {
                $(loginAlert).removeClass("d-none");
                $(loginAlert).html(errorMessage)
                setTimeout(() => {
                    $(loginAlert).addClass("d-none");
                }, 5000);
            }
        }
    };

    function changeConnectionStatus() {
        var connectionStatus = $("#connection-status");
        if (connectionStatus) {
            if ($(connectionStatus).hasClass("badge-danger")) {
                $(connectionStatus).removeClass("badge-danger").addClass("badge-success");
                $(connectionStatus).html("Online")
            }
        }
    };

    function init() {
        var btnJoinChat = $("#btn-join-chat");
        var btnSendMessage = $("#btn-send-message");
        if (btnJoinChat) $(btnJoinChat).on("click", btnJoinChat_click);
        if (btnSendMessage) $(btnSendMessage).on("click", btnSendMessage_click);
    };

    function btnJoinChat_click() {
        var txtNickname = $("#txt-nickname");
        if (txtNickname) {
            var nickname = $(txtNickname).val();
            if (nickname == "") {
                var errorMessage = "Nickname is required";
                setLoginError(errorMessage);
            }
            else {
                loginRequest(nickname);
            }
        }
    };

    function btnSendMessage_click() {
        var txtMessages = $("#txt-messages");
        if (txtMessages) {
            var messages = $(txtMessages).val();
            var chatRoomName = $(txtMessages).attr("data-chatroomname");
            if (messages != "") {
                $(txtMessages).val("");
                var username = getUsernameFromCookie();
                sendMessageToChatRoomRequest(username, chatRoomName, messages);
            }
        }
    }

    function btnChatRoom_click(e) {
        var chatRoomName = $(e).data("chatroomname");
        if (chatRoomName) {
            var chatRoomContainer = $("#chat-room-container");
            if (chatRoomContainer) {
                var chatRoomCards = $(chatRoomContainer).find(".card");
                if (chatRoomCards && chatRoomCards.length != 0) {
                    var chatRoomCard = chatRoomCards[0];
                    if (chatRoomCard) $(chatRoomCard).attr("id", ("chat-room-" + chatRoomName));
                }
            }
            var txtMessages = $("#txt-messages");
            if (txtMessages) $(txtMessages).attr("data-chatroomname", chatRoomName);
            var username = getUsernameFromCookie();
            joinTheChatRoomRequest(username, chatRoomName);
        }
    }

    function joinTheChatRoomRequest(username, chatRoomName) {
        lockChatRoomArea();
        connection.invoke("JoinTheChatRoomRequest", username, chatRoomName);
    }

    function loginRequest(nickname) {
        connection.invoke("LoginRequest", nickname);
    }

    function sendMessageToChatRoomRequest(username, chatRoomName, message) {
        connection.invoke("SendMessageToChatRoomRequest", username, chatRoomName, message);
    }

    function getChatRoomListRequest() {
        connection.invoke("GetChatRoomListRequest");
    }

    function prepareChatRoomList(jsonChatRoomList) {
        var navChatRoomList = $("#nav-chat-room-list");
        if (navChatRoomList) {
            $(navChatRoomList).empty();
            var chatRoomList = JSON.parse(jsonChatRoomList);
            for (var chatRoom in chatRoomList) {
                var chatRoomId = chatRoomList[chatRoom].Id;
                var chatRoomName = chatRoomList[chatRoom].Name;
                var innerHtml = "<li class='nav-item'><a class='nav-link btn btn-primary mb-2' href='#' data-chatroomid='";
                innerHtml += chatRoomId;
                innerHtml += "' data-chatroomname='";
                innerHtml += chatRoomName;
                innerHtml += "' id='btn-chat-room-";
                innerHtml += chatRoomName;
                innerHtml += "'>Join ";
                innerHtml += chatRoomName;
                innerHtml += "</a></li>";
                $(navChatRoomList).append(innerHtml).on("click", ("li #btn-chat-room-" + chatRoomName), function (e) {
                    btnChatRoom_click(this);
                });
            }
        }
    }

    function setCookie(username) {
        document.cookie = "username=" + username;
    }

    function getUsernameFromCookie() {
        var username;
        var documentCookie = document.cookie;
        if (documentCookie != null || documentCookie != "") {
            var strArray = documentCookie.split("=");
            username = strArray[1];
        }
        return username;
    }

});