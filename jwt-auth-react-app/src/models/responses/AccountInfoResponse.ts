export default interface AccountInfoResponse{
  id: string;
  username:string;
  email:string;
  emailConfirmed: boolean;
  roles: string[];
}