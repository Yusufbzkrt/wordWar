import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';
import { useGameConnection } from '../hooks/useGameConnection';
import { api } from '../services/api';
import GamePage from './GamePage';

const ADJECTIVES = ["Dark", "Shadow", "Pro", "Neon", "Cyber", "Fast", "Crazy", "Epic", "Ghost", "Alpha", "Savage", "Silent", "Iron", "Venom", "Deadly", "Mystic", "Turbo", "Cosmic", "Toxic", "Swift", "Mad", "Ice", "Fire", "Elite", "Prime", "Deli", "Karanlik", "Yenilmez", "Bordo", "Mavi", "Kizil", "Efsane", "Genc", "Kral", "Usta", "Cesur", "Gizli", "Hizli", "Zehir"];
const NOUNS = ["Hunter", "Gamer", "Slayer", "Wolf", "Ninja", "Blade", "King", "Queen", "Knight", "Master", "Sniper", "Dragon", "Wizard", "Rogue", "Walker", "Striker", "Phantom", "Beast", "Fox", "Viper", "Hawk", "Eagle", "Storm", "Thunder", "Coder", "Ahmet", "Mehmet", "Zeynep", "Elif", "Bora", "Kaan", "Can", "Kerem", "Yusuf", "Ayse", "Fatma", "Efe", "Canavar", "Avci", "Kartal", "Kurt"];

function generateRandomNick() {
  const adj = ADJECTIVES[Math.floor(Math.random() * ADJECTIVES.length)];
  const noun = NOUNS[Math.floor(Math.random() * NOUNS.length)];
  const num = Math.random() > 0.5 ? Math.floor(Math.random() * 90) + 10 : "";
  return `${adj}_${noun}${num}`;
}

function AvatarRoulette() {
  const [currentName, setCurrentName] = useState(generateRandomNick());
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentName(generateRandomNick());
    }, 80);
    return () => clearInterval(interval);
  }, []);

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: '12px', background: 'rgba(0,0,0,0.5)', padding: '12px 24px', borderRadius: 'var(--radius-full)', border: '1px solid var(--accent-glow)' }}>
      <div style={{ width: '28px', height: '28px', borderRadius: '50%', background: 'linear-gradient(135deg, var(--accent), var(--accent-cyan))', animation: 'pulse 0.5s infinite' }} />
      <span style={{ fontSize: '1.1rem', fontWeight: 900, color: 'var(--text-primary)', fontFamily: 'monospace', letterSpacing: '1px' }}>{currentName}</span>
    </div>
  );
}

export default function LobbyPage() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const { connection, connected, connect } = useGameConnection();
  const [searching, setSearching] = useState(false);
  const [gameState, setGameState] = useState(null);
  const [quests, setQuests] = useState([]);
  const [timeLeft, setTimeLeft] = useState('');

  useEffect(() => {
    const calculateTimeLeft = () => {
      const now = new Date();
      const tomorrow = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1);
      const diff = tomorrow - now;
      
      const hours = Math.floor((diff / (1000 * 60 * 60)) % 24);
      const minutes = Math.floor((diff / 1000 / 60) % 60);
      const seconds = Math.floor((diff / 1000) % 60);
      
      return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    };

    setTimeLeft(calculateTimeLeft());
    const timer = setInterval(() => {
      setTimeLeft(calculateTimeLeft());
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  useEffect(() => { connect(); }, [connect]);

  const fetchQuests = useCallback(async () => {
    try {
      const response = await api.get('/quests');
      setQuests(response || []);
    } catch (err) {
      console.error('Görevler yüklenirken hata oluştu:', err);
      setQuests([]);
    }
  }, []);

  useEffect(() => {
    fetchQuests();
  }, [fetchQuests]);

  const handleClaimReward = async (questId) => {
    try {
      const res = await api.post(`/quests/${questId}/claim`);
      addToast('Ödül başarıyla alındı! 🎉', 'success');
      setQuests(prev => prev.map(q => q.id === questId ? { ...q, isClaimed: true } : q));
      // AuthContext içindeki updateUser çağrılarak altın güncellenebilir 
      // User update is available via window refresh or explicit call if updateUser supports it
      setTimeout(() => window.location.reload(), 1000); // En garanti yol
    } catch (err) {
      addToast(err.response?.data || 'Ödül alınamadı', 'error');
    }
  };

  useEffect(() => {
    if (!connection) return;
    connection.on('MatchFound', (state) => {
      setSearching(false);
      setGameState(state);
      addToast('Rakip bulundu! Oyun başlıyor...', 'success');
    });
    connection.on('SearchingMatch', () => {
      addToast('Rakip aranıyor...', 'info');
    });
    connection.on('SearchCancelled', () => {
      setSearching(false);
    });
    connection.on('QuestCompleted', (questTitle) => {
      addToast(`🎉 GÖREV TAMAMLANDI: ${questTitle}! Lobiye dönerek ödülünü alabilirsin.`, 'success');
      fetchQuests(); // Update quests list behind the scenes
    });
    return () => {
      connection.off('MatchFound');
      connection.off('SearchingMatch');
      connection.off('SearchCancelled');
      connection.off('QuestCompleted');
    };
  }, [connection, addToast, fetchQuests]);

  const handleSearch = useCallback(async () => {
    if (!connection || !connected) {
      addToast('Bağlantı kurulamadı', 'error');
      return;
    }
    setSearching(true);
    try {
      await connection.invoke('SearchMatch');
    } catch {
      setSearching(false);
      addToast('Eşleşme başlatılamadı', 'error');
    }
  }, [connection, connected, addToast]);

  const handleCancel = useCallback(async () => {
    if (connection) {
      await connection.invoke('CancelSearch');
      setSearching(false);
    }
  }, [connection]);

  // Oyun başladıysa GamePage'e geç
  if (gameState) {
    return <GamePage gameState={gameState} connection={connection} onGameEnd={() => setGameState(null)} />;
  }

  const totalGames = (user?.totalWins || 0) + (user?.totalLosses || 0);
  const winRate = totalGames > 0 ? Math.round((user.totalWins / totalGames) * 100) : 0;

  const [localTokens, setLocalTokens] = useState(user?.tokens || 10);
  const [tokenTimer, setTokenTimer] = useState(null);

  useEffect(() => {
    if (!user) return;
    
    const calculateTokens = () => {
      // Backend'den veri gelmemişse veya eski sürümse varsayılan değerleri kullan
      const currentTokens = user.tokens !== undefined ? user.tokens : 10;
      
      if (currentTokens >= 10 || !user.lastTokenUpdateTime) {
        setLocalTokens(currentTokens >= 10 ? 10 : currentTokens);
        setTokenTimer(null);
        return;
      }
      
      const lastUpdate = new Date(user.lastTokenUpdateTime).getTime();
      const now = new Date().getTime();
      const diffMinutes = (now - lastUpdate) / 1000 / 60;
      
      const earned = Math.floor(diffMinutes / 7);
      const newTokens = Math.min(10, currentTokens + earned);
      
      setLocalTokens(newTokens);
      
      if (newTokens < 10) {
        const remainingSeconds = Math.floor((7 * 60) - (((now - lastUpdate) / 1000) % (7 * 60)));
        const m = Math.floor(remainingSeconds / 60).toString().padStart(2, '0');
        const s = (remainingSeconds % 60).toString().padStart(2, '0');
        setTokenTimer(`${m}:${s}`);
      } else {
        setTokenTimer(null);
      }
    };

    calculateTokens();
    const interval = setInterval(calculateTokens, 1000);
    return () => clearInterval(interval);
  }, [user]);

  return (
    <div className="page" style={{ paddingBottom: '100px' }}>
      {/* Üst Header Alanı */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '32px', paddingTop: '10px' }}>
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
          <div className="glass" style={{ padding: '8px 16px', borderRadius: 'var(--radius-full)', display: 'flex', alignItems: 'center', gap: '16px', border: '1px solid rgba(251, 191, 36, 0.4)', background: 'rgba(251, 191, 36, 0.1)' }}>
            <div style={{ display: 'flex', marginLeft: '6px' }}>
              {[...Array(10)].map((_, i) => (
                <div key={i} style={{
                  width: '8px',
                  height: '24px',
                  borderRadius: '3px',
                  marginLeft: '-4px', // Tırtıklı dizilim (overlap)
                  background: i < localTokens ? 'linear-gradient(to right, #f59e0b, #fcd34d 50%, #d97706)' : 'rgba(0,0,0,0.6)',
                  backgroundImage: i < localTokens ? 'repeating-linear-gradient(to bottom, transparent, transparent 2px, rgba(0,0,0,0.2) 2px, rgba(0,0,0,0.2) 4px)' : 'none',
                  border: i < localTokens ? '1px solid #d97706' : '1px solid rgba(255,255,255,0.1)',
                  boxShadow: i < localTokens ? '2px 0 4px rgba(0,0,0,0.3)' : 'none',
                  zIndex: 10 - i,
                  transition: 'all 0.3s'
                }} />
              ))}
            </div>
            <div style={{ display: 'flex', flexDirection: 'column' }}>
              <span style={{ fontWeight: 900, fontSize: '1rem', lineHeight: 1, color: 'white', textShadow: '0 0 10px rgba(251,191,36,0.5)' }}>{localTokens} / 10</span>
              {tokenTimer && <span style={{ fontSize: '0.7rem', color: '#fcd34d', fontWeight: 800, marginTop: '2px' }}>{tokenTimer}</span>}
            </div>
          </div>
        </div>
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
          <div className="glass" style={{ padding: '6px 12px', borderRadius: 'var(--radius-full)', display: 'flex', alignItems: 'center', gap: '6px', fontWeight: 800, fontSize: '0.85rem' }}>
            <span style={{ color: 'var(--gold)', textShadow: '0 0 10px var(--gold-glow)' }}>🪙</span> 
            {user?.gold || 0}
          </div>
          <div className="glass" style={{ padding: '6px 12px', borderRadius: 'var(--radius-full)', display: 'flex', alignItems: 'center', gap: '6px', fontWeight: 800, fontSize: '0.85rem' }}>
            <span style={{ color: 'var(--diamond)', textShadow: '0 0 10px var(--diamond-glow)' }}>💎</span> 
            {user?.diamonds || 0}
          </div>
        </div>
      </div>

      {/* Dev Hızlı Oyna Kartı */}
      <div className="glass" style={{ 
        position: 'relative', overflow: 'hidden', padding: '0', marginBottom: '20px', 
        border: '1px solid rgba(6, 182, 212, 0.3)', background: 'linear-gradient(135deg, rgba(11, 15, 25, 0.9), rgba(139, 92, 246, 0.15))',
        boxShadow: '0 0 40px rgba(139, 92, 246, 0.15)'
      }}>
        {/* Dekoratif Daireler */}
        <div style={{ position: 'absolute', top: '-20px', right: '-20px', width: '120px', height: '120px', background: 'var(--accent-glow)', borderRadius: '50%', animation: 'pulse 3s infinite', filter: 'blur(30px)' }}></div>
        <div style={{ position: 'absolute', bottom: '-40px', left: '-20px', width: '160px', height: '160px', background: 'var(--diamond-glow)', borderRadius: '50%', filter: 'blur(40px)' }}></div>
        
        <div style={{ padding: '40px 24px', textAlign: 'center', position: 'relative', zIndex: 2 }}>
          <div style={{ fontSize: '4.5rem', marginBottom: '16px', filter: 'drop-shadow(0 0 20px var(--accent-glow))' }}>⚔️</div>
          <h2 className="gradient-text" style={{ fontSize: '1.8rem', fontWeight: 900, marginBottom: '8px', letterSpacing: '1px', textTransform: 'uppercase' }}>Kelime Düellosu</h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginBottom: '32px', fontWeight: 500 }}>Gerçek zamanlı, 30 saniyelik arena.</p>
          
          {!searching ? (
            <button className="btn btn-lg btn-full" onClick={handleSearch} disabled={!connected || localTokens < 1} 
              style={{ background: localTokens < 1 ? 'rgba(255,255,255,0.05)' : 'var(--bg-primary)', color: localTokens < 1 ? 'var(--text-muted)' : 'var(--accent-cyan)', fontSize: '1.2rem', padding: '18px', boxShadow: localTokens < 1 ? 'none' : '0 8px 32px rgba(0,0,0,0.5)', borderRadius: 'var(--radius-xl)', border: localTokens < 1 ? '2px solid rgba(255,255,255,0.1)' : '2px solid rgba(6, 182, 212, 0.4)', textTransform: 'uppercase', letterSpacing: '2px', transition: 'all 0.3s' }}>
              {localTokens < 1 ? 'YETERLİ JETON YOK' : 'Rakip Bul (1 🪙)'}
            </button>
          ) : (
            <div style={{ background: 'rgba(11, 15, 25, 0.8)', padding: '24px', borderRadius: 'var(--radius-xl)', backdropFilter: 'blur(20px)', border: '1px solid rgba(6, 182, 212, 0.3)' }}>
              <p style={{ color: 'var(--accent-cyan)', fontWeight: 800, marginBottom: '16px', letterSpacing: '1px', fontSize: '0.85rem' }}>ARENAYA BAĞLANILIYOR...</p>
              
              <div style={{ marginBottom: '24px', display: 'flex', justifyContent: 'center' }}>
                <AvatarRoulette />
              </div>

              <button className="btn btn-sm btn-ghost" onClick={handleCancel} style={{ color: 'var(--text-muted)' }}>İptal Et</button>
            </div>
          )}
          {!connected && <p style={{ color: 'var(--wrong)', fontSize: '0.8rem', marginTop: '16px', fontWeight: 600 }}>⚠ Sunucuya bağlanılıyor...</p>}
        </div>
      </div>

      {/* Bento Grid: İstatistikler */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '24px' }}>
        {/* Kazanma Oranı */}
        <div className="card" style={{ padding: '20px', display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', fontWeight: 700, marginBottom: '8px' }}>KAZANMA ORANI</div>
          <div style={{ display: 'flex', alignItems: 'flex-end', gap: '8px' }}>
            <span style={{ fontSize: '2.4rem', fontWeight: 900, lineHeight: '1', color: winRate >= 50 ? 'var(--correct)' : 'var(--wrong)' }}>%{winRate}</span>
          </div>
          <div style={{ width: '100%', height: '6px', background: 'var(--bg-hover)', borderRadius: '10px', marginTop: '12px', overflow: 'hidden' }}>
            <div style={{ width: `${winRate}%`, height: '100%', background: winRate >= 50 ? 'var(--correct)' : 'var(--wrong)', borderRadius: '10px' }}></div>
          </div>
        </div>

        {/* Oyun Sayısı */}
        <div className="card" style={{ padding: '20px', display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', fontWeight: 700, marginBottom: '8px' }}>TOPLAM MAÇ</div>
          <div style={{ fontSize: '2.4rem', fontWeight: 900, lineHeight: '1', color: 'var(--text-primary)' }}>{totalGames}</div>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', fontWeight: 600, marginTop: '8px' }}>
            <span style={{ color: 'var(--correct)' }}>{user?.totalWins || 0} G</span> / <span style={{ color: 'var(--wrong)' }}>{user?.totalLosses || 0} M</span>
          </div>
        </div>
      </div>

      {/* Günlük Görevler Modülü */}
      <div className="glass" style={{ padding: '24px', borderRadius: 'var(--radius-lg)', marginBottom: '24px', border: '1px solid rgba(139,92,246,0.3)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
          <h3 style={{ fontSize: '1.1rem', fontWeight: 800, display: 'flex', alignItems: 'center', gap: '8px' }}>
            <span style={{ filter: 'drop-shadow(0 0 8px var(--gold))' }}>🎯</span> Günlük Görevler
          </h3>
          <span style={{ fontSize: '0.75rem', fontWeight: 700, color: 'var(--accent-cyan)', background: 'rgba(6,182,212,0.1)', padding: '4px 8px', borderRadius: '4px' }}>YENİLENİYOR: {timeLeft}</span>
        </div>
        
        <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
          {(quests || []).slice(0, 5).map(quest => {
            const percent = Math.min(100, Math.round((quest.currentProgress / quest.targetProgress) * 100));
            let color = 'var(--correct)'; // easy
            if (quest.difficulty === 1) color = 'var(--accent-cyan)'; // medium
            if (quest.difficulty === 2) color = 'var(--accent-pink)'; // hard
            if (quest.isClaimed) color = 'var(--text-muted)';
            
            return (
              <div key={quest.id} style={{ background: 'rgba(0,0,0,0.3)', borderRadius: '12px', padding: '16px', borderLeft: `4px solid ${color}`, opacity: quest.isClaimed ? 0.6 : 1, transition: 'all 0.3s' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '8px' }}>
                  <div style={{ flex: 1, paddingRight: '8px' }}>
                    <div style={{ fontWeight: 700, fontSize: '0.95rem', textDecoration: quest.isClaimed ? 'line-through' : 'none' }}>{quest.title}</div>
                    <div style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>{quest.description}</div>
                  </div>
                  {quest.rewardDiamonds > 0 ? (
                      <div className="badge badge-diamond" style={{ fontSize: '0.75rem', padding: '4px 8px', flexShrink: 0 }}>💎 +{quest.rewardDiamonds}</div>
                  ) : (
                      <div className="badge badge-gold" style={{ fontSize: '0.75rem', padding: '4px 8px', flexShrink: 0 }}>🪙 +{quest.rewardGold}</div>
                  )}
                </div>
                
                {quest.isCompleted && !quest.isClaimed ? (
                  <button onClick={() => handleClaimReward(quest.id)} className="btn btn-sm btn-full" style={{ background: 'linear-gradient(135deg, var(--correct), #10b981)', color: 'white', border: 'none', padding: '10px', fontSize: '0.9rem', fontWeight: 800, boxShadow: '0 4px 15px rgba(34,197,94,0.4)' }}>
                    Ödülü Al
                  </button>
                ) : (
                  <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                    <div style={{ flex: 1, height: '6px', background: 'var(--bg-hover)', borderRadius: '10px', overflow: 'hidden' }}>
                      <div style={{ width: `${percent}%`, height: '100%', background: color, borderRadius: '10px' }}></div>
                    </div>
                    <span style={{ fontSize: '0.8rem', fontWeight: 800, color: quest.isCompleted ? 'var(--correct)' : 'var(--text-muted)' }}>
                      {quest.isCompleted ? 'TAMAMLANDI' : `${quest.currentProgress} / ${quest.targetProgress}`}
                    </span>
                  </div>
                )}
              </div>
            );
          })}
          {(quests || []).length === 0 && <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', textAlign: 'center' }}>Görevler yükleniyor...</p>}
        </div>
      </div>

      {/* Mini Liderlik Panosu */}
      <div className="glass" style={{ padding: '24px', borderRadius: 'var(--radius-lg)' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
          <h3 style={{ fontSize: '1.1rem', fontWeight: 800, display: 'flex', alignItems: 'center', gap: '8px' }}>
            <span style={{ filter: 'drop-shadow(0 0 8px var(--accent))' }}>👑</span> Haftanın En İyileri
          </h3>
          <span style={{ fontSize: '0.8rem', color: 'var(--accent-cyan)', fontWeight: 700, cursor: 'pointer' }}>Tümünü Gör →</span>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
          <div style={{ display: 'flex', alignItems: 'center', padding: '12px', background: 'linear-gradient(90deg, rgba(234,179,8,0.15) 0%, transparent 100%)', borderRadius: '8px', borderLeft: '2px solid var(--gold)' }}>
            <div style={{ width: '24px', fontWeight: 900, color: 'var(--gold)', fontSize: '1.1rem' }}>#1</div>
            <div style={{ flex: 1, fontWeight: 700, marginLeft: '8px' }}>Pro_Slayer99</div>
            <div style={{ fontWeight: 800, color: 'var(--text-muted)' }}>142 Maç</div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', padding: '12px', background: 'linear-gradient(90deg, rgba(148,163,184,0.1) 0%, transparent 100%)', borderRadius: '8px', borderLeft: '2px solid #94a3b8' }}>
            <div style={{ width: '24px', fontWeight: 900, color: '#94a3b8', fontSize: '1.1rem' }}>#2</div>
            <div style={{ flex: 1, fontWeight: 700, marginLeft: '8px' }}>Cyber_Queen</div>
            <div style={{ fontWeight: 800, color: 'var(--text-muted)' }}>115 Maç</div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', padding: '12px', background: 'linear-gradient(90deg, rgba(180,83,9,0.1) 0%, transparent 100%)', borderRadius: '8px', borderLeft: '2px solid #b45309' }}>
            <div style={{ width: '24px', fontWeight: 900, color: '#b45309', fontSize: '1.1rem' }}>#3</div>
            <div style={{ flex: 1, fontWeight: 700, marginLeft: '8px' }}>{user?.username}</div>
            <div style={{ fontWeight: 800, color: 'var(--correct)', fontSize: '0.8rem' }}>🔥 Yükselişte!</div>
          </div>
        </div>
      </div>
    </div>
  );
}
