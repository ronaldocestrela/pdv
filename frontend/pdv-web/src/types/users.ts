export interface UserAdminDto {
  id: number;
  email: string;
  isActive: boolean;
  roleIds: number[];
}
