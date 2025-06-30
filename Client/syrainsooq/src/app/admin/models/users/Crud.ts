export interface GetAllUsers {
  userName: string;
  id: string;
  image:string
  email: string;
  phoneNumber: string;
  lockoutEnd: Date | null;
  isBlocked: boolean;
  isConfirmeEmail:boolean;
  roles:string
}
export class BlockUserDto {
  userId: string='';
  blockUntil: Date=new Date();
}
