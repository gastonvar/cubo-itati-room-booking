import axios from 'axios'

import { env } from '@/config/env'

/** Axios instance without auth interceptors — used for login/refresh/logout/me bootstrap. */
export const authApi = axios.create({
  baseURL: env.apiUrl,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})
