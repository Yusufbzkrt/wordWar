import { createContext, useContext, useEffect, useState, useRef } from 'react';
import { Howl } from 'howler';

const AudioContext = createContext(null);

export const useAudio = () => useContext(AudioContext);

export const AudioProvider = ({ children }) => {
  // Oku cihaz hafızasından:
  const storedMusic = localStorage.getItem('musicEnabled') !== 'false';
  const storedSound = localStorage.getItem('soundEnabled') !== 'false';

  const [musicEnabled, setMusicEnabled] = useState(storedMusic);
  const [soundEnabled, setSoundEnabled] = useState(storedSound);

  // Howler instances
  const bgmRef = useRef(null);
  const clickRef = useRef(null);

  useEffect(() => {
    // 1. Arkaplan müziği
    bgmRef.current = new Howl({
      src: ['/sounds/bg-music.mp3'],
      loop: true,
      volume: 0.3,
      autoplay: musicEnabled, // İlk açılışta musicEnabled true ise çal
    });

    // 2. Tıklama Sesi
    clickRef.current = new Howl({
      src: ['/sounds/click.wav'],
      volume: 0.8,
    });

    // Sayfa kapatılırken sesleri temizle
    return () => {
      bgmRef.current?.unload();
      clickRef.current?.unload();
    };
  }, []);

  // Müzik ayarı değiştiğinde Howler'a müdahale et
  useEffect(() => {
    localStorage.setItem('musicEnabled', musicEnabled);
    if (!bgmRef.current) return;

    if (musicEnabled) {
      if (!bgmRef.current.playing()) {
        bgmRef.current.play();
      }
    } else {
      bgmRef.current.pause();
    }
  }, [musicEnabled]);

  // Ses ayarı değiştiğinde
  useEffect(() => {
    localStorage.setItem('soundEnabled', soundEnabled);
  }, [soundEnabled]);

  // Dışarıya açılacak fonksiyonlar
  const toggleMusic = () => setMusicEnabled(prev => !prev);
  const toggleSound = () => setSoundEnabled(prev => !prev);

  const playClick = () => {
    if (soundEnabled && clickRef.current) {
      clickRef.current.play();
    }
  };

  return (
    <AudioContext.Provider value={{
      musicEnabled,
      soundEnabled,
      toggleMusic,
      toggleSound,
      playClick
    }}>
      {children}
    </AudioContext.Provider>
  );
};
