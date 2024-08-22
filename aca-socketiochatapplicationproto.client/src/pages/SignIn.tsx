import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const SignIn: React.FC = () => {
    const [username, setUsername] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        const token = localStorage.getItem('jwtToken');
        if (token) {
            const isExpired = checkTokenExpiry(token);
            if (!isExpired) {
                navigate('/');
            } else {
                localStorage.removeItem('jwtToken');
            }
        }
    }, [navigate]);

    const handleSignIn = async (e: React.FormEvent) => {
        e.preventDefault();

        const loginData = {
            userName: username,
            password: password
        };

        try {
            const response = await fetch('https://localhost:7113/Chat/Login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(loginData),
            });

            if (response.ok) {
                const token = await response.text(); // Expecting a plain token string as the response
                if (token === "Username or password is incorrect") {
                    setError(token);
                } else {
                    localStorage.setItem('jwtToken', token);
                    navigate('/');
                }
            } else {
                setError('Login failed. Please try again.');
            }
        } catch (error) {
            console.error('Error during login:', error);
            setError('An unexpected error occurred. Please try again.');
        }
    };

    const checkTokenExpiry = (token: string): boolean => {
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
        <div className="flex justify-center items-center h-screen bg-gray-100">
            <div className="w-full max-w-md bg-white p-8 rounded shadow-lg">
                <h2 className="text-2xl font-bold mb-6 text-center">Sign In</h2>
                <form onSubmit={handleSignIn}>
                    {error && <p className="text-red-500 mb-4">{error}</p>}
                    <div className="mb-4">
                        <label className="block text-gray-700 mb-2">Username</label>
                        <input
                            type="text"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            required
                            className="w-full px-3 py-2 border rounded"
                        />
                    </div>
                    <div className="mb-6">
                        <label className="block text-gray-700 mb-2">Password</label>
                        <input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            className="w-full px-3 py-2 border rounded"
                        />
                    </div>
                    <button
                        type="submit"
                        className="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600"
                    >
                        Sign In
                    </button>
                </form>
            </div>
        </div>
    );
};

export default SignIn;
