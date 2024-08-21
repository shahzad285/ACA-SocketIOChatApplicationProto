import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const SignIn: React.FC = () => {
    const [email, setEmail] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const navigate = useNavigate();

    useEffect(() => {
        const token = localStorage.getItem('jwtToken');
        if (token) {
            const isExpired = checkTokenExpiry(token);
            if (!isExpired) {
                // Redirect to a protected route if the token exists and is not expired
                navigate('/');
            } else {
                // Optionally, you can clear the expired token from localStorage
                localStorage.removeItem('jwtToken');
            }
        }
    }, [navigate]);

    const handleSignIn = (e: React.FormEvent) => {
        e.preventDefault();
        // Handle sign in logic here, such as calling an API and storing the JWT in localStorage
        console.log('Signing in with', { email, password });

        // Mock response for demonstration
        const mockToken = "your.jwt.token.here"; // Replace with actual JWT token
        localStorage.setItem('jwtToken', mockToken);

        // Redirect to the protected route after successful sign-in
        navigate('/');
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
            return true; // Assume the token is expired if an error occurs
        }
        return false;
    };

    return (
        <div className="flex justify-center items-center h-screen bg-gray-100">
            <div className="w-full max-w-md bg-white p-8 rounded shadow-lg">
                <h2 className="text-2xl font-bold mb-6 text-center">Sign In</h2>
                <form onSubmit={handleSignIn}>
                    <div className="mb-4">
                        <label className="block text-gray-700 mb-2">Email</label>
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
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