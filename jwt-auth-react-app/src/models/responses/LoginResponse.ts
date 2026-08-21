
export default interface LoginResponse{
  accessToken:string;
  refreshToken: string;
  tokenType: string|null;
  expiresIn: number,

}