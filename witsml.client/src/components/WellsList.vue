<template>
  <div>
    <h1 class="mb-4">Visor de datos WITSML</h1>
    <p class="text-muted">Pozos cargados en la base de datos WitsmlData</p>

    <div v-if="loading" class="alert alert-info">Cargando...</div>
    <div v-else-if="error" class="alert alert-danger">{{ error }}</div>
    <div v-else-if="wells.length === 0" class="alert alert-info">
      No hay pozos cargados. Procese archivos WITSML con <code>BochazoEtpWitsml</code> para cargar datos.
    </div>
    <div v-else>
      <div class="table-responsive">
        <table class="table table-striped table-hover">
          <thead class="table-dark">
            <tr>
              <th>UID</th>
              <th>Nombre</th>
              <th>Zona horaria</th>
              <th>Estado</th>
              <th>Creación</th>
              <th>Última modificación</th>
              <th>Procesado</th>
              <th>Archivo origen</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="well in wells" :key="well.uid">
              <td><code>{{ well.uid }}</code></td>
              <td>{{ well.name }}</td>
              <td>{{ well.timeZone }}</td>
              <td>{{ well.statusWell }}</td>
              <td>{{ formatDate(well.dTimCreation) }}</td>
              <td>{{ formatDate(well.dTimLastChange) }}</td>
              <td>{{ formatDate(well.processedAt) }}</td>
              <td class="small text-truncate" style="max-width: 200px" :title="well.sourceFile">{{ well.sourceFile }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <p class="text-muted small">{{ wells.length }} pozo(s) en total</p>
    </div>
  </div>
</template>

<script>
import api from '../api';

export default {
  name: 'WellsList',
  data() {
    return { wells: [], loading: true, error: null };
  },
  mounted() {
    this.loadWells();
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
      if (!d) return '-';
      const dt = new Date(d);
      return isNaN(dt.getTime()) ? '-' : dt.toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' });
    }
  }
};
</script>
