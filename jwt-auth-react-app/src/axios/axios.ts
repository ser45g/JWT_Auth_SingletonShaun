import axios, { type AxiosResponse } from "axios";
import type LoginResponse from "../models/responses/LoginResponse";
import type RefreshRequest from "../models/requests/RefreshRequest";

const BASE_URL = "https://localhost:8081/auth";

const $api = axios.create({
  withCredentials:true,
  baseURL: BASE_URL,
  headers:{
    "Content-Type":"application/json"
  }
});

$api.interceptors.request.use((config)=>{
  if(config.headers)
    config.headers.Authorization = `Bearer ${localStorage.getItem("access_token")}`;
  return config;
});

$api.interceptors.response.use(response => response, async (error) => {
  const { response, config } = error

  if (response.status !== 401) {
    return Promise.reject(error)
  }
  console.log(config.url)
  if (config.url === '/refresh') {
    return Promise.reject(error);
  }
  const refreshToken=localStorage.getItem("refresh_token");

  //console.log("here")
  // Use a 'clean' instance of axios without the interceptor to refresh the token. No more infinite refresh loop.

  try{
    const resp = await axios.post<RefreshRequest,  AxiosResponse<LoginResponse>>(BASE_URL+'/refresh', {refreshToken});
    console.log(resp);
      localStorage.setItem("access_token", resp.data.accessToken);
      localStorage.setItem("refresh_token", resp.data.refreshToken);
      console.log("return right")
      return $api(config);
  }catch(error){
    return Promise.reject(error)
  }
})

export default $api;

