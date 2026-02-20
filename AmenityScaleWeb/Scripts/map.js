/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Patrick                 Init Script
/// 0.2             2026-16-02  Greeley                 Isolated this to just for google maps logic
/// 0.3				2026-19-02 	Cody					Heatmaps
///
import { displayScore } from './app.js'

const kingstonCenter = { lat: 44.245019, lng: -76.54911 }

const DEFAULT_RADIUS = 1000

let map
let markers = []
let activeMarker = null
let radiusCircle = null
let heatmap = null
let heatmapVisible = false
let onLocationSelected = null

function buildLocationPin() {
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

function clearActiveMarker() {
    if (activeMarker) {
        activeMarker.map = null
        activeMarker = null
    }
}

function drawRadiusCircle(position) {
    if (radiusCircle) {
        radiusCircle.setMap(null)
    }

    radiusCircle = new google.maps.Circle({
        map,
        center: position,
        radius: DEFAULT_RADIUS,
        strokeColor: '#006cb5',
        strokeOpacity: 0.8,
        strokeWeight: 2,
        fillColor: '#006cb5',
        fillOpacity: 0.08,
    })
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
    if (!heatmap) return
    heatmapVisible = !heatmapVisible
    heatmap.setMap(heatmapVisible ? map : null)
}

export function registerClickHandler(callback) {
    onLocationSelected = callback
}

function main() {
    map = new google.maps.Map(document.getElementById('map'), {
        center: kingstonCenter,
        zoom: 13,
        mapId: 'YOUR_MAP_ID',
    })

    attachMapClickListener()
    listenForAddressResolved()
}

function listenForAddressResolved() {
    document.addEventListener('address:resolved', (e) => {
        const { lat, lon, displayName } = e.detail
        const position = { lat, lng: lon }

        clearActiveMarker()

        activeMarker = new google.maps.marker.AdvancedMarkerElement({
            position,
            map,
            title: displayName,
            content: buildLocationPin(),
            zIndex: 999,
        })

        drawRadiusCircle(position)
        map.panTo(position)
        map.setZoom(15)

        console.log('[map] Panned to address:', position)
    })
}

function attachMapClickListener() {
    map.addListener('click', (event) => {
        const lat = event.latLng.lat()
        const lng = event.latLng.lng()
        const position = { lat, lng }

        clearActiveMarker()

        activeMarker = new google.maps.marker.AdvancedMarkerElement({
            position,
            map,
            title: 'Selected Location',
            content: buildLocationPin(),
            zIndex: 999,
        })

        drawRadiusCircle(position)

        console.log('[map] Click location selected:', position)

        if (typeof onLocationSelected === 'function') {
            onLocationSelected(position)
        }
    })
}

function attachInfoWindow(marker, content) {
    const infoWindow = new google.maps.InfoWindow({ content })

    marker.addListener('click', () => {
        infoWindow.open({ anchor: marker, map, shouldFocus: true })
    })
}

export function displayResults(data, score) {
    for (const m of markers) m.map = null
    markers = []

    for (const amenity of data) {
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
    displayScore(score)
    buildHeatmap(data)
}

window.addEventListener('load', main)