let clickOutsideHandler = null;

export function addClickListener(rootElement, dotNetHelper) {
    if (clickOutsideHandler) removeClickListener();

    clickOutsideHandler = (event) => {
        if (!rootElement.contains(event.target)) {
            dotNetHelper.invokeMethodAsync('CloseDropdown');
        }
    };

    document.addEventListener('click', clickOutsideHandler);
}

export function removeClickListener() {
    if (clickOutsideHandler) {
        document.removeEventListener('click', clickOutsideHandler);
        clickOutsideHandler = null;
    }
}
