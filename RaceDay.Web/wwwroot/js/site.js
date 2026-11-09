// RaceDay Platform Interactive Front-End Scripts
document.addEventListener("DOMContentLoaded", function () {
    // Auto-dismiss alerts after 6 seconds
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 6000);
    });

    // Auto calculate ranks from finish times in Organiser Results entry page
    const autoRankBtn = document.getElementById("btnAutoRank");
    if (autoRankBtn) {
        autoRankBtn.addEventListener("click", function () {
            const rows = Array.from(document.querySelectorAll(".results-entry-row"));
            const times = [];

            rows.forEach((row, index) => {
                const timeInput = row.querySelector(".time-input");
                const posInput = row.querySelector(".pos-input");
                const val = (timeInput.value || "").trim();

                if (val && val !== "00:00:00") {
                    times.push({
                        row: row,
                        posInput: posInput,
                        timeSec: parseTimeToSeconds(val)
                    });
                }
            });

            // Sort ascending by time
            times.sort((a, b) => a.timeSec - b.timeSec);
            times.forEach((item, idx) => {
                item.posInput.value = idx + 1;
                item.row.classList.add("table-success");
            });
        });
    }

    function parseTimeToSeconds(str) {
        const parts = str.split(":").map(Number);
        if (parts.length === 3) {
            return parts[0] * 3600 + parts[1] * 60 + parts[2];
        } else if (parts.length === 2) {
            return parts[0] * 60 + parts[1];
        }
        return 999999;
    }
});

// Quick fill test credentials for presentation video
function fillCredentials(email, password) {
    const emailField = document.getElementById("loginEmail");
    const passField = document.getElementById("loginPassword");
    if (emailField && passField) {
        emailField.value = email;
        passField.value = password;
        emailField.focus();
    }
}
