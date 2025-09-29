export interface AppendData {
  filePath: string
  content: string
  logMessage: string
}

export interface DeleteData {
  filePath: string
  contentToRemove: string
  logMessage: string
}
