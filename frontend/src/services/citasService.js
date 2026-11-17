import api from '@/services/api'

export async function obtenerDisponibilidad(fecha) {
  const { data } = await api.get('/citas/disponibilidad', { params: { fecha } })
  return data
}

export async function crearCita(payload) {
  const { data } = await api.post('/citas', payload)
  return data
}

export async function obtenerMisCitas() {
  const { data } = await api.get('/citas/mis-citas')
  return data
}

export async function cancelarCita(id) {
  const { data } = await api.post(`/citas/${id}/cancelar`)
  return data
}