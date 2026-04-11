/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Patrick                 Init Script
/// 0.2             2026-16-02  Greeley                 Isolated this to just for google maps logic
/// 0.3				2026-19-02 	Greeley					Heatmaps
/// 0.4             2026-07-03  Patrick                 Changes from radius to isochrone
/// 0.4.1           2026-12-03  Cody                    Added location score to the initial pin
/// 0.5             2026-04-02  Greeley                 Added battle markers 
/// 0.6             2026-04-02  Greeley                 clearBattleMarkers on new map click
/// 0.7             2026-04-02  Greeley                 displayBattleMarkers supports showAll flag for past battles

/*
This file is for Google maps specific task
*/

import { TRAVEL_TIMES } from './constants.js'
import { generateIsochroneWKT } from './isochrones.js';
import { reverseGeocode } from "./address.js";
import { isLoadingFn, showScoreFn, endLoadingFn, setLocationSelected, amenityState, locationSelected } from './ui.js'

let battleMarkers = []

const kingstonCenter = { lat: 44.245019, lng: -76.54911 }


let map
let markers = []
let isochronePolygons = [];
let activeMarker = null
let heatmap = null
let heatmapVisible = false
let onLocationSelected = null
let amenities = null

let hoverIsochronePolygons = []

/**
 *  clears existing polygons and redraws them from a new path list
 * @param {any} pathList
 */
function buildLocationPin() {
    const pin = new google.maps.marker.PinElement({
        background: '#006cb5',
        borderColor: '#004f8a',
        glyphColor: '#ffffff',
    })
    return pin.element
}

/**
 *  clears existing polygons and redraws them from a new path list
 * @param {any} pathList
 */
function buildAmenityPin() {
    const pin = new google.maps.marker.PinElement({
        background: '#33b5bd',
        borderColor: '#228a91',
        glyphColor: '#ffffff',
    })
    return pin.element
}

function clearActiveMarker() {
    if (activeMarker) {
        activeMarker.map = null
        activeMarker = null
    }
}
/**
 *  clears existing polygons and redraws them from a new path list
 * @param {any} pathList
 * Original Auth: Patrick
 */
function drawIsochronePolygons(pathList = []) {

    isochronePolygons.forEach(isoPoly => {
        if (isoPoly) isoPoly.setMap(null);
    });

    isochronePolygons = [];

    pathList.forEach(isoPath => {
        const newIsoPoly = new google.maps.Polygon({
            paths: isoPath,
            map: map,
            strokeColor: '#006cb5',
            strokeOpacity: 0.8,
            strokeWeight: 2,
            fillColor: '#006cb5',
            fillOpacity: 0.08,
        });

        isochronePolygons.push(newIsoPoly);

    });
}


function buildHeatmap(data) {
    const points = data.map((amenity) =>
        new google.maps.LatLng(amenity.Latitude, amenity.Longitude)
    )

    if (heatmap) {
        heatmap.setMap(null)
    }

    heatmap = new google.maps.visualization.HeatmapLayer({
        data: points,
        map: heatmapVisible ? map : null,
        radius: 40,
    })
}


export function toggleHeatmap() {
    if (!locationSelected) {
        new bootstrap.Modal(document.getElementById('noLocationModal')).show()
        return
    }
    if (!heatmap) return
    heatmapVisible = !heatmapVisible
    heatmap.setMap(heatmapVisible ? map : null)
}

export function registerClickHandler(callback) {
    onLocationSelected = callback
}

/**
 * Called by app.js when address:resolved fires.
 * Pans the map to the resolved address and drops a marker.
 */
export function panToAddress({ lat, lon, displayName }) {
    const position = { lat, lng: lon }

    clearActiveMarker()


    activeMarker = new google.maps.marker.AdvancedMarkerElement({
        position,
        map,
        title: displayName,
        content: buildLocationPin(),
        zIndex: 999,
    })
    const content = `<h2>${displayName}</h2>`
    attachInfoWindow(activeMarker, content)
    map.panTo(position)
    map.setZoom(15)

    console.log('[Location] Panned to address:', position)
}

/**
 * Calls the function that also calls the UI function to show the score
 * @param {any} score
 * @returns
 */
export function displayScore(score) {
    console.log('[Location] Score:', score)

    if (!activeMarker) return

    showScoreFn(score)
}

async function main() {
    await google.maps.importLibrary("maps");
    await google.maps.importLibrary("marker");
    await google.maps.importLibrary("visualization");

    map = new google.maps.Map(document.getElementById('map'), {
        center: kingstonCenter,
        zoom: 13,
        mapId: 'SomeID',
        mapTypeControl: false,
        fullscreenControl: false
    })

    attachMapClickListener()
}


/**
 * Attaches a click listener to the map.
 * On click: drops a marker, reverse geocodes the position, generates isochrone
 * polygons sorted largest to smallest, and fires the onLocationSelected callback
 * with the WKT data and address info.
 */
function attachMapClickListener() {
    map.addListener('click', async (event) => {

        const lat = event.latLng.lat()
        const lng = event.latLng.lng()
        const position = { lat, lng }

        clearActiveMarker()
        clearBattleMarkers()
        setLocationSelected(true)

        activeMarker = new google.maps.marker.AdvancedMarkerElement({
            position,
            map,
            title: 'Selected Location',
            content: buildLocationPin(),
            zIndex: 999,
        })

        try {
            isLoadingFn()
            amenities = null
            const address = await reverseGeocode({ lat: lat, lon: lng });

            const allIsochrones = await generateIsochroneWKT(event.latLng, TRAVEL_TIMES);

            const sortedIsochrones = [...allIsochrones].sort((a, b) => b.minutes - a.minutes);
            drawIsochronePolygons(sortedIsochrones.map(iso => iso.paths));

            if (typeof onLocationSelected === 'function') {
                let wktData = {}
                let counter = 1;

                for (let i = sortedIsochrones.length - 1; i >= 0; i--) {
                    wktData["wkt" + counter] = sortedIsochrones[i].wkt;
                    counter++;
                }

                wktData.lat = lat;
                wktData.lng = lng;

                wktData.streetNumber = address.streetNumber
                wktData.street = address.street
                wktData.city = address.city

                onLocationSelected(wktData);
            }

        } catch (error) {
            console.error("Isochrone calculation failed:", error);
            endLoadingFn()
        }

    })
}

/**
 * Attaches a click-triggered info window to a marker.
 * @param {google.maps.marker.AdvancedMarkerElement} marker
 * @param {string} content - HTML string to display in the info window.
 * Auth Greeley
 */
function attachInfoWindow(marker, content) {
    const infoWindow = new google.maps.InfoWindow({ content })

    marker.addListener('click', () => {
        infoWindow.open({ anchor: marker, map, shouldFocus: true })
    })
}

/**
 * Attaches a click-triggered info window to a marker.
 * @param {google.maps.marker.AdvancedMarkerElement} marker
 * @param {string} content - HTML string to display in the info window.
 * Auth Greeley
 */
export async function displayResults(data, score) {
    for (const m of markers) m.map = null
    markers = []
    amenities = data

    for (const amenity of amenities) {
        if (!amenityState[amenity.CategoryName]) continue

        const amenityMarker = new google.maps.marker.AdvancedMarkerElement({
            position: { lat: amenity.Latitude, lng: amenity.Longitude },
            map,
            title: amenity.Name,
            content: buildAmenityPin(),
        })

        const content = `<h2>${amenity.Name}</h2><p>Distance: ${amenity.DistanceInMeters} meters</p>`
        attachInfoWindow(amenityMarker, content)
        markers.push(amenityMarker)
    }

    endLoadingFn()
    displayScore(score)
    buildHeatmap(data)
}

/**
 * Re-renders markers used when filtering amenities.
 * @returns
 * Auth Greeley
 */
export function redrawMarkers() {
    if (!amenities) return
    for (const m of markers) m.map = null
    markers = []

    for (const amenity of amenities) {
        if (!amenityState[amenity.CategoryName]) continue

        const amenityMarker = new google.maps.marker.AdvancedMarkerElement({
            position: { lat: amenity.Latitude, lng: amenity.Longitude },
            map,
            title: amenity.Name,
            content: buildAmenityPin(),
        })

        const content = `<h2>${amenity.Name}</h2><p>Distance: ${amenity.DistanceInMeters} meters</p>`
        attachInfoWindow(amenityMarker, content)
        markers.push(amenityMarker)
    }
}

/**
 * Removes all battle markers from the map.
 * Auth: Greeley
 * 
 */
export function clearBattleMarkers() {
    for (const marker of battleMarkers) marker.map = null
}

/**
 * Plots markers for all battle participants on the map. Color code them. Skips the current user of marker if show all is true (Leaderboard view for example)
 * @param {any} participants
 * @param {any} currentUserID
 * @param {any} showAll
 * Auth: Greeley
 */
export function displayBattleMarkers(participants, currentUserID, showAll = false) {
    clearBattleMarkers()

    for (const participant of participants) {
        if (!participant.Latitude || !participant.Longitude) continue

        const isCurrentUser = participant.UserID === currentUserID
        if (isCurrentUser && !showAll) continue

        const pin = new google.maps.marker.PinElement({
            background: isCurrentUser ? '#006cb5' : '#f05a2a',
            borderColor: isCurrentUser ? '#004f8a' : '#c43d0e',
            glyphColor: '#ffffff',
        })

        const marker = new google.maps.marker.AdvancedMarkerElement({
            position: { lat: Number(participant.Latitude), lng: Number(participant.Longitude) },
            map,
            title: participant.DisplayName,
            content: pin.element,
            zIndex: 998,
        })

        const content = `
            <div>
                <h3>${participant.DisplayName}${isCurrentUser ? ' (You)' : ''}</h3>
                <p>Score: <strong>${participant.Score}</strong></p>
                <p>${participant.LocationName || ''}</p>
            </div>`

        attachInfoWindow(marker, content)
        battleMarkers.push(marker)
    }
}

export function clearHoverIsochrones() {
    hoverIsochronePolygons.forEach(p => p.setMap(null))
    hoverIsochronePolygons = []
}

export function drawHoverIsochrones(wktList) {
    console.log('[map] drawHoverIsochrones called with:', wktList)
    clearHoverIsochrones()

    wktList.forEach(iso => {
        const polygon = new google.maps.Polygon({
            paths: parseWKTToLatLng(iso.PolygonWKT),
            map: map,
            strokeColor: '#f05a2a',
            strokeOpacity: 0.6,
            strokeWeight: 2,
            fillColor: '#f05a2a',
            fillOpacity: 0.05,
        })
        hoverIsochronePolygons.push(polygon)
    })
}

function parseWKTToLatLng(wkt) {
    const coords = wkt.replace('POLYGON((', '').replace('))', '').split(', ')
    return coords
        .map(coord => {
            const parts = coord.trim().split(' ')
            const lng = parseFloat(parts[0])
            const lat = parseFloat(parts[1])
            return { lat, lng }
        })
        .filter(point => isFinite(point.lat) && isFinite(point.lng))
}

window.addEventListener('load', main)
