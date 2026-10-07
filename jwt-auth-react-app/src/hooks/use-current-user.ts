// src/auth/useCurrentUser.ts
import { useQuery } from '@tanstack/react-query';

import AuthService from '../services/AuthService';
import { tokenStore } from '../store/token-store';

export function useCurrentUser() {
  return useQuery({
    queryKey: ['currentUser'],
    queryFn: async () => {
      console.log("Get user info")
      const { data } = await AuthService.getAccountInfo();
      return data;
    },
    enabled: !!tokenStore.get()?.access_token,
    staleTime: 5 * 60 * 1000,
    retry: false,
  });
}