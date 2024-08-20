import React from 'react';
import { BrowserRouter as Router, Route, Routes, Link } from 'react-router-dom';
import SignIn from './pages/SignIn';
import SignUp from './pages/SignUp';
import './App.css';

const App: React.FC = () => {
    return (
        <Router>
            <div className="flex justify-center items-center h-screen bg-gray-100">
                <Routes>
                    {/* Default Route to Sign In Page */}
                    <Route path="/" element={<SignIn />} />
                    <Route path="/signin" element={<SignIn />} />
                    <Route path="/signup" element={<SignUp />} />
                </Routes>

                {/* Navigation Links */}
                <div className="absolute bottom-10 text-center">
                    <p className="text-gray-700">
                        Don't have an account? 
                        <Link to="/signup" className="text-blue-500 hover:text-blue-700 ml-2">Sign Up</Link>
                    </p>
                </div>
            </div>
        </Router>
    );
}

export default App;