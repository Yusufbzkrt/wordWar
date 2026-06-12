import { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useToast } from '../contexts/ToastContext';

export default function FriendsPage() {
  const { addToast } = useToast();
  const [tab, setTab] = useState('friends');
  const [friends, setFriends] = useState([]);
  const [pending, setPending] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [f, p] = await Promise.all([api.get('/friend'), api.get('/friend/pending')]);
      setFriends(f);
      setPending(p);
    } catch {} finally { setLoading(false); }
  };

  const handleAccept = async (id) => {
    try {
      await api.post(`/friend/accept/${id}`);
      addToast('Arkadaşlık kabul edildi!', 'success');
      loadData();
    } catch (err) { addToast(err.message, 'error'); }
  };

  const handleReject = async (id) => {
    try {
      await api.post(`/friend/reject/${id}`);
      loadData();
    } catch (err) { addToast(err.message, 'error'); }
  };

  const handleRemove = async (id) => {
    try {
      await api.delete(`/friend/${id}`);
      addToast('Arkadaşlıktan çıkarıldı', 'info');
      loadData();
    } catch (err) { addToast(err.message, 'error'); }
  };

  if (loading) return <div className="loading-screen"><div className="spinner" /><p style={{ color: 'var(--text-secondary)' }}>Yükleniyor...</p></div>;

  return (
    <div className="page">
      <h1 className="page-title">👥 Arkadaşlar</h1>

      {/* Tab Bar */}
      <div style={{ display: 'flex', gap: '4px', marginBottom: '20px', background: 'var(--bg-secondary)', borderRadius: 'var(--radius-md)', padding: '4px' }}>
        <button className={`btn btn-sm btn-full ${tab === 'friends' ? 'btn-primary' : 'btn-ghost'}`} onClick={() => setTab('friends')} style={{ flex: 1, border: 'none' }}>Arkadaşlar ({friends.length})</button>
        <button className={`btn btn-sm btn-full ${tab === 'pending' ? 'btn-primary' : 'btn-ghost'}`} onClick={() => setTab('pending')} style={{ flex: 1, border: 'none' }}>İstekler ({pending.length})</button>
      </div>

      {tab === 'friends' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
          {friends.length === 0 && <p style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '40px 0' }}>Henüz arkadaşınız yok</p>}
          {friends.map(f => (
            <div key={f.friendshipId} className="card" style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '14px' }}>
              <div style={{ width: 40, height: 40, borderRadius: '50%', background: 'var(--accent)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700 }}>{f.friendName[0]?.toUpperCase()}</div>
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 700, fontSize: '0.9rem' }}>{f.friendName}</div>
                <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>{f.friendWins} galibiyet</div>
              </div>
              <button className="btn btn-ghost btn-sm" onClick={() => handleRemove(f.friendshipId)} style={{ color: 'var(--wrong)', fontSize: '0.7rem' }}>Çıkar</button>
            </div>
          ))}
        </div>
      )}

      {tab === 'pending' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
          {pending.length === 0 && <p style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '40px 0' }}>Bekleyen istek yok</p>}
          {pending.map(p => (
            <div key={p.friendshipId} className="card" style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '14px' }}>
              <div style={{ width: 40, height: 40, borderRadius: '50%', background: 'var(--gold)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700, color: '#1a1a1a' }}>{p.friendName[0]?.toUpperCase()}</div>
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 700, fontSize: '0.9rem' }}>{p.friendName}</div>
              </div>
              <div style={{ display: 'flex', gap: '6px' }}>
                <button className="btn btn-success btn-sm" onClick={() => handleAccept(p.friendshipId)}>✓</button>
                <button className="btn btn-ghost btn-sm" onClick={() => handleReject(p.friendshipId)}>✗</button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
