export type ChatRole = 'user' | 'assistant'

export type ChatMessage = {
  role: ChatRole
  content: string
}

export type ChatRequest = {
  messages: ChatMessage[]
}

export type ChatResponse = {
  reply: string
}
