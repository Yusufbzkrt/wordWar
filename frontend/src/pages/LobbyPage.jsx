import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';
import { useGameConnection } from '../hooks/useGameConnection';
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

  useEffect(() => { connect(); }, [connect]);

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
    return () => {
      connection.off('MatchFound');
      connection.off('SearchingMatch');
      connection.off('SearchCancelled');
    };
  }, [connection, addToast]);

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

  return (
    <div className="page" style={{ paddingBottom: '100px' }}>
      {/* Üst Header Alanı */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '32px', paddingTop: '10px' }}>
        <div>
          <h2 style={{ fontSize: '1.6rem', fontWeight: 900, letterSpacing: '-0.03em', lineHeight: '1.2' }}>
            Selam, <span className="gradient-text">{user?.username}</span> 👋
          </h2>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', fontWeight: 500, marginTop: '4px' }}>Meydan okumaya hazır mısın?</p>
        </div>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', alignItems: 'flex-end' }}>
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
            <button className="btn btn-lg btn-full" onClick={handleSearch} disabled={!connected} 
              style={{ background: 'var(--bg-primary)', color: 'var(--accent-cyan)', fontSize: '1.2rem', padding: '18px', boxShadow: '0 8px 32px rgba(0,0,0,0.5)', borderRadius: 'var(--radius-xl)', border: '2px solid rgba(6, 182, 212, 0.4)', textTransform: 'uppercase', letterSpacing: '2px' }}>
              Rakip Bul
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

      {/* Hızlı Kurallar Modülü */}
      <div className="glass" style={{ padding: '24px', borderRadius: 'var(--radius-lg)' }}>
        <h3 style={{ fontSize: '1rem', fontWeight: 800, marginBottom: '16px', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <span>📖</span> Nasıl Oynanır?
        </h3>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <div style={{ display: 'flex', alignItems: 'flex-start', gap: '12px' }}>
            <div style={{ background: 'var(--bg-hover)', padding: '8px', borderRadius: '10px', fontSize: '1.2rem' }}>⏱</div>
            <div>
              <div style={{ fontWeight: 700, fontSize: '0.9rem' }}>30 Saniye</div>
              <div style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>Sorulan soruya uygun kelimeleri süre bitmeden gönder.</div>
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'flex-start', gap: '12px' }}>
            <div style={{ background: 'var(--bg-hover)', padding: '8px', borderRadius: '10px', fontSize: '1.2rem' }}>🏆</div>
            <div>
              <div style={{ fontWeight: 700, fontSize: '0.9rem' }}>Best of 3</div>
              <div style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>En çok doğru cevabı veren turu kazanır. 2 turu alan maçı kazanır.</div>
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'flex-start', gap: '12px' }}>
            <div style={{ background: 'var(--bg-hover)', padding: '8px', borderRadius: '10px', fontSize: '1.2rem' }}>🪙</div>
            <div>
              <div style={{ fontWeight: 700, fontSize: '0.9rem' }}>Bonuslar</div>
              <div style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>Popüler kelimeleri bularak +10 Altın bonus kazanabilirsin.</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
