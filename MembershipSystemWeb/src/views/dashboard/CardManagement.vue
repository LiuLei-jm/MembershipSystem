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
      <div class="filter-controls">
        <div class="filter-item">
          <span class="filter-label">会员名:</span>
          <el-input
            v-model="filterForm.membershipName"
            placeholder="请输入会员名"
            clearable
            @input="handleSearch"
            style="width: 180px"
          />
        </div>
        <div class="filter-item">
          <span class="filter-label">卡号:</span>
          <el-input
            v-model="filterForm.cdk"
            placeholder="请输入卡号"
            clearable
            @input="handleSearch"
            style="width: 180px"
          />
        </div>
        <div class="filter-item">
          <span class="filter-label">开始时间:</span>
          <el-date-picker
            v-model="filterForm.dateRange"
            type="datetimerange"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DD HH:mm:ss"
            @change="handleSearch"
            style="width: 260px"
          />
        </div>
        <div class="filter-item">
          <el-switch
            v-model="showAllCards"
            size="large"
            inline-prompt
            active-text="显示所有"
            inactive-text="仅显示有效"
          />
        </div>
        <div class="filter-actions">
          <el-button type="primary" :icon="Search" @click="handleSearch">搜索</el-button>
          <el-button :icon="Delete" @click="handleReset">重置</el-button>
        </div>
      </div>

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
      <el-table-column label="操作" width="220">
        <template #default="{ row }">
          <el-button class="card-button" type="primary" size="small" @click="openEditDialog(row)"
            >编辑</el-button
          >
          <el-button class="card-button" type="info" size="small" @click="reissueCard(row)"
            >补发</el-button
          >
          <el-button class="card-button" type="warning" size="small" @click="clearCard(row)"
            >清理</el-button
          >
          <el-button class="card-button" type="danger" size="small" @click="deleteCard(row)"
            >删除</el-button
          >
        </template>
      </el-table-column>
    </el-table>
    <div class="pagination-container">
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
        :total="filteredCards.length"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </div>

    <!-- 金额合计显示区域 -->
    <div class="total-summary" v-if="filteredCards.length > 0">
      <el-card shadow="never">
        <template #header>
          <div class="total-summary-header">
            <span>筛选结果统计</span>
            <span class="total-cards">共 {{ filteredCards.length }} 张卡</span>
          </div>
        </template>
        <div class="total-details">
          <div class="total-amount-main">
            总金额: <span class="amount-highlight">¥{{ totalAmount.toFixed(2) }}</span>
          </div>
          <div class="total-hint">
            <el-text type="info" size="small">
              {{
                filterForm.dateRange && filterForm.dateRange.length === 2
                  ? '显示筛选日期范围内的金额合计'
                  : '显示所有筛选结果的金额合计'
              }}
            </el-text>
          </div>
        </div>
      </el-card>
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
import { Plus, Refresh, Setting, Search, Delete } from '@element-plus/icons-vue'
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

// 筛选表单状态
const filterForm = ref({
  membershipName: '',
  cdk: '',
  dateRange: [] as (Date | undefined)[],
})

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

// 高级筛选逻辑
const filteredCards = computed(() => {
  let cards = store.cards

  // 应用名称筛选
  if (filterForm.value.membershipName) {
    cards = cards.filter((card) =>
      card.membershipName.toLowerCase().includes(filterForm.value.membershipName.toLowerCase()),
    )
  }

  // 应用卡号筛选
  if (filterForm.value.cdk) {
    cards = cards.filter((card) =>
      card.cdk.toLowerCase().includes(filterForm.value.cdk.toLowerCase()),
    )
  }

  // 应用日期范围筛选
  if (
    filterForm.value.dateRange &&
    filterForm.value.dateRange.length === 2 &&
    filterForm.value.dateRange[0] &&
    filterForm.value.dateRange[1]
  ) {
    const startDate = new Date(filterForm.value.dateRange[0])
    const endDate = new Date(filterForm.value.dateRange[1])

    cards = cards.filter((card) => {
      if (card.startTime === null) return false
      const cardStartDate = new Date(card.startTime)
      return cardStartDate >= startDate && cardStartDate <= endDate
    })
  }

  // 应用有效卡片筛选
  if (!showAllCards.value) {
    cards = cards.filter(isCardActive)
  }

  return cards
})

// 计算筛选后的总金额 - 没有筛选开始时间时显示全部金额合计，筛选后只统计起始区间的合计
const totalAmount = computed(() => {
  // 如果没有日期筛选，计算全部符合条件的卡的金额合计
  if (
    !filterForm.value.dateRange ||
    filterForm.value.dateRange.length !== 2 ||
    !filterForm.value.dateRange[0] ||
    !filterForm.value.dateRange[1]
  ) {
    // 只应用名称和卡号筛选，不应用日期和状态筛选，显示全部金额
    let allCards = store.cards

    if (filterForm.value.membershipName) {
      allCards = allCards.filter((card) =>
        card.membershipName.toLowerCase().includes(filterForm.value.membershipName.toLowerCase()),
      )
    }

    if (filterForm.value.cdk) {
      allCards = allCards.filter((card) =>
        card.cdk.toLowerCase().includes(filterForm.value.cdk.toLowerCase()),
      )
    }

    return allCards.reduce((sum, card) => sum + card.amount, 0)
  }

  // 有日期筛选时，只统计筛选范围内的金额
  const startDate = new Date(filterForm.value.dateRange[0]!)
  const endDate = new Date(filterForm.value.dateRange[1]!)

  const cardsInDateRange = filteredCards.value.filter((card) => {
    if (card.startTime === null) return false
    const cardStartDate = new Date(card.startTime)
    return cardStartDate >= startDate && cardStartDate <= endDate
  })

  if (filterForm.value.membershipName) {
    return cardsInDateRange
      .filter((card) =>
        card.membershipName.toLowerCase().includes(filterForm.value.membershipName.toLowerCase()),
      )
      .reduce((sum, card) => sum + card.amount, 0)
  }

  if (filterForm.value.cdk) {
    return cardsInDateRange
      .filter((card) => card.cdk.toLowerCase().includes(filterForm.value.cdk.toLowerCase()))
      .reduce((sum, card) => sum + card.amount, 0)
  }

  return cardsInDateRange.reduce((sum, card) => sum + card.amount, 0)
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

const reissueCard = (card: MemberCard) => {
  store.reissueCard(card.cdk)
}

const clearCard = (card: MemberCard) => {
  store.clearCard(card.cdk)
}

const deleteCard = (card: MemberCard) => {
  store.deleteCard(card.id)
}

// 搜索和重置功能
const handleSearch = () => {
  currentPage.value = 1 // 重置到第一页
}

const handleReset = () => {
  filterForm.value = {
    membershipName: '',
    cdk: '',
    dateRange: [],
  }
  currentPage.value = 1
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

.filter-controls {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 15px;
  padding: 15px;
  background-color: #f8f9fa;
  border-radius: 8px;
}

.filter-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.filter-label {
  font-weight: 500;
  color: #333;
  white-space: nowrap;
  min-width: 60px;
}

.filter-actions {
  display: flex;
  gap: 8px;
  margin-left: auto;
}

.filter-bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 15px;
  background-color: #fff;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
}

.total-amount {
  font-weight: 500;
  color: #333;
}

.amount-number {
  font-weight: 700;
  color: #409eff;
  font-size: 16px;
}

.pagination-container {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  margin-top: 20px;
}

.card-button {
  margin: 0;
}

.total-summary {
  margin-top: 20px;
}

.total-summary-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: 500;
}

.total-cards {
  color: #909399;
  font-weight: normal;
}

.total-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.total-amount-main {
  font-size: 18px;
  font-weight: 600;
  color: #303133;
  margin-bottom: 8px;
}

.amount-highlight {
  color: #f56c6c;
  font-size: 24px;
  font-weight: 700;
}

.total-hint {
  line-height: 1;
}
</style>
