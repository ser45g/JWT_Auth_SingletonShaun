import { useMutation } from "@tanstack/react-query";
import AuthService from "../services/AuthService";
import type RegisterRequest from "../models/requests/RegisterRequest";

// 2. Register Mutation
export function useRegister() {
  
  return useMutation({
    mutationFn: async ({email, password}:RegisterRequest) => {
      await AuthService.register(email, password);
      return true;
    },
  });
}