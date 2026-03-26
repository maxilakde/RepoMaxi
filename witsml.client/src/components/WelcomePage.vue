<template>
  <div class="welcome-page container-fluid">
    <h2 class="mb-3">Página de Bienvenida</h2>
    <p class="text-muted mb-4">Pozos accesibles – Vista rápida de las últimas 24 horas</p>

    <ul class="nav nav-tabs" role="tablist">
      <li class="nav-item">
        <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#well-list" type="button">Lista de Pozos</button>
      </li>
      <li class="nav-item">
        <button class="nav-link" data-bs-toggle="tab" data-bs-target="#rig-activity" type="button">Vista Rápida Rig</button>
      </li>
      <li class="nav-item">
        <button class="nav-link" data-bs-toggle="tab" data-bs-target="#gis-view" type="button">Vista GIS</button>
      </li>
    </ul>

    <div class="tab-content mt-3">
      <div id="well-list" class="tab-pane fade show active" role="tabpanel">
        <div class="well-list-toolbar mb-2 d-flex justify-content-between align-items-center flex-wrap gap-2">
          <div>
            <label class="me-2">Actualizar cada:</label>
            <select v-model="refreshInterval" class="form-select form-select-sm d-inline-block w-auto">
              <option value="10">10 seg</option>
              <option value="30">30 seg</option>
              <option value="60">1 min</option>
              <option value="300">5 min</option>
              <option value="600">10 min</option>
            </select>
          </div>
        </div>
        <div v-if="loading" class="alert alert-info">Cargando...</div>
        <div v-else-if="error" class="alert alert-danger">{{ error }}</div>
        <div v-else-if="wells.length === 0" class="alert alert-info">
          No hay pozos cargados. Procese archivos WITSML para cargar datos.
        </div>
        <div v-else>
          <div class="table-responsive">
            <table class="table table-sm table-hover table-striped wells-grid">
              <thead class="table-dark">
                <tr>
                  <th style="width: 2rem"></th>
                  <th>Nombre Rig <input v-model="filterRig" class="filter-input" placeholder="Filtrar" /></th>
                  <th>Nombre Pozo <input v-model="filterName" class="filter-input" placeholder="Filtrar" /></th>
                  <th>Empresa <input v-model="filterCompany" class="filter-input" placeholder="Filtrar" /></th>
                  <th>Prof. Pozo</th>
                  <th>Prof. Perf.</th>
                  <th>Actividad actual <input v-model="filterActivity" class="filter-input" placeholder="Filtrar" /></th>
                  <th>Último dato recibido</th>
                  <th>Estado <input v-model="filterStatus" class="filter-input" placeholder="Filtrar" /></th>
                  <th>Notificaciones</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                <template v-for="well in filteredWells" :key="well.uid">
                  <tr
                    class="well-row"
                    @dblclick="openWellPage(well)"
                  >
                    <td>
                      <button type="button" class="btn btn-sm btn-outline-secondary p-0 px-1" @click.stop="toggleExpand(well.uid)" title="Ver más">
                        {{ expandedWell === well.uid ? '−' : '+' }}
                      </button>
                    </td>
                    <td>{{ well.rig?.name || '–' }}</td>
                    <td><strong>{{ well.name || well.uid }}</strong></td>
                    <td>{{ well.rig?.owner || '–' }}</td>
                    <td>{{ well.holeDepth ?? '–' }}</td>
                    <td>{{ well.drillDepth ?? '–' }}</td>
                    <td>{{ well.currentActivity || '–' }}</td>
                    <td>{{ formatDate(well.processedAt) }}</td>
                    <td>
                      <span :class="['badge', statusClass(well.statusWell)]">{{ formatStatus(well.statusWell) }}</span>
                    </td>
                    <td><span class="badge bg-secondary">–</span></td>
                    <td>
                      <div class="quick-links">
                        <a href="#" @click.prevent="openWellPage(well, 'alarms')" class="link">Alarmas</a>
                        <a href="#" @click.prevent="openWellPage(well, 'document-manager')" class="link">Documentos</a>
                        <a href="#" @click.prevent="openWellPage(well, 'export')" class="link">Exportar</a>
                      </div>
                    </td>
                  </tr>
                  <tr v-if="expandedWell === well.uid" class="expanded-info">
                    <td colspan="11" class="bg-light py-3">
                      <div class="row">
                        <div class="col-md-4">
                          <strong>Actividad del Rig últimas 24 h</strong>
                          <p class="small text-muted mb-0">(Placeholder – gráfico circular)</p>
                        </div>
                        <div class="col-md-4">
                          <strong>Último survey</strong>
                          <p class="small text-muted mb-0">MD, inclinación, acimut, TVD (placeholders)</p>
                        </div>
                        <div class="col-md-4">
                          <strong>Accesos rápidos</strong>
                          <div class="quick-links mt-1">
                            <a href="#" @click.prevent="openWellPage(well)">Abrir Página del Pozo</a>
                          </div>
                        </div>
                      </div>
                    </td>
                  </tr>
                </template>
              </tbody>
            </table>
          </div>
          <p class="text-muted small mt-2">{{ filteredWells.length }} pozo(s) • Doble clic en la fila para abrir la Página del Pozo</p>
        </div>
      </div>

      <div id="rig-activity" class="tab-pane fade" role="tabpanel">
        <p class="text-muted">Vista gráfica de pozos activos – plantilla Realtime mosaico. (Placeholder)</p>
        <div class="alert alert-secondary">Doble clic en un pozo abrirá la Página del Pozo.</div>
      </div>

      <div id="gis-view" class="tab-pane fade" role="tabpanel">
        <p class="text-muted">Mapa mundial con etiquetas por pozo (GPS). (Placeholder)</p>
        <div class="alert alert-secondary">Doble clic en una etiqueta de pozo abrirá la Página del Pozo.</div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../api';

export default {
  name: 'WelcomePage',
  data() {
    return {
      wells: [],
      loading: true,
      error: null,
      refreshInterval: 60,
      filterName: '',
      filterRig: '',
      filterCompany: '',
      filterActivity: '',
      filterStatus: '',
      expandedWell: null,
      refreshTimer: null
    };
  },
  computed: {
    filteredWells() {
      let list = this.wells;
      if (this.filterName) {
        const q = this.filterName.toLowerCase();
        list = list.filter(w => (w.name || w.uid || '').toLowerCase().includes(q));
      }
      if (this.filterRig) {
        const q = this.filterRig.toLowerCase();
        list = list.filter(w => (w.rig?.name || '').toLowerCase().includes(q));
      }
      if (this.filterCompany) {
        const q = this.filterCompany.toLowerCase();
        list = list.filter(w => (w.rig?.owner || '').toLowerCase().includes(q));
      }
      if (this.filterActivity) {
        const q = this.filterActivity.toLowerCase();
        list = list.filter(w => (w.currentActivity || '').toLowerCase().includes(q));
      }
      if (this.filterStatus) {
        const q = this.filterStatus.toLowerCase();
        list = list.filter(w => this.formatStatus(w.statusWell).toLowerCase().includes(q));
      }
      return list;
    }
  },
  mounted() {
    this.loadWells();
    this.refreshTimer = setInterval(() => this.loadWells(), this.refreshInterval * 1000);
  },
  beforeUnmount() {
    if (this.refreshTimer) clearInterval(this.refreshTimer);
  },
  methods: {
    async loadWells() {
      try {
        this.loading = true;
        this.error = null;
        const { data } = await api.get('/wells');
        this.wells = data;
      } catch (e) {
        this.error = e.response?.data?.detail || e.message || 'Error al cargar pozos';
      } finally {
        this.loading = false;
      }
    },
    formatDate(d) {
      if (!d) return '–';
      const dt = new Date(d);
      return isNaN(dt.getTime()) ? '–' : dt.toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' });
    },
    formatStatus(s) {
      if (!s) return '–';
      const v = (s || '').toLowerCase();
      if (v.includes('active')) return 'Activo';
      if (v.includes('inactive') || v.includes('abandoned') || v.includes('plugged')) return 'Inactivo';
      return s;
    },
    statusClass(s) {
      if (!s) return 'bg-secondary';
      const v = (s || '').toLowerCase();
      if (v.includes('active')) return 'bg-success';
      if (v.includes('inactive') || v.includes('abandoned') || v.includes('plugged')) return 'bg-secondary';
      return 'bg-info';
    },
    toggleExpand(uid) {
      this.expandedWell = this.expandedWell === uid ? null : uid;
    },
    openWellPage(well, module = null) {
      this.$router.push({ name: 'WellPage', params: { uid: well.uid }, query: module ? { open: module } : {} });
    }
  }
};
</script>

<style scoped>
.filter-input { font-size: 0.75rem; width: 100%; max-width: 100px; padding: 2px 6px; }
.quick-links { font-size: 0.85rem; }
.quick-links .link { margin-right: 0.5rem; }
.well-row { cursor: pointer; }
</style>
