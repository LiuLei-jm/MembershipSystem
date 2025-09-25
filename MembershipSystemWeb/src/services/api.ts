import type { LoginCredentials, RegisterCredentials, ChangePassword } from '@/types/api'
import type { CreateCardData, UpdateCardData } from '@/types/card'
import type { PathConfiguration, PathConfigurationUpdateRequest } from '@/types/path'
import axios from 'axios'
import { ElMessage } from 'element-plus'

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
  regitster(credentials: LoginCredentials) {
    return apiClient.post('/user/register', credentials)
  },
  login(credentials: RegisterCredentials) {
    return apiClient.post('/user/login', credentials)
  },
  changePassword(data: ChangePassword) {
    return apiClient.post('/user/change-password', data)
  },
  getApiKey() {
    return apiClient.get('/user/apikey')
  },
  updateApiKey() {
    return apiClient.post('/user/apikey')
  },
  getConnections() {
    return apiClient.get('/user/connections')
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
}
