<template>
  <div id="app">
    <header>
      <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
        <div class="container-fluid">
          <router-link to="/" class="navbar-brand">Visor de datos de Pozos</router-link>
        </div>
      </nav>
    </header>
    <main class="main-wrapper">
      <router-view />
    </main>
  </div>
</template>

<script>
export default { name: 'App' };
</script>

<style>
#app { font-family: 'Segoe UI', sans-serif; min-height: 100vh; background: #f5f5f5; }
.main-wrapper { background: #fff; padding: 1rem; border-radius: 4px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); min-height: calc(100vh - 56px); }
</style>
