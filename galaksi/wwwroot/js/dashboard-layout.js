(function () {
  var root = document.getElementById('dash');
  if (!root) return;

  var slots = Array.prototype.slice.call(root.querySelectorAll('.slot'));
  var live = document.getElementById('dash-live');
  var resetBtn = document.getElementById('dash-reset');
  var user = root.getAttribute('data-user') || 'anon';
  var KEY = 'galaksi.dash.v1.' + user;
  var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  var drag = null;

  function cardOf(slot) { return slot.querySelector(':scope > .card'); }
  function order() { return slots.map(function (s) { return cardOf(s).getAttribute('data-card'); }); }
  var defaultOrder = order();

  function sameSet(a, b) {
    return a.length === b.length && a.every(function (x) { return b.indexOf(x) !== -1; });
  }
  function load() {
    try {
      var v = JSON.parse(localStorage.getItem(KEY) || 'null');
      return Array.isArray(v) && sameSet(v, defaultOrder) ? v : null;
    } catch (e) { return null; }
  }
  function save() {
    try {
      var o = order();
      if (o.join() === defaultOrder.join()) localStorage.removeItem(KEY);
      else localStorage.setItem(KEY, JSON.stringify(o));
    } catch (e) { }
  }
  function place(ids) {
    var map = {};
    slots.forEach(function (s) { var c = cardOf(s); map[c.getAttribute('data-card')] = c; });
    ids.forEach(function (id, i) { slots[i].appendChild(map[id]); });
  }
  function refreshReset() {
    if (resetBtn) resetBtn.hidden = order().join() === defaultOrder.join();
  }
  function say(msg) {
    if (!live) return;
    live.textContent = '';
    setTimeout(function () { live.textContent = msg; }, 30);
  }

  // İki slottaki kartı yer değiştir (kısa bir geçiş animasyonuyla)
  function swap(sa, sb) {
    var a = cardOf(sa), b = cardOf(sb);
    var ra = a.getBoundingClientRect(), rb = b.getBoundingClientRect();
    sa.appendChild(b);
    sb.appendChild(a);
    if (!reduce && a.animate) {
      var na = a.getBoundingClientRect(), nb = b.getBoundingClientRect();
      var opts = { duration: 240, easing: 'cubic-bezier(.2,.8,.2,1)' };
      a.animate([{ transform: 'translate(' + (ra.left - na.left) + 'px,' + (ra.top - na.top) + 'px)' }, { transform: 'none' }], opts);
      b.animate([{ transform: 'translate(' + (rb.left - nb.left) + 'px,' + (rb.top - nb.top) + 'px)' }, { transform: 'none' }], opts);
    }
    save();
    refreshReset();
  }

  // Klavye: ok yönündeki en yakın slotla yer değiştir
  function neighbor(slot, key) {
    var r = slot.getBoundingClientRect();
    var cx = r.left + r.width / 2, cy = r.top + r.height / 2;
    var best = null, bestScore = Infinity;
    slots.forEach(function (s) {
      if (s === slot) return;
      var q = s.getBoundingClientRect();
      var dx = q.left + q.width / 2 - cx, dy = q.top + q.height / 2 - cy;
      var ok = key === 'ArrowRight' ? dx > 10 : key === 'ArrowLeft' ? dx < -10 : key === 'ArrowDown' ? dy > 10 : dy < -10;
      if (!ok) return;
      var horizontal = key === 'ArrowRight' || key === 'ArrowLeft';
      var score = horizontal ? Math.abs(dx) + 2 * Math.abs(dy) : Math.abs(dy) + 2 * Math.abs(dx);
      if (score < bestScore) { best = s; bestScore = score; }
    });
    return best;
  }
  function keyMove(e, card, handle) {
    if (['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown'].indexOf(e.key) === -1) return;
    e.preventDefault();
    var from = card.parentElement;
    var to = neighbor(from, e.key);
    if (!to) { say('Bu yönde başka konum yok.'); return; }
    swap(from, to);
    handle.focus();
    say(card.getAttribute('data-name') + ' kartı ' + (slots.indexOf(to) + 1) + '. konuma taşındı.');
  }

  // Fare, dokunmatik ve kalem ile sürükleme
  function hit(x, y) {
    var els = document.elementsFromPoint(x, y);
    var insideRoot = false, found = null;
    for (var i = 0; i < els.length; i++) {
      var el = els[i];
      if (el === root || root.contains(el)) insideRoot = true;
      var s = el.closest ? el.closest('#dash .slot') : null;
      if (s && !found) found = s;
    }
    var target = drag.target;
    if (found) target = found === drag.slot ? null : found;
    else if (!insideRoot) target = null;
    if (target !== drag.target) {
      if (drag.target) drag.target.classList.remove('drop-target');
      if (target) target.classList.add('drop-target');
      drag.target = target;
    }
  }
  function autoScroll() {
    if (!drag || drag.raf) return;
    var step = function () {
      if (!drag) return;
      var y = drag.y, h = window.innerHeight, v = 0;
      if (y < 80) v = -Math.ceil((80 - y) / 6);
      else if (y > h - 80) v = Math.ceil((y - (h - 80)) / 6);
      if (v) {
        window.scrollBy(0, v);
        hit(drag.x, drag.y);
        drag.raf = requestAnimationFrame(step);
      } else {
        drag.raf = 0;
      }
    };
    drag.raf = requestAnimationFrame(step);
  }
  function onMove(e) {
    if (!drag || e.pointerId !== drag.pid) return;
    drag.x = e.clientX;
    drag.y = e.clientY;
    drag.ghost.style.left = (e.clientX - drag.dx) + 'px';
    drag.ghost.style.top = (e.clientY - drag.dy) + 'px';
    hit(e.clientX, e.clientY);
    autoScroll();
  }
  function onUp(e) { if (drag && e.pointerId === drag.pid) endDrag(true); }
  function onCancel(e) { if (drag && e.pointerId === drag.pid) endDrag(false); }
  function onEsc(e) { if (e.key === 'Escape') endDrag(false); }

  function startDrag(e, card, handle) {
    if (e.button !== undefined && e.button !== 0) return;
    e.preventDefault();
    var rect = card.getBoundingClientRect();
    var ghost = card.cloneNode(true);
    ghost.querySelectorAll('[id]').forEach(function (n) { n.removeAttribute('id'); });
    ghost.querySelectorAll('button, a').forEach(function (n) { n.setAttribute('tabindex', '-1'); });
    ghost.classList.add('drag-ghost');
    ghost.setAttribute('aria-hidden', 'true');
    ghost.style.width = rect.width + 'px';
    ghost.style.height = rect.height + 'px';
    ghost.style.left = rect.left + 'px';
    ghost.style.top = rect.top + 'px';
    document.body.appendChild(ghost);

    card.classList.add('dragging');
    document.body.classList.add('is-dragging');
    drag = {
      card: card, slot: card.parentElement, ghost: ghost, handle: handle, pid: e.pointerId,
      dx: e.clientX - rect.left, dy: e.clientY - rect.top,
      x: e.clientX, y: e.clientY, target: null, raf: 0
    };
    try { handle.setPointerCapture(e.pointerId); } catch (x) { }
    window.addEventListener('pointermove', onMove);
    window.addEventListener('pointerup', onUp);
    window.addEventListener('pointercancel', onCancel);
    document.addEventListener('keydown', onEsc);
  }

  function endDrag(commit) {
    if (!drag) return;
    var d = drag;
    drag = null;
    window.removeEventListener('pointermove', onMove);
    window.removeEventListener('pointerup', onUp);
    window.removeEventListener('pointercancel', onCancel);
    document.removeEventListener('keydown', onEsc);
    if (d.raf) cancelAnimationFrame(d.raf);
    d.ghost.remove();
    d.card.classList.remove('dragging');
    document.body.classList.remove('is-dragging');
    if (d.target) d.target.classList.remove('drop-target');
    try { d.handle.releasePointerCapture(d.pid); } catch (x) { }
    if (commit && d.target) {
      swap(d.slot, d.target);
      say(d.card.getAttribute('data-name') + ' kartı ' + (slots.indexOf(d.target) + 1) + '. konuma taşındı.');
    }
  }

  // Her kartın başlığına tutamaç ekle
  slots.forEach(function (s) {
    var card = cardOf(s);
    var t = card.querySelector('.card-title .t');
    if (!t) return;
    var name = t.textContent.trim();
    card.setAttribute('data-name', name);

    var b = document.createElement('button');
    b.type = 'button';
    b.className = 'drag-handle';
    b.title = 'Sürükleyerek taşıyın';
    b.setAttribute('aria-label', name + ' kartını taşı. Fare ile sürükleyin ya da ok tuşlarını kullanın.');
    b.innerHTML = '<svg class="ico" aria-hidden="true"><use href="#i-grip"></use></svg>';
    b.addEventListener('pointerdown', function (e) { startDrag(e, card, b); });
    b.addEventListener('keydown', function (e) { keyMove(e, card, b); });
    t.insertBefore(b, t.firstChild);
  });

  if (resetBtn) {
    resetBtn.addEventListener('click', function () {
      place(defaultOrder);
      save();
      refreshReset();
      if (window.GalaksiToast) window.GalaksiToast.show('Düzen varsayılana döndürüldü.', 'success');
    });
  }

  var saved = load();
  if (saved) place(saved);
  refreshReset();
})();
