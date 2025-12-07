#!/usr/bin/env python3
"""
Flowery.NET Static Site Generator

Converts markdown documentation into a static HTML website for GitHub Pages.

Usage:
    python Utils/generate_site.py                # Use curated llms-static/ only (default)
    python Utils/generate_site.py --use-generated # Use llms/ (auto-generated) docs

Input (markdown):
    Default mode (curated):
        llms-static/*.md         - Curated per-control docs
        llms/categories/*.md     - Category docs

    With --use-generated:
        llms/llms.txt            - Main overview
        llms/controls/*.md       - Per-control docs (auto-generated)
        llms/categories/*.md     - Category docs

Output (HTML):
    docs/index.html          - Main landing page
    docs/controls/*.html     - Per-control pages
    docs/categories/*.html   - Category pages
    docs/style.css           - Stylesheet
    docs/llms.txt            - Machine-readable docs for AI assistants

GitHub Pages Setup:
    1. Push the docs/ folder to your repo
    2. Go to Settings → Pages
    3. Source: "Deploy from a branch"
    4. Branch: main (or master), folder: /docs
    5. Save - site will be live in ~1 minute
"""

import argparse
import re
import shutil
from pathlib import Path
from typing import Optional


class MarkdownToHtml:
    """Simple markdown to HTML converter."""

    def convert(self, markdown: str) -> str:
        """Convert markdown to HTML."""
        html = markdown

        # Code blocks - extract and replace with placeholders
        code_blocks = []
        def save_code_block(m):
            lang = m.group(1) or "text"
            code = self._escape_html(self._clean_code_block(m.group(2)))
            code_blocks.append(f'<pre><code class="language-{lang}">{code}</code></pre>')
            return f'__CODE_BLOCK_{len(code_blocks) - 1}__'

        html = re.sub(r'```(\w+)?\n(.*?)```', save_code_block, html, flags=re.DOTALL)

        # Inline code
        html = re.sub(r'`([^`]+)`', r'<code>\1</code>', html)

        # Images - convert ![alt](src) to <img> and fix paths for controls/ subfolder
        def convert_image(m):
            alt = m.group(1)
            src = m.group(2)
            # If src is a local file (not http), prepend ../ for controls/ pages
            if not src.startswith(('http://', 'https://', '/')):
                src = '../' + src
            return f'<img src="{src}" alt="{alt}" style="max-width:100%;height:auto;">'
        html = re.sub(r'!\[([^\]]*)\]\(([^)]+)\)', convert_image, html)

        # Links - convert [text](url) to <a>
        html = re.sub(r'\[([^\]]+)\]\(([^)]+)\)', r'<a href="\2">\1</a>', html)

        # Headers
        html = re.sub(r'^### (.+)$', r'<h3>\1</h3>', html, flags=re.MULTILINE)
        html = re.sub(r'^## (.+)$', r'<h2>\1</h2>', html, flags=re.MULTILINE)
        html = re.sub(r'^# (.+)$', r'<h1>\1</h1>', html, flags=re.MULTILINE)

        # Bold and italic
        html = re.sub(r'\*\*(.+?)\*\*', r'<strong>\1</strong>', html)
        html = re.sub(r'\*(.+?)\*', r'<em>\1</em>', html)

        # Tables
        html = self._convert_tables(html)

        # Lists
        html = re.sub(r'^- (.+)$', r'<li>\1</li>', html, flags=re.MULTILINE)
        html = re.sub(r'(<li>.*</li>\n?)+', r'<ul>\g<0></ul>', html)

        # Paragraphs (lines not already wrapped and not placeholders)
        lines = html.split('\n')
        result = []
        for line in lines:
            stripped = line.strip()
            if stripped and not stripped.startswith('<') and not stripped.startswith('__CODE_BLOCK_'):
                result.append(f'<p>{stripped}</p>')
            else:
                result.append(line)
        html = '\n'.join(result)

        # Clean up empty paragraphs
        html = re.sub(r'<p>\s*</p>', '', html)

        # Restore code blocks
        for i, block in enumerate(code_blocks):
            html = html.replace(f'__CODE_BLOCK_{i}__', block)

        return html

    def _escape_html(self, text: str) -> str:
        """Escape HTML entities in code blocks."""
        return (text
                .replace('&', '&amp;')
                .replace('<', '&lt;')
                .replace('>', '&gt;'))

    def _clean_code_block(self, code: str) -> str:
        """Clean up code block content - remove excessive blank lines."""
        lines = code.split('\n')
        # Remove leading/trailing blank lines
        while lines and not lines[0].strip():
            lines.pop(0)
        while lines and not lines[-1].strip():
            lines.pop()
        # Collapse multiple consecutive blank lines into one
        result = []
        prev_blank = False
        for line in lines:
            is_blank = not line.strip()
            if is_blank:
                if not prev_blank:
                    result.append('')
                prev_blank = True
            else:
                result.append(line)
                prev_blank = False
        return '\n'.join(result)

    def _convert_tables(self, html: str) -> str:
        """Convert markdown tables to HTML."""
        lines = html.split('\n')
        result = []
        in_table = False
        table_lines = []

        for line in lines:
            if '|' in line and line.strip().startswith('|'):
                if not in_table:
                    in_table = True
                    table_lines = []
                table_lines.append(line)
            else:
                if in_table:
                    result.append(self._build_table(table_lines))
                    in_table = False
                    table_lines = []
                result.append(line)

        if in_table:
            result.append(self._build_table(table_lines))

        return '\n'.join(result)

    def _build_table(self, lines: list[str]) -> str:
        """Build HTML table from markdown table lines."""
        if len(lines) < 2:
            return '\n'.join(lines)

        html = ['<div class="table-wrapper"><table>']

        # Header row
        header_cells = [c.strip() for c in lines[0].split('|')[1:-1]]
        html.append('<thead><tr>')
        for cell in header_cells:
            html.append(f'<th>{cell}</th>')
        html.append('</tr></thead>')

        # Body rows (skip separator line)
        html.append('<tbody>')
        for line in lines[2:]:
            cells = [c.strip() for c in line.split('|')[1:-1]]
            html.append('<tr>')
            for cell in cells:
                # Convert inline code in cells
                cell = re.sub(r'`([^`]+)`', r'<code>\1</code>', cell)
                html.append(f'<td>{cell}</td>')
            html.append('</tr>')
        html.append('</tbody>')

        html.append('</table></div>')
        return '\n'.join(html)


class SiteGenerator:
    """Generates static HTML site from markdown docs."""

    def __init__(self, docs_dir: Path, output_dir: Path, curated_dir: Path | None = None):
        self.docs_dir = docs_dir
        self.output_dir = output_dir
        self.curated_dir = curated_dir  # llms-static/ for curated-only mode
        self.converter = MarkdownToHtml()
        self.controls: list[dict] = []
        self.categories: list[dict] = []
        self.use_curated_only = curated_dir is not None

    def generate(self):
        """Generate the complete static site."""
        print("Flowery.NET Site Generator")
        print("=" * 40)
        if self.use_curated_only:
            print("Mode: CURATED ONLY (llms-static/)")
        else:
            print("Mode: GENERATED (llms/)")

        # Create output directories
        self.output_dir.mkdir(exist_ok=True)
        (self.output_dir / "controls").mkdir(exist_ok=True)
        (self.output_dir / "categories").mkdir(exist_ok=True)

        # Collect all controls
        print("\n[1/4] Scanning control docs...")
        seen_controls = set()
        # Helper/internal classes shown in a separate section
        helper_control_names = {
            'DaisyPaginationItem',  # Part of DaisyPagination
            'DaisyAccessibility',   # Accessibility utilities
        }

        if self.use_curated_only:
            # First, read curated docs from llms-static/
            for md_file in sorted(self.curated_dir.glob("Daisy*.md")):
                name = md_file.stem
                self.controls.append({
                    'name': name,
                    'file': md_file,
                    'html_name': f"{name}.html",
                    'is_helper': name in helper_control_names
                })
                seen_controls.add(name)

            # Then, also include auto-generated docs from llms/controls/ for controls
            # that don't have curated docs (e.g., weather controls, custom controls)
            controls_dir = self.docs_dir / "controls"
            if controls_dir.exists():
                for md_file in sorted(controls_dir.glob("*.md")):
                    name = md_file.stem
                    if name.startswith("Daisy") and name not in seen_controls:
                        entry = {
                            'name': name,
                            'file': md_file,
                            'html_name': f"{name}.html",
                            'is_helper': name in helper_control_names
                        }
                        self.controls.append(entry)
                        seen_controls.add(name)
        else:
            # Read from llms/controls/
            controls_dir = self.docs_dir / "controls"
            for md_file in sorted(controls_dir.glob("*.md")):
                name = md_file.stem
                if name.startswith("Daisy"):
                    self.controls.append({
                        'name': name,
                        'file': md_file,
                        'html_name': f"{name}.html",
                        'is_helper': name in helper_control_names
                    })

        # Sort all controls alphabetically by name
        self.controls.sort(key=lambda c: c['name'])
        print(f"      Found {len(self.controls)} controls")

        # Collect categories (always from llms/categories/)
        print("\n[2/4] Scanning category docs...")
        categories_dir = self.docs_dir / "categories"
        if categories_dir.exists():
            for md_file in sorted(categories_dir.glob("*.md")):
                self.categories.append({
                    'name': md_file.stem.replace('-', ' ').title(),
                    'file': md_file,
                    'html_name': f"{md_file.stem}.html"
                })
            print(f"      Found {len(self.categories)} categories")
        else:
            print("      No categories folder found (run generate_docs.py first)")

        # Copy images from llms-static/ to docs/
        print("\n[3/5] Copying images...")
        self._copy_images()

        # Generate CSS
        print("\n[4/5] Generating stylesheet...")
        self._write_css()

        # Generate HTML pages
        print("\n[5/5] Generating HTML pages...")
        self._generate_shell()
        self._generate_home()
        self._generate_control_pages()
        self._generate_category_pages()

        print("\n" + "=" * 40)
        print("Site generated successfully!")
        print(f"Output: {self.output_dir}")
        print(f"Open:   {self.output_dir / 'index.html'}")

    def _copy_images(self):
        """Copy image files from llms-static/ to docs/ for HTML pages."""
        if not self.curated_dir:
            return
        image_extensions = ['*.gif', '*.png', '*.jpg', '*.jpeg', '*.webp', '*.svg']
        copied = 0
        for ext in image_extensions:
            for img_file in self.curated_dir.glob(ext):
                dest = self.output_dir / img_file.name
                shutil.copy2(img_file, dest)
                copied += 1
        print(f"      Copied {copied} image(s)")

    def _write_css(self):
        """Write the stylesheet (read from external template file)."""
        css_template = Path(__file__).parent / "site_template.css"
        css = css_template.read_text(encoding='utf-8')
        (self.output_dir / "style.css").write_text(css, encoding='utf-8')

    def _page_template(self, title: str, content: str, depth: int = 0) -> str:
        """Generate HTML page for content (loaded in iframe)."""
        css_prefix = "../" * depth
        content_js = (Path(__file__).parent / "site_content.js").read_text(encoding='utf-8')
        return f'''<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{title}</title>
    <link rel="stylesheet" href="{css_prefix}style.css">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github-dark.min.css">
    <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/xml.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/csharp.min.js"></script>
</head>
<body class="content-body">
    {content}
    <script>
{content_js}
    </script>
</body>
</html>'''

    def _generate_shell(self):
        """Generate the main app shell (index.html) with sidebar and iframe."""
        shell_js = (Path(__file__).parent / "site_shell.js").read_text(encoding='utf-8')

        sidebar_items = []

        # Home link
        sidebar_items.append('<li><a href="home.html" target="viewer" class="active">← Home</a></li>')

        # Categories
        if self.categories:
            sidebar_items.append('<li><h2>Categories</h2></li>')
            for cat in self.categories:
                sidebar_items.append(f'<li><a href="categories/{cat["html_name"]}" target="viewer">{cat["name"]}</a></li>')

        # Controls (main controls only, not helpers)
        sidebar_items.append('<li><h2>Controls</h2></li>')
        # Custom controls (Avalonia-specific, not in original DaisyUI)
        custom_prefixes = ('Color', 'Weather', 'ModifierKeys', 'ComponentSidebar')
        main_controls = [c for c in self.controls if not c.get('is_helper', False)]
        helper_controls = [c for c in self.controls if c.get('is_helper', False)]

        for ctrl in main_controls:
            display_name = ctrl['name'].replace('Daisy', '')
            is_custom = display_name.startswith(custom_prefixes)
            badge = '<sup class="custom-badge">✦</sup>' if is_custom else ''
            sidebar_items.append(f'<li><a href="controls/{ctrl["html_name"]}" target="viewer">{display_name}{badge}</a></li>')

        # Helpers section (if any)
        if helper_controls:
            sidebar_items.append('<li><h2 class="helpers-header">Helpers</h2></li>')
            for ctrl in helper_controls:
                display_name = ctrl['name'].replace('Daisy', '')
                sidebar_items.append(f'<li><a href="controls/{ctrl["html_name"]}" target="viewer">{display_name}</a></li>')

        sidebar_html = '\n'.join(sidebar_items)

        html = f'''<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Flowery.NET Documentation</title>
    <link rel="stylesheet" href="style.css">
</head>
<body>
    <div class="shell">
        <div class="overlay"></div>
        <button class="menu-toggle" aria-label="Toggle Menu">
            <svg viewBox="0 0 24 24" width="24" height="24" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round">
                <line x1="3" y1="12" x2="21" y2="12"></line>
                <line x1="3" y1="6" x2="21" y2="6"></line>
                <line x1="3" y1="18" x2="21" y2="18"></line>
            </svg>
        </button>

        <nav class="sidebar">
            <h1>
                <div class="brand">
                    <a href="https://github.com/tobitege/Flowery.NET" target="_blank" rel="noopener" class="github-link" title="View on GitHub">
                        <svg class="github-icon" viewBox="0 0 16 16" width="20" height="20">
                            <path fill="currentColor" d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0016 8c0-4.42-3.58-8-8-8z"/>
                        </svg>
                    </a>
                    <span>Flowery.NET</span>
                </div>
                <button class="theme-toggle" aria-label="Toggle Theme" title="Toggle Theme">
                    <!-- Sun Icon (for Dark mode) -->
                    <svg class="sun-icon" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="display: none;">
                        <circle cx="12" cy="12" r="5"></circle>
                        <line x1="12" y1="1" x2="12" y2="3"></line>
                        <line x1="12" y1="21" x2="12" y2="23"></line>
                        <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line>
                        <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line>
                        <line x1="1" y1="12" x2="3" y2="12"></line>
                        <line x1="21" y1="12" x2="23" y2="12"></line>
                        <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line>
                        <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line>
                    </svg>
                    <!-- Moon Icon (for Light mode) -->
                    <svg class="moon-icon" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>
                    </svg>
                </button>
            </h1>
            <p class="subtitle">Avalonia UI Components</p>
            <ul>
                {sidebar_html}
            </ul>
        </nav>
        <iframe name="viewer" class="viewer" src="home.html"></iframe>
    </div>

    <script>
{shell_js}
    </script>
</body>
</html>'''
        (self.output_dir / "index.html").write_text(html, encoding='utf-8')

    def _generate_home(self):
        """Generate the home content page (home.html)."""
        # Generate llms.txt for AI assistants (combine all curated docs)
        if self.use_curated_only:
            llms_content = self._generate_llms_txt_from_curated()
        else:
            llms_content = (self.docs_dir / "llms.txt").read_text(encoding='utf-8')

        # Write llms.txt to output directory for AI assistants
        (self.output_dir / "llms.txt").write_text(llms_content, encoding='utf-8')

        # Convert to HTML
        html_content = self.converter.convert(llms_content)

        # Insert LLM documentation link after Quick Start section
        llm_link_html = '''<div class="llm-link">
    <h2>For AI Assistants</h2>
    <p>📄 <a href="llms.txt"><strong>llms.txt</strong></a> — Machine-readable documentation in plain markdown format, optimized for LLMs and AI code assistants.</p>
</div>
'''
        # Insert after Quick Start (after the first </pre> which closes the code block)
        if '</pre>' in html_content:
            insert_pos = html_content.find('</pre>') + len('</pre>')
            html_content = html_content[:insert_pos] + '\n' + llm_link_html + html_content[insert_pos:]

        # Add footer with project links
        footer_html = '''
<hr class="footer-separator">
<footer class="index-footer">
    <p class="disclaimer">This project is not affiliated with, endorsed by, or sponsored by DaisyUI or Avalonia UI.</p>
    <p>Built with inspiration from:</p>
    <div class="footer-links">
        <a href="https://daisyui.com" target="_blank" rel="noopener" title="DaisyUI">
            <svg class="footer-icon" viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M12 2C6.477 2 2 6.477 2 12s4.477 10 10 10 10-4.477 10-10S17.523 2 12 2zm0 18c-4.411 0-8-3.589-8-8s3.589-8 8-8 8 3.589 8 8-3.589 8-8 8zm-1-13h2v6h-2zm0 8h2v2h-2z"/></svg>
            <span>DaisyUI</span>
        </a>
        <a href="https://avaloniaui.net" target="_blank" rel="noopener" title="Avalonia UI">
            <svg class="footer-icon" viewBox="0 0 24 24" width="24" height="24"><path fill="currentColor" d="M12 2L2 19h20L12 2zm0 4l7 11H5l7-11z"/></svg>
            <span>Avalonia UI</span>
        </a>
        <a href="https://github.com/saadeghi/daisyui" target="_blank" rel="noopener" title="DaisyUI GitHub">
            <svg class="footer-icon" viewBox="0 0 16 16" width="20" height="20"><path fill="currentColor" d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0016 8c0-4.42-3.58-8-8-8z"/></svg>
            <span>daisyui</span>
        </a>
        <a href="https://github.com/AvaloniaUI/Avalonia" target="_blank" rel="noopener" title="Avalonia GitHub">
            <svg class="footer-icon" viewBox="0 0 16 16" width="20" height="20"><path fill="currentColor" d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0016 8c0-4.42-3.58-8-8-8z"/></svg>
            <span>Avalonia</span>
        </a>
    </div>
</footer>
'''

        full_content = html_content + footer_html
        page = self._page_template("Documentation", full_content, depth=0)
        (self.output_dir / "home.html").write_text(page, encoding='utf-8')

    def _generate_llms_txt_from_curated(self) -> str:
        """Generate a master llms.txt from curated docs."""
        lines = []
        lines.append("# Flowery.NET Component Library")
        lines.append("")
        lines.append("Flowery.NET is an Avalonia UI component library inspired by DaisyUI.")
        lines.append("It provides styled controls for building modern desktop applications.")
        lines.append("")
        lines.append("## Quick Start")
        lines.append("")
        lines.append("Add the namespace to your AXAML:")
        lines.append("```xml")
        lines.append('xmlns:controls="clr-namespace:Flowery.Controls;assembly=Flowery.NET"')
        lines.append("```")
        lines.append("")

        # Controls Overview (main controls only)
        lines.append("## Controls Overview")
        lines.append("")
        lines.append("| Control | Description |")
        lines.append("|---------|-------------|")

        # Custom controls (Avalonia-specific, not in original DaisyUI)
        custom_prefixes = ('Color', 'Weather', 'ModifierKeys', 'ComponentSidebar')
        main_controls = [c for c in self.controls if not c.get('is_helper', False)]
        helper_controls = [c for c in self.controls if c.get('is_helper', False)]

        for ctrl in sorted(main_controls, key=lambda c: c['name']):
            name = ctrl['name']
            display_name = name.replace('Daisy', '')
            is_custom = display_name.startswith(custom_prefixes)
            badge = ' <sup>✦</sup>' if is_custom else ''
            # Try to extract description from the markdown file
            desc = f"{display_name} control"
            try:
                content = ctrl['file'].read_text(encoding='utf-8')
                # Look for first paragraph after "# Overview" or first non-header line
                content_clean = re.sub(r'<!--.*?-->', '', content, flags=re.DOTALL)
                # Find first meaningful paragraph
                for line in content_clean.split('\n'):
                    line = line.strip()
                    if line and not line.startswith('#') and not line.startswith('|') and not line.startswith('-'):
                        desc = line
                        break
            except Exception:
                pass
            lines.append(f"| [{name}](controls/{name}.html){badge} | {desc} |")

        # Helpers section
        if helper_controls:
            lines.append("")
            lines.append("### Helper Classes")
            lines.append("")
            lines.append("| Class | Description |")
            lines.append("|-------|-------------|")
            for ctrl in sorted(helper_controls, key=lambda c: c['name']):
                name = ctrl['name']
                desc = f"{name.replace('Daisy', '')} helper"
                try:
                    content = ctrl['file'].read_text(encoding='utf-8')
                    content_clean = re.sub(r'<!--.*?-->', '', content, flags=re.DOTALL)
                    for line in content_clean.split('\n'):
                        line = line.strip()
                        if line and not line.startswith('#') and not line.startswith('|') and not line.startswith('-'):
                            desc = line
                            break
                except Exception:
                    pass
                lines.append(f"| [{name}](controls/{name}.html) | {desc} |")

        lines.append("")
        lines.append("## Common Patterns")
        lines.append("")
        lines.append("### Shared Enums")
        lines.append("")
        lines.append("**DaisyColor** - Theme colors used across many controls:")
        lines.append("```")
        lines.append("Default, Primary, Secondary, Accent, Neutral, Info, Success, Warning, Error")
        lines.append("```")
        lines.append("")
        lines.append("**DaisySize** - Size variants:")
        lines.append("```")
        lines.append("ExtraSmall, Small, Medium (default), Large, ExtraLarge")
        lines.append("```")
        lines.append("")
        lines.append("**DaisyPlacement** - Position options:")
        lines.append("```")
        lines.append("Top, Bottom, Start, End")
        lines.append("```")
        lines.append("")
        lines.append("### Control-Specific Enums")
        lines.append("")
        lines.append("- **DaisyButtonStyle**: `Default`, `Outline`, `Dash`, `Soft`")
        lines.append("- **DaisyButtonShape**: `Default`, `Wide`, `Block`, `Square`, `Circle`")
        lines.append("- **DaisyMaskVariant**: `Squircle`, `Heart`, `Hexagon`, `Circle`, `Square`, `Diamond`, `Triangle`")
        lines.append("- **DaisyAvatarShape**: `Square`, `Rounded`, `Circle`")
        lines.append("- **DaisyStatus**: `None`, `Online`, `Offline`")
        lines.append("- **DaisyAlertVariant**: `Info`, `Success`, `Warning`, `Error`")
        lines.append("- **DaisyLoadingVariant**: `Spinner`, `Dots`, `Ring`, `Ball`, `Bars`, `Infinity`, `Orbit`, `Snake`, `Pulse`, `Wave`, `Bounce`, `Matrix`, `Hourglass`, `Heartbeat`, `CursorBlink`, ...")
        lines.append("- **DaisyStatusIndicatorVariant**: `Default`, `Ping`, `Bounce`, `Pulse`, `Blink`, `Ripple`, `Heartbeat`, `Spin`, `Wave`, `Glow`, `Radar`, `Sonar`, `Beacon`, ...")
        lines.append("- **WeatherCondition**: `Sunny`, `PartlyCloudy`, `Cloudy`, `Overcast`, `Mist`, `Fog`, `LightRain`, `Rain`, `HeavyRain`, `Drizzle`, `Showers`, `Thunderstorm`, `LightSnow`, `Snow`, `HeavySnow`, `Sleet`, `FreezingRain`, `Hail`, `Windy`, `Clear`")
        lines.append("- **ColorSliderChannel**: `Red`, `Green`, `Blue`, `Alpha`, `Hue`, `Saturation`, `Lightness`")
        lines.append("")
        lines.append("### Theming")
        lines.append("")
        lines.append("Use `DaisyThemeManager` to switch themes:")
        lines.append("```csharp")
        lines.append('DaisyThemeManager.ApplyTheme("dracula");')
        lines.append("```")
        lines.append("")
        lines.append("**Light themes:** acid, autumn, bumblebee, cmyk, corporate, cupcake, emerald, fantasy, garden, lemonade, light, lofi, nord, pastel, retro, valentine, winter, wireframe")
        lines.append("")
        lines.append("**Dark themes:** aqua, black, business, coffee, cyberpunk, dark, dim, dracula, forest, halloween, luxury, night, sunset, synthwave")
        lines.append("")
        return '\n'.join(lines)

    def _generate_control_pages(self):
        """Generate HTML pages for each control."""
        # 1. Build map of control -> category
        control_category_map = {}
        category_controls_map = {} # cat_name -> list of controls

        # Parse categories to find which controls belong where
        for cat in self.categories:
            cat_content = cat['file'].read_text(encoding='utf-8')
            # Extract control names from list items
            # - **[DaisyButton](../controls/DaisyButton.html)**
            found_controls = re.findall(r'\*\*\[?(Daisy\w+)', cat_content)
            category_controls_map[cat['name']] = found_controls
            for ctrl_name in found_controls:
                control_category_map[ctrl_name] = cat

        for ctrl in self.controls:
            md_content = ctrl['file'].read_text(encoding='utf-8')
            # Strip HTML comments from curated docs
            md_content = re.sub(r'<!--.*?-->', '', md_content, flags=re.DOTALL)

            # Fix Headings: If it starts with "# Overview", demote it and add proper title
            stripped_content = md_content.strip()
            if stripped_content.startswith('# Overview'):
                # Replace the first occurrence
                md_content = md_content.replace('# Overview', f'# {ctrl["name"]}\n\n## Overview', 1)
            elif not stripped_content.startswith('# '):
                # If no H1 at all, add one
                md_content = f"# {ctrl['name']}\n\n{md_content}"

            html_content = self.converter.convert(md_content)

            # --- Navigation & Breadcrumbs ---
            nav_html = ""
            category = control_category_map.get(ctrl['name'])

            if category:
                # Breadcrumbs
                nav_html += f'''<div class="breadcrumbs">
    <a href="../home.html">Home</a> &gt;
    <a href="../categories/{category["html_name"]}">{category["name"]}</a>
</div>'''

                # Prev/Next
                siblings = category_controls_map.get(category['name'], [])
                try:
                    idx = siblings.index(ctrl['name'])
                    links = []

                    if idx > 0:
                        prev_name = siblings[idx-1]
                        links.append(f'<a href="{prev_name}.html" class="nav-prev">← {prev_name.replace("Daisy", "")}</a>')
                    else:
                         links.append('<span></span>') # Spacer

                    if idx < len(siblings) - 1:
                        next_name = siblings[idx+1]
                        links.append(f'<a href="{next_name}.html" class="nav-next">{next_name.replace("Daisy", "")} →</a>')
                    else:
                        links.append('<span></span>') # Spacer

                    if any(l != '<span></span>' for l in links):
                        nav_html += f'<div class="doc-nav">{"".join(links)}</div>'
                except ValueError:
                    pass # Control not found in its category list (shouldn't happen if map is built correct)

            # Inject nav at top (breadcrumbs) and bottom (prev/next)
            # Find the end of content to append bottom nav
            full_page_content = nav_html.split('<div class="doc-nav">')[0] + html_content # Breadcrumbs + Content
            if '<div class="doc-nav">' in nav_html:
                 full_page_content += nav_html.split('</div>')[-2] + '</div>' # Append doc-nav

            # Actually, let's keep it simple: Breadcrumbs top, Nav bottom
            breadcrumbs = f'''<div class="breadcrumbs">
    <a href="../home.html">Home</a> &gt;
    <a href="../categories/{category["html_name"]}">{category["name"]}</a>
</div>''' if category else f'<div class="breadcrumbs"><a href="../home.html">Home</a></div>'

            prev_next = ""
            if category:
                 siblings = category_controls_map.get(category['name'], [])
                 if ctrl['name'] in siblings:
                    idx = siblings.index(ctrl['name'])
                    prev_link = f'<a href="{siblings[idx-1]}.html">← {siblings[idx-1].replace("Daisy", "")}</a>' if idx > 0 else ""
                    next_link = f'<a href="{siblings[idx+1]}.html">{siblings[idx+1].replace("Daisy", "")} →</a>' if idx < len(siblings) - 1 else ""

                    if prev_link or next_link:
                        prev_next = f'''<div class="doc-nav">
    <div class="nav-left">{prev_link}</div>
    <div class="nav-right">{next_link}</div>
</div>'''

            final_content = breadcrumbs + html_content + prev_next

            page = self._page_template(ctrl['name'], final_content, depth=1)
            (self.output_dir / "controls" / ctrl['html_name']).write_text(page, encoding='utf-8')

    def _generate_category_pages(self):
        """Generate HTML pages for each category."""
        for cat in self.categories:
            md_content = cat['file'].read_text(encoding='utf-8')
            html_content = self.converter.convert(md_content)
            page = self._page_template(cat['name'], html_content, depth=1)
            (self.output_dir / "categories" / cat['html_name']).write_text(page, encoding='utf-8')


def main():
    parser = argparse.ArgumentParser(
        description="Generate Flowery.NET static documentation site.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python Utils/generate_site.py                # Use curated llms-static/ only (default)
  python Utils/generate_site.py --use-generated # Use llms/ (auto-generated) docs
        """
    )
    parser.add_argument(
        '--use-generated',
        action='store_true',
        default=False,
        help='Use llms/ (auto-generated) docs instead of curated llms-static/'
    )
    args = parser.parse_args()

    script_dir = Path(__file__).parent
    root_dir = script_dir.parent
    llms_dir = root_dir / "llms"
    curated_dir = root_dir / "llms-static"
    docs_dir = root_dir / "docs"

    if args.use_generated:
        # Use auto-generated llms/ folder
        if not llms_dir.exists():
            print("Error: llms/ folder not found. Run generate_docs.py --auto-parse first.")
            return
        generator = SiteGenerator(llms_dir, docs_dir, curated_dir=None)
    else:
        # Use curated llms-static/ folder (default)
        if not curated_dir.exists():
            print("Error: llms-static/ folder not found.")
            return
        generator = SiteGenerator(llms_dir, docs_dir, curated_dir=curated_dir)

    generator.generate()


if __name__ == "__main__":
    main()
