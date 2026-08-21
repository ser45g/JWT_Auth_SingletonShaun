export default interface AccountInfoResponse{
  id: string;
  username:string;
  email:string;
  isEmailConfirmed: boolean;
  roles: string[];
}