import { defineStore } from 'pinia'
import { ElMessage } from 'element-plus'
import apiClient from '@/services/api'
import type { PathConfiguration, PathConfigurationUpdateRequest } from '@/types/path'

interface PathState {
  config: PathConfiguration | null
  status: 'idle' | 'loading' | 'error' | 'success'
  error: string | null
}

export const usePathStore = defineStore('path', {
  state: (): PathState => ({
    config: null,
    status: 'idle',
    error: null,
  }),
  getters: {
    isLoading: (state) => state.status === 'loading',
    hasBeenFetched: (state) => state.status === 'success' || state.status === 'error',
    basePath: (state) => state.config?.basePath || 'D:',
    membershipCardFilePath: (state) => state.config?.membershipCardFilePath || 'CDK.txt',
  },
  actions: {
    async fetchPathConfiguration(force = false) {
      if (!force && this.hasBeenFetched) return
      this.status = 'loading'
      try {
        const response = await apiClient.getPathConfiguration()
        this.config = response.data
        this.status = 'success'
        this.error = null
      } catch (error) {
        this.status = 'error'
        this.error = '获取路径配置失败'
        ElMessage.error(this.error)
        console.error('Fetch path config error:', error)
      }
    },

    async updatePathConfiguration(updateData: PathConfigurationUpdateRequest) {
      try {
        const response = await apiClient.updatePathConfiguration(updateData)
        this.config = response.data
        ElMessage.success('路径配置更新成功')
      } catch (error) {
        ElMessage.error('路径配置更新失败')
        console.error('Update path config error:', error)
      }
    },
  },
})
