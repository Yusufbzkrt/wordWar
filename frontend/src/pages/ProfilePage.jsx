import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';
import { api } from '../services/api';
import { GAME } from '../utils/constants';

export default function ProfilePage() {
  const { user, updateUser, logout } = useAuth();
  const { addToast } = useToast();
  const [editing, setEditing] = useState(false);
  const [newName, setNewName] = useState(user?.username || '');
  const [claiming, setClaiming] = useState(false);
  const [adTimeLeft, setAdTimeLeft] = useState(null);


  useEffect(() => {
    if (!user?.lastAdRewardTime) {
      setAdTimeLeft(null);
      return;
    }

    const calculateTimeLeft = () => {
      // Backend'den gelen tarih UTC olabilir, new Date(Z'li tarih) doğru parse eder.
      const lastWatch = new Date(user.lastAdRewardTime);
      const nextWatch = new Date(lastWatch.getTime() + (GAME.AD_COOLDOWN_HOURS || 2) * 60 * 60 * 1000);
      const now = new Date();
      const diff = nextWatch - now;

      if (diff <= 0) {
        return null;
      }

      const hours = Math.floor((diff / (1000 * 60 * 60)) % 24);
      const minutes = Math.floor((diff / 1000 / 60) % 60);
      const seconds = Math.floor((diff / 1000) % 60);

      return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    };

    setAdTimeLeft(calculateTimeLeft());
    const timer = setInterval(() => {
      setAdTimeLeft(calculateTimeLeft());
    }, 1000);

    return () => clearInterval(timer);
  }, [user?.lastAdRewardTime]);

  const handleUpdateName = async () => {
    try {
      await api.put('/profile/username', newName);
      updateUser({ username: newName });
      setEditing(false);
      addToast('İsim güncellendi!', 'success');
    } catch (err) { addToast(err.message, 'error'); }
  };

  const handleClaimAd = async () => {
    setClaiming(true);
    try {
      await api.post('/profile/ad-reward');
      // Kullanıcının elmasını artır ve son izleme zamanını şu an olarak güncelle
      updateUser({ 
        diamonds: (user?.diamonds || 0) + 3,
        lastAdRewardTime: new Date().toISOString()
      });
      addToast('3 Elmas kazandınız! 💎', 'success');
    } catch (err) { addToast(err.message, 'error'); }
    finally { setClaiming(false); }
  };

  const winRate = user?.totalWins + user?.totalLosses > 0
    ? Math.round((user.totalWins / (user.totalWins + user.totalLosses)) * 100)
    : 0;

  return (
    <div className="page">
      <h1 className="page-title">👤 Profil</h1>

      {/* Avatar & İsim */}
      <div className="card" style={{ textAlign: 'center', padding: '28px 20px', marginBottom: '16px' }}>
        <div style={{ width: 80, height: 80, borderRadius: '50%', background: 'linear-gradient(135deg, var(--accent), var(--accent-pink))', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '2rem', margin: '0 auto 16px', boxShadow: '0 0 24px var(--accent-glow)' }}>
          {(user?.username || '?')[0]?.toUpperCase()}
        </div>
        {!editing ? (
          <div>
            <h2 style={{ fontSize: '1.3rem', fontWeight: 800 }}>{user?.username}</h2>
            <button className="btn btn-ghost btn-sm" onClick={() => setEditing(true)} style={{ marginTop: '8px' }}>✏️ Düzenle</button>
          </div>
        ) : (
          <div style={{ display: 'flex', gap: '8px', marginTop: '8px' }}>
            <input className="input" value={newName} onChange={e => setNewName(e.target.value)} style={{ flex: 1 }} />
            <button className="btn btn-success btn-sm" onClick={handleUpdateName}>✓</button>
            <button className="btn btn-ghost btn-sm" onClick={() => setEditing(false)}>✗</button>
          </div>
        )}
      </div>

      {/* Bakiye */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px', marginBottom: '16px' }}>
        <div className="card" style={{ textAlign: 'center', padding: '20px', background: 'linear-gradient(135deg, rgba(245,158,11,0.1), rgba(245,158,11,0.05))', borderColor: 'rgba(245,158,11,0.2)' }}>
          <div style={{ fontSize: '2rem', fontWeight: 900, color: 'var(--gold)' }}>🪙 {user?.gold || 0}</div>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', fontWeight: 600 }}>Altın</div>
        </div>
        <div className="card" style={{ textAlign: 'center', padding: '20px', background: 'linear-gradient(135deg, rgba(6,182,212,0.1), rgba(6,182,212,0.05))', borderColor: 'rgba(6,182,212,0.2)' }}>
          <div style={{ fontSize: '2rem', fontWeight: 900, color: 'var(--diamond)' }}>💎 {user?.diamonds || 0}</div>
          <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', fontWeight: 600 }}>Elmas</div>
        </div>
      </div>

      {/* İstatistikler */}
      <div className="card" style={{ marginBottom: '16px' }}>
        <h3 style={{ fontSize: '0.9rem', fontWeight: 700, marginBottom: '14px' }}>📊 İstatistikler</h3>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Toplam Maç</span>
            <span style={{ fontWeight: 700 }}>{(user?.totalWins || 0) + (user?.totalLosses || 0)}</span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Kazanma Oranı</span>
            <span style={{ fontWeight: 700, color: winRate >= 50 ? 'var(--correct)' : 'var(--wrong)' }}>%{winRate}</span>
          </div>
        </div>
      </div>

      {/* Reklam Ödülü */}
      <div className="card" style={{ marginBottom: '16px', background: 'linear-gradient(135deg, rgba(6,182,212,0.1), rgba(139,92,246,0.1))', borderColor: 'rgba(6,182,212,0.2)', opacity: adTimeLeft ? 0.8 : 1, transition: 'all 0.3s' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <div style={{ fontSize: '2rem', filter: adTimeLeft ? 'grayscale(1)' : 'none' }}>🎬</div>
          <div style={{ flex: 1 }}>
            <h3 style={{ fontSize: '0.9rem', fontWeight: 700 }}>Video İzle, Elmas Kazan!</h3>
            <p style={{ fontSize: '0.75rem', color: adTimeLeft ? 'var(--accent-pink)' : 'var(--correct)', fontWeight: 700, marginTop: '2px' }}>
              {adTimeLeft ? `Sıradaki video: ${adTimeLeft}` : 'Hemen izle, 3 elmas kap!'}
            </p>
          </div>
          <button className="btn btn-diamond btn-sm" onClick={handleClaimAd} disabled={claiming || adTimeLeft !== null} style={{ opacity: adTimeLeft ? 0.5 : 1 }}>
            {claiming ? '...' : (adTimeLeft ? '⏳ Bekle' : '▶️ İzle')}
          </button>
        </div>
      </div>

      {/* Çıkış */}
      <button className="btn btn-danger btn-full" onClick={logout}>🚪 Çıkış Yap</button>
    </div>
  );
}
