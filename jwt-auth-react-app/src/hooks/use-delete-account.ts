import { useMutation, useQueryClient } from "@tanstack/react-query";
import AuthService from "../services/AuthService";
import { tokenStore } from "../store/token-store";

export function useDeleteAccount() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      await AuthService.deleteAccount();
    },
    onSuccess: () => {
      // Store the token so subsequent requests are authenticated
      tokenStore.clear();
      
      // Invalidate the user query to trigger a refetch
      queryClient.invalidateQueries({ queryKey: ['currentUser'] });
    },
  });
}