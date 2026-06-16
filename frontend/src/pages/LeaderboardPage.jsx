import { useState, useEffect } from 'react';
import { api } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const TrophyIcon = ({ color, glow }) => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" style={{ filter: `drop-shadow(0 0 8px ${glow})` }}>
    <path d="M17 3H7V6C7 8.76142 9.23858 11 12 11C14.7614 11 17 8.76142 17 6V3Z" fill={color}/>
    <path d="M12 11V16" stroke={color} strokeWidth="2" strokeLinecap="round"/>
    <path d="M8 21H16" stroke={color} strokeWidth="3" strokeLinecap="round"/>
    <path d="M10 16H14" stroke={color} strokeWidth="2" strokeLinecap="round"/>
    <path d="M7 4H4C3.44772 4 3 4.44772 3 5V6C3 7.65685 4.34315 9 6 9H7" stroke={color} strokeWidth="2" strokeLinecap="round"/>
    <path d="M17 4H20C20.5523 4 21 4.44772 21 5V6C21 7.65685 19.6569 9 18 9H17" stroke={color} strokeWidth="2" strokeLinecap="round"/>
  </svg>
);

const GlobeIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10"></circle>
    <line x1="2" y1="12" x2="22" y2="12"></line>
    <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"></path>
  </svg>
);

const UsersIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
    <circle cx="9" cy="7" r="4"></circle>
    <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
    <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
  </svg>
);

const TURKISH_NAMES = ["Kral", "Zeynep", "Usta", "Efsane", "Yusuf", "Gece", "Elif", "Siyah", "Caner", "Ayse", "Burak", "Kaan", "Deniz", "Cem", "Gizem", "Ceren", "Mert", "Okan", "Ruzgar", "Karanlik", "Gokhan", "Bora", "Efe", "Derya", "Selin", "Aslan", "Yigit", "Demir", "Bulut", "Firtina", "Zehir", "Genc", "Deli"];
const SUFFIXES = ["54", "_K", "Oyuncu", "Bey", "1907", "Kusu", "Nur", "Inci", "_G", "34", "Pro", "Avci", "99", "Reis", "X", "Baba", "Kral", "_01", "35", "Can", "TR", "Gaming", "_", "Han"];

function generateFakeGlobalLeaderboard() {
  const list = [];
  const top10 = [
    { username: 'Kral54', totalWins: 1245 },
    { username: 'Zeynep_K', totalWins: 1103 },
    { username: 'UstaOyuncu', totalWins: 985 },
    { username: 'EfsaneBey', totalWins: 874 },
    { username: 'Yusuf1907', totalWins: 742 },
    { username: 'GeceKusu', totalWins: 651 },
    { username: 'ElifNur', totalWins: 520 },
    { username: 'SiyahInci', totalWins: 418 },
    { username: 'CanerBey', totalWins: 395 },
    { username: 'Ayse_G', totalWins: 341 }
  ];

  list.push(...top10.map((u, i) => ({ ...u, userId: `fake_top_${i}`, rank: i + 1 })));
  
  let currentScore = 328;
  let seed = 12345;
  const random = () => {
    seed = (seed * 9301 + 49297) % 233280;
    return seed / 233280;
  };

  for (let i = 10; i < 100; i++) {
    const namePart = TURKISH_NAMES[Math.floor(random() * TURKISH_NAMES.length)];
    const suffixPart = SUFFIXES[Math.floor(random() * SUFFIXES.length)];
    const username = `${namePart}${suffixPart}`;
    
    currentScore -= Math.floor(random() * 4) + 1;
    if (currentScore < 0) currentScore = 0;
    
    list.push({
      userId: `fake_${i}`,
      username: username,
      totalWins: currentScore,
      rank: i + 1
    });
  }
  return list;
}

export default function LeaderboardPage() {
  const { user } = useAuth();
  const [tab, setTab] = useState('global');
  const [globalData, setGlobalData] = useState([]);
  const [friendsData, setFriendsData] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fakeGlobal = generateFakeGlobalLeaderboard();
    Promise.all([
      api.get('/leaderboard/friends').catch(() => [])
    ]).then(([f]) => { 
      setGlobalData(fakeGlobal); 
      setFriendsData(f); 
    }).finally(() => setLoading(false));
  }, []);

  const data = tab === 'global' ? globalData : friendsData;

  if (loading) return <div className="loading-screen"><div className="spinner" /><p style={{ color: 'var(--text-secondary)' }}>Yükleniyor...</p></div>;

  return (
    <div className="page" style={{ paddingBottom: '180px' }}>
      <div style={{ textAlign: 'center', marginBottom: '32px', marginTop: '16px' }}>
        <div style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center', width: '64px', height: '64px', borderRadius: '50%', background: 'rgba(139, 92, 246, 0.1)', border: '1px solid rgba(139, 92, 246, 0.3)', marginBottom: '16px', boxShadow: '0 0 30px rgba(139, 92, 246, 0.2)' }}>
          <TrophyIcon color="var(--accent)" glow="var(--accent-glow)" />
        </div>
        <h1 className="page-title gradient-text" style={{ fontSize: '2rem', marginBottom: '4px', textTransform: 'uppercase', letterSpacing: '2px' }}>Liderlik Tablosu</h1>
        <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', fontWeight: 500 }}>En iyiler arenası</p>
      </div>

      <div style={{ display: 'flex', gap: '8px', marginBottom: '24px', background: 'rgba(0,0,0,0.3)', borderRadius: 'var(--radius-lg)', padding: '6px', border: '1px solid rgba(255,255,255,0.05)' }}>
        <button 
          onClick={() => setTab('global')} 
          style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', padding: '12px', border: 'none', borderRadius: 'var(--radius-md)', background: tab === 'global' ? 'linear-gradient(135deg, var(--accent), var(--accent-hover))' : 'transparent', color: tab === 'global' ? 'white' : 'var(--text-muted)', fontWeight: 800, fontSize: '0.95rem', cursor: 'pointer', transition: 'all 0.3s', boxShadow: tab === 'global' ? '0 4px 16px var(--accent-glow)' : 'none' }}>
          <GlobeIcon /> Global
        </button>
        <button 
          onClick={() => setTab('friends')} 
          style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', padding: '12px', border: 'none', borderRadius: 'var(--radius-md)', background: tab === 'friends' ? 'linear-gradient(135deg, var(--accent-cyan), #0284c7)' : 'transparent', color: tab === 'friends' ? 'white' : 'var(--text-muted)', fontWeight: 800, fontSize: '0.95rem', cursor: 'pointer', transition: 'all 0.3s', boxShadow: tab === 'friends' ? '0 4px 16px rgba(6, 182, 212, 0.4)' : 'none' }}>
          <UsersIcon /> Arkadaşlar
        </button>
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
        {data.length === 0 && (
          <div className="glass" style={{ padding: '40px 20px', textAlign: 'center', borderRadius: 'var(--radius-lg)' }}>
            <UsersIcon />
            <p style={{ color: 'var(--text-muted)', marginTop: '12px', fontWeight: 600 }}>Bu listede henüz kimse yok.</p>
          </div>
        )}
        
        {data.map((entry, i) => {
          let rankStyle = { color: 'var(--text-muted)', border: 'transparent', bg: 'rgba(255,255,255,0.03)', glow: 'transparent' };
          
          if (i === 0) { rankStyle = { color: 'var(--gold)', border: 'var(--gold)', bg: 'linear-gradient(90deg, rgba(251,191,36,0.15) 0%, transparent 100%)', glow: 'var(--gold-glow)' }; }
          else if (i === 1) { rankStyle = { color: '#94a3b8', border: '#94a3b8', bg: 'linear-gradient(90deg, rgba(148,163,184,0.15) 0%, transparent 100%)', glow: 'rgba(148,163,184,0.3)' }; }
          else if (i === 2) { rankStyle = { color: '#b45309', border: '#b45309', bg: 'linear-gradient(90deg, rgba(180,83,9,0.15) 0%, transparent 100%)', glow: 'rgba(180,83,9,0.3)' }; }

          return (
            <div key={entry.userId} className="glass" style={{ 
              display: 'flex', alignItems: 'center', gap: '16px', padding: '16px 20px', 
              borderRadius: 'var(--radius-lg)', background: rankStyle.bg, 
              borderLeft: `3px solid ${rankStyle.border}`, 
              boxShadow: i < 3 ? `0 4px 20px ${rankStyle.glow}` : 'none',
              transition: 'transform 0.2s',
              cursor: 'pointer'
            }} onMouseOver={e => e.currentTarget.style.transform = 'translateY(-2px)'} onMouseOut={e => e.currentTarget.style.transform = 'none'}>
              
              <div style={{ width: '40px', display: 'flex', justifyContent: 'center' }}>
                {i < 3 ? (
                  <TrophyIcon color={rankStyle.color} glow={rankStyle.glow} />
                ) : (
                  <span style={{ fontWeight: 900, fontSize: '1.2rem', color: 'var(--text-muted)' }}>#{i + 1}</span>
                )}
              </div>
              
              <div style={{ 
                width: '42px', height: '42px', borderRadius: '12px', 
                background: i < 3 ? `linear-gradient(135deg, ${rankStyle.color}, rgba(0,0,0,0.5))` : 'var(--bg-hover)', 
                display: 'flex', alignItems: 'center', justifyContent: 'center', 
                fontWeight: 800, fontSize: '1.2rem', color: i < 3 ? '#1a1a1a' : 'var(--text-primary)',
                boxShadow: i < 3 ? `inset 0 0 10px rgba(255,255,255,0.5)` : 'none'
              }}>
                {entry.username[0]?.toUpperCase()}
              </div>
              
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 800, fontSize: '1.05rem', color: i < 3 ? 'var(--text-primary)' : 'var(--text-secondary)', letterSpacing: '0.5px' }}>{entry.username}</div>
                {i < 3 && <div style={{ fontSize: '0.75rem', color: rankStyle.color, fontWeight: 700, marginTop: '2px', textTransform: 'uppercase' }}>Elit Oyuncu</div>}
              </div>
              
              <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end' }}>
                <div style={{ fontWeight: 900, color: 'var(--correct)', fontSize: '1.2rem', textShadow: '0 0 10px rgba(16,185,129,0.4)' }}>
                  {entry.totalWins}
                </div>
                <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)', fontWeight: 700, textTransform: 'uppercase' }}>Galibiyet</div>
              </div>
              
            </div>
          );
        })}
      </div>

      {/* User's own sticky rank banner at the bottom */}
      {tab === 'global' && user && (
        <div style={{ 
          position: 'fixed', bottom: '70px', left: '0', right: '0', 
          padding: '16px', background: 'rgba(11, 15, 25, 0.95)', 
          backdropFilter: 'blur(20px)', borderTop: '1px solid rgba(16,185,129,0.3)', 
          boxShadow: '0 -10px 40px rgba(16,185,129,0.15)', zIndex: 90 
        }}>
          <div style={{ maxWidth: '480px', margin: '0 auto', display: 'flex', alignItems: 'center', gap: '16px' }}>
            <div style={{ width: '40px', display: 'flex', justifyContent: 'center' }}>
              <span style={{ fontWeight: 900, fontSize: '1.2rem', color: 'var(--correct)' }}>-</span>
            </div>
            <div style={{ flex: 1 }}>
              <div style={{ fontWeight: 800, fontSize: '1.05rem', color: 'var(--correct)' }}>{user.username}</div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', fontWeight: 700 }}>Senin Sıralaman (🔥 Yükselişte)</div>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end' }}>
              <div style={{ fontWeight: 900, color: 'var(--correct)', fontSize: '1.2rem' }}>
                {user.totalWins || 0}
              </div>
              <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)', fontWeight: 700 }}>Galibiyet</div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
