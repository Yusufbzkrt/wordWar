import { useState, useEffect } from 'react';
import { api } from '../services/api';

export default function LeaderboardPage() {
  const [tab, setTab] = useState('global');
  const [globalData, setGlobalData] = useState([]);
  const [friendsData, setFriendsData] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      api.get('/leaderboard/global').catch(() => []),
      api.get('/leaderboard/friends').catch(() => [])
    ]).then(([g, f]) => { setGlobalData(g); setFriendsData(f); }).finally(() => setLoading(false));
  }, []);

  const data = tab === 'global' ? globalData : friendsData;
  const medals = ['🥇', '🥈', '🥉'];

  if (loading) return <div className="loading-screen"><div className="spinner" /><p style={{ color: 'var(--text-secondary)' }}>Yükleniyor...</p></div>;

  return (
    <div className="page">
      <h1 className="page-title">🏆 Liderlik Tablosu</h1>

      <div style={{ display: 'flex', gap: '4px', marginBottom: '20px', background: 'var(--bg-secondary)', borderRadius: 'var(--radius-md)', padding: '4px' }}>
        <button className={`btn btn-sm btn-full ${tab === 'global' ? 'btn-primary' : 'btn-ghost'}`} onClick={() => setTab('global')} style={{ flex: 1, border: 'none' }}>🌍 Global</button>
        <button className={`btn btn-sm btn-full ${tab === 'friends' ? 'btn-primary' : 'btn-ghost'}`} onClick={() => setTab('friends')} style={{ flex: 1, border: 'none' }}>👥 Arkadaşlar</button>
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
        {data.length === 0 && <p style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '40px 0' }}>Henüz veri yok</p>}
        {data.map((entry, i) => (
          <div key={entry.userId} className="card" style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '14px', background: i < 3 ? 'linear-gradient(135deg, rgba(245,158,11,0.08), transparent)' : undefined, borderColor: i < 3 ? 'rgba(245,158,11,0.2)' : undefined }}>
            <div style={{ width: 32, textAlign: 'center', fontWeight: 900, fontSize: i < 3 ? '1.2rem' : '0.9rem', color: i < 3 ? 'var(--gold)' : 'var(--text-muted)' }}>
              {i < 3 ? medals[i] : entry.rank}
            </div>
            <div style={{ width: 36, height: 36, borderRadius: '50%', background: i < 3 ? 'linear-gradient(135deg, var(--gold), #D97706)' : 'var(--bg-hover)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700, fontSize: '0.85rem', color: i < 3 ? '#1a1a1a' : 'var(--text-primary)' }}>
              {entry.username[0]?.toUpperCase()}
            </div>
            <div style={{ flex: 1 }}>
              <div style={{ fontWeight: 700, fontSize: '0.9rem' }}>{entry.username}</div>
            </div>
            <div style={{ fontWeight: 800, color: 'var(--correct)', fontSize: '0.95rem' }}>{entry.totalWins} 🏆</div>
          </div>
        ))}
      </div>
    </div>
  );
}
