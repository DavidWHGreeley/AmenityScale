/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Patrick                 Init Script
/// 0.2             2026-20-02  Cody & Greeley          Refactor and added Save address function
///


/*
This file is for API specific task
*/

import { displayResults } from './map.js'


export async function whenLocationSelected(wktData) {

    const url = `/api/GetFullNeighborhoodScore`;

    try {
        // Need to use POST because there isnt enough room in a url to send the 4 polygon shapes
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            // Convert to JSON string
            body: JSON.stringify({
                wkt1: wktData.wkt1,
                wkt2: wktData.wkt2,
                wkt3: wktData.wkt3,
                wkt4: wktData.wkt4
            })
        
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Server call failed: `, response.status);
        }

        const data = await response.json();

        // Display the results
        if (data.amenities) {
            displayResults(data.amenities, data.totalScore);
        }
    } catch (error) {
        console.error('[api] Scoring fetch failed:', error);
    }
}


// Fetches all amenities within the generated isochrone polygons

export async function getAmenitiesInIsochrone(wkt) {

    if (!wkt) {
        console.error('Missing parameter for getAmenitiesInIsochrone');
        return;
    }

    if (typeof wkt != 'string') {
        console.error('WKT is not a string.', wkt);
        return;
    }


    const param = new URLSearchParams({ wkt: wkt });
    const url = `/api/getAmenitiesInIsochrone?${param}`;

    try {
        const response = await fetch(url)
        if (!response.ok) throw new Error('Server call failed')
        const { amenities, score } = await response.json()
        console.log(amenities)
        if (Array.isArray(amenities)) {
            displayResults(amenities, score)
        }
    } catch (error) {
        console.error('[api] Amenity fetch failed (isochronal):', error)
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