<template>
  <el-dialog
    :title="`编辑会员卡 - ${formModel.cdk || ''}`"
    :model-value="modelValue"
    width="600px"
    @close="handleClose"
    :close-on-click-modal="false"
  >
    <el-form
      ref="formRef"
      :model="formModel"
      :rules="formRules"
      label-width="120px"
      v-loading="loading"
    >
      <el-form-item label="会员名" prop="membershipName">
        <el-input v-model="formModel.membershipName" placeholder="请输入会员名" />
      </el-form-item>
      <el-form-item label="有效天数" prop="durationInDays">
        <el-input-number v-model="formModel.durationInDays" :min="0" />
      </el-form-item>
      <el-form-item label="金额" prop="amount">
        <el-input-number v-model="formModel.amount" :min="0" :step="0.01" :precision="2" />
      </el-form-item>
      <el-form-item label="开始时间" prop="startTime">
        <el-date-picker
          v-model="formModel.startTime"
          type="datetime"
          placeholder="选择开始时间"
          format="YYYY-MM-DD HH:mm:ss"
          style="width: 100%"
        />
      </el-form-item>
      <el-form-item label="备注" prop="notes">
        <el-input
          type="textarea"
          v-model="formModel.notes"
          placeholder="请输入备注（可选）"
          :rows="3"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="handleClose">取 消</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">确 定</el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive, watch, onMounted } from 'vue'
import { useMembershipStore } from '@/stores/membership'
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage } from 'element-plus'
import type { MemberCard, UpdateCardData } from '@/types/card'

const props = defineProps<{
  modelValue: boolean
  card?: MemberCard | null
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'success'): void
}>()

const store = useMembershipStore()
const loading = ref<boolean>(false)
const formRef = ref<FormInstance>()

const initialFormModel = {
  membershipName: '',
  durationInDays: 0,
  amount: 0,
  startTime: null as Date | null,
  notes: '',
  cdk: '',
}

const formModel = reactive<{ [key: string]: any }>({
  ...initialFormModel,
})

const formRules = reactive<FormRules>({
  membershipName: [{ required: true, message: '请输入会员名', trigger: 'blur' }],
  durationInDays: [
    { required: true, type: 'number', min: 1, message: '请输入有效天数', trigger: 'blur' },
  ],
  amount: [{ required: true, type: 'number', min: 0, message: '请输入金额', trigger: 'blur' }],
  startTime: [{ required: true, type: 'date', message: '请选择开始时间', trigger: 'change' }],
})

const resetForm = () => {
  Object.assign(formModel, initialFormModel)
}

const handleClose = () => {
  emit('update:modelValue', false)
  resetForm()
}

const populateForm = () => {
  if (props.card) {
    formModel.membershipName = props.card.membershipName
    formModel.durationInDays = props.card.durationInDays
    formModel.amount = props.card.amount
    formModel.startTime = props.card.startTime ? new Date(props.card.startTime) : null
    formModel.notes = props.card.notes || ''
    formModel.cdk = props.card.cdk
  } else {
    resetForm()
  }
}

// Watch for changes to the card prop to immediately populate the form
watch(
  () => props.card,
  (newCard) => {
    if (newCard) {
      populateForm()
    } else {
      resetForm()
    }
  },
  { immediate: true }
)

// Watch for dialog visibility changes
watch(
  () => props.modelValue,
  (newVal) => {
    if (!newVal) {
      // When dialog closes, reset form after a short delay to allow for animation
      setTimeout(resetForm, 300)
    }
  },
)

const handleSubmit = async () => {
  if (!formRef.value || !props.card) return

  await formRef.value.validate(async (valid) => {
    if (valid) {
      loading.value = true
      try {
        // Prepare update data (only include changed fields)
        const updateData: UpdateCardData = {
          membershipName: formModel.membershipName,
          durationInDays: formModel.durationInDays,
          amount: formModel.amount,
          startTime: formModel.startTime,
          notes: formModel.notes,
        }

        await store.updateCard(props.card!.id, updateData)
        ElMessage.success('会员卡更新成功')
        emit('success')
        handleClose()
      } catch (error) {
        ElMessage.error('会员卡更新失败，请稍后重试')
      } finally {
        loading.value = false
      }
    }
  })
}
</script>

<style scoped>
.dialog-footer {
  text-align: right;
}
</style>
