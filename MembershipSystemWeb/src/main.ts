import { createApp } from 'vue'
import { createPinia } from 'pinia'

import ElementPlus from 'element-plus'
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import 'element-plus/dist/index.css'

import App from './App.vue'
import router from './router'
import { initializeStoreCleaner } from '@/stores/storeCleaner'
import './assets/main.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)

// 初始化 store 清理器
initializeStoreCleaner()

app.use(ElementPlus, {
  locale: zhCn,
})
app.mount('#app')
