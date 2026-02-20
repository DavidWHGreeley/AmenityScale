/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Cody & Greeley          Generic Script created
///

import './address.js'
import { getAmenitiesInRadius } from './api-requests.js'
import { registerClickHandler, toggleHeatmap } from './map.js'

const DEFAULT_RADIUS = 1000

registerClickHandler((location) => {
    getAmenitiesInRadius(location.lat, location.lng, DEFAULT_RADIUS)
})

document.getElementById('toggle-heatmap').addEventListener('click', () => {
    toggleHeatmap()
})

export function displayScore(score) {
    console.log('Score', score)
}