<template>
  <el-dialog
    :model-value="modelValue"
    title="修改密码"
    width="500px"
    :before-close="handleClose"
    :close-on-click-modal="false"
  >
    <el-form
      ref="formRef"
      :model="formModel"
      :rules="formRules"
      label-width="100px"
      v-loading="loading"
    >
      <el-form-item label="旧密码" prop="currentPassword">
        <el-input
          v-model="formModel.currentPassword"
          type="password"
          placeholder="请输入旧密码"
          show-password
        />
      </el-form-item>
      <el-form-item label="新密码" prop="newPassword">
        <el-input
          v-model="formModel.newPassword"
          type="password"
          placeholder="请输入新密码"
          show-password
        />
      </el-form-item>
      <el-form-item label="确认新密码" prop="confirmNewPassword">
        <el-input
          v-model="formModel.confirmNewPassword"
          type="password"
          placeholder="请再次输入新密码"
          show-password
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
import { ref, reactive } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import type { ChangePassword } from '@/types/api'

const props = defineProps<{
  modelValue: boolean
}>()
const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
}>()
const authStore = useAuthStore()
const loading = ref<boolean>(false)
const formRef = ref<FormInstance>()
interface ChangePasswordForm extends ChangePassword {
  confirmNewPassword: string
}
const formModel = reactive<ChangePasswordForm>({
  currentPassword: '',
  newPassword: '',
  confirmNewPassword: '',
})
const validateConfirmPassword = (rule: any, value: any, callback: (error?: Error) => void) => {
  if (value !== formModel.newPassword) {
    callback(new Error('两次输入的密码不一致'))
  } else {
    callback()
  }
}
const formRules = reactive<FormRules>({
  currentPassword: [{ required: true, message: '请输入旧密码', trigger: 'blur' }],
  newPassword: [{ required: true, message: '请输入新密码', trigger: 'blur' }],
  confirmNewPassword: [
    { required: true, message: '请再次输入新密码', trigger: 'blur' },
    { validator: validateConfirmPassword, trigger: 'blur' },
  ],
})
const resetForm = () => {
  formRef.value?.resetFields()
}
const handleClose = () => {
  resetForm()
  emit('update:modelValue', false)
}
const handleSubmit = () => {
  if (!formRef.value) return
  formRef.value.validate(async (valid) => {
    if (valid) {
      loading.value = true
      try {
        const payload: ChangePassword = {
          currentPassword: formModel.currentPassword,
          newPassword: formModel.newPassword,
        }
        const success = await authStore.changePassword(payload)

        if (success) {
          ElMessage.success('密码修改成功，请重新登录')
          handleClose()
          authStore.logout()
        }
      } catch (error) {
        ElMessage.error('密码修改失败，请检查旧密码是否正确')
      }

      loading.value = false
    }
  })
}
</script>

<style scoped>
.dialog-footer {
  text-align: right;
}
</style>
