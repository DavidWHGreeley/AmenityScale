/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Patrick                 Init Script
/// 0.2             2026-20-02  Cody & Greeley          Refactor and added Save address function
/// 0.2.1           2026-16-03  Cody                    Added Street address to JSON payload
/// 0.3             2026-02-04  Greeley                 Create User, Start battle
/// 0.4             2026-02-04  Greeley                 Added getBattlesByUser for past battles list
/// 0.5             2026-11-04  Greeley                 Wire up Create / save  Polygon from database
/// 0.6             2026-11-04  Greeley                 Wire up read Polygon from database


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

            // TODO: That also means any DTO used in GetFullNeighborhoodScore MIGHT have to change as well.
            body: JSON.stringify({
                wkt1: wktData.wkt1,
                wkt2: wktData.wkt2,
                wkt3: wktData.wkt3,
                wkt4: wktData.wkt4,
                lat: wktData.lat,
                lng: wktData.lng,

                // Street address info added
                streetNumber: wktData.streetNumber,
                street: wktData.street,
                city: wktData.city
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

        return data
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

}

/**
 * Creates a new user in the database with the given display name
 * @param {string} DisplayName
 * @returns {Object} 
 * 
 */
export async function createUser(DisplayName) {
    const response = await fetch('/api/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(DisplayName)
    })
    return await response.json()
}

/**
 * Creates a new battle in the database for the given user
 * @param {any} UserID
 * @returns
 */
export async function startBattle(UserID) {
    const response = await fetch('/api/battles', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(UserID)
    })
    return await response.json()
}

/**
 * Joins an existing battle with a scored location
 * @param {any} battleCode
 * @param {any} userID
 * @param {any} locationID
 * @returns
 */
export async function joinBattle(battleCode, userID, locationID) {
    const response = await fetch(`/api/battles/${battleCode}/join`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ userID, locationID })
    });

    if (!response.ok) {
        const err = await response.text();
        throw new Error(err);
    }

    return await response.json();
}
/**
 * Fetches the current leaderboard for a given battle
 * @param {string} battleCode 
 * @returns {Array} 
 */
export async function getLeaderboard(battleCode) {
    const response = await fetch(`/api/battles/${battleCode}/leaderboard`);
    return await response.json();
}
/**
 * Fetches all battles a user has participated in
 * @param {number} userID 
 * @returns {Array}
 */
export async function getBattlesByUser(userID) {
    const response = await fetch(`/api/battles/user/${userID}`);
    return await response.json();
}

/**
* Saves all 4 isochrone rings for a scored location
* @param {number} locationID 
* @param {Object} wktData
*/
export async function saveIsochrones(locationID, wktData) {
    const rings = [
        { travelTime: 5, wkt: wktData.wkt1 },
        { travelTime: 10, wkt: wktData.wkt2 },
        { travelTime: 20, wkt: wktData.wkt3 },
        { travelTime: 30, wkt: wktData.wkt4 },
    ]

    await fetch('/api/isochrones', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ locationID, rings })
    })
}

/**
 * Reads all isochrone rings for a given location
 * @param {number} locationID
 * @returns {Array} 
 */

export async function getIsochrones(locationID) {
    console.log('fetch')
    const response = await fetch(`/api/isochrones/${locationID}`)
    return await response.json()
}