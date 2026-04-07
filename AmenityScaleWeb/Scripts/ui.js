import Gauge from 'https://esm.sh/svg-gauge'
import { AMENITIES } from './constants.js'
import { redrawMarkers } from './map.js'

const btn = document.getElementById('hammy-btn')
const menu = document.getElementById('menu-container')
const navIcon = document.getElementById('nav-hamburger')
const map = document.getElementById('map')
const loadingEl = document.getElementById('loading')
const scorePopupEl = document.getElementById('score-popup')
const closeScoreEl = document.getElementById('close-score')
let isOpen = false
let showScore = false

export let locationSelected = false
export function setLocationSelected(val) { locationSelected = val }


btn.addEventListener('click', () => {
    isOpen = !isOpen
    menu.classList.toggle('open', isOpen)
    btn.classList.toggle('open', isOpen)
    navIcon.classList.toggle('open', isOpen)
})

export function isLoadingFn() {
    map.classList.add('loading')
    loadingEl.classList.add('loading')
    gauge3.setValueAnimated(0, 0.3)
}
export function endLoadingFn() {
    map.classList.remove('loading')
    loadingEl.classList.remove('loading')
}

export function showScoreFn(score) {
    showScore = true
    scorePopupEl.classList.toggle('showScore', showScore)
    setTimeout(() => {
        setGaugeValue(score, 2)
    }, 1)
}

closeScoreEl.addEventListener('click', () => {
    dismissScore()
})
export function dismissScore() {
    showScore = false
    scorePopupEl.classList.toggle('showScore', showScore)
    gauge2.setValueAnimated(0, 1)
}

export let gauge2 = Gauge(document.getElementById('gauge2'), {
    min: 0,
    max: 100,
    dialStartAngle: 180,
    dialEndAngle: 0,
    value: 0,
    color: function (value) {
        return '#006cb5'
    },
})

export let gauge3 = Gauge(document.getElementById('gauge3'), {
    min: 0,
    max: 100,
    dialStartAngle: 180,
    dialEndAngle: 0,
    value: 0,
    color: function (value) {
        return '#006cb5'
    },
})

export function setGaugeValue(value = 0, duration = 2) {
    gauge2.setValueAnimated(value, duration)
    gauge3.setValueAnimated(value, duration)
}


export const amenityState = {};
Object.keys(AMENITIES).forEach(k => amenityState[k] = true);

const grid = document.getElementById('amenity-grid');

function renderAmenityPanel() {
    grid.innerHTML = '';
    Object.entries(AMENITIES).forEach(([key, label]) => {
        const el = document.createElement('div');
        el.classList.add('amenity-item');
        if (amenityState[key]) el.classList.add('active');
        el.textContent = label;
        el.title = label;
        el.addEventListener('click', () => {
            amenityState[key] = !amenityState[key];
            el.classList.toggle('active', amenityState[key]);
            redrawMarkers()
        });
        grid.appendChild(el);
    });
}

document.getElementById('show-all').addEventListener('click', () => {
    Object.keys(amenityState).forEach(k => amenityState[k] = true);
    renderAmenityPanel();
    redrawMarkers()
});

document.getElementById('hide-all').addEventListener('click', () => {
    Object.keys(amenityState).forEach(k => amenityState[k] = false);
    renderAmenityPanel();
    redrawMarkers()
});


renderAmenityPanel();