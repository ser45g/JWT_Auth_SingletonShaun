// src/lib/queryClient.ts
import { QueryClient } from '@tanstack/react-query'

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,   // 5 минут — данные считаются свежими
      gcTime: 1000 * 60 * 60,     // 1 час — кэш живёт после того, как стал неактивным
      retry: 1,                    // одна повторная попытка при ошибке
      refetchOnWindowFocus: false, // не перезапрашивать при возврате на вкладку
    },
  },
})