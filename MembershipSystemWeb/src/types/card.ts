export interface MemberCard {
  id: string
  membershipName: string
  durationInDays: number
  amount: number
  cdk: string
  notes?: string
  startTime: null | Date // 或 Date 类型
  endTime: Date
}

export type CreateCardData = Omit<MemberCard, 'id' | 'endTime' | 'cdk'> 

export type UpdateCardData = Partial<CreateCardData>
