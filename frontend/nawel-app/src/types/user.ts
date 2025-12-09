export interface User {
  id: number;
  login: string;
  email: string;
  firstName: string;
  lastName: string;
  avatar: string;
  pseudo?: string;
  notifyListEdit: boolean;
  notifyGiftTaken: boolean;
  displayPopup: boolean;
  isChildren: boolean;
  isAdmin: boolean;
  familyId: number;
  familyName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface LoginCredentials {
  login: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface UpdateProfileData {
  email?: string;
  firstName?: string;
  lastName?: string;
  pseudo?: string;
  notifyListEdit?: boolean;
  notifyGiftTaken?: boolean;
  displayPopup?: boolean;
}

export interface ChangePasswordData {
  currentPassword: string;
  newPassword: string;
}

export interface ManagingChild {
  userId: number;
  userName: string;
  avatarUrl: string;
}
