<template>
  <el-card>
    <template #header>
      <div class="card-header">
        <span>用户管理</span>
        <el-button type="primary" @click="showCreateDialog = true">新建用户</el-button>
      </div>
    </template>

    <!-- 搜索栏 -->
    <div class="search-bar">
      <el-input v-model="searchKeyword" placeholder="搜索用户名" style="width: 300px" clearable @input="handleSearch">
        <template #prefix>
          <el-icon>
            <Search />
          </el-icon>
        </template>
      </el-input>
      <el-button type="primary" @click="loadUsers" style="margin-left: 10px">刷新</el-button>
    </div>

    <!-- 用户列表 -->
    <el-table :data="filteredUsers" v-loading="loading" style="width: 100%; margin-top: 20px" stripe>
      <el-table-column prop="username" label="用户名" width="150" />
      <el-table-column prop="role" label="角色" width="100">
        <template #default="{ row }">
          <el-tag :type="row.role === 'Admin' ? 'danger' : 'info'">{{ row.role }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="isActive" label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'warning'">{{
            row.isActive ? '激活' : '禁用'
            }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="createdAt" label="创建时间" width="180">
        <template #default="{ row }">
          {{ formatDate(row.createdAt) }}
        </template>
      </el-table-column>
      <el-table-column prop="lastLoginAt" label="最后登录" width="180">
        <template #default="{ row }">
          {{ formatDate(row.lastLoginAt) }}
        </template>
      </el-table-column>
      <el-table-column label="操作" min-width="200">
        <template #default="{ row }">
          <el-button size="small" @click="handleResetPassword(row)">重置密码</el-button>
          <el-button size="small" :type="row.isActive ? 'warning' : 'success'" @click="handleToggleStatus(row)">
            {{ row.isActive ? '禁用' : '激活' }}
          </el-button>
          <el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>
    <div class="pagination-container">
      <el-pagination v-model:current-page="currentPage" v-model:page-size="pageSize" :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper" :total="filteredUsers.length" @size-change="handleSizeChange"
        @current-change="handleCurrentChange" />
    </div>

    <!-- 新建用户对话框 -->
    <el-dialog v-model="showCreateDialog" title="新建用户" width="400px">
      <el-form :model="createForm" :rules="createRules" ref="createFormRef" label-width="80px">
        <el-form-item label="用户名" prop="username">
          <el-input v-model="createForm.username" placeholder="请输入用户名" />
        </el-form-item>
        <el-form-item label="密码" prop="password">
          <el-input v-model="createForm.password" type="password" placeholder="请输入密码" show-password />
        </el-form-item>
        <el-form-item label="角色" prop="role">
          <el-select v-model="createForm.role" placeholder="请选择角色">
            <el-option label="普通用户" value="User" />
            <el-option label="管理员" value="Admin" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showCreateDialog = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">确定</el-button>
      </template>
    </el-dialog>
  </el-card>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import api from '@/services/api'
import type { User } from '@/types/user'

// 响应式数据
const users = ref<User[]>([])
const loading = ref<boolean>(false)
const searchKeyword = ref<string>('')
const showCreateDialog = ref<boolean>(false)
const createFormRef = ref<FormInstance>()
const pageSize = ref<number>(1)
const currentPage = ref<number>(10)
const defaultPassword = '1qaz@WSX'

// 创建用户表单
const createForm = ref({
  username: '',
  password: '',
  role: 'User' as 'User' | 'Admin',
})

// 表单验证规则
const createRules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 3, max: 20, message: '用户名长度应在 3 到 20 个字符', trigger: 'blur' },
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 6, message: '密码长度不能少于 6 个字符', trigger: 'blur' },
  ],
  role: [{ required: true, message: '请选择角色', trigger: 'change' }],
}

// 计算属性：过滤用户
const filteredUsers = computed(() => {
  if (!searchKeyword.value) return users.value
  return users.value.filter((user) =>
    user.username.toLowerCase().includes(searchKeyword.value.toLowerCase()),
  )
})

// 格式化日期
const formatDate = (dateString: string) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN')
}

// 加载用户列表
const loadUsers = async () => {
  loading.value = true
  try {
    const response = await api.getUsers()
    users.value = response.data
  } catch (error) {
    ElMessage.error('加载用户列表失败')
    console.error('加载用户列表失败:', error)
  } finally {
    loading.value = false
  }
}

// 搜索处理
const handleSearch = () => {
  // 计算属性会自动处理过滤
}

// 重置密码
const handleResetPassword = async (user: User) => {
  try {
    await ElMessageBox.confirm(
      `确定要重置用户 "${user.username}" 的密码吗？密码将重置为 "${defaultPassword}"。`,
      '重置密码确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )

    const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
    if (user.id === currentUser.userId) {
      ElMessage.warning('不能重置自己的密码')
      return
    }

    await api.changeUserPassword({
      userId: user.id,
      newPassword: defaultPassword,
    })

    ElMessage.success(`用户 "${user.username}" 的密码已重置为 "${defaultPassword}"`)
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('重置密码失败')
      console.error('重置密码失败:', error)
    }
  }
}

// 切换用户状态
const handleToggleStatus = async (user: User) => {
  try {
    const action = user.isActive ? '禁用' : '激活'
    await ElMessageBox.confirm(`确定要${action}用户 "${user.username}" 吗？`, `${action}用户确认`, {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })

    const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
    if (user.id === currentUser.userId) {
      ElMessage.warning('不能修改自己的状态')
      return
    }

    await api.toggleUserStatus({
      userId: user.id,
      isActive: !user.isActive,
    })

    ElMessage.success(`用户 "${user.username}" 已${action}`)
    await loadUsers() // 刷新列表
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('切换用户状态失败')
      console.error('切换用户状态失败:', error)
    }
  }
}

// 删除用户
const handleDelete = async (user: User) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除用户 "${user.username}" 吗？此操作不可恢复。`,
      '删除用户确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'error',
      },
    )

    const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
    if (user.id === currentUser.userId) {
      ElMessage.warning('不能删除自己')
      return
    }

    await api.deleteUser(user.id)
    ElMessage.success(`用户 "${user.username}" 已删除`)
    await loadUsers() // 刷新列表
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('删除用户失败')
      console.error('删除用户失败:', error)
    }
  }
}

// 创建用户
const handleCreate = async () => {
  if (!createFormRef.value) return

  try {
    await createFormRef.value.validate()

    await api.createUser(createForm.value)
    ElMessage.success('用户创建成功')
    showCreateDialog.value = false

    // 重置表单
    createForm.value = {
      username: '',
      password: '',
      role: 'User',
    }

    await loadUsers() // 刷新列表
  } catch (error: any) {
    if (error?.response?.status === 409) {
      ElMessage.error('用户名已存在')
    } else if (error !== false) {
      // 表单验证错误返回 false
      ElMessage.error('创建用户失败')
      console.error('创建用户失败:', error)
    }
  }
}

const paginatedUsers = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredUsers.value.slice(start, end)
})

const handleSizeChange = (val: number) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val: number) => {
  currentPage.value = val
}

// 组件挂载时加载用户列表
onMounted(() => {
  loadUsers()
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
