<template>
  <div class="top-header">
    <div class="header-content">
      <div class="logo">
        <el-button v-if="isMobile" @click="toggleMenu" :icon="Menu" text size="large" class="menu-toggle-btn" />
        <span>会员管理系统</span>
      </div>
      <div class="user-info">
        <el-dropdown @command="handleCommand">
          <span class="el-dropdown-link">
            {{ authStore.user?.username || '用户' }}
            <el-icon class="el-icon--right">
              <arrow-down />
            </el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="changePassword">修改密码</el-dropdown-item>
              <el-dropdown-item command="logout" divided>退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>
    <el-dialog v-model="passwordDialogVisible" title="修改密码" width="30%">
      <ChangePasswordDialog
        :model-value="passwordDialogVisible"
        @update:modelValue="passwordDialogVisible = $event"
      />
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { ArrowDown, Menu } from '@element-plus/icons-vue'
import ChangePasswordDialog from './ChangePasswordDialog.vue'

const authStore = useAuthStore()
const passwordDialogVisible = ref(false)
const isMobile = ref(false)

// Define emit for toggling menu
const emit = defineEmits(['toggleMenu'])

// Function to check if screen is mobile
const checkIfMobile = () => {
  isMobile.value = window.innerWidth < 768
}

// Handle menu toggle
const toggleMenu = () => {
  emit('toggleMenu')
}

const handleCommand = (command: string) => {
  if (command === 'logout') {
    authStore.logout()
  } else if (command === 'changePassword') {
    passwordDialogVisible.value = true
  }
}

// Initialize mobile detection
onMounted(() => {
  checkIfMobile()
  window.addEventListener('resize', checkIfMobile)
})

// Clean up event listener
onUnmounted(() => {
  window.removeEventListener('resize', checkIfMobile)
})
</script>

<style scoped>
.top-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 100%;
  padding: 0 20px;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.logo {
  font-size: 20px;
  font-weight: bold;
  display: flex;
  align-items: center;
  gap: 10px;
}

.menu-toggle-btn {
  margin-right: 10px;
}

.el-dropdown-link {
  cursor: pointer;
  display: flex;
  align-items: center;
}

/* Responsive styles */
@media (max-width: 768px) {
  .logo {
    font-size: 18px;
  }

  .top-header {
    padding: 0 10px;
  }
}
</style>
