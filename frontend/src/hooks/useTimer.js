import { useState, useEffect, useRef, useCallback } from 'react';

export function useTimer(initialSeconds = 30, onTimeUp) {
  const [seconds, setSeconds] = useState(initialSeconds);
  const [isRunning, setIsRunning] = useState(false);
  const intervalRef = useRef(null);
  const onTimeUpRef = useRef(onTimeUp);
  onTimeUpRef.current = onTimeUp;

  const start = useCallback(() => setIsRunning(true), []);
  const stop = useCallback(() => setIsRunning(false), []);
  const reset = useCallback((newSeconds = initialSeconds) => {
    setSeconds(newSeconds);
    setIsRunning(false);
  }, [initialSeconds]);
  const addTime = useCallback((extra) => setSeconds(prev => prev + extra), []);

  useEffect(() => {
    if (!isRunning) return;
    intervalRef.current = setInterval(() => {
      setSeconds(prev => {
        if (prev <= 1) {
          clearInterval(intervalRef.current);
          setIsRunning(false);
          onTimeUpRef.current?.();
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(intervalRef.current);
  }, [isRunning]);

  const timerColor = seconds <= 5 ? '#EF4444' : seconds <= 10 ? '#F59E0B' : '#22C55E';
  const progress = (seconds / initialSeconds) * 100;

  return { seconds, isRunning, start, stop, reset, addTime, timerColor, progress };
}
