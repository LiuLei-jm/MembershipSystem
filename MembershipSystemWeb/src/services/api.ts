import type {
  LoginCredentials,
  RegisterCredentials,
  ChangePassword,
  ApiKeyResponse,
  GenerateApiKeyResponse,
  ConnectionInfo,
} from '@/types/api'
import type {
  User,
  CreateUserRequest,
  ChangeUserPasswordRequest,
  ToggleUserStatusRequest,
} from '@/types/user'
import type { CreateCardData, UpdateCardData } from '@/types/card'
import type { PathConfiguration, PathConfigurationUpdateRequest } from '@/types/path'
import type { AppendData, DeleteData } from '@/types/pushContent'
import axios from 'axios'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

const apiClient = axios.create({
  baseURL: 'https://localhost:5451', // Replace with your API base URL
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    let message = '发生未知错误'
    if (error.response) {
      switch (error.response.status) {
        case 400:
          message = '请求参数错误'
          break
        case 401:
          message = '未授权，请登录'
          // 清除认证状态并跳转到登录页
          const authStore = useAuthStore()
          authStore.logout()
          router.push({ name: 'login' })
          break
        case 403:
          message = '拒绝访问'
          break
        case 404:
          message = '请求资源未找到'
          break
        case 409:
          message = '资源冲突,例如重复的用户名'
          break
        case 429:
          message = '请求过于频繁，请稍后再试'
          break
        case 500:
          message = '服务器内部错误'
          break
        default:
          message = `连接错误: ${error.response.status}`
      }
    } else if (error.request) {
      message = '请求未响应，请检查网络'
    }
    ElMessage.error(message)
    return Promise.reject(error)
  },
)

export default {
  register(credentials: LoginCredentials) {
    return apiClient.post('/user/register', credentials)
  },
  login(credentials: RegisterCredentials) {
    return apiClient.post('/user/login', credentials)
  },
  changePassword(data: ChangePassword) {
    return apiClient.post('/user/change-password', data)
  },
  getApiKey() {
    return apiClient.get<ApiKeyResponse>('/user/apikey')
  },
  updateApiKey() {
    return apiClient.post<GenerateApiKeyResponse>('/user/apikey')
  },
  getCards() {
    return apiClient.get('/membership/cards')
  },
  createCard(cardData: CreateCardData) {
    return apiClient.post('/membership/create', cardData)
  },
  deleteCard(cardId: string) {
    return apiClient.delete(`/membership/${cardId}`)
  },
  updateCard(cardId: string, cardData: UpdateCardData) {
    return apiClient.put(`/membership/${cardId}/update`, cardData)
  },
  getPathConfiguration() {
    return apiClient.get<PathConfiguration>('/user/path-config')
  },
  updatePathConfiguration(data: PathConfigurationUpdateRequest) {
    return apiClient.put<PathConfiguration>('/user/path-config', data)
  },
  appendCdk(cdkData: AppendData) {
    return apiClient.post<AppendData>('/file-push/append', cdkData)
  },
  deleteCdk(cdkData: DeleteData) {
    return apiClient.post<DeleteData>('/file-push/delete', cdkData)
  },
  // 用户管理相关接口
  getUsers() {
    return apiClient.get<User[]>('/admin/users')
  },
  createUser(userData: CreateUserRequest) {
    return apiClient.post('/user/register', userData)
  },
  changeUserPassword(data: ChangeUserPasswordRequest) {
    return apiClient.put(`/admin/${data.userId}/change-password`, { newPassword: data.newPassword })
  },
  toggleUserStatus(data: ToggleUserStatusRequest) {
    return apiClient.put(`/admin/${data.userId}/toggle-status`, { isActive: data.isActive })
  },
  deleteUser(userId: string) {
    return apiClient.delete(`/admin/${userId}`)
  },
  getAllConnections() {
    return apiClient.get<ConnectionInfo[]>('/admin/connections')
  },
}
