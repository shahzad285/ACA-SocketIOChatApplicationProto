import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import io, { Socket } from 'socket.io-client';

const Home: React.FC = () => {
    const navigate = useNavigate();
    const [socket, setSocket] = useState<Socket | null>(null);
    const [messages, setMessages] = useState<string[]>([]);
    const [status, setStatus] = useState<string | null>(null);

    useEffect(() => {
        const token = localStorage.getItem('jwtToken');
        if (!token || isTokenExpired(token)) {
            navigate('/signin');
        } else {
            // Create a Socket.IO connection with the token in the query string
            const newSocket = io('https://localhost:7113', {
                query: {
                    token: token,
                },
                transports: ['websocket'], // Ensure WebSocket is used as the transport method
            });

            newSocket.on('connect', () => {
                console.log('Connected to WebSocket server');
                setStatus('Connected');

                // Send "ImAlive" status every 8 seconds
                const intervalId = setInterval(() => {
                    console.log('Emitting ImAlive event');  // Log this to verify
                    if (newSocket.connected) {
                        newSocket.emit('ImAlive', { status: 'I am alive' });
                        setStatus('Alive status sent');
                    }
                }, 8000);

                // Cleanup interval on component unmount
                return () => clearInterval(intervalId);
            });

            newSocket.on('message', (message: string) => {
                setMessages((prevMessages) => [...prevMessages, message]);
            });

            newSocket.on('disconnect', () => {
                console.log('Disconnected from WebSocket server');
                setStatus('Disconnected');
            });

            setSocket(newSocket);

            return () => {
                newSocket.disconnect();
            };
        }
    }, [navigate]);

    const isTokenExpired = (token: string): boolean => {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const exp = payload.exp;
            if (exp) {
                return Date.now() >= exp * 1000;
            }
        } catch (e) {
            console.error('Error decoding token:', e);
            return true;
        }
        return false;
    };

    return (
        <div className="flex flex-col justify-center items-center h-screen bg-gray-100">
            <h1 className="text-3xl font-bold mb-4">Welcome to the Home Page!</h1>
            <h2 className="text-2xl">Socket.IO Messages</h2>
            <div className="mt-4 p-4 bg-white rounded shadow-lg w-1/2">
                {messages.length > 0 ? (
                    <ul>
                        {messages.map((message, index) => (
                            <li key={index} className="text-lg">{message}</li>
                        ))}
                    </ul>
                ) : (
                    <p>No messages yet.</p>
                )}
            </div>
        </div>
    );
};

export default Home;
