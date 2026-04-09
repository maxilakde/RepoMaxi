<template>
  <div class="well-page">
    <div class="well-page-layout">
      <!-- Navigation Pane (dockable) -->
      <aside
        class="nav-pane"
        :class="{ collapsed: navCollapsed }"
      >
        <div class="nav-pane-header d-flex justify-content-between align-items-center">
          <span v-if="!navCollapsed">Navegación</span>
          <button class="btn btn-sm btn-outline-secondary" @click="navCollapsed = !navCollapsed" title="Expandir/Colapsar">
            {{ navCollapsed ? '›' : '‹' }}
          </button>
        </div>
        <div v-if="!navCollapsed" class="nav-tree">
          <ul class="list-unstyled mb-0">
            <li><router-link to="/" class="nav-link">◀ Página de Bienvenida</router-link></li>
            <li class="nav-node parent"><span>{{ wellName || 'Pozo' }} – {{ wellUid || '–' }}</span></li>
            <li class="nav-node"><a href="#" @click.prevent="openModule('data-analysis')" class="nav-link">Análisis de datos</a></li>
            <li class="nav-node"><a href="#" @click.prevent="openModule('surveys')" class="nav-link">Desvío</a></li>
            <li class="nav-node"><a href="#" @click.prevent="openModule('statistics')" class="nav-link">Estadísticas</a></li>
            <li class="nav-node"><a href="#" @click.prevent="openModule('alarms')" class="nav-link">Alarmas</a></li>
            <li class="nav-node"><a href="#" @click.prevent="openModule('export')" class="nav-link">Exportar</a></li>
            <li class="nav-node"><a href="#" @click.prevent="openModule('document-manager')" class="nav-link">Gestor de documentos</a></li>
          </ul>
        </div>
      </aside>

      <!-- Main content (tabbed) -->
      <main class="main-content">
        <div class="tabs-header">
          <ul class="nav nav-pills nav-tabs-custom">
            <li v-for="tab in openTabs" :key="tab.id" class="nav-item">
              <a
                href="#"
                class="nav-link"
                :class="{ active: activeTab === tab.id }"
                @click.prevent="activeTab = tab.id"
              >
                {{ tab.title }}
                <button v-if="tab.closable" class="tab-close" @click.stop="closeTab(tab.id)">×</button>
              </a>
            </li>
          </ul>
        </div>
        <div class="tabs-body">
          <div v-for="tab in openTabs" :key="tab.id" v-show="activeTab === tab.id" class="tab-panel">
            <DataStatistics v-if="tab.component === 'DataStatistics'" :well-uid="wellUid || ''" />
            <WellAlarms v-else-if="tab.component === 'WellAlarms'" :well-uid="wellUid || ''" />
            <PlaceholderContent v-else :message="tab.props?.message || 'Módulo en desarrollo.'" />
          </div>
        </div>
      </main>
    </div>
    <AlarmNotifications v-if="wellUid" :well-uid="wellUid" />
  </div>
</template>

<script>
import api from '../api';
import DataStatistics from './DataStatistics.vue';
import WellAlarms from './WellAlarms.vue';
import AlarmNotifications from './AlarmNotifications.vue';

const PlaceholderContent = {
  template: '<div class="p-4 text-muted"><p>{{ message }}</p></div>',
  props: { message: { type: String, default: 'Módulo en desarrollo.' } }
};

export default {
  name: 'WellPage',
  components: { PlaceholderContent, DataStatistics, WellAlarms, AlarmNotifications },
  data() {
    return {
      well: null,
      wellName: '',
      wellUid: '',
      navCollapsed: false,
      openTabs: [],
      activeTab: null,
      tabIdCounter: 0,
    };
  },
  watch: {
    '$route.params.uid'() {
      this.loadWell();
    },
    '$route.query.open'(v) {
      if (v) this.openModule(v);
    }
  },
  mounted() {
    this.loadWell();
    if (this.$route.query.open) {
      this.$nextTick(() => this.openModule(this.$route.query.open));
    } else {
      this.addTab('welcome-tab', 'Pozo', 'placeholder', { message: 'Seleccione un módulo en el panel de navegación.' }, false);
    }
  },
  methods: {
    async loadWell() {
      const uid = this.$route.params.uid;
      if (!uid) return;
      // El UID debe estar disponible de inmediato para pestañas que cargan en paralelo (p. ej. ?open=alarms).
      // Si solo se asigna tras await /wells, WellAlarms pide /wells//alarms y la lista sale vacía o falla.
      this.wellUid = uid;
      this.wellName = uid;
      try {
        const { data } = await api.get('/wells');
        this.well = data.find(w => w.uid === uid) || null;
        this.wellName = this.well?.name || uid;
      } catch {
        this.well = null;
        this.wellName = uid;
      }
    },
    addTab(id, title, component, props = {}, closable = true) {
      if (this.openTabs.some(t => t.id === id)) {
        this.activeTab = id;
        return;
      }
      const tab = { id, title, component, props, closable };
      this.openTabs.push(tab);
      this.activeTab = id;
    },
    closeTab(id) {
      const idx = this.openTabs.findIndex(t => t.id === id);
      if (idx < 0) return;
      this.openTabs.splice(idx, 1);
      if (this.activeTab === id && this.openTabs.length) {
        this.activeTab = this.openTabs[0].id;
      }
    },
    openModule(module) {
      const map = {
        'data-analysis': ['data-analysis', 'Análisis de datos', 'Log Analysis, gráfico XY', 'placeholder'],
        surveys: ['surveys', 'Desvío', 'Informes de desvío', 'placeholder'],
        statistics: ['statistics', 'Estadísticas de datos', 'Variables, grid, estadísticas', 'DataStatistics'],
        alarms: ['alarms', 'Alarmas', 'Umbrales, historial y notificaciones', 'WellAlarms'],
        export: ['export', 'Exportar', 'Descarga de datos', 'placeholder'],
        'document-manager': ['document-manager', 'Gestor de documentos', 'Gestión de documentos', 'placeholder']
      };
      const [id, title, msg, component] = map[module] || [module, module, 'Módulo en desarrollo.', 'placeholder'];
      this.addTab(id, title, component, { message: msg });
    }
  }
};
</script>

<style scoped>
.well-page { height: calc(100vh - 80px); min-height: 450px; margin: -1rem; padding: 0; }
.well-page-layout {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 0;
  height: 100%;
  background: #fff;
  border-radius: 4px;
  overflow: hidden;
}

.nav-pane {
  width: 240px;
  min-width: 240px;
  background: #f8f9fa;
  border-right: 1px solid #dee2e6;
  display: flex;
  flex-direction: column;
  transition: width 0.2s, min-width 0.2s;
}
.nav-pane.collapsed { width: 48px; min-width: 48px; }
.nav-pane.collapsed .nav-tree { display: none; }

.nav-pane-header {
  padding: 0.5rem 0.75rem;
  border-bottom: 1px solid #dee2e6;
  background: #e9ecef;
}

.nav-tree { padding: 0.5rem; overflow-y: auto; flex: 1; }
.nav-tree .nav-link { padding: 0.25rem 0; display: block; text-decoration: none; color: #212529; }
.nav-tree .nav-link:hover { color: #0d6efd; }
.nav-node { padding: 0.2rem 0; font-size: 0.9rem; }
.nav-node.parent { color: #6c757d; font-weight: 600; }

.main-content {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 0;
}

.tabs-header {
  border-bottom: 1px solid #dee2e6;
  background: #f8f9fa;
}
.nav-tabs-custom { padding: 0.25rem 0.5rem 0 0.5rem; }
.nav-tabs-custom .nav-link {
  border-radius: 4px 4px 0 0;
  padding: 0.35rem 0.75rem;
  margin-right: 2px;
  border: 1px solid transparent;
  background: #e9ecef;
  color: #495057;
  text-decoration: none;
}
.nav-tabs-custom .nav-link.active { background: #fff; border-color: #dee2e6; border-bottom-color: #fff; }
.tab-close { margin-left: 0.25rem; background: none; border: none; cursor: pointer; font-size: 1.1rem; }

.tabs-body { flex: 1; overflow: auto; padding: 1rem; }
</style>
