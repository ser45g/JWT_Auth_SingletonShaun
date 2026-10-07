import { useMutation, useQueryClient } from "@tanstack/react-query";
import AuthService from "../services/AuthService";
import { tokenStore } from "../store/token-store";

// 3. Logout Mutation
export function useLogout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      await AuthService.logout();
    },
    onSuccess: () => {
      // Clear the token
      tokenStore.clear();
      
      // Remove all user-related data from the cache
      queryClient.removeQueries({ queryKey: ['currentUser'] });
      // Or clear everything if the app is entirely user-scoped
      // queryClient.clear();
    },
  });
}