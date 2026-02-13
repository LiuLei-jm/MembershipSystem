<template>
  <el-menu :default-active="activeMenu" class="el-menu-vertical" router @select="handleSelect">
    <el-menu-item index="/cards">
      <el-icon><CreditCard /></el-icon>
      <span>会员卡管理</span>
    </el-menu-item>

    <el-menu-item index="/communication-key">
      <el-icon><Key /></el-icon>
      <span>通讯密钥管理</span>
    </el-menu-item>

    <el-sub-menu v-if="authStore.isAdmin" index="/admin">
      <template #title>
        <el-icon><Setting /></el-icon>
        <span>系统管理</span>
      </template>
      <el-menu-item index="/admin/users">
        <el-icon><User /></el-icon>
        <span>用户管理</span>
      </el-menu-item>
      <el-menu-item index="/admin/clients">
        <el-icon><Connection /></el-icon>
        <span>在线客户端</span>
      </el-menu-item>
    </el-sub-menu>
  </el-menu>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { CreditCard, Setting, User, Connection, Key } from '@element-plus/icons-vue'

const authStore = useAuthStore()
const route = useRoute()
const activeMenu = computed<string>(() => {
  return route.path
})

// Function to handle menu item selection on mobile
const handleSelect = () => {
  // On mobile, close the menu after selection
  if (window.innerWidth < 768) {
    // Emit an event to close the menu in the parent component
    window.dispatchEvent(new CustomEvent('menuItemSelected'));
  }
}

// Add event listener to close menu after selection on mobile
onMounted(() => {
  const handleMenuClose = () => {
    if (window.innerWidth < 768) {
      // This will be handled by the parent component
    }
  }

  window.addEventListener('menuItemSelected', handleMenuClose);
})

// Clean up event listener
onUnmounted(() => {
  window.removeEventListener('menuItemSelected', () => {});
})
</script>

<style scoped>
.el-menu-vertical {
  height: 100%;
  border-right: none;
}

/* Responsive styles for mobile */
@media (max-width: 768px) {
  .el-menu-vertical {
    width: 100%;
  }

  .el-sub-menu .el-menu {
    background-color: #f5f5f5;
  }
}
</style>
