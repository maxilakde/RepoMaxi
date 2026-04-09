<template>
  <div class="well-alarms">
    <div class="alarms-toolbar d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
      <div class="d-flex flex-wrap gap-2 align-items-center">
        <button type="button" class="btn btn-sm btn-primary" :disabled="loading" @click="openCreate">Nueva alarma</button>
        <button type="button" class="btn btn-sm btn-outline-primary" :disabled="loading" @click="runEvaluate">Evaluar ahora</button>
        <button type="button" class="btn btn-sm btn-outline-secondary" :disabled="!selected || loading" @click="openEdit">Editar</button>
        <button type="button" class="btn btn-sm btn-outline-secondary" :disabled="!selected || loading || !canDelete" @click="removeAlarm">Eliminar</button>
        <button type="button" class="btn btn-sm btn-outline-secondary" :disabled="!selected || loading || !canToggleEnable" @click="toggleEnable">{{ selected && selected.isEnabled ? 'Deshabilitar' : 'Habilitar' }}</button>
        <button type="button" class="btn btn-sm btn-outline-secondary" :disabled="!selected || loading" @click="openEvents">Historial eventos</button>
        <button type="button" class="btn btn-sm btn-outline-info" :disabled="loading" @click="openSubscribe">Suscribirse a alarma pública</button>
        <label class="small mb-0 d-flex align-items-center gap-1">
          Clave suscriptor
          <input v-model="subscriberKey" class="form-control form-control-sm" style="width: 120px" title="Se guarda en el navegador" @change="persistSubscriber" />
        </label>
      </div>
      <div class="d-flex flex-wrap gap-2 align-items-center">
        <input v-model="filterText" type="search" class="form-control form-control-sm" placeholder="Filtrar…" style="max-width: 180px" />
        <label class="small mb-0 text-muted">{{ filteredRows.length }} alarma(s)</label>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>
    <div v-if="evalMsg" class="alert alert-info py-2">{{ evalMsg }}</div>

    <div class="alarms-grid-outer">
      <div v-if="loading" class="text-muted py-4 text-center">Cargando…</div>
      <div v-else class="alarms-container">
        <div class="table-wrap">
          <table class="table table-sm table-bordered table-hover mb-0">
            <thead class="table-dark">
              <tr>
                <th v-for="c in gridColumns" :key="c.key" class="sortable" @click="sortBy(c.key)">{{ c.label }} <span v-if="sortColumn === c.key">{{ sortAsc ? '↑' : '↓' }}</span></th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="row in pagedRows"
                :key="row.id"
                :class="{ 'table-active': selected && selected.id === row.id }"
                @click="selected = row"
              >
                <td>{{ row.variableMnemonic }}</td>
                <td>{{ row.conditionDisplay }}</td>
                <td>{{ row.secondVariableMnemonic || '–' }}</td>
                <td>{{ row.secondConditionDisplay || '–' }}</td>
                <td><span class="badge" :class="severityClass(row.severity)">{{ severityLabel(row.severity) }}</span></td>
                <td>{{ row.isEnabled ? 'Sí' : 'No' }}</td>
                <td>{{ row.isTriggered ? 'Disparada' : 'Normal' }}</td>
                <td>{{ row.lastTriggeredValueDisplay || '–' }}</td>
                <td>{{ row.timesTriggered }}</td>
                <td>{{ row.timeElapsedDisplay || '–' }}</td>
                <td>{{ row.lastHoleDepthMd != null ? Number(row.lastHoleDepthMd).toFixed(2) : '–' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="totalRowCount > 0" class="pagination-bar d-flex flex-wrap justify-content-between align-items-center gap-2">
          <span class="small text-muted">Mostrando {{ rangeStart }}–{{ rangeEnd }} de {{ totalRowCount }}</span>
          <div class="d-flex flex-wrap align-items-center gap-2">
            <select v-model.number="pageSize" class="form-select form-select-sm" style="width: auto">
              <option :value="10">10</option>
              <option :value="25">25</option>
              <option :value="50">50</option>
              <option :value="0">Todas</option>
            </select>
            <div class="btn-group btn-group-sm">
              <button type="button" class="btn btn-outline-secondary" :disabled="currentPage <= 1" @click.stop="goToPage(1)">«</button>
              <button type="button" class="btn btn-outline-secondary" :disabled="currentPage <= 1" @click.stop="goToPage(currentPage - 1)">‹</button>
            </div>
            <span class="small">Página {{ currentPage }} / {{ totalPages }}</span>
            <div class="btn-group btn-group-sm">
              <button type="button" class="btn btn-outline-secondary" :disabled="currentPage >= totalPages" @click.stop="goToPage(currentPage + 1)">›</button>
              <button type="button" class="btn btn-outline-secondary" :disabled="currentPage >= totalPages" @click.stop="goToPage(totalPages)">»</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="mt-2 d-flex flex-wrap gap-2">
      <button type="button" class="btn btn-sm btn-outline-secondary" :disabled="!selected || !isOwner" @click="exportXml">Exportar XML</button>
      <label class="btn btn-sm btn-outline-secondary mb-0">
        Importar XML
        <input type="file" accept=".xml,text/xml" class="d-none" @change="importXml" />
      </label>
      <a v-if="selected" class="btn btn-sm btn-outline-secondary" :href="csvHref" download>Exportar historial CSV</a>
    </div>

    <!-- Modal edición -->
    <div v-if="showModal" class="modal d-block" tabindex="-1" style="background: rgba(0,0,0,0.4)">
      <div class="modal-dialog modal-lg modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editingId ? 'Editar alarma' : 'Nueva alarma' }}</h5>
            <button type="button" class="btn-close" @click="closeModal" />
          </div>
          <div class="modal-body">
            <div class="row g-2">
              <div class="col-md-6">
                <label class="form-label small">Nombre (9.1)</label>
                <input v-model="form.name" class="form-control form-control-sm" />
              </div>
              <div class="col-md-6">
                <label class="form-label small">Severidad (9.7)</label>
                <select v-model.number="form.severity" class="form-select form-select-sm">
                  <option :value="1">Crítico</option>
                  <option :value="2">Mayor</option>
                  <option :value="3">Menor</option>
                  <option :value="4">Normal</option>
                </select>
              </div>
              <div class="col-md-4">
                <label class="form-label small">Variable (9.2)</label>
                <select v-model="form.variableMnemonic" class="form-select form-select-sm">
                  <option v-for="v in variables" :key="v.mnemonic" :value="v.mnemonic">{{ v.caption }} ({{ v.mnemonic }})</option>
                </select>
              </div>
              <div class="col-md-2">
                <label class="form-label small">Condición (9.3)</label>
                <select v-model="form.conditionOperator" class="form-select form-select-sm">
                  <option value=">">&gt;</option>
                  <option value="<">&lt;</option>
                  <option value="=">=</option>
                </select>
              </div>
              <div class="col-md-3">
                <label class="form-label small">Umbral</label>
                <input v-model.number="form.thresholdValue" type="number" step="any" class="form-control form-control-sm" />
              </div>
              <div class="col-md-3">
                <label class="form-label small">Unidad</label>
                <input v-model="form.unit" class="form-control form-control-sm" />
              </div>
              <div class="col-md-4">
                <label class="form-label small">2ª variable (9.4–9.5)</label>
                <select v-model="form.secondVariableMnemonic" class="form-select form-select-sm">
                  <option :value="null">— Ninguna —</option>
                  <option v-for="v in variables" :key="'s-' + v.mnemonic" :value="v.mnemonic">{{ v.mnemonic }}</option>
                </select>
              </div>
              <div class="col-md-2">
                <label class="form-label small">2ª cond. (9.6)</label>
                <select v-model="form.secondConditionOperator" class="form-select form-select-sm" :disabled="!form.secondVariableMnemonic">
                  <option :value="null">—</option>
                  <option value=">">&gt;</option>
                  <option value="<">&lt;</option>
                  <option value="=">=</option>
                </select>
              </div>
              <div class="col-md-3">
                <label class="form-label small">2º umbral</label>
                <input v-model.number="form.secondThresholdValue" type="number" step="any" class="form-control form-control-sm" :disabled="!form.secondVariableMnemonic" />
              </div>
              <div class="col-md-12">
                <div class="form-check form-check-inline"><input id="ne" v-model="form.notifyEmail" type="checkbox" class="form-check-input" /><label class="form-check-label small" for="ne">Email (9.8)</label></div>
                <div class="form-check form-check-inline"><input id="ns" v-model="form.notifySms" type="checkbox" class="form-check-input" /><label class="form-check-label small" for="ns">SMS (9.9)</label></div>
                <div class="form-check form-check-inline"><input id="ip" v-model="form.isPublic" type="checkbox" class="form-check-input" /><label class="form-check-label small" for="ip">Pública (9.11)</label></div>
                <div class="form-check form-check-inline"><input id="ie" v-model="form.isEnabled" type="checkbox" class="form-check-input" /><label class="form-check-label small" for="ie">Habilitada (9.12)</label></div>
              </div>
              <div class="col-md-6">
                <label class="form-label small">Teléfono extra (9.10)</label>
                <input v-model="form.extraPhone" class="form-control form-control-sm" />
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary btn-sm" @click="closeModal">Cancelar</button>
            <button type="button" class="btn btn-primary btn-sm" :disabled="saving" @click="saveAlarm">Guardar (9.13)</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Historial -->
    <div v-if="showEvents" class="modal d-block" tabindex="-1" style="background: rgba(0,0,0,0.4)">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Historial de eventos</h5>
            <button type="button" class="btn-close" @click="showEvents = false" />
          </div>
          <div class="modal-body">
            <div v-if="eventsLoading" class="text-muted">Cargando…</div>
            <table v-else class="table table-sm">
              <thead><tr><th>Fecha</th><th>Severidad</th><th>Valor 1</th><th>Valor 2</th><th>Prof. (MD)</th></tr></thead>
              <tbody>
                <tr v-for="e in eventItems" :key="e.id">
                  <td>{{ formatDt(e.triggeredAt) }}</td>
                  <td>{{ e.severity }}</td>
                  <td>{{ e.valuePrimary }}</td>
                  <td>{{ e.valueSecondary }}</td>
                  <td>{{ e.holeDepthMd }}</td>
                </tr>
              </tbody>
            </table>
            <div class="d-flex justify-content-between align-items-center small">
              <span>Total: {{ eventTotal }}</span>
              <div>
                <button class="btn btn-sm btn-outline-secondary" :disabled="eventPage <= 1" @click="eventPage--; loadEvents()">Anterior</button>
                <span class="mx-2">Pág. {{ eventPage }}</span>
                <button class="btn btn-sm btn-outline-secondary" :disabled="eventPage * eventPageSize >= eventTotal" @click="eventPage++; loadEvents()">Siguiente</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Suscribirse -->
    <div v-if="showSub" class="modal d-block" tabindex="-1" style="background: rgba(0,0,0,0.4)">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Alarmas públicas disponibles</h5>
            <button type="button" class="btn-close" @click="showSub = false" />
          </div>
          <div class="modal-body">
            <ul v-if="publicCandidates.length" class="list-group list-group-flush">
              <li v-for="p in publicCandidates" :key="p.id" class="list-group-item d-flex justify-content-between align-items-center">
                <span>{{ p.name }} — {{ p.conditionDisplay }}</span>
                <button type="button" class="btn btn-sm btn-primary" @click="doSubscribe(p.id)">Suscribirse</button>
              </li>
            </ul>
            <p v-else class="text-muted mb-0">No hay alarmas públicas de otros usuarios.</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../api';

const defaultForm = () => ({
  name: '',
  variableMnemonic: 'md',
  conditionOperator: '>',
  thresholdValue: 0,
  unit: 'm',
  secondVariableMnemonic: null,
  secondConditionOperator: null,
  secondThresholdValue: null,
  severity: 2,
  notifyEmail: false,
  notifySms: false,
  extraPhone: '',
  isPublic: false,
  isEnabled: true
});

export default {
  name: 'WellAlarms',
  props: { wellUid: { type: String, required: true } },
  data() {
    return {
      loading: false,
      saving: false,
      error: null,
      evalMsg: null,
      rows: [],
      variables: [],
      selected: null,
      filterText: '',
      sortColumn: 'name',
      sortAsc: true,
      currentPage: 1,
      pageSize: 25,
      subscriberKey: 'default',
      showModal: false,
      editingId: null,
      form: defaultForm(),
      showEvents: false,
      eventsLoading: false,
      eventItems: [],
      eventPage: 1,
      eventPageSize: 15,
      eventTotal: 0,
      showSub: false,
      publicCandidates: [],
      gridColumns: [
        { key: 'variableMnemonic', label: 'Variable' },
        { key: 'conditionDisplay', label: 'Condición' },
        { key: 'secondVariableMnemonic', label: '2ª variable' },
        { key: 'secondConditionDisplay', label: '2ª condición' },
        { key: 'severity', label: 'Severidad' },
        { key: 'isEnabled', label: 'Habilitada' },
        { key: 'isTriggered', label: 'Estado' },
        { key: 'lastTriggeredValueDisplay', label: 'Último disparo' },
        { key: 'timesTriggered', label: 'Nº disparos' },
        { key: 'timeElapsedDisplay', label: 'Tiempo transcurrido' },
        { key: 'lastHoleDepthMd', label: 'Prof. hoyo (MD)' }
      ]
    };
  },
  computed: {
    filteredRows() {
      const t = (this.filterText || '').toLowerCase();
      let r = this.rows;
      if (t) {
        r = r.filter((x) =>
          [x.name, x.variableMnemonic, x.conditionDisplay, x.secondVariableMnemonic]
            .filter(Boolean)
            .some((f) => String(f).toLowerCase().includes(t))
        );
      }
      const col = this.sortColumn;
      const mult = this.sortAsc ? 1 : -1;
      return [...r].sort((a, b) => {
        const va = a[col];
        const vb = b[col];
        if (va == null && vb == null) return 0;
        if (va == null) return mult;
        if (vb == null) return -mult;
        if (typeof va === 'number' && typeof vb === 'number') return mult * (va - vb);
        return mult * String(va).localeCompare(String(vb), 'es');
      });
    },
    totalRowCount() {
      return this.filteredRows.length;
    },
    totalPages() {
      if (this.totalRowCount === 0) return 1;
      if (this.pageSize <= 0) return 1;
      return Math.max(1, Math.ceil(this.totalRowCount / this.pageSize));
    },
    pagedRows() {
      if (this.pageSize <= 0) return this.filteredRows;
      const start = (this.currentPage - 1) * this.pageSize;
      return this.filteredRows.slice(start, start + this.pageSize);
    },
    rangeStart() {
      if (this.totalRowCount === 0) return 0;
      if (this.pageSize <= 0) return 1;
      return (this.currentPage - 1) * this.pageSize + 1;
    },
    rangeEnd() {
      if (this.totalRowCount === 0) return 0;
      if (this.pageSize <= 0) return this.totalRowCount;
      return Math.min(this.currentPage * this.pageSize, this.totalRowCount);
    },
    isOwner() {
      return this.selected && !this.selected.isSubscriptionOnly;
    },
    canDelete() {
      return this.selected;
    },
    canToggleEnable() {
      return this.selected && !this.selected.isSubscriptionOnly;
    },
    csvHref() {
      if (!this.selected || !this.wellUid) return '#';
      const sk = encodeURIComponent(localStorage.getItem('witsmlSubscriberKey') || 'default');
      return `/api/v1/wells/${encodeURIComponent(this.wellUid)}/alarms/${this.selected.id}/events/export?subscriberKey=${sk}`;
    }
  },
  watch: {
    pageSize() {
      this.currentPage = 1;
    },
    totalRowCount() {
      if (this.currentPage > this.totalPages) this.currentPage = Math.max(1, this.totalPages);
    },
    wellUid() {
      this.loadAll();
    }
  },
  mounted() {
    this.subscriberKey = localStorage.getItem('witsmlSubscriberKey') || 'default';
    this.persistSubscriber();
    this.loadVariables();
    this.loadAll();
    this._evalTimer = setInterval(() => this.runEvaluateSilent(), 45000);
  },
  unmounted() {
    if (this._evalTimer) clearInterval(this._evalTimer);
  },
  methods: {
    persistSubscriber() {
      localStorage.setItem('witsmlSubscriberKey', this.subscriberKey);
    },
    severityLabel(s) {
      return { 1: 'Crítico', 2: 'Mayor', 3: 'Menor', 4: 'Normal' }[s] || s;
    },
    severityClass(s) {
      return { 1: 'bg-danger', 2: 'text-dark', 3: 'bg-warning text-dark', 4: 'bg-light text-dark border' }[s] || 'bg-secondary';
    },
    sortBy(key) {
      if (this.sortColumn === key) this.sortAsc = !this.sortAsc;
      else {
        this.sortColumn = key;
        this.sortAsc = true;
      }
    },
    goToPage(p) {
      this.currentPage = Math.min(Math.max(1, p), this.totalPages);
    },
    formatDt(iso) {
      if (!iso) return '';
      const d = new Date(iso);
      return isNaN(d.getTime()) ? iso : d.toLocaleString('es-AR');
    },
    async loadVariables() {
      try {
        const { data } = await api.get(`/wells/${this.wellUid}/statistics/variables`);
        this.variables = data || [];
        if (!this.variables.length) {
          this.variables = [
            { mnemonic: 'md', caption: 'MD' },
            { mnemonic: 'tvd', caption: 'TVD' },
            { mnemonic: 'incl', caption: 'Incl' },
            { mnemonic: 'azi', caption: 'Azi' }
          ];
        }
      } catch {
        this.variables = [{ mnemonic: 'md', caption: 'MD' }];
      }
    },
    async loadAll() {
      if (!this.wellUid) return;
      this.loading = true;
      this.error = null;
      try {
        const { data } = await api.get(`/wells/${encodeURIComponent(this.wellUid)}/alarms`);
        this.rows = data || [];
        this.selected = null;
      } catch (e) {
        this.error = e.response?.data?.detail || e.message || 'Error al cargar alarmas';
        this.rows = [];
      } finally {
        this.loading = false;
      }
    },
    async runEvaluate() {
      this.evalMsg = null;
      try {
        const { data } = await api.post(`/wells/${this.wellUid}/alarms/evaluate`);
        const n = data?.newlyTriggered?.length || 0;
        this.evalMsg = n ? `Evaluación completada. ${n} nueva(s) alarma(s) disparada(s).` : 'Evaluación completada.';
        await this.loadAll();
      } catch (e) {
        this.error = e.response?.data?.detail || e.message || 'Error al evaluar';
      }
    },
    async runEvaluateSilent() {
      try {
        await api.post(`/wells/${this.wellUid}/alarms/evaluate`);
        await this.loadAll();
      } catch {
        /* ignore */
      }
    },
    openCreate() {
      this.editingId = null;
      this.form = { ...defaultForm(), variableMnemonic: this.variables[0]?.mnemonic || 'md' };
      this.showModal = true;
    },
    openEdit() {
      if (!this.selected || this.selected.isSubscriptionOnly) return;
      this.editingId = this.selected.id;
      const s = this.selected;
      this.form = {
        name: s.name,
        variableMnemonic: s.variableMnemonic,
        conditionOperator: s.conditionOperator,
        thresholdValue: s.thresholdValue,
        unit: s.unit || 'm',
        secondVariableMnemonic: s.secondVariableMnemonic || null,
        secondConditionOperator: s.secondConditionOperator || null,
        secondThresholdValue: s.secondThresholdValue,
        severity: s.severity,
        notifyEmail: s.notifyEmail,
        notifySms: s.notifySms,
        extraPhone: s.extraPhone || '',
        isPublic: s.isPublic,
        isEnabled: s.isEnabled
      };
      this.showModal = true;
    },
    closeModal() {
      this.showModal = false;
    },
    async saveAlarm() {
      this.saving = true;
      this.error = null;
      try {
        const body = { ...this.form };
        if (!body.secondVariableMnemonic) {
          body.secondConditionOperator = null;
          body.secondThresholdValue = null;
        }
        if (this.editingId) {
          await api.put(`/wells/${this.wellUid}/alarms/${this.editingId}`, body);
        } else {
          await api.post(`/wells/${this.wellUid}/alarms`, body);
        }
        this.showModal = false;
        await this.loadAll();
      } catch (e) {
        this.error = e.response?.data?.detail || e.message || 'Error al guardar';
      } finally {
        this.saving = false;
      }
    },
    async removeAlarm() {
      if (!this.selected || !confirm('¿Eliminar esta alarma o cancelar suscripción?')) return;
      try {
        await api.delete(`/wells/${this.wellUid}/alarms/${this.selected.id}`);
        await this.loadAll();
      } catch (e) {
        this.error = e.response?.data?.detail || e.message || 'Error';
      }
    },
    async toggleEnable() {
      if (!this.selected || this.selected.isSubscriptionOnly) return;
      const id = this.selected.id;
      try {
        if (this.selected.isEnabled) {
          await api.post(`/wells/${this.wellUid}/alarms/${id}/disable`);
        } else {
          await api.post(`/wells/${this.wellUid}/alarms/${id}/enable`);
        }
        await this.loadAll();
      } catch (e) {
        this.error = e.response?.data?.detail || e.message || 'Error';
      }
    },
    openEvents() {
      if (!this.selected) return;
      this.showEvents = true;
      this.eventPage = 1;
      this.loadEvents();
    },
    async loadEvents() {
      if (!this.selected) return;
      this.eventsLoading = true;
      try {
        const { data } = await api.get(`/wells/${this.wellUid}/alarms/${this.selected.id}/events`, {
          params: { page: this.eventPage, pageSize: this.eventPageSize }
        });
        this.eventItems = data.items || [];
        this.eventTotal = data.totalCount || 0;
      } catch {
        this.eventItems = [];
      } finally {
        this.eventsLoading = false;
      }
    },
    async openSubscribe() {
      this.showSub = true;
      try {
        const { data } = await api.get(`/wells/${this.wellUid}/alarms/public-candidates`);
        this.publicCandidates = data || [];
      } catch {
        this.publicCandidates = [];
      }
    },
    async doSubscribe(id) {
      try {
        await api.post(`/wells/${this.wellUid}/alarms/${id}/subscribe`, { notifyEmail: true, notifySms: false });
        this.showSub = false;
        await this.loadAll();
      } catch (e) {
        this.error = e.response?.data?.detail || e.message || 'Error al suscribirse';
      }
    },
    async exportXml() {
      if (!this.selected) return;
      const sk = encodeURIComponent(localStorage.getItem('witsmlSubscriberKey') || 'default');
      window.open(`/api/v1/wells/${encodeURIComponent(this.wellUid)}/alarms/${this.selected.id}/export/xml?subscriberKey=${sk}`, '_blank');
    },
    async importXml(ev) {
      const file = ev.target.files?.[0];
      ev.target.value = '';
      if (!file) return;
      const text = await file.text();
      try {
        await api.post(`/wells/${this.wellUid}/alarms/import/xml`, { xml: text });
        await this.loadAll();
      } catch (e) {
        this.error = e.response?.data?.detail || e.message || 'Error al importar';
      }
    }
  }
};
</script>

<style scoped>
.well-alarms { padding: 0.25rem 0; }
.alarms-grid-outer {
  border: 1px solid #dee2e6;
  border-radius: 4px;
  max-height: 480px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.alarms-container {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
}
.table-wrap {
  flex: 1;
  overflow: auto;
  min-height: 0;
}
.pagination-bar {
  flex-shrink: 0;
  padding: 0.4rem 0.5rem;
  border-top: 1px solid #dee2e6;
  background: #f8f9fa;
}
.sortable {
  cursor: pointer;
  user-select: none;
  white-space: nowrap;
}
</style>
