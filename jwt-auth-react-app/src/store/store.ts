import { makeAutoObservable } from "mobx";
import AuthService from "../services/AuthService";
import type AccountInfoResponse from "../models/responses/AccountInfoResponse";
import { makePersistable } from "mobx-persist-store";

export default class Store{
  isAuthenticated: boolean=false;
  user: AccountInfoResponse|null = null;

  constructor(){
    makeAutoObservable(this);
    makePersistable(this, {
      name: 'AuthStore',
      properties: ['user', 'isAuthenticated'],
      storage: window.localStorage,
    });
  }

  setIsAuthenticated(isAuthenticated:boolean){
    this.isAuthenticated=isAuthenticated;
  }

  setUser(user:AccountInfoResponse|null){
    this.user=user;
  }

  async login(email:string, password:string){

      const response = await AuthService.login(email,password);
      localStorage.setItem("access_token", response.data.accessToken);
      localStorage.setItem("refresh_token", response.data.refreshToken);

      this.setIsAuthenticated(true);
  }

  async register(email:string, password:string){ 
    return await AuthService.register(email,password);
  }

  async logout(){
    const response = await AuthService.logout();
    console.log(response.status);
    localStorage.removeItem("access_token");
    localStorage.removeItem("refresh_token");
    this.setIsAuthenticated(false);
    this.setUser(null);
  }

  async deleteAccount(){
    await AuthService.deleteAccount();
    localStorage.removeItem("access_token");
    localStorage.removeItem("refresh_token");
    this.setIsAuthenticated(false);
    this.setUser(null);
  }

  async getAccountInfo(){
    return await AuthService.getAccountInfo();
  }

  

}