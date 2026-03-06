globalThis.BlazorHelper = new class {

    constructor() {
        if (this.isDarkModePreferred()) {
            this.setTheme(true);
        }
    }

    async saveFileAsync(streamRef, fileName) {
        const arrayBuffer = await streamRef.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.click();
        anchorElement.remove();
        URL.revokeObjectURL(url);
    }

    setTheme(isDark) {
        document.body.setAttribute("data-bs-theme", isDark ? "dark" : "light");
    }

    isDarkModePreferred() {
        return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    }

}();