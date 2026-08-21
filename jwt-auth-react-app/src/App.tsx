import { useContext, useState } from 'react';
import { LoginModal } from './components/LoginModal';
import { RegisterModal } from './components/RegisterModal';
import { DeleteAccountModal } from './components/DeleteAccountModal';
import { StoreContext } from './main';
import { observer } from 'mobx-react-lite';

function App() {

  const [showLogin, setShowLogin] = useState(false);
  const [showRegister, setShowRegister] = useState(false);
  const [showDelete, setShowDelete] = useState(false);
  const { store } = useContext(StoreContext);

  async function getUserInfo(){
    const response = await store.getAccountInfo();
    if(response.status === 200){
      store.setUser(response.data);
    }else{
      alert("Couldn't get user info!");
    }
  }

  return (
    <div className="min-h-screen bg-linear-to-br from-blue-50 to-indigo-100">
      {/* Navigation */}
      <nav className="bg-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-bold text-gray-800">MyApp</h1>
            </div>
            <div className="flex items-center space-x-4">
              {store.isAuthenticated ? (
                <>
                  <span className="text-gray-700">
                    Welcome, <span className="font-semibold">{"user"}</span>
                  </span>
                  <button
                    onClick={() => store.logout()}
                    className="px-4 py-2 bg-yellow-500 text-white rounded-md hover:bg-yellow-600 transition-colors"
                  >
                    Logout
                  </button>
                  <button
                    onClick={() => setShowDelete(true)}
                    className="px-4 py-2 bg-red-500 text-white rounded-md hover:bg-red-600 transition-colors"
                  >
                    Delete Account
                  </button>
                </>
              ) : (
                <>
                  <button
                    onClick={() => setShowLogin(true)}
                    className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 transition-colors"
                  >
                    Login
                  </button>
                  <button
                    onClick={() => setShowRegister(true)}
                    className="px-4 py-2 bg-green-500 text-white rounded-md hover:bg-green-600 transition-colors"
                  >
                    Register
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      </nav>

      {/* Hero Section */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="text-center">
          <h2 className="text-4xl font-bold text-gray-900 mb-4">
            Welcome to MyApp
          </h2>
          <p className="text-xl text-gray-600 mb-8">
            {store.isAuthenticated 
              ? `You are logged in as ${"user"}` 
              : 'Please login or register to continue'}
          </p>
          
          {/* Feature Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-12">
            <div className="bg-white p-6 rounded-lg shadow-md">
              <div className="text-3xl mb-3">🚀</div>
              <h3 className="text-lg font-semibold mb-2">User account</h3>
              <button className='bg-green-200' onClick={getUserInfo}>Get user account information</button>
              {store.user !== null?
                <div>
                  <h4 className='text-sm'>{"Id: "+ store.user.id}</h4>
                  <h4 className='text-sm'>{"Email: "+ store.user.email}</h4>
                  <h4 className='text-sm'>{"Is email confirmed: "+ store.user.isEmailConfirmed}</h4>
                  <h4 className='text-sm'>{"Username: "+ store.user.username}</h4>
                </div>
                :null}
            </div>
            <div className="bg-white p-6 rounded-lg shadow-md">
              <div className="text-3xl mb-3">💡</div>
              <h3 className="text-lg font-semibold mb-2">Feature 2</h3>
              <p className="text-gray-600">Description of feature 2</p>
            </div>
            <div className="bg-white p-6 rounded-lg shadow-md">
              <div className="text-3xl mb-3">🎯</div>
              <h3 className="text-lg font-semibold mb-2">Feature 3</h3>
              <p className="text-gray-600">Description of feature 3</p>
            </div>
          </div>
        </div>
      </main>

      {/* Modals */}
      <LoginModal isOpen={showLogin} onClose={() => setShowLogin(false)} />
      <RegisterModal isOpen={showRegister} onClose={() => setShowRegister(false)} />
      <DeleteAccountModal isOpen={showDelete} onClose={() => setShowDelete(false)} />
    </div>
  );
}

export default observer(App);