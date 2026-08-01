/*
 * mvp documentation site behaviour.
 *
 * Loaded as an external file rather than inlined so the site can ship a strict
 * Content-Security-Policy with no 'unsafe-inline'. The cost is a brief flash of the
 * OS-preferred theme for visitors who have explicitly toggled against it; that is
 * preferable to widening the policy for every visitor.
 */
(function () {
  'use strict';

  var STORAGE_KEY = 'mvp-docs-theme';

  /* ---------- Theme ---------- */

  function storedTheme() {
    try {
      return window.localStorage.getItem(STORAGE_KEY);
    } catch (error) {
      return null; // Private-mode or blocked storage: fall back to the OS preference.
    }
  }

  function applyTheme(theme) {
    if (theme === 'light' || theme === 'dark') {
      document.documentElement.setAttribute('data-theme', theme);
    } else {
      document.documentElement.removeAttribute('data-theme');
    }
  }

  function currentTheme() {
    var explicit = document.documentElement.getAttribute('data-theme');
    if (explicit) {
      return explicit;
    }
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  // Runs while the document is still parsing (the tag is in <head>, not deferred), so a
  // stored override is applied before first paint rather than flashing the OS theme.
  applyTheme(storedTheme());

  function initTheme() {
    var toggle = document.getElementById('theme-toggle');
    if (!toggle) {
      return;
    }

    toggle.addEventListener('click', function () {
      var next = currentTheme() === 'dark' ? 'light' : 'dark';
      applyTheme(next);
      try {
        window.localStorage.setItem(STORAGE_KEY, next);
      } catch (error) {
        /* Persisting is a nicety; the toggle still works for this page view. */
      }
      toggle.setAttribute('aria-label', next === 'dark' ? 'Switch to light theme' : 'Switch to dark theme');
    });
  }

  /* ---------- Mobile navigation ---------- */

  function initNavToggle() {
    var toggle = document.getElementById('nav-toggle');
    var sidebar = document.getElementById('sidebar');
    if (!toggle || !sidebar) {
      return;
    }

    toggle.addEventListener('click', function () {
      var open = sidebar.classList.toggle('is-open');
      toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
    });

    sidebar.addEventListener('click', function (event) {
      if (event.target.closest('a')) {
        sidebar.classList.remove('is-open');
        toggle.setAttribute('aria-expanded', 'false');
      }
    });

    document.addEventListener('keydown', function (event) {
      if (event.key === 'Escape' && sidebar.classList.contains('is-open')) {
        sidebar.classList.remove('is-open');
        toggle.setAttribute('aria-expanded', 'false');
        toggle.focus();
      }
    });
  }

  /* ---------- Copy buttons ---------- */

  function initCopyButtons() {
    var blocks = document.querySelectorAll('.code, .terminal');

    Array.prototype.forEach.call(blocks, function (block) {
      var pre = block.querySelector('pre');
      if (!pre || block.hasAttribute('data-no-copy')) {
        return;
      }

      var button = document.createElement('button');
      button.type = 'button';
      button.className = 'copy-button';
      button.textContent = 'Copy';
      button.setAttribute('aria-label', 'Copy to clipboard');

      button.addEventListener('click', function () {
        // Prompt markers are display-only; copying them would break paste-and-run.
        var clone = pre.cloneNode(true);
        Array.prototype.forEach.call(clone.querySelectorAll('.prompt'), function (node) {
          node.parentNode.removeChild(node);
        });
        var text = clone.textContent.replace(/^\n+/, '').replace(/\s+$/, '');

        var done = function (ok) {
          button.textContent = ok ? 'Copied' : 'Press Ctrl+C';
          button.setAttribute('data-copied', ok ? 'true' : 'false');
          window.setTimeout(function () {
            button.textContent = 'Copy';
            button.removeAttribute('data-copied');
          }, 1600);
        };

        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(text).then(function () { done(true); }, function () { done(false); });
        } else {
          done(false);
        }
      });

      block.appendChild(button);
    });
  }

  /* ---------- In-page table of contents ---------- */

  function slugify(text) {
    return text
      .toLowerCase()
      .replace(/[^\w\s-]/g, '')
      .trim()
      .replace(/\s+/g, '-');
  }

  function initToc() {
    var toc = document.getElementById('toc');
    var main = document.getElementById('content');
    if (!toc || !main) {
      return;
    }

    // Card headings are navigation labels, not sections — listing them would bury the
    // page's actual structure under a duplicate of the links already on screen.
    var headings = Array.prototype.filter.call(
      main.querySelectorAll('h2, h3'),
      function (heading) { return !heading.closest('.card'); }
    );
    if (headings.length < 2) {
      return;
    }

    var list = document.createElement('ul');
    var links = [];
    var used = Object.create(null);

    Array.prototype.forEach.call(headings, function (heading) {
      if (!heading.id) {
        var base = slugify(heading.textContent) || 'section';
        var id = base;
        var n = 2;
        while (used[id] || document.getElementById(id)) {
          id = base + '-' + n;
          n += 1;
        }
        heading.id = id;
      }
      used[heading.id] = true;

      var item = document.createElement('li');
      if (heading.tagName === 'H3') {
        item.className = 'toc-h3';
      }
      var link = document.createElement('a');
      link.href = '#' + heading.id;
      // data-toc lets a heading that carries a badge read cleanly in the contents list.
      link.textContent = heading.getAttribute('data-toc') || heading.textContent;
      item.appendChild(link);
      list.appendChild(item);
      links.push({ link: link, heading: heading });
    });

    var title = document.createElement('p');
    title.className = 'toc-title';
    title.textContent = 'On this page';
    toc.appendChild(title);
    toc.appendChild(list);

    initScrollSpy(links);
  }

  function initScrollSpy(links) {
    if (!('IntersectionObserver' in window)) {
      return;
    }

    var visible = Object.create(null);

    var observer = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        visible[entry.target.id] = entry.isIntersecting;
      });

      var active = null;
      for (var i = 0; i < links.length; i += 1) {
        if (visible[links[i].heading.id]) {
          active = links[i];
          break;
        }
      }
      // Nothing in the trigger band (long section, or scrolled past the last heading):
      // keep the last heading above the fold marked instead of clearing the highlight.
      if (!active) {
        for (var j = links.length - 1; j >= 0; j -= 1) {
          if (links[j].heading.getBoundingClientRect().top < 120) {
            active = links[j];
            break;
          }
        }
      }

      links.forEach(function (entry) {
        entry.link.classList.toggle('is-active', entry === active);
      });
    }, { rootMargin: '-72px 0px -70% 0px', threshold: 0 });

    links.forEach(function (entry) {
      observer.observe(entry.heading);
    });
  }

  /* ---------- Current page in the sidebar ---------- */

  function initActiveNav() {
    var here = window.location.pathname.split('/').pop() || 'index.html';
    var links = document.querySelectorAll('#sidebar a[href]');

    Array.prototype.forEach.call(links, function (link) {
      var target = link.getAttribute('href').split('#')[0].split('/').pop();
      if (target === here || (here === '' && target === 'index.html')) {
        link.setAttribute('aria-current', 'page');
      }
    });
  }

  function init() {
    initTheme();
    initNavToggle();
    initCopyButtons();
    initToc();
    initActiveNav();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
