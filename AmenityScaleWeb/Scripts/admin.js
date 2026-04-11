// <summary>
/// Version         Date        Coder                   Remarks
/// 0.1             2026-04-10  Greeley                 Admin dashboard! Amenities, Locations, and Battles CRUD via AG Grid. Ripped from the old Loot.io game I worked on.

const BASE = ''

async function api(method, path, body) {
    const options = { method, headers: { 'Content-Type': 'application/json' } }
    if (body) options.body = JSON.stringify(body)
    const response = await fetch(BASE + path, options)
    if (!response.ok) throw new Error(`${method} ${path} → ${response.status}`)
    return response.json().catch(() => null)
}

function toast(msg, type = 'ok') {
    const colors = { ok: '#1ec97e', err: '#ef4444', info: '#3b82f6' }
    document.getElementById('toastDot').style.background = colors[type] || colors.ok
    document.getElementById('toastMsg').textContent = msg
    const toastEl = document.getElementById('toast')
    toastEl.classList.add('show')
    setTimeout(() => toastEl.classList.remove('show'), 2800)
}

const TAB_TITLES = { amenities: 'Amenities', locations: 'Locations', battles: 'Battles' }

function showTab(id, btn) {
    document.querySelectorAll('.tab-page').forEach(page => page.classList.remove('active'))
    document.querySelectorAll('.nav-item').forEach(navBtn => navBtn.classList.remove('active'))
    document.getElementById('tab-' + id).classList.add('active')
    btn.classList.add('active')
    document.getElementById('pageTitle').textContent = TAB_TITLES[id]
    if (id === 'battles') refreshBattleGrid()
}

function openModal(id) {
    document.getElementById(id).classList.add('open')
}

function closeModal(id) {
    document.getElementById(id).classList.remove('open')
}

let amenityData = []
let amenityEditId = null
let categories = []
let subdivisions = []
let amenityGrid

const amenityColDefs = [
    { headerCheckboxSelection: true, checkboxSelection: true, width: 44, pinned: 'left', resizable: false, sortable: false, suppressMenu: true },
    { field: 'AmenityID', headerName: '#', width: 70, sortable: true },
    { field: 'Name', headerName: 'Name', flex: 1.5, sortable: true, editable: true },
    {
        field: 'CategoryName', headerName: 'Category', flex: 1, sortable: true,
        cellRenderer: params => {
            const category = categories.find(c => c.CategoryName === params.value)
            const cssClass = category?.IsNegative ? 'badge-red' : 'badge-green'
            return `<span class="badge ${cssClass}">${params.value}</span>`
        }
    },
    { field: 'Street', headerName: 'Street', flex: 1, sortable: true, editable: true },
    { field: 'City', headerName: 'City', width: 110, sortable: true, editable: true },
    { field: 'Latitude', headerName: 'Lat', width: 100, sortable: true, valueFormatter: params => params.value?.toFixed(5) ?? '—' },
    { field: 'Longitude', headerName: 'Lng', width: 110, sortable: true, valueFormatter: params => params.value?.toFixed(5) ?? '—' },
    {
        headerName: 'Actions', width: 140, pinned: 'right', sortable: false, editable: false, suppressMenu: true,
        cellRenderer: params => {
            const wrapper = document.createElement('div')
            wrapper.className = 'cell-acts'

            const editBtn = document.createElement('button')
            editBtn.className = 'cell-btn cell-btn-edit'
            editBtn.textContent = 'Edit'
            editBtn.onclick = () => openAmenityModal(params.data)

            const deleteBtn = document.createElement('button')
            deleteBtn.className = 'cell-btn cell-btn-del'
            deleteBtn.textContent = 'Delete'
            deleteBtn.onclick = async () => {
                await api('DELETE', `/api/amenities/${params.data.AmenityID}`)
                amenityData = amenityData.filter(row => row.AmenityID !== params.data.AmenityID)
                refreshAmenityGrid()
                toast('Amenity deleted', 'err')
            }

            wrapper.appendChild(editBtn)
            wrapper.appendChild(deleteBtn)
            return wrapper
        }
    }
]

async function initAmenityGrid() {
    const [fetchedAmenities, fetchedCategories] = await Promise.all([
        api('GET', '/api/amenities'),
        api('GET', '/api/amenities/categories')
    ])

    amenityData = fetchedAmenities ?? []
    categories = fetchedCategories ?? []
    subdivisions = [
        { SubdivisionID: 1, CountryID: 1, SubdivisionCode: 'ON', SubdivisionName: 'Ontario' }
    ]

    amenityGrid = agGrid.createGrid(document.getElementById('amenityGrid'), {
        columnDefs: amenityColDefs,
        rowData: amenityData,
        rowSelection: 'multiple',
        animateRows: true,
        stopEditingWhenCellsLoseFocus: true,
        defaultColDef: { resizable: true },
        onSelectionChanged() {
            document.getElementById('amenityDelSelBtn').disabled = amenityGrid.getSelectedRows().length === 0
            document.getElementById('stat-amenity-filtered').textContent = amenityGrid.getDisplayedRowCount()
        },
        onCellValueChanged(event) {
            const index = amenityData.findIndex(row => row.AmenityID === event.data.AmenityID)
            if (index !== -1) amenityData[index] = { ...event.data }
            api('PUT', '/api/amenities', event.data)
            toast(`${event.colDef.headerName} updated`)
        },
        onModelUpdated() { updateAmenityStats() }
    })

    updateAmenityStats()
    populateSelects()
}

function refreshAmenityGrid() {
    amenityGrid.setGridOption('rowData', amenityData)
    updateAmenityStats()
}

function updateAmenityStats() {
    if (!amenityGrid) return
    document.getElementById('stat-amenity-total').textContent = amenityData.length
    document.getElementById('stat-amenity-cats').textContent = [...new Set(amenityData.map(row => row.CategoryName))].length
    document.getElementById('stat-amenity-filtered').textContent = amenityGrid.getDisplayedRowCount()
}

async function deleteSelectedAmenities() {
    const selectedIds = amenityGrid.getSelectedRows().map(row => row.AmenityID)
    await Promise.all(selectedIds.map(id => api('DELETE', `/api/amenities/${id}`)))
    amenityData = amenityData.filter(row => !selectedIds.includes(row.AmenityID))
    refreshAmenityGrid()
    toast(`${selectedIds.length} amenity(s) deleted`, 'err')
}

function openAmenityModal(data = null) {
    amenityEditId = data ? data.AmenityID : null
    document.getElementById('amenityModalTitle').textContent = data ? 'Edit Amenity' : 'Add Amenity'
    document.getElementById('am-name').value = data?.Name ?? ''
    document.getElementById('am-street').value = data?.Street ?? ''
    document.getElementById('am-city').value = data?.City ?? ''
    document.getElementById('am-lat').value = data?.Latitude ?? ''
    document.getElementById('am-lng').value = data?.Longitude ?? ''
    document.getElementById('am-category').value = data?.CategoryID ?? ''
    document.getElementById('am-subdivision').value = data?.SubdivisionID ?? ''
    openModal('amenityModal')
}

async function saveAmenity() {
    const name = document.getElementById('am-name').value.trim()
    if (!name) { toast('Name is required', 'err'); return }

    const categoryId = parseInt(document.getElementById('am-category').value)
    const matchedCategory = categories.find(category => category.CategoryID === categoryId)

    const payload = {
        AmenityID: amenityEditId ?? 0,
        Name: name,
        CategoryID: categoryId || 0,
        CategoryName: matchedCategory?.CategoryName ?? '',
        Street: document.getElementById('am-street').value.trim(),
        City: document.getElementById('am-city').value.trim(),
        SubdivisionID: parseInt(document.getElementById('am-subdivision').value) || 0,
        Latitude: parseFloat(document.getElementById('am-lat').value) || null,
        Longitude: parseFloat(document.getElementById('am-lng').value) || null,
        GeometryType: 'POINT',
        LocationWKT: ''
    }

    if (amenityEditId) {
        await api('PUT', '/api/amenities', payload)
        const index = amenityData.findIndex(row => row.AmenityID === amenityEditId)
        if (index !== -1) amenityData[index] = payload
        toast('Amenity updated')
    } else {
        const created = await api('POST', '/api/amenities', payload)
        amenityData.push(created ?? payload)
        toast('Amenity created')
    }

    refreshAmenityGrid()
    closeModal('amenityModal')
}

let locationData = []
let locationEditId = null
let locationGrid

const locationColDefs = [
    { headerCheckboxSelection: true, checkboxSelection: true, width: 44, pinned: 'left', resizable: false, sortable: false, suppressMenu: true },
    { field: 'LocationID', headerName: '#', width: 70, sortable: true },
    { field: 'LocationName', headerName: 'Name', flex: 1.5, sortable: true, editable: true },
    { field: 'StreetNumber', headerName: 'No.', width: 70, sortable: true },
    { field: 'Street', headerName: 'Street', flex: 1, sortable: true, editable: true },
    { field: 'City', headerName: 'City', width: 110, sortable: true, editable: true },
    {
        field: 'CalculatedScore', headerName: 'Score', width: 110, sortable: true,
        cellRenderer: params => {
            const score = params.value ?? 0
            const cssClass = score >= 80 ? 'badge-green' : score >= 60 ? 'badge-amber' : 'badge-red'
            return `<span class="badge ${cssClass}">${score.toFixed(1)}</span>`
        }
    },
    { field: 'Latitude', headerName: 'Lat', width: 100, sortable: true, valueFormatter: params => params.value?.toFixed(5) ?? '—' },
    { field: 'Longitude', headerName: 'Lng', width: 110, sortable: true, valueFormatter: params => params.value?.toFixed(5) ?? '—' },
    {
        headerName: 'Actions', width: 140, pinned: 'right', sortable: false, editable: false, suppressMenu: true,
        cellRenderer: params => {
            const wrapper = document.createElement('div')
            wrapper.className = 'cell-acts'

            const editBtn = document.createElement('button')
            editBtn.className = 'cell-btn cell-btn-edit'
            editBtn.textContent = 'Edit'
            editBtn.onclick = () => openLocationModal(params.data)

            const deleteBtn = document.createElement('button')
            deleteBtn.className = 'cell-btn cell-btn-del'
            deleteBtn.textContent = 'Delete'
            deleteBtn.onclick = async () => {
                await api('DELETE', `/api/locations/${params.data.LocationID}`)
                locationData = locationData.filter(row => row.LocationID !== params.data.LocationID)
                refreshLocationGrid()
                toast('Location deleted', 'err')
            }

            wrapper.appendChild(editBtn)
            wrapper.appendChild(deleteBtn)
            return wrapper
        }
    }
]

async function initLocationGrid() {
    const fetchedLocations = await api('GET', '/api/locations')
    locationData = fetchedLocations ?? []

    locationGrid = agGrid.createGrid(document.getElementById('locationGrid'), {
        columnDefs: locationColDefs,
        rowData: locationData,
        rowSelection: 'multiple',
        animateRows: true,
        stopEditingWhenCellsLoseFocus: true,
        defaultColDef: { resizable: true },
        onSelectionChanged() {
            document.getElementById('locDelSelBtn').disabled = locationGrid.getSelectedRows().length === 0
        },
        onCellValueChanged(event) {
            const index = locationData.findIndex(row => row.LocationID === event.data.LocationID)
            if (index !== -1) locationData[index] = { ...event.data }
            api('PUT', '/api/locations', event.data)
            toast(`${event.colDef.headerName} updated`)
        },
        onModelUpdated() { updateLocationStats() }
    })

    updateLocationStats()
}

function refreshLocationGrid() {
    locationGrid.setGridOption('rowData', locationData)
    updateLocationStats()
}

function updateLocationStats() {
    if (!locationGrid) return
    document.getElementById('stat-loc-total').textContent = locationData.length
    const average = locationData.length
        ? (locationData.reduce((sum, row) => sum + (row.CalculatedScore || 0), 0) / locationData.length).toFixed(1)
        : '—'
    document.getElementById('stat-loc-avg').textContent = average
    document.getElementById('stat-loc-filtered').textContent = locationGrid.getDisplayedRowCount()
}

async function deleteSelectedLocations() {
    const selectedIds = locationGrid.getSelectedRows().map(row => row.LocationID)
    await Promise.all(selectedIds.map(id => api('DELETE', `/api/locations/${id}`)))
    locationData = locationData.filter(row => !selectedIds.includes(row.LocationID))
    refreshLocationGrid()
    toast(`${selectedIds.length} location(s) deleted`, 'err')
}

function openLocationModal(data = null) {
    locationEditId = data ? data.LocationID : null
    document.getElementById('locationModalTitle').textContent = data ? 'Edit Location' : 'Add Location'
    document.getElementById('lm-name').value = data?.LocationName ?? ''
    document.getElementById('lm-streetnum').value = data?.StreetNumber ?? ''
    document.getElementById('lm-street').value = data?.Street ?? ''
    document.getElementById('lm-city').value = data?.City ?? ''
    document.getElementById('lm-lat').value = data?.Latitude ?? ''
    document.getElementById('lm-lng').value = data?.Longitude ?? ''
    document.getElementById('lm-score').value = data?.CalculatedScore ?? ''
    document.getElementById('lm-subdivision').value = data?.SubdivisionID ?? ''
    openModal('locationModal')
}

async function saveLocation() {
    const name = document.getElementById('lm-name').value.trim()
    if (!name) { toast('Location name is required', 'err'); return }

    const payload = {
        LocationID: locationEditId ?? 0,
        LocationName: name,
        StreetNumber: document.getElementById('lm-streetnum').value.trim(),
        Street: document.getElementById('lm-street').value.trim(),
        City: document.getElementById('lm-city').value.trim(),
        SubdivisionID: parseInt(document.getElementById('lm-subdivision').value) || 1,
        Latitude: parseFloat(document.getElementById('lm-lat').value) || null,
        Longitude: parseFloat(document.getElementById('lm-lng').value) || null,
        CalculatedScore: parseFloat(document.getElementById('lm-score').value) || 0,
        GeometryType: 'POINT',
        LocationWKT: ''
    }

    if (locationEditId) {
        await api('PUT', '/api/locations', payload)
        const index = locationData.findIndex(row => row.LocationID === locationEditId)
        if (index !== -1) locationData[index] = payload
        toast('Location updated')
    } else {
        const created = await api('POST', '/api/locations', payload)
        locationData.push(created ?? payload)
        toast('Location created')
    }

    refreshLocationGrid()
    closeModal('locationModal')
}

let battleData = []
let selectedBattleCode = null
let battleGrid, leaderboardGrid

const battleColDefs = [
    { field: 'BattleID', headerName: '#', width: 70, sortable: true },
    {
        field: 'BattleCode', headerName: 'Code', flex: 2, sortable: false,
        cellRenderer: params => `<span class="code-pill">${params.value.slice(0, 8)}…</span>`
    },
    {
        field: 'Status', headerName: 'Status', width: 110, sortable: true,
        cellRenderer: params => {
            const statusMap = { open: ['badge-green', 'Open'], closed: ['badge-red', 'Closed'] }
            const [cssClass, label] = statusMap[params.value] ?? ['badge-blue', params.value]
            return `<span class="badge ${cssClass}"><span class="badge-dot" style="background:currentColor"></span>${label}</span>`
        }
    },
    {
        field: 'ExpiresAt', headerName: 'Expires', flex: 1, sortable: true,
        valueFormatter: params => {
            if (!params.value) return '—'
            const date = new Date(params.value)
            return date.toLocaleDateString('en-CA', { month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })
        }
    },
    {
        headerName: 'Leaderboard', width: 130, sortable: false, suppressMenu: true,
        cellRenderer: params => {
            const viewBtn = document.createElement('button')
            viewBtn.className = 'cell-btn cell-btn-edit'
            viewBtn.textContent = '📊 View'
            viewBtn.onclick = () => loadLeaderboard(params.data.BattleCode, params.data.BattleID)
            return viewBtn
        }
    }
]

const leaderboardColDefs = [
    {
        field: 'UserID', headerName: 'Rank', width: 80, sortable: false,
        cellRenderer: params => {
            const rankClasses = ['gold', 'silver', 'bronze']
            const cssClass = rankClasses[params.rowIndex] ?? ''
            return `<div class="rank-cell"><span class="rank-num ${cssClass}">${params.rowIndex + 1}</span></div>`
        }
    },
    { field: 'DisplayName', headerName: 'Player', flex: 1, sortable: true },
    { field: 'LocationName', headerName: 'Location', flex: 1.5, sortable: true },
    {
        field: 'Score', headerName: 'Score', width: 120, sortable: true,
        cellRenderer: params => {
            const score = params.value ?? 0
            const barWidth = Math.round(score)
            return `<div style="display:flex;align-items:center;gap:8px;height:100%">
        <div style="flex:1;height:5px;background:#e5e7eb;border-radius:3px;overflow:hidden">
          <div style="width:${barWidth}%;height:100%;background:var(--green);border-radius:3px;"></div>
        </div>
        <span style="font-family:var(--mono);font-size:12px;font-weight:600;min-width:36px;">${score.toFixed(1)}</span>
      </div>`
        }
    },
    { field: 'Latitude', headerName: 'Lat', width: 95, sortable: true, valueFormatter: params => params.value?.toFixed(5) ?? '—' },
    { field: 'Longitude', headerName: 'Lng', width: 105, sortable: true, valueFormatter: params => params.value?.toFixed(5) ?? '—' }
]

async function initBattleGrids() {
    const fetchedBattles = await api('GET', '/api/battles')
    battleData = fetchedBattles ?? []

    battleGrid = agGrid.createGrid(document.getElementById('battleGrid'), {
        columnDefs: battleColDefs,
        rowData: battleData,
        animateRows: true,
        defaultColDef: { resizable: true },
        onModelUpdated() { updateBattleStats() }
    })

    leaderboardGrid = agGrid.createGrid(document.getElementById('leaderboardGrid'), {
        columnDefs: leaderboardColDefs,
        rowData: [],
        animateRows: true,
        defaultColDef: { resizable: true },
        rowClassRules: {
            'ag-row-selected': params => params.rowIndex === 0
        }
    })

    updateBattleStats()
}

async function refreshBattleGrid() {
    if (!battleGrid) { await initBattleGrids(); return }
    const fetchedBattles = await api('GET', '/api/battles')
    battleData = fetchedBattles ?? []
    battleGrid.setGridOption('rowData', battleData)
    updateBattleStats()
}

function updateBattleStats() {
    const now = new Date()
    document.getElementById('stat-battle-total').textContent = battleData.length
    document.getElementById('stat-battle-active').textContent = battleData.filter(battle => battle.Status === 'open' && new Date(battle.ExpiresAt) > now).length
}

async function loadLeaderboard(code, id) {
    selectedBattleCode = code
    document.getElementById('leaderboardTitle').textContent = `Leaderboard — Battle #${id}`
    document.getElementById('leaderboardRefresh').disabled = false

    const leaderboard = await api('GET', `/api/battles/${code}/leaderboard`)
    leaderboardGrid.setGridOption('rowData', leaderboard ?? [])
    document.getElementById('stat-battle-participants').textContent = (leaderboard ?? []).length
    toast(`Leaderboard loaded for Battle #${id}`, 'info')
}

function refreshLeaderboard() {
    if (selectedBattleCode) loadLeaderboard(selectedBattleCode, '?')
}

function openCreateBattleModal() {
    openModal('battleModal')
}

async function createBattle() {
    const userId = parseInt(document.getElementById('bm-userid').value)
    if (!userId) { toast('User ID is required', 'err'); return }

    const result = await api('POST', '/api/battles', userId)
    const battle = result?.battle

    if (battle) {
        battleData.push(battle)
        battleGrid.setGridOption('rowData', battleData)
        updateBattleStats()
        toast(`Battle created — code: ${battle.BattleCode.slice(0, 8)}…`)
        closeModal('battleModal')
    }
}

function populateSelects() {
    const categorySelect = document.getElementById('am-category')
    categorySelect.innerHTML = '<option value="">— Select —</option>'
    categories.forEach(category => {
        const option = document.createElement('option')
        option.value = category.CategoryID
        option.textContent = category.CategoryName
        categorySelect.appendChild(option)
    })

    const amenitySubdivisionSelect = document.getElementById('am-subdivision')
    amenitySubdivisionSelect.innerHTML = '<option value="">— Select —</option>'
    subdivisions.forEach(subdivision => {
        const option = document.createElement('option')
        option.value = subdivision.SubdivisionID
        option.textContent = `${subdivision.SubdivisionCode} — ${subdivision.SubdivisionName}`
        amenitySubdivisionSelect.appendChild(option)
    })

    const locationSubdivisionSelect = document.getElementById('lm-subdivision')
    locationSubdivisionSelect.innerHTML = '<option value="">— Select —</option>'
    subdivisions.forEach(subdivision => {
        const option = document.createElement('option')
        option.value = subdivision.SubdivisionID
        option.textContent = `${subdivision.SubdivisionCode} — ${subdivision.SubdivisionName}`
        locationSubdivisionSelect.appendChild(option)
    })
}

window.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.backdrop').forEach(backdrop => {
        backdrop.addEventListener('click', event => {
            if (event.target === backdrop) backdrop.classList.remove('open')
        })
    })
    initAmenityGrid()
    initLocationGrid()
})