import axios from 'axios';

const api = axios.create({
  baseURL: '/api/v1',
  headers: { 'Content-Type': 'application/json' }
});

api.interceptors.request.use((config) => {
  const key = typeof localStorage !== 'undefined' ? (localStorage.getItem('witsmlSubscriberKey') || 'default') : 'default';
  config.headers['X-Subscriber-Key'] = key;
  return config;
});

export default api;
