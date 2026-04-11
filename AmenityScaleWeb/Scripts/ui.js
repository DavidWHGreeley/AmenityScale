/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-02-04  Greeley                 Gets or creates a user object
/// 0.2             2026-02-04  Greeley                  UI logic, guages
/// 0.3             2026-02-04  Greeley                 Battle Mode!!!!

import Gauge from 'https://esm.sh/svg-gauge'
import { AMENITIES } from './constants.js'
import { redrawMarkers, displayBattleMarkers } from './map.js'
import { getLeaderboard, getBattlesByUser } from './api-requests.js'
import { getBattleCodeFromURL } from './battle.js'
import { drawHoverIsochrones, clearHoverIsochrones } from './map.js'
import { getIsochrones } from './api-requests.js'

const btn = document.getElementById('hammy-btn')
const menu = document.getElementById('menu-container')
const navIcon = document.getElementById('nav-hamburger')
const map = document.getElementById('map')
const loadingEl = document.getElementById('loading')
const scorePopupEl = document.getElementById('score-popup')
const closeScoreEl = document.getElementById('close-score')
const battlesView = document.getElementById('battles-view')
const leaderboardView = document.getElementById('leaderboard-view')
const battlesList = document.getElementById('battles-list')
const leaderboardList = document.getElementById('leaderboard-list')
const leaderboardStatus = document.getElementById('leaderboard-status')
const leaderboardShareUrl = document.getElementById('leaderboard-share-url')
const backToBattles = document.getElementById('back-to-battles')
let isOpen = false
let showScore = false
let showCopied = false
const togglePanel = document.getElementById('toggle-panel')
const amenityPanel = document.getElementById('amenity-panel')
const panelToggleText = document.getElementById('panel-toggle-text')
let panelOpen = true

export let locationSelected = false
export function setLocationSelected(val) { locationSelected = val }

/**
 * Async function that fetches the leaderboard data and than conditionally renders the 
 * share URL OR hides it if expired.
 * @param {any} battleCode
 * @param {any} status
 */
async function showLeaderboardView(battleCode, status) {
    const { currentUser } = await import('./app.js')

    battlesView.style.display = 'none'
    leaderboardView.style.display = 'block'

    leaderboardStatus.textContent = status
    leaderboardStatus.className = `battle-status-badge ${status}`

    const shareContainer = leaderboardView.querySelector('.input-container')
    if (status === 'expired') {
        shareContainer.style.display = 'none'
    } else {
        shareContainer.style.display = 'block'
        const shareUrl = `${window.location.origin}?code=${battleCode}`
        leaderboardShareUrl.value = shareUrl
    }

    const participants = await getLeaderboard(battleCode)
    renderLeaderboard(participants, currentUser?.UserID)
    displayBattleMarkers(participants, currentUser?.UserID, true)
}

/**
 * Builds the leaderboard table to be added to the DOM.
 * Users in the top spot are highlighted
 * @param {any} participants
 * @param {any} currentUserID
 * @returns
 */
function renderLeaderboard(participants, currentUserID) {
    leaderboardList.innerHTML = ''

    if (!participants.length) {
        leaderboardList.innerHTML = '<p class="battles-empty">No participants yet.</p>'
        return
    }

    participants.forEach((p, i) => {
        const isYou = p.UserID === currentUserID
        const row = document.createElement('div')
        row.classList.add('leaderboard-row')
        row.innerHTML = `
            <span class="leaderboard-rank ${i === 0 ? 'gold' : ''}">${i + 1}</span>
            <div style="flex:1;">
                <div class="leaderboard-name">${p.DisplayName} ${isYou ? '<span class="you-badge">YOU</span>' : ''}</div>
                <div class="leaderboard-location">${p.LocationName || ''}</div>
            </div>
            <span class="leaderboard-score">${p.Score}</span>
        `

        row.addEventListener('mouseenter', async () => {
            console.log('[hover] participant:', p)
            if (!p.LocationID) return
            const isochrones = await getIsochrones(p.LocationID)
            console.log('[hover] isochrones:', isochrones)
            drawHoverIsochrones(isochrones)
        })

        row.addEventListener('mouseleave', () => {
            clearHoverIsochrones()
        })


        leaderboardList.appendChild(row)
    })
}

/**
 * Fetches the users battles, determins expiry status, and renders history items
 * @param {any} userID
 * @returns
 */
export async function renderBattlesList(userID) {
    if (!userID) return

    const battles = await getBattlesByUser(userID)

    if (!battles.length) {
        battlesList.innerHTML = '<p class="battles-empty">No battles yet. Click the map to start one!</p>'
        return
    }

    battlesList.innerHTML = ''
    battles.forEach(b => {
        const item = document.createElement('button')
        item.classList.add('battle-history-item')

        const isExpired = new Date(b.ExpiresAt) < new Date()
        const status = isExpired ? 'expired' : b.Status

        const shortCode = b.BattleCode.substring(0, 13) + '...'
        const date = new Date(b.ExpiresAt).toLocaleDateString('en-CA', { month: 'short', day: 'numeric' })
        item.innerHTML = `
    <div class="battle-item-row">
        <span class="battle-item-code">${shortCode}</span>
        <span class="battle-status-badge ${status}">${status}</span>
    </div>
    <div class="battle-item-meta">Expires ${date}</div>
`
        item.addEventListener('click', () => showLeaderboardView(b.BattleCode, status))
        battlesList.appendChild(item)
    })
}

/*
~~~~ UI LOGIC!~~~~
Since we have a ton of battle mode, and Amenity selecting code, errors and UI / UX
issues are bound to happen. Most of the code below is surrounding UI related stuff. like
UI interactions, menu toggles, loading states, trigger for popups etc etc.
*/


btn.addEventListener('click', () => {
    isOpen = !isOpen
    menu.classList.toggle('open', isOpen)
    btn.classList.toggle('open', isOpen)
    navIcon.classList.toggle('open', isOpen)
})

export function isLoadingFn() {
    map.classList.add('loading')
    loadingEl.classList.add('loading')
    gauge3.setValueAnimated(0, 0.3)
}
export function endLoadingFn() {
    map.classList.remove('loading')
    loadingEl.classList.remove('loading')
}

export function showScoreFn(score) {
    showScore = true
    scorePopupEl.classList.toggle('showScore', showScore)
    setTimeout(() => {
        setGaugeValue(score, 2)
    }, 1)
}

closeScoreEl.addEventListener('click', () => {
    dismissScore()
})
export function dismissScore() {
    showScore = false
    scorePopupEl.classList.toggle('showScore', showScore)
    gauge2.setValueAnimated(0, 1)
}

export let gauge2 = Gauge(document.getElementById('gauge2'), {
    min: 0,
    max: 100,
    dialStartAngle: 180,
    dialEndAngle: 0,
    value: 0,
    color: function (value) {
        return '#006cb5'
    },
})

export let gauge3 = Gauge(document.getElementById('gauge3'), {
    min: 0,
    max: 100,
    dialStartAngle: 180,
    dialEndAngle: 0,
    value: 0,
    color: function (value) {
        return '#006cb5'
    },
})

export function setGaugeValue(value = 0, duration = 2) {
    gauge2.setValueAnimated(value, duration)
    gauge3.setValueAnimated(value, duration)
}


export const amenityState = {};
Object.keys(AMENITIES).forEach(k => amenityState[k] = true);

const grid = document.getElementById('amenity-grid');

function renderAmenityPanel() {
    grid.innerHTML = '';
    Object.entries(AMENITIES).forEach(([key, label]) => {
        const el = document.createElement('div');
        el.classList.add('amenity-item');
        if (amenityState[key]) el.classList.add('active');
        el.textContent = label;
        el.title = label;
        el.addEventListener('click', () => {
            amenityState[key] = !amenityState[key];
            el.classList.toggle('active', amenityState[key]);
            redrawMarkers()
        });
        grid.appendChild(el);
    });
}

document.getElementById('show-all').addEventListener('click', () => {
    Object.keys(amenityState).forEach(k => amenityState[k] = true);
    renderAmenityPanel();
    redrawMarkers()
});

document.getElementById('hide-all').addEventListener('click', () => {
    Object.keys(amenityState).forEach(k => amenityState[k] = false);
    renderAmenityPanel();
    redrawMarkers()
});

document.getElementById('share-url')?.addEventListener('click', async () => {
    const box = document.getElementById('share-url')
    await navigator.clipboard.writeText(box.value)
    document.getElementById('copied').classList.add('show')
    setTimeout(() => {
        document.getElementById('copied').classList.remove('show')
    }, 3000)
})

document.getElementById('leaderboard-share-url')?.addEventListener('click', async () => {
    const box = document.getElementById('leaderboard-share-url')
    await navigator.clipboard.writeText(box.value)
    document.getElementById('leaderboard-copied').classList.add('show')
    setTimeout(() => {
        document.getElementById('leaderboard-copied').classList.remove('show')
    }, 3000)
})

function showBattlesView() {
    battlesView.style.display = 'block'
    leaderboardView.style.display = 'none'
}

backToBattles.addEventListener('click', showBattlesView)

leaderboardShareUrl?.addEventListener('click', () => {
    navigator.clipboard.writeText(leaderboardShareUrl.value)
    document.getElementById('leaderboard-copied').classList.add('show')
    setTimeout(() => {
        document.getElementById('leaderboard-copied').classList.remove('show')
    }, 3000)
})

togglePanel.addEventListener('click', () => {
    panelOpen = !panelOpen
    amenityPanel.classList.toggle('hidden', !panelOpen)
    panelToggleText.textContent = panelOpen ? 'Hide Details' : 'Show Details'
})

renderAmenityPanel();