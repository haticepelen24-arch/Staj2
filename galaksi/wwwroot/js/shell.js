(function() {
    var bar = document.querySelector('.topbar');
    var menus = document.querySelectorAll('.menu');

    // Açılır menüler
    function closeAll(except) {
        menus.forEach(function(m) {
            if (m === except) return;
            m.classList.remove('open');
            var b = m.querySelector('.menu-btn');
            if (b) b.setAttribute('aria-expanded', 'false');
        });
    }
    menus.forEach(function(m) {
        var btn = m.querySelector('.menu-btn');
        btn.addEventListener('click', function(e) {
            e.stopPropagation();
            var willOpen = !m.classList.contains('open');
            closeAll(m);
            m.classList.toggle('open', willOpen);
            btn.setAttribute('aria-expanded', willOpen ? 'true' : 'false');
            var f = m.querySelector('input[type="search"]');
            if (willOpen && f) setTimeout(function() { f.focus(); }, 0);
        });
    });
    document.addEventListener('click', function(e) { if (!e.target.closest('.menu')) closeAll(); });
    document.addEventListener('keydown', function(e) { if (e.key === 'Escape') closeAll(); });

    // Dar ekranda menü düğmesi
    var toggle = document.querySelector('.nav-toggle');
    if (toggle && bar) {
        toggle.addEventListener('click', function() {
            var open = bar.classList.toggle('is-open');
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    }

    // Bildirimler: okundu bilgisi bu tarayıcıda saklanır
    var KEY = 'galaksi.read';
    var badge = document.getElementById('bell-badge');
    var list = document.getElementById('nlist');
    function readIds() { try { return JSON.parse(localStorage.getItem(KEY) || '[]'); } catch (e) { return []; } }
    function saveIds(ids) { try { localStorage.setItem(KEY, JSON.stringify(ids)); } catch (e) { } }
    function refreshBadge() {
        var n = list ? list.querySelectorAll('.nitem.unread').length : 0;
        if (badge) { badge.textContent = n; badge.hidden = n === 0; }
    }
    if (list) {
        var done = readIds();
        list.querySelectorAll('.nitem').forEach(function(li) {
            var id = li.getAttribute('data-id');
            if (done.indexOf(id) !== -1) li.classList.remove('unread');
            li.querySelector('a').addEventListener('click', function() {
                var ids = readIds();
                if (ids.indexOf(id) === -1) { ids.push(id); saveIds(ids); }
            });
        });
        var all = document.getElementById('read-all');
        if (all) {
            all.addEventListener('click', function() {
                var ids = [];
                list.querySelectorAll('.nitem').forEach(function(li) {
                    ids.push(li.getAttribute('data-id'));
                    li.classList.remove('unread');
                });
                saveIds(ids);
                refreshBadge();
            });
        }
        refreshBadge();
    }

    // Uyarı kutuları: GalaksiToast.show('mesaj', 'error' | 'success' | 'info')
    var box = document.getElementById('toasts');
    function toast(message, type, ms) {
        if (!box || !message) return;
        var el = document.createElement('div');
        el.className = 'toast ' + (type || 'info');
        el.setAttribute('role', type === 'error' ? 'alert' : 'status');
        var p = document.createElement('p');
        p.textContent = message;
        var x = document.createElement('button');
        x.type = 'button';
        x.setAttribute('aria-label', 'Kapat');
        x.textContent = '×';
        x.addEventListener('click', function() { el.remove(); });
        el.appendChild(p);
        el.appendChild(x);
        box.appendChild(el);
        setTimeout(function() { el.remove(); }, ms || (type === 'error' ? 8000 : 5000));
    }
    window.GalaksiToast = { show: toast };

    // Sunucudan gelen mesajlar (TempData["Ok"] ve TempData["Error"])
    var src = document.getElementById('server-toasts');
    if (src) {
        toast(src.getAttribute('data-ok'), 'success');
        toast(src.getAttribute('data-error'), 'error');
    }

    // Formlarda hatalı alan varsa tek bir uyarı göster
    var bad = Array.prototype.filter.call(document.querySelectorAll('.field-error'), function(s) {
        return s.textContent.trim().length > 0;
    });
    if (bad.length) toast('Formda eksik veya hatalı alanlar var, lütfen kontrol edin.', 'error');
})();