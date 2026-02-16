// Define a point in the center of Kingston, Ontario
const kingstonCenter = { lat: 44.245019, lng: -76.54911 };

var map;

let markers = [];


// Create function to initialize map
function initMap () {
    
    // Create google maps Map object
    map = new google.maps.Map(document.getElementById("map"), {
        center: kingstonCenter,
        zoom: 13
    });

    new google.maps.Marker({
        position: kingstonCenter,
        map: map,
        title: "Kingston Center"
    });
};



// "async" lets everything else work while the database is being accessed
async function getAmenitiesInRadius(lat, lng, radius) {

    // Store the web address that runs the GetAmenitiesInRadius funtion when loaded
    const urlRadiusFunction = `/api/NearbyAmenities?lat=${lat}&lng=${lng}&radius=${radius}`;

    try {
        const response = await fetch(urlRadiusFunction);

        // If server call fails
        if (!response.ok) throw new Error("Server call failed");


        // Store output from server call
        const nearbyAmenities = await response.json();

        // Verify that an array was recieved from the server, and pass it into displayResults if so
        if (Array.isArray(nearbyAmenities)) {
            displayResults(nearbyAmenities);
        }
    } catch (error) {
        console.error("Amenity fetch failed: ", error);
    }
}



// Update map
function displayResults(data) {

    // Remove markers currently on the map and reset the list of markers
    for (let i = 0; i < markers.length; i++) {
        const marker = markers[i];
        marker.setMap(null);
    }
    markers = [];


    // Iterate through the input set of amenities
    // for (var j = 0; j < data.length; j++) {
    for (var j = 0; j < 200; j++) {

        var amenity = data[j];

        var lat = amenity.Latitude;
        var lng = amenity.Longitude;
        var name = amenity.Name;
        var distance = amenity.DistanceInMeters;


        // Create marker for amenity
        var amenityMarker = new google.maps.Marker({
            position: {lat: lat, lng: lng},
            map: map,
            title: name
        });

        // Create pop up window for amenity marker
        const amenityInfoWindow = new google.maps.InfoWindow({
            content: "<h2>" + name + "</h2><p>Distance: " + distance + " meters</p>"
        });

        // Assign info window to amenity marker
        amenityMarker.addListener("click", () => {
            amenityInfoWindow.open({
                anchor: amenityMarker,
                map,
                shouldFocus: true,
            })
        });

        // Add marker to list of markers
        markers.push(amenityMarker);
    };
};

