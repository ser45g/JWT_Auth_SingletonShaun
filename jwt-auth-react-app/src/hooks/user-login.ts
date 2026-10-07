import { useMutation, useQueryClient } from "@tanstack/react-query";
import AuthService from "../services/AuthService";
import type LoginRequest from "../models/requests/LoginRequest";
import { tokenStore } from "../store/token-store";

// 1. Login Mutation
export function useLogin() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({email, password}:LoginRequest) => {
      const { data } = await AuthService.login(email, password);
      return data;
    },
    onSuccess: (data) => {
      // Store the token so subsequent requests are authenticated
      tokenStore.set(data.accessToken, data.refreshToken);
      
      // Invalidate the user query to trigger a refetch
      queryClient.invalidateQueries({ queryKey: ['currentUser'] });
    },
  });
}
