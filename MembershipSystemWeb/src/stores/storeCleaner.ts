import { useAuthStore } from '@/stores/auth'
import { useMembershipStore } from '@/stores/membership'
import { usePathStore } from '@/stores/path'
import { registerClearAllStores } from '@/stores/auth'

// 清理所有 stores 的数据（防止用户切换时的数据污染）
export function clearAllStores() {
  const authStore = useAuthStore()
  const membershipStore = useMembershipStore()
  const pathStore = usePathStore()

  // 清理各个 store 的状态
  membershipStore.$reset()
  pathStore.$reset()

  // 清理本地存储中的业务数据（除了认证相关的在 auth.logout 中处理）
  // 如果有其他本地存储的数据也需要清理，在这里添加
  localStorage.removeItem('cards')
  localStorage.removeItem('config')
}

// 注册清理函数到 auth store
export function initializeStoreCleaner() {
  registerClearAllStores(clearAllStores)
}
