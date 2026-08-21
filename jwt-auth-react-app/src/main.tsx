import { createContext, StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import Store from './store/store.ts';

type State= {
  store:Store;
}
const store = new Store();
export const StoreContext = createContext<State>({store});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <StoreContext.Provider value={{store}}>

      <App />
    </StoreContext.Provider>
  </StrictMode>,
)
