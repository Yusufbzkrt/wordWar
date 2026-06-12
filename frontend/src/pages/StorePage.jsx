export default function StorePage() {
  const storeItems = [
    { id: 'gold_100', name: '100 Altın', emoji: '🪙', amount: '100', currency: 'Altın', price: '₺9.99', gradient: 'rgba(245,158,11,0.1)', border: 'rgba(245,158,11,0.2)' },
    { id: 'gold_500', name: '500 Altın', emoji: '🪙', amount: '500', currency: 'Altın', price: '₺39.99', gradient: 'rgba(245,158,11,0.15)', border: 'rgba(245,158,11,0.3)', popular: true },
    { id: 'gold_1000', name: '1000 Altın', emoji: '🪙', amount: '1000', currency: 'Altın', price: '₺69.99', gradient: 'rgba(245,158,11,0.1)', border: 'rgba(245,158,11,0.2)' },
    { id: 'diamond_10', name: '10 Elmas', emoji: '💎', amount: '10', currency: 'Elmas', price: '₺19.99', gradient: 'rgba(6,182,212,0.1)', border: 'rgba(6,182,212,0.2)' },
    { id: 'diamond_50', name: '50 Elmas', emoji: '💎', amount: '50', currency: 'Elmas', price: '₺79.99', gradient: 'rgba(6,182,212,0.15)', border: 'rgba(6,182,212,0.3)', popular: true },
    { id: 'starter', name: 'Başlangıç Paketi', emoji: '🎁', amount: '300🪙 + 15💎', currency: '', price: '₺29.99', gradient: 'rgba(139,92,246,0.15)', border: 'rgba(139,92,246,0.3)', popular: true },
  ];

  return (
    <div className="page">
      <h1 className="page-title">🛒 Market</h1>
      <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
        {storeItems.map(item => (
          <div key={item.id} className="card" style={{ display: 'flex', alignItems: 'center', gap: '14px', padding: '16px', background: `linear-gradient(135deg, ${item.gradient}, transparent)`, borderColor: item.border, position: 'relative', overflow: 'hidden' }}>
            {item.popular && <div style={{ position: 'absolute', top: 8, right: -28, background: 'var(--accent)', color: 'white', padding: '2px 32px', fontSize: '0.6rem', fontWeight: 700, transform: 'rotate(45deg)' }}>POPÜLER</div>}
            <div style={{ fontSize: '2rem', flexShrink: 0 }}>{item.emoji}</div>
            <div style={{ flex: 1 }}>
              <div style={{ fontWeight: 700, fontSize: '0.95rem' }}>{item.name}</div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{item.amount} {item.currency}</div>
            </div>
            <button className="btn btn-primary btn-sm">{item.price}</button>
          </div>
        ))}
      </div>
    </div>
  );
}
