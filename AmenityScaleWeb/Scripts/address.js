/// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-16-02  Patrick & Greeley       Init Script
///

/*
Open Street Maps API to get the Address Information
*/

export const addressData = {
    streetNumber: null,
    streetName: null,
    city: null,
    province: null,
    lat: null,
    lon: null,
    displayName: null,
}

const DEBUG_ADDRESS = {
    streetNumber: '216',
    streetName: 'Ontario St',
    city: 'Kingston',
    province: 'ON',
}

const toggleBtn = document.getElementById('toggle-slider')
const slider = document.getElementById('right-slider')
const form = document.getElementById('address-form')
const statusMsg = document.getElementById('geocode-status')
const submitBtn = document.getElementById('find-amenities')
const debugBtn = document.getElementById('debug-fill')

toggleBtn.addEventListener('click', () => {
    slider.classList.toggle('open')
})

debugBtn.addEventListener('click', () => {
    document.getElementById('street-number').value = DEBUG_ADDRESS.streetNumber
    document.getElementById('street-name').value = DEBUG_ADDRESS.streetName
    document.getElementById('city').value = DEBUG_ADDRESS.city
    document.getElementById('province').value = DEBUG_ADDRESS.province
    form.requestSubmit()
})

async function geocodeAddress({ streetNumber, streetName, city, province }) {
    const query = `${streetNumber} ${streetName}, ${city}, ${province}, Canada`
    const url = `https://nominatim.openstreetmap.org/search?format=json&limit=1&q=${encodeURIComponent(query)}`

    const response = await fetch(url, {
        headers: { 'Accept-Language': 'en' },
    })

    if (!response.ok) throw new Error('Nominatim request failed')

    const results = await response.json()
    if (!results.length) throw new Error('No results found for that address')

    return {
        lat: parseFloat(results[0].lat),
        lon: parseFloat(results[0].lon),
        displayName: results[0].display_name,
    }
}

form.addEventListener('submit', async (e) => {
    e.preventDefault()

    const streetNumber = document.getElementById('street-number').value.trim()
    const streetName = document.getElementById('street-name').value.trim()
    const city = document.getElementById('city').value.trim()
    const province = document.getElementById('province').value

    setStatus('Geocoding address...', '')
    submitBtn.disabled = true

    try {
        const { lat, lon, displayName } = await geocodeAddress({ streetNumber, streetName, city, province })

        Object.assign(addressData, { streetNumber, streetName, city, province, lat, lon, displayName })

        setStatus(`Found: ${displayName}`, 'success')
        console.log('[address] Address data ready:', addressData)

        document.dispatchEvent(new CustomEvent('address:resolved', { detail: { ...addressData } }))

    } catch (err) {
        setStatus(err.message, 'error')
        console.error('[address] Geocoding error:', err)
    } finally {
        submitBtn.disabled = false
    }
})

function setStatus(msg, type) {
    statusMsg.textContent = msg
    statusMsg.className = `status-msg ${type}`
}