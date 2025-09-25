<template>
  <el-container class="home-container">
    <el-header class="home-header">
      <TopHeader />
    </el-header>
    <el-container>
      <el-aside width="200px">
        <SideMenu />
      </el-aside>
      <el-main>
        <router-view v-slot="{ Component }">
          <transition name="fade-transform" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import TopHeader from '@/components/TopHeader.vue'
import SideMenu from '@/components/SideMenu.vue'
</script>

<style scoped>
.home-container {
  height: 100vh;
  width: 100vw;
}
.home-header {
  background-color: #ffffff;
  border-bottom: 1px solid #e6e6e6;
  padding: 0;
}
.el-aside {
  background-color: #ffffff;
}
.el-main {
  background-color: #f0f2f5;
  padding: 20px;
}
.fade-transform-leave-active,
.fade-transform-enter-active {
  transition: all 0.5s;
}
.fade-transform-enter-from {
  opacity: 0;
  transform: translateX(-30px);
}
.fade-transform-leave-to {
  opacity: 0;
  transform: translateX(30px);
}
</style>
