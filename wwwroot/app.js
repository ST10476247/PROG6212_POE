let eventsData = [];
let participantsData = [];
let entriesData = [];
let resultsData = [];

document.addEventListener('DOMContentLoaded', () => {
  loadAllData();
});

function switchTab(tabId) {
  document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
  document.querySelectorAll('.nav-btn').forEach(el => el.classList.remove('active'));
  
  const targetTab = document.getElementById(`tab-${tabId}`);
  if (targetTab) targetTab.classList.add('active');
  
  const activeBtn = Array.from(document.querySelectorAll('.nav-btn')).find(b => b.getAttribute('onclick')?.includes(tabId));
  if (activeBtn) activeBtn.classList.add('active');
}

async function loadAllData() {
  try {
    const [evRes, partRes, entRes, resRes] = await Promise.all([
      fetch('/api/events'),
      fetch('/api/participants'),
      fetch('/api/entries'),
      fetch('/api/results')
    ]);

    eventsData = await evRes.json();
    participantsData = await partRes.json();
    entriesData = await entRes.json();
    resultsData = await resRes.json();

    renderEvents(eventsData);
    renderRegistrationDropdowns();
    renderEntriesTable(entriesData);
    renderResultsTable(resultsData);

    const countEl = document.getElementById('stat-events-count');
    if (countEl) countEl.innerText = eventsData.length;
  } catch (err) {
    console.error('Error loading data:', err);
  }
}

function renderEvents(list) {
  const container = document.getElementById('events-list');
  if (!container) return;

  container.innerHTML = list.map(ev => `
    <div class="event-card">
      <div class="event-img" style="background-image: url('${ev.bannerUrl}');">
        <span class="event-type-tag">${ev.eventType}</span>
      </div>
      <div class="event-body">
        <div class="event-title">${ev.eventName}</div>
        <div class="event-meta">
          📍 ${ev.location} (${ev.province})<br>
          📅 ${new Date(ev.eventDate).toLocaleDateString('en-ZA', { year: 'numeric', month: 'long', day: 'numeric' })}
        </div>
        <p style="font-size: 0.9rem; color: var(--text-muted); margin-bottom: 1rem;">${ev.description}</p>

        <!-- Weather Forecast Widget -->
        ${ev.weatherForecast ? `
          <div class="weather-widget">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 4px;">
              <span>🌤️ Live Weather: <strong>${ev.weatherForecast.locationName}</strong></span>
              <span class="weather-temp">${ev.weatherForecast.tempCelsius}°C</span>
            </div>
            <div style="color: var(--text-muted);">${ev.weatherForecast.condition} • Wind ${ev.weatherForecast.windSpeedKmH}km/h • Hum ${ev.weatherForecast.humidityPercent}%</div>
            <div style="color: var(--accent-gold); margin-top: 4px; font-weight: 600;">💡 Advice: ${ev.weatherForecast.raceDayAdvice}</div>
          </div>
        ` : ''}

        <!-- Categories List -->
        <div style="font-weight: 700; font-size: 0.85rem; color: var(--text-muted); margin-bottom: 6px;">Available Categories:</div>
        <div class="category-pill-list">
          ${ev.categories.map(c => `
            <div class="category-pill">
              <strong>${c.categoryName}</strong> (${c.distanceKm}km) - R${c.entryFeeZAR} ZAR
            </div>
          `).join('')}
        </div>

        <div style="margin-top: auto; display: flex; justify-content: space-between; align-items: center;">
          <span style="font-size: 0.8rem; color: var(--accent-green); font-weight: 700;">Status: ${ev.status}</span>
          <button class="action-btn gold" style="padding: 6px 14px; font-size: 0.85rem;" onclick="switchTab('register'); selectEventCategory(${ev.eventID});">
            Register Now
          </button>
        </div>
      </div>
    </div>
  `).join('');
}

function filterProvince(province) {
  if (province === 'All') {
    renderEvents(eventsData);
  } else {
    const filtered = eventsData.filter(e => e.province === province);
    renderEvents(filtered);
  }
}

function renderRegistrationDropdowns() {
  const pSelect = document.getElementById('reg-participant-id');
  const cSelect = document.getElementById('reg-category-id');

  if (pSelect) {
    pSelect.innerHTML = participantsData.map(p => `
      <option value="${p.participantID}">${p.firstName} ${p.lastName} (${p.clubName || 'Individual'}) - SA ID: ${p.saidOrPassport}</option>
    `).join('');
  }

  if (cSelect) {
    let optionsHtml = '';
    eventsData.forEach(ev => {
      ev.categories.forEach(cat => {
        optionsHtml += `<option value="${cat.categoryID}">${ev.eventName} - ${cat.categoryName} (${cat.distanceKm}km) - R${cat.entryFeeZAR} ZAR</option>`;
      });
    });
    cSelect.innerHTML = optionsHtml;
  }
}

function selectEventCategory(eventId) {
  const ev = eventsData.find(e => e.eventID === eventId);
  if (ev && ev.categories.length > 0) {
    const cSelect = document.getElementById('reg-category-id');
    if (cSelect) cSelect.value = ev.categories[0].categoryID;
  }
}

async function handleRegister(e) {
  e.preventDefault();
  const pId = parseInt(document.getElementById('reg-participant-id').value);
  const cId = parseInt(document.getElementById('reg-category-id').value);
  const med = document.getElementById('reg-medical').value;

  try {
    const res = await fetch('/api/entries', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ participantId: pId, categoryId: cId, medicalNotes: med })
    });

    if (res.ok) {
      const newEntry = await res.json();
      showToast(`🎉 Registration Successful! Race Bib Allocated: ${newEntry.bibNumber}`);
      loadAllData();
    }
  } catch (err) {
    console.error('Registration failed:', err);
  }
}

async function handleCreateEvent(e) {
  e.preventDefault();
  const name = document.getElementById('ev-name').value;
  const type = document.getElementById('ev-type').value;
  const date = document.getElementById('ev-date').value;
  const location = document.getElementById('ev-location').value;
  const province = document.getElementById('ev-province').value;
  const banner = document.getElementById('ev-banner').value;

  const newEv = {
    organiserID: 1,
    eventName: name,
    eventType: type,
    eventDate: date,
    location: location,
    province: province,
    bannerUrl: banner,
    status: 'Upcoming',
    description: `Official ${name} road event in ${location}.`,
    categories: [
      { categoryName: 'Main Feature Race', distanceKm: 42.2, entryFeeZAR: 450, maxCapacity: 5000, startTime: '06:00:00', cutoffHours: 6.0 }
    ],
    weatherForecast: {
      locationName: location,
      tempCelsius: 21,
      condition: 'Clear Skies',
      humidityPercent: 50,
      windSpeedKmH: 12,
      raceDayAdvice: 'Optimal race day conditions expected.'
    }
  };

  try {
    const res = await fetch('/api/events', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(newEv)
    });

    if (res.ok) {
      showToast(`✅ Event Published: ${name}`);
      document.getElementById('create-event-form').reset();
      loadAllData();
      switchTab('events');
    }
  } catch (err) {
    console.error('Event creation failed:', err);
  }
}

function renderEntriesTable(entries) {
  const tbody = document.getElementById('entries-table-body');
  if (!tbody) return;

  tbody.innerHTML = entries.map(ent => `
    <tr>
      <td>#${ent.entryID}</td>
      <td><strong style="color: var(--accent-gold);">${ent.bibNumber}</strong></td>
      <td>${ent.participantName || 'Athlete'}</td>
      <td>${ent.eventName || 'Road Race'}</td>
      <td>${ent.categoryName || 'Standard Category'}</td>
      <td><span style="color: var(--accent-green); font-weight: 700;">${ent.paymentStatus}</span></td>
      <td>${new Date(ent.registrationDate).toLocaleDateString('en-ZA')}</td>
    </tr>
  `).join('');
}

function renderResultsTable(results) {
  const tbody = document.getElementById('results-table-body');
  if (!tbody) return;

  tbody.innerHTML = results.map(r => `
    <tr>
      <td><strong>#${r.overallRank}</strong></td>
      <td style="color: var(--accent-gold); font-weight: 700;">${r.bibNumber}</td>
      <td>${r.participantName}</td>
      <td>${r.eventName}</td>
      <td>${r.categoryName}</td>
      <td>${r.gunTime}</td>
      <td>${r.chipTime}</td>
      <td><span style="color: var(--accent-green); font-weight: 700;">${r.status}</span></td>
    </tr>
  `).join('');
}

function showToast(msg) {
  const t = document.getElementById('toast');
  if (t) {
    t.innerText = msg;
    t.style.display = 'block';
    setTimeout(() => { t.style.display = 'none'; }, 4000);
  }
}
