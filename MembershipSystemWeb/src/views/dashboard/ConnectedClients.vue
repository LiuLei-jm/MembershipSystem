<template>
  <el-card>
    <template #header>
      <div class="card-header">
        <span>在线客户端</span>
        <el-button type="primary" @click="loadConnections">刷新</el-button>
      </div>
    </template>

    <!-- 搜索栏 -->
    <div class="search-bar">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索用户名"
        style="width: 300px"
        clearable
        @input="handleSearch"
      >
        <template #prefix>
          <el-icon>
            <Search />
          </el-icon>
        </template>
      </el-input>
    </div>

    <!-- 客户端列表 -->
    <el-table
      :data="paginatedConnections"
      v-loading="loading"
      style="width: 100%; margin-top: 20px"
      stripe
    >
      <el-table-column prop="connectionId" label="连接ID" width="250" />
      <el-table-column prop="userName" label="用户名" width="150" />
      <el-table-column prop="deviceName" label="设备名称" width="200" />
      <el-table-column prop="connectionAt" label="连接时间" width="180">
        <template #default="{ row }">
          {{ formatDate(row.connectionAt) }}
        </template>
      </el-table-column>
    </el-table>
    <div class="pagination-container">
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
        :total="filteredConnections.length"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </div>

    <!-- 空状态 -->
    <el-empty
      v-if="!loading && filteredConnections.length === 0"
      description="暂无在线客户端"
      style="margin-top: 50px"
    />
  </el-card>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import api from '@/services/api'
import type { ConnectionInfo } from '@/types/api'

// 响应式数据
const connections = ref<ConnectionInfo[]>([])
const loading = ref<boolean>(false)
const searchKeyword = ref<string>('')
const currentPage = ref<number>(1)
const pageSize = ref<number>(10)

// 计算属性：过滤连接
const filteredConnections = computed(() => {
  if (!searchKeyword.value) return connections.value
  return connections.value.filter((connection) =>
    connection.userName.toLowerCase().includes(searchKeyword.value.toLowerCase()),
  )
})

// 格式化日期
const formatDate = (dateString: string) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN')
}

// 加载连接列表
const loadConnections = async () => {
  loading.value = true
  try {
    const response = await api.getAllConnections()
    connections.value = response.data
  } catch (error) {
    ElMessage.error('加载客户端列表失败')
    console.error('加载客户端列表失败:', error)
  } finally {
    loading.value = false
  }
}

// 搜索处理
const handleSearch = () => {
  // 计算属性会自动处理过滤
}

const paginatedConnections = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredConnections.value.slice(start, end)
})

const handleSizeChange = (val: number) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val: number) => {
  currentPage.value = val
}

// 组件挂载时加载连接列表
onMounted(() => {
  loadConnections()
})
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.search-bar {
  display: flex;
  align-items: center;
  gap: 10px;
}

:deep(.el-table) {
  margin-top: 20px;
}

:deep(.el-button) {
  margin-right: 8px;
}

.pagination-container {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  margin-top: 20px;
}
</style>
