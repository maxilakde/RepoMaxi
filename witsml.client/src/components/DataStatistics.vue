<template>
  <div class="data-statistics">
    <div class="stats-toolbar d-flex justify-content-between align-items-center flex-wrap gap-2 mb-2">
      <div class="d-flex gap-2 align-items-center flex-wrap">
        <button class="btn btn-sm btn-outline-primary" @click="showVariablePane = !showVariablePane">
          {{ showVariablePane ? '▼' : '▶' }} Variables
        </button>
        <span class="text-muted small">{{ rows.length }} fila(s)</span>
      </div>
      <div class="d-flex gap-2 align-items-center">
        <label class="small mb-0">Rango por:</label>
        <select v-model="indexType" class="form-select form-select-sm w-auto">
          <option value="depth">Profundidad</option>
          <option value="time">Tiempo</option>
        </select>
        <template v-if="indexType === 'depth'">
          <input v-model.number="minDepth" type="number" class="form-control form-control-sm" placeholder="Min (m)" style="width: 90px" step="0.1" />
          <input v-model.number="maxDepth" type="number" class="form-control form-control-sm" placeholder="Max (m)" style="width: 90px" step="0.1" />
        </template>
        <template v-else>
          <input v-model="minTime" type="datetime-local" class="form-control form-control-sm" style="width: 180px" />
          <input v-model="maxTime" type="datetime-local" class="form-control form-control-sm" style="width: 180px" />
        </template>
        <button class="btn btn-sm btn-primary" @click="loadData" :disabled="loading">Aplicar</button>
      </div>
    </div>

    <div v-if="showVariablePane" class="variable-pane mb-3 p-3 border rounded bg-light">
      <h6 class="mb-2">Selección de variables</h6>
      <div class="row">
        <div class="col-md-6">
          <p class="small text-muted mb-1">Variables disponibles:</p>
          <div class="variable-list">
            <label v-for="v in availableVariables" :key="v.mnemonic" class="d-block mb-1">
              <input type="checkbox" :value="v.mnemonic" v-model="selectedVariables" />
              {{ v.caption }} ({{ v.mnemonic }}) {{ v.unit ? `[${v.unit}]` : '' }}
            </label>
          </div>
        </div>
        <div class="col-md-6">
          <p class="small text-muted mb-1">Orden de columnas:</p>
          <ul class="list-group list-group-flush small">
            <li v-for="(m, i) in selectedVariables" :key="m" class="list-group-item py-1 d-flex justify-content-between">
              {{ availableVariables.find(a => a.mnemonic === m)?.caption || m }}
              <span class="badge bg-secondary">{{ i + 1 }}</span>
            </li>
          </ul>
          <button class="btn btn-sm btn-outline-primary mt-2" @click="loadData">Aplicar selección</button>
        </div>
      </div>
    </div>

    <div class="stats-grid-wrapper">
      <div v-if="loading" class="text-center py-5 text-muted">Cargando...</div>
      <div v-else-if="error" class="alert alert-danger">{{ error }}</div>
      <div v-else-if="rows.length === 0" class="alert alert-info">
        No hay datos de survey para este pozo. Procese archivos WITSML con trayectorias.
      </div>
      <div v-else class="stats-container">
        <!-- Summary Grid (fijo arriba) -->
        <div class="summary-grid bg-warning bg-opacity-10 border-bottom">
          <table class="table table-sm table-bordered mb-0">
            <tbody>
              <tr v-for="(statRow, si) in summaryRows" :key="si">
                <td class="stat-label">{{ statRow.label }}</td>
                <td v-for="col in columns" :key="col" class="text-end">{{ formatVal(statRow.values[col]) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <!-- Data Grid (scrollable) -->
        <div class="data-grid-wrapper">
          <table class="table table-sm table-bordered table-hover table-striped mb-0">
            <thead class="table-dark sticky-top">
              <tr>
                <th v-for="col in columns" :key="col" @click="sortBy(col)" class="sortable">
                  {{ variableCaption(col) }}
                  <span v-if="sortColumn === col">{{ sortAsc ? '↑' : '↓' }}</span>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, ri) in sortedRows" :key="ri">
                <td v-for="col in columns" :key="col">{{ formatVal(row[col]) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div class="mt-2 d-flex gap-2">
      <button class="btn btn-sm btn-outline-secondary" @click="addStat('count')">+ Conteo</button>
      <button class="btn btn-sm btn-outline-secondary" @click="addStat('sum')">+ Suma</button>
      <button class="btn btn-sm btn-outline-secondary" @click="addStat('min')">+ Mín</button>
      <button class="btn btn-sm btn-outline-secondary" @click="addStat('max')">+ Máx</button>
      <button class="btn btn-sm btn-outline-secondary" @click="addStat('mean')">+ Media</button>
      <button class="btn btn-sm btn-outline-secondary" @click="addStat('stddev')">+ Desv.Est.</button>
      <span class="small text-muted align-self-center">Máx 3 funciones por columna (se aplican a la primera)</span>
    </div>
  </div>
</template>

<script>
import api from '../api';

export default {
  name: 'DataStatistics',
  props: { wellUid: { type: String, required: true } },
  data() {
    return {
      loading: false,
      error: null,
      rows: [],
      columns: [],
      availableVariables: [],
      selectedVariables: ['md', 'tvd', 'incl', 'azi'],
      showVariablePane: true,
      indexType: 'depth',
      minDepth: null,
      maxDepth: null,
      minTime: null,
      maxTime: null,
      sortColumn: null,
      sortAsc: true,
      activeStats: ['count', 'min', 'max']
    };
  },
  computed: {
    sortedRows() {
      if (!this.sortColumn) return this.rows;
      const col = this.sortColumn;
      const mult = this.sortAsc ? 1 : -1;
      return [...this.rows].sort((a, b) => {
        const va = a[col];
        const vb = b[col];
        if (va == null && vb == null) return 0;
        if (va == null) return mult;
        if (vb == null) return -mult;
        if (typeof va === 'number' && typeof vb === 'number') return mult * (va - vb);
        if (va instanceof Date && vb instanceof Date) return mult * (va - vb);
        return mult * String(va).localeCompare(String(vb));
      });
    },
    summaryRows() {
      const stats = this.activeStats.slice(0, 3);
      const labels = { count: 'Conteo', sum: 'Suma', min: 'Mín', max: 'Máx', mean: 'Media', stddev: 'Desv.Est.' };
      return stats.map(s => ({
        label: labels[s] || s,
        values: this.computeStat(s)
      }));
    }
  },
  mounted() {
    this.loadVariables();
    this.loadData();
  },
  methods: {
    async loadVariables() {
      try {
        const { data } = await api.get(`/wells/${this.wellUid}/statistics/variables`);
        this.availableVariables = data;
      } catch (e) {
        this.availableVariables = [];
      }
    },
    async loadData() {
      this.loading = true;
      this.error = null;
      try {
        const body = {
          indexType: this.indexType,
          variables: this.selectedVariables.length ? this.selectedVariables : ['md', 'tvd', 'incl', 'azi']
        };
        if (this.indexType === 'depth') {
          if (this.minDepth != null) body.minDepth = this.minDepth;
          if (this.maxDepth != null) body.maxDepth = this.maxDepth;
        } else {
          if (this.minTime) body.minTime = this.minTime;
          if (this.maxTime) body.maxTime = this.maxTime;
        }
        const { data } = await api.post(`/wells/${this.wellUid}/statistics/data`, body);
        this.rows = data.rows;
        this.columns = data.columns;
      } catch (e) {
        this.error = e.response?.data?.detail || e.message || 'Error al cargar datos';
        this.rows = [];
        this.columns = [];
      } finally {
        this.loading = false;
      }
    },
    computeStat(stat) {
      const result = {};
      for (const col of this.columns) {
        const vals = this.rows.map(r => r[col]).filter(v => v != null && typeof v === 'number');
        if (vals.length === 0) {
          result[col] = '–';
          continue;
        }
        switch (stat) {
          case 'count': result[col] = vals.length; break;
          case 'sum': result[col] = vals.reduce((a, b) => a + b, 0).toFixed(4); break;
          case 'min': result[col] = Math.min(...vals).toFixed(4); break;
          case 'max': result[col] = Math.max(...vals).toFixed(4); break;
          case 'mean':
            result[col] = (vals.reduce((a, b) => a + b, 0) / vals.length).toFixed(4);
            break;
          case 'stddev': {
            const mean = vals.reduce((a, b) => a + b, 0) / vals.length;
            const sq = vals.reduce((acc, v) => acc + (v - mean) ** 2, 0);
            result[col] = (Math.sqrt(sq / vals.length) || 0).toFixed(4);
            break;
          }
          default: result[col] = '–';
        }
      }
      return result;
    },
    addStat(stat) {
      if (this.activeStats.includes(stat)) return;
      if (this.activeStats.length >= 3) this.activeStats.shift();
      this.activeStats.push(stat);
    },
    sortBy(col) {
      if (this.sortColumn === col) this.sortAsc = !this.sortAsc;
      else {
        this.sortColumn = col;
        this.sortAsc = true;
      }
    },
    formatVal(v) {
      if (v == null) return '–';
      if (typeof v === 'number') return Number.isInteger(v) ? v : v.toFixed(4);
      if (v instanceof Date || (typeof v === 'string' && v.match(/^\d{4}/))) {
        const d = new Date(v);
        return isNaN(d.getTime()) ? v : d.toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' });
      }
      return String(v);
    },
    variableCaption(mnemonic) {
      return this.availableVariables.find(v => v.mnemonic === mnemonic)?.caption || mnemonic;
    }
  }
};
</script>

<style scoped>
.data-statistics { padding: 0.5rem 0; }
.variable-list { max-height: 200px; overflow-y: auto; }
/* Contenedor con altura fija para forzar scroll en el grid */
.stats-grid-wrapper {
  height: 420px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid #dee2e6;
  border-radius: 4px;
}
.summary-grid { flex-shrink: 0; }
.data-grid-wrapper {
  flex: 1;
  min-height: 0;
  overflow: auto;
  -webkit-overflow-scrolling: touch;
}
.data-grid-wrapper .table { margin-bottom: 0; }
.sortable { cursor: pointer; user-select: none; }
.sortable:hover { background-color: #495057 !important; }
</style>
