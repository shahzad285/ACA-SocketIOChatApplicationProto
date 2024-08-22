import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { io, Socket } from 'socket.io-client';

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
           

            // Function to call the /Chat/ImAlive API
            const callIamAlive = async () => {
                try {
                    const response = await fetch('https://localhost:7113/Chat/ImAlive', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer ${token}`, // Include token in headers
                        },
                    });

                    if (response.ok) {
                        setStatus('Alive status updated');
                    } else {
                        console.error('Failed to call ImAlive API:', response.statusText);
                        setStatus('Failed to update status');
                    }
                } catch (error) {
                    console.error('Error calling ImAlive API:', error);
                    setStatus('Error updating status');
                }
            };

            // Call the API every 10 seconds
            const intervalId = setInterval(callIamAlive, 10000);

            // Call it immediately on component mount
            callIamAlive();
            // Cleanup function to clear the interval when the component unmounts
            return () => {
                clearInterval(intervalId);
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
