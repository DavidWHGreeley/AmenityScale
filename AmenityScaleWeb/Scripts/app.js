/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Cody & Greeley          Generic Script created
/// 0.2             2026-07-03  Patrick                 Changed calls for radius to isochrone

/*
File is mostly for UI related tasks.
Acts as the central orchestrator, listens for events and delegates
to the appropriate modules.
*/

import './address.js'
import { saveAddress, getAmenitiesInIsochrone, whenLocationSelected } from './api-requests.js'
import { registerClickHandler, toggleHeatmap, panToAddress } from './map.js'

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
registerClickHandler((wkt) => {
    // Pass to API request
    whenLocationSelected(wkt);
});

document.getElementById('toggle-heatmap').addEventListener('click', () => {
    toggleHeatmap()
})