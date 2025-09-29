import { defineStore } from 'pinia'
import { ElMessage, ElMessageBox } from 'element-plus'
import apiClient from '@/services/api'
import type { MemberCard, CreateCardData, UpdateCardData } from '@/types/card'
import type { AppendData, DeleteData } from '@/types/pushContent'
import { useAuthStore } from '@/stores/auth.ts'
import { usePathStore } from '@/stores/path.ts'
import router from '@/router'
interface MembershipState {
  cards: MemberCard[]
  status: 'idle' | 'loading' | 'error' | 'success'
  error: string | null
}

export const useMembershipStore = defineStore('membership', {
  state: (): MembershipState => ({
    cards: [],
    status: 'idle',
    error: null,
  }),
  getters: {
    isLoading: (state) => state.status === 'loading',
    hasBeenFetched: (state) => state.status === 'success' || state.status === 'error',
  },
  actions: {
    // 重置store状态
    $reset() {
      this.cards = []
      this.status = 'idle'
      this.error = null
    },

    async fetchCards(force = false) {
      if (!force && this.hasBeenFetched) return
      this.status = 'loading'
      try {
        const response = await apiClient.getCards()
        this.cards = response.data
        this.status = 'success'
        this.error = null
      } catch (error) {
        this.status = 'error'
        this.error = '获取会员卡列表失败'
        const authStore = useAuthStore()
        authStore.logout()
        router.push({ name: 'login' })
        ElMessage.error(this.error)
        console.error('Fetch cards error:', error)
      }
    },

    async createCard(cardData: CreateCardData) {
      try {
        const response = await apiClient.createCard(cardData)
        this.cards.push(response.data)
        ElMessage.success('创建会员卡成功')
      } catch (error) {
        ElMessage.error('创建会员卡失败')
        console.error('Create card error:', error)
      }
    },

    async reissueCard(cardKey: string) {
      try {
        const pathStore = usePathStore()
        await pathStore.fetchPathConfiguration()
        // Browser-compatible path joining
        const basePath = pathStore.config?.basePath || 'D:'
        const membershipCardFilePath = pathStore.config?.membershipCardFilePath || 'CDK.txt'
        const fullPath =
          basePath.endsWith('/') || basePath.endsWith('\\')
            ? `${basePath}${membershipCardFilePath}`
            : `${basePath}/${membershipCardFilePath}`

        const cdkData: AppendData = {
          filePath: fullPath,
          content: cardKey.trimEnd(),
          logMessage: `补发CDK ${cardKey}`,
        }

        await apiClient.appendCdk(cdkData)
        ElMessage.success('补发成功')
      } catch (error) {
        ElMessage.error('补发失败')
        console.error('Reissue card error:', error)
      }
    },

    async clearCard(cardKey: string) {
      try {
        await ElMessageBox.confirm(
          '确定要清理该会员卡吗？此操作将从文件中移除该卡号。',
          '清理确认',
          {
            confirmButtonText: '清理',
            cancelButtonText: '取消',
            type: 'warning',
          },
        )

        const pathStore = usePathStore()
        await pathStore.fetchPathConfiguration()
        // Browser-compatible path joining
        const basePath = pathStore.config?.basePath || 'D:'
        const membershipCardFilePath = pathStore.config?.membershipCardFilePath || 'CDK.txt'
        const fullPath =
          basePath.endsWith('/') || basePath.endsWith('\\')
            ? `${basePath}${membershipCardFilePath}`
            : `${basePath}/${membershipCardFilePath}`

        const cdkData: DeleteData = {
          filePath: fullPath,
          contentToRemove: cardKey.trimEnd(),
          logMessage: `清理CDK ${cardKey}`,
        }

        await apiClient.deleteCdk(cdkData)
        ElMessage.success('清理成功')
      } catch (error) {
        if (error !== 'cancel') {
          ElMessage.error('清理失败')
          console.error('Clear card error:', error)
        }
      }
    },

    async deleteCard(cardId: string) {
      try {
        await ElMessageBox.confirm('确定要删除该会员卡吗？此操作不可撤销。', '删除确认', {
          confirmButtonText: '删除',
          cancelButtonText: '取消',
          type: 'warning',
        })

        await apiClient.deleteCard(cardId)
        this.cards = this.cards.filter((card) => card.id !== cardId)
        ElMessage.success('删除会员卡成功')
      } catch (error) {
        if (error !== 'cancel') {
          ElMessage.error('删除会员卡失败')
          console.error('Delete card error:', error)
        }
      }
    },

    async updateCard(cardId: string, updateData: UpdateCardData) {
      try {
        const response = await apiClient.updateCard(cardId, updateData)
        const index = this.cards.findIndex((card) => card.id === cardId)
        if (index !== -1) {
          this.cards[index] = response.data
        }
        ElMessage.success('更新会员卡成功')
      } catch (error) {
        ElMessage.error('更新会员卡失败')
        console.error('Update card error:', error)
      }
    },
  },
})
