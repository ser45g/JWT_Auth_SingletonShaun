import React, { useContext, useState } from 'react';
import { useDeleteAccount } from '../hooks/use-delete-account';

interface DeleteAccountModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const DeleteAccountModal: React.FC<DeleteAccountModalProps> = ({ isOpen, onClose }) => {
  
  const [confirmText, setConfirmText] = useState('');
  const [error, setError] = useState('');
  
  const deleteAccount = useDeleteAccount();

  const handleDelete = async () => {
    if (confirmText !== 'delete my account') {
      setError('Please type "delete my account" to confirm');
      return;
    }
    try {
      await deleteAccount.mutateAsync();

      
      onClose();
      setConfirmText('');
    } catch (err) {
     
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-8 max-w-md w-full">
        <h2 className="text-2xl font-bold mb-4 text-red-600">Delete Account</h2>
        <p className="text-gray-600 mb-4">
          This action cannot be undone. Type <strong>"delete my account"</strong> below to confirm.
        </p>
        <input
          type="text"
          value={confirmText}
          onChange={(e) => setConfirmText(e.target.value)}
          placeholder="Type 'delete my account'"
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-red-500 mb-4"
        />
        {error && (
          <div className="mb-4 text-red-500 text-sm">{error}</div>
        )}

        {deleteAccount.error &&(
          <div className="mb-4 text-red-500 text-sm">{deleteAccount.error.message}</div>
        )}
        <div className="flex justify-end space-x-3">
          <button
            onClick={onClose}
            className="px-4 py-2 text-gray-600 hover:text-gray-800"
          >
            Cancel
          </button>
          <button
            onClick={handleDelete}
            disabled={deleteAccount.isPending}
            className="px-4 py-2 bg-red-500 text-white rounded-md hover:bg-red-600 disabled:opacity-50"
          >
            {deleteAccount.isPending ? 'Deleting...' : 'Delete Account'}
          </button>
        </div>
      </div>
    </div>
  );
};