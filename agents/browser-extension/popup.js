const API_LOGIN_URL = "http://localhost:5028/api/auth/login";

document.addEventListener("DOMContentLoaded", () => {
  const loginSection = document.getElementById("loginSection");
  const statusSection = document.getElementById("statusSection");
  const agentEmailLabel = document.getElementById("agentEmail");
  const msgBox = document.getElementById("message");

  // Check state
  chrome.storage.local.get(["focustrack_jwt", "focustrack_email"], (data) => {
    if (data.focustrack_jwt) {
      loginSection.style.display = "none";
      statusSection.style.display = "block";
      agentEmailLabel.innerText = data.focustrack_email || "Authorized User";
    } else {
      loginSection.style.display = "block";
      statusSection.style.display = "none";
    }
  });

  const submitBtn = document.getElementById("loginBtn");
  submitBtn.addEventListener("click", () => {
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    if (!email || !password) {
      msgBox.innerText = "Please fill in all fields.";
      msgBox.className = "error";
      return;
    }

    submitBtn.innerText = "Authorizing...";
    
    fetch(API_LOGIN_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    })
    .then(async res => {
      const data = await res.json();
      if (!res.ok) throw new Error(data.error || "Login Failed.");
      
      chrome.storage.local.set({ focustrack_jwt: data.token, focustrack_email: email }, () => {
        msgBox.innerText = "Authorized Successfully!";
        msgBox.className = "success";
        setTimeout(() => window.close(), 1000);
      });
    })
    .catch(err => {
      msgBox.innerText = err.message;
      msgBox.className = "error";
      submitBtn.innerText = "Authorize Agent";
    });
  });

  document.getElementById("logoutBtn").addEventListener("click", () => {
    chrome.storage.local.remove(["focustrack_jwt", "focustrack_email"], () => {
      window.close();
    });
  });
});
