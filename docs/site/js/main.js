// ALX Documentation — Navigation
document.addEventListener('DOMContentLoaded', function() {
    // Highlight current page in nav
    const currentPath = window.location.pathname.split('/').pop() || 'index.html';
    document.querySelectorAll('.nav-links a').forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });

    // Highlight current section in sidebar
    const headings = document.querySelectorAll('.content h2[id]');
    if (headings.length > 0) {
        const observer = new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const id = entry.target.getAttribute('id');
                    document.querySelectorAll('.sidebar a').forEach(a => {
                        a.classList.toggle('active', a.getAttribute('href') === '#' + id);
                    });
                }
            });
        }, { rootMargin: '-80px 0px -80% 0px' });

        headings.forEach(h => observer.observe(h));
    }
});
