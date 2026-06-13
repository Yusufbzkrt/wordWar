import React from 'react';

const DuelAvatar = ({ isOpponent, isTyping, timeLeft, playerName, gender = 'boy', isActive = false }) => {
  const isStressed = isActive && timeLeft !== null && timeLeft <= 10;
  
  // Animasyon sınıfları (Tailwind)
  let animationClass = 'animate-pulse-slow'; // idle
  if (isStressed && isTyping) {
    animationClass = 'animate-shake';
  } else if (isStressed && !isTyping) {
    animationClass = 'animate-pulse';
  } else if (!isStressed && isTyping) {
    animationClass = 'animate-bounce';
  }

  // Renk kodları (kendi ve rakip ayrımı)
  const baseColor = isOpponent ? 'bg-red-500/20 border-red-500/50' : 'bg-blue-500/20 border-blue-500/50';
  const glowColor = isOpponent ? 'shadow-[0_0_15px_rgba(239,68,68,0.5)]' : 'shadow-[0_0_15px_rgba(59,130,246,0.5)]';
  const typingTextColor = isOpponent ? 'text-red-400' : 'text-blue-400';

  return (
    <div className={`flex flex-col items-center ${isOpponent ? 'order-last' : 'order-first'}`}>
      
      {/* İsim ve Yazıyor Bildirimi */}
      <div className="h-8 flex flex-col items-center justify-end mb-2">
        <span className="text-white font-bold text-lg drop-shadow-md">{playerName || (isOpponent ? 'Rakip' : 'Sen')}</span>
        {isTyping && (
          <span className={`text-xs font-semibold ${typingTextColor} animate-pulse`}>
            yazıyor...
          </span>
        )}
      </div>

      {/* Avatar Kapsayıcısı */}
      <div 
        className={`
          relative w-24 h-24 rounded-full border-2 backdrop-blur-md flex items-center justify-center
          transition-all duration-300
          ${baseColor}
          ${isTyping ? glowColor : ''}
          ${isStressed ? 'border-orange-500/80 shadow-[0_0_20px_rgba(249,115,22,0.6)] bg-orange-500/10' : ''}
          ${animationClass}
        `}
        style={{ transform: isOpponent ? 'scaleX(-1)' : 'scaleX(1)' }}
      >
        <svg 
          viewBox="0 0 100 100" 
          className={`w-20 h-20 transition-colors duration-300 ${isStressed ? 'text-orange-300' : 'text-white'} mt-4`} 
          fill="currentColor"
        >
          {/* Masa (Desk) */}
          <rect x="5" y="80" width="90" height="20" fill="rgba(30, 41, 59, 0.8)" rx="4" />
          
          {/* Vücut */}
          <path d="M25 90 Q50 40 75 90 Z" opacity="0.8" />
          
          {/* Kafa */}
          <circle cx="50" cy="35" r="18" opacity="0.9" />
          
          {/* Gözler */}
          {isStressed ? (
            <g stroke="#1e293b" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" fill="none">
              <path d="M40 31 L44 34 L40 37" />
              <path d="M60 31 L56 34 L60 37" />
            </g>
          ) : (
            <g fill="#1e293b">
              <circle cx="43" cy="33" r="3" />
              <circle cx="57" cy="33" r="3" />
            </g>
          )}
          
          {/* Stres Durumunda Ter Damlası */}
          {isStressed && (
             <path d="M65 20 Q65 25 62 25 Q59 25 59 20 Q62 15 65 20 Z" fill="#38bdf8" className="animate-bounce" />
          )}
          
          {/* Ağız */}
          {isStressed ? (
            <path d="M45 44 L48 41 L51 44 L54 41 L57 44" stroke="#1e293b" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" fill="none" />
          ) : isTyping ? (
            <path d="M46 41 Q50 47 54 41" stroke="#1e293b" strokeWidth="2" fill="none" />
          ) : (
            <path d="M46 43 L54 43" stroke="#1e293b" strokeWidth="2" fill="none" />
          )}

          {/* Laptop / Klavye */}
          <path d="M30 82 L70 82 L75 90 L25 90 Z" fill="rgba(148, 163, 184, 0.9)" />
          <rect x="35" y="84" width="30" height="3" fill="rgba(51, 65, 85, 0.8)" rx="1" />
          
          {/* Sol Kol ve El */}
          <g className={isTyping ? 'animate-typing-left' : ''}>
            <path d="M35 60 Q25 70 35 83" stroke="currentColor" strokeWidth="5" fill="none" strokeLinecap="round" opacity="0.9" />
            <circle cx="35" cy="83" r="4" fill={isStressed ? '#fdba74' : '#fcd34d'} />
          </g>

          {/* Sağ Kol ve El */}
          <g className={isTyping ? 'animate-typing-right' : ''}>
            <path d="M65 60 Q75 70 65 83" stroke="currentColor" strokeWidth="5" fill="none" strokeLinecap="round" opacity="0.9" />
            <circle cx="65" cy="83" r="4" fill={isStressed ? '#fdba74' : '#fcd34d'} />
          </g>

          {/* Aksesuarlar (Kız / Erkek) */}
          {gender === 'girl' && (
            <path d="M35 30 Q50 15 65 30 Q55 10 45 10 Q35 15 35 30 Z" fill="rgba(236, 72, 153, 0.8)" />
          )}
          {gender === 'boy' && (
            <path d="M30 35 Q50 15 70 35 L65 20 Q50 10 35 20 Z" fill="rgba(59, 130, 246, 0.8)" />
          )}
        </svg>

        {/* Emoji (İsteğe Bağlı Süsleme) */}
        {isTyping && (
          <div className="absolute -bottom-2 -right-2 text-2xl" style={{ transform: isOpponent ? 'scaleX(-1)' : 'scaleX(1)' }}>
            ⌨️
          </div>
        )}
      </div>
      
    </div>
  );
};

export default DuelAvatar;
