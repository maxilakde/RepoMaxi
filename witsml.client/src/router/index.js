import { createRouter, createWebHistory } from 'vue-router';

const routes = [
  { path: '/', name: 'Welcome', component: () => import('../components/WelcomePage.vue') },
  { path: '/well/:uid', name: 'WellPage', component: () => import('../components/WellPage.vue'), props: true }
];

export default createRouter({ history: createWebHistory(), routes });
