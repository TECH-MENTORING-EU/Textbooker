// Chat page helpers: keep the transcript pinned to the newest message and
// advance the polling cursor past everything already rendered.

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

function chatAfterActivity(element) {
    if (element.hasAttribute("hx-trigger")) {
        // The polling span: point the next request past the rendered messages.
        const dealId = new URLSearchParams(element.getAttribute("hx-get") ?? "").get("dealId") ?? "";
        element.setAttribute(
            "hx-get",
            `?handler=Since&dealId=${encodeURIComponent(dealId)}&afterMessageId=${chatLastMessageId()}`);
    } else {
        // The send form: clear the textarea for the next message.
        const textarea = element.querySelector("textarea");
        if (textarea) {
            textarea.value = "";
        }
    }

    chatScrollToNewest();
}

document.addEventListener("DOMContentLoaded", chatScrollToNewest);
