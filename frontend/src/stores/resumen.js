import { defineStore } from 'pinia'
import { ref } from 'vue'
import { obtenerResumen } from '@/services/adminCitasService'

export const useResumenStore = defineStore('resumen', () => {
  const conteos = ref({
    pendientes: 0,
    aceptadas: 0,
    completadas: 0,
    rechazadas: 0,
    canceladas: 0,
    total: 0,
  })

  async function cargar() {
    try {
      conteos.value = await obtenerResumen()
    } catch {
      // se ignora: es una actualización en segundo plano
    }
  }

  return { conteos, cargar }
})