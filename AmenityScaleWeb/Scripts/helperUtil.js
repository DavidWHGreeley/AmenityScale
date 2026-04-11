
// I should really debounce the map clicks.... 
// TODO: Greeley debouce the mouse clicks....
export function debounce(func, timeout = 300) {
    let timer;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => { func.apply(this, args); }, timeout);
    };
}