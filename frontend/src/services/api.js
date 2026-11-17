import axios from 'axios'

export const CLAVE_SESION = 'soporte_sesion'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
})

api.interceptors.request.use((config) => {
  const guardada = localStorage.getItem(CLAVE_SESION)
  if (guardada) {
    try {
      const { token } = JSON.parse(guardada)
      if (token) config.headers.Authorization = `Bearer ${token}`
    } catch {
      localStorage.removeItem(CLAVE_SESION)
    }
  }
  return config
})

api.interceptors.response.use(
  (respuesta) => respuesta,
  (error) => {
    // Token vencido o inválido: se limpia la sesión y se vuelve al login
    if (error.response?.status === 401 && localStorage.getItem(CLAVE_SESION)) {
      localStorage.removeItem(CLAVE_SESION)
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

export default api