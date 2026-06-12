import { useState } from 'react';
import { useAuth } from './contexts/AuthContext';
import LoginPage from './pages/LoginPage';
import LobbyPage from './pages/LobbyPage';
import ProfilePage from './pages/ProfilePage';
import StorePage from './pages/StorePage';
import FriendsPage from './pages/FriendsPage';
import LeaderboardPage from './pages/LeaderboardPage';

function App() {
  const { user, loading } = useAuth();
  const [activePage, setActivePage] = useState('lobby');

  if (loading) {
    return (
      <div className="loading-screen">
        <div className="spinner" style={{ width: 48, height: 48, borderWidth: 4 }} />
        <p style={{ color: 'var(--text-secondary)', fontWeight: 600 }}>Yükleniyor...</p>
      </div>
    );
  }

  if (!user) return <LoginPage />;

  const renderPage = () => {
    switch (activePage) {
      case 'lobby': return <LobbyPage />;
      case 'leaderboard': return <LeaderboardPage />;
      case 'store': return <StorePage />;
      case 'friends': return <FriendsPage />;
      case 'profile': return <ProfilePage />;
      default: return <LobbyPage />;
    }
  };

  return (
    <>
      {renderPage()}

      {/* Bottom Navigation */}
      <nav className="bottom-nav">
        <button className={`nav-item ${activePage === 'lobby' ? 'active' : ''}`} onClick={() => setActivePage('lobby')}>
          <span className="nav-icon">🎮</span>
          <span>Oyun</span>
        </button>
        <button className={`nav-item ${activePage === 'leaderboard' ? 'active' : ''}`} onClick={() => setActivePage('leaderboard')}>
          <span className="nav-icon">🏆</span>
          <span>Sıralama</span>
        </button>
        <button className={`nav-item ${activePage === 'store' ? 'active' : ''}`} onClick={() => setActivePage('store')}>
          <span className="nav-icon">🛒</span>
          <span>Market</span>
        </button>
        <button className={`nav-item ${activePage === 'friends' ? 'active' : ''}`} onClick={() => setActivePage('friends')}>
          <span className="nav-icon">👥</span>
          <span>Arkadaşlar</span>
        </button>
        <button className={`nav-item ${activePage === 'profile' ? 'active' : ''}`} onClick={() => setActivePage('profile')}>
          <span className="nav-icon">👤</span>
          <span>Profil</span>
        </button>
      </nav>
    </>
  );
}

export default App;
