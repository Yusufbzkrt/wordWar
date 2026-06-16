import { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';
import { api } from '../services/api';
import { GAME } from '../utils/constants';
import { AdMob, RewardAdPluginEvents } from '@capacitor-community/admob';
import { Capacitor } from '@capacitor/core';
import { useTranslation } from 'react-i18next';
import { useAudio } from '../contexts/AudioContext';

const EditIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M12 20h9"></path>
    <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"></path>
  </svg>
);

const CheckIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="20 6 9 17 4 12"></polyline>
  </svg>
);

const XIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="18" y1="6" x2="6" y2="18"></line>
    <line x1="6" y1="6" x2="18" y2="18"></line>
  </svg>
);

const LogOutIcon = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
    <polyline points="16 17 21 12 16 7"></polyline>
    <line x1="21" y1="12" x2="9" y2="12"></line>
  </svg>
);

const StatsIcon = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="18" y1="20" x2="18" y2="10"></line>
    <line x1="12" y1="20" x2="12" y2="4"></line>
    <line x1="6" y1="20" x2="6" y2="14"></line>
  </svg>
);

const PlayIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10"></circle>
    <polygon points="10 8 16 12 10 16 10 8"></polygon>
  </svg>
);

const SettingsIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="3"></circle>
    <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"></path>
  </svg>
);

const BackIcon = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="15 18 9 12 15 6"></polyline>
  </svg>
);

export default function ProfilePage() {
  const { user, updateUser, logout } = useAuth();
  const { addToast } = useToast();
  const { t, i18n } = useTranslation();
  
  const [editing, setEditing] = useState(false);
  const [newName, setNewName] = useState(user?.username || '');
  const [claiming, setClaiming] = useState(false);
  const [adTimeLeft, setAdTimeLeft] = useState(null);
  const [showSettings, setShowSettings] = useState(false);

  // Settings State
  const { musicEnabled, soundEnabled, toggleMusic, toggleSound } = useAudio();
  const [notificationsEnabled, setNotificationsEnabled] = useState(true);

  const [showLangModal, setShowLangModal] = useState(false);
  const [pendingLang, setPendingLang] = useState(null);
  
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleting, setDeleting] = useState(false);

  const handleDeleteAccount = async () => {
    try {
      setDeleting(true);
      await api.delete('/profile');
      addToast(t('settings.account_deleted') || 'Hesabınız silindi.', 'success');
      logout();
    } catch (err) {
      addToast(err.response?.data?.message || 'Hesap silinirken bir hata oluştu.', 'error');
      setDeleting(false);
      setShowDeleteModal(false);
    }
  };

  const openLangModal = () => {
    setPendingLang(null);
    setShowLangModal(true);
  };

  const confirmLanguage = () => {
    if (pendingLang) {
      i18n.changeLanguage(pendingLang);
    }
    setShowLangModal(false);
    setPendingLang(null);
  };

  useEffect(() => {
    if (!user?.lastAdRewardTime) {
      setAdTimeLeft(null);
      return;
    }

    const calculateTimeLeft = () => {
      const lastWatch = new Date(user.lastAdRewardTime);
      const nextWatch = new Date(lastWatch.getTime() + (GAME.AD_COOLDOWN_HOURS || 2) * 60 * 60 * 1000);
      const now = new Date();
      const diff = nextWatch - now;

      if (diff <= 0) return null;

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
    if (claiming || adTimeLeft !== null) return;
    setClaiming(true);

    try {
      if (Capacitor.isNativePlatform()) {
        await AdMob.initialize();
        
        // Resmi Test Reklam ID'leri
        const adId = Capacitor.getPlatform() === 'ios' 
          ? 'ca-app-pub-3940256099942544/1712485313' 
          : 'ca-app-pub-3940256099942544/5224354917';

        // Reklam izlendiğinde tetiklenir
        AdMob.addListener(RewardAdPluginEvents.Rewarded, async () => {
          await api.post('/profile/ad-reward');
          updateUser({ 
            diamonds: (user?.diamonds || 0) + 3,
            lastAdRewardTime: new Date().toISOString()
          });
          addToast(t('profile.ad_reward_success'), 'success');
        });

        // Reklam kapatıldığında
        AdMob.addListener(RewardAdPluginEvents.Dismissed, () => {
          setClaiming(false);
        });

        // Reklam yüklenemezse
        AdMob.addListener(RewardAdPluginEvents.FailedToLoad, (error) => {
          addToast(t('profile.ad_failed'), 'error');
          setClaiming(false);
        });

        await AdMob.prepareRewardVideoAd({ adId });
        await AdMob.showRewardVideoAd();
      } else {
        // Web tarayıcısı simülasyonu
        setTimeout(async () => {
          await api.post('/profile/ad-reward');
          updateUser({ 
            diamonds: (user?.diamonds || 0) + 3,
            lastAdRewardTime: new Date().toISOString()
          });
          addToast(t('profile.ad_reward_web'), 'success');
          setClaiming(false);
        }, 2000);
      }
    } catch (err) { 
      addToast(err.message, 'error'); 
      setClaiming(false); 
    }
  };

  const totalMatches = (user?.totalWins || 0) + (user?.totalLosses || 0);
  const winRate = totalMatches > 0
    ? Math.round((user.totalWins / totalMatches) * 100)
    : 0;

  const getRank = (rate, matches) => {
    if (matches === 0) return { title: t('profile.rank_newbie'), color: 'var(--text-muted)' };
    if (rate < 40) return { title: t('profile.rank_rookie'), color: 'var(--wrong)' };
    if (rate < 50) return { title: t('profile.rank_developing'), color: 'var(--gold)' };
    if (rate < 60) return { title: t('profile.rank_experienced'), color: 'var(--accent-cyan)' };
    if (rate < 80) return { title: t('profile.rank_elite'), color: 'var(--accent-pink)' };
    return { title: t('profile.rank_legendary'), color: 'var(--diamond)' };
  };

  const currentRank = getRank(winRate, totalMatches);

  if (showSettings) {
    return (
      <div className="page" style={{ paddingBottom: '100px' }}>
        <div style={{ display: 'flex', alignItems: 'center', marginBottom: '24px' }}>
          <button onClick={() => setShowSettings(false)} style={{ background: 'transparent', border: 'none', color: 'white', cursor: 'pointer', padding: '8px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <BackIcon />
          </button>
          <h1 className="page-title gradient-text" style={{ fontSize: '1.6rem', margin: '0 0 0 12px', textTransform: 'uppercase', letterSpacing: '1px' }}>{t('settings.title')}</h1>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
          {/* Ses ve Bildirimler */}
          <div className="glass" style={{ padding: '20px', borderRadius: 'var(--radius-lg)' }}>
            <h3 style={{ fontSize: '0.9rem', color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '1px', margin: '0 0 16px 0' }}>{t('settings.sound_notif')}</h3>
            
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
              <span style={{ fontWeight: 700 }}>{t('settings.sound_effects')}</span>
              <div onClick={toggleSound} style={{ width: '44px', height: '24px', background: soundEnabled ? 'var(--correct)' : 'rgba(255,255,255,0.2)', borderRadius: '12px', position: 'relative', cursor: 'pointer', transition: 'background 0.3s' }}>
                <div style={{ width: '20px', height: '20px', background: 'white', borderRadius: '50%', position: 'absolute', top: '2px', left: soundEnabled ? '22px' : '2px', transition: 'left 0.3s', boxShadow: '0 2px 4px rgba(0,0,0,0.2)' }}></div>
              </div>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
              <span style={{ fontWeight: 700 }}>{t('settings.bg_music')}</span>
              <div onClick={toggleMusic} style={{ width: '44px', height: '24px', background: musicEnabled ? 'var(--correct)' : 'rgba(255,255,255,0.2)', borderRadius: '12px', position: 'relative', cursor: 'pointer', transition: 'background 0.3s' }}>
                <div style={{ width: '20px', height: '20px', background: 'white', borderRadius: '50%', position: 'absolute', top: '2px', left: musicEnabled ? '22px' : '2px', transition: 'left 0.3s', boxShadow: '0 2px 4px rgba(0,0,0,0.2)' }}></div>
              </div>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontWeight: 700 }}>{t('settings.notifications')}</span>
              <div onClick={() => setNotificationsEnabled(!notificationsEnabled)} style={{ width: '44px', height: '24px', background: notificationsEnabled ? 'var(--correct)' : 'rgba(255,255,255,0.2)', borderRadius: '12px', position: 'relative', cursor: 'pointer', transition: 'background 0.3s' }}>
                <div style={{ width: '20px', height: '20px', background: 'white', borderRadius: '50%', position: 'absolute', top: '2px', left: notificationsEnabled ? '22px' : '2px', transition: 'left 0.3s', boxShadow: '0 2px 4px rgba(0,0,0,0.2)' }}></div>
              </div>
            </div>
          </div>

          {/* Uygulama */}
          <div className="glass" style={{ padding: '20px', borderRadius: 'var(--radius-lg)' }}>
            <h3 style={{ fontSize: '0.9rem', color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '1px', margin: '0 0 16px 0' }}>{t('settings.app_settings')}</h3>
            
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontWeight: 700 }}>{t('settings.language')}</span>
              <div onClick={openLangModal} style={{ background: 'rgba(0,0,0,0.5)', color: 'white', border: '1px solid rgba(255,255,255,0.2)', borderRadius: '8px', padding: '8px 16px', cursor: 'pointer', fontWeight: 600, display: 'flex', alignItems: 'center', gap: '8px' }}>
                {i18n.language === 'tr' ? '🇹🇷 Türkçe' : '🇬🇧 English'}
                <span style={{ fontSize: '0.8rem', opacity: 0.7 }}>▼</span>
              </div>
            </div>
          </div>

          {/* Hakkında */}
          <div className="glass" style={{ padding: '20px', borderRadius: 'var(--radius-lg)' }}>
            <h3 style={{ fontSize: '0.9rem', color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '1px', margin: '0 0 16px 0' }}>{t('settings.about')}</h3>
            
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', cursor: 'pointer' }} onClick={() => addToast(t('settings.privacy_policy') + '...', 'info')}>
              <span style={{ fontWeight: 700 }}>{t('settings.privacy_policy')}</span>
              <span style={{ color: 'var(--text-muted)' }}>{'>'}</span>
            </div>
            
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', cursor: 'pointer' }} onClick={() => addToast(t('settings.terms_of_use') + '...', 'info')}>
              <span style={{ fontWeight: 700 }}>{t('settings.terms_of_use')}</span>
              <span style={{ color: 'var(--text-muted)' }}>{'>'}</span>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontWeight: 700 }}>{t('settings.version')}</span>
              <span style={{ color: 'var(--text-muted)' }}>v1.0.0</span>
            </div>
          </div>

          {/* Hesap İşlemleri */}
          <div className="glass" style={{ padding: '20px', borderRadius: 'var(--radius-lg)', border: '1px solid rgba(220, 38, 38, 0.3)' }}>
            <h3 style={{ fontSize: '0.9rem', color: 'var(--wrong)', textTransform: 'uppercase', letterSpacing: '1px', margin: '0 0 16px 0' }}>{t('settings.account_mgmt')}</h3>
            
            <button onClick={logout} style={{ width: '100%', background: 'rgba(220, 38, 38, 0.1)', border: '1px solid rgba(220, 38, 38, 0.3)', padding: '14px', borderRadius: 'var(--radius-md)', color: 'var(--wrong)', fontWeight: 800, fontSize: '0.95rem', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px', transition: 'all 0.3s', marginBottom: '12px' }} onMouseOver={e => e.currentTarget.style.background='rgba(220, 38, 38, 0.2)'} onMouseOut={e => e.currentTarget.style.background='rgba(220, 38, 38, 0.1)'}>
              <LogOutIcon /> {t('profile.btn_logout')}
            </button>
            
            <button onClick={() => setShowDeleteModal(true)} style={{ width: '100%', background: 'var(--wrong)', border: 'none', padding: '14px', borderRadius: 'var(--radius-md)', color: 'white', fontWeight: 800, fontSize: '0.95rem', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', transition: 'all 0.3s', boxShadow: '0 4px 15px rgba(220, 38, 38, 0.4)' }} onMouseOver={e => e.currentTarget.style.filter='brightness(1.1)'} onMouseOut={e => e.currentTarget.style.filter='brightness(1)'}>
              {t('settings.delete_account')}
            </button>
          </div>

        </div>

        {/* Hesap Silme Onay Modalı */}
        {showDeleteModal && (
          <div className="modal-overlay" onClick={() => !deleting && setShowDeleteModal(false)} style={{ zIndex: 1000, display: 'flex', alignItems: 'flex-end', padding: 0 }}>
            <div className="modal" onClick={e => e.stopPropagation()} style={{ width: '100%', borderBottomLeftRadius: 0, borderBottomRightRadius: 0, borderTopLeftRadius: '24px', borderTopRightRadius: '24px', padding: '32px 24px', background: 'var(--bg-primary)', borderTop: '1px solid var(--wrong)' }}>
              
              <div style={{ width: '40px', height: '4px', background: 'rgba(255,255,255,0.2)', borderRadius: '2px', margin: '0 auto 24px' }} />
              
              <div style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '3rem', marginBottom: '16px', filter: 'drop-shadow(0 0 10px rgba(220, 38, 38, 0.6))' }}>⚠️</div>
                <h2 style={{ fontSize: '1.2rem', fontWeight: 800, marginBottom: '12px', lineHeight: '1.4', color: 'var(--text-primary)' }}>
                  Emin misiniz? / Are you sure?
                </h2>
                <p style={{ color: 'var(--text-secondary)', fontSize: '0.95rem', marginBottom: '32px', lineHeight: '1.5' }}>
                  {t('settings.delete_confirm')}
                </p>
                
                <div style={{ display: 'flex', gap: '12px' }}>
                  <button onClick={() => setShowDeleteModal(false)} disabled={deleting} style={{ flex: 1, padding: '16px', background: 'rgba(255,255,255,0.1)', border: 'none', borderRadius: '12px', color: 'white', fontWeight: 700, fontSize: '1rem', cursor: deleting ? 'not-allowed' : 'pointer' }}>
                    İptal
                  </button>
                  <button onClick={handleDeleteAccount} disabled={deleting} style={{ flex: 1, padding: '16px', background: 'var(--wrong)', border: 'none', borderRadius: '12px', color: 'white', fontWeight: 800, fontSize: '1rem', cursor: deleting ? 'not-allowed' : 'pointer', boxShadow: '0 4px 15px rgba(220, 38, 38, 0.4)' }}>
                    {deleting ? '...' : t('settings.delete_account')}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Dil Seçimi Modal Overlay */}
        {showLangModal && (
          <div className="modal-overlay" onClick={() => setShowLangModal(false)} style={{ zIndex: 1000, display: 'flex', alignItems: 'flex-end', padding: 0 }}>
            <div className="modal" onClick={e => e.stopPropagation()} style={{ width: '100%', borderBottomLeftRadius: 0, borderBottomRightRadius: 0, borderTopLeftRadius: '24px', borderTopRightRadius: '24px', padding: '32px 24px', background: 'var(--bg-primary)', borderTop: '1px solid var(--accent-glow)' }}>
              
              <div style={{ width: '40px', height: '4px', background: 'rgba(255,255,255,0.2)', borderRadius: '2px', margin: '0 auto 24px' }} />
              
              {!pendingLang ? (
                <>
                  <h2 style={{ fontSize: '1.4rem', fontWeight: 800, marginBottom: '24px', textAlign: 'center', color: 'var(--text-primary)' }}>Dil Seçin / Select Language</h2>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                    <button onClick={() => setPendingLang('tr')} style={{ padding: '16px', background: i18n.language === 'tr' ? 'rgba(6, 182, 212, 0.15)' : 'rgba(255,255,255,0.05)', border: i18n.language === 'tr' ? '1px solid var(--accent-cyan)' : '1px solid transparent', borderRadius: '12px', color: 'white', fontWeight: 700, fontSize: '1.1rem', cursor: 'pointer', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span>🇹🇷 Türkçe</span>
                      {i18n.language === 'tr' && <CheckIcon />}
                    </button>
                    <button onClick={() => setPendingLang('en')} style={{ padding: '16px', background: i18n.language === 'en' ? 'rgba(6, 182, 212, 0.15)' : 'rgba(255,255,255,0.05)', border: i18n.language === 'en' ? '1px solid var(--accent-cyan)' : '1px solid transparent', borderRadius: '12px', color: 'white', fontWeight: 700, fontSize: '1.1rem', cursor: 'pointer', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span>🇬🇧 English</span>
                      {i18n.language === 'en' && <CheckIcon />}
                    </button>
                  </div>
                </>
              ) : (
                <div style={{ textAlign: 'center' }}>
                  <div style={{ fontSize: '3rem', marginBottom: '16px' }}>🤔</div>
                  <h2 style={{ fontSize: '1.2rem', fontWeight: 800, marginBottom: '16px', lineHeight: '1.4' }}>
                    <span style={{ color: 'var(--accent-cyan)' }}>{pendingLang === 'tr' ? 'Türkçe' : 'English'}</span> diline geçmeyi onaylıyor musunuz?
                  </h2>
                  <div style={{ display: 'flex', gap: '12px', marginTop: '32px' }}>
                    <button onClick={() => setPendingLang(null)} style={{ flex: 1, padding: '16px', background: 'rgba(255,255,255,0.1)', border: 'none', borderRadius: '12px', color: 'white', fontWeight: 700, fontSize: '1rem', cursor: 'pointer' }}>
                      İptal
                    </button>
                    <button onClick={confirmLanguage} style={{ flex: 1, padding: '16px', background: 'linear-gradient(135deg, var(--correct), #10b981)', border: 'none', borderRadius: '12px', color: 'white', fontWeight: 800, fontSize: '1rem', cursor: 'pointer', boxShadow: '0 4px 15px rgba(34,197,94,0.4)' }}>
                      Onayla
                    </button>
                  </div>
                </div>
              )}
              
            </div>
          </div>
        )}

      </div>
    );
  }

  return (
    <div className="page" style={{ paddingBottom: '100px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '24px' }}>
        <h1 className="page-title gradient-text" style={{ fontSize: '2rem', margin: 0, textTransform: 'uppercase', letterSpacing: '2px' }}>{t('profile.title')}</h1>
        <button onClick={() => setShowSettings(true)} style={{ background: 'transparent', border: 'none', color: 'var(--text-secondary)', cursor: 'pointer', transition: 'color 0.3s', padding: '8px' }} onMouseOver={e => e.currentTarget.style.color='white'} onMouseOut={e => e.currentTarget.style.color='var(--text-secondary)'}>
          <SettingsIcon />
        </button>
      </div>

      {/* Avatar & İsim */}
      <div className="glass" style={{ textAlign: 'center', padding: '32px 20px', marginBottom: '24px', borderRadius: 'var(--radius-xl)', position: 'relative', overflow: 'hidden', border: '1px solid rgba(139, 92, 246, 0.2)' }}>
        <div style={{ position: 'absolute', top: '-50%', left: '-50%', width: '200%', height: '200%', background: 'radial-gradient(circle, rgba(139,92,246,0.1) 0%, transparent 50%)', zIndex: 0, animation: 'spin 20s linear infinite' }}></div>
        
        <div style={{ position: 'relative', zIndex: 1 }}>
          <div style={{ width: 90, height: 90, borderRadius: '24px', background: 'linear-gradient(135deg, var(--accent), var(--accent-pink))', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '2.5rem', fontWeight: 900, color: 'white', margin: '0 auto 16px', boxShadow: '0 10px 30px var(--accent-glow)', textShadow: '0 2px 10px rgba(0,0,0,0.3)', transform: 'rotate(-5deg)' }}>
            <div style={{ transform: 'rotate(5deg)' }}>
              {(user?.username || '?')[0]?.toUpperCase()}
            </div>
          </div>
          
          {!editing ? (
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
              <h2 style={{ fontSize: '1.6rem', fontWeight: 900, letterSpacing: '1px', textShadow: '0 2px 10px rgba(0,0,0,0.5)' }}>{user?.username}</h2>
              <div style={{ color: currentRank.color, fontSize: '0.85rem', fontWeight: 800, textTransform: 'uppercase', letterSpacing: '2px', marginTop: '4px', marginBottom: '16px', textShadow: `0 0 10px ${currentRank.color}` }}>{currentRank.title}</div>
              
              <button onClick={() => setEditing(true)} style={{ background: 'rgba(255,255,255,0.1)', border: '1px solid rgba(255,255,255,0.2)', padding: '8px 16px', borderRadius: 'var(--radius-full)', color: 'white', fontSize: '0.85rem', fontWeight: 700, cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '8px', transition: 'all 0.3s' }} onMouseOver={e => e.currentTarget.style.background='rgba(255,255,255,0.2)'} onMouseOut={e => e.currentTarget.style.background='rgba(255,255,255,0.1)'}>
                <EditIcon /> {t('profile.edit')}
              </button>
            </div>
          ) : (
            <div style={{ display: 'flex', gap: '8px', marginTop: '16px', maxWidth: '300px', margin: '16px auto 0' }}>
              <input value={newName} onChange={e => setNewName(e.target.value)} style={{ flex: 1, padding: '12px 16px', borderRadius: 'var(--radius-md)', background: 'rgba(0,0,0,0.5)', border: '1px solid var(--accent)', color: 'white', outline: 'none', fontWeight: 700, textAlign: 'center', letterSpacing: '1px' }} autoFocus />
              <button onClick={handleUpdateName} style={{ background: 'var(--correct)', border: 'none', width: '44px', borderRadius: 'var(--radius-md)', color: 'white', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center', boxShadow: '0 0 15px var(--correct-glow)' }}>
                <CheckIcon />
              </button>
              <button onClick={() => setEditing(false)} style={{ background: 'rgba(255,255,255,0.1)', border: '1px solid rgba(255,255,255,0.2)', width: '44px', borderRadius: 'var(--radius-md)', color: 'white', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <XIcon />
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Bakiye */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '24px' }}>
        <div className="glass" style={{ textAlign: 'center', padding: '24px', background: 'linear-gradient(135deg, rgba(245,158,11,0.15), rgba(245,158,11,0.05))', borderTop: '2px solid rgba(245,158,11,0.5)', borderRadius: 'var(--radius-lg)' }}>
          <div style={{ fontSize: '2.5rem', fontWeight: 900, color: 'var(--gold)', textShadow: '0 0 20px var(--gold-glow)', lineHeight: 1 }}>{user?.gold || 0}</div>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', fontWeight: 800, textTransform: 'uppercase', letterSpacing: '2px', marginTop: '8px' }}>{t('profile.gold')}</div>
        </div>
        <div className="glass" style={{ textAlign: 'center', padding: '24px', background: 'linear-gradient(135deg, rgba(6,182,212,0.15), rgba(6,182,212,0.05))', borderTop: '2px solid rgba(6,182,212,0.5)', borderRadius: 'var(--radius-lg)' }}>
          <div style={{ fontSize: '2.5rem', fontWeight: 900, color: 'var(--diamond)', textShadow: '0 0 20px var(--diamond-glow)', lineHeight: 1 }}>{user?.diamonds || 0}</div>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', fontWeight: 800, textTransform: 'uppercase', letterSpacing: '2px', marginTop: '8px' }}>{t('profile.diamonds')}</div>
        </div>
      </div>

      {/* İstatistikler */}
      <div className="glass" style={{ marginBottom: '24px', padding: '24px', borderRadius: 'var(--radius-lg)', borderLeft: '4px solid var(--accent-pink)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '20px' }}>
          <div style={{ padding: '8px', background: 'rgba(236, 72, 153, 0.1)', borderRadius: '8px', color: 'var(--accent-pink)' }}>
            <StatsIcon />
          </div>
          <h3 style={{ fontSize: '1.1rem', fontWeight: 800, textTransform: 'uppercase', letterSpacing: '1px', margin: 0 }}>{t('profile.battle_records')}</h3>
        </div>
        
        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', paddingBottom: '16px', borderBottom: '1px solid rgba(255,255,255,0.05)' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.95rem', fontWeight: 600 }}>{t('profile.matches_played')}</span>
            <span style={{ fontWeight: 900, fontSize: '1.2rem', color: 'white' }}>{totalMatches}</span>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', paddingBottom: '16px', borderBottom: '1px solid rgba(255,255,255,0.05)' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.95rem', fontWeight: 600 }}>{t('profile.win_rate')}</span>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <span style={{ fontWeight: 900, fontSize: '1.2rem', color: winRate >= 50 ? 'var(--correct)' : 'var(--wrong)', textShadow: `0 0 10px ${winRate >= 50 ? 'var(--correct-glow)' : 'var(--wrong-glow)'}` }}>%{winRate}</span>
              {winRate >= 50 ? <span style={{ fontSize: '1.2rem' }}>🔥</span> : <span style={{ fontSize: '1.2rem' }}>🥶</span>}
            </div>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.95rem', fontWeight: 600 }}>{t('profile.wins')}</span>
            <span style={{ fontWeight: 900, fontSize: '1.2rem', color: 'var(--correct)' }}>{user?.totalWins || 0}</span>
          </div>
        </div>
      </div>

      {/* Reklam Ödülü */}
      <div className="glass" style={{ marginBottom: '32px', padding: '20px', background: 'linear-gradient(135deg, rgba(6,182,212,0.1), rgba(139,92,246,0.1))', border: '1px solid rgba(6,182,212,0.3)', borderRadius: 'var(--radius-lg)', position: 'relative', overflow: 'hidden' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '16px', position: 'relative', zIndex: 1 }}>
          <div style={{ width: '50px', height: '50px', borderRadius: '50%', background: adTimeLeft ? 'rgba(255,255,255,0.1)' : 'linear-gradient(135deg, var(--diamond), #0284c7)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white', boxShadow: adTimeLeft ? 'none' : '0 0 20px var(--diamond-glow)' }}>
            <PlayIcon />
          </div>
          <div style={{ flex: 1 }}>
            <h3 style={{ fontSize: '1rem', fontWeight: 800, margin: 0, textTransform: 'uppercase', letterSpacing: '0.5px' }}>{t('profile.ad_title')}</h3>
            <p style={{ fontSize: '0.8rem', color: adTimeLeft ? 'var(--accent-pink)' : 'var(--correct)', fontWeight: 700, margin: '4px 0 0' }}>
              {adTimeLeft ? t('profile.ad_wait', { time: adTimeLeft }) : t('profile.ad_ready')}
            </p>
          </div>
          <button onClick={handleClaimAd} disabled={claiming || adTimeLeft !== null} style={{ background: adTimeLeft ? 'rgba(255,255,255,0.05)' : 'linear-gradient(135deg, var(--correct), #059669)', border: 'none', padding: '10px 20px', borderRadius: 'var(--radius-full)', color: adTimeLeft ? 'rgba(255,255,255,0.3)' : 'white', fontWeight: 800, fontSize: '0.9rem', cursor: adTimeLeft ? 'not-allowed' : 'pointer', boxShadow: adTimeLeft ? 'none' : '0 4px 15px var(--correct-glow)', transition: 'all 0.3s' }}>
            {claiming ? '...' : (adTimeLeft ? t('profile.btn_wait') : t('profile.btn_watch'))}
          </button>
        </div>
      </div>

    </div>
  );
}
