import { API_BASE } from '../utils/constants';

const getHeaders = () => {
  const token = localStorage.getItem('token');
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
};

const handleResponse = async (res) => {
  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: 'Bir hata oluştu' }));
    throw new Error(err.message || `HTTP ${res.status}`);
  }
  return res.json();
};

export const api = {
  get: (path) => fetch(`${API_BASE}/api${path}`, { headers: getHeaders() }).then(handleResponse),
  post: (path, body) => fetch(`${API_BASE}/api${path}`, { method: 'POST', headers: getHeaders(), body: JSON.stringify(body) }).then(handleResponse),
  put: (path, body) => fetch(`${API_BASE}/api${path}`, { method: 'PUT', headers: getHeaders(), body: JSON.stringify(body) }).then(handleResponse),
  delete: (path) => fetch(`${API_BASE}/api${path}`, { method: 'DELETE', headers: getHeaders() }).then(handleResponse),
};
