import { useState, useEffect, useRef, useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';
import { useTimer } from '../hooks/useTimer';
import { GAME } from '../utils/constants';
import { api } from '../services/api';

export default function GamePage({ gameState: initialState, connection, onGameEnd }) {
  const { user, updateUser } = useAuth();
  const { addToast } = useToast();
  const [answer, setAnswer] = useState('');
  const [answers, setAnswers] = useState([]);
  const [gameState, setGameState] = useState(initialState);
  const [activeTurnPlayerId, setActiveTurnPlayerId] = useState(initialState?.activeTurnPlayerId);
  const [myScore, setMyScore] = useState(0);
  const [opponentScore, setOpponentScore] = useState(0);
  const [roundEnded, setRoundEnded] = useState(false);
  const [matchEnded, setMatchEnded] = useState(false);
  const [matchResult, setMatchResult] = useState(null);
  const [hint, setHint] = useState(null);
  const [changeRequestedByMe, setChangeRequestedByMe] = useState(false);
  const [changeRequestedByOpponent, setChangeRequestedByOpponent] = useState(false);
  const [isStartingUp, setIsStartingUp] = useState(false);
  const [startupTimer, setStartupTimer] = useState(10);
  const inputRef = useRef(null);

  const isPlayer1 = user?.id === initialState?.player1Name ? true : false;
  const isMyTurn = user?.id === activeTurnPlayerId;

  const startIntermission = useCallback(() => {
    setIsStartingUp(true);
    setStartupTimer(10);
    // 10 saniye geriye say
    const int = setInterval(() => {
      setStartupTimer(prev => {
        if (prev <= 1) {
          clearInterval(int);
          setIsStartingUp(false);
          timer.reset(GAME.ROUND_DURATION);
          timer.start();
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  }, []);

  const handleTimeUp = useCallback(async () => {
    if (!connection || !gameState) return;
    try {
      await connection.invoke('TimeUp', gameState.sessionId, gameState.roundId);
    } catch (err) {
      console.error('TimeUp error:', err);
    }
  }, [connection, gameState]);

  const timer = useTimer(GAME.ROUND_DURATION, handleTimeUp);

  // Oyun başlayınca timer'ı başlat (Intermission üzerinden)
  useEffect(() => {
    timer.stop(); // ilk girişte dursun
    setAnswers([]);
    setMyScore(0);
    setOpponentScore(0);
    setRoundEnded(false);
    setChangeRequestedByMe(false);
    setChangeRequestedByOpponent(false);
    startIntermission();
  }, [gameState?.roundId]);

  // Alt menüyü gizlemek için CSS class'ı ekle
  useEffect(() => {
    document.body.classList.add('in-game');
    return () => document.body.classList.remove('in-game');
  }, []);

  const handleExitGame = useCallback(async () => {
    if (window.confirm("Oyundan çıkmak istediğinize emin misiniz? Hükmen mağlup sayılacaksınız.")) {
      if (connection && gameState?.sessionId) {
        try {
          await connection.invoke('Surrender', gameState.sessionId);
        } catch (e) {
          console.error("Surrender error:", e);
        }
      }
      
      // Kullanıcı istatistiklerini güncelle (Toplam maç vb. yansıması için)
      try {
        const updatedProfile = await api.get('/profile');
        updateUser(updatedProfile);
      } catch (e) {
        console.error("Profile update error:", e);
      }

      onGameEnd();
    }
  }, [connection, gameState, onGameEnd, updateUser]);

  // SignalR event dinleyicileri
  useEffect(() => {
    if (!connection) return;

    connection.on('AnswerResult', (result) => {
      setAnswers(prev => [...prev, { ...result, id: Date.now() }]);
      setMyScore(result.newScore);
      if (result.isCorrect) {
        if (result.isPopular) {
          addToast(`Popüler cevap! +${GAME.POPULAR_ANSWER_BONUS} Altın 🪙`, 'gold');
        }
      }
    });

    connection.on('OpponentScoreUpdate', (data) => {
      setOpponentScore(data.newScore);
    });

    connection.on('OpponentAnswer', (data) => {
      setAnswers(prev => [...prev, { ...data, submittedAnswer: data.answer, isOpponent: true, id: Date.now() }]);
    });

    connection.on('TurnChanged', (newTurnPlayerId) => {
      setActiveTurnPlayerId(newTurnPlayerId);
      timer.reset(GAME.ROUND_DURATION);
      timer.start();
      if (newTurnPlayerId === user?.id) {
          addToast("Sıra Sende!", "info");
      }
    });

    connection.on('RoundEnded', (result) => {
      timer.stop();
      setRoundEnded(true);
      if (result.status === 'Completed') {
        setMatchEnded(true);
        setMatchResult(result);
      }
    });

    connection.on('NewRound', (data) => {
      setGameState(prev => ({ ...prev, roundId: data.roundId, questionText: data.questionText, roundNumber: data.roundNumber }));
      setActiveTurnPlayerId(data.activeTurnPlayerId);
      setRoundEnded(false);
      setHint(null);
    });

    connection.on('JokerResult', (data) => {
      if (data.success && data.jokerType === 'ExtraTime') {
        timer.addTime(5);
        addToast('+5 saniye eklendi! ⏱', 'success');
      } else if (!data.success) {
        addToast('Yeterli altın yok!', 'error');
      }
    });

    connection.on('HintResult', (data) => {
      if (data.success) {
        setHint(data.firstLetter);
        addToast(`İpucu: Popüler cevap "${data.firstLetter}" ile başlıyor`, 'info');
      }
    });

    connection.on('RematchStarted', (state) => {
      setGameState(state);
      setMatchEnded(false);
      setMatchResult(null);
      setRoundEnded(false);
      setAnswers([]);
      setMyScore(0);
      setOpponentScore(0);
    });

    connection.on('RematchRequested', () => {
      addToast('Rakibiniz rövanş istiyor!', 'info');
    });

    connection.on('ChangeQuestionRequested', () => {
      setChangeRequestedByOpponent(true);
      addToast('Rakip soruyu değiştirmek istiyor!', 'info');
    });

    connection.on('QuestionChanged', (state) => {
      setGameState(prev => ({ ...prev, questionText: state.questionText }));
      setActiveTurnPlayerId(state.activeTurnPlayerId);
      setAnswers([]);
      setMyScore(0);
      setOpponentScore(0);
      setChangeRequestedByMe(false);
      setChangeRequestedByOpponent(false);
      startIntermission();
      addToast('Soru değiştirildi!', 'success');
    });

    return () => {
      ['AnswerResult', 'OpponentScoreUpdate', 'OpponentAnswer', 'TurnChanged', 'RoundEnded', 'NewRound', 'JokerResult', 'HintResult', 'RematchStarted', 'RematchRequested', 'ChangeQuestionRequested', 'QuestionChanged']
        .forEach(e => connection.off(e));
    };
  }, [connection]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!answer.trim() || !connection || roundEnded || !isMyTurn) return;

    const isAlreadyFound = answers.some(a => 
      a.isCorrect && 
      (a.submittedAnswer?.toLowerCase() === answer.trim().toLowerCase() || 
       a.matchedAnswer?.toLowerCase() === answer.trim().toLowerCase())
    );

    if (isAlreadyFound) {
      document.body.classList.add('flash-dark');
      setTimeout(() => document.body.classList.remove('flash-dark'), 400);
      addToast('Bu kelime zaten bulundu!', 'warning');
      setAnswer('');
      return;
    }

    try {
      await connection.invoke('SubmitAnswer', { sessionId: gameState.sessionId, roundId: gameState.roundId, answer: answer.trim() });
      setAnswer('');
      inputRef.current?.focus();
    } catch {
      addToast('Cevap gönderilemedi', 'error');
    }
  };

  const handleJoker = async (type) => {
    if (!connection) return;
    try {
      await connection.invoke('UseJoker', { sessionId: gameState.sessionId, roundId: gameState.roundId, jokerType: type });
    } catch {
      addToast('Joker kullanılamadı', 'error');
    }
  };

  const handleRematch = async () => {
    if (!connection) return;
    try {
      await connection.invoke('RequestRematch', gameState.sessionId);
      addToast('Rövanş isteği gönderildi!', 'info');
    } catch {
      addToast('Rövanş gönderilemedi', 'error');
    }
  };

  const handleChangeQuestion = async () => {
    if (!connection || changeRequestedByMe) return;
    try {
      await connection.invoke('RequestChangeQuestion', gameState.sessionId, gameState.roundId);
      setChangeRequestedByMe(true);
      addToast('Soru değiştirme isteği gönderildi.', 'info');
    } catch {
      addToast('İstek gönderilemedi', 'error');
    }
  };

  // ======== MATCH ENDED SCREEN ========
  if (matchEnded) {
    const won = matchResult?.winnerId === user?.id;
    return (
      <div className="page" style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '100vh', paddingBottom: 16 }}>
        <div className="card" style={{ textAlign: 'center', padding: '40px 24px', width: '100%', background: won ? 'linear-gradient(135deg, rgba(34,197,94,0.1), rgba(16,185,129,0.1))' : 'linear-gradient(135deg, rgba(239,68,68,0.1), rgba(249,115,22,0.1))', borderColor: won ? 'rgba(34,197,94,0.3)' : 'rgba(239,68,68,0.3)' }}>
          <div style={{ fontSize: '4rem', marginBottom: '16px' }}>{won ? '🏆' : '😔'}</div>
          <h2 style={{ fontSize: '1.6rem', fontWeight: 900, color: won ? 'var(--correct)' : 'var(--wrong)', textTransform: 'uppercase', letterSpacing: '2px' }}>{won ? 'ZAFer' : 'BOZGun'}</h2>
          <p style={{ color: 'var(--text-secondary)', margin: '8px 0 24px', fontSize: '0.9rem' }}>{won ? 'Arenanın hakimi sensin!' : 'Daha fazla pratik yapmalısın...'}</p>
          
          {won && (
            <div style={{ display: 'flex', justifyContent: 'center', gap: '16px', marginBottom: '24px' }}>
              <div className="badge badge-gold" style={{ fontSize: '1rem', padding: '8px 16px' }}>🪙 +{GAME.MATCH_WIN_GOLD}</div>
              <div className="badge badge-diamond" style={{ fontSize: '1rem', padding: '8px 16px' }}>💎 +{GAME.MATCH_WIN_DIAMONDS}</div>
            </div>
          )}

          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
            <button className="btn btn-primary btn-lg btn-full" onClick={handleRematch}>🔄 Rövanş İste</button>
            <button className="btn btn-ghost btn-full" onClick={onGameEnd}>🏠 Lobiye Dön</button>
          </div>
        </div>
      </div>
    );
  }

  // ======== GAME SCREEN ========
  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', maxWidth: 480, margin: '0 auto', padding: '12px' }}>
      {/* Header */}
      <div className="glass" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '12px 20px', marginBottom: '16px', borderRadius: 'var(--radius-full)', opacity: isStartingUp ? 0.3 : 1, transition: '0.3s', border: '1px solid rgba(255,255,255,0.1)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <button onClick={handleExitGame} style={{ background: 'rgba(239, 68, 68, 0.15)', border: '1px solid rgba(239, 68, 68, 0.3)', color: 'var(--wrong)', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '6px', borderRadius: '8px', transition: 'var(--transition)' }} title="Pes Et ve Çık">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
              <polyline points="16 17 21 12 16 7"></polyline>
              <line x1="21" y1="12" x2="9" y2="12"></line>
            </svg>
          </button>
          <div style={{ fontSize: '0.85rem', fontWeight: 900, color: 'var(--accent)', letterSpacing: '2px' }}>TUR {gameState?.roundNumber || 1}/3</div>
        </div>
        <div style={{ display: 'flex', gap: '12px', alignItems: 'center' }}>
          <span style={{ fontSize: '0.9rem', fontWeight: 900, color: 'var(--correct)', textShadow: '0 0 10px var(--correct-glow)' }}>{gameState?.player1RoundWins || 0}</span>
          <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>⚔️</span>
          <span style={{ fontSize: '0.9rem', fontWeight: 900, color: 'var(--wrong)', textShadow: '0 0 10px var(--wrong-glow)' }}>{gameState?.player2RoundWins || 0}</span>
        </div>
      </div>

      {isStartingUp ? (
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
          <div className="glass animate-slide-up" style={{ textAlign: 'center', padding: '40px 24px', width: '100%', background: 'linear-gradient(135deg, rgba(139,92,246,0.2), rgba(6,182,212,0.1))', border: '1px solid var(--accent-glow)', boxShadow: '0 0 50px var(--accent-glow)' }}>
            <div style={{ fontSize: '1.2rem', color: 'var(--accent-cyan)', fontWeight: 900, marginBottom: '12px', letterSpacing: '4px', textShadow: '0 0 10px var(--accent-cyan)' }}>SAVAŞ ALANI KURULUYOR</div>
            <h2 style={{ fontSize: '2rem', fontWeight: 900, marginBottom: '32px', color: 'white', textTransform: 'uppercase', textShadow: '0 4px 12px rgba(0,0,0,0.5)' }}>{gameState?.questionText}</h2>
            
            <div style={{ position: 'relative', width: 120, height: 120, margin: '0 auto' }}>
              <svg viewBox="0 0 80 80" style={{ transform: 'rotate(-90deg)' }}>
                <circle cx="40" cy="40" r="34" fill="none" stroke="rgba(255,255,255,0.05)" strokeWidth="4" />
                <circle cx="40" cy="40" r="34" fill="none" stroke="var(--accent-cyan)" strokeWidth="4" strokeDasharray={`${(startupTimer / 10) * 213.6} 213.6`} strokeLinecap="round" style={{ transition: 'stroke-dasharray 1s linear', filter: 'drop-shadow(0 0 8px var(--accent-cyan))' }} />
              </svg>
              <div style={{ position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '2.5rem', fontWeight: 900, color: 'var(--accent-cyan)', textShadow: '0 0 20px var(--accent-cyan)' }}>
                {startupTimer}
              </div>
            </div>
            
            <div style={{ marginTop: '24px', fontSize: '0.9rem', color: 'var(--text-muted)', letterSpacing: '2px', marginBottom: '16px' }}>HAZIRLANIN...</div>

            {/* Soru Değiştirme Paneli */}
            <div style={{ display: 'flex', gap: '8px', marginTop: '16px' }}>
              <button 
                  className="btn btn-full" 
                  onClick={handleChangeQuestion} 
                  disabled={changeRequestedByMe}
                  style={{ 
                      background: changeRequestedByOpponent ? 'linear-gradient(135deg, var(--correct), #10b981)' : (changeRequestedByMe ? 'rgba(255,255,255,0.1)' : 'linear-gradient(135deg, rgba(59,130,246,0.2), rgba(37,99,235,0.1))'), 
                      color: changeRequestedByOpponent ? 'white' : (changeRequestedByMe ? 'var(--text-muted)' : 'var(--accent-cyan)'), 
                      border: changeRequestedByOpponent ? 'none' : '1px solid rgba(59,130,246,0.3)', 
                      fontSize: '0.9rem',
                      padding: '12px',
                      fontWeight: 800,
                      boxShadow: changeRequestedByOpponent ? '0 0 15px var(--correct-glow)' : 'none',
                      transition: 'all 0.3s'
                  }}
              >
                  {changeRequestedByOpponent ? (
                      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>
                        RAKİP SORUYU DEĞİŞTİRMEK İSTİYOR (ONAYLA)
                      </div>
                  ) : changeRequestedByMe ? (
                      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"></circle><polyline points="12 6 12 12 16 14"></polyline></svg>
                        İSTEK GÖNDERİLDİ (BEKLENİYOR)
                      </div>
                  ) : (
                      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><polyline points="16 3 21 3 21 8"></polyline><line x1="4" y1="20" x2="21" y2="3"></line><polyline points="21 16 21 21 16 21"></polyline><line x1="15" y1="15" x2="21" y2="21"></line><line x1="4" y1="4" x2="9" y2="9"></line></svg>
                        SORUYU DEĞİŞTİR
                      </div>
                  )}
              </button>
            </div>
          </div>
        </div>
      ) : (
        <>
          {/* Timer */}
      <div style={{ textAlign: 'center', marginBottom: '12px' }}>
        <div style={{ position: 'relative', width: 80, height: 80, margin: '0 auto', marginBottom: '8px' }}>
          <svg viewBox="0 0 80 80" style={{ transform: 'rotate(-90deg)' }}>
            <circle cx="40" cy="40" r="34" fill="none" stroke="var(--border)" strokeWidth="6" />
            <circle cx="40" cy="40" r="34" fill="none" stroke={timer.timerColor} strokeWidth="6"
              strokeDasharray={`${timer.progress * 2.136} 213.6`}
              strokeLinecap="round" style={{ transition: 'stroke-dasharray 1s linear, stroke 0.5s' }} />
          </svg>
          <div style={{ position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.5rem', fontWeight: 900, color: timer.timerColor, animation: timer.seconds <= 5 ? 'countdownPulse 1s infinite' : 'none' }}>
            {timer.seconds}
          </div>
        </div>
      </div>

      {/* Soru */}
      <div className="glass" style={{ textAlign: 'center', padding: '24px 16px', marginBottom: '16px', background: 'linear-gradient(135deg, rgba(139,92,246,0.15), rgba(6,182,212,0.15))', border: '1px solid rgba(139,92,246,0.3)', borderRadius: 'var(--radius-lg)', boxShadow: '0 8px 32px rgba(0,0,0,0.3)' }}>
        <div style={{ fontSize: '0.75rem', color: 'var(--accent-cyan)', fontWeight: 800, marginBottom: '8px', letterSpacing: '3px' }}>HEDEF KELİME GRUBU</div>
        <h2 style={{ fontSize: '1.3rem', fontWeight: 900, textTransform: 'uppercase', textShadow: '0 2px 4px rgba(0,0,0,0.5)' }}>{gameState?.questionText}</h2>
        {hint && <div style={{ marginTop: '12px', color: 'var(--diamond)', fontSize: '0.9rem', fontWeight: 700, filter: 'drop-shadow(0 0 5px var(--diamond-glow))' }}>💡 İpucu: "{hint}..." ile başlar</div>}
      </div>

      {/* Skor (VS Ekranı) */}
      <div className="glass" style={{ display: 'flex', justifyContent: 'space-around', alignItems: 'center', padding: '16px', marginBottom: '16px', borderRadius: 'var(--radius-lg)' }}>
        <div style={{ textAlign: 'center', opacity: isMyTurn ? 1 : 0.4, transform: isMyTurn ? 'scale(1.1)' : 'scale(0.9)', transition: 'all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1)' }}>
          <div style={{ fontSize: '0.75rem', color: isMyTurn ? 'white' : 'var(--text-muted)', fontWeight: 800, letterSpacing: '1px' }}>{gameState?.player1Name || 'Sen'} {isMyTurn && '⏳'}</div>
          <div style={{ fontSize: '2rem', fontWeight: 900, color: 'var(--correct)', textShadow: isMyTurn ? '0 0 20px var(--correct-glow)' : 'none' }}>{myScore}</div>
        </div>
        <div style={{ fontSize: '1.2rem', color: 'var(--accent-pink)', fontWeight: 900, fontStyle: 'italic', opacity: 0.8 }}>VS</div>
        <div style={{ textAlign: 'center', opacity: !isMyTurn ? 1 : 0.4, transform: !isMyTurn ? 'scale(1.1)' : 'scale(0.9)', transition: 'all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1)' }}>
          <div style={{ fontSize: '0.75rem', color: !isMyTurn ? 'white' : 'var(--text-muted)', fontWeight: 800, letterSpacing: '1px' }}>{gameState?.player2Name || 'Rakip'} {!isMyTurn && '⏳'}</div>
          <div style={{ fontSize: '2rem', fontWeight: 900, color: 'var(--wrong)', textShadow: !isMyTurn ? '0 0 20px var(--wrong-glow)' : 'none' }}>{opponentScore}</div>
        </div>
      </div>

      {/* Cevap Akışı (Feed) */}
      <div style={{ flex: 1, overflowY: 'auto', marginBottom: '16px', maxHeight: '25vh', paddingRight: '4px' }}>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
          {answers.map((a) => (
            <div key={a.id} className="animate-slide-up" style={{ 
              display: 'flex', alignItems: 'center', gap: '10px', padding: '12px 16px', 
              borderRadius: 'var(--radius-md)', background: 'rgba(0,0,0,0.4)', 
              borderLeft: `4px solid ${a.isCorrect ? 'var(--correct)' : 'var(--wrong)'}`,
              boxShadow: '0 4px 12px rgba(0,0,0,0.3)',
              opacity: a.isOpponent ? 0.7 : 1 
            }}>
              <span style={{ fontSize: '1.1rem', flexShrink: 0, filter: `drop-shadow(0 0 5px ${a.isCorrect ? 'var(--correct)' : 'var(--wrong)'})` }}>{a.isCorrect ? '✓' : '✗'}</span>
              <span style={{ flex: 1, fontSize: '0.95rem', fontWeight: 700, color: a.isCorrect ? 'white' : 'var(--text-muted)' }}>
                {a.submittedAnswer} {a.isOpponent && <span style={{fontSize:'0.7rem', color:'var(--text-secondary)', marginLeft: '4px', textTransform: 'uppercase'}}>[Rakip]</span>}
              </span>
              {a.isCorrect && a.matchedAnswer !== a.submittedAnswer && (
                <span style={{ fontSize: '0.75rem', color: 'var(--accent-cyan)', fontWeight: 600 }}>→ {a.matchedAnswer}</span>
              )}
              {a.isPopular && <span className="badge badge-gold" style={{ fontSize: '0.7rem', boxShadow: '0 0 10px var(--gold-glow)' }}>🪙+10</span>}
            </div>
          ))}
        </div>
      </div>

      {/* Joker Panel */}
      <div style={{ display: 'flex', gap: '8px', marginBottom: '10px' }}>
        <button className="btn btn-sm" onClick={() => handleJoker('ExtraTime')} disabled={roundEnded}
          style={{ flex: 1, background: 'linear-gradient(135deg, rgba(139,92,246,0.2), rgba(139,92,246,0.1))', color: 'var(--accent)', border: '1px solid rgba(139,92,246,0.3)', fontSize: '0.75rem' }}>
          ⏱ +5sn <span style={{ color: 'var(--gold)', fontSize: '0.7rem' }}>({GAME.EXTRA_TIME_COST}🪙)</span>
        </button>
        <button className="btn btn-sm" onClick={() => handleJoker('Hint')} disabled={roundEnded}
          style={{ flex: 1, background: 'linear-gradient(135deg, rgba(236,72,153,0.2), rgba(236,72,153,0.1))', color: 'var(--accent-pink)', border: '1px solid rgba(236,72,153,0.3)', fontSize: '0.75rem' }}>
          💡 İpucu <span style={{ color: 'var(--gold)', fontSize: '0.7rem' }}>({GAME.HINT_COST}🪙)</span>
        </button>
      </div>

      {/* Cevap Girişi */}
      <form onSubmit={handleSubmit} style={{ display: 'flex', gap: '8px', background: 'rgba(0,0,0,0.4)', padding: '8px', borderRadius: 'var(--radius-lg)', border: '1px solid rgba(255,255,255,0.05)' }}>
        <input ref={inputRef} type="text" placeholder={isMyTurn ? "Cevabını yaz..." : "Sıra rakipte..."} value={answer} onChange={e => setAnswer(e.target.value)} disabled={roundEnded || !isMyTurn} autoFocus autoComplete="off" style={{ flex: 1, padding: '14px 16px', background: 'transparent', border: 'none', color: 'white', fontSize: '1rem', outline: 'none', opacity: (!isMyTurn || roundEnded) ? 0.5 : 1 }} />
        <button className="btn" type="submit" disabled={roundEnded || !answer.trim() || !isMyTurn} style={{ padding: '12px 24px', background: 'linear-gradient(135deg, var(--accent), var(--accent-hover))', color: 'white', borderRadius: 'var(--radius-md)', border: 'none', opacity: (!isMyTurn || roundEnded) ? 0.5 : 1, boxShadow: '0 4px 12px var(--accent-glow)' }}>Gönder</button>
      </form>
        </>
      )}
    </div>
  );
}
