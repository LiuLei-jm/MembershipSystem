export interface LoginCredentials {
  username: string
  password: string
}

export interface RegisterCredentials {
  username: string
  password: string
}

export interface ChangePassword {
  currentPassword: string
  newPassword: string
}

export interface ApiKeyResponse {
  apiKey: string
  createdAt: string
}

export interface GenerateApiKeyResponse {
  apiKey: string
}
export interface ConnectionInfo {
  connectionId: string
  userName: string
  deviceName: string
  connectionAt: string
}
