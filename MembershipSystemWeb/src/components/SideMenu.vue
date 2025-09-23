<template>
  <el-menu :default-active="activeMenu" class="el-menu-vertical" router>
    <el-menu-item index="/cards">
      <el-icon><CreditCard /></el-icon>
      <span>会员卡管理</span>
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
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { CreditCard, Setting, User, Connection } from '@element-plus/icons-vue'

const authStore = useAuthStore()
const route = useRoute()
const activeMenu = computed<string>(() => {
  return route.path
})
</script>

<style scoped>
.el-menu-vertical {
  height: 100%;
  border-right: none;
}
</style>
