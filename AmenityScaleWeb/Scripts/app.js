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
import { saveAddress, getAmenitiesInIsochrone, whenLocationSelected, startBattle, joinBattle } from './api-requests.js'
import { getOrCreateUser } from './user.js'
import { getBattleCodeFromURL } from './battle.js'
import { registerClickHandler, toggleHeatmap, panToAddress } from './map.js'
import { renderBattlesList } from './ui.js'

export let currentUser = null
export let activeBattleCode = getBattleCodeFromURL()

async function init() {
    currentUser = await getOrCreateUser()
    console.log(' Active user:', currentUser)

    await renderBattlesList(currentUser.UserID)

    if (activeBattleCode) {
        console.log('Battle Code Detected!')
        document.getElementById('battle-invite-banner').style.display = 'block'
        document.getElementById('battle-start-section').style.display = 'none'
        document.getElementById('battle-ribbon').style.display = 'block'
    }
}

export function showShareURL(shareUrl) {
    const box = document.getElementById('share-url');
    if (box) {
        box.value = shareUrl;
        box.style.display = 'block';
    }
}

export function renderLeaderboard(participants) {
    const board = document.getElementById('leaderboard');
    if (!board) return;

    board.innerHTML = '<h3> Battle Leaderboard</h3>';
    participants.forEach((p, i) => {
        board.innerHTML += `<div>${i + 1}. ${p.DisplayName} � ${p.Score}</div>`;
    });
    board.style.display = 'block';
}

// Central event listener for when an address is resolved. It will pan the map to the address,
// fetch amenities in the area, and save the address to local storage* (Not implemented eyt).
document.addEventListener('address:resolved', (e) => {
    const addressData = e.detail
    panToAddress(addressData)
    getAmenitiesInIsochrone(wkt)
    saveAddress(addressData)
})

// Triggered on direct map clicks, allowing users to explore amenities
// for any location without entering an address.
registerClickHandler(async (wktData) => {
    const result = await whenLocationSelected(wktData);

    if (result?.locationID && currentUser && !activeBattleCode) {
        const { shareUrl, battle } = await startBattle(currentUser.UserID);
        showShareURL(shareUrl);
        activeBattleCode = battle.BattleCode;
    }

    if (activeBattleCode && result?.locationID) {
        try {
            const leaderboard = await joinBattle(activeBattleCode, currentUser.UserID, result.locationID);
            console.log('Leaderboard', leaderboard)
            renderLeaderboard(leaderboard)
            await renderBattlesList(currentUser.UserID)
            window.history.replaceState({}, '', window.location.pathname)
            activeBattleCode = null
        } catch (err) {
            console.error('Join failed:', err);
        }
    }
});

document.getElementById('toggle-heatmap').addEventListener('click', () => {
    toggleHeatmap()
})

function SaveLocationRequest(locaitonData) {
    //TODO - HELP WITH CREATING THIS FUNCTION
}

// document.getElementById('save-location-score').addEventListener('click', () => {
//     if (!currentLocation || currentScore === null) {
//         alert('No location or score data available to save.');
//         return;
//     }

//     const locationData = {
//         LocationName: currentLocation.name || '',
//         StreetNumber: document.getElementById('street-number').value,
//         Street: document.getElementById('street-name').value,
//         City: document.getElementById('city').value,
//         SubdivisionID: currentLocation.subdivisionId || 0, // adjust as needed
//         Latitude: currentLocation.Latitude,
//         Longitude: currentLocation.Longitude,
//         LocationWKT: currentLocation.LocationWKT || '',
//         GeometryType: currentLocation.GeometryType || 'POINT',
//         CalculatedScore: currentScore,
//         CreatedDate: new Date().toISOString()
//     };

//     SaveLocationRequest(locationData);
// });

init();
