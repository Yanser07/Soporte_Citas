import { onMounted, onUnmounted } from 'vue'

export function useAutoRefresh(callback, intervaloMs = 30000) {
  let temporizador = null

  function alCambiarVisibilidad() {
    if (document.visibilityState === 'visible') callback()
  }

  onMounted(() => {
    temporizador = setInterval(() => {
      if (document.visibilityState === 'visible') callback()
    }, intervaloMs)
    document.addEventListener('visibilitychange', alCambiarVisibilidad)
  })

  onUnmounted(() => {
    clearInterval(temporizador)
    document.removeEventListener('visibilitychange', alCambiarVisibilidad)
  })
}