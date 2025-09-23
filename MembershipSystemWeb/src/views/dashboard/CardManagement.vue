<template>
  <el-card>
    <template #header>
      <div class="card-header">
        <span>会员卡列表</span>
        <div class="header-buttons">
          <el-button type="primary" :icon="Plus" @click="openCreateDialog">创建新卡</el-button>
          <el-button :icon="Setting" @click="openPathConfigDialog">路径配置</el-button>
          <el-button :icon="Refresh" @click="handleRefresh" title="强制刷新"></el-button>
        </div>
      </div>
    </template>
    <div class="filter-bar">
      <el-switch
        v-model="showAllCards"
        size="large"
        inline-prompt
        active-text="显示所有"
        inactive-text="仅显示有效"
      />
      <el-alert
        v-if="store.status === 'error'"
        :title="store.error || '加载会员卡失败'"
        type="error"
        show-icon
        :closable="false"
        style="margin-bottom: 20px"
      />
    </div>
    <el-table :data="paginatedCards" stripe style="width: 100%" v-loading="loading">
      <el-table-column prop="membershipName" label="会员名" width="150" />
      <el-table-column prop="cdk" label="卡号" />
      <el-table-column prop="durationInDays" label="时长（天）" width="100" />
      <el-table-column prop="amount" label="金额" width="100" />
      <el-table-column prop="startTime" label="开始时间">
        <template #default="{ row }">
          {{ formatDate(row.startTime) }}
        </template>
      </el-table-column>
      <el-table-column prop="endTime" label="结束时间">
        <template #default="{ row }">
          {{ formatDate(row.endTime) }}
        </template>
      </el-table-column>
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="isCardActive(row) ? 'success' : 'info'">
            {{ isCardActive(row) ? '有效' : '已失效' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="180">
        <template #default="{ row }">
          <el-button type="primary" size="small" @click="openEditDialog(row)">编辑</el-button>
          <el-button type="danger" size="small" @click="deleteCard(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>
    <div class="pagination-container">
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next, jumper"
        :total="filteredCards.length"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </div>
    <el-dialog v-model="dialogVisible" :title="dialogTitle">
      <CreateCardDialog
        :model-value="dialogVisible"
        @update:modelValue="dialogVisible = $event"
        @success="handleRefresh"
      />
    </el-dialog>
    <el-dialog v-model="editDialogVisible" title="编辑会员卡">
      <EditCardDialog
        :model-value="editDialogVisible"
        :card="selectedCard"
        @update:modelValue="editDialogVisible = $event"
        @success="handleRefresh"
      />
    </el-dialog>
    <el-dialog v-model="pathConfigDialogVisible" title="路径配置">
      <PathConfigDialog
        :model-value="pathConfigDialogVisible"
        @update:modelValue="pathConfigDialogVisible = $event"
      />
    </el-dialog>
  </el-card>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Plus, Refresh, Setting } from '@element-plus/icons-vue'
import { useMembershipStore } from '@/stores/membership'
import type { MemberCard } from '@/types/card'
import CreateCardDialog from '@/components/CreateCardDialog.vue'
import EditCardDialog from '@/components/EditCardDialog.vue'
import PathConfigDialog from '@/components/PathConfigDialog.vue'

const store = useMembershipStore()

const loading = ref<boolean>(true)
const showAllCards = ref<boolean>(false)
const dialogVisible = ref<boolean>(false)
const editDialogVisible = ref<boolean>(false)
const pathConfigDialogVisible = ref<boolean>(false)
const dialogTitle = ref<string>('创建新卡')
const selectedCard = ref<MemberCard | null>(null)
const currentPage = ref<number>(1)
const pageSize = ref<number>(10)

onMounted(() => {
  store.fetchCards().finally(() => {
    loading.value = false
  })
})

const handleRefresh = () => {
  store.fetchCards(true).finally(() => {
    loading.value = false
  })
}

const isCardActive = (card: MemberCard) => new Date(card.endTime) >= new Date()

const filteredCards = computed(() => {
  if (showAllCards.value) {
    return store.cards
  }
  return store.cards.filter(isCardActive)
})

const paginatedCards = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredCards.value.slice(start, end)
})

const handleSizeChange = (val: number) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val: number) => {
  currentPage.value = val
}

const openCreateDialog = () => {
  dialogTitle.value = '创建新卡'
  dialogVisible.value = true
}
const openEditDialog = (card: MemberCard) => {
  selectedCard.value = card
  editDialogVisible.value = true
}

const openPathConfigDialog = () => {
  pathConfigDialogVisible.value = true
}
const deleteCard = (card: MemberCard) => {
  store.deleteCard(card.id)
}

const formatDate = (dateStr: string) => {
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '-'
  return new Intl.DateTimeFormat('zh-CN', {
    timeZone: 'Asia/Shanghai',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false,
  })
    .format(date)
    .replace(/\//g, '-')
}
</script>

<style scoped>
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-buttons {
  display: flex;
  gap: 10px;
}

.filter-bar {
  margin-bottom: 20px;
}

.pagination-container {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  margin-top: 20px;
}
</style>
