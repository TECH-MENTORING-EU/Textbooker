// Chat page helpers: keep the transcript pinned to the newest message and
// feed the polling cursor (hx-vals evaluates chatLastMessageId per request).

function chatLastMessageId() {
    const items = document.querySelectorAll("#chat-history [data-msg-id]");
    return items.length ? items[items.length - 1].dataset.msgId : "0";
}

function chatScrollToNewest() {
    const history = document.getElementById("chat-history");
    if (history) {
        history.scrollTop = history.scrollHeight;
    }
}

function chatAfterSend(form) {
    // The send form: clear the textarea for the next message.
    const textarea = form.querySelector("textarea");
    if (textarea) {
        textarea.value = "";
    }

    chatScrollToNewest();
}

document.addEventListener("DOMContentLoaded", chatScrollToNewest);
