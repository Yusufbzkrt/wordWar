import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App';
import { AuthProvider } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import { AudioProvider } from './contexts/AudioContext';
import './i18n';

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <AuthProvider>
      <ToastProvider>
        <AudioProvider>
          <App />
        </AudioProvider>
      </ToastProvider>
    </AuthProvider>
  </StrictMode>
);
