export interface UserInfo {
  username: string
  role: 'User' | 'Admin'
  userId: string
}

export interface User {
  id: string
  username: string
  isActive: boolean
  role: 'User' | 'Admin'
  createdAt: string
  lastLoginAt: string
}

export interface CreateUserRequest {
  username: string
  password: string
  role: 'User' | 'Admin'
}

export interface ChangeUserPasswordRequest {
  userId: string
  newPassword: string
}

export interface ToggleUserStatusRequest {
  userId: string
  isActive: boolean
}
