// Syntax highlighting
hljs.highlightAll();

// Theme Sync Logic for Iframe Content
function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
}

// 1. Initial Load: Try to get theme from localStorage
const savedTheme = localStorage.getItem('theme') || 'dark';
applyTheme(savedTheme);

// 2. Listen for messages from parent (shell)
window.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'setTheme') {
        applyTheme(event.data.theme);
    }
});

