import { useState } from 'react';
import { useRegister } from '../hooks/user-register';
import type RegisterRequest from '../models/requests/RegisterRequest';

interface RegisterModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const RegisterModal: React.FC<RegisterModalProps> = ({ isOpen, onClose }) => {
 
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const register = useRegister();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
   
    try {
      await register.mutateAsync({email, password} as RegisterRequest)
      
      setEmail('');
      setPassword('');
    } catch (err) {
    
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-8 max-w-md w-full">
        <h2 className="text-2xl font-bold mb-6">Register</h2>
        <form onSubmit={handleSubmit}>
         
          <div className="mb-4">
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Email
            </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
          <div className="mb-6">
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Password
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
          {register.isSuccess && (
            <div className="text-green-600">Registration is successful! You need to confirm your email (log in to your email account and follow a link), and then you will be able to sign in to your account </div>
          )}
          {register.error && (
            <div className="mb-4 text-red-500 text-sm">{register.error.message}</div>
          )}
          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-gray-600 hover:text-gray-800"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={register.isPending}
              className="px-4 py-2 bg-green-500 text-white rounded-md hover:bg-green-600 disabled:opacity-50"
            >
              {register.isPending ? 'Registering...' : 'Register'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};