import { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';

export default function LoginPage() {
  const [isLogin, setIsLogin] = useState(true);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { login, register } = useAuth();
  const { addToast } = useToast();

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!username.trim() || !password.trim()) return;
    setLoading(true);
    try {
      if (isLogin) {
        await login(username, password);
        addToast('Hoş geldiniz!', 'success');
      } else {
        await register(username, password);
        addToast('Hesap oluşturuldu!', 'success');
      }
    } catch (err) {
      addToast(err.message, 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page" style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '100vh', paddingBottom: '16px' }}>
      <div style={{ textAlign: 'center', marginBottom: '40px', animation: 'slideUp 0.6s ease-out' }}>
        <div style={{ fontSize: '4rem', marginBottom: '8px', filter: 'drop-shadow(0 0 16px rgba(139, 92, 246, 0.6))' }}>⚔️</div>
        <h1 className="gradient-text" style={{ fontSize: '2.5rem', fontWeight: 900, letterSpacing: '-0.05em', textTransform: 'uppercase' }}>Kelime Arenası</h1>
        <p style={{ color: 'var(--text-muted)', marginTop: '8px', fontSize: '0.9rem', fontWeight: 500, letterSpacing: '1px' }}>Zekanı konuştur, rakibini yok et!</p>
      </div>

      <div className="glass" style={{ width: '100%', maxWidth: '380px', borderRadius: 'var(--radius-xl)', padding: '28px' }}>
        <div style={{ display: 'flex', gap: '4px', marginBottom: '24px', background: 'rgba(0,0,0,0.3)', borderRadius: 'var(--radius-lg)', padding: '6px' }}>
          <button className={`btn btn-sm btn-full ${isLogin ? 'btn-primary' : 'btn-ghost'}`} onClick={() => setIsLogin(true)} style={{ flex: 1, border: 'none', borderRadius: 'var(--radius-md)' }}>Giriş Yap</button>
          <button className={`btn btn-sm btn-full ${!isLogin ? 'btn-primary' : 'btn-ghost'}`} onClick={() => setIsLogin(false)} style={{ flex: 1, border: 'none', borderRadius: 'var(--radius-md)' }}>Kayıt Ol</button>
        </div>

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
          <div>
            <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 600, color: 'var(--text-secondary)', marginBottom: '6px' }}>Kullanıcı Adı</label>
            <input className="input" type="text" placeholder="kullanıcı adınız" value={username} onChange={e => setUsername(e.target.value)} autoComplete="username" />
          </div>
          <div>
            <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 600, color: 'var(--text-secondary)', marginBottom: '6px' }}>Şifre</label>
            <input className="input" type="password" placeholder="••••••••" value={password} onChange={e => setPassword(e.target.value)} autoComplete="current-password" />
          </div>
          <button className="btn btn-primary btn-lg btn-full" type="submit" disabled={loading} style={{ marginTop: '8px' }}>
            {loading ? <span className="spinner" style={{ width: 20, height: 20, borderWidth: 2 }} /> : isLogin ? '🎮 Giriş Yap' : '🚀 Kayıt Ol'}
          </button>
        </form>
      </div>
    </div>
  );
}
