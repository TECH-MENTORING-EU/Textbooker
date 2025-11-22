'use strict';
window.chatIsland = (function () {
  function isNearBottom(el) {
    const threshold = 60; // px
    return (el.scrollHeight - el.scrollTop - el.clientHeight) < threshold;
  }
  function scrollIfNearBottom(el) {
    if (!el) return;
    if (isNearBottom(el)) {
      el.scrollTop = el.scrollHeight;
    }
  }
  function scrollToBottom(el) {
    if (!el) return;
    el.scrollTop = el.scrollHeight;
  }
  return { scrollIfNearBottom, scrollToBottom };
})();
