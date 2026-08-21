import type { AxiosResponse } from "axios";
import type LoginResponse from "../models/responses/LoginResponse";
import $api from "../axios/axios";
import type LoginRequest from "../models/requests/LoginRequest";
import type RegisterRequest from "../models/requests/RegisterRequest";
import type AccountInfoResponse from "../models/responses/AccountInfoResponse";
import type ResendConfirmationEmailRequest from "../models/requests/ResendConfirmationEmailRequest​";
import type ForgotPasswordRequest from "../models/requests/ForgotPasswordRequest";
import type ResetPasswordRequest from "../models/requests/ResetPasswordRequest";
import type RefreshRequest from "../models/requests/RefreshRequest";

export default class AuthService {

  static async login(email: string, password: string): Promise<AxiosResponse<LoginResponse, LoginRequest>> {
    return $api.post<LoginResponse>("/login", {
      email,
      password,
    } as LoginRequest );
  }

  static async register(email: string, password: string): Promise<AxiosResponse<void, RegisterRequest>> {
    return $api.post("/register", {
      email,
      password
    } as RegisterRequest);
  }

  static async resendEmailConfirmation(email: string): Promise<AxiosResponse<void, ResendConfirmationEmailRequest>> {
    return $api.post("/resendConfirmationEmail", {
      email,
    } as ResendConfirmationEmailRequest);
  }

  static async forgotPassword(email: string): Promise<AxiosResponse<void, ForgotPasswordRequest>> {
    return $api.post("/resendConfirmationEmail", {
      email,
    } as ForgotPasswordRequest);
  }
  static async refresh(refreshToken: string): Promise<AxiosResponse<void, RefreshRequest>> {
    return $api.post("/resendConfirmationEmail", {
      refreshToken,
    } as RefreshRequest);
  }

  static async resetPassword(email: string, newPassword: string, resetCode:string): Promise<AxiosResponse<void, ResetPasswordRequest>> {
    return $api.post("/resendConfirmationEmail", {
      email,
      newPassword,
      resetCode,
    } as ResetPasswordRequest);
  }

  static async logout(): Promise<AxiosResponse<void, void>> {
    return $api.delete("/logout");
  }

  static async deleteAccount(): Promise<AxiosResponse<void, void>> {
    return $api.delete("/delete-account");
  }

  static async getAccountInfo(): Promise<AxiosResponse<AccountInfoResponse, void>> {
    return $api.get("/account-info");
  }
}
