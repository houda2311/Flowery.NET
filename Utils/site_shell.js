// --- Theme Logic ---
const themeToggle = document.querySelector('.theme-toggle');
const sunIcon = document.querySelector('.sun-icon');
const moonIcon = document.querySelector('.moon-icon');
const iframe = document.querySelector('iframe');

// Check local storage or default to dark
const currentTheme = localStorage.getItem('theme') || 'dark';
applyTheme(currentTheme);

function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);

    // Update icons
    if (theme === 'dark') {
        sunIcon.style.display = 'block';
        moonIcon.style.display = 'none';
    } else {
        sunIcon.style.display = 'none';
        moonIcon.style.display = 'block';
    }

    // Sync iframe (Direct access + PostMessage fallback for local files)
    try {
        // 1. Try direct access (works for same origin)
        if (iframe.contentDocument && iframe.contentDocument.documentElement) {
            iframe.contentDocument.documentElement.setAttribute('data-theme', theme);
        }
    } catch(e) {
        // console.log('Direct access restricted');
    }

    // 2. PostMessage (works for cross-origin/local files)
    try {
        if (iframe.contentWindow) {
            iframe.contentWindow.postMessage({ type: 'setTheme', theme: theme }, '*');
        }
    } catch(e) {}
}

themeToggle.addEventListener('click', () => {
    const current = document.documentElement.getAttribute('data-theme');
    const newTheme = current === 'dark' ? 'light' : 'dark';
    localStorage.setItem('theme', newTheme);
    applyTheme(newTheme);
});

// When iframe loads, ensure it gets the theme
iframe.addEventListener('load', () => {
    const theme = localStorage.getItem('theme') || 'dark';
    applyTheme(theme);
});

// --- Sidebar Logic ---
const sidebar = document.querySelector('.sidebar');
const overlay = document.querySelector('.overlay');
const toggleBtn = document.querySelector('.menu-toggle');
const links = document.querySelectorAll('.sidebar a');

function toggleMenu() {
    sidebar.classList.toggle('open');
    overlay.classList.toggle('open');
}

function closeMenu() {
    sidebar.classList.remove('open');
    overlay.classList.remove('open');
}

toggleBtn.addEventListener('click', toggleMenu);
overlay.addEventListener('click', closeMenu);

// Handle active state & mobile close
links.forEach(link => {
    link.addEventListener('click', (e) => {
        // Don't mess with external links
        if (link.target === '_blank') return;

        links.forEach(l => l.classList.remove('active'));
        link.classList.add('active');

        // Close menu on mobile when link is clicked
        if (window.innerWidth <= 768) {
            closeMenu();
        }
    });
});
