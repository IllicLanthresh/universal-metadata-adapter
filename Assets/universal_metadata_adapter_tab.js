function formatTimeAgo(utcString) {
  if (!utcString) return '';
  const then = new Date(utcString);
  const now = new Date();
  const diffMs = now - then;
  const diffSec = Math.floor(diffMs / 1000);
  if (diffSec < 60) return `${diffSec}s ago`;
  const diffMin = Math.floor(diffSec / 60);
  if (diffMin < 60) return `${diffMin}m ago`;
  const diffHr = Math.floor(diffMin / 60);
  if (diffHr < 24) return `${diffHr}h ago`;
  const diffDay = Math.floor(diffHr / 24);
  return `${diffDay}d ago`;
}

async function updateCliStatus() {
  const statusDiv = document.getElementById("uma-cli-status");
  const lastCheckedSpan = document.getElementById("uma-cli-last-checked");
  statusDiv.textContent = "Checking...";
  lastCheckedSpan.textContent = "";
  const res = await fetch("/api/universal_metadata_adapter/check_cli_status");
  const data = await res.json();
  statusDiv.textContent = data.installed
    ? `Installed: ${data.version} (${data.path})`
    : "Not installed";
  lastCheckedSpan.textContent = data.last_checked_utc
    ? `Checked ${formatTimeAgo(data.last_checked_utc)}`
    : "";
}

document.addEventListener("DOMContentLoaded", function() {
  const resultsDiv = document.getElementById("uma-fixer-results");
  updateCliStatus();

  document.getElementById("uma-refresh-cli").onclick = async function() {
    const statusDiv = document.getElementById("uma-cli-status");
    const lastCheckedSpan = document.getElementById("uma-cli-last-checked");
    statusDiv.textContent = "Refreshing...";
    lastCheckedSpan.textContent = "";
    const res = await fetch("/api/universal_metadata_adapter/refresh_cli_status", { method: "POST" });
    const data = await res.json();
    statusDiv.textContent = data.installed
      ? `Installed: ${data.version} (${data.path})`
      : "Not installed";
    lastCheckedSpan.textContent = data.last_checked_utc
      ? `Checked ${formatTimeAgo(data.last_checked_utc)}`
      : "";
  };

  document.getElementById("uma-download-cli").onclick = async function() {
    const statusDiv = document.getElementById("uma-cli-status");
    const lastCheckedSpan = document.getElementById("uma-cli-last-checked");
    statusDiv.textContent = "Downloading/updating...";
    lastCheckedSpan.textContent = "";
    const res = await fetch("/api/universal_metadata_adapter/download_or_update_cli", { method: "POST" });
    const data = await res.json();
    statusDiv.textContent = data.installed
      ? `Downloaded/Updated: ${data.version} (${data.path})`
      : "Failed to download/update CLI";
    lastCheckedSpan.textContent = data.last_checked_utc
      ? `Checked ${formatTimeAgo(data.last_checked_utc)}`
      : "";
  };

  document.getElementById("uma-run-fixer").onclick = async function() {
    resultsDiv.textContent = "Running fixer...";
    const res = await fetch("/api/universal_metadata_adapter/run_metadata_fixer", { method: "POST" });
    const data = await res.json();
    resultsDiv.textContent = `Fixed: ${data.fixed}, Errors: ${data.errors}`;
  };
}); 