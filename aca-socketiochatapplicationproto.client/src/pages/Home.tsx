import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { io, Socket } from 'socket.io-client';

const Home: React.FC = () => {
    const navigate = useNavigate();
    const [socket, setSocket] = useState<Socket | null>(null);
    const [messages, setMessages] = useState<string[]>([]);

    useEffect(() => {
        const token = localStorage.getItem('jwtToken');
        if (!token || isTokenExpired(token)) {
            // Redirect to sign-in page if token is missing or expired
            navigate('/signin');
        } else {
            // Establish Socket.IO connection
            const newSocket = io('http://localhost:3000', {
                auth: {
                    token: `Bearer ${token}`, // Pass the JWT token with the connection
                },
            });

            setSocket(newSocket);

            newSocket.on('connect', () => {
                console.log('Connected to the server');
            });

            newSocket.on('disconnect', () => {
                console.log('Disconnected from the server');
            });

            // Listen for incoming messages
            newSocket.on('message', (message: string) => {
                setMessages((prevMessages) => [...prevMessages, message]);
            });

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
            return true; // Assume the token is expired if an error occurs
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
