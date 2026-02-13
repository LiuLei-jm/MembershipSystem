<template>
  <el-container class="home-container">
    <el-header class="home-header">
      <TopHeader @toggle-menu="handleMenuToggle" :is-menu-collapsed="isMenuCollapsed" />
    </el-header>
    <el-container v-if="!isMobile">
      <el-aside width="200px" class="desktop-aside">
        <SideMenu />
      </el-aside>
      <el-main class="desktop-main">
        <router-view v-slot="{ Component }">
          <transition name="fade-transform" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </el-main>
    </el-container>
    <el-container v-else>
      <el-aside v-if="!isMenuCollapsed" width="200px" class="mobile-aside">
        <SideMenu />
      </el-aside>
      <el-main :class="{'mobile-main-collapsed': isMenuCollapsed, 'mobile-main-expanded': !isMenuCollapsed}">
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
import { ref, onMounted, onUnmounted } from 'vue'
import TopHeader from '@/components/TopHeader.vue'
import SideMenu from '@/components/SideMenu.vue'

const isMenuCollapsed = ref(false) // Changed from true to false for desktop
const isMobile = ref(false)

// Function to check if screen is mobile
const checkIfMobile = () => {
  isMobile.value = window.innerWidth < 768
  if (isMobile.value) {
    isMenuCollapsed.value = true // On mobile, menu should be collapsed by default
  } else {
    isMenuCollapsed.value = false // On desktop, menu should be expanded by default
  }
}

// Handle menu toggle
const handleMenuToggle = () => {
  isMenuCollapsed.value = !isMenuCollapsed.value
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
.home-container {
  height: 100vh;
  width: 100vw;
  overflow-x: hidden;
}
.home-header {
  background-color: #ffffff;
  border-bottom: 1px solid #e6e6e6;
  padding: 0;
  z-index: 1000;
}
.desktop-aside {
  background-color: #ffffff;
  height: calc(100vh - 60px);
  position: fixed;
  top: 60px;
  left: 0;
  z-index: 999;
}
.desktop-main {
  margin-left: 200px;
  padding: 20px;
  transition: margin-left 0.3s ease;
  height: calc(100vh - 60px);
  overflow-y: auto;
}
.mobile-aside {
  background-color: #ffffff;
  height: calc(100vh - 60px);
  position: fixed;
  top: 60px;
  left: 0;
  z-index: 999;
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.15);
}
.mobile-main-collapsed {
  padding: 10px;
  margin-left: 0;
  min-height: calc(100vh - 60px);
}
.mobile-main-expanded {
  padding: 10px;
  margin-left: 200px;
  min-height: calc(100vh - 60px);
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

/* Responsive styles */
@media (max-width: 768px) {
  .desktop-aside {
    display: none;
  }

  .desktop-main {
    margin-left: 0;
    height: calc(100vh - 60px);
  }

  .mobile-aside {
    width: 200px !important;
  }
}
</style>
