/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Patrick                 Init Script
/// 0.2             2026-20-02  Cody & Greeley          Refactor and added Save address function
///


/*
This file is for API specific task
*/

import { displayResults } from './map.js'
import { DEFAULT_RADIUS } from './constants.js'

// Fetches all amenities within the given radius of a lat/lng coordinate.

export async function getAmenitiesInRadius(lat, lng, radius = DEFAULT_RADIUS) {
    if (lat == null || lng == null || radius == null) {
        console.error('Missing parameters for getAmenitiesInRadius')
        return
    }

    const params = new URLSearchParams({ lat, lng, radius })
    const url = `/api/GetAmenitiesInRadius?${params}`

    try {
        const response = await fetch(url)
        if (!response.ok) throw new Error('Server call failed')
        const { amenities, score } = await response.json()
        console.log(amenities)
        if (Array.isArray(amenities)) {
            displayResults(amenities, score)
        }
    } catch (error) {
        console.error('[api] Amenity fetch failed:', error)
    }
}

export async function saveAddress(addressData) {
    const payload = {
        streetNumber: addressData.streetNumber,
        streetName: addressData.streetName,
        city: addressData.city,
        province: addressData.province,
        lat: addressData.lat,
        lon: addressData.lon,
    }

    console.log('[api] TODO saveAddress — would POST to /api/SaveAddress:', payload)
}