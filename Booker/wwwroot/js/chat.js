'use strict';

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

function chatSelectThread(link) {
    // Sidebar HTMX swap: keep the highlight on the thread just opened.
    document.querySelectorAll(".chat-sidebar .thread-list a.active").forEach((active) => {
        active.classList.remove("active");
        active.removeAttribute("aria-current");
    });
    link.classList.add("active");
    link.setAttribute("aria-current", "page");
    chatScrollToNewest();
}

function chatAfterSend(form) {
    // The first sent message clears the empty-state placeholder.
    document.querySelector("#chat-history .chat-empty")?.remove();

    // The send form: clear the textarea for the next message.
    const textarea = form.querySelector("textarea");
    if (textarea) {
        textarea.value = "";
    }

    chatScrollToNewest();
}

document.addEventListener("DOMContentLoaded", chatScrollToNewest);
