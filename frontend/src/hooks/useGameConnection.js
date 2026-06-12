import { useState, useEffect, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { GAME_HUB_URL } from '../utils/constants';

export function useGameConnection() {
  const [connection, setConnection] = useState(null);
  const [connected, setConnected] = useState(false);
  const connectionRef = useRef(null);

  const connect = useCallback(async () => {
    const token = localStorage.getItem('token');
    if (!token) return;

    const conn = new signalR.HubConnectionBuilder()
      .withUrl(GAME_HUB_URL, { accessTokenFactory: () => token })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    conn.onreconnecting(() => setConnected(false));
    conn.onreconnected(() => setConnected(true));
    conn.onclose(() => setConnected(false));

    try {
      await conn.start();
      setConnected(true);
      setConnection(conn);
      connectionRef.current = conn;
    } catch (err) {
      console.error('GameHub bağlantı hatası:', err);
    }
  }, []);

  const disconnect = useCallback(async () => {
    if (connectionRef.current) {
      await connectionRef.current.stop();
      setConnected(false);
      setConnection(null);
    }
  }, []);

  useEffect(() => {
    return () => { connectionRef.current?.stop(); };
  }, []);

  return { connection, connected, connect, disconnect };
}
