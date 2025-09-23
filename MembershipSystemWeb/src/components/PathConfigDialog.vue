<template>
  <el-dialog
    title="路径配置"
    :model-value="modelValue"
    width="600px"
    @close="handleClose"
    :close-on-click-modal="false"
  >
    <el-form
      ref="formRef"
      :model="formModel"
      :rules="formRules"
      label-width="150px"
      v-loading="loading"
    >
      <el-form-item label="基础路径" prop="basePath">
        <el-input v-model="formModel.basePath" placeholder="请输入基础路径，例如: D:\" />
        <div class="path-hint">会员卡文件将保存在此路径下</div>
      </el-form-item>

      <el-form-item label="会员卡文件名" prop="membershipCardFilePath">
        <el-input
          v-model="formModel.membershipCardFilePath"
          placeholder="请输入会员卡文件名，例如: CDK.txt"
        />
      </el-form-item>

      <el-form-item label="允许自定义路径" prop="allowCustomPaths">
        <el-switch v-model="formModel.allowCustomPaths" active-text="允许" inactive-text="禁止" />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" :loading="loading" @click="handleSubmit"> 保存配置 </el-button>
        <el-button @click="handleReset"> 重置 </el-button>
      </el-form-item>
    </el-form>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, watch } from 'vue'
import { usePathStore } from '@/stores/path'
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage } from 'element-plus'
import type { PathConfigurationUpdateRequest } from '@/types/path'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
}>()

const pathStore = usePathStore()
const loading = ref<boolean>(false)
const formRef = ref<FormInstance>()

const formModel = reactive<PathConfigurationUpdateRequest>({
  basePath:  "D:",
  membershipCardFilePath: 'CDK.txt',
  allowCustomPaths: true,
})

const formRules = reactive<FormRules>({
  basePath: [
    { required: true, message: '请输入基础路径', trigger: 'blur' },
    { validator: validatePath, trigger: 'blur' },
  ],
  membershipCardFilePath: [{ required: true, message: '请输入会员卡文件名', trigger: 'blur' }],
})

// Validate path format
function validatePath(rule: any, value: string, callback: any) {
  if (!value) {
    callback(new Error('请输入基础路径'))
    return
  }

  // Basic path validation
  if (value.includes('..')) {
    callback(new Error('路径不能包含 ..'))
    return
  }

  callback()
}

// Load current configuration
onMounted(async () => {
  await pathStore.fetchPathConfiguration()
  if (pathStore.config) {
    formModel.basePath = pathStore.config.basePath
    formModel.membershipCardFilePath = pathStore.config.membershipCardFilePath
    formModel.allowCustomPaths = pathStore.config.allowCustomPaths
  }
})

// Watch for modelValue changes to reload data when dialog opens
watch(
  () => props.modelValue,
  async (newVal) => {
    if (newVal) {
      await pathStore.fetchPathConfiguration()
      if (pathStore.config) {
        formModel.basePath = pathStore.config.basePath
        formModel.membershipCardFilePath = pathStore.config.membershipCardFilePath
        formModel.allowCustomPaths = pathStore.config.allowCustomPaths
      }
    }
  }
)

const handleSubmit = async () => {
  if (!formRef.value) return
  await formRef.value.validate(async (valid) => {
    if (valid) {
      loading.value = true
      try {
        await pathStore.updatePathConfiguration(formModel)
        ElMessage.success('路径配置保存成功')
        handleClose()
      } catch (error) {
        ElMessage.error('路径配置保存失败')
      } finally {
        loading.value = false
      }
    }
  })
}

const handleReset = () => {
  if (pathStore.config) {
    formModel.basePath = pathStore.config.basePath
    formModel.membershipCardFilePath = pathStore.config.membershipCardFilePath
    formModel.allowCustomPaths = pathStore.config.allowCustomPaths
  } else {
    formModel.basePath = 'D:'
    formModel.membershipCardFilePath = 'CDK.txt'
    formModel.allowCustomPaths = true
  }
}

const handleClose = () => {
  emit('update:modelValue', false)
}
</script>

<style scoped>
.path-hint {
  font-size: 12px;
  color: #909399;
  margin-top: 5px;
}

.el-form-item {
  margin-bottom: 20px;
}
</style>