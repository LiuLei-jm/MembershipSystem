<template>
  <el-card>
    <template #header>
      <div class="card-header">
        <span>通讯密钥管理</span>
      </div>
    </template>

    <el-alert
      v-if="error"
      :title="error"
      type="error"
      show-icon
      :closable="false"
      style="margin-bottom: 20px"
    />

    <el-form :model="form" label-width="120px" v-loading="loading">
      <el-form-item label="当前通讯密钥:">
        <el-input v-model="form.apiKey" type="textarea" :rows="6" readonly class="key-display" />
      </el-form-item>

      <el-form-item label="创建时间:">
        <el-input v-model="form.createdAt" readonly />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="copyKey">复制密钥</el-button>
        <el-button type="warning" @click="updateKey">更新密钥</el-button>
      </el-form-item>
    </el-form>

    <el-dialog v-model="showConfirmDialog" title="确认更新密钥" width="30%">
      <span>更新密钥将使当前密钥失效，确定要继续吗？</span>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showConfirmDialog = false">取消</el-button>
          <el-button type="primary" @click="confirmUpdateKey">确定</el-button>
        </span>
      </template>
    </el-dialog>
  </el-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import api from '@/services/api'
import type { ApiKeyResponse, GenerateApiKeyResponse } from '@/types/api'

interface ApiKeyForm {
  apiKey: string
  createdAt: string
}

const form = ref<ApiKeyForm>({
  apiKey: '',
  createdAt: '',
})

const loading = ref<boolean>(false)
const error = ref<string | null>(null)
const showConfirmDialog = ref<boolean>(false)

const fetchApiKey = async () => {
  try {
    loading.value = true
    error.value = null
    const response = await api.getApiKey()
    const data = response.data as ApiKeyResponse
    form.value.apiKey = data.apiKey
    form.value.createdAt = new Date(data.createdAt).toLocaleString('zh-CN')
  } catch (err: any) {
    if (err.response && err.response.status === 404) {
      form.value.apiKey = '未生成通讯密钥'
      form.value.createdAt = ''
    } else {
      error.value = '获取通讯密钥失败'
      console.error('Failed to fetch API key:', err)
    }
  } finally {
    loading.value = false
  }
}

const copyKey = () => {
  if (form.value.apiKey) {
    navigator.clipboard
      .writeText(form.value.apiKey)
      .then(() => {
        ElMessage.success('密钥已复制到剪贴板')
      })
      .catch(() => {
        ElMessage.error('复制失败')
      })
  }
}

const updateKey = () => {
  showConfirmDialog.value = true
}

const confirmUpdateKey = async () => {
  try {
    loading.value = true
    showConfirmDialog.value = false
    const response = await api.updateApiKey()
    const data = response.data as GenerateApiKeyResponse
    form.value.apiKey = data.apiKey
    form.value.createdAt = new Date().toLocaleString('zh-CN')
    ElMessage.success('密钥更新成功')
  } catch (err) {
    ElMessage.error('密钥更新失败')
    console.error('Failed to update API key:', err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchApiKey()
})
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.key-display {
  font-family: 'Courier New', monospace;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}
</style>
