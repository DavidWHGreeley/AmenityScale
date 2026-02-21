/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Cody & Greeley          Generic Script created
///

/*
File is mostly for UI related tasks.
Acts as the central orchestrator, listens for events and delegates
to the appropriate modules.
*/

import './address.js'
import { getAmenitiesInRadius, saveAddress } from './api-requests.js'
import { registerClickHandler, toggleHeatmap, panToAddress } from './map.js'
import { DEFAULT_RADIUS } from './constants.js'

document.addEventListener('address:resolved', (e) => {
    const addressData = e.detail
    panToAddress(addressData)
    getAmenitiesInRadius(addressData.lat, addressData.lon, DEFAULT_RADIUS)
    saveAddress(addressData)
})

registerClickHandler((location) => {
    getAmenitiesInRadius(location.lat, location.lng, DEFAULT_RADIUS)
})

document.getElementById('toggle-heatmap').addEventListener('click', () => {
    toggleHeatmap()
})