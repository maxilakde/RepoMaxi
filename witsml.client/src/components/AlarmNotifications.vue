<template>
  <div v-if="active" class="alarm-popup position-fixed bottom-0 end-0 p-3" style="z-index: 1080; max-width: 420px">
    <div class="card shadow border-danger">
      <div class="card-header py-2 d-flex justify-content-between align-items-center" :class="headerClass">
        <strong>Alarma: {{ active.name }}</strong>
        <span class="badge bg-dark">{{ severityLabel(active.severity) }}</span>
      </div>
      <div class="card-body py-2 small">
        <p class="mb-1"><strong>Pozo:</strong> {{ active.wellName }} ({{ active.wellUid }})</p>
        <p class="mb-1"><strong>Valor:</strong> {{ active.lastTriggeredValueDisplay || '–' }}</p>
        <p class="mb-1"><strong>Disparo:</strong> {{ formatDt(active.triggeredAt) }}</p>
        <p class="mb-2 text-muted"><strong>Transcurrido:</strong> {{ elapsed }}</p>
        <div class="btn-group btn-group-sm w-100 flex-wrap">
          <button type="button" class="btn btn-outline-secondary" @click="snooze(5)">Snooze 5 min</button>
          <button type="button" class="btn btn-outline-secondary" @click="snooze(15)">15 min</button>
          <button type="button" class="btn btn-outline-primary" @click="dismiss">Cerrar</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../api';

export default {
  name: 'AlarmNotifications',
  props: { wellUid: { type: String, default: '' } },
  data() {
    return {
      queue: [],
      active: null,
      _timer: null,
      _poll: null,
      _elapsedTimer: null,
      elapsed: ''
    };
  },
  computed: {
    headerClass() {
      const s = this.active?.severity;
      if (s === 1) return 'bg-danger text-white';
      if (s === 2) return 'bg-warning';
      if (s === 3) return 'bg-info';
      return 'bg-light';
    }
  },
  watch: {
    wellUid: {
      immediate: true,
      handler() {
        this.poll();
      }
    },
    active(v) {
      if (this._elapsedTimer) clearInterval(this._elapsedTimer);
      if (!v) return;
      this.tickElapsed();
      this._elapsedTimer = setInterval(this.tickElapsed, 1000);
    }
  },
  mounted() {
    this._poll = setInterval(this.poll, 12000);
  },
  unmounted() {
    if (this._poll) clearInterval(this._poll);
    if (this._elapsedTimer) clearInterval(this._elapsedTimer);
  },
  methods: {
    severityLabel(s) {
      return { 1: 'Crítico', 2: 'Mayor', 3: 'Menor', 4: 'Normal' }[s] || s;
    },
    formatDt(iso) {
      const d = new Date(iso);
      return isNaN(d.getTime()) ? iso : d.toLocaleString('es-AR');
    },
    tickElapsed() {
      if (!this.active) return;
      const d = new Date(this.active.triggeredAt);
      const sec = Math.floor((Date.now() - d.getTime()) / 1000);
      if (sec < 60) this.elapsed = `${sec} s`;
      else if (sec < 3600) this.elapsed = `${Math.floor(sec / 60)} min`;
      else this.elapsed = `${Math.floor(sec / 3600)} h`;
    },
    async poll() {
      if (!this.wellUid) return;
      try {
        const { data } = await api.get(`/wells/${this.wellUid}/alarms/notifications/pending`);
        const list = data || [];
        if (!list.length) return;
        const ids = new Set((this.queue || []).concat(this.active ? [this.active.id] : []).map((x) => x.id));
        for (const n of list) {
          if (!ids.has(n.id)) this.queue.push(n);
        }
        if (!this.active && this.queue.length) {
          this.active = this.queue.shift();
        }
      } catch {
        /* ignore */
      }
    },
    async snooze(min) {
      if (!this.active) return;
      const id = this.active.id;
      await api.post(`/wells/${this.wellUid}/alarms/${id}/snooze`, { minutes: min });
      this.next();
    },
    async dismiss() {
      if (!this.active) return;
      const id = this.active.id;
      await api.post(`/wells/${this.wellUid}/alarms/${id}/dismiss-notification`);
      this.next();
    },
    next() {
      this.active = this.queue.length ? this.queue.shift() : null;
      if (!this.active) this.poll();
    }
  }
};
</script>
