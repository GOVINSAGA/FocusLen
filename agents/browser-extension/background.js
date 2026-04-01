const API_URL = "http://localhost:5028/api/activity";

let activeTabId = null;
let activeWindowId = null;
let startTimestamp = null;
let currentDomain = null;
let currentTitle = null;

// Listeners for tracking active tab
chrome.tabs.onActivated.addListener((activeInfo) => {
  handleTabFocusChange(activeInfo.tabId, activeInfo.windowId);
});

chrome.windows.onFocusChanged.addListener((windowId) => {
  if (windowId === chrome.windows.WINDOW_ID_NONE) {
    // Chrome lost focus completely
    flushCurrentActivity();
    activeTabId = null;
    activeWindowId = null;
  } else {
    chrome.tabs.query({ active: true, windowId: windowId }, (tabs) => {
      if (tabs[0]) {
        handleTabFocusChange(tabs[0].id, windowId);
      }
    });
  }
});

chrome.tabs.onUpdated.addListener((tabId, changeInfo, tab) => {
  if (tabId === activeTabId && changeInfo.status === "complete") {
    // Same tab, but URL or title changed
    flushCurrentActivity();
    startNewActivity(tab);
  }
});

function handleTabFocusChange(tabId, windowId) {
  flushCurrentActivity();
  
  activeTabId = tabId;
  activeWindowId = windowId;

  chrome.tabs.get(tabId, (tab) => {
    if (chrome.runtime.lastError || !tab) return;
    startNewActivity(tab);
  });
}

function startNewActivity(tab) {
  if (!tab.url || tab.url.startsWith("chrome://")) {
    currentDomain = "chrome-internal";
    currentTitle = tab.title || "Chrome Built-in";
  } else {
    try {
      const url = new URL(tab.url);
      currentDomain = url.hostname;
      currentTitle = tab.title;
    } catch {
      currentDomain = "unknown";
      currentTitle = "Unknown";
    }
  }
  startTimestamp = Date.now();
}

function flushCurrentActivity() {
  if (!startTimestamp || !currentDomain) return;

  const durationMs = Date.now() - startTimestamp;
  const durationSeconds = Math.floor(durationMs / 1000);

  const payload = {
    appOrDomain: currentDomain,
    windowTitle: currentTitle || currentDomain,
    durationSeconds: durationSeconds,
    source: "Browser",
    timestamp: new Date().toISOString()
  };

  startTimestamp = null;

  if (durationSeconds < 1) {
    return; // Ignore sub-second activities
  }

  sendTelemetryActivity(payload);
}

function sendTelemetryActivity(payload) {
  chrome.storage.local.get(["focustrack_jwt"], (result) => {
    const token = result.focustrack_jwt;
    if (!token) {
      console.log("No backend JWT found in storage. Skipping telemetry.");
      return;
    }

    fetch(API_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(payload)
    })
    .then(r => console.log("FocusTrack Telemetry Logged", r.status))
    .catch(e => console.error("FocusTrack Telemetry Error", e));
  });
}

// Periodically flush active tab if it stays open for a long time (every 5 mins)
chrome.alarms.create("flushActivity", { periodInMinutes: 5 });
chrome.alarms.onAlarm.addListener((alarm) => {
  if (alarm.name === "flushActivity") {
    if (activeTabId) {
      flushCurrentActivity();
      // Restart clock for current tab
      chrome.tabs.get(activeTabId, (tab) => {
        if (!chrome.runtime.lastError && tab) startNewActivity(tab);
      });
    }
  }
});
