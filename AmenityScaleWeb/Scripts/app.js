/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Cody & Greeley          Generic Script created
/// 0.2             2026-07-03  Patrick                 Changed calls for radius to isochrone
/// 0.3             2026-12-03  Cody                    Added save-location-score event handler
/// 0.4             2026-02-04  Greeley                 Time to dual! 

/*
File is mostly for UI related tasks.
Acts as the central orchestrator, listens for events and delegates
to the appropriate modules.
*/

import './address.js'
import { saveAddress, whenLocationSelected, startBattle, joinBattle, getLeaderboard } from './api-requests.js'
import { getOrCreateUser } from './user.js'
import { getBattleCodeFromURL } from './battle.js'
import { registerClickHandler, toggleHeatmap, panToAddress, displayBattleMarkers } from './map.js'
import { renderBattlesList } from './ui.js'

export let currentUser = null
export let activeBattleCode = getBattleCodeFromURL()
let lastLocationID = null

async function init() {
    currentUser = await getOrCreateUser()
    console.log('Active user:', currentUser)

    await renderBattlesList(currentUser.UserID)

    if (activeBattleCode) {
        const existingLeaderboard = await getLeaderboard(activeBattleCode)
        const alreadyJoined = existingLeaderboard.some(p => p.UserID === currentUser.UserID)

        if (alreadyJoined) {
            new bootstrap.Modal(document.getElementById('alreadyInBattleModal')).show()
            window.history.replaceState({}, '', window.location.pathname)
            activeBattleCode = null
            return
        }

        new bootstrap.Tab(document.getElementById('tab-battle')).show()
        document.getElementById('battle-invite-banner').style.display = 'block'
        document.getElementById('battle-start-section').style.display = 'none'
        document.getElementById('battle-ribbon').style.display = 'block'
    }
}

export function showShareURL(shareUrl) {
    const box = document.getElementById('share-url')
    if (box) {
        box.value = shareUrl
        box.style.display = 'block'
    }
}

export function renderLeaderboard(participants) {
    const board = document.getElementById('leaderboard')
    if (!board) return
    board.innerHTML = '<h3>Battle Leaderboard</h3>'
    participants.forEach((p, i) => {
        board.innerHTML += `<div>${i + 1}. ${p.DisplayName} — ${p.Score}</div>`
    })
    board.setAttribute('style', 'display: block; padding: 10px;')
}

document.addEventListener('address:resolved', (e) => {
    const addressData = e.detail
    panToAddress(addressData)
    saveAddress(addressData)
})

registerClickHandler(async (wktData) => {
    const result = await whenLocationSelected(wktData)

    if (result?.locationID) {
        lastLocationID = result.locationID
        document.getElementById('start-battle-btn').disabled = false
    }

    if (activeBattleCode && result?.locationID) {
        try {
            const leaderboard = await joinBattle(activeBattleCode, currentUser.UserID, result.locationID)
            renderLeaderboard(leaderboard)
            displayBattleMarkers(leaderboard, currentUser.UserID)
            await renderBattlesList(currentUser.UserID)
            window.history.replaceState({}, '', window.location.pathname)
            activeBattleCode = null
            new bootstrap.Tab(document.getElementById('tab-battle')).show()
            document.getElementById('battle-invite-banner').style.display = 'none'
            document.getElementById('battle-ribbon').style.display = 'none'
            document.getElementById('battle-start-section').style.display = 'block'
        } catch (err) {
            console.error('Join failed:', err)
        }
    }
})

document.getElementById('start-battle-btn').addEventListener('click', async () => {
    if (!currentUser) {
        console.error('No user found')
        return
    }
    const { shareUrl, battle } = await startBattle(currentUser.UserID)
    activeBattleCode = battle.BattleCode
    showShareURL(shareUrl)

    const leaderboard = await joinBattle(activeBattleCode, currentUser.UserID, lastLocationID)
    activeBattleCode = null
    renderLeaderboard(leaderboard)
    displayBattleMarkers(leaderboard, currentUser.UserID)
    await renderBattlesList(currentUser.UserID)
})

document.getElementById('submit-location-btn').addEventListener('click', () => {
    new bootstrap.Tab(document.getElementById('tab-amenities')).show()
})

document.getElementById('toggle-heatmap').addEventListener('click', () => {
    toggleHeatmap()
})

init()