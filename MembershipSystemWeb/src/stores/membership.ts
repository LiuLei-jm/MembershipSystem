import { defineStore } from 'pinia'
import { ElMessage, ElMessageBox } from 'element-plus'
import apiClient from '@/services/api'
import type { MemberCard, CreateCardData, UpdateCardData } from '@/types/card'

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
