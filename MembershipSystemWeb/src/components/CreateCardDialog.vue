<template>
  <el-dialog
    title="创建新卡"
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
      <el-form-item label="CDK" prop="cdk">
        <el-input v-model="formModel.cdk" placeholder="留空将自动生成" />
      </el-form-item>
      <el-form-item label="有效天数" prop="durationInDays">
        <el-input-number v-model="formModel.durationInDays" :min="0" />
      </el-form-item>
      <el-form-item label="金额" prop="amount">
        <el-input-number v-model="formModel.amount" :min="0" :step="0.01" :precision="2" />
      </el-form-item>
      <el-form-item lable="开始时间" prop="startTime">
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
        <el-button type="primary" :loading="loading" @click="handleSubmit">确认创建</el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive, watch, onMounted } from 'vue'
import { useMembershipStore } from '@/stores/membership'
import { usePathStore } from '@/stores/path'
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage } from 'element-plus'
import type { CreateCardData } from '@/types/card'
const props = defineProps<{
  modelValue: boolean
}>()
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'success'): void
}>()
const store = useMembershipStore()
const pathStore = usePathStore()
const loading = ref<boolean>(false)
const formRef = ref<FormInstance>()
// Initialize form with default values from path store
const initialFormModel: CreateCardData = {
  membershipName: '',
  cdk: '',
  durationInDays: 0,
  amount: 0,
  startTime: new Date(),
  notes: '',
}
const formModel = reactive<CreateCardData>({
  ...initialFormModel,
})
const formRules = reactive<FormRules>({
  membershipName: [{ required: true, message: '请输入会员名', trigger: 'blur' }],
  cdk: [
    {
      validator: (rule, value, callback) => {
        if (!value) {
          callback() // 空值直接通过
        } else if (value.length !== 20) {
          callback(new Error('CDK长度必须为20位'))
        } else if (!/\d{4}$/.test(value)) {
          callback(new Error('CDK最后4位必须是数字'))
        } else {
          callback()
        }
      },
      trigger: 'blur',
    },
  ],
  durationInDays: [
    { required: true, type: 'number', min: 1, message: '请输入有效天数', trigger: 'blur' },
  ],
  amount: [{ required: true, type: 'number', min: 0, message: '请输入金额', trigger: 'blur' }],
  startTime: [{ required: true, type: 'date', message: '请选择开始时间', trigger: 'change' }],
})
const resetForm = () => {
  formRef.value?.resetFields()
  Object.assign(formModel, initialFormModel)
  formModel.startTime = new Date()
}

// Fetch path configuration when component mounts
onMounted(async () => {
  await pathStore.fetchPathConfiguration()
})

const handleClose = () => {
  emit('update:modelValue', false)
  resetForm()
}
watch(
  () => props.modelValue,
  (newVal) => {
    if (!newVal) {
      setTimeout(resetForm, 300)
    }
  },
)
const handleSubmit = async () => {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (valid) {
      loading.value = true
      try {
        await store.createCard(formModel)
        ElMessage.success('会员卡创建成功')
        emit('success')
        handleClose()
      } catch (error) {
        ElMessage.error('会员卡创建失败，请稍后重试')
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
