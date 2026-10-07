import axios, { type AxiosResponse } from "axios";
import type LoginResponse from "../models/responses/LoginResponse";
import type RefreshRequest from "../models/requests/RefreshRequest";
import { tokenStore } from "../store/token-store";

const BASE_URL = "https://localhost:8081/auth";

const $api = axios.create({
  withCredentials: true,
  baseURL: BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

$api.interceptors.request.use((config) => {
  const access_token = tokenStore.get()?.access_token;

  if (config.headers)
    config.headers.Authorization = `Bearer ${access_token}`;

  return config;
});

$api.interceptors.response.use((response) => response,
  async (error) => {
    const { response, config } = error;

    if (response.status !== 401) {
      return Promise.reject(error);
    }
 
    if (config.url === "/refresh") {
      return Promise.reject(error);
    }

    const refreshToken = tokenStore.get()?.refresh_token;

    if(!refreshToken){
      return Promise.reject(error);
    }

    try {
      const resp = await axios.post<RefreshRequest, AxiosResponse<LoginResponse>>(BASE_URL + "/refresh", { refreshToken });

      tokenStore.set(resp.data.accessToken, resp.data.refreshToken);
     
      return $api(config);
    } catch (error) {
      return Promise.reject(error);
    }
  },
);

export default $api;
