/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Patrick                 Init Script
/// 0.2             2026-16-02  Greeley                 Isolated this to just for google maps logic
/// 0.3				2026-19-02 	Cody					Heatmaps
/// 0.4             2026-07-03  Patrick                 Changes from radius to isochrone
/// 0.4.1           2026-12-03  Cody                    Added location score to the initial pin


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


function buildLocationPin() {
    console.log('testing')
    const pin = new google.maps.marker.PinElement({
        background: '#006cb5',
        borderColor: '#004f8a',
        glyphColor: '#ffffff',
    })
    return pin.element
}

function buildAmenityPin() {
    const pin = new google.maps.marker.PinElement({
        background: '#33b5bd',
        borderColor: '#228a91',
        glyphColor: '#ffffff',
    })
    return pin.element
}

// Removes the previously placed location marker from the map.
// Called before placing a new one to ensure only one active marker exists at a time.
function clearActiveMarker() {
    if (activeMarker) {
        activeMarker.map = null
        activeMarker = null
    }
}

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


// Builds a heatmap layer from amenity coordinates.
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
            // Gets address from coordinates
            const address = await reverseGeocode({ lat: lat, lon: lng });

            // Call the function in isochrones.js to create isochrone polygons
            const allIsochrones = await generateIsochroneWKT(event.latLng, TRAVEL_TIMES);

            // Add each polygon to the map from largest to smallest
            const sortedIsochrones = [...allIsochrones].sort((a, b) => b.minutes - a.minutes);
            drawIsochronePolygons(sortedIsochrones.map(iso => iso.paths));

            // Format the polygons inside the sorted list
            if (typeof onLocationSelected === 'function') {
                let wktData = {}
                let counter = 1;

                for (let i = sortedIsochrones.length - 1; i >= 0; i--) {
                    wktData["wkt" + counter] = sortedIsochrones[i].wkt;
                    counter++;
                }

                wktData.lat = lat;
                wktData.lng = lng;

                // Address data
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

function attachInfoWindow(marker, content) {
    const infoWindow = new google.maps.InfoWindow({ content })

    marker.addListener('click', () => {
        infoWindow.open({ anchor: marker, map, shouldFocus: true })
    })
}

// Clears all existing amenity markers, plots fresh ones from the result set,
// attaches info windows, updates the score display, and rebuilds the heatmap.
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

export function clearBattleMarkers() {
    for (const marker of battleMarkers) marker.map = null
}

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

window.addEventListener('load', main)
