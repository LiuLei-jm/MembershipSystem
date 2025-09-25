import { defineStore } from 'pinia'
import api from '@/services/api'
import { ElMessage } from 'element-plus'
import router from '@/router'
import { jwtDecode } from 'jwt-decode'
import type { UserInfo } from '@/types/user'
import type { LoginCredentials, RegisterCredentials, ChangePassword } from '@/types/api'

interface AuthState {
  token: string | null
  user: UserInfo | null
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    token: localStorage.getItem('token') || null,
    user: JSON.parse(localStorage.getItem('user') || 'null'),
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => state.user && state.user.role === 'Admin',
  },
  actions: {
    async register(credentials: RegisterCredentials): Promise<boolean> {
      try {
        api.regitster(credentials)
        ElMessage.success('注册成功，请登录')
        router.push('/login')
        return true
      } catch (error) {
        console.log(error)
        return false
      }
    },
    async login(credentials: LoginCredentials): Promise<boolean> {
      try {
        const response = await api.login(credentials)
        this.token = response.data.token
        if (!this.token) {
          throw new Error('登录失败,未收到令牌')
        }
        localStorage.setItem('token', this.token)
        try {
          const decodedToken = jwtDecode<UserInfo>(this.token)
          this.user = {
            username: decodedToken.username,
            role: decodedToken.role,
            userId: decodedToken.userId,
          }
          localStorage.setItem('user', JSON.stringify(this.user))
        } catch (error) {
          console.log('解析令牌失败', error)
          ElMessage.error('登录失败,无效的令牌')
          this.logout()
          return false
        }
        ElMessage.success('登录成功')
        router.push({ name: 'home' })
        return true
      } catch (error) {
        console.log(error)
        this.logout()
        return false
      }
    },
    async changePassword(data: ChangePassword): Promise<boolean> {
      try {
        const response = await api.changePassword(data)
        ElMessage.success(response.data.message)
        this.logout()
        return true
      } catch (error) {
        console.log(error)
        ElMessage.error('密码修改失败')
        return false
      }
    },
    logout() {
      this.token = null
      this.user = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      ElMessage.info('已登出')
      router.push({ name: 'login' })
    },
  },
})
