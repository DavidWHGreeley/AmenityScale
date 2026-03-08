/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-03-04  Patrick                 Init Script
/// 0.2             2026-03-07  Patrick                 Updated for ORS isochrones



export async function generateIsochroneWKT(inputPoint, minutesArray) {

    // Check type of input (may not be necessary but I added this when fixing bugs)
    const lng = typeof inputPoint.lng === 'function' ? inputPoint.lng() : inputPoint.lng;
    const lat = typeof inputPoint.lat === 'function' ? inputPoint.lat() : inputPoint.lat;
    console.log(minutesArray);

    // Convert array into a comma separated string
    const minutesStr = minutesArray.join(',');

    // Call api using GetIsochrone function in ORSController.cs
    const url = `/api/GetIsochrone?lng=${lng}&lat=${lat}&minutes=${minutesStr}`;
    const response = await fetch(url);
    if (!response.ok) throw new Error("Server proxy failed");

    const data = await response.json();

    // Organize ORS data
    const isochrones = data.features.map(feature => {
        // ORS uses seconds for time, so convert to minutes
        const minutes = feature.properties.value / 60;

        const coordinates = feature.geometry.coordinates[0];
        const finalPoints = coordinates.map(coord => new google.maps.LatLng(coord[1], coord[0]));

        // Convert the features in the array ORS returns into WKT format
        const wktCoords = coordinates.map(coord => `${coord[0]} ${coord[1]}`).join(', ');
        const wkt = `POLYGON((${wktCoords}))`;

        // Return time, polygon WKT, and nodes of isochrone polygons
        return { minutes, wkt, paths: finalPoints };
    });

    return isochrones;
}

