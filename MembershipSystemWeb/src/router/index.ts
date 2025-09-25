import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'
import LoginView from '../views/LoginView.vue'
import HomeView from '@/views/HomeView.vue'

import CardManagement from '../views/dashboard/CardManagement.vue'
import UserManagement from '../views/dashboard/UserManagement.vue'
import ConnectedClients from '../views/dashboard/ConnectedClients.vue'

const routes: RouteRecordRaw[] = [
  { path: '/login', name: 'login', component: LoginView },
  {
    path: '/',
    name: 'home',
    component: HomeView,
    redirect: '/cards',
    meta: { requiresAuth: true },
    children: [
      {
        path: 'cards',
        name: 'card-management',
        component: CardManagement,
      },
      {
        path: 'admin/users',
        name: 'user-management',
        component: UserManagement,
        meta: { requiresAdmin: true },
      },
      {
        path: 'admin/clients',
        name: 'connected-clients',
        component: ConnectedClients,
        meta: { requiresAdmin: true },
      },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth)
  const requiresAdmin = to.matched.some((record) => record.meta.requresAdmin)
  if (requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else if (requiresAdmin && !authStore.isAdmin) {
    ElMessage.error('您没有权限访问此页面！')
    next({ name: 'card-management' })
  } else if (to.name === 'login' && authStore.isAuthenticated) {
    next({ name: 'home' })
  } else {
    next()
  }
})

export default router
