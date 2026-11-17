import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import api, { CLAVE_SESION } from '@/services/api'

function leerSesion() {
  try {
    const sesion = JSON.parse(localStorage.getItem(CLAVE_SESION))
    if (sesion && new Date(sesion.expira) > new Date()) return sesion
  } catch {
    // sesión corrupta: se descarta
  }
  localStorage.removeItem(CLAVE_SESION)
  return null
}

export const useAuthStore = defineStore('auth', () => {
  const sesion = ref(leerSesion())

  const usuario = computed(() => sesion.value)
  const estaAutenticado = computed(() => !!sesion.value)
  const esAdmin = computed(() => sesion.value?.rol === 'Admin')

  function guardar(datos) {
    sesion.value = datos
    localStorage.setItem(CLAVE_SESION, JSON.stringify(datos))
  }

  async function login(correo, password) {
    const { data } = await api.post('/auth/login', { correo, password })
    guardar(data)
    return data
  }

  async function registro(payload) {
    const { data } = await api.post('/auth/registro', payload)
    guardar(data)
    return data
  }

  function actualizarNombre(nombre) {
    if (!sesion.value) return
    guardar({ ...sesion.value, nombre })
  }

  function logout() {
    sesion.value = null
    localStorage.removeItem(CLAVE_SESION)
  }

  return { usuario, estaAutenticado, esAdmin, login, registro, actualizarNombre, logout }
})